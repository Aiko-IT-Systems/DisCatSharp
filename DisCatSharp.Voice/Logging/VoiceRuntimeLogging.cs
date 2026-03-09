using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Voice.Logging;

/// <summary>
///     Per-logger runtime switch for enabling verbose voice diagnostics.
/// </summary>
internal static class VoiceRuntimeLogging
{
	/// <summary>
	///     Mutable debug flag container attached to an <see cref="ILogger"/> instance.
	/// </summary>
	private sealed class DebugFlag
	{
		/// <summary>
		///     1 when debug logging is enabled; otherwise 0.
		/// </summary>
		public int Enabled;
	}

	private static readonly ConditionalWeakTable<ILogger, DebugFlag> _loggerDebugFlags = [];

	/// <summary>
	///     Sets whether voice debug logs are enabled for a logger instance.
	/// </summary>
	/// <param name="logger">Target logger.</param>
	/// <param name="enabled">Whether to enable debug logs.</param>
	internal static void SetEnableDebugLogs(ILogger logger, bool enabled)
	{
		ArgumentNullException.ThrowIfNull(logger);
		var state = _loggerDebugFlags.GetValue(logger, static _ => new DebugFlag());
		Volatile.Write(ref state.Enabled, enabled ? 1 : 0);
	}

	/// <summary>
	///     Gets whether voice debug logs are enabled for a logger instance.
	/// </summary>
	/// <param name="logger">Target logger.</param>
	/// <returns><see langword="true"/> when enabled; otherwise <see langword="false"/>.</returns>
	internal static bool IsDebugEnabled(ILogger logger)
	{
		ArgumentNullException.ThrowIfNull(logger);
		return _loggerDebugFlags.TryGetValue(logger, out var state) && Volatile.Read(ref state.Enabled) == 1;
	}
}
