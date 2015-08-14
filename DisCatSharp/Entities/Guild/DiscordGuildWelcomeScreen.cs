using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord welcome screen object.
/// </summary>
public class DiscordGuildWelcomeScreen : ObservableApiObject
{
	/// <summary>
	/// Gets the server description shown in the welcome screen.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// Gets the channels shown in the welcome screen.
	/// </summary>
	[JsonProperty("welcome_channels", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordGuildWelcomeScreenChannel> WelcomeChannels { get; internal set; }
}
