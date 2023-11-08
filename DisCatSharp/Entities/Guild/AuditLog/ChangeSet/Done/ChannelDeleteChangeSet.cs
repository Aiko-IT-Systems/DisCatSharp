using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a channel deletion.
/// </summary>
public sealed class ChannelDeleteChangeSet : DiscordAuditLogEntry
{
	public ChannelDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelDelete;
	}

	/// <summary>
	/// Gets the old name of the deleted channel.
	/// </summary>
	public string Name => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;

	/// <summary>
	/// Gets the old type of the deleted channel.
	/// </summary>
	public ChannelType Type => (ChannelType)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;

	/// <summary>
	/// Gets the old topic of the deleted channel.
	/// </summary>
	public string? Topic => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.OldValue;

	/// <summary>
	/// Gets the old position of the deleted channel.
	/// </summary>
	public int? Position => (int?)this.Changes.FirstOrDefault(x => x.Key == "position")?.OldValue;

	/// <summary>
	/// Gets the old permission overwrites of the deleted channel.
	/// </summary>
	public DiscordOverwrite[] PermissionOverwrites => (DiscordOverwrite[])this.Changes.FirstOrDefault(x => x.Key == "permission_overwrites")?.OldValue;

	/// <summary>
	/// Gets a value indicating whether the deleted channel was marked as NSFW.
	/// </summary>
	public bool IsNfsw => (bool)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.OldValue;

	/// <summary>
	/// Gets the old per-user rate limit of the deleted channel.
	/// </summary>
	public int PerUserRateLimit => (int)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.OldValue;

	/// <summary>
	/// Gets the old icon emoji of the deleted channel.
	/// </summary>
	public DiscordEmoji? IconEmoji => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.OldValue;

	/// <summary>
	/// Gets the old default auto-archive duration of the deleted channel.
	/// </summary>
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.OldValue;

	/// <summary>
	/// Gets the old flags of the deleted channel.
	/// </summary>
	public ChannelFlags Flags => (ChannelFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;

	/// <summary>
	/// Gets the old parent ID of the deleted channel.
	/// </summary>
	public ulong? ParentId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "parent_id")?.OldValue;

	/// <summary>
	/// Gets the old parent channel of the deleted channel.
	/// </summary>
	public DiscordChannel? Parent => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ParentId ?? 0ul, out var channel) ? channel : null;

#region Voice

	/// <summary>
	/// Gets the old bitrate of the deleted voice channel.
	/// </summary>
	public int? Bitrate => (int?)this.Changes.FirstOrDefault(x => x.Key == "bitrate")?.OldValue;

	/// <summary>
	/// Gets the old user limit of the deleted voice channel.
	/// </summary>
	public int? UserLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "user_limit")?.OldValue;

	/// <summary>
	/// Gets the old RTC region ID of the deleted voice channel.
	/// </summary>
	public string? RtcRegionId => (string?)this.Changes.FirstOrDefault(x => x.Key == "rtc_region")?.OldValue;

	/// <summary>
	/// Gets the old RTC region of the deleted voice channel.
	/// </summary>
	public DiscordVoiceRegion? RtcRegion => this.Discord.VoiceRegions[this.RtcRegionId];

	/// <summary>
	/// Gets the old video quality mode of the deleted voice channel.
	/// </summary>
	public VideoQualityMode? VideoQualityMode => (VideoQualityMode?)this.Changes.FirstOrDefault(x => x.Key == "video_quality_mode")?.OldValue;

#endregion

#region Forum

	/// <summary>
	/// Gets the old available tags of the deleted forum channel.
	/// </summary>
	public ForumPostTag? AvailableTags => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.OldValue;

	/// <summary>
	/// Gets the old default reaction emoji of the deleted forum channel.
	/// </summary>
	public ForumReactionEmoji? DefaultReactionEmoji => (ForumReactionEmoji?)this.Changes.FirstOrDefault(x => x.Key == "default_reaction_emoji")?.OldValue;

	/// <summary>
	/// Gets the old default sort order of the deleted forum channel.
	/// </summary>
	public ForumPostSortOrder? DefaultSortOrder => (ForumPostSortOrder?)this.Changes.FirstOrDefault(x => x.Key == "default_sort_order")?.OldValue;

	/// <summary>
	/// Gets the old default forum layout of the deleted forum channel.
	/// </summary>
	public ForumLayout? DefaultLayout => (ForumLayout?)this.Changes.FirstOrDefault(x => x.Key == "default_forum_layout")?.OldValue;

	/// <summary>
	/// Gets the old default thread per-user rate limit of the deleted forum channel.
	/// </summary>
	public int? DefaultThreadPerUserRateLimit => (int?)this.Changes.FirstOrDefault(x => x.Key == "default_thread_rate_limit_per_user")?.OldValue;

#endregion
}
