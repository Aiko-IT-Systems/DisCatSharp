using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DisCatSharp;

/// <summary>
///     Read-only view of a given <see cref="ConcurrentDictionary{TKey,TValue}" />.
/// </summary>
/// <remarks>
///     This type exists because <see cref="ConcurrentDictionary{TKey,TValue}" /> is not an
///     <see cref="IReadOnlyDictionary{TKey,TValue}" /> in .NET Standard 1.1.
/// </remarks>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
internal readonly struct ReadOnlyConcurrentDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
	/// <summary>
	///     Gets the underlying dictionary.
	/// </summary>
	private readonly ConcurrentDictionary<TKey, TValue> _underlyingDict;

	/// <summary>
	///     Creates a new read-only view of the given dictionary.
	/// </summary>
	/// <param name="underlyingDict">Dictionary to create a view over.</param>
	public ReadOnlyConcurrentDictionary(ConcurrentDictionary<TKey, TValue> underlyingDict)
	{
		this._underlyingDict = underlyingDict;
	}

	/// <summary>
	///     Gets the enumerator.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._underlyingDict.GetEnumerator();

	/// <summary>
	///     Gets the enumerator.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this._underlyingDict).GetEnumerator();

	/// <summary>
	///     Gets the count.
	/// </summary>
	public int Count => this._underlyingDict.Count;

	/// <summary>
	///     Contains the key.
	/// </summary>
	/// <param name="key">The key.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ContainsKey(TKey key) => this._underlyingDict.ContainsKey(key);

	/// <summary>
	///     Tries the get value.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue(TKey key, out TValue value) => this._underlyingDict.TryGetValue(key, out value);

	public TValue this[TKey key] => this._underlyingDict[key];

	/// <summary>
	///     Gets the keys.
	/// </summary>
	public IEnumerable<TKey> Keys => this._underlyingDict.Keys;

	/// <summary>
	///     Gets the values.
	/// </summary>
	public IEnumerable<TValue> Values => this._underlyingDict.Values;
}
