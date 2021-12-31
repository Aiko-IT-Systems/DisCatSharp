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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DisCatSharp.VoiceNext.Codec
{
    /// <summary>
    /// The sodium.
    /// </summary>
    internal sealed class Sodium : IDisposable
    {
        /// <summary>
        /// Gets the supported modes.
        /// </summary>
        public static IReadOnlyDictionary<string, EncryptionMode> SupportedModes { get; }

        /// <summary>
        /// Gets the nonce size.
        /// </summary>
        public static int NonceSize => Interop.SodiumNonceSize;

        /// <summary>
        /// Gets the c s p r n g.
        /// </summary>
        private RandomNumberGenerator Csprng { get; }
        /// <summary>
        /// Gets the buffer.
        /// </summary>
        private byte[] Buffer { get; }
        /// <summary>
        /// Gets the key.
        /// </summary>
        private ReadOnlyMemory<byte> Key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sodium"/> class.
        /// </summary>
        static Sodium()
        {
            SupportedModes = new ReadOnlyDictionary<string, EncryptionMode>(new Dictionary<string, EncryptionMode>()
            {
                ["xsalsa20_poly1305_lite"] = EncryptionMode.XSalsa20Poly1305Lite,
                ["xsalsa20_poly1305_suffix"] = EncryptionMode.XSalsa20Poly1305Suffix,
                ["xsalsa20_poly1305"] = EncryptionMode.XSalsa20Poly1305
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sodium"/> class.
        /// </summary>
        /// <param name="Key">The key.</param>
        public Sodium(ReadOnlyMemory<byte> Key)
        {
            if (Key.Length != Interop.SodiumKeySize)
                throw new ArgumentException($"Invalid Sodium key size. Key needs to have a length of {Interop.SodiumKeySize} bytes.", nameof(Key));

            this.Key = Key;

            this.Csprng = RandomNumberGenerator.Create();
            this.Buffer = new byte[Interop.SodiumNonceSize];
        }

        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <param name="RtpHeader">The rtp header.</param>
        /// <param name="Target">The target.</param>
        public void GenerateNonce(ReadOnlySpan<byte> RtpHeader, Span<byte> Target)
        {
            if (RtpHeader.Length != Rtp.HeaderSize)
                throw new ArgumentException($"RTP header needs to have a length of exactly {Rtp.HeaderSize} bytes.", nameof(RtpHeader));

            if (Target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(Target));

            // Write the header to the beginning of the span.
            RtpHeader.CopyTo(Target);

            // Zero rest of the span.
            Helpers.ZeroFill(Target[RtpHeader.Length..]);
        }

        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <param name="Target">The target.</param>
        public void GenerateNonce(Span<byte> Target)
        {
            if (Target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(Target));

            this.Csprng.GetBytes(this.Buffer);
            this.Buffer.AsSpan().CopyTo(Target);
        }

        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <param name="Nonce">The nonce.</param>
        /// <param name="Target">The target.</param>
        public void GenerateNonce(uint Nonce, Span<byte> Target)
        {
            if (Target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(Target));

            // Write the uint to memory
            BinaryPrimitives.WriteUInt32BigEndian(Target, Nonce);

            // Zero rest of the buffer.
            Helpers.ZeroFill(Target[4..]);
        }

        /// <summary>
        /// Appends the nonce.
        /// </summary>
        /// <param name="Nonce">The nonce.</param>
        /// <param name="Target">The target.</param>
        /// <param name="EncryptionMode">The encryption mode.</param>
        public void AppendNonce(ReadOnlySpan<byte> Nonce, Span<byte> Target, EncryptionMode EncryptionMode)
        {
            switch (EncryptionMode)
            {
                case EncryptionMode.XSalsa20Poly1305:
                    return;

                case EncryptionMode.XSalsa20Poly1305Suffix:
                    Nonce.CopyTo(Target[^12..]);
                    return;

                case EncryptionMode.XSalsa20Poly1305Lite:
                    Nonce[..4].CopyTo(Target[^4..]);
                    return;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(EncryptionMode));
            }
        }

        /// <summary>
        /// Gets the nonce.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="Target">The target.</param>
        /// <param name="EncryptionMode">The encryption mode.</param>
        public void GetNonce(ReadOnlySpan<byte> Source, Span<byte> Target, EncryptionMode EncryptionMode)
        {
            if (Target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(Target));

            switch (EncryptionMode)
            {
                case EncryptionMode.XSalsa20Poly1305:
                    Source[..12].CopyTo(Target);
                    return;

                case EncryptionMode.XSalsa20Poly1305Suffix:
                    Source[^Interop.SodiumNonceSize..].CopyTo(Target);
                    return;

                case EncryptionMode.XSalsa20Poly1305Lite:
                    Source[^4..].CopyTo(Target);
                    return;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(EncryptionMode));
            }
        }

        /// <summary>
        /// Encrypts the Sodium.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="Target">The target.</param>
        /// <param name="Nonce">The nonce.</param>
        public void Encrypt(ReadOnlySpan<byte> Source, Span<byte> Target, ReadOnlySpan<byte> Nonce)
        {
            if (Nonce.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(Nonce));

            if (Target.Length != Interop.SodiumMacSize + Source.Length)
                throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is a sum of input buffer length and Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(Target));

            int result;
            if ((result = Interop.Encrypt(Source, Target, this.Key.Span, Nonce)) != 0)
                throw new CryptographicException($"Could not encrypt the buffer. Sodium returned code {result}.");
        }

        /// <summary>
        /// Decrypts the Sodium.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="Target">The target.</param>
        /// <param name="Nonce">The nonce.</param>
        public void Decrypt(ReadOnlySpan<byte> Source, Span<byte> Target, ReadOnlySpan<byte> Nonce)
        {
            if (Nonce.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(Nonce));

            if (Target.Length != Source.Length - Interop.SodiumMacSize)
                throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is input buffer decreased by Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(Target));

            int result;
            if ((result = Interop.Decrypt(Source, Target, this.Key.Span, Nonce)) != 0)
                throw new CryptographicException($"Could not decrypt the buffer. Sodium returned code {result}.");
        }

        /// <summary>
        /// Disposes the Sodium.
        /// </summary>
        public void Dispose() => this.Csprng.Dispose();

        /// <summary>
        /// Selects the mode.
        /// </summary>
        /// <param name="AvailableModes">The available modes.</param>
        /// <returns>A KeyValuePair.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyValuePair<string, EncryptionMode> SelectMode(IEnumerable<string> AvailableModes)
        {
            foreach (var kvMode in SupportedModes)
                if (AvailableModes.Contains(kvMode.Key))
                    return kvMode;

            throw new CryptographicException("Could not negotiate Sodium encryption modes, as none of the modes offered by Discord are supported. This is usually an indicator that something went very wrong.");
        }

        /// <summary>
        /// Calculates the target size.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns>An int.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateTargetSize(ReadOnlySpan<byte> Source)
            => Source.Length + Interop.SodiumMacSize;

        /// <summary>
        /// Calculates the source size.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns>An int.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateSourceSize(ReadOnlySpan<byte> Source)
            => Source.Length - Interop.SodiumMacSize;
    }

    /// <summary>
    /// Specifies an encryption mode to use with Sodium.
    /// </summary>
    public enum EncryptionMode
    {
        /// <summary>
        /// The nonce is an incrementing uint32 value. It is encoded as big endian value at the beginning of the nonce buffer. The 4 bytes are also appended at the end of the packet.
        /// </summary>
        XSalsa20Poly1305Lite,

        /// <summary>
        /// The nonce consists of random bytes. It is appended at the end of a packet.
        /// </summary>
        XSalsa20Poly1305Suffix,

        /// <summary>
        /// The nonce consists of the RTP header. Nothing is appended to the packet.
        /// </summary>
        XSalsa20Poly1305
    }
}
