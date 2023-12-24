using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// <para>Specifies that this enum should be serialized and deserialized as its underlying numeric type.</para>
/// <para>This is used to change the behaviour of enum serialization.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NumericEnumAttribute : SerializationAttribute;

/// <summary>
/// <para>Specifies that this enum should be serialized and deserialized as its string representation.</para>
/// <para>This is used to change the behaviour of enum serialization.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class StringEnumAttribute : SerializationAttribute;
