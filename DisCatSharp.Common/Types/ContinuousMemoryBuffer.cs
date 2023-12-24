using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisCatSharp.Common.Types;

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
	public ContinuousMemoryBuffer(int initialSize = 65536, MemoryPool<byte>? memPool = default, bool clearOnDispose = false)
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
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		var bytes = MemoryMarshal.AsBytes(data);
		this.EnsureSize(this._pos + bytes.Length);

		bytes.CopyTo(this._buff[this._pos..].Span);
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
		if (stream.Length > int.MaxValue)
			throw new ArgumentException("Stream is too long.", nameof(stream));

		this.EnsureSize(this._pos + (int)stream.Length);
		var memo = ArrayPool<byte>.Shared.Rent((int)stream.Length);
		try
		{
			var br = stream.Read(memo, 0, memo.Length);
			memo.AsSpan(0, br).CopyTo(this._buff[this._pos..].Span);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(memo);
		}

		this._pos += (int)stream.Length;
	}

	/// <summary>
	/// Writes the stream unseekable.
	/// </summary>
	/// <param name="stream">The stream.</param>
	private void WriteStreamUnseekable(Stream stream)
	{
		var memo = ArrayPool<byte>.Shared.Rent(4096);
		try
		{
			var br = 0;
			while ((br = stream.Read(memo, 0, memo.Length)) != 0)
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
	}

	/// <inheritdoc />
	public bool Read(Span<T> destination, ulong source, out int itemsWritten)
	{
		itemsWritten = 0;
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		source *= (ulong)this._itemSize;
		if (source > this.Count)
			throw new ArgumentOutOfRangeException(nameof(source), "Cannot copy data from beyond the buffer.");

		var start = (int)source;
		var sbuff = this._buff[start..this._pos].Span;
		var dbuff = MemoryMarshal.AsBytes(destination);
		if (sbuff.Length > dbuff.Length)
			sbuff = sbuff[..dbuff.Length];

		itemsWritten = sbuff.Length / this._itemSize;
		sbuff.CopyTo(dbuff);

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

		return MemoryMarshal.Cast<byte, T>(this._buff[..this._pos].Span).ToArray();
	}

	/// <inheritdoc />
	public void CopyTo(Stream destination)
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		var buff = this._buff[..this._pos].ToArray();
		destination.Write(buff, 0, buff.Length);
	}

	/// <inheritdoc />
	public void Clear()
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

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
	/// <param name="newCapacity">The new capacity.</param>
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
