using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting a single message.
/// </summary>
public sealed class MessageDeleteChangeSet : DiscordAuditLogEntry
{
	public MessageDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.MessageDelete;
	}

	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);

	public int Count => (int)this.Options.Count;

	public ulong ChannelId => (ulong)this.Options.ChannelId;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId, out var channel) ? channel : null;
}
