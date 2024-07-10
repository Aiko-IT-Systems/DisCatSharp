using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice identify payload.
/// </summary>
internal sealed class VoiceIdentifyPayload
{
	/// <summary>
	/// Gets or sets the server id.
	/// </summary>
	[JsonProperty("server_id")]
	public ulong ServerId { get; set; }

	/// <summary>
	/// Gets or sets the user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; set; }

	/// <summary>
	/// Gets or sets the session id.
	/// </summary>
	[JsonProperty("session_id")]
	public string SessionId { get; set; }

	/// <summary>
	/// Gets or sets the token.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; set; }
}
