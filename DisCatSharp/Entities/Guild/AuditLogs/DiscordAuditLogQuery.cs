using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a query for fetching guild audit log entries.
/// </summary>
/// <remarks>
///     Discord allows combining the action type and actor filters with either a <c>before</c> or <c>after</c> cursor.
///     When both cursors are omitted, Discord returns its default descending page order.
///     <see cref="Before" /> and <see cref="After" /> are mutually exclusive in DisCatSharp's audit log API.
/// </remarks>
public sealed class DiscordAuditLogQuery
{
	/// <summary>
	///     Gets or sets the maximum number of entries.
	/// </summary>
	/// <remarks>
	///     Discord accepts values from 1 through 100. <see cref="DiscordGuild.GetAuditLogEntriesAsync(DiscordAuditLogQuery?)" />
	///     clamps out-of-range values before sending the request.
	/// </remarks>
	public int? Limit { get; set; }

	/// <summary>
	///     Gets or sets the acting user id filter.
	/// </summary>
	public ulong? UserId { get; set; }

	/// <summary>
	///     Gets or sets the action type filter.
	/// </summary>
	public AuditLogActionType? ActionType { get; set; }

	/// <summary>
	///     Gets or sets the before cursor.
	/// </summary>
	/// <remarks>
	///     Supplying a <c>before</c> cursor keeps Discord's default descending order.
	///     This cursor cannot be combined with <see cref="After" />.
	/// </remarks>
	public ulong? Before { get; set; }

	/// <summary>
	///     Gets or sets the after cursor.
	/// </summary>
	/// <remarks>
	///     Supplying an <c>after</c> cursor causes Discord to return an ascending page.
	///     This cursor cannot be combined with <see cref="Before" />.
	/// </remarks>
	public ulong? After { get; set; }
}
