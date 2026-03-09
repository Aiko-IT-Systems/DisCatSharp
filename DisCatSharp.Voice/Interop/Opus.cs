using System;
using System.Runtime.InteropServices;

using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Enums;
using DisCatSharp.Voice.Enums.Interop;

namespace DisCatSharp.Voice.Interop;

/// <summary>
///     This is an interop class. It contains wrapper methods for Opus.
/// </summary>
internal static class OpusNative
{
	/// <summary>
	///     The opus library name.
	/// </summary>
	private const string OPUS_LIBRARY_NAME = "libopus";

	/// <summary>
	///     Native binding for <c>opus_encoder_create</c>.
	/// </summary>
	/// <param name="sampleRate">The sample rate.</param>
	/// <param name="channels">The channels.</param>
	/// <param name="application">The application.</param>
	/// <param name="error">The error.</param>
	/// <returns>An IntPtr.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
	private static extern IntPtr _OpusCreateEncoder(int sampleRate, int channels, int application, out OpusError error);

	/// <summary>
	///     Destroys an Opus encoder instance.
	/// </summary>
	/// <param name="encoder">The encoder.</param>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
	public static extern void OpusDestroyEncoder(IntPtr encoder);

	/// <summary>
	///     Native binding for <c>opus_encode</c>.
	/// </summary>
	/// <param name="encoder">The encoder.</param>
	/// <param name="pcmData">The pcm data.</param>
	/// <param name="frameSize">The frame size.</param>
	/// <param name="data">The data.</param>
	/// <param name="maxDataBytes">The max data bytes.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
	private static extern unsafe int _OpusEncode(IntPtr encoder, byte* pcmData, int frameSize, byte* data, int maxDataBytes);

	/// <summary>
	///     Native binding for <c>opus_encoder_ctl</c>.
	/// </summary>
	/// <param name="encoder">The encoder.</param>
	/// <param name="request">The request.</param>
	/// <param name="value">The value.</param>
	/// <returns>An OpusError.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
	private static extern OpusError _OpusEncoderControl(IntPtr encoder, OpusControl request, int value);

	/// <summary>
	///     Native binding for <c>opus_decoder_create</c>.
	/// </summary>
	/// <param name="sampleRate">The sample rate.</param>
	/// <param name="channels">The channels.</param>
	/// <param name="error">The error.</param>
	/// <returns>An IntPtr.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
	private static extern IntPtr _OpusCreateDecoder(int sampleRate, int channels, out OpusError error);

	/// <summary>
	///     Destroys an Opus decoder instance.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
	public static extern void OpusDestroyDecoder(IntPtr decoder);

	/// <summary>
	///     Native binding for <c>opus_decode</c>.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="opusData">The opus data.</param>
	/// <param name="opusDataLength">The opus data length.</param>
	/// <param name="data">The data.</param>
	/// <param name="frameSize">The frame size.</param>
	/// <param name="decodeFec">The decode fec.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decode")]
	private static extern unsafe int _OpusDecode(IntPtr decoder, byte* opusData, int opusDataLength, byte* data, int frameSize, int decodeFec);

	/// <summary>
	///     Native binding for <c>opus_packet_get_nb_channels</c>.
	/// </summary>
	/// <param name="opusData">The opus data.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_channels")]
	private static extern unsafe int _OpusGetPacketChannelCount(byte* opusData);

	/// <summary>
	///     Native binding for <c>opus_packet_get_nb_frames</c>.
	/// </summary>
	/// <param name="opusData">The opus data.</param>
	/// <param name="length">The length.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_frames")]
	private static extern unsafe int _OpusGetPacketFrameCount(byte* opusData, int length);

	/// <summary>
	///     Native binding for <c>opus_packet_get_samples_per_frame</c>.
	/// </summary>
	/// <param name="opusData">The opus data.</param>
	/// <param name="samplingRate">The sampling rate.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_samples_per_frame")]
	private static extern unsafe int _OpusGetPacketSamplePerFrameCount(byte* opusData, int samplingRate);

	/// <summary>
	///     Native binding for <c>opus_decoder_ctl</c>.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="request">The request.</param>
	/// <param name="value">The value.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_ctl")]
	private static extern int _OpusDecoderControl(IntPtr decoder, OpusControl request, out int value);

	/// <summary>
	///     Creates an Opus encoder for the specified audio format.
	/// </summary>
	/// <param name="audioFormat">The audio format.</param>
	/// <returns>An IntPtr.</returns>
	public static IntPtr OpusCreateEncoder(AudioFormat audioFormat)
	{
		var encoder = _OpusCreateEncoder(audioFormat.SampleRate, audioFormat.ChannelCount, (int)audioFormat.VoiceApplication, out var error);
		return error != OpusError.Ok ? throw new($"Could not instantiate Opus encoder: {error} ({(int)error}).") : encoder;
	}

	/// <summary>
	///     Applies an encoder control option to a native Opus encoder.
	/// </summary>
	/// <param name="encoder">The encoder.</param>
	/// <param name="option">The option.</param>
	/// <param name="value">The value.</param>
	public static void OpusSetEncoderOption(IntPtr encoder, OpusControl option, int value)
	{
		var error = OpusError.Ok;
		if ((error = _OpusEncoderControl(encoder, option, value)) != OpusError.Ok)
			throw new($"Could not set Opus encoder option: {error} ({(int)error}).");
	}

