using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for moving a member to a different voice channel.
/// </summary>
public sealed class MemberMoveChangeSet : DiscordAuditLogEntry
{
	public MemberMoveChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberMove;
	}

	public int Count => (int)this.Options.Count;

	public ulong ChannelId => (ulong)this.Options.ChannelId;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId, out var channel) ? channel : null;
}
