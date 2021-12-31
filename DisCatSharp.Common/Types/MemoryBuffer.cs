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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisCatSharp.Common.Types
{
    /// <summary>
    /// Provides a resizable memory buffer, which can be read from and written to. It will automatically resize whenever required.
    /// </summary>
    /// <typeparam name="T">Type of item to hold in the buffer.</typeparam>
    public sealed class MemoryBuffer<T> : IMemoryBuffer<T> where T : unmanaged
    {
        /// <inheritdoc />
        public ulong Capacity => this._segments.Aggregate(0UL, (A, X) => A + (ulong)X.Memory.Length); // .Sum() does only int

        /// <inheritdoc />
        public ulong Length { get; private set; }

        /// <inheritdoc />
        public ulong Count => this.Length / (ulong)this._itemSize;

        private readonly MemoryPool<byte> _pool;
        private readonly int _segmentSize;
        private int _lastSegmentLength;
        private int _segNo;
        private readonly bool _clear;
        private readonly List<IMemoryOwner<byte>> _segments;
        private readonly int _itemSize;
        private bool _isDisposed;

        /// <summary>
        /// Creates a new buffer with a specified segment size, specified number of initially-allocated segments, and supplied memory pool.
        /// </summary>
        /// <param name="SegmentSize">Byte size of an individual segment. Defaults to 64KiB.</param>
        /// <param name="InitialSegmentCount">Number of segments to allocate. Defaults to 0.</param>
        /// <param name="MemPool">Memory pool to use for renting buffers. Defaults to <see cref="System.Buffers.MemoryPool{T}.Shared"/>.</param>
        /// <param name="ClearOnDispose">Determines whether the underlying buffers should be cleared on exit. If dealing with sensitive data, it might be a good idea to set this option to true.</param>
        public MemoryBuffer(int SegmentSize = 65536, int InitialSegmentCount = 0, MemoryPool<byte> MemPool = default, bool ClearOnDispose = false)
        {
            this._itemSize = Unsafe.SizeOf<T>();
            if (SegmentSize % this._itemSize != 0)
                throw new ArgumentException("Segment size must match size of individual item.");

            this._pool = MemPool ?? MemoryPool<byte>.Shared;

            this._segmentSize = SegmentSize;
            this._segNo = 0;
            this._lastSegmentLength = 0;
            this._clear = ClearOnDispose;

            this._segments = new List<IMemoryOwner<byte>>(InitialSegmentCount + 1);
            for (var i = 0; i < InitialSegmentCount; i++)
                this._segments.Add(this._pool.Rent(this._segmentSize));

            this.Length = 0;

            this._isDisposed = false;
        }

        /// <inheritdoc />
        public void Write(ReadOnlySpan<T> Data)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            var src = MemoryMarshal.AsBytes(Data);
            this.Grow(src.Length);

            while (this._segNo < this._segments.Count && src.Length > 0)
            {
                var seg = this._segments[this._segNo];
                var mem = seg.Memory;
                var avs = mem.Length - this._lastSegmentLength;
                avs = avs > src.Length
                    ? src.Length
                    : avs;
                var dmem = mem[this._lastSegmentLength..];

                src[..avs].CopyTo(dmem.Span);
                src = src[avs..];

                this.Length += (ulong)avs;
                this._lastSegmentLength += avs;

                if (this._lastSegmentLength == mem.Length)
                {
                    this._segNo++;
                    this._lastSegmentLength = 0;
                }
            }
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
            var len = (int)(Stream.Length - Stream.Position);
            this.Grow(len);

#if !HAS_SPAN_STREAM_OVERLOADS
            var buff = new byte[this._segmentSize];
#endif

            while (this._segNo < this._segments.Count && len > 0)
            {
                var seg = this._segments[this._segNo];
                var mem = seg.Memory;
                var avs = mem.Length - this._lastSegmentLength;
                avs = avs > len
                    ? len
                    : avs;
                var dmem = mem[this._lastSegmentLength..];

#if HAS_SPAN_STREAM_OVERLOADS
                stream.Read(dmem.Span);
#else
                var lsl = this._lastSegmentLength;
                var slen = dmem.Span.Length - lsl;
                Stream.Read(buff, 0, slen);
                buff.AsSpan(0, slen).CopyTo(dmem.Span);
#endif
                len -= dmem.Span.Length;

                this.Length += (ulong)avs;
                this._lastSegmentLength += avs;

                if (this._lastSegmentLength == mem.Length)
                {
                    this._segNo++;
                    this._lastSegmentLength = 0;
                }
            }
        }

        /// <summary>
        /// Writes the stream unseekable.
        /// </summary>
        /// <param name="Stream">The stream.</param>
        private void WriteStreamUnseekable(Stream Stream)
        {
            var read = 0;
#if HAS_SPAN_STREAM_OVERLOADS
            Span<byte> buffs = stackalloc byte[this._segmentSize];
            while ((read = stream.Read(buffs)) != 0)
#else
            var buff = new byte[this._segmentSize];
            var buffs = buff.AsSpan();
            while ((read = Stream.Read(buff, 0, buff.Length - this._lastSegmentLength)) != 0)
#endif
                this.Write(MemoryMarshal.Cast<byte, T>(buffs[..read]));
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

            // Find where to begin
            var i = 0;
            for (; i < this._segments.Count; i++)
            {
                var seg = this._segments[i];
                var mem = seg.Memory;
                if ((ulong)mem.Length > Source)
                    break;

                Source -= (ulong)mem.Length;
            }

            // Do actual copy
            var dl = (int)(this.Length - Source);
            var sri = (int)Source;
            var dst = MemoryMarshal.AsBytes(Destination);
            for (; i < this._segments.Count && dst.Length > 0; i++)
            {
                var seg = this._segments[i];
                var mem = seg.Memory;
                var src = mem.Span;

                if (sri != 0)
                {
                    src = src[sri..];
                    sri = 0;
                }

                if (ItemsWritten + src.Length > dl)
                    src = src[..(dl - ItemsWritten)];

                if (src.Length > dst.Length)
                    src = src[..dst.Length];

                src.CopyTo(dst);
                dst = dst[src.Length..];
                ItemsWritten += src.Length;
            }

            ItemsWritten /= this._itemSize;
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

            var bytes = new T[this.Count];
            this.Read(bytes, 0, out _);
            return bytes;
        }

        /// <inheritdoc />
        public void CopyTo(Stream Destination)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

#if HAS_SPAN_STREAM_OVERLOADS
            foreach (var seg in this._segments)
                destination.Write(seg.Memory.Span);
#else
            var longest = this._segments.Max(X => X.Memory.Length);
            var buff = new byte[longest];

            foreach (var seg in this._segments)
            {
                var mem = seg.Memory.Span;
                var spn = buff.AsSpan(0, mem.Length);

                mem.CopyTo(spn);
                Destination.Write(buff, 0, spn.Length);
            }
#endif
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("This buffer is disposed.");

            this._segNo = 0;
            this._lastSegmentLength = 0;
            this.Length = 0;
        }

        /// <summary>
        /// Disposes of any resources claimed by this buffer.
        /// </summary>
        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            foreach (var segment in this._segments)
            {
                if (this._clear)
                    segment.Memory.Span.Clear();

                segment.Dispose();
            }
        }

        /// <summary>
        /// Grows the.
        /// </summary>
        /// <param name="MinAmount">The min amount.</param>
        private void Grow(int MinAmount)
        {
            var capacity = this.Capacity;
            var length = this.Length;
            var totalAmt = (length + (ulong)MinAmount);
            if (capacity >= totalAmt)
                return; // we're good

            var amt = (int)(totalAmt - capacity);
            var segCount = amt / this._segmentSize;
            if (amt % this._segmentSize != 0)
                segCount++;

            // Basically List<T>.EnsureCapacity
            // Default grow behaviour is minimum current*2
            var segCap = this._segments.Count + segCount;
            if (segCap > this._segments.Capacity)
                this._segments.Capacity = segCap < this._segments.Capacity * 2
                    ? this._segments.Capacity * 2
                    : segCap;

            for (var i = 0; i < segCount; i++)
                this._segments.Add(this._pool.Rent(this._segmentSize));
        }
    }
}
