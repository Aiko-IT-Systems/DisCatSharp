// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.VoiceNext.Codec
{
    /// <summary>
    /// This is an interop class. It contains wrapper methods for Opus and Sodium.
    /// </summary>
    internal static class Interop
    {
        #region Sodium wrapper
        /// <summary>
        /// The sodium library name.
        /// </summary>
        private const string SodiumLibraryName = "libsodium";

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
        [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_keybytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr _SodiumSecretBoxKeySize();

        /// <summary>
        /// _S the sodium secret box nonce size.
        /// </summary>
        /// <returns>An UIntPtr.</returns>
        [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr _SodiumSecretBoxNonceSize();

        /// <summary>
        /// _S the sodium secret box mac size.
        /// </summary>
        /// <returns>An UIntPtr.</returns>
        [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_macbytes")]
        [return: MarshalAs(UnmanagedType.SysUInt)]
        private static extern UIntPtr _SodiumSecretBoxMacSize();

        /// <summary>
        /// _S the sodium secret box create.
        /// </summary>
        /// <param name="Buffer">The buffer.</param>
        /// <param name="Message">The message.</param>
        /// <param name="MessageLength">The message length.</param>
        /// <param name="Nonce">The nonce.</param>
        /// <param name="Key">The key.</param>
        /// <returns>An int.</returns>
        [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
        private static unsafe extern int _SodiumSecretBoxCreate(byte* Buffer, byte* Message, ulong MessageLength, byte* Nonce, byte* Key);

        /// <summary>
        /// _S the sodium secret box open.
        /// </summary>
        /// <param name="Buffer">The buffer.</param>
        /// <param name="EncryptedMessage">The encrypted message.</param>
        /// <param name="EncryptedLength">The encrypted length.</param>
        /// <param name="Nonce">The nonce.</param>
        /// <param name="Key">The key.</param>
        /// <returns>An int.</returns>
        [DllImport(SodiumLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_open_easy")]
        private static unsafe extern int _SodiumSecretBoxOpen(byte* Buffer, byte* EncryptedMessage, ulong EncryptedLength, byte* Nonce, byte* Key);

        /// <summary>
        /// Encrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform encryption.
        /// </summary>
        /// <param name="Source">Contents to encrypt.</param>
        /// <param name="Target">Buffer to encrypt to.</param>
        /// <param name="Key">Key to use for encryption.</param>
        /// <param name="Nonce">Nonce to use for encryption.</param>
        /// <returns>Encryption status.</returns>
        public static unsafe int Encrypt(ReadOnlySpan<byte> Source, Span<byte> Target, ReadOnlySpan<byte> Key, ReadOnlySpan<byte> Nonce)
        {
            var status = 0;
            fixed (byte* sourcePtr = &Source.GetPinnableReference())
            fixed (byte* targetPtr = &Target.GetPinnableReference())
            fixed (byte* keyPtr = &Key.GetPinnableReference())
            fixed (byte* noncePtr = &Nonce.GetPinnableReference())
                status = _SodiumSecretBoxCreate(targetPtr, sourcePtr, (ulong)Source.Length, noncePtr, keyPtr);

            return status;
        }

        /// <summary>
        /// Decrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform decryption.
        /// </summary>
        /// <param name="Source">Buffer to decrypt from.</param>
        /// <param name="Target">Decrypted message buffer.</param>
        /// <param name="Key">Key to use for decryption.</param>
        /// <param name="Nonce">Nonce to use for decryption.</param>
        /// <returns>Decryption status.</returns>
        public static unsafe int Decrypt(ReadOnlySpan<byte> Source, Span<byte> Target, ReadOnlySpan<byte> Key, ReadOnlySpan<byte> Nonce)
        {
            var status = 0;
            fixed (byte* sourcePtr = &Source.GetPinnableReference())
            fixed (byte* targetPtr = &Target.GetPinnableReference())
            fixed (byte* keyPtr = &Key.GetPinnableReference())
            fixed (byte* noncePtr = &Nonce.GetPinnableReference())
                status = _SodiumSecretBoxOpen(targetPtr, sourcePtr, (ulong)Source.Length, noncePtr, keyPtr);

            return status;
        }
        #endregion

        #region Opus wrapper
        /// <summary>
        /// The opus library name.
        /// </summary>
        private const string OpusLibraryName = "libopus";

        /// <summary>
        /// _S the opus create encoder.
        /// </summary>
        /// <param name="SampleRate">The sample rate.</param>
        /// <param name="Channels">The channels.</param>
        /// <param name="Application">The application.</param>
        /// <param name="Error">The error.</param>
        /// <returns>An IntPtr.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
        private static extern IntPtr _OpusCreateEncoder(int SampleRate, int Channels, int Application, out OpusError Error);

        /// <summary>
        /// Opuses the destroy encoder.
        /// </summary>
        /// <param name="Encoder">The encoder.</param>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
        public static extern void OpusDestroyEncoder(IntPtr Encoder);

        /// <summary>
        /// _S the opus encode.
        /// </summary>
        /// <param name="Encoder">The encoder.</param>
        /// <param name="PcmData">The pcm data.</param>
        /// <param name="FrameSize">The frame size.</param>
        /// <param name="Data">The data.</param>
        /// <param name="MaxDataBytes">The max data bytes.</param>
        /// <returns>An int.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
        private static unsafe extern int _OpusEncode(IntPtr Encoder, byte* PcmData, int FrameSize, byte* Data, int MaxDataBytes);

        /// <summary>
        /// _S the opus encoder control.
        /// </summary>
        /// <param name="Encoder">The encoder.</param>
        /// <param name="Request">The request.</param>
        /// <param name="Value">The value.</param>
        /// <returns>An OpusError.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
        private static extern OpusError _OpusEncoderControl(IntPtr Encoder, OpusControl Request, int Value);

        /// <summary>
        /// _S the opus create decoder.
        /// </summary>
        /// <param name="SampleRate">The sample rate.</param>
        /// <param name="Channels">The channels.</param>
        /// <param name="Error">The error.</param>
        /// <returns>An IntPtr.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
        private static extern IntPtr _OpusCreateDecoder(int SampleRate, int Channels, out OpusError Error);

        /// <summary>
        /// Opuses the destroy decoder.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
        public static extern void OpusDestroyDecoder(IntPtr Decoder);

        /// <summary>
        /// _S the opus decode.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <param name="OpusData">The opus data.</param>
        /// <param name="OpusDataLength">The opus data length.</param>
        /// <param name="Data">The data.</param>
        /// <param name="FrameSize">The frame size.</param>
        /// <param name="DecodeFec">The decode fec.</param>
        /// <returns>An int.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decode")]
        private static unsafe extern int _OpusDecode(IntPtr Decoder, byte* OpusData, int OpusDataLength, byte* Data, int FrameSize, int DecodeFec);

        /// <summary>
        /// _S the opus get packet chanel count.
        /// </summary>
        /// <param name="OpusData">The opus data.</param>
        /// <returns>An int.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_channels")]
        private static unsafe extern int _OpusGetPacketChanelCount(byte* OpusData);

        /// <summary>
        /// _S the opus get packet frame count.
        /// </summary>
        /// <param name="OpusData">The opus data.</param>
        /// <param name="Length">The length.</param>
        /// <returns>An int.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_nb_frames")]
        private static unsafe extern int _OpusGetPacketFrameCount(byte* OpusData, int Length);

        /// <summary>
        /// _S the opus get packet sample per frame count.
        /// </summary>
        /// <param name="OpusData">The opus data.</param>
        /// <param name="SamplingRate">The sampling rate.</param>
        /// <returns>An int.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_packet_get_samples_per_frame")]
        private static unsafe extern int _OpusGetPacketSamplePerFrameCount(byte* OpusData, int SamplingRate);

        /// <summary>
        /// _S the opus decoder control.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <param name="Request">The request.</param>
        /// <param name="Value">The value.</param>
        /// <returns>An int.</returns>
        [DllImport(OpusLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_ctl")]
        private static extern int _OpusDecoderControl(IntPtr Decoder, OpusControl Request, out int Value);

        /// <summary>
        /// Opuses the create encoder.
        /// </summary>
        /// <param name="AudioFormat">The audio format.</param>
        /// <returns>An IntPtr.</returns>
        public static IntPtr OpusCreateEncoder(AudioFormat AudioFormat)
        {
            var encoder = _OpusCreateEncoder(AudioFormat.SampleRate, AudioFormat.ChannelCount, (int)AudioFormat.VoiceApplication, out var error);
            return error != OpusError.Ok ? throw new Exception($"Could not instantiate Opus encoder: {error} ({(int)error}).") : encoder;
        }

        /// <summary>
        /// Opuses the set encoder option.
        /// </summary>
        /// <param name="Encoder">The encoder.</param>
        /// <param name="Option">The option.</param>
        /// <param name="Value">The value.</param>
        public static void OpusSetEncoderOption(IntPtr Encoder, OpusControl Option, int Value)
        {
            var error = OpusError.Ok;
            if ((error = _OpusEncoderControl(Encoder, Option, Value)) != OpusError.Ok)
                throw new Exception($"Could not set Opus encoder option: {error} ({(int)error}).");
        }

        /// <summary>
        /// Opuses the encode.
        /// </summary>
        /// <param name="Encoder">The encoder.</param>
        /// <param name="Pcm">The pcm.</param>
        /// <param name="FrameSize">The frame size.</param>
        /// <param name="Opus">The opus.</param>
        public static unsafe void OpusEncode(IntPtr Encoder, ReadOnlySpan<byte> Pcm, int FrameSize, ref Span<byte> Opus)
        {
            var len = 0;

            fixed (byte* pcmPtr = &Pcm.GetPinnableReference())
            fixed (byte* opusPtr = &Opus.GetPinnableReference())
                len = _OpusEncode(Encoder, pcmPtr, FrameSize, opusPtr, Opus.Length);

            if (len < 0)
            {
                var error = (OpusError)len;
                throw new Exception($"Could not encode PCM data to Opus: {error} ({(int)error}).");
            }

            Opus = Opus[..len];
        }

        /// <summary>
        /// Opuses the create decoder.
        /// </summary>
        /// <param name="AudioFormat">The audio format.</param>
        /// <returns>An IntPtr.</returns>
        public static IntPtr OpusCreateDecoder(AudioFormat AudioFormat)
        {
            var decoder = _OpusCreateDecoder(AudioFormat.SampleRate, AudioFormat.ChannelCount, out var error);
            return error != OpusError.Ok ? throw new Exception($"Could not instantiate Opus decoder: {error} ({(int)error}).") : decoder;
        }

        /// <summary>
        /// Opuses the decode.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <param name="Opus">The opus.</param>
        /// <param name="FrameSize">The frame size.</param>
        /// <param name="Pcm">The pcm.</param>
        /// <param name="UseFec">If true, use fec.</param>
        /// <returns>An int.</returns>
        public static unsafe int OpusDecode(IntPtr Decoder, ReadOnlySpan<byte> Opus, int FrameSize, Span<byte> Pcm, bool UseFec)
        {
            var len = 0;

            fixed (byte* opusPtr = &Opus.GetPinnableReference())
            fixed (byte* pcmPtr = &Pcm.GetPinnableReference())
                len = _OpusDecode(Decoder, opusPtr, Opus.Length, pcmPtr, FrameSize, UseFec ? 1 : 0);

            if (len < 0)
            {
                var error = (OpusError)len;
                throw new Exception($"Could not decode PCM data from Opus: {error} ({(int)error}).");
            }

            return len;
        }

        /// <summary>
        /// Opuses the decode.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <param name="FrameSize">The frame size.</param>
        /// <param name="Pcm">The pcm.</param>
        /// <returns>An int.</returns>
        public static unsafe int OpusDecode(IntPtr Decoder, int FrameSize, Span<byte> Pcm)
        {
            var len = 0;

            fixed (byte* pcmPtr = &Pcm.GetPinnableReference())
                len = _OpusDecode(Decoder, null, 0, pcmPtr, FrameSize, 1);

            if (len < 0)
            {
                var error = (OpusError)len;
                throw new Exception($"Could not decode PCM data from Opus: {error} ({(int)error}).");
            }

            return len;
        }

        /// <summary>
        /// Opuses the get packet metrics.
        /// </summary>
        /// <param name="Opus">The opus.</param>
        /// <param name="SamplingRate">The sampling rate.</param>
        /// <param name="Channels">The channels.</param>
        /// <param name="Frames">The frames.</param>
        /// <param name="SamplesPerFrame">The samples per frame.</param>
        /// <param name="FrameSize">The frame size.</param>
        public static unsafe void OpusGetPacketMetrics(ReadOnlySpan<byte> Opus, int SamplingRate, out int Channels, out int Frames, out int SamplesPerFrame, out int FrameSize)
        {
            fixed (byte* opusPtr = &Opus.GetPinnableReference())
            {
                Frames = _OpusGetPacketFrameCount(opusPtr, Opus.Length);
                SamplesPerFrame = _OpusGetPacketSamplePerFrameCount(opusPtr, SamplingRate);
                Channels = _OpusGetPacketChanelCount(opusPtr);
            }

            FrameSize = Frames * SamplesPerFrame;
        }

        /// <summary>
        /// Opuses the get last packet duration.
        /// </summary>
        /// <param name="Decoder">The decoder.</param>
        /// <param name="SampleCount">The sample count.</param>
        public static void OpusGetLastPacketDuration(IntPtr Decoder, out int SampleCount) => _OpusDecoderControl(Decoder, OpusControl.GetLastPacketDuration, out SampleCount);
        #endregion
    }
}
