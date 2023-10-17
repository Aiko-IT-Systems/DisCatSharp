using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for adding a bot user to the server.
/// </summary>
public sealed class BotAddChangeSet : DiscordAuditLogEntry
{
	internal BotAddChangeSet()
	{
		this.ValidFor = AuditLogActionType.BotAdd;
	}
}
