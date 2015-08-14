using DisCatSharp.Lavalink.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink exception.
/// </summary>
public sealed class LavalinkException
{
	/// <summary>
	/// Gets message of the exception.
	/// </summary>
	[JsonProperty("message")]
	public string? Message { get; internal set; }

	/// <summary>
	/// Gets the severity of the exception
	/// </summary>
	[JsonProperty("severity")]
	public Severity Severity { get; internal set; }

	/// <summary>
	/// Gets the cause of the exception.
	/// </summary>
	[JsonProperty("cause")]
	public string Cause { get; internal set; }
}
