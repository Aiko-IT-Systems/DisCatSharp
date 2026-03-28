namespace DisCatSharp.Telemetry;

/// <summary>
///     Well-known tag keys and values used across the diagnostics sink.
///     Extension packages should use these constants for consistency.
/// </summary>
internal static class DiagnosticTags
{
	// ── Tag keys ──────────────────────────────────────────────────

	/// <summary>
	///     The library package that produced the event (e.g. <c>"DisCatSharp"</c>, <c>"DisCatSharp.Lavalink"</c>).
	/// </summary>
	public const string Source = "dcs.source";

	/// <summary>
	///     Discord API version the client is targeting.
	/// </summary>
	public const string ApiVersion = "dcs.api_version";

	/// <summary>
	///     Shard ID the event originated from.
	/// </summary>
	public const string ShardId = "dcs.shard_id";

	/// <summary>
	///     Total shard count.
	/// </summary>
	public const string ShardCount = "dcs.shard_count";

	/// <summary>
	///     Distinguishes library-internal errors from upstream service errors.
	/// </summary>
	public const string ErrorOrigin = "dcs.error_origin";

	/// <summary>
	///     The upstream service name when <see cref="ErrorOrigin" /> is <see cref="OriginUpstream" />
	///     (e.g. <c>"lavalink"</c>, <c>"spotify"</c>).
	/// </summary>
	public const string UpstreamService = "dcs.upstream_service";

	/// <summary>
	///     REST route that triggered the event.
	/// </summary>
	public const string RestRoute = "dcs.rest_route";

	/// <summary>
	///     HTTP status code returned by the REST call.
	/// </summary>
	public const string RestStatus = "dcs.rest_status";

	/// <summary>
	///     Entity type being deserialized when a field mismatch is detected.
	/// </summary>
	public const string EntityType = "dcs.entity_type";

	/// <summary>
	///     Gateway session lifecycle event (<c>"start"</c> / <c>"end"</c>).
	/// </summary>
	public const string SessionEvent = "dcs.session_event";

	/// <summary>
	///     Metric name for metric-style events.
	/// </summary>
	public const string MetricName = "dcs.metric_name";

	/// <summary>
	///     Metric unit for metric-style events.
	/// </summary>
	public const string MetricUnit = "dcs.metric_unit";

	// ── Tag values ────────────────────────────────────────────────

	/// <summary>
	///     Error originated inside the library itself.
	/// </summary>
	public const string OriginLibrary = "library";

	/// <summary>
	///     Error originated in an upstream service (e.g. Lavalink server, Discord API bug).
	/// </summary>
	public const string OriginUpstream = "upstream";
}
