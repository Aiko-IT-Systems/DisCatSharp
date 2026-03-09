using System;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Voice.Logging;

/// <summary>
///     Logger wrapper used to scope voice runtime logging toggles to a specific connection instance.
/// </summary>
internal sealed class VoiceScopedLogger : ILogger
{
	private readonly ILogger _inner;

	/// <summary>
	///     Initializes a new scoped logger wrapper.
	/// </summary>
	/// <param name="inner">Underlying logger instance.</param>
	public VoiceScopedLogger(ILogger inner)
	{
		this._inner = inner ?? throw new ArgumentNullException(nameof(inner));
	}

	/// <inheritdoc />
	public IDisposable? BeginScope<TState>(TState state)
		where TState : notnull
		=> this._inner.BeginScope(state);

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel)
		=> this._inner.IsEnabled(logLevel);

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		=> this._inner.Log(logLevel, eventId, state, exception, formatter);
}
