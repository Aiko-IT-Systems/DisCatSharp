using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity.Enums;

/// <summary>
/// A response from the paginated modal response
/// </summary>
public class PaginatedModalResponse
{
	/// <summary>
	/// The responses. The key is the customid of each component.
	/// </summary>
	public IReadOnlyDictionary<string, string> Responses { get; internal set; }

	/// <summary>
	/// The last interaction. This is automatically replied to with a ephemeral "thinking" state. Use EditOriginalResponseAsync to modify this.
	/// </summary>
	public DiscordInteraction Interaction { get; internal set; }

	/// <summary>
	/// Whether the interaction timed out.
	/// </summary>
	public bool TimedOut { get; internal set; }
}
