using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DisCatSharp.Voice.Codec;
using DisCatSharp.Voice.Interfaces;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     Scheduler-based output controller for external voice playback.
///     Replaces the timer-driven always-mix behavior with three explicit modes:
///     idle, Opus passthrough, and interrupt/overlay playback.
/// </summary>
/// <remarks>
///     <para>Design goals:</para>
///     <list type="bullet">
///         <item>Music bypasses decode/mix/re-encode completely in steady-state.</item>
///         <item>TTS/system audio interrupts or ducks music instead of being hard-mixed.</item>
///         <item>Backpressure is bounded and explicit.</item>
///         <item>No PeriodicTimer-based master clock in the passthrough path.</item>
///     </list>
///     <para>
///         Bind this controller to a voice connection via
///         <see cref="VoiceConnection.BindExternalOpusSourceAsync" />.
///     </para>
/// </remarks>
public sealed class VoiceOutputController : IExternalOpusSource, IAsyncDisposable
{
	private const int FRAME_DURATION_MS = 20;

	private readonly AudioFormat _format;
	private readonly Opus _opus;
	private readonly int _pcmFrameSize;
	private readonly CancellationTokenSource _disposeCts = new();

	/// <summary>
	///     Final output frames consumed by <see cref="VoiceConnection.BindExternalOpusSourceAsync" />.
	/// </summary>
	private readonly Channel<ExternalOpusFrame> _outgoing = Channel.CreateBounded<ExternalOpusFrame>(
		new BoundedChannelOptions(8)
		{
			SingleReader = true,
			SingleWriter = false,
			FullMode = BoundedChannelFullMode.DropOldest
		});

	/// <summary>
	///     Overlay PCM jobs (TTS, system prompts, etc.) processed serially.
	/// </summary>
	private readonly Channel<OverlayJob> _overlayJobs = Channel.CreateUnbounded<OverlayJob>(
		new UnboundedChannelOptions
		{
			SingleReader = true,
			SingleWriter = false
		});

	private readonly Lock _gate = new();

	private CancellationTokenSource? _musicPumpCts;
	private Task? _musicPumpTask;

	private readonly Task? _overlayPumpTask;

	private int _activeReader;
	private bool _disposed;
	private bool _hasMusicSource;
	private uint _sequence;
	private float _musicGain = 1.0f;
	private bool _pauseMusicForOverlays = true;
	private int _overlayDepth;

	/// <summary>
	///     Initializes a new instance of the <see cref="VoiceOutputController" /> class.
	/// </summary>
	/// <param name="format">
	///     Audio format for PCM encode/decode operations. Defaults to 48 kHz stereo music if not specified.
	/// </param>
	public VoiceOutputController(AudioFormat format = default)
	{
		this._format = format.IsValid() ? format : AudioFormat.Default;
		this._pcmFrameSize = this._format.CalculateSampleSize(FRAME_DURATION_MS);
		this._opus = new(this._format);
		this._overlayPumpTask = Task.Run(() => this.RunOverlayLoopAsync(this._disposeCts.Token));
	}

	/// <summary>
	///     Gets or sets the current music gain multiplier used in passthrough mode.
	///     Values below 1 require decode/mix/re-encode and should be avoided for steady-state music.
	/// </summary>
	public float MusicGain
	{
		get
		{
			lock (this._gate)
				return this._musicGain;
		}
		set
		{
			lock (this._gate)
				this._musicGain = Math.Clamp(value, 0f, 1f);
		}
	}

	/// <summary>
	///     Gets or sets a value indicating whether music should be paused while overlays are playing.
	///     When <see langword="true" />, overlay playback avoids costly hard-mixing and gives the lowest latency.
	/// </summary>
	public bool PauseMusicForOverlays
	{
		get
		{
			lock (this._gate)
				return this._pauseMusicForOverlays;
		}
		set
		{
			lock (this._gate)
				this._pauseMusicForOverlays = value;
		}
	}

