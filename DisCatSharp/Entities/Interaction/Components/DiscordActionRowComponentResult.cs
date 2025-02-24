using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a <see cref="DiscordActionRowComponentResult" /> resolved from a
///     <see cref="DisCatSharp.Enums.InteractionType.ModalSubmit" />.
/// </summary>
public sealed class DiscordActionRowComponentResult : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordActionRowComponentResult" /> class.
	/// </summary>
	internal DiscordActionRowComponentResult()
	{ }

	/// <summary>
	///     The type of component this represents.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ComponentType Type { get; internal set; }

	/// <summary>
	///     The components contained within the resolved action row.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordComponentResult> Components { get; internal set; }

	/// <summary>
	///     Gets the Id of the compenent. Auto populated by discord if not provided.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public uint Id { get; internal set; }
}
