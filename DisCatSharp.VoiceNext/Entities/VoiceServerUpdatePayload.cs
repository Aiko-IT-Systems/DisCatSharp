using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice server update payload.
/// </summary>
internal sealed class VoiceServerUpdatePayload
{
	/// <summary>
	/// Gets or sets the token.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; set; }

	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }

	/// <summary>
	/// Gets or sets the endpoint.
	/// </summary>
	[JsonProperty("endpoint")]
	public string Endpoint { get; set; }
}
