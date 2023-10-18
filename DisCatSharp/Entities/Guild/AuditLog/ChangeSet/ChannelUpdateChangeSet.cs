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

	public DiscordChannel Channel => this.Discord.Guilds[this.GuildId].Channels[this.TargetId!.Value];

	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	public bool TypeChanged => this.TypeBefore is not null || this.TypeAfter is not null;
	public ChannelType? TypeBefore => (ChannelType?)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;
	public ChannelType? TypeAfter => (ChannelType?)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;

	public bool TopicChanged => this.TopicBefore is not null || this.TopicAfter is not null;
	public string? TopicBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.OldValue;
	public string? TopicAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.NewValue;

	public bool PositionChanged => this.PositionBefore is not null || this.PositionAfter is not null;
	public int? PositionBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.OldValue;
	public int? PositionAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.NewValue;

	public bool IsNfswChanged => this.IsNfswBefore is not null || this.IsNfswAfter is not null;
	public bool? IsNfswBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.OldValue;
	public bool? IsNfswAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.NewValue;

	public bool PerUserRateLimitChanged => this.PerUserRateLimitBefore is not null || this.PerUserRateLimitAfter is not null;
	public int? PerUserRateLimitBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.OldValue;
	public int? PerUserRateLimitAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.NewValue;

	public bool IconEmojiChanged => this.IconEmojiBefore is not null || this.IconEmojiAfter is not null;
	public DiscordEmoji? IconEmojiBefore => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.OldValue;
	public DiscordEmoji? IconEmojiAfter => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.NewValue;

	public bool DefaultAutoArchiveDurationChanged => this.DefaultAutoArchiveDurationBefore is not null || this.DefaultAutoArchiveDurationAfter is not null;
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDurationBefore => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.OldValue;
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDurationAfter => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.NewValue;

	public bool FlagsChanged => this.FlagsBefore is not null || this.FlagsAfter is not null;
	public ChannelFlags? FlagsBefore => (ChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;
	public ChannelFlags? FlagsAfter => (ChannelFlags?)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;

	public bool ParentIdChanged => this.ParentIdBefore is not null || this.ParentIdAfter is not null;
	public ulong? ParentIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.OldValue;
	public ulong? ParentIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.NewValue;
	public DiscordChannel? ParentBefore => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ParentIdBefore ?? 0ul, out var channel) ? channel : null;
	public DiscordChannel? ParentAfter => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ParentIdAfter ?? 0ul, out var channel) ? channel : null;

	#region Voice
	public bool BitrateChanged => this.BitrateBefore is not null || this.BitrateAfter is not null;
	public int? BitrateBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.OldValue;
	public int? BitrateAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.NewValue;

	public bool UserLimitChanged => this.UserLimitBefore is not null || this.UserLimitAfter is not null;
	public int? UserLimitBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.OldValue;
	public int? UserLimitAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.NewValue;

	public bool RtcRegionIdChanged => this.RtcRegionIdBefore is not null || this.RtcRegionIdAfter is not null;
	public string? RtcRegionIdBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.OldValue;
	public string? RtcRegionIdAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.NewValue;
	public DiscordVoiceRegion? RtcRegionBefore => this.Discord.VoiceRegions[this.RtcRegionIdBefore];
	public DiscordVoiceRegion? RtcRegionAfter => this.Discord.VoiceRegions[this.RtcRegionIdAfter];

	public bool VideoQualityModeChanged => this.VideoQualityModeBefore is not null || this.VideoQualityModeAfter is not null;
	public VideoQualityMode? VideoQualityModeBefore => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.OldValue;
	public VideoQualityMode? VideoQualityModeAfter => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.NewValue;
	#endregion

	#region Forum
	public bool AvailableTagsChanged => this.AvailableTagsBefore is not null || this.AvailableTagsAfter is not null;
	public ForumPostTag? AvailableTagsBefore => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.OldValue;
	public ForumPostTag? AvailableTagsAfter => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.NewValue;

	public bool DefaultReactionEmojiChanged => this.DefaultReactionEmojiBefore is not null || this.DefaultReactionEmojiAfter is not null;
	public ForumReactionEmoji? DefaultReactionEmojiBefore => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.OldValue;
	public ForumReactionEmoji? DefaultReactionEmojiAfter => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.NewValue;

	public bool DefaultSortOrderChanged => this.DefaultSortOrderBefore is not null || this.DefaultSortOrderAfter is not null;
	public ForumPostSortOrder? DefaultSortOrderBefore => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.OldValue;
	public ForumPostSortOrder? DefaultSortOrderAfter => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.NewValue;

	public bool DefaultLayoutChanged => this.DefaultLayoutBefore is not null || this.DefaultLayoutAfter is not null;
	public ForumLayout? DefaultLayoutBefore => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.OldValue;
	public ForumLayout? DefaultLayoutAfter => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.NewValue;

	public bool DefaultThreadPerUserRateLimitChanged => this.DefaultThreadPerUserRateLimitBefore is not null || this.DefaultThreadPerUserRateLimitAfter is not null;
	public int? DefaultThreadPerUserRateLimitBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.OldValue;
	public int? DefaultThreadPerUserRateLimitAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.NewValue;
	#endregion
}
