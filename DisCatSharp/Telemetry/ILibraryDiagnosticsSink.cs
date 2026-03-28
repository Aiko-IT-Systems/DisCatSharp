using System;
using System.Collections.Generic;

namespace DisCatSharp.Telemetry;

/// <summary>
///     Defines the contract for a library diagnostics sink.
///     All library-originated telemetry (from core and extension packages) is routed
///     through this abstraction so the implementation can be swapped without touching call sites.
/// </summary>
/// <remarks>
///     Extension packages access this through <c>BaseExtension.Client.DiagnosticsSink</c> via InternalsVisibleTo.
/// </remarks>
internal interface ILibraryDiagnosticsSink
{
	/// <summary>
	///     Gets whether this sink is enabled and accepting events.
	/// </summary>
	bool IsEnabled { get; }

	/// <summary>
	///     Captures an exception event with optional structured context.
	/// </summary>
	/// <param name="source">The reporting source identifier (e.g. <c>"DisCatSharp"</c>, <c>"DisCatSharp.Lavalink"</c>).</param>
	/// <param name="exception">The exception to report.</param>
	/// <param name="context">Optional structured context to attach (e.g. route, timing).</param>
	/// <param name="tags">Optional tags for indexing and filtering (e.g. route, status code).</param>
	void CaptureException(string source, Exception exception, IDictionary<string, object>? context = null, IDictionary<string, string>? tags = null);

	/// <summary>
	///     Captures a structured diagnostic event (e.g., missing-field report).
	/// </summary>
	/// <param name="report">The diagnostic report to capture.</param>
	void CaptureReport(DiagnosticReport report);

	/// <summary>
	///     Signals the start of a gateway session (e.g., on READY event).
	///     Used for release health tracking and session lifecycle visibility.
	/// </summary>
	void StartSession();

	/// <summary>
	///     Signals the end of a gateway session (e.g., on disconnect/dispose).
	/// </summary>
	void EndSession();

	/// <summary>
	///     Adds a breadcrumb to the diagnostics trail.
	///     Breadcrumbs are attached to subsequent error/report events to provide operation context.
	/// </summary>
	/// <param name="source">The reporting source identifier.</param>
	/// <param name="category">The breadcrumb category (e.g. <c>"gateway"</c>, <c>"rest"</c>, <c>"dispatch"</c>).</param>
	/// <param name="message">The human-readable breadcrumb message.</param>
	/// <param name="level">The severity level of the breadcrumb.</param>
	/// <param name="data">Optional key-value data to attach to the breadcrumb.</param>
	void AddBreadcrumb(string source, string category, string message, DiagnosticSeverity level = DiagnosticSeverity.Info, IDictionary<string, string>? data = null);

	/// <summary>
	///     Emits a metric-style event that can be aggregated and charted in the backend (e.g. Sentry Discover).
	///     Use for counters, gauges, and timing measurements.
	/// </summary>
	/// <param name="source">The reporting source identifier.</param>
	/// <param name="name">The metric name (e.g. <c>"rest_latency"</c>, <c>"gateway_dispatch_count"</c>).</param>
	/// <param name="value">The metric value.</param>
	/// <param name="unit">The unit of measurement (e.g. <c>"ms"</c>, <c>"count"</c>, <c>"bytes"</c>).</param>
	/// <param name="tags">Optional tags for filtering and grouping.</param>
	void EmitMetric(string source, string name, double value, string unit, IDictionary<string, string>? tags = null);

	/// <summary>
	///     Creates a disposable timer that automatically emits a duration metric when disposed.
	/// </summary>
	/// <param name="source">The reporting source identifier.</param>
	/// <param name="name">The metric name for the timing measurement.</param>
	/// <param name="tags">Optional tags for filtering.</param>
	/// <returns>A disposable timer. Dispose it (or use <c>using</c>) to emit the metric.</returns>
	IDisposable StartTiming(string source, string name, IDictionary<string, string>? tags = null);

	/// <summary>
	///     Flushes any buffered events.
	///     Call sites should not need to manage flush timing themselves.
	/// </summary>
	void Flush();
}
