using System;
using System.Collections.Generic;

namespace DisCatSharp.Telemetry;

/// <summary>
///     Marker base class for exceptions that support Sentry structured context.
///     Used by the REST client to attach route/timing metadata.
/// </summary>
internal class SentryCapturableException : Exception
{
	private readonly Dictionary<string, IDictionary<string, object>> _contexts = [];

	/// <summary>
	///     Initializes a new instance of the <see cref="SentryCapturableException" /> class.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The inner exception.</param>
	public SentryCapturableException(string message, Exception innerException)
		: base(message, innerException)
	{ }

	/// <summary>
	///     Adds a Sentry context section to this exception.
	/// </summary>
	/// <param name="key">The context key.</param>
	/// <param name="data">The context data.</param>
	public void AddSentryContext(string key, IDictionary<string, object> data)
		=> this._contexts[key] = data;

	/// <summary>
	///     Gets the attached Sentry contexts.
	/// </summary>
	internal IReadOnlyDictionary<string, IDictionary<string, object>> Contexts => this._contexts;
}
