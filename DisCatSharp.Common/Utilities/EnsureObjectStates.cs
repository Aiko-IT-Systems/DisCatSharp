using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	public static bool EmptyOrNull<T1, T2>([NotNullWhen(false)] this Dictionary<T1, T2?>? dictionary) where T1 : notnull
		=> dictionary is null || dictionary.Count is 0 || dictionary.Keys.Count is 0;

	/// <summary>
	/// Checks whether the dictionary is not null and not empty.
	/// </summary>
	/// <typeparam name="T1">Any key type.</typeparam>
	/// <typeparam name="T2">Any value type.</typeparam>
	/// <param name="dictionary">The dictionary to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool NotEmptyAndNotNull<T1, T2>([NotNullWhen(true)] this Dictionary<T1, T2?>? dictionary) where T1 : notnull
		=> dictionary is not null && dictionary.Count is not 0 && dictionary.Keys.Count is not 0;

	/// <summary>
	/// Checks whether the list is null or empty.
	/// </summary>
	/// <typeparam name="T">Any value type.</typeparam>
	/// <param name="list">The list to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool EmptyOrNull<T>([NotNullWhen(false)] this List<T?>? list)
		=> list is null || list.Count is 0;

	/// <summary>
	/// Checks whether the list is not null and not empty.
	/// </summary>
	/// <typeparam name="T">Any value type.</typeparam>
	/// <param name="list">The list to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool NotEmptyAndNotNull<T>([NotNullWhen(true)] this List<T?>? list)
		=> list is not null && list.Count is not 0;

	/// <summary>
	/// Checks whether the dictionary is null or empty.
	/// </summary>
	/// <typeparam name="T1">Any key type.</typeparam>
	/// <typeparam name="T2">Any value type.</typeparam>
	/// <param name="dictionary">The dictionary to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool Empty<T1, T2>(this Dictionary<T1, T2?> dictionary) where T1 : notnull
		=> dictionary.Count is 0 || dictionary.Keys.Count is 0;

	/// <summary>
	/// Checks whether the dictionary is not null and not empty.
	/// </summary>
	/// <typeparam name="T1">Any key type.</typeparam>
	/// <typeparam name="T2">Any value type.</typeparam>
	/// <param name="dictionary">The dictionary to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool NotEmpty<T1, T2>(this Dictionary<T1, T2?> dictionary) where T1 : notnull
		=> dictionary.Count is not 0 && dictionary.Keys.Count is not 0;

	/// <summary>
	/// Checks whether the list is null or empty.
	/// </summary>
	/// <typeparam name="T">Any value type.</typeparam>
	/// <param name="list">The list to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool Empty<T>(this List<T> list) where T : notnull
		=> list.Count is 0;

	/// <summary>
	/// Checks whether the list is not null and not empty.
	/// </summary>
	/// <typeparam name="T">Any value type.</typeparam>
	/// <param name="list">The list to check on.</param>
	/// <returns>True if satisfied, false otherwise.</returns>
	public static bool NotEmpty<T>(this List<T> list) where T : notnull
		=> list.Count is not 0;
}
