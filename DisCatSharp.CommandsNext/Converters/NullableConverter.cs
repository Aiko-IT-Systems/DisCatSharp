using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents a nullable converter.
/// </summary>
public class NullableConverter<T> : IArgumentConverter<T?> where T : struct
{
	/// <summary>
	/// Converts a string.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="ctx">The command context.</param>
	async Task<Optional<T?>> IArgumentConverter<T?>.ConvertAsync(string value, CommandContext ctx)
	{
		if (!ctx.Config.CaseSensitive)
			value = value.ToLowerInvariant();

		if (value == "null")
			return null;

		if (ctx.CommandsNext.ArgumentConverters.TryGetValue(typeof(T), out var cv))
		{
			var cvx = cv as IArgumentConverter<T>;
			var val = await cvx.ConvertAsync(value, ctx).ConfigureAwait(false);
			return val.Map<T?>(x => x);
		}

		return Optional.None;
	}
}
