using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for moving a member to a different voice channel.
/// </summary>
public sealed class MemberMoveChangeSet : DiscordAuditLogEntry
{
	internal MemberMoveChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberMove;
	}

	public int Count => (int)this.Changes.FirstOrDefault(x => x.Key == "count")?.OldValue;

	public ulong ChannelId => (ulong)this.Changes.FirstOrDefault(x => x.Key == "channel_id")?.OldValue;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId, out var channel) ? channel : null;
}
