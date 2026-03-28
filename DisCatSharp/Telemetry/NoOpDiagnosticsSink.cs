using System;
using System.Collections.Generic;

namespace DisCatSharp.Telemetry;

/// <summary>
///     A no-op diagnostics sink used when telemetry is disabled.
///     All operations are safe and do nothing.
/// </summary>
internal sealed class NoOpDiagnosticsSink : ILibraryDiagnosticsSink
{
	/// <summary>
	///     Gets the shared singleton instance.
	/// </summary>
	public static NoOpDiagnosticsSink Instance { get; } = new();

	/// <inheritdoc />
	public bool IsEnabled => false;

	/// <inheritdoc />
	public void CaptureException(string source, Exception exception, IDictionary<string, object>? context = null, IDictionary<string, string>? tags = null)
	{ }

	/// <inheritdoc />
	public void CaptureReport(DiagnosticReport report)
	{ }

	/// <inheritdoc />
	public void StartSession()
	{ }

	/// <inheritdoc />
	public void EndSession()
	{ }

	/// <inheritdoc />
	public void AddBreadcrumb(string source, string category, string message, DiagnosticSeverity level = DiagnosticSeverity.Info, IDictionary<string, string>? data = null)
	{ }

	/// <inheritdoc />
	public void EmitMetric(string source, string name, double value, string unit, IDictionary<string, string>? tags = null)
	{ }

	/// <inheritdoc />
	public IDisposable StartTiming(string source, string name, IDictionary<string, string>? tags = null)
		=> NoOpTimer.Instance;

	/// <inheritdoc />
	public void Flush()
	{ }

	/// <summary>
	///     A no-op disposable returned by <see cref="StartTiming" />.
	/// </summary>
	private sealed class NoOpTimer : IDisposable
	{
		public static NoOpTimer Instance { get; } = new();

		public void Dispose()
		{ }
	}
}
