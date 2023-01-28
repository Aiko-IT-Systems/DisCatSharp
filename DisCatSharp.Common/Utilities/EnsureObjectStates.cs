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

using System.Collections.Generic;
using System.Linq;

namespace DisCatSharp.Common;

/// <summary>
/// Ensures that certain objects have the target state.
/// </summary>
public static class EnsureObjectStates
{
	/// <summary>
	/// Checks whether the dictionary is null or empty.
	/// </summary>
	/// <typeparam name="T1">Any key type.</typeparam>
	/// <typeparam name="T2">Any value type.</typeparam>
	/// <param name="dictionary">The dictionary to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool EmptyOrNull<T1, T2>(this Dictionary<T1, T2?>? dictionary) where T1 : notnull
		=> dictionary == null || !dictionary.Any() || !dictionary.Keys.Any();

	/// <summary>
	/// Checks whether the dictionary is not null and not empty.
	/// </summary>
	/// <typeparam name="T1">Any key type.</typeparam>
	/// <typeparam name="T2">Any value type.</typeparam>
	/// <param name="dictionary">The dictionary to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool NotEmptyAndNotNull<T1, T2>(this Dictionary<T1, T2?>? dictionary) where T1 : notnull
		=> dictionary != null && dictionary.Any() && dictionary.Keys.Any();

	/// <summary>
	/// Checks whether the list is null or empty.
	/// </summary>
	/// <typeparam name="T">Any value type.</typeparam>
	/// <param name="list">The list to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool EmptyOrNull<T>(this List<T?>? list)
		=> list == null || !list.Any();

	/// <summary>
	/// Checks whether the list is not null and not empty.
	/// </summary>
	/// <typeparam name="T">Any value type.</typeparam>
	/// <param name="list">The list to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool NotEmptyAndNotNull<T>(this List<T?>? list)
		=> list != null && list.Any();
}
