using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisCatSharp.Voice.Interop;

/// <summary>
///     Resolves native voice dependencies from the local runtimes folder when they are not surfaced via deps runtime targets.
/// </summary>
internal static class VoiceNativeLibraryResolver
{
	[ModuleInitializer]
	internal static void Initialize()
		=> NativeLibrary.SetDllImportResolver(typeof(VoiceNativeLibraryResolver).Assembly, Resolve);

	private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (!IsKnownLibrary(libraryName))
			return IntPtr.Zero;

		var fileName = GetPlatformFileName(libraryName);
		if (fileName is null)
			return IntPtr.Zero;

		var baseDirectory = AppContext.BaseDirectory;
		foreach (var candidate in GetCandidatePaths(baseDirectory, fileName))
		{
			if (!File.Exists(candidate))
				continue;

			if (NativeLibrary.TryLoad(candidate, out var handle))
				return handle;
		}

		return IntPtr.Zero;
	}

	private static bool IsKnownLibrary(string libraryName)
		=> string.Equals(libraryName, "libopus", StringComparison.Ordinal)
		   || string.Equals(libraryName, "libsodium", StringComparison.Ordinal);

	private static string? GetPlatformFileName(string libraryName)
	{
		if (OperatingSystem.IsWindows())
			return $"{libraryName}.dll";

		if (OperatingSystem.IsLinux())
			return $"{libraryName}.so";

		if (OperatingSystem.IsMacOS())
			return $"{libraryName}.dylib";

		return null;
	}

	private static string[] GetCandidatePaths(string baseDirectory, string fileName)
	{
		var rid = GetCurrentRuntimeIdentifier();
		return rid is null
			? [Path.Combine(baseDirectory, fileName)]
			:
			[
				Path.Combine(baseDirectory, fileName),
				Path.Combine(baseDirectory, "runtimes", rid, "native", fileName)
			];
	}

	private static string? GetCurrentRuntimeIdentifier()
	{
		if (OperatingSystem.IsWindows())
			return RuntimeInformation.OSArchitecture switch
			{
				Architecture.X64 => "win-x64",
				Architecture.X86 => "win-x86",
				Architecture.Arm64 => "win-arm64",
				_ => null
			};

		if (OperatingSystem.IsLinux())
			return RuntimeInformation.OSArchitecture switch
			{
				Architecture.X64 => "linux-x64",
				Architecture.Arm64 => "linux-arm64",
				_ => null
			};

		if (OperatingSystem.IsMacOS())
			return RuntimeInformation.OSArchitecture switch
			{
				Architecture.X64 => "osx-x64",
				Architecture.Arm64 => "osx-arm64",
				_ => null
			};

		return null;
	}
}
