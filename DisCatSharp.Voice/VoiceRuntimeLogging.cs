using System;
using System.Threading;

namespace DisCatSharp.Voice;

internal static class VoiceRuntimeLogging
{
	private static int _enableDebugLogs = 1;

	internal static bool EnableDebugLogs
	{
		get => Volatile.Read(ref _enableDebugLogs) == 1;
		set => Volatile.Write(ref _enableDebugLogs, value ? 1 : 0);
	}
}
