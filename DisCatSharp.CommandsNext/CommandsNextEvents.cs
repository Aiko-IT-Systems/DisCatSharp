using Microsoft.Extensions.Logging;

namespace DisCatSharp.CommandsNext;

/// <summary>
/// Contains well-defined event IDs used by CommandsNext.
/// </summary>
public static class CommandsNextEvents
{
	/// <summary>
	/// Miscellaneous events, that do not fit in any other category.
	/// </summary>
	internal static EventId Misc { get; } = new(200, "CommandsNext");

	/// <summary>
	/// Events pertaining to Gateway Intents. Typically diagnostic information.
	/// </summary>
	internal static EventId Intents { get; } = new(201, nameof(Intents));
}
