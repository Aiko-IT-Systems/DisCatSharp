using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for disconnecting a member from a voice channel.
/// </summary>
public sealed class MemberDisconnectChangeSet : DiscordAuditLogEntry
{
	public MemberDisconnectChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberDisconnect;
	}

	/// <summary>
	/// Gets the count of members that were disconnected.
	/// </summary>
	public int Count => this.Options!.Count.Value;

	/// <summary>
	/// Gets the channel from which the members were disconnected.
	/// </summary>
	public DiscordChannel Channel => this.Discord.Guilds[this.GuildId].GetChannel(this.TargetId.Value!);

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"{this.User} disconnected {this.Count} users from {this.Channel} with reason {this.Reason ?? "none"}";
}
