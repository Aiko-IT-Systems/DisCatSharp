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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DSharpPlusNextGen.Common
{
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
        ICollection<string> IDictionary<string, TValue>.Keys => this.GetKeysInternal();
        ICollection IDictionary.Keys => this.GetKeysInternal();

        /// <summary>
        /// Gets the collection of all values present in this dictionary.
        /// </summary>
        public IEnumerable<TValue> Values => this.GetValuesInternal();
        ICollection<TValue> IDictionary<string, TValue>.Values => this.GetValuesInternal();
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

#if NETCOREAPP
            set => this.TryInsertInternal(new string(key), value, true);
#else
            set
            {
                unsafe
                {
                    fixed (char* chars = &key.GetPinnableReference())
                        this.TryInsertInternal(new string(chars, 0, key.Length), value, true);
                }
            }
#endif
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
        /// <param name="initialCapacity">Initial capacity of the dictionary.</param>
        public CharSpanLookupDictionary(int initialCapacity)
        {
            this.InternalBuckets = new Dictionary<ulong, KeyedValue>(initialCapacity);
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
#if NETCOREAPP
        {
            if (!this.TryInsertInternal(new string(key), value, false))
                throw new ArgumentException("Given key is already present in the dictionary.", nameof(key));
        }
#else
        {
            unsafe
            {
                fixed (char* chars = &key.GetPinnableReference())
                    if (!this.TryInsertInternal(new string(chars, 0, key.Length), value, false))
                        throw new ArgumentException("Given key is already present in the dictionary.", nameof(key));
            }
        }
#endif

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
#if NETCOREAPP
            => this.TryInsertInternal(new string(key), value, false);
#else
        {
            unsafe
            {
                fixed (char* chars = &key.GetPinnableReference())
                    return this.TryInsertInternal(new string(chars, 0, key.Length), value, false);
            }
        }
#endif

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
            this.InternalBuckets.Clear();
            this.Count = 0;
        }

        /// <summary>
        /// Gets an enumerator over key-value pairs in this dictionary.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
            => new Enumerator(this);

        bool IDictionary<string, TValue>.Remove(string key)
            => this.TryRemove(key.AsSpan(), out _);

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

        void IDictionary.Remove(object key)
        {
            if (!(key is string tkey))
                throw new ArgumentException("Key needs to be an instance of a string.");

            this.TryRemove(tkey, out _);
        }

        bool IDictionary.Contains(object key)
        {
            if (!(key is string tkey))
                throw new ArgumentException("Key needs to be an instance of a string.");

            return this.ContainsKey(tkey);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
            => new Enumerator(this);

        void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
            => this.Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
            => this.TryRemove(item.Key, out _);

        bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
            => this.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

        void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            if (array.Length - arrayIndex < this.Count)
                throw new ArgumentException("Target array is too small.", nameof(array));

            var i = arrayIndex;
            foreach (var (k, v) in this.InternalBuckets)
            {
                var kdv = v;
                while (kdv != null)
                {
                    array[i++] = new KeyValuePair<string, TValue>(kdv.Key, kdv.Value);
                    kdv = kdv.Next;
                }
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array is KeyValuePair<string, TValue>[] tarray)
            {
                (this as ICollection<KeyValuePair<string, TValue>>).CopyTo(tarray, arrayIndex);
                return;
            }

            if (!(array is object[]))
                throw new ArgumentException($"Array needs to be an instance of {typeof(TValue[])} or object[].");

            var i = arrayIndex;
            foreach (var (k, v) in this.InternalBuckets)
            {
                var kdv = v;
                while (kdv != null)
                {
                    array.SetValue(new KeyValuePair<string, TValue>(kdv.Key, kdv.Value), i++);
                    kdv = kdv.Next;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        private bool TryInsertInternal(string key, TValue value, bool replace)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key cannot be null.");

            var hash = key.CalculateKnuthHash();
            if (!this.InternalBuckets.ContainsKey(hash))
            {
                this.InternalBuckets.Add(hash, new KeyedValue(key, hash, value));
                this.Count++;
                return true;
            }

            var kdv = this.InternalBuckets[hash];
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

        private bool TryRetrieveInternal(ReadOnlySpan<char> key, out TValue value)
        {
            value = default;

            var hash = key.CalculateKnuthHash();
            if (!this.InternalBuckets.TryGetValue(hash, out var kdv))
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

        private bool TryRemoveInternal(ReadOnlySpan<char> key, out TValue value)
        {
            value = default;

            var hash = key.CalculateKnuthHash();
            if (!this.InternalBuckets.TryGetValue(hash, out var kdv))
                return false;

            if (kdv.Next == null && key.SequenceEqual(kdv.Key.AsSpan()))
            {
                // Only bucket under this hash and key matches, pop the entire bucket

                value = kdv.Value;
                this.InternalBuckets.Remove(hash);
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
                this.InternalBuckets[hash] = kdv.Next;
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

        private bool ContainsKeyInternal(ReadOnlySpan<char> key)
        {
            var hash = key.CalculateKnuthHash();
            if (!this.InternalBuckets.TryGetValue(hash, out var kdv))
                return false;

            while (kdv != null)
            {
                if (key.SequenceEqual(kdv.Key.AsSpan()))
                    return true;

                kdv = kdv.Next;
            }

            return false;
        }

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

        private class KeyedValue
        {
            public ulong KeyHash { get; }
            public string Key { get; }
            public TValue Value { get; set; }

            public KeyedValue Next { get; set; }

            public KeyedValue(string key, ulong keyHash, TValue value)
            {
                this.KeyHash = keyHash;
                this.Key = key;
                this.Value = value;
            }
        }

        private class Enumerator :
            IEnumerator<KeyValuePair<string, TValue>>,
            IDictionaryEnumerator
        {
            public KeyValuePair<string, TValue> Current { get; private set; }
            object IEnumerator.Current => this.Current;
            object IDictionaryEnumerator.Key => this.Current.Key;
            object IDictionaryEnumerator.Value => this.Current.Value;
            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(this.Current.Key, this.Current.Value);

            private CharSpanLookupDictionary<TValue> InternalDictionary { get; }
            private IEnumerator<KeyValuePair<ulong, KeyedValue>> InternalEnumerator { get; }
            private KeyedValue CurrentValue { get; set; } = null;

            public Enumerator(CharSpanLookupDictionary<TValue> spDict)
            {
                this.InternalDictionary = spDict;
                this.InternalEnumerator = this.InternalDictionary.InternalBuckets.GetEnumerator();
            }

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

            public void Reset()
            {
                this.InternalEnumerator.Reset();
                this.Current = default;
                this.CurrentValue = null;
            }

            public void Dispose()
            {
                this.Reset();
            }
        }
    }
}
