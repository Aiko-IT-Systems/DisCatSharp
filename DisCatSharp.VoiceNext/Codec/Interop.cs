using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.VoiceNext.Codec;

/// <summary>
/// This is an interop class. It contains wrapper methods for Opus and Sodium.
/// </summary>
internal static class Interop
{
#region Sodium wrapper

	/// <summary>
	/// The sodium library name.
	/// </summary>
	private const string SODIUM_LIBRARY_NAME = "libsodium";

	/// <summary>
	/// Gets the Sodium key size for xsalsa20_poly1305 algorithm.
	/// </summary>
	public static int SodiumKeySize { get; } = (int)_SodiumSecretBoxKeySize();

	/// <summary>
	/// Gets the Sodium nonce size for xsalsa20_poly1305 algorithm.
	/// </summary>
	public static int SodiumNonceSize { get; } = (int)_SodiumSecretBoxNonceSize();

	/// <summary>
	/// Gets the Sodium MAC size for xsalsa20_poly1305 algorithm.
	/// </summary>
	public static int SodiumMacSize { get; } = (int)_SodiumSecretBoxMacSize();

	/// <summary>
	/// _S the sodium secret box key size.
	/// </summary>
	/// <returns>An UIntPtr.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_keybytes")]
	[return: MarshalAs(UnmanagedType.SysUInt)]
	private static extern UIntPtr _SodiumSecretBoxKeySize();

	/// <summary>
	/// _S the sodium secret box nonce size.
	/// </summary>
	/// <returns>An UIntPtr.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
	[return: MarshalAs(UnmanagedType.SysUInt)]
	private static extern UIntPtr _SodiumSecretBoxNonceSize();

	/// <summary>
	/// _S the sodium secret box mac size.
	/// </summary>
	/// <returns>An UIntPtr.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_macbytes")]
	[return: MarshalAs(UnmanagedType.SysUInt)]
	private static extern UIntPtr _SodiumSecretBoxMacSize();

	/// <summary>
	/// _S the sodium secret box create.
	/// </summary>
	/// <param name="buffer">The buffer.</param>
	/// <param name="message">The message.</param>
	/// <param name="messageLength">The message length.</param>
	/// <param name="nonce">The nonce.</param>
	/// <param name="key">The key.</param>
	/// <returns>An int.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
	private static extern unsafe int _SodiumSecretBoxCreate(byte* buffer, byte* message, ulong messageLength, byte* nonce, byte* key);

	/// <summary>
	/// _S the sodium secret box open.
	/// </summary>
	/// <param name="buffer">The buffer.</param>
	/// <param name="encryptedMessage">The encrypted message.</param>
	/// <param name="encryptedLength">The encrypted length.</param>
	/// <param name="nonce">The nonce.</param>
	/// <param name="key">The key.</param>
	/// <returns>An int.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_open_easy")]
	private static extern unsafe int _SodiumSecretBoxOpen(byte* buffer, byte* encryptedMessage, ulong encryptedLength, byte* nonce, byte* key);

	/// <summary>
	/// Encrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform encryption.
	/// </summary>
	/// <param name="source">Contents to encrypt.</param>
	/// <param name="target">Buffer to encrypt to.</param>
	/// <param name="key">Key to use for encryption.</param>
	/// <param name="nonce">Nonce to use for encryption.</param>
	/// <returns>Encryption status.</returns>
	public static unsafe int Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
	{
		var status = 0;
		fixed (byte* sourcePtr = &source.GetPinnableReference())
		fixed (byte* targetPtr = &target.GetPinnableReference())
		fixed (byte* keyPtr = &key.GetPinnableReference())
		fixed (byte* noncePtr = &nonce.GetPinnableReference())
		{
			status = _SodiumSecretBoxCreate(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);
		}

		return status;
	}

	/// <summary>
	/// Decrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform decryption.
	/// </summary>
	/// <param name="source">Buffer to decrypt from.</param>
	/// <param name="target">Decrypted message buffer.</param>
	/// <param name="key">Key to use for decryption.</param>
	/// <param name="nonce">Nonce to use for decryption.</param>
	/// <returns>Decryption status.</returns>
	public static unsafe int Decrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
	{
		var status = 0;
		fixed (byte* sourcePtr = &source.GetPinnableReference())
		fixed (byte* targetPtr = &target.GetPinnableReference())
		fixed (byte* keyPtr = &key.GetPinnableReference())
		fixed (byte* noncePtr = &nonce.GetPinnableReference())
		{
			status = _SodiumSecretBoxOpen(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);
		}

		return status;
	}

#endregion

#region Opus wrapper

