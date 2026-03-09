using System;
using System.Collections.Generic;

using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Enums;
using DisCatSharp.Voice.Enums.Interop;
using DisCatSharp.Voice.Interop;

namespace DisCatSharp.Voice.Codec;

/// <summary>
///     The opus.
/// </summary>
public class Opus : IDisposable
{
	/// <summary>
	///     Gets the encoder.
	/// </summary>
	private readonly IntPtr _encoder;

	/// <summary>
	///     Gets the managed decoders.
	/// </summary>
	private readonly List<OpusDecoder> _managedDecoders;

	/// <summary>
	///     Initializes a new instance of the <see cref="Opus" /> class.
	/// </summary>
	/// <param name="audioFormat">The audio format.</param>
	public Opus(AudioFormat audioFormat)
	{
		if (!audioFormat.IsValid())
			throw new ArgumentException("Invalid audio format specified.", nameof(audioFormat));

		this.AudioFormat = audioFormat;
		this._encoder = OpusNative.OpusCreateEncoder(this.AudioFormat);

		// Set appropriate encoder options
		var sig = this.AudioFormat.VoiceApplication switch
		{
			VoiceApplication.Music => OpusSignal.Music,
			VoiceApplication.Voice => OpusSignal.Voice,
			_ => OpusSignal.Auto
		};

		OpusNative.OpusSetEncoderOption(this._encoder, OpusControl.SetSignal, (int)sig);
		OpusNative.OpusSetEncoderOption(this._encoder, OpusControl.SetPacketLossPercent, 15);
		OpusNative.OpusSetEncoderOption(this._encoder, OpusControl.SetInBandFec, 1);
		OpusNative.OpusSetEncoderOption(this._encoder, OpusControl.SetBitrate, 131072);

		this._managedDecoders = [];
	}

	/// <summary>
	///     Gets the audio format.
	/// </summary>
	public AudioFormat AudioFormat { get; }

	/// <summary>
	///     Disposes the Opus.
	/// </summary>
	public void Dispose()
	{
		OpusNative.OpusDestroyEncoder(this._encoder);

		lock (this._managedDecoders)
		{
			foreach (var decoder in this._managedDecoders)
				decoder.Dispose();
		}

		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Encodes the Opus.
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

		OpusNative.OpusEncode(this._encoder, pcm, frameSize, ref target);
	}

	/// <summary>
	///     Decodes the Opus.
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

		OpusNative.OpusGetPacketMetrics(opus, this.AudioFormat.SampleRate, out var channels, out _, out _, out var frameSize);
		outputFormat = this.AudioFormat.ChannelCount != channels
			? new(this.AudioFormat.SampleRate, channels, this.AudioFormat.VoiceApplication)
			: this.AudioFormat;

		if (decoder.AudioFormat.ChannelCount != channels)
			decoder.Initialize(outputFormat);

		var sampleCount = OpusNative.OpusDecode(decoder.Decoder, opus, frameSize, target, useFec);

		var sampleSize = outputFormat.SampleCountToSampleSize(sampleCount);
		target = target[..sampleSize];
	}

	/// <summary>
	///     Processes the packet loss.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="frameSize">The frame size.</param>
	/// <param name="target">The target.</param>
	public void ProcessPacketLoss(OpusDecoder decoder, int frameSize, ref Span<byte> target)
		=> OpusNative.OpusDecode(decoder.Decoder, frameSize, target);

	/// <summary>
	///     Gets the last packet sample count.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <returns>An int.</returns>
	public int GetLastPacketSampleCount(OpusDecoder decoder)
	{
		OpusNative.OpusGetLastPacketDuration(decoder.Decoder, out var sampleCount);
		return sampleCount;
	}

	/// <summary>
	///     Creates the decoder.
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
	///     Destroys the decoder.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	public void DestroyDecoder(OpusDecoder? decoder)
	{
		lock (this._managedDecoders)
		{
			if (decoder is null || !this._managedDecoders.Contains(decoder))
				return;

			this._managedDecoders.Remove(decoder);
			decoder.Dispose();
		}
	}
}
