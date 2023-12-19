using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildMemberTimeoutRemoved"/> event.
/// </summary>
public class GuildMemberTimeoutRemoveEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the member that was affected by the timeout.
	/// </summary>
	public DiscordMember Target { get; internal set; }

	/// <summary>
	/// Gets the member that removed the timeout for the member.
	/// </summary>
	public DiscordMember Actor { get; internal set; }

	/// <summary>
	/// Gets the guild this member was timed out.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the audit log id.
	/// </summary>
	public ulong? AuditLogId { get; internal set; }

	/// <summary>
	/// Gets the audit log reason.
	/// </summary>
	public string AuditLogReason { get; internal set; }

	/// <summary>
	/// Gets the timeout time before the remove.
	/// </summary>
	public DateTimeOffset TimeoutBefore { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildMemberTimeoutRemoveEventArgs"/> class.
	/// </summary>
	internal GuildMemberTimeoutRemoveEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
