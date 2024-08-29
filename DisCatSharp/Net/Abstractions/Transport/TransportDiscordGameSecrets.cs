using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents information about secret values for the Join, Spectate, and Match actions.
/// </summary>
public class TransportDiscordGameSecrets
{
	/// <summary>
	///     Gets the secret value for join action.
	/// </summary>
	[JsonProperty("join", NullValueHandling = NullValueHandling.Ignore)]
	public string? Join { get; internal set; }

	/// <summary>
	///     Gets the secret value for match action.
	/// </summary>
	[JsonProperty("match", NullValueHandling = NullValueHandling.Ignore)]
	public string? Match { get; internal set; }

	/// <summary>
	///     Gets the secret value for spectate action.
	/// </summary>
	[JsonProperty("spectate", NullValueHandling = NullValueHandling.Ignore)]
	public string? Spectate { get; internal set; }
}
