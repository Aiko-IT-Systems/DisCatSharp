using System;
using System.Collections.Generic;

namespace DisCatSharp.Telemetry;

/// <summary>
///     Represents a structured diagnostic report for non-exception events
///     such as missing-field discoveries and deserialization warnings.
/// </summary>
internal sealed class DiagnosticReport
{
	/// <summary>
	///     Gets or sets the reporting source identifier
	///     (e.g. <c>"DisCatSharp"</c>, <c>"DisCatSharp.Lavalink"</c>, <c>"DisCatSharp.Extensions.OAuth2Web"</c>).
	/// </summary>
	public required string Source { get; init; }

	/// <summary>
	///     Gets or sets the severity level.
	/// </summary>
	public required DiagnosticSeverity Severity { get; init; }

	/// <summary>
	///     Gets or sets the logger/source name (e.g. the class that produced the report).
	/// </summary>
	public required string Logger { get; init; }

	/// <summary>
	///     Gets or sets the human-readable message.
	/// </summary>
	public required string Message { get; init; }

	/// <summary>
	///     Gets or sets the exception, if any.
	/// </summary>
	public Exception? Exception { get; init; }

	/// <summary>
	///     Gets or sets extra structured data to attach.
	/// </summary>
	public IDictionary<string, object>? Extra { get; init; }

	/// <summary>
	///     Gets or sets additional fingerprint components for deduplication.
	/// </summary>
	public string? AdditionalFingerprint { get; init; }

	/// <summary>
	///     Gets or sets whether the report should be deduplicated per client instance using its computed fingerprint.
	///     This is intended for noisy schema-drift style diagnostics that only need to be emitted once per unique shape.
	/// </summary>
	public bool DeduplicateByFingerprint { get; init; }

	/// <summary>
	///     Gets or sets structured tags for indexing and filtering.
	///     Tags are low-cardinality key-value pairs (e.g., <c>"dcs.entity_type"</c> → <c>"DiscordGuild"</c>).
	/// </summary>
	public IDictionary<string, string>? Tags { get; init; }

	/// <summary>
	///     Gets or sets a file payload to include as a Sentry attachment.
	///     Used for large scrubbed JSON payloads that exceed the inline extra field limit.
	/// </summary>
	public byte[]? FilePayload { get; init; }

	/// <summary>
	///     Gets or sets the file name for the <see cref="FilePayload" /> attachment.
	/// </summary>
	public string? FilePayloadName { get; init; }

	/// <summary>
	///     Gets or sets the user info to attach, if any.
	/// </summary>
	public DiagnosticUserInfo? UserInfo { get; init; }
}
