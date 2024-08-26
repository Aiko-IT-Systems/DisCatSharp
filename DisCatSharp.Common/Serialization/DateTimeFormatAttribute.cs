using System;

// ReSharper disable InconsistentNaming

namespace DisCatSharp.Common.Serialization;

/// <summary>
///     Defines the format for string-serialized <see cref="DateTime" /> and <see cref="DateTimeOffset" /> objects.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DateTimeFormatAttribute : SerializationAttribute
{
	/// <summary>
	///     Gets the ISO 8601 format string of "yyyy-MM-ddTHH:mm:ss.fffzzz".
	/// </summary>
	public const string FORMAT_ISO_8601 = "yyyy-MM-ddTHH:mm:ss.fffzzz";

	/// <summary>
	///     Gets the RFC 1123 format string of "R".
	/// </summary>
	public const string FORMAT_RFC_1123 = "R";

	/// <summary>
	///     Gets the general long format.
	/// </summary>
	public const string FORMAT_LONG = "G";

	/// <summary>
	///     Gets the general short format.
	/// </summary>
	public const string FORMAT_SHORT = "g";

	/// <summary>
	///     Specifies a predefined format to use.
	/// </summary>
	/// <param name="kind">Predefined format kind to use.</param>
	public DateTimeFormatAttribute(DateTimeFormatKind kind)
	{
		if (kind is < 0 or > DateTimeFormatKind.InvariantLocaleShort)
			throw new ArgumentOutOfRangeException(nameof(kind), "Specified format kind is not legal or supported.");

		this.Kind = kind;
		this.Format = null;
	}

	/// <summary>
	///     <para>Specifies a custom format to use.</para>
	///     <para>
	///         See https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings for more
	///         details.
	///     </para>
	/// </summary>
	/// <param name="format">Custom format string to use.</param>
	public DateTimeFormatAttribute(string format)
	{
		if (string.IsNullOrWhiteSpace(format))
			throw new ArgumentNullException(nameof(format), "Specified format cannot be null or empty.");

		this.Kind = DateTimeFormatKind.Custom;
		this.Format = format;
	}

	/// <summary>
	///     Gets the custom datetime format string to use.
	/// </summary>
	public string? Format { get; }

	/// <summary>
	///     Gets the predefined datetime format kind.
	/// </summary>
	public DateTimeFormatKind Kind { get; }
}

/// <summary>
///     <para>
///         Defines which built-in format to use for for <see cref="DateTime" /> and <see cref="System.DateTimeOffset" />
///         serialization.
///     </para>
///     <para>
///         See https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings and
///         https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings for more
///         details.
///     </para>
/// </summary>
public enum DateTimeFormatKind
{
	/// <summary>
	///     Specifies ISO 8601 format, which is equivalent to .NET format string of "yyyy-MM-ddTHH:mm:ss.fffzzz".
	/// </summary>
	ISO8601 = 0,

	/// <summary>
	///     Specifies RFC 1123 format, which is equivalent to .NET format string of "R".
	/// </summary>
	RFC1123 = 1,

	/// <summary>
	///     Specifies a format defined by <see cref="System.Globalization.CultureInfo.CurrentCulture" />, with a format string
	///     of "G". This format is not recommended for portability reasons.
	/// </summary>
	CurrentLocaleLong = 2,

	/// <summary>
	///     Specifies a format defined by <see cref="System.Globalization.CultureInfo.CurrentCulture" />, with a format string
	///     of "g". This format is not recommended for portability reasons.
	/// </summary>
	CurrentLocaleShort = 3,

	/// <summary>
	///     Specifies a format defined by <see cref="System.Globalization.CultureInfo.InvariantCulture" />, with a format
	///     string of "G".
	/// </summary>
	InvariantLocaleLong = 4,

	/// <summary>
	///     Specifies a format defined by <see cref="System.Globalization.CultureInfo.InvariantCulture" />, with a format
	///     string of "g".
	/// </summary>
	InvariantLocaleShort = 5,

	/// <summary>
	///     Specifies a custom format. This value is not usable directly.
	/// </summary>
	Custom = int.MaxValue
}
