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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DisCatSharp.Common
{
    /// <summary>
    /// Assortment of various extension and utility methods, designed to make working with various types a little easier.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// <para>Deconstructs a <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> key-value pair item (<see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/>) into 2 separate variables.</para>
        /// <para>This allows for enumerating over dictionaries in foreach blocks by using a (k, v) tuple as the enumerator variable, instead of having to use a <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> directly.</para>
        /// </summary>
        /// <typeparam name="TKey">Type of dictionary item key.</typeparam>
        /// <typeparam name="TValue">Type of dictionary item value.</typeparam>
        /// <param name="Kvp">Key-value pair to deconstruct.</param>
        /// <param name="Key">Deconstructed key.</param>
        /// <param name="Value">Deconstructed value.</param>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> Kvp, out TKey Key, out TValue Value)
        {
            Key = Kvp.Key;
            Value = Kvp.Value;
        }

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated number length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this sbyte Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(Num == sbyte.MinValue ? Num + 1 : Num))) + (Num < 0 ? 2 /* include sign */ : 1);

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated nuembr length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this byte Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Num)) + 1;

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated number length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this short Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(Num == short.MinValue ? Num + 1 : Num))) + (Num < 0 ? 2 /* include sign */ : 1);

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated nuembr length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this ushort Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Num)) + 1;

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated number length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this int Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(Num == int.MinValue ? Num + 1 : Num))) + (Num < 0 ? 2 /* include sign */ : 1);

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated nuembr length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this uint Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Num)) + 1;

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated number length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this long Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(Num == long.MinValue ? Num + 1 : Num))) + (Num < 0 ? 2 /* include sign */ : 1);

        /// <summary>
        /// Calculates the length of string representation of given number in base 10 (including sign, if present).
        /// </summary>
        /// <param name="Num">Number to calculate the length of.</param>
        /// <returns>Calculated nuembr length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateLength(this ulong Num)
            => Num == 0 ? 1 : (int)Math.Floor(Math.Log10(Num)) + 1;

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this sbyte Num, sbyte Min, sbyte Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this byte Num, byte Min, byte Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this short Num, short Min, short Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this ushort Num, ushort Min, ushort Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this int Num, int Min, int Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this uint Num, uint Min, uint Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this long Num, long Min, long Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this ulong Num, ulong Min, ulong Max, bool Inclusive = true)
        {
            if (Min > Max)
            {
                Min ^= Max;
                Max ^= Min;
                Min ^= Max;
            }

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this float Num, float Min, float Max, bool Inclusive = true)
        {
            if (Min > Max)
                return false;

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Tests wheter given value is in supplied range, optionally allowing it to be an exclusive check.
        /// </summary>
        /// <param name="Num">Number to test.</param>
        /// <param name="Min">Lower bound of the range.</param>
        /// <param name="Max">Upper bound of the range.</param>
        /// <param name="Inclusive">Whether the check is to be inclusive.</param>
        /// <returns>Whether the value is in range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this double Num, double Min, double Max, bool Inclusive = true)
        {
            if (Min > Max)
                return false;

            return Inclusive ? (Num >= Min && Num <= Max) : (Num > Min && Num < Max);
        }

        /// <summary>
        /// Returns whether supplied character is in any of the following ranges: a-z, A-Z, 0-9.
        /// </summary>
        /// <param name="C">Character to test.</param>
        /// <returns>Whether the character is in basic alphanumeric character range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBasicAlphanumeric(this char C)
            => (C >= 'a' && C <= 'z') || (C >= 'A' && C <= 'Z') || (C >= '0' && C <= '9');

        /// <summary>
        /// Returns whether supplied character is in the 0-9 range.
        /// </summary>
        /// <param name="C">Character to test.</param>
        /// <returns>Whether the character is in basic numeric digit character range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBasicDigit(this char C)
            => C >= '0' && C <= '9';

        /// <summary>
        /// Returns whether supplied character is in the a-z or A-Z range.
        /// </summary>
        /// <param name="C">Character to test.</param>
        /// <returns>Whether the character is in basic letter character range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBasicLetter(this char C)
            => (C >= 'a' && C <= 'z') || (C >= 'A' && C <= 'Z');

        /// <summary>
        /// Tests whether given string ends with given character.
        /// </summary>
        /// <param name="S">String to test.</param>
        /// <param name="C">Character to test for.</param>
        /// <returns>Whether the supplied string ends with supplied character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithCharacter(this string S, char C)
            => S.Length >= 1 && S[^1] == C;

        /// <summary>
        /// Tests whether given string starts with given character.
        /// </summary>
        /// <param name="S">String to test.</param>
        /// <param name="C">Character to test for.</param>
        /// <returns>Whether the supplied string starts with supplied character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithCharacter(this string S, char C)
            => S.Length >= 1 && S[0] == C;

        // https://stackoverflow.com/questions/9545619/a-fast-hash-function-for-string-in-c-sharp
        // Calls are inlined to call the underlying method directly
        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this ReadOnlySpan<char> Chars)
            => Knuth(Chars);

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this Span<char> Chars)
            => Knuth(Chars);

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this ReadOnlyMemory<char> Chars)
            => Knuth(Chars.Span);

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this Memory<char> Chars)
            => Knuth(Chars.Span);

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this ArraySegment<char> Chars)
            => Knuth(Chars.AsSpan());

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this char[] Chars)
            => Knuth(Chars.AsSpan());

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <param name="Start">Offset in the array to start calculating from.</param>
        /// <param name="Count">Number of characters to compute the hash from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this char[] Chars, int Start, int Count)
            => Knuth(Chars.AsSpan(Start, Count));

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this string Chars)
            => Knuth(Chars.AsSpan());

        /// <summary>
        /// Computes a 64-bit Knuth hash from supplied characters.
        /// </summary>
        /// <param name="Chars">Characters to compute the hash value from.</param>
        /// <param name="Start">Offset in the array to start calculating from.</param>
        /// <param name="Count">Number of characters to compute the hash from.</param>
        /// <returns>Computer 64-bit Knuth hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CalculateKnuthHash(this string Chars, int Start, int Count)
            => Knuth(Chars.AsSpan(Start, Count));

        /// <summary>
        /// Firsts the two or default.
        /// </summary>
        /// <param name="Enumerable">The enumerable.</param>
        /// <returns>A (T first, T second) .</returns>
        internal static (T first, T second) FirstTwoOrDefault<T>(this IEnumerable<T> Enumerable)
        {
            using var enumerator = Enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
                return (default, default);

            var first = enumerator.Current;

            if (!enumerator.MoveNext())
                return (first, default);

            return (first, enumerator.Current);
        }

        /// <summary>
        /// Knuths the.
        /// </summary>
        /// <param name="Chars">The chars.</param>
        /// <returns>An ulong.</returns>
        private static ulong Knuth(ReadOnlySpan<char> Chars)
        {
            var hash = 3074457345618258791ul;
            for (var i = 0; i < Chars.Length; i++)
                hash = (hash + Chars[i]) * 3074457345618258799ul;
            return hash;
        }
    }
}