	/// <summary>
	///     Gets a value indicating whether one or more overlay jobs are currently active.
	/// </summary>
	public bool HasActiveOverlay => Volatile.Read(ref this._overlayDepth) > 0;

	/// <summary>
	///     Gets a value indicating whether a music source is currently bound.
	/// </summary>
	public bool HasMusicSource
	{
		get
		{
			lock (this._gate)
				return this._hasMusicSource;
		}
	}

	/// <summary>
	///     Binds the main music source. Rebinding cleanly stops the prior source pump.
	///     The music source is expected to produce Opus frames and is forwarded directly when possible.
	/// </summary>
	/// <param name="source">
	///     The new music source, or <see langword="null" /> to stop music playback.
	/// </param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	public async Task SetMusicSourceAsync(IExternalOpusSource? source, CancellationToken cancellationToken = default)
	{
		this.ThrowIfDisposed();

		CancellationTokenSource? oldCts;
		Task? oldTask;

		lock (this._gate)
		{
			this._hasMusicSource = source is not null;

			oldCts = this._musicPumpCts;
			oldTask = this._musicPumpTask;

			this._musicPumpCts = null;
			this._musicPumpTask = null;

			if (source is null)
				return;

			var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(this._disposeCts.Token, cancellationToken);
			this._musicPumpCts = linkedCts;
			this._musicPumpTask = Task.Run(() => this.RunMusicLoopAsync(source, linkedCts.Token), linkedCts.Token);
		}

		if (oldCts is not null)
		{
			await oldCts.CancelAsync().ConfigureAwait(false);
			oldCts.Dispose();
		}

		if (oldTask is not null)
		{
			try
			{
				await oldTask.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// Expected during source swap.
			}
		}
	}

	/// <summary>
	///     Queues a PCM overlay job, such as TTS or a system prompt.
	///     The PCM stream must be 16-bit little-endian PCM matching the configured output format.
	/// </summary>
	/// <param name="pcmStream">The PCM audio stream to play as an overlay.</param>
	/// <param name="name">An optional display name for the overlay (used for diagnostics).</param>
	/// <param name="cancellationToken">A token to cancel the queue operation.</param>
	public ValueTask QueuePcmOverlayAsync(Stream pcmStream, string? name = null, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(pcmStream);
		this.ThrowIfDisposed();

		return this._overlayJobs.Writer.WriteAsync(
			new(
				Name: name ?? "overlay",
				PcmStream: pcmStream,
				CloseStreamWhenFinished: false),
			cancellationToken);
	}

	/// <summary>
	///     Queues a PCM overlay job and disposes the stream when playback completes.
	/// </summary>
	/// <param name="pcmStream">The PCM audio stream to play. Ownership transfers to the controller.</param>
	/// <param name="name">An optional display name for the overlay (used for diagnostics).</param>
	/// <param name="cancellationToken">A token to cancel the queue operation.</param>
	public ValueTask QueueOwnedPcmOverlayAsync(Stream pcmStream, string? name = null, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(pcmStream);
		this.ThrowIfDisposed();

		return this._overlayJobs.Writer.WriteAsync(
			new(
				Name: name ?? "overlay",
				PcmStream: pcmStream,
				CloseStreamWhenFinished: true),
			cancellationToken);
	}

