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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents a date time converter.
/// </summary>
public class DateTimeConverter : IArgumentConverter<DateTime>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<DateTime>> IArgumentConverter<DateTime>.ConvertAsync(string value, CommandContext ctx) =>
		DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<DateTime>.None);
}

/// <summary>
/// Represents a date time offset converter.
/// </summary>
public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<DateTimeOffset>> IArgumentConverter<DateTimeOffset>.ConvertAsync(string value, CommandContext ctx) =>
		DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
			? Task.FromResult(Optional.Some(result))
			: Task.FromResult(Optional<DateTimeOffset>.None);
}

/// <summary>
/// Represents a time span converter.
/// </summary>
public class TimeSpanConverter : IArgumentConverter<TimeSpan>
{
	/// <summary>
	/// Gets or sets the time span regex.
	/// </summary>
	private static Regex s_timeSpanRegex { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TimeSpanConverter"/> class.
	/// </summary>
	static TimeSpanConverter()
	{
		s_timeSpanRegex = CommonRegEx.TimeSpan;
	}

	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<TimeSpan>> IArgumentConverter<TimeSpan>.ConvertAsync(string value, CommandContext ctx)
	{
		if (value == "0")
			return Task.FromResult(Optional.Some(TimeSpan.Zero));

		if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
			return Task.FromResult(Optional<TimeSpan>.None);

		if (!ctx.Config.CaseSensitive)
			value = value.ToLowerInvariant();

		if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
			return Task.FromResult(Optional.Some(result));

		var gps = new string[] { "days", "hours", "minutes", "seconds" };
		var mtc = s_timeSpanRegex.Match(value);
		if (!mtc.Success)
			return Task.FromResult(Optional<TimeSpan>.None);

		var d = 0;
		var h = 0;
		var m = 0;
		var s = 0;
		foreach (var gp in gps)
		{
			var gpc = mtc.Groups[gp].Value;
			if (string.IsNullOrWhiteSpace(gpc))
				continue;

			var gpt = gpc[^1];
			int.TryParse(gpc[0..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var val);
			switch (gpt)
			{
				case 'd':
					d = val;
					break;

				case 'h':
					h = val;
					break;

				case 'm':
					m = val;
					break;

				case 's':
					s = val;
					break;
			}
		}
		result = new TimeSpan(d, h, m, s);
		return Task.FromResult(Optional.Some(result));
	}
}
