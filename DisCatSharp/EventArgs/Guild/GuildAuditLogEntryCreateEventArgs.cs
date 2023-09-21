using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildAuditLogEntryCreated"/> event.
/// </summary>
public class GuildAuditLogEntryCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the audit log entry.
	/// Cast to correct type by action type.
	/// </summary>
	public DiscordAuditLogEntry AuditLogEntry { get; internal set; }

	/// <summary>
	/// Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildAuditLogEntryCreateEventArgs"/> class.
	/// </summary>
	internal GuildAuditLogEntryCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
