
using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink voice state with credentials to connect to discord voice servers.
/// </summary>
internal sealed class LavalinkVoiceState
{
	/// <summary>
	/// The token for the voice connection.
	/// </summary>
	[JsonProperty("token")]
	internal string Token { get; set; }

	/// <summary>
	/// The voice server to connect to.
	/// </summary>
	[JsonProperty("endpoint")]
	internal string Endpoint { get; set; }

	/// <summary>
	/// The session id for the voice connection.
	/// </summary>
	[JsonProperty("sessionId")]
	internal string SessionId { get; set; }
}
