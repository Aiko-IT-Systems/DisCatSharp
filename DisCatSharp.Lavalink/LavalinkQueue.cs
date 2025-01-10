using System;
using System.Collections;
using System.Collections.Generic;

namespace DisCatSharp.Lavalink;

/// <summary>
///     Represents a queue of objects.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LavalinkQueue<T> : IEnumerable<T>
{
	/// <summary>
	///     Gets the internal entries.
	/// </summary>
	private readonly LinkedList<T> _list;

	/// <summary>
	///     Creates a new instance of <see cref="LavalinkQueue{T}" />.
	/// </summary>
	public LavalinkQueue()
	{
		this._list = [];
	}

	/// <summary>
	///     Gets the number of elements contained in the <see cref="LavalinkQueue{T}" />.
	/// </summary>
	public int Count
	{
		get
		{
			lock (this._list)
			{
				return this._list.Count;
			}
		}
	}

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
	{
		lock (this._list)
		{
			for (var node = this._list.First; node != null; node = node.Next)
				yield return node.Value;
		}
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
		=> this.GetEnumerator();

	/// <summary>
	///     Adds an object to the end of the <see cref="LavalinkQueue{T}" />.
	/// </summary>
	/// <param name="value">The object to add to the <see cref="LavalinkQueue{T}" />. The value cannot be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
	public void Enqueue(T value)
	{
		ArgumentNullException.ThrowIfNull(value);

		lock (this._list)
		{
			this._list.AddLast(value);
		}
	}

	/// <summary>
	///     Attempts to remove and return the object at the beginning of the <see cref="LavalinkQueue{T}" />.
	/// </summary>
	/// <param name="value">
	///     When this method returns, contains the object removed from the beginning of the
	///     <see cref="LavalinkQueue{T}" />, or the default value of <typeparamref name="T" /> if the queue is empty.
	/// </param>
	/// <returns><see langword="true" /> if an object was successfully removed; otherwise, <see langword="false" />.</returns>
	public bool TryDequeue(out T value)
	{
		lock (this._list)
		{
			if (this._list.Count < 1)
			{
				value = default!;
				return false;
			}

			if (this._list.First is null)
			{
				value = default!;
				return true;
			}

			var result = this._list.First.Value;
			if (result is null)
			{
				value = default!;
				return false;
			}

			this._list.RemoveFirst();
			value = result;

			return true;
		}
	}

	/// <summary>
	///     Attempts to return the object at the beginning of the <see cref="LavalinkQueue{T}" /> without removing it.
	/// </summary>
	/// <param name="value">
	///     When this method returns, contains the object at the beginning of the
	///     <see cref="LavalinkQueue{T}" />, or the default value of <typeparamref name="T" /> if the queue is empty.
	/// </param>
	/// <returns>
	///     <see langword="true" /> if there is an object at the beginning of the <see cref="LavalinkQueue{T}" />;
	///     otherwise, <see langword="false" />.
	/// </returns>
	public bool TryPeek(out T value)
	{
		lock (this._list)
		{
			if (this._list.First is null)
			{
				value = default!;
				return false;
			}

			value = this._list.First.Value;
			return true;
		}
	}

	/// <summary>
	///     Removes the first occurrence of a specific object from the <see cref="LavalinkQueue{T}" />.
	/// </summary>
	/// <param name="value">The object to remove from the <see cref="LavalinkQueue{T}" />. The value cannot be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
	public void Remove(T value)
	{
		ArgumentNullException.ThrowIfNull(value);

		lock (this._list)
		{
			this._list.Remove(value);
		}
	}

	/// <summary>
	///     Removes all elements from the <see cref="LavalinkQueue{T}" />.
	/// </summary>
	public void Clear()
	{
		lock (this._list)
		{
			this._list.Clear();
		}
	}

	/// <summary>
	///     Shuffles the elements in the <see cref="LavalinkQueue{T}" /> randomly.
	/// </summary>
	public void Shuffle()
	{
		lock (this._list)
		{
			if (this._list.Count < 2)
				return;

			var shadow = new T[this._list.Count];
			var i = 0;
			for (var node = this._list.First; node is not null; node = node.Next)
			{
				var j = Random.Shared.Next(i + 1);
				if (i != j)
					shadow[i] = shadow[j];

				shadow[j] = node.Value;
				i++;
			}

			this._list.Clear();
			foreach (var value in shadow)
				this._list.AddLast(value);
		}
	}

	/// <summary>
	///     Removes the element at the specified index of the <see cref="LavalinkQueue{T}" />.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <returns>The value of the element that was removed.</returns>
	/// <exception cref="Exception">Thrown when the node at the specified index is null.</exception>
	public T RemoveAt(int index)
	{
		lock (this._list)
		{
			var currentNode = this._list.First;

			for (var i = 0; i <= index && currentNode is not null; i++)
			{
				if (i != index)
				{
					currentNode = currentNode.Next;
					continue;
				}

				this._list.Remove(currentNode);
				break;
			}

			return currentNode is null
				? throw new("Node was null.")
				: currentNode.Value;
		}
	}

	/// <summary>
	///     Reverses the order of the elements in the entire <see cref="LavalinkQueue{T}" />.
	/// </summary>
	public void Reverse()
	{
		lock (this._list)
		{
			if (this._list.Count < 2)
				return;

			LinkedList<T> reversedList = new();
			for (var node = this._list.Last; node is not null; node = node.Previous)
				reversedList.AddLast(node.Value);

			this._list.Clear();
			foreach (var value in reversedList)
				this._list.AddLast(value);
		}
	}

	/// <summary>
	/// </summary>
	/// <param name="index"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public ICollection<T> RemoveRange(int index, int count)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(count, this.Count - index);

		var tempIndex = 0;
		var removed = new T[count];
		lock (this._list)
		{
			var currentNode = this._list.First;
			while (tempIndex != index && currentNode is not null)
			{
				tempIndex++;
				currentNode = currentNode.Next;
			}

			var nextNode = currentNode?.Next;
			for (var i = 0; i < count && currentNode is not null; i++)
			{
				var tempValue = currentNode.Value;
				removed[i] = tempValue;

				this._list.Remove(currentNode);
				currentNode = nextNode;
				nextNode = nextNode?.Next;
			}

			return removed;
		}
	}
}
