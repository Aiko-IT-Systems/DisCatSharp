using System;

using DisCatSharp.Voice.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
///     Voice-specific logging extensions that respect runtime debug toggles.
/// </summary>
internal static class VoiceLoggerExtensions
{
	/// <summary>
	///     Logs a debug message when voice debug logging is enabled for the logger instance.
	/// </summary>
	/// <param name="logger">Target logger.</param>
	/// <param name="message">Message template.</param>
	/// <param name="args">Template arguments.</param>
	public static void VoiceDebug(this ILogger logger, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogDebug(message, args);
	}

	/// <summary>
	///     Logs a debug message with event ID when voice debug logging is enabled.
	/// </summary>
	/// <param name="logger">Target logger.</param>
	/// <param name="eventId">Associated event identifier.</param>
	/// <param name="message">Message template.</param>
	/// <param name="args">Template arguments.</param>
	public static void VoiceDebug(this ILogger logger, EventId eventId, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogDebug(eventId, message, args);
	}

	/// <summary>
	///     Logs a debug message with exception and event ID when voice debug logging is enabled.
	/// </summary>
	/// <param name="logger">Target logger.</param>
	/// <param name="eventId">Associated event identifier.</param>
	/// <param name="exception">Exception to include in the log entry.</param>
	/// <param name="message">Message template.</param>
	/// <param name="args">Template arguments.</param>
	public static void VoiceDebug(this ILogger logger, EventId eventId, Exception exception, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogDebug(eventId, exception, message, args);
	}

	/// <summary>
	///     Logs a trace message when voice debug logging is enabled.
	/// </summary>
	/// <param name="logger">Target logger.</param>
	/// <param name="message">Message template.</param>
	/// <param name="args">Template arguments.</param>
	public static void VoiceTrace(this ILogger logger, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogTrace(message, args);
	}

	/// <summary>
	///     Logs a trace message with event ID when voice debug logging is enabled.
	/// </summary>
	/// <param name="logger">Target logger.</param>
	/// <param name="eventId">Associated event identifier.</param>
	/// <param name="message">Message template.</param>
	/// <param name="args">Template arguments.</param>
	public static void VoiceTrace(this ILogger logger, EventId eventId, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogTrace(eventId, message, args);
	}
}
