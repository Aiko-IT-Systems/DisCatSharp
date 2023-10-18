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

	public DiscordUser Bot => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);

    internal override string? ChangeDescription
		=> $"{this.UserId} added {this.Bot.Username ?? "Not cached"}.Italic() ({this.TargetId})";
}
