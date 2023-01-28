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
using System.Reflection;
using System.Runtime.Serialization;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Contains various utilities for use with .NET's reflection.
/// </summary>
public static class ReflectionUtilities
{
	/// <summary>
	/// <para>Creates an empty, uninitialized instance of specified type.</para>
	/// <para>This method will not call the constructor for the specified type. As such, the object might not be properly initialized.</para>
	/// </summary>
	/// <remarks>
	/// This method is intended for reflection use only.
	/// </remarks>
	/// <param name="t">Type of the object to instantiate.</param>
	/// <returns>Empty, uninitialized object of specified type.</returns>
	public static object CreateEmpty(this Type t)
		=> FormatterServices.GetUninitializedObject(t);

	/// <summary>
	/// <para>Creates an empty, uninitialized instance of type <typeparamref name="T"/>.</para>
	/// <para>This method will not call the constructor for type <typeparamref name="T"/>. As such, the object might not be properly initialized.</para>
	/// </summary>
	/// <remarks>
	/// This method is intended for reflection use only.
	/// </remarks>
	/// <typeparam name="T">Type of the object to instantiate.</typeparam>
	/// <returns>Empty, uninitialized object of specified type.</returns>
	public static T CreateEmpty<T>()
		=> (T)FormatterServices.GetUninitializedObject(typeof(T));

	/// <summary>
	/// Converts a given object into a dictionary of property name to property value mappings.
	/// </summary>
	/// <typeparam name="T">Type of object to convert.</typeparam>
	/// <param name="obj">Object to convert.</param>
	/// <returns>Converted dictionary.</returns>
	public static IReadOnlyDictionary<string, object> ToDictionary<T>(this T obj)
	{
		if (obj == null)
			throw new NullReferenceException();

		return new CharSpanLookupReadOnlyDictionary<object>(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(obj))));
	}
}
