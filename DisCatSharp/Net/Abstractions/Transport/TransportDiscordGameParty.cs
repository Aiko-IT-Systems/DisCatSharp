using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents information about rich presence game party.
/// </summary>
public class TransportDiscordGameParty
{
	/// <summary>
	///     Gets the game party ID.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string? Id { get; internal set; }

	/// <summary>
	///     Gets the size of the party.
	/// </summary>
	[JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGamePartySize? Size { get; internal set; }
}