	/// <summary>
	/// The opus library name.
	/// </summary>
	private const string OPUS_LIBRARY_NAME = "libopus";

	/// <summary>
	/// _S the opus create encoder.
	/// </summary>
	/// <param name="sampleRate">The sample rate.</param>
	/// <param name="channels">The channels.</param>
	/// <param name="application">The application.</param>
	/// <param name="error">The error.</param>
	/// <returns>An IntPtr.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
	private static extern IntPtr _OpusCreateEncoder(int sampleRate, int channels, int application, out OpusError error);

	/// <summary>
	/// Opuses the destroy encoder.
	/// </summary>
	/// <param name="encoder">The encoder.</param>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
	public static extern void OpusDestroyEncoder(IntPtr encoder);

	/// <summary>
	/// _S the opus encode.
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
	/// _S the opus encoder control.
	/// </summary>
	/// <param name="encoder">The encoder.</param>
	/// <param name="request">The request.</param>
	/// <param name="value">The value.</param>
	/// <returns>An OpusError.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
	private static extern OpusError _OpusEncoderControl(IntPtr encoder, OpusControl request, int value);

	/// <summary>
	/// _S the opus create decoder.
	/// </summary>
	/// <param name="sampleRate">The sample rate.</param>
	/// <param name="channels">The channels.</param>
	/// <param name="error">The error.</param>
	/// <returns>An IntPtr.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
	private static extern IntPtr _OpusCreateDecoder(int sampleRate, int channels, out OpusError error);

	/// <summary>
	/// Opuses the destroy decoder.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
	public static extern void OpusDestroyDecoder(IntPtr decoder);

	/// <summary>
	/// _S the opus decode.
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
	/// _S the opus get packet channel count.
	/// </summary>
	/// <param name="opusData">The opus data.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_channels")]
	private static extern unsafe int _OpusGetPacketChannelCount(byte* opusData);

	/// <summary>
	/// _S the opus get packet frame count.
	/// </summary>
	/// <param name="opusData">The opus data.</param>
	/// <param name="length">The length.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_frames")]
	private static extern unsafe int _OpusGetPacketFrameCount(byte* opusData, int length);

	/// <summary>
	/// _S the opus get packet sample per frame count.
	/// </summary>
	/// <param name="opusData">The opus data.</param>
	/// <param name="samplingRate">The sampling rate.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_samples_per_frame")]
	private static extern unsafe int _OpusGetPacketSamplePerFrameCount(byte* opusData, int samplingRate);

	/// <summary>
	/// _S the opus decoder control.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="request">The request.</param>
	/// <param name="value">The value.</param>
	/// <returns>An int.</returns>
	[DllImport(OPUS_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_ctl")]
	private static extern int _OpusDecoderControl(IntPtr decoder, OpusControl request, out int value);

	/// <summary>
	/// Opuses the create encoder.
	/// </summary>
	/// <param name="audioFormat">The audio format.</param>
	/// <returns>An IntPtr.</returns>
	public static IntPtr OpusCreateEncoder(AudioFormat audioFormat)
	{
		var encoder = _OpusCreateEncoder(audioFormat.SampleRate, audioFormat.ChannelCount, (int)audioFormat.VoiceApplication, out var error);
		return error != OpusError.Ok ? throw new($"Could not instantiate Opus encoder: {error} ({(int)error}).") : encoder;
	}

	/// <summary>
	/// Opuses the set encoder option.
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
	/// Opuses the encode.
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
	/// Opuses the create decoder.
	/// </summary>
	/// <param name="audioFormat">The audio format.</param>
	/// <returns>An IntPtr.</returns>
	public static IntPtr OpusCreateDecoder(AudioFormat audioFormat)
	{
		var decoder = _OpusCreateDecoder(audioFormat.SampleRate, audioFormat.ChannelCount, out var error);
		return error != OpusError.Ok ? throw new($"Could not instantiate Opus decoder: {error} ({(int)error}).") : decoder;
	}

	/// <summary>
	/// Opuses the decode.
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
	/// Opuses the decode.
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
	/// Opuses the get packet metrics.
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
	/// Opuses the get last packet duration.
	/// </summary>
	/// <param name="decoder">The decoder.</param>
	/// <param name="sampleCount">The sample count.</param>
	public static void OpusGetLastPacketDuration(IntPtr decoder, out int sampleCount)
		=> _OpusDecoderControl(decoder, OpusControl.GetLastPacketDuration, out sampleCount);

#endregion
}
