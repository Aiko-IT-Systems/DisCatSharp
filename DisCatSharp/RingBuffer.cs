using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Common;

namespace DisCatSharp;

/// <summary>
///     A circular buffer collection.
/// </summary>
/// <typeparam name="T">Type of elements within this ring buffer.</typeparam>
public class RingBuffer<T> : ICollection<T>
{
	/// <summary>
	///     Gets whether the buffer reached the end.
	/// </summary>
	private bool _reachedEnd;

	/// <summary>
	///     Creates a new ring buffer with specified size.
	/// </summary>
	/// <param name="size">Size of the buffer to create.</param>
	/// <exception cref="ArgumentOutOfRangeException" />
	public RingBuffer(int size)
	{
		if (size <= 0)
			throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");

		this.CurrentIndex = 0;
		this.Capacity = size;
		this.InternalBuffer = new T[this.Capacity];
	}

	/// <summary>
	///     Creates a new ring buffer, filled with specified elements.
	/// </summary>
	/// <param name="elements">Elements to fill the buffer with.</param>
	/// <exception cref="ArgumentException" />
	/// <exception cref="ArgumentOutOfRangeException" />
	public RingBuffer(IEnumerable<T> elements)
		: this(elements, 0)
	{ }

	/// <summary>
	///     Creates a new ring buffer, filled with specified elements, and starting at specified index.
	/// </summary>
	/// <param name="elements">Elements to fill the buffer with.</param>
	/// <param name="index">Starting element index.</param>
	/// <exception cref="ArgumentException" />
	/// <exception cref="ArgumentOutOfRangeException" />
	public RingBuffer(IEnumerable<T> elements, int index)
	{
		if (elements == null || !elements.Any())
			throw new ArgumentException("The collection cannot be null or empty.", nameof(elements));

		this.CurrentIndex = index;
		this.InternalBuffer = elements.ToArray();
		this.Capacity = this.InternalBuffer.Length;

		if (this.CurrentIndex >= this.InternalBuffer.Length || this.CurrentIndex < 0)
			throw new ArgumentOutOfRangeException(nameof(index), "Index must be less than buffer capacity, and greater than zero.");
	}

	/// <summary>
	///     Gets the current index of the buffer items.
	/// </summary>
	public int CurrentIndex { get; protected set; }

	/// <summary>
	///     Gets the capacity of this ring buffer.
	/// </summary>
	public int Capacity { get; protected set; }

	/// <summary>
	///     Gets or sets the internal collection of items.
	/// </summary>
	protected T[] InternalBuffer { get; set; }

	/// <summary>
	///     Gets the number of items in this ring buffer.
	/// </summary>
	public int Count
		=> this._reachedEnd ? this.Capacity : this.CurrentIndex;

	/// <summary>
	///     Gets whether this ring buffer is read-only.
	/// </summary>
	public bool IsReadOnly
		=> false;

	/// <summary>
	///     Inserts an item into this ring buffer.
	/// </summary>
	/// <param name="item">Item to insert.</param>
	public void Add(T item)
	{
		this.InternalBuffer[this.CurrentIndex++] = item;

		if (this.CurrentIndex == this.Capacity)
		{
			this.CurrentIndex = 0;
			this._reachedEnd = true;
		}
	}

	/// <summary>
	///     Clears this ring buffer and resets the current item index.
	/// </summary>
	public void Clear()
	{
		this.InternalBuffer.Populate(default);
		this.CurrentIndex = 0;
	}

	/// <summary>
	///     Checks whether given item is present in the buffer. This method is not implemented. Use
	///     <see cref="Contains(Func{T, bool})" /> instead.
	/// </summary>
	/// <param name="item">Item to check for.</param>
	/// <returns>Whether the buffer contains the item.</returns>
	/// <exception cref="NotImplementedException" />
	public bool Contains(T item) => throw new NotImplementedException("This method is not implemented. Use .Contains(predicate) instead.");

	/// <summary>
	///     Copies this ring buffer to target array, attempting to maintain the order of items within.
	/// </summary>
	/// <param name="array">Target array.</param>
	/// <param name="index">Index starting at which to copy the items to.</param>
	public void CopyTo(T[] array, int index)
	{
		if (array.Length - index < 1)
			throw new ArgumentException("Target array is too small to contain the elements from this buffer.", nameof(array));

		var ci = 0;
		for (var i = this.CurrentIndex; i < this.InternalBuffer.Length; i++)
			array[ci++] = this.InternalBuffer[i];
		for (var i = 0; i < this.CurrentIndex; i++)
			array[ci++] = this.InternalBuffer[i];
	}

	/// <summary>
	///     Removes an item from the buffer. This method is not implemented. Use <see cref="Remove(Func{T, bool})" /> instead.
	/// </summary>
	/// <param name="item">Item to remove.</param>
	/// <returns>Whether an item was removed or not.</returns>
	public bool Remove(T item) => throw new NotImplementedException("This method is not implemented. Use .Remove(predicate) instead.");

	/// <summary>
	///     Returns an enumerator for this ring buffer.
	/// </summary>
	/// <returns>Enumerator for this ring buffer.</returns>
	public IEnumerator<T> GetEnumerator() =>
		!this._reachedEnd
			? this.InternalBuffer.AsEnumerable().GetEnumerator()
			: this.InternalBuffer.Skip(this.CurrentIndex)
				.Concat(this.InternalBuffer.Take(this.CurrentIndex))
				.GetEnumerator();

	/// <summary>
	///     Returns an enumerator for this ring buffer.
	/// </summary>
	/// <returns>Enumerator for this ring buffer.</returns>
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	/// <summary>
	///     Gets first item from the buffer that matches the predicate.
	/// </summary>
	/// <param name="predicate">Predicate used to find the item.</param>
	/// <param name="item">
	///     Item that matches the predicate, or default value for the type of the items in this ring buffer, if
	///     one is not found.
	/// </param>
	/// <returns>Whether an item that matches the predicate was found or not.</returns>
	public bool TryGet(Func<T, bool> predicate, out T item)
	{
		for (var i = this.CurrentIndex; i < this.InternalBuffer.Length; i++)
			if (this.InternalBuffer[i] != null && predicate(this.InternalBuffer[i]))
			{
				item = this.InternalBuffer[i];
				return true;
			}

		for (var i = 0; i < this.CurrentIndex; i++)
			if (this.InternalBuffer[i] != null && predicate(this.InternalBuffer[i]))
			{
				item = this.InternalBuffer[i];
				return true;
			}

		item = default;
		return false;
	}

	/// <summary>
	///     Checks whether given item is present in the buffer using given predicate to find it.
	/// </summary>
	/// <param name="predicate">Predicate used to check for the item.</param>
	/// <returns>Whether the buffer contains the item.</returns>
	public bool Contains(Func<T, bool> predicate) => this.InternalBuffer.Any(predicate);

	/// <summary>
	///     Removes an item from the buffer using given predicate to find it.
	/// </summary>
	/// <param name="predicate">Predicate used to find the item.</param>
	/// <returns>Whether an item was removed or not.</returns>
	public bool Remove(Func<T, bool> predicate)
	{
		for (var i = 0; i < this.InternalBuffer.Length; i++)
			if (this.InternalBuffer[i] != null && predicate(this.InternalBuffer[i]))
			{
				this.InternalBuffer[i] = default;
				return true;
			}

		return false;
	}
}
