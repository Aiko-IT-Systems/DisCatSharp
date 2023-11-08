using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for adding a bot user to the server.
/// </summary>
public sealed class BotAddChangeSet : DiscordAuditLogEntry
{
	public BotAddChangeSet()
	{
		this.ValidFor = AuditLogActionType.BotAdd;
	}

	/// <summary>
	/// Gets the bot user that was added.
	/// </summary>
	public DiscordUser Bot => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"{this.User} added {this.Bot}";
}
