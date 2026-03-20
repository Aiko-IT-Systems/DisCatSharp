using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Controls which audit log references should be upgraded by
///     <see cref="Entities.DiscordAuditLogEntry.HydrateAsync(AuditLogHydrationTargets, bool)" />.
/// </summary>
/// <remarks>
///     Audit log parsing is intentionally side-effect free and may therefore produce partial or synthetic entities.
///     These flags allow callers to selectively replace those placeholders with cached or freshly fetched live data.
/// </remarks>
[Flags]
public enum AuditLogHydrationTargets
{
	/// <summary>
	///     Does not hydrate any references.
	/// </summary>
	None = 0,

	/// <summary>
	///     Hydrates the acting moderator or actor reference.
	/// </summary>
	Actor = 1 << 0,

	/// <summary>
	///     Hydrates the primary target reference for the entry family.
	/// </summary>
	/// <remarks>
	///     Examples include the target member, role, webhook, scheduled event, thread, or soundboard sound.
	/// </remarks>
	Target = 1 << 1,

	/// <summary>
	///     Hydrates secondary or contextual references attached to the entry family.
	/// </summary>
	/// <remarks>
	///     Examples include channels referenced through the options payload, overwrite principals, or related members on
	///     message and auto moderation entries.
	/// </remarks>
	Related = 1 << 2,

	/// <summary>
	///     Hydrates the actor, primary target, and related references.
	/// </summary>
	All = Actor | Target | Related
}
