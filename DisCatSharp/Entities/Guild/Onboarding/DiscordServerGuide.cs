using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild's server guide.
/// </summary>
public sealed class DiscordServerGuide : ObservableApiObject
{
	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null!;

	/// <summary>
	/// Gets whether the server guide is enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Enabled { get; internal set; }

	/// <summary>
	/// Gets the welcome message.
	/// </summary>
	[JsonProperty("welcome_message", NullValueHandling = NullValueHandling.Ignore)]
	public WelcomeMessage WelcomeMessage { get; internal set; }

	/// <summary>
	/// Gets the new member actions.
	/// </summary>
	[JsonProperty("new_member_actions", NullValueHandling = NullValueHandling.Ignore)]
	public List<NewMemberAction> NewMemberActions { get; internal set; } = [];

	/// <summary>
	/// Gets the resource channels.
	/// </summary>
	[JsonProperty("resource_channels", NullValueHandling = NullValueHandling.Ignore)]
	public List<ResourceChannel> ResourceChannels { get; internal set; } = [];
}
