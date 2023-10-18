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
