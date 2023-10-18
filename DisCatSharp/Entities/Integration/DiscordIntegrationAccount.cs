using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord integration account.
/// </summary>
public class DiscordIntegrationAccount : ObservableApiObject
{
	/// <summary>
	/// Gets the ID of the account.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; internal set; }

	/// <summary>
	/// Gets the name of the account.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordIntegrationAccount"/> class.
	/// </summary>
	internal DiscordIntegrationAccount()
	{ }
}
