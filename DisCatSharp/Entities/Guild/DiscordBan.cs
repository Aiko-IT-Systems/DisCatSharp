using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord ban
/// </summary>
public class DiscordBan : ObservableApiObject
{
	/// <summary>
	/// Gets the reason for the ban
	/// </summary>
	[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
	public string Reason { get; internal set; }

	/// <summary>
	/// Gets the banned user
	/// </summary>
	[JsonIgnore]
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the raw user.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	internal TransportUser RawUser { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordBan"/> class.
	/// </summary>
	internal DiscordBan()
	{ }
}
