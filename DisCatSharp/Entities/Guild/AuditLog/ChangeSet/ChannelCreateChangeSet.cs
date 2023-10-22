using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a new channel creation.
/// </summary>
public sealed class ChannelCreateChangeSet : DiscordAuditLogEntry
{
	public ChannelCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelCreate;
	}

	public DiscordChannel Channel => this.Discord.Guilds[this.GuildId].Channels[this.TargetId!.Value];

	public string Name => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public ChannelType Type => (ChannelType)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;
	public string? Topic => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.NewValue;
	public int? Position => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.NewValue;
	public DiscordOverwrite[] PermissionOverwrites => (DiscordOverwrite[])this.Changes.FirstOrDefault(x => x.Key == "permission_overwrites")?.NewValue;
	public bool IsNfsw => (bool)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.NewValue;
	public int PerUserRateLimit => (int)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.NewValue;
	public DiscordEmoji? IconEmoji => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.NewValue;
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.NewValue;
	public ChannelFlags Flags => (ChannelFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;

	public ulong? ParentId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.NewValue;
	public DiscordChannel? Parent => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ParentId ?? 0ul, out var channel) ? channel : null;

#region Voice

	public int? Bitrate => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.NewValue;
	public int? UserLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.NewValue;
	public string? RtcRegionId => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.NewValue;
	public DiscordVoiceRegion? RtcRegion => this.Discord.VoiceRegions[this.RtcRegionId];
	public VideoQualityMode? VideoQualityMode => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.NewValue;

#endregion

#region Forum

	public ForumPostTag? AvailableTags => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.NewValue;
	public ForumReactionEmoji? DefaultReactionEmoji => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.NewValue;
	public ForumPostSortOrder? DefaultSortOrder => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.NewValue;
	public ForumLayout? DefaultLayout => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.NewValue;
	public int? DefaultThreadPerUserRateLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.NewValue;

#endregion
}
