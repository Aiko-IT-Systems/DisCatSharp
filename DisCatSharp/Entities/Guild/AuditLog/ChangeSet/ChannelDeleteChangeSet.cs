using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a channel deletion.
/// </summary>
public sealed class ChannelDeleteChangeSet : DiscordAuditLogEntry
{
	internal ChannelDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelDelete;
	}

	public string Name => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public ChannelType Type => (ChannelType)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;
	public string? Topic => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.OldValue;
	public int? Position => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.OldValue;
	public DiscordOverwrite[] PermissionOverwrites => (DiscordOverwrite[])this.Changes.FirstOrDefault(x => x.Key == "permission_overwrites")?.OldValue;
	public bool IsNfsw => (bool)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.OldValue;
	public int PerUserRateLimit => (int)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.OldValue;
	public DiscordEmoji? IconEmoji => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.OldValue;
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.OldValue;
	public ChannelFlags Flags => (ChannelFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;

	public ulong? ParentId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.OldValue;
	public DiscordChannel? Parent => this.Discord.Guilds[this.GuildId].Channels[this.ParentId.Value];

	#region Voice
	public int? Bitrate => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.OldValue;
	public int? UserLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.OldValue;
	public string? RtcRegionId => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.OldValue;
	public DiscordVoiceRegion? RtcRegion => this.Discord.VoiceRegions[this.RtcRegionId];
	public VideoQualityMode? VideoQualityMode => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.OldValue;
	#endregion

	#region Forum
	public ForumPostTag? AvailableTags => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.OldValue;
	public ForumReactionEmoji? DefaultReactionEmoji => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.OldValue;
	public ForumPostSortOrder? DefaultSortOrder => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.OldValue;
	public ForumLayout? DefaultLayout => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.OldValue;
	public int? DefaultThreadPerUserRateLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.OldValue;
	#endregion
}
