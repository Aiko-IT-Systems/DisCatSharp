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
using System.Runtime.CompilerServices;

namespace DisCatSharp.Common;

/// <summary>
/// Assortment of various extension and utility methods, designed to make working with various types a little easier.
/// </summary>
public static class Extensions
{
	/// <summary>
	/// <para>Deconstructs a <see cref="Dictionary{TKey, TValue}"/> key-value pair item (<see cref="KeyValuePair{TKey, TValue}"/>) into 2 separate variables.</para>
	/// <para>This allows for enumerating over dictionaries in foreach blocks by using a (k, v) tuple as the enumerator variable, instead of having to use a <see cref="KeyValuePair{TKey, TValue}"/> directly.</para>
	/// </summary>
	/// <typeparam name="TKey">Type of dictionary item key.</typeparam>
	/// <typeparam name="TValue">Type of dictionary item value.</typeparam>
	/// <param name="kvp">Key-value pair to deconstruct.</param>
	/// <param name="key">Deconstructed key.</param>
	/// <param name="value">Deconstructed value.</param>
	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
	{
		key = kvp.Key;
		value = kvp.Value;
	}

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this sbyte num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(num == sbyte.MinValue ? num + 1 : num))) + (num < 0 ? 2 /* include sign */ : 1);

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this byte num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(num)) + 1;

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this short num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(num == short.MinValue ? num + 1 : num))) + (num < 0 ? 2 /* include sign */ : 1);

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this ushort num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(num)) + 1;

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this int num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(num == int.MinValue ? num + 1 : num))) + (num < 0 ? 2 /* include sign */ : 1);

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this uint num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(num)) + 1;

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this long num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(num == long.MinValue ? num + 1 : num))) + (num < 0 ? 2 /* include sign */ : 1);

	/// <summary>
	/// Calculates the length of string representation of given number in base 10 (including sign, if present).
	/// </summary>
	/// <param name="num">Number to calculate the length of.</param>
	/// <returns>Calculated number length.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateLength(this ulong num)
		=> num == 0 ? 1 : (int)Math.Floor(Math.Log10(num)) + 1;

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this sbyte num, sbyte min, sbyte max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this byte num, byte min, byte max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this short num, short min, short max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this ushort num, ushort min, ushort max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this int num, int min, int max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this uint num, uint min, uint max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this long num, long min, long max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this ulong num, ulong min, ulong max, bool inclusive = true)
	{
		if (min > max)
		{
			min ^= max;
			max ^= min;
			min ^= max;
		}

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this float num, float min, float max, bool inclusive = true)
	{
		if (min > max)
			return false;

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Tests whether given value is in supplied range, optionally allowing it to be an exclusive check.
	/// </summary>
	/// <param name="num">Number to test.</param>
	/// <param name="min">Lower bound of the range.</param>
	/// <param name="max">Upper bound of the range.</param>
	/// <param name="inclusive">Whether the check is to be inclusive.</param>
	/// <returns>Whether the value is in range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInRange(this double num, double min, double max, bool inclusive = true)
	{
		if (min > max)
			return false;

		return inclusive ? num >= min && num <= max : num > min && num < max;
	}

	/// <summary>
	/// Returns whether supplied character is in any of the following ranges: a-z, A-Z, 0-9.
	/// </summary>
	/// <param name="c">Character to test.</param>
	/// <returns>Whether the character is in basic alphanumeric character range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBasicAlphanumeric(this char c)
		=> (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');

	/// <summary>
	/// Returns whether supplied character is in the 0-9 range.
	/// </summary>
	/// <param name="c">Character to test.</param>
	/// <returns>Whether the character is in basic numeric digit character range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBasicDigit(this char c)
		=> c >= '0' && c <= '9';

	/// <summary>
	/// Returns whether supplied character is in the a-z or A-Z range.
	/// </summary>
	/// <param name="c">Character to test.</param>
	/// <returns>Whether the character is in basic letter character range.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBasicLetter(this char c)
		=> (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

	/// <summary>
	/// Tests whether given string ends with given character.
	/// </summary>
	/// <param name="s">String to test.</param>
	/// <param name="c">Character to test for.</param>
	/// <returns>Whether the supplied string ends with supplied character.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EndsWithCharacter(this string s, char c)
		=> s.Length >= 1 && s[^1] == c;

	/// <summary>
	/// Tests whether given string starts with given character.
	/// </summary>
	/// <param name="s">String to test.</param>
	/// <param name="c">Character to test for.</param>
	/// <returns>Whether the supplied string starts with supplied character.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool StartsWithCharacter(this string s, char c)
		=> s.Length >= 1 && s[0] == c;

	// https://stackoverflow.com/questions/9545619/a-fast-hash-function-for-string-in-c-sharp
	// Calls are inlined to call the underlying method directly
	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this ReadOnlySpan<char> chars)
		=> Knuth(chars);

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this Span<char> chars)
		=> Knuth(chars);

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this ReadOnlyMemory<char> chars)
		=> Knuth(chars.Span);

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this Memory<char> chars)
		=> Knuth(chars.Span);

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this ArraySegment<char> chars)
		=> Knuth(chars.AsSpan());

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this char[] chars)
		=> Knuth(chars.AsSpan());

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <param name="start">Offset in the array to start calculating from.</param>
	/// <param name="count">Number of characters to compute the hash from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this char[] chars, int start, int count)
		=> Knuth(chars.AsSpan(start, count));

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this string chars)
		=> Knuth(chars.AsSpan());

	/// <summary>
	/// Computes a 64-bit Knuth hash from supplied characters.
	/// </summary>
	/// <param name="chars">Characters to compute the hash value from.</param>
	/// <param name="start">Offset in the array to start calculating from.</param>
	/// <param name="count">Number of characters to compute the hash from.</param>
	/// <returns>Computer 64-bit Knuth hash.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong CalculateKnuthHash(this string chars, int start, int count)
		=> Knuth(chars.AsSpan(start, count));

	/// <summary>
	/// Gets the two first elements of the <see cref="IEnumerable{T}"/>, if they exist.
	/// </summary>
	/// <param name="enumerable">The enumerable.</param>
	/// <param name="values">The output values. Undefined if <code>false</code> is returned.</param>
	/// <returns>Whether the <see cref="IEnumerable{T}"/> contained enough elements.</returns>
	internal static bool TryFirstTwo<T>(this IEnumerable<T> enumerable, out (T first, T second) values)
	{
		values = default;

		using var enumerator = enumerable.GetEnumerator();

		if (!enumerator.MoveNext())
			return false;

		var first = enumerator.Current;

		if (!enumerator.MoveNext())
			return false;


		values = (first, enumerator.Current);
		return true;
	}

	/// <summary>
	/// Knuths the.
	/// </summary>
	/// <param name="chars">The chars.</param>
	/// <returns>An ulong.</returns>
	private static ulong Knuth(ReadOnlySpan<char> chars)
	{
		var hash = 3074457345618258791ul;
		foreach (var ch in chars)
			hash = (hash + ch) * 3074457345618258799ul;

		return hash;
	}

	/// <summary>
	/// Removes the first item matching the predicate from the list.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	/// <param name="predicate"></param>
	/// <returns>Whether an item was removed.</returns>
	internal static bool RemoveFirst<T>(this IList<T> list, Predicate<T> predicate)
	{
		for (var i = 0; i < list.Count; i++)
		{
			if (predicate(list[i]))
			{
				list.RemoveAt(i);
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Populates an array with the given value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="arr"></param>
	/// <param name="value"></param>
	internal static void Populate<T>(this T[] arr, T value)
	{
		for (var i = 0; i < arr.Length; i++)
		{
			arr[i] = value;
		}
	}
}