	/// <summary>
	///     Encodes a PCM frame into Opus.
	/// </summary>
	/// <param name="encoder">The encoder.</param>
	/// <param name="pcm">The pcm.</param>
	/// <param name="frameSize">The frame size.</param>
	/// <param name="opus">The opus.</param>
	public static unsafe void OpusEncode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize, ref Span<byte> opus)
	{
		var len = 0;

		fixed (byte* pcmPtr = &pcm.GetPinnableReference())
		fixed (byte* opusPtr = &opus.GetPinnableReference())
		{
			len = _OpusEncode(encoder, pcmPtr, frameSize, opusPtr, opus.Length);
		}

		if (len < 0)
		{
			var error = (OpusError)len;
			throw new($"Could not encode PCM data to Opus: {error} ({(int)error}).");
		}

		opus = opus[..len];
	}

	/// <summary>
	///     Creates an Opus decoder for the specified audio format.
	/// </summary>
	/// <param name="audioFormat">The audio format.</param>
	/// <returns>An IntPtr.</returns>
	public static IntPtr OpusCreateDecoder(AudioFormat audioFormat)
	{
		var decoder = _OpusCreateDecoder(audioFormat.SampleRate, audioFormat.ChannelCount, out var error);
		return error != OpusError.Ok ? throw new($"Could not instantiate Opus decoder: {error} ({(int)error}).") : decoder;
	}

	/// <summary>
	///     Decodes an Opus frame into PCM.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="opus">The opus.</param>
	/// <param name="frameSize">The frame size.</param>
	/// <param name="pcm">The pcm.</param>
	/// <param name="useFec">If true, use fec.</param>
	/// <returns>An int.</returns>
	public static unsafe int OpusDecode(IntPtr decoder, ReadOnlySpan<byte> opus, int frameSize, Span<byte> pcm, bool useFec)
	{
		var len = 0;

		fixed (byte* opusPtr = &opus.GetPinnableReference())
		fixed (byte* pcmPtr = &pcm.GetPinnableReference())
		{
			len = _OpusDecode(decoder, opusPtr, opus.Length, pcmPtr, frameSize, useFec ? 1 : 0);
		}

		if (len < 0)
		{
			var error = (OpusError)len;
			throw new($"Could not decode PCM data from Opus: {error} ({(int)error}).");
		}

		return len;
	}

	/// <summary>
	///     Decodes a concealment (packet-loss) frame into PCM.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="frameSize">The frame size.</param>
	/// <param name="pcm">The pcm.</param>
	/// <returns>An int.</returns>
	public static unsafe int OpusDecode(IntPtr decoder, int frameSize, Span<byte> pcm)
	{
		var len = 0;

		fixed (byte* pcmPtr = &pcm.GetPinnableReference())
		{
			len = _OpusDecode(decoder, null, 0, pcmPtr, frameSize, 1);
		}

		if (len < 0)
		{
			var error = (OpusError)len;
			throw new($"Could not decode PCM data from Opus: {error} ({(int)error}).");
		}

		return len;
	}

	/// <summary>
	///     Extracts packet-level Opus metrics from an encoded frame.
	/// </summary>
	/// <param name="opus">The opus.</param>
	/// <param name="samplingRate">The sampling rate.</param>
	/// <param name="channels">The channels.</param>
	/// <param name="frames">The frames.</param>
	/// <param name="samplesPerFrame">The samples per frame.</param>
	/// <param name="frameSize">The frame size.</param>
	public static unsafe void OpusGetPacketMetrics(ReadOnlySpan<byte> opus, int samplingRate, out int channels, out int frames, out int samplesPerFrame, out int frameSize)
	{
		fixed (byte* opusPtr = &opus.GetPinnableReference())
		{
			frames = _OpusGetPacketFrameCount(opusPtr, opus.Length);
			samplesPerFrame = _OpusGetPacketSamplePerFrameCount(opusPtr, samplingRate);
			channels = _OpusGetPacketChannelCount(opusPtr);
		}

		frameSize = frames * samplesPerFrame;
	}

	/// <summary>
	///     Reads the decoder's last packet duration (in samples).
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="sampleCount">The sample count.</param>
	public static void OpusGetLastPacketDuration(IntPtr decoder, out int sampleCount)
		=> _OpusDecoderControl(decoder, OpusControl.GetLastPacketDuration, out sampleCount);

	public static IntPtr CreateEncoder(int sampleRate, int channelCount, int application)
	{
		var format = new AudioFormat(sampleRate, channelCount, (VoiceApplication)application);
		return OpusCreateEncoder(format);
	}

	public static void SetEncoderOption(IntPtr encoder, OpusControl option, int value)
		=> OpusSetEncoderOption(encoder, option, value);

	public static void Encode(IntPtr encoder, ReadOnlySpan<byte> pcm, int frameSize, ref Span<byte> data)
		=> OpusEncode(encoder, pcm, frameSize, ref data);

	public static IntPtr CreateDecoder(int sampleRate, int channelCount)
	{
		var format = new AudioFormat(sampleRate, channelCount);
		return OpusCreateDecoder(format);
	}

	public static int Decode(IntPtr decoder, ReadOnlySpan<byte> data, int frameSize, Span<byte> pcm, bool useFec)
		=> OpusDecode(decoder, data, frameSize, pcm, useFec);

	public static int Decode(IntPtr decoder, int frameSize, Span<byte> pcm)
		=> OpusDecode(decoder, frameSize, pcm);

	public static OpusPacketMetrics GetPacketMetrics(ReadOnlySpan<byte> data, int samplingRate)
	{
		OpusGetPacketMetrics(data, samplingRate, out var channels, out var frames, out var samplesPerFrame, out var frameSize);
		return new()
		{
			ChannelCount = channels,
			FrameCount = frames,
			SamplesPerFrame = samplesPerFrame,
			FrameSize = frameSize
		};
	}

	public static void GetLastPacketDuration(IntPtr decoder, out int sampleCount)
		=> OpusGetLastPacketDuration(decoder, out sampleCount);
}
