using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Rich Presence activity.
/// </summary>
public class DiscordMessageActivity : ObservableApiObject
{
	/// <summary>
	/// Gets the activity type.
	/// </summary>
	[JsonProperty("type")]
	public MessageActivityType Type { get; internal set; }

	/// <summary>
	/// Gets the party id of the activity.
	/// </summary>
	[JsonProperty("party_id")]
	public string PartyId { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMessageActivity"/> class.
	/// </summary>
	internal DiscordMessageActivity()
	{ }
}
