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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DisCatSharp.Common
{
    /// <summary>
    /// Represents collection of string keys and <typeparamref name="TValue"/> values, allowing the use of <see cref="System.ReadOnlySpan{T}"/> for dictionary operations.
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
        public int Count { get; private set; } = 0;

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
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets or sets a value corresponding to given key in this dictionary.
        /// </summary>
        /// <param name="Key">Key to get or set the value for.</param>
        /// <returns>Value matching the supplied key, if applicable.</returns>
        public TValue this[string Key]
        {
            get
            {
                if (Key == null)
                    throw new ArgumentNullException(nameof(Key));

                if (!this.TryRetrieveInternal(Key.AsSpan(), out var value))
                    throw new KeyNotFoundException($"The given key '{Key}' was not present in the dictionary.");

                return value;
            }

            set
            {
                if (Key == null)
                    throw new ArgumentNullException(nameof(Key));

                this.TryInsertInternal(Key, value, true);
            }
        }

        /// <summary>
        /// Gets or sets a value corresponding to given key in this dictionary.
        /// </summary>
        /// <param name="Key">Key to get or set the value for.</param>
        /// <returns>Value matching the supplied key, if applicable.</returns>
        public TValue this[ReadOnlySpan<char> Key]
        {
            get
            {
                if (!this.TryRetrieveInternal(Key, out var value))
                    throw new KeyNotFoundException($"The given key was not present in the dictionary.");

                return value;
            }

#if NETCOREAPP
            set => this.TryInsertInternal(new string(key), value, true);
#else
            set
            {
                unsafe
                {
                    fixed (char* chars = &Key.GetPinnableReference())
                        this.TryInsertInternal(new string(chars, 0, Key.Length), value, true);
                }
            }
#endif
        }

        object IDictionary.this[object Key]
        {
            get
            {
                if (!(Key is string tkey))
                    throw new ArgumentException("Key needs to be an instance of a string.");

                if (!this.TryRetrieveInternal(tkey.AsSpan(), out var value))
                    throw new KeyNotFoundException($"The given key '{tkey}' was not present in the dictionary.");

                return value;
            }

            set
            {
                if (!(Key is string tkey))
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
        private Dictionary<ulong, KeyedValue> InternalBuckets { get; }

        /// <summary>
        /// Creates a new, empty <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/>.
        /// </summary>
        public CharSpanLookupDictionary()
        {
            this.InternalBuckets = new Dictionary<ulong, KeyedValue>();
        }

        /// <summary>
        /// Creates a new, empty <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and sets its initial capacity to specified value.
        /// </summary>
        /// <param name="InitialCapacity">Initial capacity of the dictionary.</param>
        public CharSpanLookupDictionary(int InitialCapacity)
        {
            this.InternalBuckets = new Dictionary<ulong, KeyedValue>(InitialCapacity);
        }

        /// <summary>
        /// Creates a new <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied dictionary.
        /// </summary>
        /// <param name="Values">Dictionary containing items to populate this dictionary with.</param>
        public CharSpanLookupDictionary(IDictionary<string, TValue> Values)
            : this(Values.Count)
        {
            foreach (var (k, v) in Values)
                this.Add(k, v);
        }

        /// <summary>
        /// Creates a new <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied dictionary.
        /// </summary>
        /// <param name="Values">Dictionary containing items to populate this dictionary with.</param>
        public CharSpanLookupDictionary(IReadOnlyDictionary<string, TValue> Values)
             : this(Values.Count)
        {
            foreach (var (k, v) in Values)
                this.Add(k, v);
        }

        /// <summary>
        /// Creates a new <see cref="CharSpanLookupDictionary{TValue}"/> with string keys and items of type <typeparamref name="TValue"/> and populates it with key-value pairs from supplied key-value collection.
        /// </summary>
        /// <param name="Values">Dictionary containing items to populate this dictionary with.</param>
        public CharSpanLookupDictionary(IEnumerable<KeyValuePair<string, TValue>> Values)
            : this()
        {
            foreach (var (k, v) in Values)
                this.Add(k, v);
        }

        /// <summary>
        /// Inserts a specific key and corresponding value into this dictionary.
        /// </summary>
        /// <param name="Key">Key to insert.</param>
        /// <param name="Value">Value corresponding to this key.</param>
        public void Add(string Key, TValue Value)
        {
            if (!this.TryInsertInternal(Key, Value, false))
                throw new ArgumentException("Given key is already present in the dictionary.", nameof(Key));
        }

        /// <summary>
        /// Inserts a specific key and corresponding value into this dictionary.
        /// </summary>
        /// <param name="Key">Key to insert.</param>
        /// <param name="Value">Value corresponding to this key.</param>
        public void Add(ReadOnlySpan<char> Key, TValue Value)
#if NETCOREAPP
        {
            if (!this.TryInsertInternal(new string(key), value, false))
                throw new ArgumentException("Given key is already present in the dictionary.", nameof(key));
        }
#else
        {
            unsafe
            {
                fixed (char* chars = &Key.GetPinnableReference())
                    if (!this.TryInsertInternal(new string(chars, 0, Key.Length), Value, false))
                        throw new ArgumentException("Given key is already present in the dictionary.", nameof(Key));
            }
        }
#endif

        /// <summary>
        /// Attempts to insert a specific key and corresponding value into this dictionary.
        /// </summary>
        /// <param name="Key">Key to insert.</param>
        /// <param name="Value">Value corresponding to this key.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryAdd(string Key, TValue Value)
            => this.TryInsertInternal(Key, Value, false);

        /// <summary>
        /// Attempts to insert a specific key and corresponding value into this dictionary.
        /// </summary>
        /// <param name="Key">Key to insert.</param>
        /// <param name="Value">Value corresponding to this key.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryAdd(ReadOnlySpan<char> Key, TValue Value)
#if NETCOREAPP
            => this.TryInsertInternal(new string(key), value, false);
#else
        {
            unsafe
            {
                fixed (char* chars = &Key.GetPinnableReference())
                    return this.TryInsertInternal(new string(chars, 0, Key.Length), Value, false);
            }
        }
#endif

        /// <summary>
        /// Attempts to retrieve a value corresponding to the supplied key from this dictionary.
        /// </summary>
        /// <param name="Key">Key to retrieve the value for.</param>
        /// <param name="Value">Retrieved value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryGetValue(string Key, out TValue Value)
        {
            if (Key == null)
                throw new ArgumentNullException(nameof(Key));

            return this.TryRetrieveInternal(Key.AsSpan(), out Value);
        }

        /// <summary>
        /// Attempts to retrieve a value corresponding to the supplied key from this dictionary.
        /// </summary>
        /// <param name="Key">Key to retrieve the value for.</param>
        /// <param name="Value">Retrieved value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryGetValue(ReadOnlySpan<char> Key, out TValue Value)
            => this.TryRetrieveInternal(Key, out Value);

        /// <summary>
        /// Attempts to remove a value corresponding to the supplied key from this dictionary.
        /// </summary>
        /// <param name="Key">Key to remove the value for.</param>
        /// <param name="Value">Removed value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryRemove(string Key, out TValue Value)
        {
            if (Key == null)
                throw new ArgumentNullException(nameof(Key));

            return this.TryRemoveInternal(Key.AsSpan(), out Value);
        }

        /// <summary>
        /// Attempts to remove a value corresponding to the supplied key from this dictionary.
        /// </summary>
        /// <param name="Key">Key to remove the value for.</param>
        /// <param name="Value">Removed value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryRemove(ReadOnlySpan<char> Key, out TValue Value)
            => this.TryRemoveInternal(Key, out Value);

        /// <summary>
        /// Checks whether this dictionary contains the specified key.
        /// </summary>
        /// <param name="Key">Key to check for in this dictionary.</param>
        /// <returns>Whether the key was present in the dictionary.</returns>
        public bool ContainsKey(string Key)
            => this.ContainsKeyInternal(Key.AsSpan());

        /// <summary>
        /// Checks whether this dictionary contains the specified key.
        /// </summary>
        /// <param name="Key">Key to check for in this dictionary.</param>
        /// <returns>Whether the key was present in the dictionary.</returns>
        public bool ContainsKey(ReadOnlySpan<char> Key)
            => this.ContainsKeyInternal(Key);

        /// <summary>
        /// Removes all items from this dictionary.
        /// </summary>
        public void Clear()
        {
            this.InternalBuckets.Clear();
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
        /// <param name="Key">The key.</param>
        /// <returns>A bool.</returns>
        bool IDictionary<string, TValue>.Remove(string Key)
            => this.TryRemove(Key.AsSpan(), out _);

        /// <summary>
        /// Adds the.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="Value">The value.</param>
        void IDictionary.Add(object Key, object Value)
        {
            if (!(Key is string tkey))
                throw new ArgumentException("Key needs to be an instance of a string.");

            if (!(Value is TValue tvalue))
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
        /// <param name="Key">The key.</param>
        void IDictionary.Remove(object Key)
        {
            if (!(Key is string tkey))
                throw new ArgumentException("Key needs to be an instance of a string.");

            this.TryRemove(tkey, out _);
        }

        /// <summary>
        /// Contains the.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns>A bool.</returns>
        bool IDictionary.Contains(object Key)
        {
            if (!(Key is string tkey))
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
        /// <param name="Item">The item.</param>
        void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> Item)
            => this.Add(Item.Key, Item.Value);

        /// <summary>
        /// Removes the.
        /// </summary>
        /// <param name="Item">The item.</param>
        /// <returns>A bool.</returns>
        bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> Item)
            => this.TryRemove(Item.Key, out _);

        /// <summary>
        /// Contains the.
        /// </summary>
        /// <param name="Item">The item.</param>
        /// <returns>A bool.</returns>
        bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> Item)
            => this.TryGetValue(Item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, Item.Value);

        /// <summary>
        /// Copies the to.
        /// </summary>
        /// <param name="Array">The array.</param>
        /// <param name="ArrayIndex">The array index.</param>
        void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] Array, int ArrayIndex)
        {
            if (Array.Length - ArrayIndex < this.Count)
                throw new ArgumentException("Target array is too small.", nameof(Array));

            var i = ArrayIndex;
            foreach (var (k, v) in this.InternalBuckets)
            {
                var kdv = v;
                while (kdv != null)
                {
                    Array[i++] = new KeyValuePair<string, TValue>(kdv.Key, kdv.Value);
                    kdv = kdv.Next;
                }
            }
        }

        /// <summary>
        /// Copies the to.
        /// </summary>
        /// <param name="Array">The array.</param>
        /// <param name="ArrayIndex">The array index.</param>
        void ICollection.CopyTo(Array Array, int ArrayIndex)
        {
            if (Array is KeyValuePair<string, TValue>[] tarray)
            {
                (this as ICollection<KeyValuePair<string, TValue>>).CopyTo(tarray, ArrayIndex);
                return;
            }

            if (Array is not object[])
                throw new ArgumentException($"Array needs to be an instance of {typeof(TValue[])} or object[].");

            var i = ArrayIndex;
            foreach (var (k, v) in this.InternalBuckets)
            {
                var kdv = v;
                while (kdv != null)
                {
                    Array.SetValue(new KeyValuePair<string, TValue>(kdv.Key, kdv.Value), i++);
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
        /// <param name="Key">The key.</param>
        /// <param name="Value">The value.</param>
        /// <param name="Replace">If true, replace.</param>
        /// <returns>A bool.</returns>
        private bool TryInsertInternal(string Key, TValue Value, bool Replace)
        {
            if (Key == null)
                throw new ArgumentNullException(nameof(Key), "Key cannot be null.");

            var hash = Key.CalculateKnuthHash();
            if (!this.InternalBuckets.ContainsKey(hash))
            {
                this.InternalBuckets.Add(hash, new KeyedValue(Key, hash, Value));
                this.Count++;
                return true;
            }

            var kdv = this.InternalBuckets[hash];
            var kdvLast = kdv;
            while (kdv != null)
            {
                if (kdv.Key == Key)
                {
                    if (!Replace)
                        return false;

                    kdv.Value = Value;
                    return true;
                }

                kdvLast = kdv;
                kdv = kdv.Next;
            }

            kdvLast.Next = new KeyedValue(Key, hash, Value);
            this.Count++;
            return true;
        }

        /// <summary>
        /// Tries the retrieve internal.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="Value">The value.</param>
        /// <returns>A bool.</returns>
        private bool TryRetrieveInternal(ReadOnlySpan<char> Key, out TValue Value)
        {
            Value = default;

            var hash = Key.CalculateKnuthHash();
            if (!this.InternalBuckets.TryGetValue(hash, out var kdv))
                return false;

            while (kdv != null)
            {
                if (Key.SequenceEqual(kdv.Key.AsSpan()))
                {
                    Value = kdv.Value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries the remove internal.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="Value">The value.</param>
        /// <returns>A bool.</returns>
        private bool TryRemoveInternal(ReadOnlySpan<char> Key, out TValue Value)
        {
            Value = default;

            var hash = Key.CalculateKnuthHash();
            if (!this.InternalBuckets.TryGetValue(hash, out var kdv))
                return false;

            if (kdv.Next == null && Key.SequenceEqual(kdv.Key.AsSpan()))
            {
                // Only bucket under this hash and key matches, pop the entire bucket

                Value = kdv.Value;
                this.InternalBuckets.Remove(hash);
                this.Count--;
                return true;
            }
            else if (kdv.Next == null)
            {
                // Only bucket under this hash and key does not match, cannot remove

                return false;
            }
            else if (Key.SequenceEqual(kdv.Key.AsSpan()))
            {
                // First key in the bucket matches, pop it and set its child as current bucket

                Value = kdv.Value;
                this.InternalBuckets[hash] = kdv.Next;
                this.Count--;
                return true;
            }

            var kdvLast = kdv;
            kdv = kdv.Next;
            while (kdv != null)
            {
                if (Key.SequenceEqual(kdv.Key.AsSpan()))
                {
                    // Key matched, remove this bucket from the chain

                    Value = kdv.Value;
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
        /// <param name="Key">The key.</param>
        /// <returns>A bool.</returns>
        private bool ContainsKeyInternal(ReadOnlySpan<char> Key)
        {
            var hash = Key.CalculateKnuthHash();
            if (!this.InternalBuckets.TryGetValue(hash, out var kdv))
                return false;

            while (kdv != null)
            {
                if (Key.SequenceEqual(kdv.Key.AsSpan()))
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
            foreach (var value in this.InternalBuckets.Values)
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
            foreach (var value in this.InternalBuckets.Values)
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
            /// <param name="Key">The key.</param>
            /// <param name="KeyHash">The key hash.</param>
            /// <param name="Value">The value.</param>
            public KeyedValue(string Key, ulong KeyHash, TValue Value)
            {
                this.KeyHash = KeyHash;
                this.Key = Key;
                this.Value = Value;
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
            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(this.Current.Key, this.Current.Value);

            /// <summary>
            /// Gets the internal dictionary.
            /// </summary>
            private CharSpanLookupDictionary<TValue> InternalDictionary { get; }
            /// <summary>
            /// Gets the internal enumerator.
            /// </summary>
            private IEnumerator<KeyValuePair<ulong, KeyedValue>> InternalEnumerator { get; }
            /// <summary>
            /// Gets or sets the current value.
            /// </summary>
            private KeyedValue CurrentValue { get; set; } = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> class.
            /// </summary>
            /// <param name="SpDict">The sp dict.</param>
            public Enumerator(CharSpanLookupDictionary<TValue> SpDict)
            {
                this.InternalDictionary = SpDict;
                this.InternalEnumerator = this.InternalDictionary.InternalBuckets.GetEnumerator();
            }

            /// <summary>
            /// Moves the next.
            /// </summary>
            /// <returns>A bool.</returns>
            public bool MoveNext()
            {
                var kdv = this.CurrentValue;
                if (kdv == null)
                {
                    if (!this.InternalEnumerator.MoveNext())
                        return false;

                    kdv = this.InternalEnumerator.Current.Value;
                    this.Current = new KeyValuePair<string, TValue>(kdv.Key, kdv.Value);

                    this.CurrentValue = kdv.Next;
                    return true;
                }

                this.Current = new KeyValuePair<string, TValue>(kdv.Key, kdv.Value);
                this.CurrentValue = kdv.Next;
                return true;
            }

            /// <summary>
            /// Resets the.
            /// </summary>
            public void Reset()
            {
                this.InternalEnumerator.Reset();
                this.Current = default;
                this.CurrentValue = null;
            }

            /// <summary>
            /// Disposes the.
            /// </summary>
            public void Dispose()
            {
                this.Reset();
            }
        }
    }
}
