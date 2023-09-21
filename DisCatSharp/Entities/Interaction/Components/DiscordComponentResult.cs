using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a <see cref="DiscordComponentResult"/> resolved within an <see cref="DiscordActionRowComponentResult"/>.
/// </summary>
public sealed class DiscordComponentResult : ObservableApiObject
{
	/// <summary>
	/// The type of component this represents.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ComponentType Type { get; internal set; }

	/// <summary>
	/// The Id of this component, if applicable. Not applicable on ActionRow(s) and link buttons.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string CustomId { get; internal set; }

	/// <summary>
	/// The users typed value.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value { get; internal set; }

	/// <summary>
	/// The selected values. Only applicable to Selects.
	/// </summary>
	[JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string> Values { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordComponentResult"/> class.
	/// </summary>
	internal DiscordComponentResult()
	{ }
}
