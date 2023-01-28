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
using System.Numerics;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// Decomposes <see cref="Complex"/> numbers into tuples (arrays of 2).
/// </summary>
public sealed class ComplexDecomposer : IDecomposer
{
	/// <summary>
	/// Gets the t complex.
	/// </summary>
	private static Type s_complex { get; } = typeof(Complex);
	/// <summary>
	/// Gets the t double array.
	/// </summary>
	private static Type s_doubleArray { get; } = typeof(double[]);
	/// <summary>
	/// Gets the t double enumerable.
	/// </summary>
	private static Type s_doubleEnumerable { get; } = typeof(IEnumerable<double>);
	/// <summary>
	/// Gets the t object array.
	/// </summary>
	private static Type s_objectArray { get; } = typeof(object[]);
	/// <summary>
	/// Gets the t object enumerable.
	/// </summary>
	private static Type s_objectEnumerable { get; } = typeof(IEnumerable<object>);

	/// <inheritdoc />
	public bool CanDecompose(Type t)
		=> t == s_complex;

	/// <inheritdoc />
	public bool CanRecompose(Type t)
		=> t == s_doubleArray
		|| t == s_objectArray
		|| s_doubleEnumerable.IsAssignableFrom(t)
		|| s_objectEnumerable.IsAssignableFrom(t);

	/// <inheritdoc />
	public bool TryDecompose(object obj, Type tobj, out object decomposed, out Type tdecomposed)
	{
		decomposed = null;
		tdecomposed = s_doubleArray;

		if (tobj != s_complex || obj is not Complex c)
			return false;

		decomposed = new[] { c.Real, c.Imaginary };
		return true;
	}

	/// <inheritdoc />
	public bool TryRecompose(object obj, Type tobj, Type trecomposed, out object recomposed)
	{
		recomposed = null;

		if (trecomposed != s_complex)
			return false;

		// ie<double>
		if (s_doubleEnumerable.IsAssignableFrom(tobj) && obj is IEnumerable<double> ied)
		{
			if (!ied.TryFirstTwo(out var values))
				return false;

			var (real, imag) = values;
			recomposed = new Complex(real, imag);
			return true;
		}

		// ie<obj>
		if (s_objectEnumerable.IsAssignableFrom(tobj) && obj is IEnumerable<object> ieo)
		{
			if (!ieo.TryFirstTwo(out var values))
				return false;

			var (real, imag) = values;
			if (real is not double dreal || imag is not double dimag)
				return false;

			recomposed = new Complex(dreal, dimag);
			return true;
		}

		return false;
	}
}
