using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a new channel creation.
/// </summary>
public sealed class ChannelCreateChangeSet : DiscordAuditLogEntry
{
	internal ChannelCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelCreate;
	}

	public DiscordChannel Channel => this.Discord.Guilds[this.GuildId].Channels[this.TargetId!.Value];

	public string Name => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public string? Topic => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.NewValue;
	public ChannelType Type => (ChannelType)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;
	public DiscordOverwrite[] PermissionOverwrites => (DiscordOverwrite[])this.Changes.FirstOrDefault(x => x.Key == "permission_overwrites")?.NewValue;
	public bool IsNfsw => (bool)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.NewValue;
	public int PerUserRateLimit => (int)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.NewValue;
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.NewValue;
	public DiscordEmoji? IconEmoji => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.NewValue;
	public ForumPostTag? AvailableTags => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.NewValue;
	public ChannelFlags Flags => (ChannelFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;
}
