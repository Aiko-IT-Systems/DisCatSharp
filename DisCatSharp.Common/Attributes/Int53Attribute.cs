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

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// <para>Specifies that this 64-bit integer uses no more than 53 bits to represent its value.</para>
/// <para>This is used to indicate that large numbers are safe for direct serialization into formats which do support 64-bit integers natively (such as JSON).</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Int53Attribute : SerializationAttribute
{
	/// <summary>
	/// <para>Gets the maximum safe value representable as an integer by a IEEE754 64-bit binary floating point value.</para>
	/// <para>This value equals to 9007199254740991.</para>
	/// </summary>
	public const long MAX_VALUE = (1L << 53) - 1;

	/// <summary>
	/// <para>Gets the minimum safe value representable as an integer by a IEEE754 64-bit binary floating point value.</para>
	/// <para>This value equals to -9007199254740991.</para>
	/// </summary>
	public const long MIN_VALUE = -MAX_VALUE;
}
