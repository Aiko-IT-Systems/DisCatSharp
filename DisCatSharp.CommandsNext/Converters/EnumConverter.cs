using System;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents a enum converter.
/// </summary>
public class EnumConverter<T> : IArgumentConverter<T> where T : struct, IComparable, IConvertible, IFormattable
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	Task<Optional<T>> IArgumentConverter<T>.ConvertAsync(string value, CommandContext ctx)
	{
		var t = typeof(T);
		var ti = t.GetTypeInfo();
		return !ti.IsEnum
			? throw new InvalidOperationException("Cannot convert non-enum value to an enum.")
			: Enum.TryParse(value, !ctx.Config.CaseSensitive, out T ev)
				? Task.FromResult(Optional.Some(ev))
				: Task.FromResult(Optional<T>.None);
	}
}
