using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DisCatSharp.Exceptions;

using Sentry;

namespace DisCatSharp.Telemetry;

/// <summary>
///     Centralizes telemetry bootstrap logic.
///     Produces a fully configured <see cref="ILibraryDiagnosticsSink" /> from a <see cref="DiscordConfiguration" />.
/// </summary>
internal static class TelemetryBootstrap
{
	private static readonly IReadOnlyDictionary<string, string> SourceCodeRoots = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["DisCatSharp"] = "DisCatSharp",
		["DisCatSharp.ApplicationCommands"] = "DisCatSharp.ApplicationCommands",
		["DisCatSharp.CommandsNext"] = "DisCatSharp.CommandsNext",
		["DisCatSharp.Hosting"] = "DisCatSharp.Hosting",
		["DisCatSharp.Hosting.DependencyInjection"] = "DisCatSharp.Hosting.DependencyInjection",
		["DisCatSharp.Interactivity"] = "DisCatSharp.Interactivity",
		["DisCatSharp.Lavalink"] = "DisCatSharp.Lavalink",
		["DisCatSharp.Voice"] = "DisCatSharp.Voice",
		["DisCatSharp.Configuration"] = "DisCatSharp.Configuration",
		["DisCatSharp.Common"] = "DisCatSharp.Common",
		["DisCatSharp.Experimental"] = "DisCatSharp.Experimental"
	};

	/// <summary>
	///     Creates the appropriate diagnostics sink based on the configuration.
	///     Returns <see cref="NoOpDiagnosticsSink" /> when telemetry is disabled.
	/// </summary>
	/// <param name="config">The Discord configuration.</param>
	/// <returns>A configured diagnostics sink.</returns>
	internal static ILibraryDiagnosticsSink CreateSink(DiscordConfiguration config)
	{
		if (!config.Telemetry.EnableSentry)
			return NoOpDiagnosticsSink.Instance;

		var options = BuildSentryOptions(config);
		return new SentryDiagnosticsSink(options, config);
	}

	/// <summary>
	///     Builds fully configured <see cref="SentryOptions" /> from the Discord configuration.
	///     This is the single source of truth for all Sentry option construction.
	/// </summary>
	/// <param name="config">The Discord configuration.</param>
	/// <returns>Configured Sentry options.</returns>
	internal static SentryOptions BuildSentryOptions(DiscordConfiguration config)
	{
		var release = GetLibraryVersion();

		SentryOptions options = new()
		{
			DetectStartupTime = StartupTimeDetectionMode.Fast,
			DiagnosticLevel = SentryLevel.Debug,
			Environment = GetSentryEnvironment(release) ? "dev" : "production",
			IsGlobalModeEnabled = false,
			TracesSampleRate = 1.0,
			ReportAssembliesMode = ReportAssembliesMode.InformationalVersion,
			Dsn = GetDsn(config),
			AttachStacktrace = true,
			StackTraceMode = StackTraceMode.Enhanced,
			SendClientReports = true,
			Release = release,
			IsEnvironmentUser = false,
			UseAsyncFileIO = true,
			EnableScopeSync = true,
			Debug = config.Telemetry.SentryDebug,
			MaxBreadcrumbs = config.Telemetry.AttachRecentLogEntries ? 100 : 0
		};

		options.AddInAppInclude("DisCatSharp");

		if (!config.Telemetry.DisableExceptionFilter)
			options.AddExceptionFilter(new DisCatSharpExceptionFilter(config));

		ConfigureScrubbers(options, config);
		ConfigureBeforeSend(options, config);

		return options;
	}

	/// <summary>
	///     Configures breadcrumb and transaction scrubbers when not disabled.
	/// </summary>
	private static void ConfigureScrubbers(SentryOptions options, DiscordConfiguration config)
	{
		if (config.Telemetry.DisableScrubber)
			return;

		options.SetBeforeBreadcrumb((b, _)
			=> new(
				Utilities.StripTokensAndOptIds(b.Message, config.Telemetry.EnableDiscordIdScrubber)!,
				b.Type!,
				b.Data?.Select(x => new KeyValuePair<string, string>(
					x.Key,
					Utilities.StripTokensAndOptIds(x.Value, config.Telemetry.EnableDiscordIdScrubber)!))
					.ToDictionary(x => x.Key, x => x.Value),
				b.Category,
				b.Level));

		options.SetBeforeSendTransaction((tr, _) =>
		{
			if (tr.Request.Data is string str)
				tr.Request.Data = Utilities.StripTokensAndOptIds(str, config.Telemetry.EnableDiscordIdScrubber);

			return tr;
		});
	}

	/// <summary>
	///     Configures the BeforeSend callback for exception filtering, user enrichment, and fingerprinting.
	/// </summary>
	private static void ConfigureBeforeSend(SentryOptions options, DiscordConfiguration config)
	{
		options.SetBeforeSend((e, _) =>
		{
			if (!config.Telemetry.DisableExceptionFilter)
			{
				if (e.Exception is not null)
				{
					var trackedException = e.Exception is SentryCapturableException { InnerException: not null } wrapper
						? wrapper.InnerException
						: e.Exception;

					if (!config.Telemetry.TrackExceptions.Contains(trackedException.GetType()))
						return null;
				}
				else if (e.Extra.ContainsKey("Found Fields"))
				{
					// Missing-field reports are allowed through the non-exception path.
				}
				else if (!e.Tags.ContainsKey(DiagnosticTags.Source))
				{
					return null;
				}
			}

			if (!e.Extra.ContainsKey("Found Fields") && (e.Fingerprint is null || e.Fingerprint.Count is 0))
				e.SetFingerprint(SentryDiagnosticsSink.GenerateFingerprint(e));

			RewriteStackFramePaths(e);

			return e;
		});
	}

	/// <summary>
	///     Rewrites stack frame paths to stable package-root-relative paths so Sentry code mappings can resolve them.
	/// </summary>
	internal static void RewriteStackFramePaths(SentryEvent e)
	{
		if (!e.Tags.TryGetValue(DiagnosticTags.Source, out var source)
			|| !SourceCodeRoots.TryGetValue(source, out var sourceRoot))
			return;

		foreach (var sentryException in e.SentryExceptions ?? [])
		{
			var frames = sentryException.Stacktrace?.Frames;
			if (frames is null)
				continue;

			foreach (var frame in frames)
			{
				var framePath = string.IsNullOrWhiteSpace(frame.AbsolutePath)
					? frame.FileName
					: frame.AbsolutePath;

				if (!TryRewriteFramePath(framePath, sourceRoot, out var rewrittenPath))
					continue;

				frame.FileName = rewrittenPath;
				frame.AbsolutePath = rewrittenPath;
			}
		}
	}

	/// <summary>
	///     Attempts to rewrite an emitted source path to a package-root-relative path.
	/// </summary>
	internal static bool TryRewriteFramePath(string? originalPath, string sourceRoot, out string rewrittenPath)
	{
		rewrittenPath = string.Empty;

		if (string.IsNullOrWhiteSpace(originalPath))
			return false;

		var trimmedOriginalPath = originalPath.Trim();
		if (trimmedOriginalPath.Contains("://", StringComparison.Ordinal))
			return false;

		var isRootedPath = Path.IsPathRooted(trimmedOriginalPath)
			|| trimmedOriginalPath.StartsWith(@"\\", StringComparison.Ordinal)
			|| trimmedOriginalPath.StartsWith("//", StringComparison.Ordinal)
			|| trimmedOriginalPath.Length >= 3
			&& char.IsLetter(trimmedOriginalPath[0])
			&& trimmedOriginalPath[1] == ':'
			&& (trimmedOriginalPath[2] == '\\' || trimmedOriginalPath[2] == '/');

		var normalizedPath = trimmedOriginalPath.Replace('/', '\\');

		var normalizedRoot = sourceRoot.Replace('/', '\\').TrimEnd('\\');
		var rootedPrefix = normalizedRoot + "\\";
		var rootIndex = normalizedPath.LastIndexOf(rootedPrefix, StringComparison.OrdinalIgnoreCase);
		if (rootIndex >= 0)
		{
			rewrittenPath = normalizedPath[rootIndex..];
			return true;
		}

		if (normalizedPath.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase)
			|| normalizedPath.StartsWith(rootedPrefix, StringComparison.OrdinalIgnoreCase))
		{
			rewrittenPath = normalizedPath;
			return true;
		}

		if (isRootedPath)
			return false;

		rewrittenPath = rootedPrefix + normalizedPath.TrimStart('\\');
		return true;
	}

	/// <summary>
	///     Resolves the effective Sentry DSN, preferring custom DSN over default.
	/// </summary>
	private static string GetDsn(DiscordConfiguration config)
		=> config.Telemetry.CustomSentryDsn ?? BaseDiscordClient.SentryDsn;

	/// <summary>
	///     Gets the library version string for Sentry release tagging.
	/// </summary>
	internal static string GetLibraryVersion()
	{
		var assembly = typeof(DiscordClient).GetTypeInfo().Assembly;
		var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

		if (infoVersion is not null)
			return $"DisCatSharp@{infoVersion.InformationalVersion}";

		var version = assembly.GetName().Version;
		return $"DisCatSharp@{version?.ToString(3) ?? "0.0.0"}";
	}

	/// <summary>
	///     Determines whether the given release string represents a pre-release version.
	///     Pre-releases contain a hyphen after the version number (e.g., 10.7.0-nightly).
	/// </summary>
	/// <param name="release">The release string (e.g., "DisCatSharp@10.7.0-nightly").</param>
	/// <returns><c>true</c> if the version is a pre-release; otherwise <c>false</c>.</returns>
	internal static bool IsPreRelease(string release)
	{
		// release format: "DisCatSharp@{version}" where version may contain "-suffix"
		var atIndex = release.IndexOf('@');
		var versionPart = atIndex >= 0 ? release[(atIndex + 1)..] : release;
		return versionPart.Contains('-');
	}

	/// <summary>
	///     Determines whether Sentry should use the dev environment for the current library build.
	/// </summary>
	/// <remarks>
	///     Pre-release versions are always treated as dev.
	///     Stable versions are treated as dev when the assembly metadata says the build did not come from CI.
	/// </remarks>
	internal static bool GetSentryEnvironment(string release)
	{
		if (IsPreRelease(release))
			return true;

		return !IsCiBuild(typeof(DiscordClient).GetTypeInfo().Assembly);
	}

	/// <summary>
	///     Returns whether the current assembly was produced by a CI build.
	/// </summary>
	internal static bool IsCiBuild(Assembly assembly)
		=> assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.Any(attr => string.Equals(attr.Key, "DisCatSharpBuildOrigin", StringComparison.Ordinal)
				&& string.Equals(attr.Value, "CI", StringComparison.OrdinalIgnoreCase));

	/// <summary>
	///     Builds a <see cref="DiagnosticUserInfo" /> from the current client state if user info attachment is enabled.
	/// </summary>
	/// <param name="config">The Discord configuration.</param>
	/// <param name="currentUser">The current bot user, if available.</param>
	/// <returns>User info, or <c>null</c> if not configured or unavailable.</returns>
	internal static DiagnosticUserInfo? BuildUserInfo(DiscordConfiguration config, Entities.DiscordUser? currentUser)
	{
		if (!config.Telemetry.AttachUserInfo || currentUser is null)
			return null;

		return new()
		{
			Id = currentUser.Id.ToString(),
			Username = currentUser.UsernameWithDiscriminator,
			DeveloperUserId = config.Telemetry.DeveloperUserId?.ToString(),
			FeedbackEmail = config.Telemetry.FeedbackEmail
		};
	}
}
