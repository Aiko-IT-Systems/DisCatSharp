using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild's onboarding configuration.
/// </summary>
public sealed class DiscordOnboarding : ObservableApiObject
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
	/// Gets the onboarding prompts
	/// </summary>
	[JsonProperty("prompts", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordOnboardingPrompt> Prompts { get; internal set; } = [];

	/// <summary>
	/// Gets the default channel ids.
	/// </summary>
	[JsonProperty("default_channel_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> DefaultChannelIds { get; internal set; } = [];

	/// <summary>
	/// Gets whether onboarding is enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Enabled { get; internal set; }

	/// <summary>
	/// Gets the onboarding mode.
	/// </summary>
	[JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
	public OnboardingMode Mode { get; internal set; }

	/// <summary>
	/// Gets whether the current configuration is below discords requirements.
	/// </summary>
	[JsonProperty("below_requirements", NullValueHandling = NullValueHandling.Ignore)]
	public bool BelowRequirements { get; internal set; }

	/// <summary>
	/// Constructs a new onboarding configuration.
	/// </summary>
	internal DiscordOnboarding()
		: base(["responses", "onboarding_prompts_seen", "onboarding_responses_seen"])
	{ }
}
