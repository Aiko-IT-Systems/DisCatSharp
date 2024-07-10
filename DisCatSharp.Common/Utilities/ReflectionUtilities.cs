using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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
		=> RuntimeHelpers.GetUninitializedObject(t);

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
		=> (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

	/// <summary>
	/// Converts a given object into a dictionary of property name to property value mappings.
	/// </summary>
	/// <typeparam name="T">Type of object to convert.</typeparam>
	/// <param name="obj">Object to convert.</param>
	/// <returns>Converted dictionary.</returns>
	public static IReadOnlyDictionary<string, object?> ToDictionary<T>(this T obj)
	{
		return obj == null
			? throw new NullReferenceException()
			: (IReadOnlyDictionary<string, object?>)new CharSpanLookupReadOnlyDictionary<object?>(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Select(x => new KeyValuePair<string, object?>(x.Name, x.GetValue(obj))));
	}
}
