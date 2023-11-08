using DisCatSharp.Lavalink.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a lavalink exception received via websocket.
/// </summary>
internal sealed class TrackException
{
	/// <summary>
	/// Gets message of the exception.
	/// </summary>
	[JsonProperty("message")]
	internal string? Message { get; set; }

	/// <summary>
	/// Gets the severity of the exception
	/// </summary>
	[JsonProperty("severity")]
	internal Severity Severity { get; set; }

	/// <summary>
	/// Gets the cause of the exception.
	/// </summary>
	[JsonProperty("cause")]
	internal string Cause { get; set; }
}
