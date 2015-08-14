using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DisCatSharp.Common;

/// <summary>
/// Various Methods for Linq
/// </summary>
public static class LinqMethods
{
	/// <summary>
	/// Safely tries to get the first match out of a list.
	/// </summary>
	/// <typeparam name="TSource">Value type of list.</typeparam>
	/// <param name="list">The list to use.</param>
	/// <param name="predicate">The predicate.</param>
	/// <param name="value">The value to get if succeeded</param>
	/// <returns>Whether a value was found.</returns>
	public static bool TryGetFirstValueWhere<TSource>(this List<TSource?>? list, Func<TSource?, bool> predicate, [NotNullWhen(true)] out TSource? value)
	{
		ArgumentNullException.ThrowIfNull(predicate);
		if (list.EmptyOrNull())
		{
			value = default;
			return false;
		}

		value = list.Where(predicate).FirstOrDefault();

		return value is not null;
	}

	/// <summary>
	/// Safely tries to extract the value of the first match where target key is found, otherwise null.
	/// </summary>
	/// <typeparam name="TKey">Key type of dictionary.</typeparam>
	/// <typeparam name="TValue">Value type of dictionary.</typeparam>
	/// <param name="dict">The dictionary to use.</param>
	/// <param name="key">The key to search for.</param>
	/// <param name="value">The value to get if succeeded.</param>
	/// <returns>Whether a value was found through the key.</returns>
	public static bool TryGetFirstValueByKey<TKey, TValue>(this Dictionary<TKey, TValue?>? dict, TKey key, [NotNullWhen(true)] out TValue? value)
		where TKey : notnull
	{
		if (dict is not null)
			return dict.TryGetValue(key, out value);

		value = default;
		return false;
	}
}
