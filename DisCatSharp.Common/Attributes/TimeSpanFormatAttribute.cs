// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
// ReSharper disable InconsistentNaming

namespace DisCatSharp.Common.Serialization;

/// <summary>
/// Defines the format for string-serialized <see cref="TimeSpan"/> objects.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class TimeSpanFormatAttribute : SerializationAttribute
{
	/// <summary>
	/// Gets the ISO 8601 format string of @"ddThh\:mm\:ss\.fff".
	/// </summary>
	public const string FORMAT_ISO_8601 = @"ddThh\:mm\:ss\.fff";

	/// <summary>
	/// Gets the constant format.
	/// </summary>
	public const string FORMAT_CONSTANT = "c";

	/// <summary>
	/// Gets the general long format.
	/// </summary>
	public const string FORMAT_LONG = "G";

	/// <summary>
	/// Gets the general short format.
	/// </summary>
	public const string FORMAT_SHORT = "g";

	/// <summary>
	/// Gets the custom datetime format string to use.
	/// </summary>
	public string Format { get; }

	/// <summary>
	/// Gets the predefined datetime format kind.
	/// </summary>
	public TimeSpanFormatKind Kind { get; }

	/// <summary>
	/// Specifies a predefined format to use.
	/// </summary>
	/// <param name="kind">Predefined format kind to use.</param>
	public TimeSpanFormatAttribute(TimeSpanFormatKind kind)
	{
		if (kind < 0 || kind > TimeSpanFormatKind.InvariantLocaleShort)
			throw new ArgumentOutOfRangeException(nameof(kind), "Specified format kind is not legal or supported.");

		this.Kind = kind;
		this.Format = null;
	}

	/// <summary>
	/// <para>Specifies a custom format to use.</para>
	/// <para>See https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings for more details.</para>
	/// </summary>
	/// <param name="format">Custom format string to use.</param>
	public TimeSpanFormatAttribute(string format)
	{
		if (string.IsNullOrWhiteSpace(format))
			throw new ArgumentNullException(nameof(format), "Specified format cannot be null or empty.");

		this.Kind = TimeSpanFormatKind.Custom;
		this.Format = format;
	}
}

/// <summary>
/// <para>Defines which built-in format to use for <see cref="TimeSpan"/> serialization.</para>
/// <para>See https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings and https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings for more details.</para>
/// </summary>
public enum TimeSpanFormatKind : int
{
	/// <summary>
	/// Specifies ISO 8601-like time format, which is equivalent to .NET format string of @"ddThh\:mm\:ss\.fff".
	/// </summary>
	ISO8601 = 0,

	/// <summary>
	/// Specifies a format defined by <see cref="System.Globalization.CultureInfo.InvariantCulture"/>, with a format string of "c".
	/// </summary>
	InvariantConstant = 1,

	/// <summary>
	/// Specifies a format defined by <see cref="System.Globalization.CultureInfo.CurrentCulture"/>, with a format string of "G". This format is not recommended for portability reasons.
	/// </summary>
	CurrentLocaleLong = 2,

	/// <summary>
	/// Specifies a format defined by <see cref="System.Globalization.CultureInfo.CurrentCulture"/>, with a format string of "g". This format is not recommended for portability reasons.
	/// </summary>
	CurrentLocaleShort = 3,

	/// <summary>
	/// Specifies a format defined by <see cref="System.Globalization.CultureInfo.InvariantCulture"/>, with a format string of "G". This format is not recommended for portability reasons.
	/// </summary>
	InvariantLocaleLong = 4,

	/// <summary>
	/// Specifies a format defined by <see cref="System.Globalization.CultureInfo.InvariantCulture"/>, with a format string of "g". This format is not recommended for portability reasons.
	/// </summary>
	InvariantLocaleShort = 5,

	/// <summary>
	/// Specifies a custom format. This value is not usable directly.
	/// </summary>
	Custom = int.MaxValue
}
