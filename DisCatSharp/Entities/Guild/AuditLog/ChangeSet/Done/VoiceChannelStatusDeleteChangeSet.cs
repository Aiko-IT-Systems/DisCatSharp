using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating voice channel status.
/// </summary>
public sealed class VoiceChannelStatusDeleteChangeSet : DiscordAuditLogEntry
{
	public VoiceChannelStatusDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.VoiceChannelStatusDelete;
	}

	public ulong ChannelId => (ulong)this.Options!.ChannelId;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId, out var channel) ? channel : null;

}
