using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// Specifies a decomposer for a given type or property.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DecomposerAttribute : SerializationAttribute
{
	/// <summary>
	/// Gets the type of the decomposer.
	/// </summary>
	public Type DecomposerType { get; }

	/// <summary>
	/// Specifies a decomposer for given type or property.
	/// </summary>
	/// <param name="type">Type of decomposer to use.</param>
	public DecomposerAttribute(Type type)
	{
		if (!typeof(IDecomposer).IsAssignableFrom(type) || !type.IsClass || type.IsAbstract) // abstract covers static - static = abstract + sealed
			throw new ArgumentException("Invalid type specified. Must be a non-abstract class which implements DisCatSharp.Common.Serialization.IDecomposer interface.", nameof(type));

		this.DecomposerType = type;
	}
}
