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

	/// <summary>
	/// Gets the Discord channel associated with this change.
	/// </summary>
	public DiscordChannel Channel => this.Discord.Guilds[this.GuildId].Channels[this.TargetId!.Value];

	/// <summary>
	/// Gets the new name of the channel.
	/// </summary>
	public string Name => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	/// <summary>
	/// Gets the type of the channel.
	/// </summary>
	public ChannelType Type => (ChannelType)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;

	/// <summary>
	/// Gets the new topic of the channel.
	/// </summary>
	public string? Topic => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.NewValue;

	/// <summary>
	/// Gets the new position of the channel.
	/// </summary>
	public int? Position => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.NewValue;

	/// <summary>
	/// Gets the new permission overwrites for the channel.
	/// </summary>
	public DiscordOverwrite[] PermissionOverwrites => (DiscordOverwrite[])this.Changes.FirstOrDefault(x => x.Key == "permission_overwrites")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the channel is marked as NSFW (Not Safe For Work).
	/// </summary>
	public bool IsNfsw => (bool)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.NewValue;

	/// <summary>
	/// Gets the new rate limit per user for the channel.
	/// </summary>
	public int PerUserRateLimit => (int)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.NewValue;

	/// <summary>
	/// Gets the new icon emoji for the channel.
	/// </summary>
	public DiscordEmoji? IconEmoji => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.NewValue;

	/// <summary>
	/// Gets the new default auto-archive duration for the channel.
	/// </summary>
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.NewValue;

	/// <summary>
	/// Gets the new flags for the channel.
	/// </summary>
	public ChannelFlags Flags => (ChannelFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;

	/// <summary>
	/// Gets the new parent ID for the channel.
	/// </summary>
	public ulong? ParentId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.NewValue;

	/// <summary>
	/// Gets the parent channel of this channel, if it has one.
	/// </summary>
	public DiscordChannel? Parent => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ParentId ?? 0ul, out var channel) ? channel : null;

#region Voice

	/// <summary>
	/// Gets the new bitrate for the voice channel.
	/// </summary>
	public int? Bitrate => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.NewValue;

	/// <summary>
	/// Gets the new user limit for the voice channel.
	/// </summary>
	public int? UserLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.NewValue;

	/// <summary>
	/// Gets the new RTC (Real-Time Communication) region ID for the voice channel.
	/// </summary>
	public string? RtcRegionId => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.NewValue;

	/// <summary>
	/// Gets the RTC region associated with the voice channel.
	/// </summary>
	public DiscordVoiceRegion? RtcRegion => this.Discord.VoiceRegions[this.RtcRegionId];

	/// <summary>
	/// Gets the new video quality mode for the voice channel.
	/// </summary>
	public VideoQualityMode? VideoQualityMode => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.NewValue;

#endregion

#region Forum

	/// <summary>
	/// Gets the new available tags for the forum channel.
	/// </summary>
	public ForumPostTag? AvailableTags => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.NewValue;

	/// <summary>
	/// Gets the new default reaction emoji for the forum channel.
	/// </summary>
	public ForumReactionEmoji? DefaultReactionEmoji => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.NewValue;

	/// <summary>
	/// Gets the new default sort order for the forum channel.
	/// </summary>
	public ForumPostSortOrder? DefaultSortOrder => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.NewValue;

	/// <summary>
	/// Gets the new default layout for the forum channel.
	/// </summary>
	public ForumLayout? DefaultLayout => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.NewValue;

	/// <summary>
	/// Gets the new default thread per user rate limit for the forum channel.
	/// </summary>
	public int? DefaultThreadPerUserRateLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.NewValue;

#endregion
}
