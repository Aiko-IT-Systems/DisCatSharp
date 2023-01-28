// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DisCatSharp.Common;

/// <summary>
/// Represents collection of string keys and <typeparamref name="TValue"/> values, allowing the use of <see cref="ReadOnlySpan{T}"/> for dictionary operations.
/// </summary>
/// <typeparam name="TValue">Type of items in this dictionary.</typeparam>
public sealed class CharSpanLookupDictionary<TValue> :
	IDictionary<string, TValue>,
	IReadOnlyDictionary<string, TValue>,
	IDictionary
{
	/// <summary>
	/// Gets the collection of all keys present in this dictionary.
	/// </summary>
	public IEnumerable<string> Keys => this.GetKeysInternal();
	/// <summary>
	/// Gets the keys.
	/// </summary>
	ICollection<string> IDictionary<string, TValue>.Keys => this.GetKeysInternal();
	/// <summary>
	/// Gets the keys.
	/// </summary>
	ICollection IDictionary.Keys => this.GetKeysInternal();

	/// <summary>
	/// Gets the collection of all values present in this dictionary.
	/// </summary>
	public IEnumerable<TValue> Values => this.GetValuesInternal();
	/// <summary>
	/// Gets the values.
	/// </summary>
	ICollection<TValue> IDictionary<string, TValue>.Values => this.GetValuesInternal();
	/// <summary>
	/// Gets the values.
	/// </summary>
	ICollection IDictionary.Values => this.GetValuesInternal();

	/// <summary>
	/// Gets the total number of items in this dictionary.
	/// </summary>
	public int Count { get; private set; }

	/// <summary>
	/// Gets whether this dictionary is read-only.
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Gets whether this dictionary has a fixed size.
	/// </summary>
	public bool IsFixedSize => false;

	/// <summary>
	/// Gets whether this dictionary is considered thread-safe.
	/// </summary>
	public bool IsSynchronized => false;

	/// <summary>
	/// Gets the object which allows synchronizing access to this dictionary.
	/// </summary>
	public object SyncRoot { get; } = new();

	/// <summary>
	/// Gets or sets a value corresponding to given key in this dictionary.
	/// </summary>
	/// <param name="key">Key to get or set the value for.</param>
	/// <returns>Value matching the supplied key, if applicable.</returns>
	public TValue this[string key]
	{
		get
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			if (!this.TryRetrieveInternal(key.AsSpan(), out var value))
				throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

			return value;
		}

		set
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			this.TryInsertInternal(key, value, true);
		}
	}

	/// <summary>
	/// Gets or sets a value corresponding to given key in this dictionary.
	/// </summary>
	/// <param name="key">Key to get or set the value for.</param>
	/// <returns>Value matching the supplied key, if applicable.</returns>
	public TValue this[ReadOnlySpan<char> key]
	{
		get
		{
			if (!this.TryRetrieveInternal(key, out var value))
				throw new KeyNotFoundException($"The given key was not present in the dictionary.");

			return value;
		}

		set
		{
			unsafe
			{
				fixed (char* chars = &key.GetPinnableReference())
					this.TryInsertInternal(new string(chars, 0, key.Length), value, true);
			}
		}
	}

	object IDictionary.this[object key]
	{
		get
		{
			if (!(key is string tkey))
				throw new ArgumentException("Key needs to be an instance of a string.");

			if (!this.TryRetrieveInternal(tkey.AsSpan(), out var value))
				throw new KeyNotFoundException($"The given key '{tkey}' was not present in the dictionary.");

			return value;
		}

		set
		{
			if (!(key is string tkey))
				throw new ArgumentException("Key needs to be an instance of a string.");

			if (!(value is TValue tvalue))
			{
				tvalue = default;
				if (tvalue != null)
					throw new ArgumentException($"Value needs to be an instance of {typeof(TValue)}.");
			}

			this.TryInsertInternal(tkey, tvalue, true);
		}
	}

	/// <summary>
	/// Gets the internal buckets.
	/// </summary>
	private readonly Dictionary<ulong, KeyedValue> _internalBuckets;

	/// <summary>
	/// Creates a new, empty <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/>.
	/// </summary>
	public CharSpanLookupDictionary()
	{
		this._internalBuckets = new Dictionary<ulong, KeyedValue>();
	}

	/// <summary>
	/// Creates a new, empty <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and sets its initial capacity to specified value.
	/// </summary>
	/// <param name="initialCapacity">Initial capacity of the dictionary.</param>
	public CharSpanLookupDictionary(int initialCapacity)
	{
		this._internalBuckets = new Dictionary<ulong, KeyedValue>(initialCapacity);
	}

	/// <summary>
	/// Creates a new <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied dictionary.
	/// </summary>
	/// <param name="values">Dictionary containing items to populate this dictionary with.</param>
	public CharSpanLookupDictionary(IDictionary<string, TValue> values)
		: this(values.Count)
	{
		foreach (var (k, v) in values)
			this.Add(k, v);
	}

	/// <summary>
	/// Creates a new <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied dictionary.
	/// </summary>
	/// <param name="values">Dictionary containing items to populate this dictionary with.</param>
	public CharSpanLookupDictionary(IReadOnlyDictionary<string, TValue> values)
		 : this(values.Count)
	{
		foreach (var (k, v) in values)
			this.Add(k, v);
	}

	/// <summary>
	/// Creates a new <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied key-value collection.
	/// </summary>
	/// <param name="values">Dictionary containing items to populate this dictionary with.</param>
	public CharSpanLookupDictionary(IEnumerable<KeyValuePair<string, TValue>> values)
		: this()
	{
		foreach (var (k, v) in values)
			this.Add(k, v);
	}

	/// <summary>
	/// Inserts a specific key and corresponding value into this dictionary.
	/// </summary>
	/// <param name="key">Key to insert.</param>
	/// <param name="value">Value corresponding to this key.</param>
	public void Add(string key, TValue value)
	{
		if (!this.TryInsertInternal(key, value, false))
			throw new ArgumentException("Given key is already present in the dictionary.", nameof(key));
	}

	/// <summary>
	/// Inserts a specific key and corresponding value into this dictionary.
	/// </summary>
	/// <param name="key">Key to insert.</param>
	/// <param name="value">Value corresponding to this key.</param>
	public void Add(ReadOnlySpan<char> key, TValue value)
	{
		unsafe
		{
			fixed (char* chars = &key.GetPinnableReference())
				if (!this.TryInsertInternal(new string(chars, 0, key.Length), value, false))
					throw new ArgumentException("Given key is already present in the dictionary.", nameof(key));
		}
	}

	/// <summary>
	/// Attempts to insert a specific key and corresponding value into this dictionary.
	/// </summary>
	/// <param name="key">Key to insert.</param>
	/// <param name="value">Value corresponding to this key.</param>
	/// <returns>Whether the operation was successful.</returns>
	public bool TryAdd(string key, TValue value)
		=> this.TryInsertInternal(key, value, false);

	/// <summary>
	/// Attempts to insert a specific key and corresponding value into this dictionary.
	/// </summary>
	/// <param name="key">Key to insert.</param>
	/// <param name="value">Value corresponding to this key.</param>
	/// <returns>Whether the operation was successful.</returns>
	public bool TryAdd(ReadOnlySpan<char> key, TValue value)
	{
		unsafe
		{
			fixed (char* chars = &key.GetPinnableReference())
				return this.TryInsertInternal(new string(chars, 0, key.Length), value, false);
		}
	}

	/// <summary>
	/// Attempts to retrieve a value corresponding to the supplied key from this dictionary.
	/// </summary>
	/// <param name="key">Key to retrieve the value for.</param>
	/// <param name="value">Retrieved value.</param>
	/// <returns>Whether the operation was successful.</returns>
	public bool TryGetValue(string key, out TValue value)
	{
		if (key == null)
			throw new ArgumentNullException(nameof(key));

		return this.TryRetrieveInternal(key.AsSpan(), out value);
	}

	/// <summary>
	/// Attempts to retrieve a value corresponding to the supplied key from this dictionary.
	/// </summary>
	/// <param name="key">Key to retrieve the value for.</param>
	/// <param name="value">Retrieved value.</param>
	/// <returns>Whether the operation was successful.</returns>
	public bool TryGetValue(ReadOnlySpan<char> key, out TValue value)
		=> this.TryRetrieveInternal(key, out value);

	/// <summary>
	/// Attempts to remove a value corresponding to the supplied key from this dictionary.
	/// </summary>
	/// <param name="key">Key to remove the value for.</param>
	/// <param name="value">Removed value.</param>
	/// <returns>Whether the operation was successful.</returns>
	public bool TryRemove(string key, out TValue value)
	{
		if (key == null)
			throw new ArgumentNullException(nameof(key));

		return this.TryRemoveInternal(key.AsSpan(), out value);
	}

	/// <summary>
	/// Attempts to remove a value corresponding to the supplied key from this dictionary.
	/// </summary>
	/// <param name="key">Key to remove the value for.</param>
	/// <param name="value">Removed value.</param>
	/// <returns>Whether the operation was successful.</returns>
	public bool TryRemove(ReadOnlySpan<char> key, out TValue value)
		=> this.TryRemoveInternal(key, out value);

	/// <summary>
	/// Checks whether this dictionary contains the specified key.
	/// </summary>
	/// <param name="key">Key to check for in this dictionary.</param>
	/// <returns>Whether the key was present in the dictionary.</returns>
	public bool ContainsKey(string key)
		=> this.ContainsKeyInternal(key.AsSpan());

	/// <summary>
	/// Checks whether this dictionary contains the specified key.
	/// </summary>
	/// <param name="key">Key to check for in this dictionary.</param>
	/// <returns>Whether the key was present in the dictionary.</returns>
	public bool ContainsKey(ReadOnlySpan<char> key)
		=> this.ContainsKeyInternal(key);

	/// <summary>
	/// Removes all items from this dictionary.
	/// </summary>
	public void Clear()
	{
		this._internalBuckets.Clear();
		this.Count = 0;
	}

	/// <summary>
	/// Gets an enumerator over key-value pairs in this dictionary.
	/// </summary>
	/// <returns></returns>
	public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
		=> new Enumerator(this);

	/// <summary>
	/// Removes the.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <returns>A bool.</returns>
	bool IDictionary<string, TValue>.Remove(string key)
		=> this.TryRemove(key.AsSpan(), out _);

	/// <summary>
	/// Adds the.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	void IDictionary.Add(object key, object value)
	{
		if (!(key is string tkey))
			throw new ArgumentException("Key needs to be an instance of a string.");

		if (!(value is TValue tvalue))
		{
			tvalue = default;
			if (tvalue != null)
				throw new ArgumentException($"Value needs to be an instance of {typeof(TValue)}.");
		}

		this.Add(tkey, tvalue);
	}

	/// <summary>
	/// Removes the.
	/// </summary>
	/// <param name="key">The key.</param>
	void IDictionary.Remove(object key)
	{
		if (!(key is string tkey))
			throw new ArgumentException("Key needs to be an instance of a string.");

		this.TryRemove(tkey, out _);
	}

	/// <summary>
	/// Contains the.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <returns>A bool.</returns>
	bool IDictionary.Contains(object key)
	{
		if (!(key is string tkey))
			throw new ArgumentException("Key needs to be an instance of a string.");

		return this.ContainsKey(tkey);
	}

	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>An IDictionaryEnumerator.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
		=> new Enumerator(this);

	/// <summary>
	/// Adds the.
	/// </summary>
	/// <param name="item">The item.</param>
	void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
		=> this.Add(item.Key, item.Value);

	/// <summary>
	/// Removes the.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <returns>A bool.</returns>
	bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
		=> this.TryRemove(item.Key, out _);

	/// <summary>
	/// Contains the.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <returns>A bool.</returns>
	bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
		=> this.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

	/// <summary>
	/// Copies the to.
	/// </summary>
	/// <param name="array">The array.</param>
	/// <param name="arrayIndex">The array index.</param>
	void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
	{
		if (array.Length - arrayIndex < this.Count)
			throw new ArgumentException("Target array is too small.", nameof(array));

		var i = arrayIndex;
		foreach (var (k, v) in this._internalBuckets)
		{
			var kdv = v;
			while (kdv != null)
			{
				array[i++] = new KeyValuePair<string, TValue>(kdv.Key, kdv.Value);
				kdv = kdv.Next;
			}
		}
	}

	/// <summary>
	/// Copies the to.
	/// </summary>
	/// <param name="array">The array.</param>
	/// <param name="arrayIndex">The array index.</param>
	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (array is KeyValuePair<string, TValue>[] tarray)
		{
			(this as ICollection<KeyValuePair<string, TValue>>).CopyTo(tarray, arrayIndex);
			return;
		}

		if (array is not object[])
			throw new ArgumentException($"Array needs to be an instance of {typeof(TValue[])} or object[].");

		var i = arrayIndex;
		foreach (var (k, v) in this._internalBuckets)
		{
			var kdv = v;
			while (kdv != null)
			{
				array.SetValue(new KeyValuePair<string, TValue>(kdv.Key, kdv.Value), i++);
				kdv = kdv.Next;
			}
		}
	}

	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>An IEnumerator.</returns>
	IEnumerator IEnumerable.GetEnumerator()
		=> this.GetEnumerator();

	/// <summary>
	/// Tries the insert internal.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <param name="replace">If true, replace.</param>
	/// <returns>A bool.</returns>
	private bool TryInsertInternal(string key, TValue value, bool replace)
	{
		if (key == null)
			throw new ArgumentNullException(nameof(key), "Key cannot be null.");

		var hash = key.CalculateKnuthHash();
		if (!this._internalBuckets.ContainsKey(hash))
		{
			this._internalBuckets.Add(hash, new KeyedValue(key, hash, value));
			this.Count++;
			return true;
		}

		var kdv = this._internalBuckets[hash];
		var kdvLast = kdv;
		while (kdv != null)
		{
			if (kdv.Key == key)
			{
				if (!replace)
					return false;

				kdv.Value = value;
				return true;
			}

			kdvLast = kdv;
			kdv = kdv.Next;
		}

		kdvLast.Next = new KeyedValue(key, hash, value);
		this.Count++;
		return true;
	}

	/// <summary>
	/// Tries the retrieve internal.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <returns>A bool.</returns>
	private bool TryRetrieveInternal(ReadOnlySpan<char> key, out TValue value)
	{
		value = default;

		var hash = key.CalculateKnuthHash();
		if (!this._internalBuckets.TryGetValue(hash, out var kdv))
			return false;

		while (kdv != null)
		{
			if (key.SequenceEqual(kdv.Key.AsSpan()))
			{
				value = kdv.Value;
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Tries the remove internal.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <returns>A bool.</returns>
	private bool TryRemoveInternal(ReadOnlySpan<char> key, out TValue value)
	{
		value = default;

		var hash = key.CalculateKnuthHash();
		if (!this._internalBuckets.TryGetValue(hash, out var kdv))
			return false;

		if (kdv.Next == null && key.SequenceEqual(kdv.Key.AsSpan()))
		{
			// Only bucket under this hash and key matches, pop the entire bucket

			value = kdv.Value;
			this._internalBuckets.Remove(hash);
			this.Count--;
			return true;
		}
		else if (kdv.Next == null)
		{
			// Only bucket under this hash and key does not match, cannot remove

			return false;
		}
		else if (key.SequenceEqual(kdv.Key.AsSpan()))
		{
			// First key in the bucket matches, pop it and set its child as current bucket

			value = kdv.Value;
			this._internalBuckets[hash] = kdv.Next;
			this.Count--;
			return true;
		}

		var kdvLast = kdv;
		kdv = kdv.Next;
		while (kdv != null)
		{
			if (key.SequenceEqual(kdv.Key.AsSpan()))
			{
				// Key matched, remove this bucket from the chain

				value = kdv.Value;
				kdvLast.Next = kdv.Next;
				this.Count--;
				return true;
			}

			kdvLast = kdv;
			kdv = kdv.Next;
		}

		return false;
	}

	/// <summary>
	/// Contains the key internal.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <returns>A bool.</returns>
	private bool ContainsKeyInternal(ReadOnlySpan<char> key)
	{
		var hash = key.CalculateKnuthHash();
		if (!this._internalBuckets.TryGetValue(hash, out var kdv))
			return false;

		while (kdv != null)
		{
			if (key.SequenceEqual(kdv.Key.AsSpan()))
				return true;

			kdv = kdv.Next;
		}

		return false;
	}

	/// <summary>
	/// Gets the keys internal.
	/// </summary>
	/// <returns>An ImmutableArray.</returns>
	private ImmutableArray<string> GetKeysInternal()
	{
		var builder = ImmutableArray.CreateBuilder<string>(this.Count);
		foreach (var value in this._internalBuckets.Values)
		{
			var kdv = value;
			while (kdv != null)
			{
				builder.Add(kdv.Key);
				kdv = kdv.Next;
			}
		}

		return builder.MoveToImmutable();
	}

	/// <summary>
	/// Gets the values internal.
	/// </summary>
	/// <returns>An ImmutableArray.</returns>
	private ImmutableArray<TValue> GetValuesInternal()
	{
		var builder = ImmutableArray.CreateBuilder<TValue>(this.Count);
		foreach (var value in this._internalBuckets.Values)
		{
			var kdv = value;
			while (kdv != null)
			{
				builder.Add(kdv.Value);
				kdv = kdv.Next;
			}
		}

		return builder.MoveToImmutable();
	}

	/// <summary>
	/// The keyed value.
	/// </summary>
	private class KeyedValue
	{
		/// <summary>
		/// Gets the key hash.
		/// </summary>
		public ulong KeyHash { get; }
		/// <summary>
		/// Gets the key.
		/// </summary>
		public string Key { get; }
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		public TValue Value { get; set; }

		/// <summary>
		/// Gets or sets the next.
		/// </summary>
		public KeyedValue Next { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyedValue"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="keyHash">The key hash.</param>
		/// <param name="value">The value.</param>
		public KeyedValue(string key, ulong keyHash, TValue value)
		{
			this.KeyHash = keyHash;
			this.Key = key;
			this.Value = value;
		}
	}

	/// <summary>
	/// The enumerator.
	/// </summary>
	private class Enumerator :
		IEnumerator<KeyValuePair<string, TValue>>,
		IDictionaryEnumerator
	{
		/// <summary>
		/// Gets the current.
		/// </summary>
		public KeyValuePair<string, TValue> Current { get; private set; }
		/// <summary>
		/// Gets the current.
		/// </summary>
		object IEnumerator.Current => this.Current;
		/// <summary>
		/// Gets the key.
		/// </summary>
		object IDictionaryEnumerator.Key => this.Current.Key;
		/// <summary>
		/// Gets the value.
		/// </summary>
		object IDictionaryEnumerator.Value => this.Current.Value;
		/// <summary>
		/// Gets the entry.
		/// </summary>
		DictionaryEntry IDictionaryEnumerator.Entry => new(this.Current.Key, this.Current.Value);

		/// <summary>
		/// Gets the internal dictionary.
		/// </summary>
		private readonly CharSpanLookupDictionary<TValue> _internalDictionary;

		/// <summary>
		/// Gets the internal enumerator.
		/// </summary>
		private readonly IEnumerator<KeyValuePair<ulong, KeyedValue>> _internalEnumerator;

		/// <summary>
		/// Gets or sets the current value.
		/// </summary>
		private KeyedValue _currentValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="Enumerator"/> class.
		/// </summary>
		/// <param name="spDict">The sp dict.</param>
		public Enumerator(CharSpanLookupDictionary<TValue> spDict)
		{
			this._internalDictionary = spDict;
			this._internalEnumerator = this._internalDictionary._internalBuckets.GetEnumerator();
		}

		/// <summary>
		/// Moves the next.
		/// </summary>
		/// <returns>A bool.</returns>
		public bool MoveNext()
		{
			var kdv = this._currentValue;
			if (kdv == null)
			{
				if (!this._internalEnumerator.MoveNext())
					return false;

				kdv = this._internalEnumerator.Current.Value;
				this.Current = new KeyValuePair<string, TValue>(kdv.Key, kdv.Value);

				this._currentValue = kdv.Next;
				return true;
			}

			this.Current = new KeyValuePair<string, TValue>(kdv.Key, kdv.Value);
			this._currentValue = kdv.Next;
			return true;
		}

		/// <summary>
		/// Resets the.
		/// </summary>
		public void Reset()
		{
			this._internalEnumerator.Reset();
			this.Current = default;
			this._currentValue = null;
		}

		/// <summary>
		/// Disposes the.
		/// </summary>
		public void Dispose() => this.Reset();
	}
}
