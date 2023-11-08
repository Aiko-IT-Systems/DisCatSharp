using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating channel settings.
/// </summary>
public sealed class ChannelUpdateChangeSet : DiscordAuditLogEntry
{
	public ChannelUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelUpdate;
	}

	/// <summary>
	/// Gets the Discord channel associated with this change.
	/// </summary>
	public DiscordChannel Channel => this.Discord.Guilds[this.GuildId].Channels[this.TargetId!.Value];

	/// <summary>
	/// Gets a value indicating whether the channel's name was changed.
	/// </summary>
	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;

	/// <summary>
	/// Gets the previous name of the channel.
	/// </summary>
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;

	/// <summary>
	/// Gets the new name of the channel.
	/// </summary>
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's type was changed.
	/// </summary>
	public bool TypeChanged => this.TypeBefore is not null || this.TypeAfter is not null;

	/// <summary>
	/// Gets the previous type of the channel.
	/// </summary>
	public ChannelType? TypeBefore => (ChannelType?)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;

	/// <summary>
	/// Gets the new type of the channel.
	/// </summary>
	public ChannelType? TypeAfter => (ChannelType?)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's topic was changed.
	/// </summary>
	public bool TopicChanged => this.TopicBefore is not null || this.TopicAfter is not null;

	/// <summary>
	/// Gets the previous topic of the channel.
	/// </summary>
	public string? TopicBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.OldValue;

	/// <summary>
	/// Gets the new topic of the channel.
	/// </summary>
	public string? TopicAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's position was changed.
	/// </summary>
	public bool PositionChanged => this.PositionBefore is not null || this.PositionAfter is not null;

	/// <summary>
	/// Gets the previous position of the channel.
	/// </summary>
	public int? PositionBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.OldValue;

	/// <summary>
	/// Gets the new position of the channel.
	/// </summary>
	public int? PositionAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's NSFW status was changed.
	/// </summary>
	public bool IsNfswChanged => this.IsNfswBefore is not null || this.IsNfswAfter is not null;

	/// <summary>
	/// Gets the previous NSFW status of the channel.
	/// </summary>
	public bool? IsNfswBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.OldValue;

	/// <summary>
	/// Gets the new NSFW status of the channel.
	/// </summary>
	public bool? IsNfswAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's rate limit per user was changed.
	/// </summary>
	public bool PerUserRateLimitChanged => this.PerUserRateLimitBefore is not null || this.PerUserRateLimitAfter is not null;

	/// <summary>
	/// Gets the previous rate limit per user of the channel.
	/// </summary>
	public int? PerUserRateLimitBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.OldValue;

	/// <summary>
	/// Gets the new rate limit per user of the channel.
	/// </summary>
	public int? PerUserRateLimitAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's icon emoji was changed.
	/// </summary>
	public bool IconEmojiChanged => this.IconEmojiBefore is not null || this.IconEmojiAfter is not null;

	/// <summary>
	/// Gets the previous icon emoji of the channel.
	/// </summary>
	public DiscordEmoji? IconEmojiBefore => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.OldValue;

	/// <summary>
	/// Gets the new icon emoji of the channel.
	/// </summary>
	public DiscordEmoji? IconEmojiAfter => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's default auto-archive duration was changed.
	/// </summary>
	public bool DefaultAutoArchiveDurationChanged => this.DefaultAutoArchiveDurationBefore is not null || this.DefaultAutoArchiveDurationAfter is not null;

	/// <summary>
	/// Gets the previous default auto-archive duration of the channel.
	/// </summary>
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDurationBefore => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.OldValue;

	/// <summary>
	/// Gets the new default auto-archive duration of the channel.
	/// </summary>
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDurationAfter => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's flags were changed.
	/// </summary>
	public bool FlagsChanged => this.FlagsBefore is not null || this.FlagsAfter is not null;

	/// <summary>
	/// Gets the previous flags of the channel.
	/// </summary>
	public ChannelFlags? FlagsBefore => (ChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;

	/// <summary>
	/// Gets the new flags of the channel.
	/// </summary>
	public ChannelFlags? FlagsAfter => (ChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's parent ID was changed.
	/// </summary>
	public bool ParentIdChanged => this.ParentIdBefore is not null || this.ParentIdAfter is not null;

	/// <summary>
	/// Gets the previous parent ID of the channel.
	/// </summary>
	public ulong? ParentIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.OldValue;

	/// <summary>
	/// Gets the new parent ID of the channel.
	/// </summary>
	public ulong? ParentIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.NewValue;

	/// <summary>
	/// Gets the previous parent channel of the channel.
	/// </summary>
	public DiscordChannel? ParentBefore => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ParentIdBefore ?? 0ul, out var channel) ? channel : null;

	/// <summary>
	/// Gets the new parent channel of the channel.
	/// </summary>
	public DiscordChannel? ParentAfter => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ParentIdAfter ?? 0ul, out var channel) ? channel : null;

