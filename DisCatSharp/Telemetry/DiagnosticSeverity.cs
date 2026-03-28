namespace DisCatSharp.Telemetry;

/// <summary>
///     Diagnostic severity levels that map to the underlying reporting backend.
/// </summary>
internal enum DiagnosticSeverity
{
	/// <summary>
	///     Debug-level diagnostic.
	/// </summary>
	Debug,

	/// <summary>
	///     Informational diagnostic.
	/// </summary>
	Info,

	/// <summary>
	///     Warning-level diagnostic.
	/// </summary>
	Warning,

	/// <summary>
	///     Error-level diagnostic.
	/// </summary>
	Error,

	/// <summary>
	///     Fatal-level diagnostic.
	/// </summary>
	Fatal
}
