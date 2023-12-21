using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// The voice state update payload.
/// </summary>
internal sealed class VoiceStateUpdatePayload
{
	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }

	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public ulong? ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; set; }

	/// <summary>
	/// Gets or sets the session id.
	/// </summary>
	[JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
	public string SessionId { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether deafened.
	/// </summary>
	[JsonProperty("self_deaf")]
	public bool Deafened { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether muted.
	/// </summary>
	[JsonProperty("self_mute")]
	public bool Muted { get; set; }
}
