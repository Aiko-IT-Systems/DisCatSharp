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
using System.IO;
using System.IO.Compression;

namespace DisCatSharp.Net.WebSocket
{
    /// <summary>
    /// Represents a payload decompressor.
    /// </summary>
    internal sealed class PayloadDecompressor : IDisposable
    {
        /// <summary>
        /// The zlib flush.
        /// </summary>
        private const uint ZlibFlush = 0x0000FFFF;

        /// <summary>
        /// The zlib prefix.
        /// </summary>
        private const byte ZlibPrefix = 0x78;

        /// <summary>
        /// Gets the compression level.
        /// </summary>
        public GatewayCompressionLevel CompressionLevel { get; }

        /// <summary>
        /// Gets the compressed stream.
        /// </summary>
        private MemoryStream CompressedStream { get; }

        /// <summary>
        /// Gets the decompressor stream.
        /// </summary>
        private DeflateStream DecompressorStream { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadDecompressor"/> class.
        /// </summary>
        /// <param name="CompressionLevel">The compression level.</param>
        public PayloadDecompressor(GatewayCompressionLevel CompressionLevel)
        {
            if (CompressionLevel == GatewayCompressionLevel.None)
                throw new InvalidOperationException("Decompressor requires a valid compression mode.");

            this.CompressionLevel = CompressionLevel;
            this.CompressedStream = new MemoryStream();
            if (this.CompressionLevel == GatewayCompressionLevel.Stream)
                this.DecompressorStream = new DeflateStream(this.CompressedStream, CompressionMode.Decompress);
        }

        /// <summary>
        /// Tries the decompress.
        /// </summary>
        /// <param name="Compressed">The compressed bytes.</param>
        /// <param name="Decompressed">The decompressed memory stream.</param>
        public bool TryDecompress(ArraySegment<byte> Compressed, MemoryStream Decompressed)
        {
            var zlib = this.CompressionLevel == GatewayCompressionLevel.Stream
                ? this.DecompressorStream
                : new DeflateStream(this.CompressedStream, CompressionMode.Decompress, true);

            if (Compressed.Array[0] == ZlibPrefix)
                this.CompressedStream.Write(Compressed.Array, Compressed.Offset + 2, Compressed.Count - 2);
            else
                this.CompressedStream.Write(Compressed.Array, Compressed.Offset, Compressed.Count);

            this.CompressedStream.Flush();
            this.CompressedStream.Position = 0;

            var cspan = Compressed.AsSpan();
            var suffix = BinaryPrimitives.ReadUInt32BigEndian(cspan[^4..]);
            if (this.CompressionLevel == GatewayCompressionLevel.Stream && suffix != ZlibFlush)
            {
                if (this.CompressionLevel == GatewayCompressionLevel.Payload)
                    zlib.Dispose();

                return false;
            }

            try
            {
                zlib.CopyTo(Decompressed);
                return true;
            }
            catch { return false; }
            finally
            {
                this.CompressedStream.Position = 0;
                this.CompressedStream.SetLength(0);

                if (this.CompressionLevel == GatewayCompressionLevel.Payload)
                    zlib.Dispose();
            }
        }

        /// <summary>
        /// Disposes the decompressor.
        /// </summary>
        public void Dispose()
        {
            this.DecompressorStream?.Dispose();
            this.CompressedStream.Dispose();
        }
    }
}
