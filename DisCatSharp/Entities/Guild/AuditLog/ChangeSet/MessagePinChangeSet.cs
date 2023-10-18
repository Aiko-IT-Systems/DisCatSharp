using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for pinning a message to a channel.
/// </summary>
public sealed class MessagePinChangeSet : DiscordAuditLogEntry
{
	internal MessagePinChangeSet()
	{
		this.ValidFor = AuditLogActionType.MessagePin;
	}

	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);

	public ulong ChannelId => (ulong)this.Options.ChannelId;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId, out var channel) ? channel : null;

	public ulong MessageId => (ulong)this.Options.MessageId;
	public DiscordMessage? Message => this.Discord.MessageCache.TryGet(x => x.Id == this.MessageId, out var message) ? message : null;
}
