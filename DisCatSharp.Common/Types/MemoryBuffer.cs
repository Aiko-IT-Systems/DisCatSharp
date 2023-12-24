using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisCatSharp.Common.Types;

/// <summary>
/// Provides a resizable memory buffer, which can be read from and written to. It will automatically resize whenever required.
/// </summary>
/// <typeparam name="T">Type of item to hold in the buffer.</typeparam>
public sealed class MemoryBuffer<T> : IMemoryBuffer<T> where T : unmanaged
{
	/// <inheritdoc />
	public ulong Capacity =>
		this._segments.Aggregate(0UL, (a, x) => a + (ulong)x.Memory.Length); // .Sum() does only int

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
	/// <param name="segmentSize">Byte size of an individual segment. Defaults to 64KiB.</param>
	/// <param name="initialSegmentCount">Number of segments to allocate. Defaults to 0.</param>
	/// <param name="memPool">Memory pool to use for renting buffers. Defaults to <see cref="MemoryPool{T}.Shared"/>.</param>
	/// <param name="clearOnDispose">Determines whether the underlying buffers should be cleared on exit. If dealing with sensitive data, it might be a good idea to set this option to true.</param>
	public MemoryBuffer(
		int segmentSize = 65536, int initialSegmentCount = 0, MemoryPool<byte>? memPool = default,
		bool clearOnDispose = false
	)
	{
		this._itemSize = Unsafe.SizeOf<T>();
		if (segmentSize % this._itemSize != 0)
			throw new ArgumentException("Segment size must match size of individual item.");

		this._pool = memPool ?? MemoryPool<byte>.Shared;

		this._segmentSize = segmentSize;
		this._segNo = 0;
		this._lastSegmentLength = 0;
		this._clear = clearOnDispose;

		this._segments = Enumerable.Range(0, initialSegmentCount)
			.Select(x => this._pool.Rent(this._segmentSize))
			.ToList();

		this.Length = 0;

		this._isDisposed = false;
	}

	/// <inheritdoc />
	public void Write(ReadOnlySpan<T> data)
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		var src = MemoryMarshal.AsBytes(data);
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

			if (this._lastSegmentLength != mem.Length)
				continue;

			this._segNo++;
			this._lastSegmentLength = 0;
		}
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
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		if (stream.CanSeek)
			this.WriteStreamSeekable(stream);
		else
			this.WriteStreamUnseekable(stream);
	}

	/// <summary>
	/// Writes the stream seekable.
	/// </summary>
	/// <param name="stream">The stream.</param>
	private void WriteStreamSeekable(Stream stream)
	{
		var len = (int)(stream.Length - stream.Position);
		this.Grow(len);

		var buff = new byte[this._segmentSize];

		while (this._segNo < this._segments.Count && len > 0)
		{
			var seg = this._segments[this._segNo];
			var mem = seg.Memory;
			var avs = mem.Length - this._lastSegmentLength;
			avs = avs > len
				? len
				: avs;
			var dmem = mem[this._lastSegmentLength..];

			var lsl = this._lastSegmentLength;
			var slen = dmem.Span.Length - lsl;
			stream.Read(buff, 0, slen);
			buff.AsSpan(0, slen).CopyTo(dmem.Span);
			len -= dmem.Span.Length;

			this.Length += (ulong)avs;
			this._lastSegmentLength += avs;

			if (this._lastSegmentLength != mem.Length)
				continue;

			this._segNo++;
			this._lastSegmentLength = 0;
		}
	}

	/// <summary>
	/// Writes the stream unseekable.
	/// </summary>
	/// <param name="stream">The stream.</param>
	private void WriteStreamUnseekable(Stream stream)
	{
		var read = 0;
		var buff = new byte[this._segmentSize];
		var buffs = buff.AsSpan();
		while ((read = stream.Read(buff, 0, buff.Length - this._lastSegmentLength)) != 0)
			this.Write(MemoryMarshal.Cast<byte, T>(buffs[..read]));
	}

	/// <inheritdoc />
	public bool Read(Span<T> destination, ulong source, out int itemsWritten)
	{
		itemsWritten = 0;
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		source *= (ulong)this._itemSize;
		if (source > this.Count)
			throw new ArgumentOutOfRangeException(nameof(source), "Cannot copy data from beyond the buffer.");

		// Find where to begin
		var i = 0;
		for (; i < this._segments.Count; i++)
		{
			var seg = this._segments[i];
			var mem = seg.Memory;
			if ((ulong)mem.Length > source)
				break;

			source -= (ulong)mem.Length;
		}

		// Do actual copy
		var dl = (int)(this.Length - source);
		var sri = (int)source;
		var dst = MemoryMarshal.AsBytes(destination);
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

			if (itemsWritten + src.Length > dl)
				src = src[..(dl - itemsWritten)];

			if (src.Length > dst.Length)
				src = src[..dst.Length];

			src.CopyTo(dst);
			dst = dst[src.Length..];
			itemsWritten += src.Length;
		}

		itemsWritten /= this._itemSize;
		return this.Length - source != (ulong)itemsWritten;
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
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		var bytes = new T[this.Count];
		this.Read(bytes, 0, out _);
		return bytes;
	}

	/// <inheritdoc />
	public void CopyTo(Stream destination)
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		var longest = this._segments.Max(x => x.Memory.Length);
		var buff = new byte[longest];

		foreach (var seg in this._segments)
		{
			var mem = seg.Memory.Span;
			var spn = buff.AsSpan(0, mem.Length);

			mem.CopyTo(spn);
			destination.Write(buff, 0, spn.Length);
		}
	}

	/// <inheritdoc />
	public void Clear()
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

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
	/// <param name="minAmount">The min amount.</param>
	private void Grow(int minAmount)
	{
		var capacity = this.Capacity;
		var length = this.Length;
		var totalAmt = length + (ulong)minAmount;
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
