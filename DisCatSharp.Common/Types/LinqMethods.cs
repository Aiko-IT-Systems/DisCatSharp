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
using System.Collections.Generic;
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
	public static bool GetFirstValueWhere<TSource>(this List<TSource?> list, Func<TSource?, bool> predicate, out TSource? value)
	{
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
	public static bool GetFirstValueByKey<TKey, TValue>(this Dictionary<TKey, TValue?>? dict, TKey key, out TValue? value)
		where TKey : notnull
	{
		if (dict == null)
		{
			value = default;
			return false;
		}

		return dict.TryGetValue(key, out value);
	}
}
