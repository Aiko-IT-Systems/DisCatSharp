using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for unpinning a message from a channel.
/// </summary>
public sealed class MessageUnpinChangeSet : DiscordAuditLogEntry
{
	public MessageUnpinChangeSet()
	{
		this.ValidFor = AuditLogActionType.MessageUnpin;
	}

	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);

	public ulong ChannelId => (ulong)this.Options.ChannelId;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId, out var channel) ? channel : null;

	public ulong MessageId => (ulong)this.Options.MessageId;
	public DiscordMessage? Message => this.Discord.MessageCache.TryGet(x => x.Id == this.MessageId, out var message) ? message : null;
}
