using System;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a channel flag extensions.
/// </summary>
public static class ChannelFlagExtensions
{
	/// <summary>
	/// Calculates whether these channeö flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasChannelFlag(this ChannelFlags baseFlags, ChannelFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
/// Represents a channel's flags.
/// </summary>
[Flags]
public enum ChannelFlags : long
{
	/// <summary>
	/// Indicates that this channel is removed from the guilds home feed / from highlights.
	/// Applicable for <see cref="ChannelType"/> Text, Forum and News.
	/// </summary>
	[DiscordDeprecated]
	RemovedFromHome = 1L << 0,

	[DiscordDeprecated]
	RemovedFromHighlights = RemovedFromHome,

	/// <summary>
	/// Indicates that this thread is pinned to the top of its parent forum channel.
	/// Forum channel thread only.
	/// </summary>
	Pinned = 1L << 1,

	/// <summary>
	/// Indicates that this channel is removed from the active now within the guilds home feed.
	/// Applicable for <see cref="ChannelType"/> Text, News, Thread, Forum, Stage and Voice.
	/// </summary>
	[DiscordDeprecated]
	RemovedFromActiveNow = 1L << 2,

	/// <summary>
	/// Indicates that the channel requires users to select at least one <see cref="ForumPostTag"/>.
	/// Only applicable for <see cref="ChannelType.Forum"/>.
	/// </summary>
	RequireTags = 1L << 4,

	/// <summary>
	/// Indicated that this channel is spam.
	/// </summary>
	IsSpam = 1L << 5,

	/// <summary>
	/// Indicated that this channel is a guild resource channel.
	/// </summary>
	[DiscordInExperiment]
	IsGuildResourceChannel = 1L << 7,

	/// <summary>
	/// Indicated that clyde has access to this thread.
	/// </summary>
	[DiscordInExperiment]
	ClydeAi = 1L << 8,

	/// <summary>
	/// Unknown what this flag does. Releates to clyde thread?
	/// </summary>
	[DiscordUnreleased("We do not have information about this.")]
	IsScheduledForDeletion = 1L << 9,

	/// <summary>
	/// Indicates that this channel is a media channel.
	/// </summary>
	[DiscordUnreleased]
	IsMediaChannel = 1L << 10,

	/// <summary>
	/// Indicates that ai powered summaries are disabled for this channel.
	/// </summary>
	[DiscordInExperiment]
	SummariesDisabled = 1L << 11,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	[DiscordInExperiment]
	ApplicationShelfConsent = 1L << 12,

	/// <summary>
	/// Indicates that this channel is part of a role subscription template preview.
	/// </summary>
	[DiscordInExperiment]
	IsRoleSubscriptionTemplatePreviewChannel = 1L << 13,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	[DiscordInExperiment]
	IsBroadcasting = 1L << 14,

	/// <summary>
	/// Hides the media download options for <see cref="ChannelType.GuildMedia"/> channels.
	/// </summary>
	[DiscordInExperiment]
	HideMediaDownloadOptions = 1L << 15
}
