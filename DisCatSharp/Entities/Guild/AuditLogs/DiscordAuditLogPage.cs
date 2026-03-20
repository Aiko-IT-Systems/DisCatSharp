using System.Collections.Generic;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a parsed audit log page.
/// </summary>
/// <remarks>
///     Discord may return pages in descending or ascending order depending on whether the request used a
///     <c>before</c> or <c>after</c> cursor. The page exposes that ordering explicitly through <see cref="IsAscending" />.
/// </remarks>
public sealed class DiscordAuditLogPage
{
	/// <summary>
	///     Gets the parsed entries.
	/// </summary>
	public required IReadOnlyList<DiscordAuditLogEntry> Entries { get; init; }

	/// <summary>
	///     Gets whether the page is ordered ascending.
	/// </summary>
	/// <remarks>
	///     When <see langword="false" />, the page follows Discord's default descending audit log order.
	/// </remarks>
	public required bool IsAscending { get; init; }

	/// <summary>
	///     Gets the first entry id.
	/// </summary>
	/// <remarks>
	///     This is the first id in the returned page order, not necessarily the newest audit log entry in the guild.
	/// </remarks>
	public ulong? FirstEntryId { get; init; }

	/// <summary>
	///     Gets the last entry id.
	/// </summary>
	/// <remarks>
	///     This is the last id in the returned page order, not necessarily the oldest audit log entry in the guild.
	/// </remarks>
	public ulong? LastEntryId { get; init; }
}
