using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// <para>Specifies that if the value of the field or property is null, it should be included in the serialized data.</para>
/// <para>This alters the default behaviour of ignoring nulls.</para>
/// </summary>
[Obsolete("Use [DataMember] with EmitDefaultValue = true."),
 AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class IncludeNullAttribute : SerializationAttribute
{
}
