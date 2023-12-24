using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// Provides an interface to decompose an object into another object or combination of objects.
/// </summary>
public interface IDecomposer
{
	/// <summary>
	/// Checks whether the decomposer can decompose a specific type.
	/// </summary>
	/// <param name="t">Type to check.</param>
	/// <returns>Whether the decomposer can decompose a given type.</returns>
	bool CanDecompose(Type t);

	/// <summary>
	/// <para>Checks whether the decomposer can recompose a specific decomposed type.</para>
	/// <para>Note that while a type might be considered recomposable, other factors might prevent recomposing operation from being successful.</para>
	/// </summary>
	/// <param name="t">Decomposed type to check.</param>
	/// <returns>Whether the decomposer can decompose a given type.</returns>
	bool CanRecompose(Type t);

	/// <summary>
	/// Attempts to decompose a given object of specified source type. The operation produces the decomposed object and the type it got decomposed into.
	/// </summary>
	/// <param name="obj">Object to decompose.</param>
	/// <param name="tobj">Type to decompose.</param>
	/// <param name="decomposed">Decomposition result.</param>
	/// <param name="tdecomposed">Type of the result.</param>
	/// <returns>Whether the operation was successful.</returns>
	bool TryDecompose(object obj, Type tobj, out object? decomposed, out Type tdecomposed);

	/// <summary>
	/// Attempts to recompose given object of specified source type, into specified target type. The operation produces the recomposed object.
	/// </summary>
	/// <param name="obj">Object to recompose from.</param>
	/// <param name="tobj">Type of data to recompose.</param>
	/// <param name="trecomposed">Type to recompose into.</param>
	/// <param name="recomposed">Recomposition result.</param>
	/// <returns>Whether the operation was successful.</returns>
	bool TryRecompose(object obj, Type tobj, Type trecomposed, out object? recomposed);
}
