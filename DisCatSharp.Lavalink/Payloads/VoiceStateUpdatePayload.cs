using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Payloads;

/// <summary>
/// The discord voice state update payload.
/// </summary>
internal sealed class VoiceStateUpdatePayload
{
	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	internal ulong GuildId { get; set; }

	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	internal ulong? ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? UserId { get; set; }

	/// <summary>
	/// Gets or sets the session id.
	/// </summary>
	[JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
	internal string SessionId { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether deafened.
	/// </summary>
	[JsonProperty("self_deaf")]
	internal bool Deafened { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether muted.
	/// </summary>
	[JsonProperty("self_mute")]
	internal bool Muted { get; set; }
}
