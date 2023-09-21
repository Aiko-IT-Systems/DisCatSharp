using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// Declares name of a property in serialized data. This is used for mapping serialized data to object properties and fields.
/// </summary>
[Obsolete("Use [DataMember] with set Name instead."),
 AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SerializedNameAttribute : SerializationAttribute
{
	/// <summary>
	/// Gets the serialized name of the field or property.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Declares name of a property in serialized data. This is used for mapping serialized data to object properties and fields.
	/// </summary>
	/// <param name="name">Name of the field or property in serialized data.</param>
	public SerializedNameAttribute(string name)
	{
		this.Name = name;
	}
}
