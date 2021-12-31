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
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisCatSharp.Common.Types
{
    /// <summary>
    /// Provides a resizable memory buffer analogous to <see cref="MemoryBuffer{T}"/>, using a single continuous memory region instead.
    /// </summary>
    /// <typeparam name="T">Type of item to hold in the buffer.</typeparam>
    public sealed class ContinuousMemoryBuffer<T> : IMemoryBuffer<T> where T : unmanaged
    {
        /// <inheritdoc />
        public ulong Capacity => (ulong)this._buff.Length;

        /// <inheritdoc />
        public ulong Length => (ulong)this._pos;

        /// <inheritdoc />
        public ulong Count => (ulong)(this._pos / this._itemSize);

        private readonly MemoryPool<byte> _pool;
        private IMemoryOwner<byte> _buffOwner;
        private Memory<byte> _buff;
        private readonly bool _clear;
        private int _pos;
        private readonly int _itemSize;
        private bool _isDisposed;

        /// <summary>
        /// Creates a new buffer with a specified segment size, specified number of initially-allocated segments, and supplied memory pool.
        /// </summary>
        /// <param name="InitialSize">Initial size of the buffer in bytes. Defaults to 64KiB.</param>
        /// <param name="MemPool">Memory pool to use for renting buffers. Defaults to <see cref="System.Buffers.MemoryPool{T}.Shared"/>.</param>
        /// <param name="ClearOnDispose">Determines whether the underlying buffers should be cleared on exit. If dealing with sensitive data, it might be a good idea to set this option to true.</param>
        public ContinuousMemoryBuffer(int InitialSize = 65536, MemoryPool<byte> MemPool = default, bool ClearOnDispose = false)
        {
            this._itemSize = Unsafe.SizeOf<T>();
            this._pool = MemPool ?? MemoryPool<byte>.Shared;
            this._clear = ClearOnDispose;

            this._buffOwner = this._pool.Rent(InitialSize);
            this._buff = this._buffOwner.Memory;

            this._isDisposed = false;
        }

        /// <inheritdoc />
        public void Write(ReadOnlySpan<T> Data)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            var bytes = MemoryMarshal.AsBytes(Data);
            this.EnsureSize(this._pos + bytes.Length);

            bytes.CopyTo(this._buff[this._pos..].Span);
            this._pos += bytes.Length;
        }

        /// <inheritdoc />
        public void Write(T[] Data, int Start, int Count)
            => this.Write(Data.AsSpan(Start, Count));

        /// <inheritdoc />
        public void Write(ArraySegment<T> Data)
            => this.Write(Data.AsSpan());

        /// <inheritdoc />
        public void Write(Stream Stream)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            if (Stream.CanSeek)
                this.WriteStreamSeekable(Stream);
            else
                this.WriteStreamUnseekable(Stream);
        }

        /// <summary>
        /// Writes the stream seekable.
        /// </summary>
        /// <param name="Stream">The stream.</param>
        private void WriteStreamSeekable(Stream Stream)
        {
            if (Stream.Length > int.MaxValue)
                throw new ArgumentException("Stream is too long.", nameof(Stream));

            this.EnsureSize(this._pos + (int)Stream.Length);
#if HAS_SPAN_STREAM_OVERLOADS
            stream.Read(this._buff.Slice(this._pos).Span);
#else
            var memo = ArrayPool<byte>.Shared.Rent((int)Stream.Length);
            try
            {
                var br = Stream.Read(memo, 0, memo.Length);
                memo.AsSpan(0, br).CopyTo(this._buff[this._pos..].Span);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(memo);
            }
#endif

            this._pos += (int)Stream.Length;
        }

        /// <summary>
        /// Writes the stream unseekable.
        /// </summary>
        /// <param name="Stream">The stream.</param>
        private void WriteStreamUnseekable(Stream Stream)
        {
#if HAS_SPAN_STREAM_OVERLOADS
            var br = 0;
            do
            {
                this.EnsureSize(this._pos + 4096);
                br = stream.Read(this._buff.Slice(this._pos).Span);
                this._pos += br;
            }
            while (br != 0);
#else
            var memo = ArrayPool<byte>.Shared.Rent(4096);
            try
            {
                var br = 0;
                while ((br = Stream.Read(memo, 0, memo.Length)) != 0)
                {
                    this.EnsureSize(this._pos + br);
                    memo.AsSpan(0, br).CopyTo(this._buff[this._pos..].Span);
                    this._pos += br;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(memo);
            }
#endif
        }

        /// <inheritdoc />
        public bool Read(Span<T> Destination, ulong Source, out int ItemsWritten)
        {
            ItemsWritten = 0;
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            Source *= (ulong)this._itemSize;
            if (Source > this.Count)
                throw new ArgumentOutOfRangeException(nameof(Source), "Cannot copy data from beyond the buffer.");

            var start = (int)Source;
            var sbuff = this._buff[start..this._pos ].Span;
            var dbuff = MemoryMarshal.AsBytes(Destination);
            if (sbuff.Length > dbuff.Length)
                sbuff = sbuff[..dbuff.Length];

            ItemsWritten = sbuff.Length / this._itemSize;
            sbuff.CopyTo(dbuff);

            return (this.Length - Source) != (ulong)ItemsWritten;
        }

        /// <inheritdoc />
        public bool Read(T[] Data, int Start, int Count, ulong Source, out int ItemsWritten)
            => this.Read(Data.AsSpan(Start, Count), Source, out ItemsWritten);

        /// <inheritdoc />
        public bool Read(ArraySegment<T> Data, ulong Source, out int ItemsWritten)
            => this.Read(Data.AsSpan(), Source, out ItemsWritten);

        /// <inheritdoc />
        public T[] ToArray()
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            return MemoryMarshal.Cast<byte, T>(this._buff[..this._pos].Span).ToArray();
        }

        /// <inheritdoc />
        public void CopyTo(Stream Destination)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

#if HAS_SPAN_STREAM_OVERLOADS
            destination.Write(this._buff.Slice(0, this._pos).Span);
#else
            var buff = this._buff[..this._pos].ToArray();
            Destination.Write(buff, 0, buff.Length);
#endif
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            this._pos = 0;
        }

        /// <summary>
        /// Disposes of any resources claimed by this buffer.
        /// </summary>
        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            if (this._clear)
                this._buff.Span.Clear();

            this._buffOwner.Dispose();
            this._buff = default;
        }

        /// <summary>
        /// Ensures the size.
        /// </summary>
        /// <param name="NewCapacity">The new capacity.</param>
        private void EnsureSize(int NewCapacity)
        {
            var cap = this._buff.Length;
            if (cap >= NewCapacity)
                return;

            var factor = NewCapacity / cap;
            if (NewCapacity % cap != 0)
                ++factor;

            var newActualCapacity = cap * factor;

            var newBuffOwner = this._pool.Rent(newActualCapacity);
            var newBuff = newBuffOwner.Memory;

            this._buff.Span.CopyTo(newBuff.Span);
            if (this._clear)
                this._buff.Span.Clear();

            this._buffOwner.Dispose();
            this._buffOwner = newBuffOwner;
            this._buff = newBuff;
        }
    }
}
