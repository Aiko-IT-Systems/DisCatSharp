using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Interfaces;

using Xunit;

namespace DisCatSharp.CopilotTests;

/// <summary>
///     Tests for <see cref="VoiceOutputController" />.
/// </summary>
public sealed class VoiceOutputControllerTests
{
	/// <summary>
	///     A stub Opus source that yields a configurable number of fake frames.
	/// </summary>
	private sealed class FakeOpusSource(int frameCount = 5) : IExternalOpusSource
	{
		public int FramesRead { get; private set; }

		public async IAsyncEnumerable<ExternalOpusFrame> ReadFramesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (var i = 0; i < frameCount; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await Task.Yield();

				// Produce a non-empty payload so it doesn't look like silence
				var payload = new byte[80];
				Array.Fill(payload, (byte)(i + 1));
				this.FramesRead++;
				yield return new(payload, 20, (uint)i, 0);
			}
		}
	}

	/// <summary>
	///     A stub Opus source that blocks until cancelled (simulates infinite music).
	/// </summary>
	private sealed class InfiniteOpusSource : IExternalOpusSource
	{
		public int FramesRead { get; private set; }

		public async IAsyncEnumerable<ExternalOpusFrame> ReadFramesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var seq = 0u;
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(5, cancellationToken).ConfigureAwait(false);
				var payload = new byte[80];
				Array.Fill(payload, (byte)0xAB);
				this.FramesRead++;
				yield return new(payload, 20, seq++, 0);
			}
		}
	}

	[Fact]
	public async Task SingleReaderEnforcement_ThrowsOnSecondReader()
	{
		await using var controller = new VoiceOutputController();

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

		// First reader starts fine
		var reader1 = controller.ReadFramesAsync(cts.Token).GetAsyncEnumerator(cts.Token);

		// Second reader should throw
		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await using var reader2 = controller.ReadFramesAsync(cts.Token).GetAsyncEnumerator(cts.Token);
			await reader2.MoveNextAsync();
		});

		await reader1.DisposeAsync();
	}

	[Fact]
	public async Task MusicPassthrough_FramesFlowToOutput()
	{
		await using var controller = new VoiceOutputController();

		var source = new FakeOpusSource(3);
		await controller.SetMusicSourceAsync(source);

		var frames = new List<ExternalOpusFrame>();
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

		await foreach (var frame in controller.ReadFramesAsync(cts.Token))
		{
			if (!frame.IsSilence)
				frames.Add(frame);

			if (frames.Count >= 3)
				break;
		}

		Assert.Equal(3, frames.Count);
		Assert.All(frames, f => Assert.False(f.Payload.IsEmpty));
	}

	[Fact]
	public async Task SetMusicSourceNull_StopsMusic()
	{
		await using var controller = new VoiceOutputController();

		var source = new InfiniteOpusSource();
		await controller.SetMusicSourceAsync(source);

		// Let some frames flow
		await Task.Delay(50);

		// Unbind music
		await controller.SetMusicSourceAsync(null);

		// The infinite source's pump should have been cancelled
		Assert.True(source.FramesRead > 0);
	}

	[Fact]
	public async Task RebindMusicSource_CleanlySwaps()
	{
		await using var controller = new VoiceOutputController();

		var source1 = new InfiniteOpusSource();
		var source2 = new FakeOpusSource(2);

		await controller.SetMusicSourceAsync(source1);
		await Task.Delay(50);

		// Rebind to a different source
		await controller.SetMusicSourceAsync(source2);

		// Source1 should have been cancelled
		Assert.True(source1.FramesRead > 0);
	}

	[Fact]
	public async Task OverlayQueuing_PlaysSerially()
	{
		await using var controller = new VoiceOutputController();

		// 48kHz stereo 20ms = 3840 bytes per frame
		var pcmFrameSize = 48000 / 1000 * 20 * 2 * 2;
		var overlay1 = new MemoryStream(new byte[pcmFrameSize * 2]); // 2 frames
		var overlay2 = new MemoryStream(new byte[pcmFrameSize]);     // 1 frame

		await controller.QueuePcmOverlayAsync(overlay1, "overlay-1");
		await controller.QueuePcmOverlayAsync(overlay2, "overlay-2");

		var frames = new List<ExternalOpusFrame>();
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

		await foreach (var frame in controller.ReadFramesAsync(cts.Token))
		{
			frames.Add(frame);

			// 2 frames from overlay1 + 1 frame from overlay2 = 3
			if (frames.Count >= 3)
				break;
		}

		Assert.Equal(3, frames.Count);
	}

	[Fact]
	public async Task QueueOwnedPcmOverlay_DisposesStream()
	{
		await using var controller = new VoiceOutputController();

		var pcmFrameSize = 48000 / 1000 * 20 * 2 * 2;
		var stream = new MemoryStream(new byte[pcmFrameSize]);

		await controller.QueueOwnedPcmOverlayAsync(stream, "owned");

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

		await foreach (var frame in controller.ReadFramesAsync(cts.Token))
		{
			// Read at least one frame then break
			break;
		}

		// Allow a moment for async disposal
		await Task.Delay(200);

		// Stream should be disposed (CanRead returns false after disposal)
		Assert.False(stream.CanRead);
	}

	[Fact]
	public async Task MusicSuppression_SilenceWhenOverlayActive()
	{
		await using var controller = new VoiceOutputController();

		// PauseMusicForOverlays is true by default
		Assert.True(controller.PauseMusicForOverlays);

		var musicSource = new InfiniteOpusSource();
		await controller.SetMusicSourceAsync(musicSource);

		// Queue an overlay
		var pcmFrameSize = 48000 / 1000 * 20 * 2 * 2;
		var overlay = new MemoryStream(new byte[pcmFrameSize * 3]); // 3 frames
		await controller.QueuePcmOverlayAsync(overlay, "tts");

		// Give the overlay loop time to pick up the job
		await Task.Delay(50);

		Assert.True(controller.HasActiveOverlay);
	}

	[Fact]
	public async Task MusicGain_DefaultIsFullVolume()
	{
		await using var controller = new VoiceOutputController();

		Assert.Equal(1.0f, controller.MusicGain);
	}

	[Fact]
	public async Task SetDucking_ChangesGain()
	{
		await using var controller = new VoiceOutputController();

		controller.SetDucking(true, 0.3f);
		Assert.Equal(0.3f, controller.MusicGain);

		controller.SetDucking(false);
		Assert.Equal(1.0f, controller.MusicGain);
	}

	[Fact]
	public async Task MusicGain_ClampedTo01()
	{
		await using var controller = new VoiceOutputController();

		controller.MusicGain = 5.0f;
		Assert.Equal(1.0f, controller.MusicGain);

		controller.MusicGain = -2.0f;
		Assert.Equal(0.0f, controller.MusicGain);
	}

	[Fact]
	public async Task Dispose_CompletesCleanly()
	{
		var controller = new VoiceOutputController();

		var musicSource = new InfiniteOpusSource();
		await controller.SetMusicSourceAsync(musicSource);

		// Queue an overlay
		var pcmFrameSize = 48000 / 1000 * 20 * 2 * 2;
		await controller.QueuePcmOverlayAsync(new MemoryStream(new byte[pcmFrameSize]), "test");

		// Dispose should not throw
		await controller.DisposeAsync();

		// Double dispose should also be safe
		await controller.DisposeAsync();
	}

	[Fact]
	public async Task ThrowsAfterDisposal()
	{
		var controller = new VoiceOutputController();
		await controller.DisposeAsync();

		await Assert.ThrowsAsync<ObjectDisposedException>(() => controller.SetMusicSourceAsync(null));
		await Assert.ThrowsAsync<ObjectDisposedException>(async () => await controller.QueuePcmOverlayAsync(new MemoryStream()));
		Assert.Throws<ObjectDisposedException>(() => controller.SetDucking(true));
	}
}
