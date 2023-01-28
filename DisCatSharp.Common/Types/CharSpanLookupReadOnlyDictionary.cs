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
using System.Collections.ObjectModel;

namespace DisCatSharp.Common;

/// <summary>
/// Represents collection of string keys and <typeparamref name="TValue"/> values, allowing the use of <see cref="ReadOnlySpan{T}"/> for dictionary operations.
/// </summary>
/// <typeparam name="TValue">Type of items in this dictionary.</typeparam>
public sealed class CharSpanLookupReadOnlyDictionary<TValue> : IReadOnlyDictionary<string, TValue>
{
	/// <summary>
	/// Gets the collection of all keys present in this dictionary.
	/// </summary>
	public IEnumerable<string> Keys => this.GetKeysInternal();

	/// <summary>
	/// Gets the collection of all values present in this dictionary.
	/// </summary>
	public IEnumerable<TValue> Values => this.GetValuesInternal();

	/// <summary>
	/// Gets the total number of items in this dictionary.
	/// </summary>
	public int Count { get; }

	/// <summary>
	/// Gets a value corresponding to given key in this dictionary.
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
	}

	/// <summary>
	/// Gets a value corresponding to given key in this dictionary.
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
	}

	/// <summary>
	/// Gets the internal buckets.
	/// </summary>
	private readonly IReadOnlyDictionary<ulong, KeyedValue> _internalBuckets;

	/// <summary>
	/// Creates a new <see cref="CharSpanLookupReadOnlyDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied dictionary.
	/// </summary>
	/// <param name="values">Dictionary containing items to populate this dictionary with.</param>
	public CharSpanLookupReadOnlyDictionary(IDictionary<string, TValue> values)
		: this(values as IEnumerable<KeyValuePair<string, TValue>>)
	{ }

	/// <summary>
	/// Creates a new <see cref="CharSpanLookupReadOnlyDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied dictionary.
	/// </summary>
	/// <param name="values">Dictionary containing items to populate this dictionary with.</param>
	public CharSpanLookupReadOnlyDictionary(IReadOnlyDictionary<string, TValue> values)
		: this(values as IEnumerable<KeyValuePair<string, TValue>>)
	{ }

	/// <summary>
	/// Creates a new <see cref="CharSpanLookupReadOnlyDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied key-value collection.
	/// </summary>
	/// <param name="values">Dictionary containing items to populate this dictionary with.</param>
	public CharSpanLookupReadOnlyDictionary(IEnumerable<KeyValuePair<string, TValue>> values)
	{
		this._internalBuckets = PrepareItems(values, out var count);
		this.Count = count;
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
	/// Gets an enumerator over key-value pairs in this dictionary.
	/// </summary>
	/// <returns></returns>
	public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
		=> new Enumerator(this);

	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>An IEnumerator.</returns>
	IEnumerator IEnumerable.GetEnumerator()
		=> this.GetEnumerator();

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
	/// Prepares the items.
	/// </summary>
	/// <param name="items">The items.</param>
	/// <param name="count">The count.</param>
	/// <returns>An IReadOnlyDictionary.</returns>
	private static IReadOnlyDictionary<ulong, KeyedValue> PrepareItems(IEnumerable<KeyValuePair<string, TValue>> items, out int count)
	{
		count = 0;
		var dict = new Dictionary<ulong, KeyedValue>();
		foreach (var (k, v) in items)
		{
			if (k == null)
				throw new ArgumentException("Keys cannot be null.", nameof(items));

			var hash = k.CalculateKnuthHash();
			if (!dict.ContainsKey(hash))
			{
				dict.Add(hash, new KeyedValue(k, hash, v));
				count++;
				continue;
			}

			var kdv = dict[hash];
			var kdvLast = kdv;
			while (kdv != null)
			{
				if (kdv.Key == k)
					throw new ArgumentException("Given key is already present in the dictionary.", nameof(items));

				kdvLast = kdv;
				kdv = kdv.Next;
			}

			kdvLast.Next = new KeyedValue(k, hash, v);
			count++;
		}

		return new ReadOnlyDictionary<ulong, KeyedValue>(dict);
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
	private class Enumerator : IEnumerator<KeyValuePair<string, TValue>>
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
		/// Gets the internal dictionary.
		/// </summary>
		private readonly CharSpanLookupReadOnlyDictionary<TValue> _internalDictionary;

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
		public Enumerator(CharSpanLookupReadOnlyDictionary<TValue> spDict)
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
