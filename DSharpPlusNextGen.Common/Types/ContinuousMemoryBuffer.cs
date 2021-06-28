// This file is part of DSharpPlusNextGen.Common project
//
// Copyright 2020 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DSharpPlusNextGen.Common.Types
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
        /// <param name="initialSize">Initial size of the buffer in bytes. Defaults to 64KiB.</param>
        /// <param name="memPool">Memory pool to use for renting buffers. Defaults to <see cref="MemoryPool{T}.Shared"/>.</param>
        /// <param name="clearOnDispose">Determines whether the underlying buffers should be cleared on exit. If dealing with sensitive data, it might be a good idea to set this option to true.</param>
        public ContinuousMemoryBuffer(int initialSize = 65536, MemoryPool<byte> memPool = default, bool clearOnDispose = false)
        {
            this._itemSize = Unsafe.SizeOf<T>();
            this._pool = memPool ?? MemoryPool<byte>.Shared;
            this._clear = clearOnDispose;

            this._buffOwner = this._pool.Rent(initialSize);
            this._buff = this._buffOwner.Memory;

            this._isDisposed = false;
        }

        /// <inheritdoc />
        public void Write(ReadOnlySpan<T> data)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            var bytes = MemoryMarshal.AsBytes(data);
            this.EnsureSize(this._pos + bytes.Length);

            bytes.CopyTo(this._buff.Slice(this._pos).Span);
            this._pos += bytes.Length;
        }

        /// <inheritdoc />
        public void Write(T[] data, int start, int count)
            => this.Write(data.AsSpan(start, count));

        /// <inheritdoc />
        public void Write(ArraySegment<T> data)
            => this.Write(data.AsSpan());

        /// <inheritdoc />
        public void Write(Stream stream)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            if (stream.CanSeek)
                this.WriteStreamSeekable(stream);
            else
                this.WriteStreamUnseekable(stream);
        }

        private void WriteStreamSeekable(Stream stream)
        {
            if (stream.Length > int.MaxValue)
                throw new ArgumentException("Stream is too long.", nameof(stream));

            this.EnsureSize(this._pos + (int)stream.Length);
#if HAS_SPAN_STREAM_OVERLOADS
            stream.Read(this._buff.Slice(this._pos).Span);
#else
            var memo = ArrayPool<byte>.Shared.Rent((int)stream.Length);
            try
            {
                var br = stream.Read(memo, 0, memo.Length);
                memo.AsSpan(0, br).CopyTo(this._buff.Slice(this._pos).Span);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(memo);
            }
#endif

            this._pos += (int)stream.Length;
        }

        private void WriteStreamUnseekable(Stream stream)
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
                while ((br = stream.Read(memo, 0, memo.Length)) != 0)
                {
                    this.EnsureSize(this._pos + br);
                    memo.AsSpan(0, br).CopyTo(this._buff.Slice(this._pos).Span);
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
        public bool Read(Span<T> destination, ulong source, out int itemsWritten)
        {
            itemsWritten = 0;
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            source *= (ulong)this._itemSize;
            if (source > this.Count)
                throw new ArgumentOutOfRangeException(nameof(source), "Cannot copy data from beyond the buffer.");

            var start = (int)source;
            var sbuff = this._buff.Slice(start, this._pos - start).Span;
            var dbuff = MemoryMarshal.AsBytes(destination);
            if (sbuff.Length > dbuff.Length)
                sbuff = sbuff.Slice(0, dbuff.Length);

            itemsWritten = sbuff.Length / this._itemSize;
            sbuff.CopyTo(dbuff);

            return (this.Length - source) != (ulong)itemsWritten;
        }

        /// <inheritdoc />
        public bool Read(T[] data, int start, int count, ulong source, out int itemsWritten)
            => this.Read(data.AsSpan(start, count), source, out itemsWritten);

        /// <inheritdoc />
        public bool Read(ArraySegment<T> data, ulong source, out int itemsWritten)
            => this.Read(data.AsSpan(), source, out itemsWritten);

        /// <inheritdoc />
        public T[] ToArray()
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            return MemoryMarshal.Cast<byte, T>(this._buff.Slice(0, this._pos).Span).ToArray();
        }

        /// <inheritdoc />
        public void CopyTo(Stream destination)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

#if HAS_SPAN_STREAM_OVERLOADS
            destination.Write(this._buff.Slice(0, this._pos).Span);
#else
            var buff = this._buff.Slice(0, this._pos).ToArray();
            destination.Write(buff, 0, buff.Length);
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

        private void EnsureSize(int newCapacity)
        {
            var cap = this._buff.Length;
            if (cap >= newCapacity)
                return;

            var factor = newCapacity / cap;
            if (newCapacity % cap != 0)
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
