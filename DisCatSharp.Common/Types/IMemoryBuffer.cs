using System;
using System.IO;

namespace DisCatSharp.Common.Types;

/// <summary>
/// An interface describing the API of resizable memory buffers, such as <see cref="MemoryBuffer{T}"/> and <see cref="ContinuousMemoryBuffer{T}"/>.
/// </summary>
/// <typeparam name="T">Type of item to hold in the buffer.</typeparam>
public interface IMemoryBuffer<T> : IDisposable where T : unmanaged
{
	/// <summary>
	/// Gets the total capacity of this buffer. The capacity is the number of segments allocated, multiplied by size of individual segment.
	/// </summary>
	ulong Capacity { get; }

	/// <summary>
	/// Gets the amount of bytes currently written to the buffer. This number is never greater than <see cref="Capacity"/>.
	/// </summary>
	ulong Length { get; }

	/// <summary>
	/// Gets the number of items currently written to the buffer. This number is equal to <see cref="Count"/> divided by size of <typeparamref name="T"/>.
	/// </summary>
	ulong Count { get; }

	/// <summary>
	/// Appends data from a supplied buffer to this buffer, growing it if necessary.
	/// </summary>
	/// <param name="data">Buffer containing data to write.</param>
	void Write(ReadOnlySpan<T> data);

	/// <summary>
	/// Appends data from a supplied array to this buffer, growing it if necessary.
	/// </summary>
	/// <param name="data">Array containing data to write.</param>
	/// <param name="start">Index from which to start reading the data.</param>
	/// <param name="count">Number of bytes to read from the source.</param>
	void Write(T[] data, int start, int count);

	/// <summary>
	/// Appends data from a supplied array slice to this buffer, growing it if necessary.
	/// </summary>
	/// <param name="data">Array slice containing data to write.</param>
	void Write(ArraySegment<T> data);

	/// <summary>
	/// Appends data from a supplied stream to this buffer, growing it if necessary.
	/// </summary>
	/// <param name="stream">Stream to copy data from.</param>
	void Write(Stream stream);

	/// <summary>
	/// Reads data from this buffer to the specified destination buffer. This method will write either as many
	/// bytes as there are in the destination buffer, or however many bytes are available in this buffer,
	/// whichever is less.
	/// </summary>
	/// <param name="destination">Buffer to read the data from this buffer into.</param>
	/// <param name="source">Starting position in this buffer to read from.</param>
	/// <param name="itemsWritten">Number of items written to the destination buffer.</param>
	/// <returns>Whether more data is available in this buffer.</returns>
	bool Read(Span<T> destination, ulong source, out int itemsWritten);

	/// <summary>
	/// Reads data from this buffer to specified destination array. This method will write either as many bytes
	/// as specified for the destination array, or however many bytes are available in this buffer, whichever is
	/// less.
	/// </summary>
	/// <param name="data">Array to read the data from this buffer into.</param>
	/// <param name="start">Starting position in the target array to write to.</param>
	/// <param name="count">Maximum number of bytes to write to target array.</param>
	/// <param name="source">Starting position in this buffer to read from.</param>
	/// <param name="itemsWritten">Number of items written to the destination buffer.</param>
	/// <returns>Whether more data is available in this buffer.</returns>
	bool Read(T[] data, int start, int count, ulong source, out int itemsWritten);

	/// <summary>
	/// Reads data from this buffer to specified destination array slice. This method will write either as many
	/// bytes as specified in the target slice, or however many bytes are available in this buffer, whichever is
	/// less.
	/// </summary>
	/// <param name="data"></param>
	/// <param name="source"></param>
	/// <param name="itemsWritten">Number of items written to the destination buffer.</param>
	/// <returns>Whether more data is available in this buffer.</returns>
	bool Read(ArraySegment<T> data, ulong source, out int itemsWritten);

	/// <summary>
	/// Converts this buffer into a single continuous byte array.
	/// </summary>
	/// <returns>Converted byte array.</returns>
	T[] ToArray();

	/// <summary>
	/// Copies all the data from this buffer to a stream.
	/// </summary>
	/// <param name="destination">Stream to copy this buffer's data to.</param>
	void CopyTo(Stream destination);

	/// <summary>
	/// Resets the buffer's pointer to the beginning, allowing for reuse.
	/// </summary>
	void Clear();
}
