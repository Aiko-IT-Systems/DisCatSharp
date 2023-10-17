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
	public string? Topic => (string?)this.Changes.FirstOrDefault(x => x.Key == "topic")?.OldValue;
	public ChannelType Type => (ChannelType)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;
	public DiscordOverwrite[] PermissionOverwrites => (DiscordOverwrite[])this.Changes.FirstOrDefault(x => x.Key == "permission_overwrites")?.OldValue;
	public bool IsNfsw => (bool)this.Changes.FirstOrDefault(x => x.Key == "nsfw")?.OldValue;
	public int PerUserRateLimit => (int)this.Changes.FirstOrDefault(x => x.Key == "rate_limit_per_user")?.OldValue;
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration => (ThreadAutoArchiveDuration?)this.Changes.FirstOrDefault(x => x.Key == "default_auto_archive_duration")?.NewValue;
	public DiscordEmoji? IconEmoji => (DiscordEmoji?)this.Changes.FirstOrDefault(x => x.Key == "icon_emoji")?.OldValue;
	public ForumPostTag? AvailableTags => (ForumPostTag?)this.Changes.FirstOrDefault(x => x.Key == "available_tags")?.OldValue;
	public ChannelFlags Flags => (ChannelFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;
}
