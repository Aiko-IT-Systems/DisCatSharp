using System;

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// <para>Specifies that this 64-bit integer uses no more than 53 bits to represent its value.</para>
/// <para>This is used to indicate that large numbers are safe for direct serialization into formats which do support 64-bit integers natively (such as JSON).</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class Int53Attribute : SerializationAttribute
{
	/// <summary>
	/// <para>Gets the maximum safe value representable as an integer by a IEEE754 64-bit binary floating point value.</para>
	/// <para>This value equals to 9007199254740991.</para>
	/// </summary>
	public const long MAX_VALUE = (1L << 53) - 1;

	/// <summary>
	/// <para>Gets the minimum safe value representable as an integer by a IEEE754 64-bit binary floating point value.</para>
	/// <para>This value equals to -9007199254740991.</para>
	/// </summary>
	public const long MIN_VALUE = -MAX_VALUE;
}
