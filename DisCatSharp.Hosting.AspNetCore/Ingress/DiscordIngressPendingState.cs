using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a pending ingress state entry, such as an OAuth request awaiting a callback.
/// </summary>
public sealed class DiscordIngressPendingState
{
	private static readonly IReadOnlyDictionary<string, string?> EmptyProperties =
		new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>());

	/// <summary>
	///     Gets or sets the stable key used to locate the pending state entry.
	/// </summary>
	public required string Key { get; init; }

	/// <summary>
	///     Gets or sets the logical flow name associated with this state entry.
	/// </summary>
	public string Flow { get; init; } = "oauth";

	/// <summary>
	///     Gets or sets the originating request URI when one is available.
	/// </summary>
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
	public IReadOnlyDictionary<string, string?> Properties { get; init; } = EmptyProperties;
}
