using System;
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents a date time converter.
/// </summary>
public sealed class DateTimeConverter : IArgumentConverter<DateTime>
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
public sealed class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
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
public sealed class TimeSpanConverter : IArgumentConverter<TimeSpan>
{
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
		var mtc = CommonRegEx.TimeSpanRegex().Match(value);
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

		result = new(d, h, m, s);
		return Task.FromResult(Optional.Some(result));
	}
}