	/// <summary>
	///     Sets ducking state explicitly.
	///     Prefer pausing music entirely for overlays when possible (lowest latency).
	/// </summary>
	/// <param name="enabled">
	///     <see langword="true" /> to duck music to <paramref name="duckedGain" />;
	///     <see langword="false" /> to restore full volume.
	/// </param>
	/// <param name="duckedGain">The gain multiplier while ducked. Clamped to [0, 1].</param>
	public void SetDucking(bool enabled, float duckedGain = 0.20f)
	{
		this.ThrowIfDisposed();

		lock (this._gate)
			this._musicGain = enabled ? Math.Clamp(duckedGain, 0f, 1f) : 1.0f;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ExternalOpusFrame> ReadFramesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		this.ThrowIfDisposed();

		if (Interlocked.CompareExchange(ref this._activeReader, 1, 0) != 0)
			throw new InvalidOperationException("Only one consumer may read from VoiceOutputController at a time.");

		try
		{
			await foreach (var frame in this._outgoing.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				yield return frame;
		}
		finally
		{
			Interlocked.Exchange(ref this._activeReader, 0);
		}
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (this._disposed)
			return;

		this._disposed = true;

		await this._disposeCts.CancelAsync().ConfigureAwait(false);
		this._overlayJobs.Writer.TryComplete();
		this._outgoing.Writer.TryComplete();

		this._musicPumpCts?.Dispose();

		if (this._musicPumpTask is not null)
		{
			try
			{
				await this._musicPumpTask.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// Expected during disposal.
			}
		}

		if (this._overlayPumpTask is not null)
		{
			try
			{
				await this._overlayPumpTask.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// Expected during disposal.
			}
		}

		this._opus.Dispose();
		this._disposeCts.Dispose();
	}

	/// <summary>
	///     Main music loop. In the happy path, it forwards Opus frames directly with no timer,
	///     no decode, and no re-encode.
	/// </summary>
	private async Task RunMusicLoopAsync(IExternalOpusSource source, CancellationToken cancellationToken)
	{
		await foreach (var frame in source.ReadFramesAsync(cancellationToken).ConfigureAwait(false))
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (this.ShouldSuppressMusic())
			{
				await this.WriteOutgoingAsync(
					new(ReadOnlyMemory<byte>.Empty, FRAME_DURATION_MS, this._sequence++, 0, IsSilence: true),
					cancellationToken).ConfigureAwait(false);
				continue;
			}

			var gain = this.GetMusicGain();

			if (gain >= 0.999f)
			{
				await this.WriteOutgoingAsync(frame, cancellationToken).ConfigureAwait(false);
				continue;
			}

			// Slow path: decode → apply gain → re-encode.
			var adjusted = this.AdjustOpusGain(frame, gain);
			await this.WriteOutgoingAsync(adjusted, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Serial overlay loop. Overlays are processed one-at-a-time to keep the mental model simple
	///     and to avoid continuous hard-mixing in the common bot scenario.
	/// </summary>
	private async Task RunOverlayLoopAsync(CancellationToken cancellationToken)
	{
		while (await this._overlayJobs.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
		{
			while (this._overlayJobs.Reader.TryRead(out var job))
			{
				Interlocked.Increment(ref this._overlayDepth);

				try
				{
					await this.PlayPcmOverlayAsync(job, cancellationToken).ConfigureAwait(false);
				}
				finally
				{
					Interlocked.Decrement(ref this._overlayDepth);

					if (job.CloseStreamWhenFinished)
						await job.PcmStream.DisposeAsync().ConfigureAwait(false);
				}
			}
		}
	}

	/// <summary>
	///     Plays one PCM overlay job by encoding one 20 ms PCM frame at a time.
	///     Partial final frames are zero-padded.
	/// </summary>
	private async Task PlayPcmOverlayAsync(OverlayJob job, CancellationToken cancellationToken)
	{
		var pcmBuffer = ArrayPool<byte>.Shared.Rent(this._pcmFrameSize);
		var opusBuffer = ArrayPool<byte>.Shared.Rent(this._pcmFrameSize);

		try
		{
			while (true)
			{
				var read = await ReadExactOrPartialAsync(job.PcmStream, pcmBuffer.AsMemory(0, this._pcmFrameSize), cancellationToken).ConfigureAwait(false);
				if (read <= 0)
					break;

				if (read < this._pcmFrameSize)
					Array.Clear(pcmBuffer, read, this._pcmFrameSize - read);

				var opusSpan = opusBuffer.AsSpan(0, this._pcmFrameSize);
				this._opus.Encode(pcmBuffer.AsSpan(0, this._pcmFrameSize), ref opusSpan);

				var copy = new byte[opusSpan.Length];
				opusSpan.CopyTo(copy);
				await this.WriteOutgoingAsync(
					new(copy, FRAME_DURATION_MS, this._sequence++, 0),
					cancellationToken).ConfigureAwait(false);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(pcmBuffer);
			ArrayPool<byte>.Shared.Return(opusBuffer);
		}
	}

	/// <summary>
	///     Applies gain by decoding Opus to PCM, scaling, then re-encoding.
	///     Intentionally isolated so it only occurs on the rare slow path.
	/// </summary>
	private ExternalOpusFrame AdjustOpusGain(ExternalOpusFrame frame, float gain)
	{
		if (gain >= 0.999f)
			return frame;

		var decoder = this._opus.CreateDecoder();

		try
		{
			var decodeBuffer = ArrayPool<byte>.Shared.Rent(this._format.SampleCountToSampleSize(this._format.GetMaximumBufferSize()));
			var encodeBuffer = ArrayPool<byte>.Shared.Rent(this._pcmFrameSize);

			try
			{
				var decodeSpan = decodeBuffer.AsSpan();
				this._opus.Decode(decoder, frame.Payload.Span, ref decodeSpan, false, out var decodedFormat);

				ApplyGainInPlace(decodeSpan, decodedFormat.ChannelCount, gain);

				var encodeSpan = encodeBuffer.AsSpan(0, decodeSpan.Length);
				this._opus.Encode(decodeSpan, ref encodeSpan);

				var output = new byte[encodeSpan.Length];
				encodeSpan.CopyTo(output);
				return new(output, frame.DurationMs, this._sequence++, frame.Timestamp);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(decodeBuffer);
				ArrayPool<byte>.Shared.Return(encodeBuffer);
			}
		}
		finally
		{
			this._opus.DestroyDecoder(decoder);
		}
	}

	/// <summary>
	///     Scales 16-bit LE PCM samples in place by the given gain multiplier.
	/// </summary>
	private static void ApplyGainInPlace(Span<byte> pcm, int channelCount, float gain)
	{
		_ = channelCount;

		var sampleCount = pcm.Length / sizeof(short);
		for (var i = 0; i < sampleCount; i++)
		{
			var offset = i * sizeof(short);
			var sample = (short)(pcm[offset] | (pcm[offset + 1] << 8));
			var scaled = (int)(sample * gain);
			var clamped = Math.Clamp(scaled, short.MinValue, short.MaxValue);
			pcm[offset] = (byte)(clamped & 0xFF);
			pcm[offset + 1] = (byte)((clamped >> 8) & 0xFF);
		}
	}

	/// <summary>
	///     Reads up to <paramref name="buffer" />.Length bytes from the stream, retrying until either
	///     the buffer is full or the stream ends.
	/// </summary>
	private static async Task<int> ReadExactOrPartialAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken)
	{
		var total = 0;

		while (total < buffer.Length)
		{
			var read = await stream.ReadAsync(buffer[total..], cancellationToken).ConfigureAwait(false);
			if (read == 0)
				break;

			total += read;
		}

		return total;
	}

	/// <summary>
	///     Writes a frame to the outgoing channel, yielding if the bounded channel cannot accept it immediately.
	/// </summary>
	private async ValueTask WriteOutgoingAsync(ExternalOpusFrame frame, CancellationToken cancellationToken)
	{
		while (!this._outgoing.Writer.TryWrite(frame))
		{
			cancellationToken.ThrowIfCancellationRequested();
			await Task.Yield();
		}
	}

	private bool ShouldSuppressMusic()
	{
		if (!this.HasActiveOverlay)
			return false;

		lock (this._gate)
			return this._pauseMusicForOverlays;
	}

	private float GetMusicGain()
	{
		lock (this._gate)
			return this._musicGain;
	}

	private void ThrowIfDisposed()
		=> ObjectDisposedException.ThrowIf(this._disposed, this);

	/// <summary>
	///     Represents a single overlay playback job in the serial overlay queue.
	/// </summary>
	private readonly record struct OverlayJob(string Name, Stream PcmStream, bool CloseStreamWhenFinished);
}
