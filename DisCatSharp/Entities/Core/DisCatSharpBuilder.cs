using System.Collections.Generic;

namespace DisCatSharp.Entities.Core;

/// <summary>
///     Represents the common base for most builders.
/// </summary>
public class DisCatSharpBuilder
{
	/// <summary>
	///     The components.
	/// </summary>
	internal readonly List<DiscordComponent> ComponentsInternal = [];

	/// <summary>
	///     Components to send.
	/// </summary>
	public IReadOnlyList<DiscordComponent> Components => this.ComponentsInternal;
}
