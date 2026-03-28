using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DisCatSharp.Telemetry;

/// <summary>
///     A disposable timer that emits a metric with the elapsed duration when disposed.
///     Use with <c>using</c> to automatically measure and report operation timing.
/// </summary>
/// <example>
///     <code>
///     using (sink.StartTiming("DisCatSharp", "rest_call", tags))
///     {
///         await httpClient.SendAsync(request);
///     }
///     // metric is emitted automatically with elapsed milliseconds
///     </code>
/// </example>
internal sealed class DiagnosticTimer : IDisposable
{
	private readonly ILibraryDiagnosticsSink _sink;
	private readonly string _source;
	private readonly string _name;
	private readonly IDictionary<string, string>? _tags;
	private readonly Stopwatch _sw;
	private bool _disposed;

	/// <summary>
	///     Initializes a new diagnostic timer and starts the stopwatch immediately.
	/// </summary>
	internal DiagnosticTimer(ILibraryDiagnosticsSink sink, string source, string name, IDictionary<string, string>? tags)
	{
		this._sink = sink;
		this._source = source;
		this._name = name;
		this._tags = tags;
		this._sw = Stopwatch.StartNew();
	}

	/// <summary>
	///     Stops the timer and emits the elapsed duration as a metric.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;
		this._sw.Stop();
		this._sink.EmitMetric(this._source, this._name, this._sw.Elapsed.TotalMilliseconds, "ms", this._tags);
	}
}
