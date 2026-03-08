using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Voice;

internal static class VoiceRuntimeLogging
{
	private sealed class DebugFlag
	{
		public int Enabled;
	}

	private static readonly ConditionalWeakTable<ILogger, DebugFlag> _loggerDebugFlags = [];

	internal static void SetEnableDebugLogs(ILogger logger, bool enabled)
	{
		ArgumentNullException.ThrowIfNull(logger);
		var state = _loggerDebugFlags.GetValue(logger, static _ => new DebugFlag());
		Volatile.Write(ref state.Enabled, enabled ? 1 : 0);
	}

	internal static bool IsDebugEnabled(ILogger logger)
	{
		ArgumentNullException.ThrowIfNull(logger);
		return _loggerDebugFlags.TryGetValue(logger, out var state) && Volatile.Read(ref state.Enabled) == 1;
	}
}
