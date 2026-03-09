using System;

using DisCatSharp.Voice.Logging;

namespace Microsoft.Extensions.Logging;

internal static class VoiceLoggerExtensions
{
	public static void VoiceDebug(this ILogger logger, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogDebug(message, args);
	}

	public static void VoiceDebug(this ILogger logger, EventId eventId, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogDebug(eventId, message, args);
	}

	public static void VoiceDebug(this ILogger logger, EventId eventId, Exception exception, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogDebug(eventId, exception, message, args);
	}

	public static void VoiceTrace(this ILogger logger, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogTrace(message, args);
	}

	public static void VoiceTrace(this ILogger logger, EventId eventId, string message, params object?[] args)
	{
		if (!VoiceRuntimeLogging.IsDebugEnabled(logger))
			return;

		logger.LogTrace(eventId, message, args);
	}
}
