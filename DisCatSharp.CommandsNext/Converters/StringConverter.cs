using System;
using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents a string converter.
/// </summary>
public class StringConverter : IArgumentConverter<string>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<string>> IArgumentConverter<string>.ConvertAsync(string value, CommandContext ctx)
		=> Task.FromResult(Optional.Some(value));
}

/// <summary>
/// Represents a uri converter.
/// </summary>
public class UriConverter : IArgumentConverter<Uri>
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<Uri>> IArgumentConverter<Uri>.ConvertAsync(string value, CommandContext ctx)
	{
		try
		{
			if (value.StartsWith("<", StringComparison.Ordinal) && value.EndsWith(">", StringComparison.Ordinal))
				value = value[1..^1];

			return Task.FromResult(Optional.Some(new Uri(value)));
		}
		catch
		{
			return Task.FromResult(Optional<Uri>.None);
		}
	}
}
