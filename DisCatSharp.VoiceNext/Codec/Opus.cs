using System;
using System.Collections.Generic;

namespace DisCatSharp.VoiceNext.Codec;

/// <summary>
/// The opus.
/// </summary>
public class Opus : IDisposable
{
	/// <summary>
	/// Gets the audio format.
	/// </summary>
	public AudioFormat AudioFormat { get; }

	/// <summary>
	/// Gets the encoder.
	/// </summary>
	private readonly IntPtr _encoder;

	/// <summary>
	/// Gets the managed decoders.
	/// </summary>
	private readonly List<OpusDecoder> _managedDecoders;

	/// <summary>
	/// Initializes a new instance of the <see cref="Opus"/> class.
	/// </summary>
	/// <param name="audioFormat">The audio format.</param>
	public Opus(AudioFormat audioFormat)
	{
		if (!audioFormat.IsValid())
			throw new ArgumentException("Invalid audio format specified.", nameof(audioFormat));

		this.AudioFormat = audioFormat;
		this._encoder = Interop.OpusCreateEncoder(this.AudioFormat);

		// Set appropriate encoder options
		var sig = OpusSignal.Auto;
		switch (this.AudioFormat.VoiceApplication)
		{
			case VoiceApplication.Music:
				sig = OpusSignal.Music;
				break;

			case VoiceApplication.Voice:
				sig = OpusSignal.Voice;
				break;
		}
		Interop.OpusSetEncoderOption(this._encoder, OpusControl.SetSignal, (int)sig);
		Interop.OpusSetEncoderOption(this._encoder, OpusControl.SetPacketLossPercent, 15);
		Interop.OpusSetEncoderOption(this._encoder, OpusControl.SetInBandFec, 1);
		Interop.OpusSetEncoderOption(this._encoder, OpusControl.SetBitrate, 131072);

		this._managedDecoders = new List<OpusDecoder>();
	}

	/// <summary>
	/// Encodes the Opus.
	/// </summary>
	/// <param name="pcm">The pcm.</param>
	/// <param name="target">The target.</param>
	public void Encode(ReadOnlySpan<byte> pcm, ref Span<byte> target)
	{
		if (pcm.Length != target.Length)
			throw new ArgumentException("PCM and Opus buffer lengths need to be equal.", nameof(target));

		var duration = this.AudioFormat.CalculateSampleDuration(pcm.Length);
		var frameSize = this.AudioFormat.CalculateFrameSize(duration);
		var sampleSize = this.AudioFormat.CalculateSampleSize(duration);

		if (pcm.Length != sampleSize)
			throw new ArgumentException("Invalid PCM sample size.", nameof(target));

		Interop.OpusEncode(this._encoder, pcm, frameSize, ref target);
	}

	/// <summary>
	/// Decodes the Opus.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="opus">The opus.</param>
	/// <param name="target">The target.</param>
	/// <param name="useFec">If true, use fec.</param>
	/// <param name="outputFormat">The output format.</param>
	public void Decode(OpusDecoder decoder, ReadOnlySpan<byte> opus, ref Span<byte> target, bool useFec, out AudioFormat outputFormat)
	{
		//if (target.Length != this.AudioFormat.CalculateMaximumFrameSize())
		//    throw new ArgumentException("PCM target buffer size needs to be equal to maximum buffer size for specified audio format.", nameof(target));

		Interop.OpusGetPacketMetrics(opus, this.AudioFormat.SampleRate, out var channels, out var frames, out var samplesPerFrame, out var frameSize);
		outputFormat = this.AudioFormat.ChannelCount != channels ? new AudioFormat(this.AudioFormat.SampleRate, channels, this.AudioFormat.VoiceApplication) : this.AudioFormat;

		if (decoder.AudioFormat.ChannelCount != channels)
			decoder.Initialize(outputFormat);

		var sampleCount = Interop.OpusDecode(decoder.Decoder, opus, frameSize, target, useFec);

		var sampleSize = outputFormat.SampleCountToSampleSize(sampleCount);
		target = target[..sampleSize];
	}

