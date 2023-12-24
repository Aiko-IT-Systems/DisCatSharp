using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// <para>Specifies that this <see cref="TimeSpan"/> will be serialized as a number of whole seconds.</para>
/// <para>This value will always be serialized as a number.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class TimeSpanSecondsAttribute : SerializationAttribute;

/// <summary>
/// <para>Specifies that this <see cref="TimeSpan"/> will be serialized as a number of whole milliseconds.</para>
/// <para>This value will always be serialized as a number.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class TimeSpanMillisecondsAttribute : SerializationAttribute;
