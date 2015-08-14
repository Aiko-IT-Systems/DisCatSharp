using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink voice state with credentials to connect to discord voice servers.
/// </summary>
public sealed class LavalinkVoiceState
{
	/// <summary>
	/// The token for the voice connection.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; set; }

	/// <summary>
	/// The voice server to connect to.
	/// </summary>
	[JsonProperty("endpoint")]
	public string Endpoint { get; set; }

	/// <summary>
	/// The session id for the voice connection.
	/// </summary>
	[JsonProperty("sessionId")]
	public string SessionId { get; set; }
}
