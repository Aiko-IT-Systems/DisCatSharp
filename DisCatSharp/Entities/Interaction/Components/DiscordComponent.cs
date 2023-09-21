using DisCatSharp.Enums;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// A component to attach to a message.
/// </summary>
[JsonConverter(typeof(DiscordComponentJsonConverter))]
public class DiscordComponent : ObservableApiObject
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
	/// Initializes a new instance of the <see cref="DiscordComponent"/> class.
	/// </summary>
	internal DiscordComponent()
	{ }
}
