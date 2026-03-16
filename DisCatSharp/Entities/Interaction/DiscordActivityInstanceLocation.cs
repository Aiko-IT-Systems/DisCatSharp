using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the location of an activity instance.
/// </summary>
public sealed class DiscordActivityInstanceLocation : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordActivityInstanceLocation" /> class.
	/// </summary>
	internal DiscordActivityInstanceLocation()
	{ }

	/// <summary>
	///     Gets the raw location identifier.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string? Id { get; internal set; }

	/// <summary>
	///     Gets the location kind.
	/// </summary>
	[JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
	public string? Kind { get; internal set; }

	/// <summary>
	///     Gets the channel id for the activity location, if available.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	/// <summary>
	///     Gets the guild id for the activity location, if available.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }
}
