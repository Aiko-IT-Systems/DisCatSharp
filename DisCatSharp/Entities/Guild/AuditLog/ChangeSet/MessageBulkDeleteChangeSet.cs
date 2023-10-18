using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting multiple messages.
/// </summary>
public sealed class MessageBulkDeleteChangeSet : DiscordAuditLogEntry
{
	internal MessageBulkDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.MessageBulkDelete;
	}

	public DiscordChannel Channel => this.Discord.Guilds[this.GuildId].Channels[this.TargetId!.Value];

	public int Count => (int)this.Options.Count;
}
