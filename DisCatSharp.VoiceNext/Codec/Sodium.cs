// This file is part of the DisCatSharp project.
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
        private RandomNumberGenerator CSPRNG { get; }
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
                ["xsalsa20_poly1305_lite"] = EncryptionMode.XSalsa20_Poly1305_Lite,
                ["xsalsa20_poly1305_suffix"] = EncryptionMode.XSalsa20_Poly1305_Suffix,
                ["xsalsa20_poly1305"] = EncryptionMode.XSalsa20_Poly1305
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sodium"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public Sodium(ReadOnlyMemory<byte> key)
        {
            if (key.Length != Interop.SodiumKeySize)
                throw new ArgumentException($"Invalid Sodium key size. Key needs to have a length of {Interop.SodiumKeySize} bytes.", nameof(key));

            this.Key = key;

            this.CSPRNG = RandomNumberGenerator.Create();
            this.Buffer = new byte[Interop.SodiumNonceSize];
        }

        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <param name="rtpHeader">The rtp header.</param>
        /// <param name="target">The target.</param>
        public void GenerateNonce(ReadOnlySpan<byte> rtpHeader, Span<byte> target)
        {
            if (rtpHeader.Length != Rtp.HeaderSize)
                throw new ArgumentException($"RTP header needs to have a length of exactly {Rtp.HeaderSize} bytes.", nameof(rtpHeader));

            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            // Write the header to the beginning of the span.
            rtpHeader.CopyTo(target);

            // Zero rest of the span.
            Helpers.ZeroFill(target.Slice(rtpHeader.Length));
        }

        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <param name="target">The target.</param>
        public void GenerateNonce(Span<byte> target)
        {
            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            this.CSPRNG.GetBytes(this.Buffer);
            this.Buffer.AsSpan().CopyTo(target);
        }

        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <param name="target">The target.</param>
        public void GenerateNonce(uint nonce, Span<byte> target)
        {
            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            // Write the uint to memory
            BinaryPrimitives.WriteUInt32BigEndian(target, nonce);

            // Zero rest of the buffer.
            Helpers.ZeroFill(target.Slice(4));
        }

        /// <summary>
        /// Appends the nonce.
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <param name="target">The target.</param>
        /// <param name="encryptionMode">The encryption mode.</param>
        public void AppendNonce(ReadOnlySpan<byte> nonce, Span<byte> target, EncryptionMode encryptionMode)
        {
            switch (encryptionMode)
            {
                case EncryptionMode.XSalsa20_Poly1305:
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Suffix:
                    nonce.CopyTo(target.Slice(target.Length - 12));
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Lite:
                    nonce.Slice(0, 4).CopyTo(target.Slice(target.Length - 4));
                    return;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
            }
        }

        /// <summary>
        /// Gets the nonce.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="encryptionMode">The encryption mode.</param>
        public void GetNonce(ReadOnlySpan<byte> source, Span<byte> target, EncryptionMode encryptionMode)
        {
            if (target.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {Interop.SodiumNonceSize} bytes.", nameof(target));

            switch (encryptionMode)
            {
                case EncryptionMode.XSalsa20_Poly1305:
                    source.Slice(0, 12).CopyTo(target);
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Suffix:
                    source.Slice(source.Length - Interop.SodiumNonceSize).CopyTo(target);
                    return;

                case EncryptionMode.XSalsa20_Poly1305_Lite:
                    source.Slice(source.Length - 4).CopyTo(target);
                    return;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
            }
        }

        /// <summary>
        /// Encrypts the Sodium.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="nonce">The nonce.</param>
        public void Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
        {
            if (nonce.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(nonce));

            if (target.Length != Interop.SodiumMacSize + source.Length)
                throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is a sum of input buffer length and Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(target));

            int result;
            if ((result = Interop.Encrypt(source, target, this.Key.Span, nonce)) != 0)
                throw new CryptographicException($"Could not encrypt the buffer. Sodium returned code {result}.");
        }

        /// <summary>
        /// Decrypts the Sodium.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="nonce">The nonce.</param>
        public void Decrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
        {
            if (nonce.Length != Interop.SodiumNonceSize)
                throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {Interop.SodiumNonceSize} bytes.", nameof(nonce));

            if (target.Length != source.Length - Interop.SodiumMacSize)
                throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is input buffer decreased by Sodium MAC size ({Interop.SodiumMacSize} bytes).", nameof(target));

            int result;
            if ((result = Interop.Decrypt(source, target, this.Key.Span, nonce)) != 0)
                throw new CryptographicException($"Could not decrypt the buffer. Sodium returned code {result}.");
        }

        /// <summary>
        /// Disposes the Sodium.
        /// </summary>
        public void Dispose() => this.CSPRNG.Dispose();

        /// <summary>
        /// Selects the mode.
        /// </summary>
        /// <param name="availableModes">The available modes.</param>
        /// <returns>A KeyValuePair.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyValuePair<string, EncryptionMode> SelectMode(IEnumerable<string> availableModes)
        {
            foreach (var kvMode in SupportedModes)
                if (availableModes.Contains(kvMode.Key))
                    return kvMode;

            throw new CryptographicException("Could not negotiate Sodium encryption modes, as none of the modes offered by Discord are supported. This is usually an indicator that something went very wrong.");
        }

        /// <summary>
        /// Calculates the target size.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>An int.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateTargetSize(ReadOnlySpan<byte> source)
            => source.Length + Interop.SodiumMacSize;

        /// <summary>
        /// Calculates the source size.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>An int.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateSourceSize(ReadOnlySpan<byte> source)
            => source.Length - Interop.SodiumMacSize;
    }

    /// <summary>
    /// Specifies an encryption mode to use with Sodium.
    /// </summary>
    public enum EncryptionMode
    {
        /// <summary>
        /// The nonce is an incrementing uint32 value. It is encoded as big endian value at the beginning of the nonce buffer. The 4 bytes are also appended at the end of the packet.
        /// </summary>
        XSalsa20_Poly1305_Lite,

        /// <summary>
        /// The nonce consists of random bytes. It is appended at the end of a packet.
        /// </summary>
        XSalsa20_Poly1305_Suffix,

        /// <summary>
        /// The nonce consists of the RTP header. Nothing is appended to the packet.
        /// </summary>
        XSalsa20_Poly1305
    }
}