	/// <summary>
	/// Processes the packet loss.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="frameSize">The frame size.</param>
	/// <param name="target">The target.</param>
	public void ProcessPacketLoss(OpusDecoder decoder, int frameSize, ref Span<byte> target) => Interop.OpusDecode(decoder.Decoder, frameSize, target);

	/// <summary>
	/// Gets the last packet sample count.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <returns>An int.</returns>
	public int GetLastPacketSampleCount(OpusDecoder decoder)
	{
		Interop.OpusGetLastPacketDuration(decoder.Decoder, out var sampleCount);
		return sampleCount;
	}

	/// <summary>
	/// Creates the decoder.
	/// </summary>
	/// <returns>An OpusDecoder.</returns>
	public OpusDecoder CreateDecoder()
	{
		lock (this._managedDecoders)
		{
			var managedDecoder = new OpusDecoder(this);
			this._managedDecoders.Add(managedDecoder);
			return managedDecoder;
		}
	}

	/// <summary>
	/// Destroys the decoder.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	public void DestroyDecoder(OpusDecoder decoder)
	{
		lock (this._managedDecoders)
		{
			if (!this._managedDecoders.Contains(decoder))
				return;

			this._managedDecoders.Remove(decoder);
			decoder.Dispose();
		}
	}

	/// <summary>
	/// Disposes the Opus.
	/// </summary>
	public void Dispose()
	{
		Interop.OpusDestroyEncoder(this._encoder);

		lock (this._managedDecoders)
		{
			foreach (var decoder in this._managedDecoders)
				decoder.Dispose();
		}
	}
}

/// <summary>
/// Represents an Opus decoder.
/// </summary>
public class OpusDecoder : IDisposable
{
	/// <summary>
	/// Gets the audio format produced by this decoder.
	/// </summary>
	public AudioFormat AudioFormat { get; private set; }

	/// <summary>
	/// Gets the opus.
	/// </summary>
	internal Opus Opus { get; }
	/// <summary>
	/// Gets the decoder.
	/// </summary>
	internal IntPtr Decoder { get; private set; }

	private volatile bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="OpusDecoder"/> class.
	/// </summary>
	/// <param name="managedOpus">The managed opus.</param>
	internal OpusDecoder(Opus managedOpus)
	{
		this.Opus = managedOpus;
	}

	/// <summary>
	/// Used to lazily initialize the decoder to make sure we're
	/// using the correct output format, this way we don't end up
	/// creating more decoders than we need.
	/// </summary>
	/// <param name="outputFormat"></param>
	internal void Initialize(AudioFormat outputFormat)
	{
		if (this.Decoder != IntPtr.Zero)
			Interop.OpusDestroyDecoder(this.Decoder);

		this.AudioFormat = outputFormat;

		this.Decoder = Interop.OpusCreateDecoder(outputFormat);
	}

	/// <summary>
	/// Disposes of this Opus decoder.
	/// </summary>
	public void Dispose()
	{
		if (this._isDisposed)
			return;

		this._isDisposed = true;
		if (this.Decoder != IntPtr.Zero)
			Interop.OpusDestroyDecoder(this.Decoder);
		GC.SuppressFinalize(this);
	}
}

/// <summary>
/// The opus error.
/// </summary>
[Flags]
internal enum OpusError
{
	Ok = 0,
	BadArgument = -1,
	BufferTooSmall = -2,
	InternalError = -3,
	InvalidPacket = -4,
	Unimplemented = -5,
	InvalidState = -6,
	AllocationFailure = -7
}

/// <summary>
/// The opus control.
/// </summary>
internal enum OpusControl : int
{
	SetBitrate = 4002,
	SetBandwidth = 4008,
	SetInBandFec = 4012,
	SetPacketLossPercent = 4014,
	SetSignal = 4024,
	ResetState = 4028,
	GetLastPacketDuration = 4039
}

/// <summary>
/// The opus signal.
/// </summary>
internal enum OpusSignal : int
{
	Auto = -1000,
	Voice = 3001,
	Music = 3002,
}
