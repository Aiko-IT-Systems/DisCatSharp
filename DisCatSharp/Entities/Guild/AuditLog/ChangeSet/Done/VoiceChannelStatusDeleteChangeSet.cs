using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating voice channel status.
/// </summary>
public sealed class VoiceChannelStatusUpdateChangeSet : DiscordAuditLogEntry
{
	public VoiceChannelStatusUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.VoiceChannelStatusUpdate;
	}

	public ulong ChannelId => (ulong)this.Options!.ChannelId;
	public DiscordChannel? Channel => this.Discord.Guilds[this.GuildId].Channels.TryGetValue(this.ChannelId, out var channel) ? channel : null;
	public string Status => this.Options!.Status!;
}