#region Voice

	/// <summary>
	/// Gets a value indicating whether the channel's bitrate was changed.
	/// </summary>
	public bool BitrateChanged => this.BitrateBefore is not null || this.BitrateAfter is not null;

	/// <summary>
	/// Gets the previous bitrate of the channel.
	/// </summary>
	public int? BitrateBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.OldValue;

	/// <summary>
	/// Gets the new bitrate of the channel.
	/// </summary>
	public int? BitrateAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's user limit was changed.
	/// </summary>
	public bool UserLimitChanged => this.UserLimitBefore is not null || this.UserLimitAfter is not null;

	/// <summary>
	/// Gets the previous user limit of the channel.
	/// </summary>
	public int? UserLimitBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.OldValue;

	/// <summary>
	/// Gets the new user limit of the channel.
	/// </summary>
	public int? UserLimitAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel's RTC region ID was changed.
	/// </summary>
	public bool RtcRegionIdChanged => this.RtcRegionIdBefore is not null || this.RtcRegionIdAfter is not null;

	/// <summary>
	/// Gets the previous RTC region ID of the channel.
	/// </summary>
	public string? RtcRegionIdBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.OldValue;

	/// <summary>
	/// Gets the new RTC region ID of the channel.
	/// </summary>
	public string? RtcRegionIdAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.NewValue;

	/// <summary>
	/// Gets the previous RTC region of the channel.
	/// </summary>
	public DiscordVoiceRegion? RtcRegionBefore => this.Discord.VoiceRegions[this.RtcRegionIdBefore];

	/// <summary>
	/// Gets the new RTC region of the channel.
	/// </summary>
	public DiscordVoiceRegion? RtcRegionAfter => this.Discord.VoiceRegions[this.RtcRegionIdAfter];

	/// <summary>
	/// Gets a value indicating whether the channel's video quality mode was changed.
	/// </summary>
	public bool VideoQualityModeChanged => this.VideoQualityModeBefore is not null || this.VideoQualityModeAfter is not null;

	/// <summary>
	/// Gets the previous video quality mode of the channel.
	/// </summary>
	public VideoQualityMode? VideoQualityModeBefore => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.OldValue;

	/// <summary>
	/// Gets the new video quality mode of the channel.
	/// </summary>
	public VideoQualityMode? VideoQualityModeAfter => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.NewValue;

#endregion

#region Forum

	/// <summary>
	/// Gets a value indicating whether the available tags for forum posts were changed.
	/// </summary>
	public bool AvailableTagsChanged => this.AvailableTagsBefore is not null || this.AvailableTagsAfter is not null;

	/// <summary>
	/// Gets the previous available tags for forum posts.
	/// </summary>
	public ForumPostTag? AvailableTagsBefore => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.OldValue;

	/// <summary>
	/// Gets the new available tags for forum posts.
	/// </summary>
	public ForumPostTag? AvailableTagsAfter => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the default reaction emoji for forum posts was changed.
	/// </summary>
	public bool DefaultReactionEmojiChanged => this.DefaultReactionEmojiBefore is not null || this.DefaultReactionEmojiAfter is not null;

	/// <summary>
	/// Gets the previous default reaction emoji for forum posts.
	/// </summary>
	public ForumReactionEmoji? DefaultReactionEmojiBefore => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.OldValue;

	/// <summary>
	/// Gets the new default reaction emoji for forum posts.
	/// </summary>
	public ForumReactionEmoji? DefaultReactionEmojiAfter => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the default sort order for forum posts was changed.
	/// </summary>
	public bool DefaultSortOrderChanged => this.DefaultSortOrderBefore is not null || this.DefaultSortOrderAfter is not null;

	/// <summary>
	/// Gets the previous default sort order for forum posts.
	/// </summary>
	public ForumPostSortOrder? DefaultSortOrderBefore => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.OldValue;

	/// <summary>
	/// Gets the new default sort order for forum posts.
	/// </summary>
	public ForumPostSortOrder? DefaultSortOrderAfter => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the default layout for forum posts was changed.
	/// </summary>
	public bool DefaultLayoutChanged => this.DefaultLayoutBefore is not null || this.DefaultLayoutAfter is not null;

	/// <summary>
	/// Gets the previous default layout for forum posts.
	/// </summary>
	public ForumLayout? DefaultLayoutBefore => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.OldValue;

	/// <summary>
	/// Gets the new default layout for forum posts.
	/// </summary>
	public ForumLayout? DefaultLayoutAfter => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the default thread rate limit per user for forum posts was changed.
	/// </summary>
	public bool DefaultThreadPerUserRateLimitChanged => this.DefaultThreadPerUserRateLimitBefore is not null || this.DefaultThreadPerUserRateLimitAfter is not null;

	/// <summary>
	/// Gets the previous default thread rate limit per user for forum posts.
	/// </summary>
	public int? DefaultThreadPerUserRateLimitBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.OldValue;

	/// <summary>
	/// Gets the new default thread rate limit per user for forum posts.
	/// </summary>
	public int? DefaultThreadPerUserRateLimitAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.NewValue;

#endregion
}
