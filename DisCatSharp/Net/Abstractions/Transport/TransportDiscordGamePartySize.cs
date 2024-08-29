using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents information about party size.
/// </summary>
[JsonConverter(typeof(TransportDiscordGamePartySizeConverter))]
public class TransportDiscordGamePartySize
{
	/// <summary>
	///     Gets the current number of players in the party.
	/// </summary>
	public long Current { get; internal set; }

	/// <summary>
	///     Gets the maximum party size.
	/// </summary>
	public long Maximum { get; internal set; }
}
