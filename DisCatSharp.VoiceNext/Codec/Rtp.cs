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

namespace DisCatSharp.VoiceNext.Codec
{
    /// <summary>
    /// The rtp.
    /// </summary>
    internal sealed class Rtp : IDisposable
    {
        /// <summary>
        /// The header size.
        /// </summary>
        public const int HeaderSize = 12;

        /// <summary>
        /// The rtp no extension.
        /// </summary>
        private const byte RtpNoExtension = 0x80;
        /// <summary>
        /// The rtp extension.
        /// </summary>
        private const byte RtpExtension = 0x90;
        /// <summary>
        /// The rtp version.
        /// </summary>
        private const byte RtpVersion = 0x78;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rtp"/> class.
        /// </summary>
        public Rtp()
        { }

        /// <summary>
        /// Encodes the header.
        /// </summary>
        /// <param name="Sequence">The sequence.</param>
        /// <param name="Timestamp">The timestamp.</param>
        /// <param name="Ssrc">The ssrc.</param>
        /// <param name="Target">The target.</param>
        public void EncodeHeader(ushort Sequence, uint Timestamp, uint Ssrc, Span<byte> Target)
        {
            if (Target.Length < HeaderSize)
                throw new ArgumentException("Header buffer is too short.", nameof(Target));

            Target[0] = RtpNoExtension;
            Target[1] = RtpVersion;

            // Write data big endian
            BinaryPrimitives.WriteUInt16BigEndian(Target[2..], Sequence);  // header + magic
            BinaryPrimitives.WriteUInt32BigEndian(Target[4..], Timestamp); // header + magic + sizeof(sequence)
            BinaryPrimitives.WriteUInt32BigEndian(Target[8..], Ssrc);      // header + magic + sizeof(sequence) + sizeof(timestamp)
        }

        /// <summary>
        /// Are the rtp header.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns>A bool.</returns>
        public bool IsRtpHeader(ReadOnlySpan<byte> Source) => Source.Length >= HeaderSize && (Source[0] == RtpNoExtension || Source[0] == RtpExtension) && Source[1] == RtpVersion;

        /// <summary>
        /// Decodes the header.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="Sequence">The sequence.</param>
        /// <param name="Timestamp">The timestamp.</param>
        /// <param name="Ssrc">The ssrc.</param>
        /// <param name="HasExtension">If true, has extension.</param>
        public void DecodeHeader(ReadOnlySpan<byte> Source, out ushort Sequence, out uint Timestamp, out uint Ssrc, out bool HasExtension)
        {
            if (Source.Length < HeaderSize)
                throw new ArgumentException("Header buffer is too short.", nameof(Source));

            if ((Source[0] != RtpNoExtension && Source[0] != RtpExtension) || Source[1] != RtpVersion)
                throw new ArgumentException("Invalid RTP header.", nameof(Source));

            HasExtension = Source[0] == RtpExtension;

            // Read data big endian
            Sequence = BinaryPrimitives.ReadUInt16BigEndian(Source[2..]);
            Timestamp = BinaryPrimitives.ReadUInt32BigEndian(Source[4..]);
            Ssrc = BinaryPrimitives.ReadUInt32BigEndian(Source[8..]);
        }

        /// <summary>
        /// Calculates the packet size.
        /// </summary>
        /// <param name="EncryptedLength">The encrypted length.</param>
        /// <param name="EncryptionMode">The encryption mode.</param>
        /// <returns>An int.</returns>
        public int CalculatePacketSize(int EncryptedLength, EncryptionMode EncryptionMode)
        {
            return EncryptionMode switch
            {
                EncryptionMode.XSalsa20Poly1305 => HeaderSize + EncryptedLength,
                EncryptionMode.XSalsa20Poly1305Suffix => HeaderSize + EncryptedLength + Interop.SodiumNonceSize,
                EncryptionMode.XSalsa20Poly1305Lite => HeaderSize + EncryptedLength + 4,
                _ => throw new ArgumentException("Unsupported encryption mode.", nameof(EncryptionMode)),
            };
        }

        /// <summary>
        /// Gets the data from packet.
        /// </summary>
        /// <param name="Packet">The packet.</param>
        /// <param name="Data">The data.</param>
        /// <param name="EncryptionMode">The encryption mode.</param>
        public void GetDataFromPacket(ReadOnlySpan<byte> Packet, out ReadOnlySpan<byte> Data, EncryptionMode EncryptionMode)
        {
            switch (EncryptionMode)
            {
                case EncryptionMode.XSalsa20Poly1305:
                    Data = Packet[HeaderSize..];
                    return;

                case EncryptionMode.XSalsa20Poly1305Suffix:
                    Data = Packet.Slice(HeaderSize, Packet.Length - HeaderSize - Interop.SodiumNonceSize);
                    return;

                case EncryptionMode.XSalsa20Poly1305Lite:
                    Data = Packet.Slice(HeaderSize, Packet.Length - HeaderSize - 4);
                    break;

                default:
                    throw new ArgumentException("Unsupported encryption mode.", nameof(EncryptionMode));
            }
        }

        /// <summary>
        /// Disposes the Rtp.
        /// </summary>
        public void Dispose()
        {

        }
    }
}
