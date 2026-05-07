using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a pending ingress state entry, such as an OAuth request awaiting a callback.
/// </summary>
/// <remarks>
///     The stored <see cref="RequestUri" /> and <see cref="Flow" /> values are used by the default OAuth callback handler to validate
///     that the callback matches the original authorization request.
/// </remarks>
public sealed class DiscordIngressPendingState
{
	private static readonly IReadOnlyDictionary<string, string?> s_emptyProperties =
		new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>());

	/// <summary>
	///     Gets or sets the stable key used to locate the pending state entry.
	/// </summary>
	public required string Key { get; init; }

	/// <summary>
	///     Gets or sets the logical flow name associated with this state entry.
	/// </summary>
	/// <remarks>
	///     Use distinct flow names when multiple callback-driven features share the same pending-state store.
	/// </remarks>
	public string Flow { get; init; } = "oauth";

	/// <summary>
	///     Gets or sets the originating request URI when one is available.
	/// </summary>
	/// <remarks>
	///     For OAuth flows this is typically the original Discord authorization URL so redirect URI, scope, and integration type can be
	///     recovered during callback processing.
	/// </remarks>
	public Uri? RequestUri { get; init; }

	/// <summary>
	///     Gets or sets when the pending state was created.
	/// </summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>
	///     Gets or sets when the pending state expires.
	/// </summary>
	public DateTimeOffset ExpiresAt { get; init; }

	/// <summary>
	///     Gets or sets additional flow-specific properties associated with the entry.
	/// </summary>
	/// <remarks>
	///     These values are not interpreted by the default store and are preserved for custom callback handlers.
	/// </remarks>
	public IReadOnlyDictionary<string, string?> Properties { get; init; } = s_emptyProperties;
}
