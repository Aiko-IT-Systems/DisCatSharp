using DisCatSharp.Lavalink.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Payloads;

/// <summary>
/// The lavalink rest voice state update payload.
/// </summary>
internal sealed class LavalinkRestVoiceStateUpdatePayload
{
	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guildId")]
	internal string GuildId { get; set; }

	/// <summary>
	/// Gets or sets the voice state.
	/// </summary>
	[JsonProperty("voice")]
	internal LavalinkVoiceState VoiceState { get; set; }

	/// <summary>
	/// Constructs a new <see cref="LavalinkRestVoiceStateUpdatePayload"/>.
	/// </summary>
	/// <param name="voiceState">The voice state.</param>
	/// <param name="guildId">The guild id.</param>
	internal LavalinkRestVoiceStateUpdatePayload(LavalinkVoiceState voiceState, string guildId)
	{
		this.VoiceState = voiceState;
		this.GuildId = guildId;
	}
}
