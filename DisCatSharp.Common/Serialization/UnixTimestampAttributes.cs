using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// <para>Specifies that this <see cref="DateTime"/> or <see cref="DateTimeOffset"/> will be serialized as Unix timestamp seconds.</para>
/// <para>This value will always be serialized as a number.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UnixSecondsAttribute : SerializationAttribute;

/// <summary>
/// <para>Specifies that this <see cref="DateTime"/> or <see cref="DateTimeOffset"/> will be serialized as Unix timestamp milliseconds.</para>
/// <para>This value will always be serialized as a number.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UnixMillisecondsAttribute : SerializationAttribute;
