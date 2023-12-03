using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an onboarding prompt option.
/// </summary>
public sealed class DiscordOnboardingPromptOption : SnowflakeObject
{
	/// <summary>
	/// Gets the option's title.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the option's description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	/// Gets the option's emoji.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmoji? Emoji { get; internal set; }

	/// <summary>
	/// Gets the option's role ids.
	/// </summary>
	[JsonProperty("role_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong>? RoleIds { get; internal set; }

	/// <summary>
	/// Gets the option's channel ids.
	/// </summary>
	[JsonProperty("channel_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong>? ChannelIds { get; internal set; }

	/// <summary>
	/// Constructs a new onboarding prompt option.
	/// <para>You need to either specify <paramref name="roleIds"/> and/or <paramref name="channelIds"/>.</para>
	/// </summary>
	/// <param name="title">The title.</param>
	/// <param name="description">The description. Defaults to <see langword="null"/>.</param>
	/// <param name="emoji">The emoji. Defaults to <see langword="null"/>.</param>
	/// <param name="roleIds">The role ids. Defaults to <see langword="null"/>.</param>
	/// <param name="channelIds">The channel ids. Defaults to <see langword="null"/>.</param>
	public DiscordOnboardingPromptOption(
		string title,
		string? description = null,
		DiscordEmoji? emoji = null,
		List<ulong>? roleIds = null,
		List<ulong>? channelIds = null
	)
	{
		this.Title = title;
		this.Description = description;
		this.Emoji = emoji;
		this.RoleIds = roleIds;
		this.ChannelIds = channelIds;
	}

	/// <summary>
	/// Constructs a new onboarding prompt option.
	/// </summary>
	internal DiscordOnboardingPromptOption()
	{ }
}
