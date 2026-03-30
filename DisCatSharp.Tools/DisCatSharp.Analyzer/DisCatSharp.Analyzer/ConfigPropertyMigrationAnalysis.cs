using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Analyzer;

internal static class ConfigPropertyMigrationAnalysis
{
	private const string DiscordConfigurationTypeName = "DisCatSharp.DiscordConfiguration";

	// Maps old property name -> (nested config property, new property name)
	// e.g., "ApiVersion" -> ("Api", "Version")
	// For deeper nesting: "DisableUpdateCheck" -> ("Diagnostics.UpdateChecks", "Disabled")
	private static readonly ImmutableDictionary<string, (string NestedPath, string NewName)> s_propertyMigrations =
		ImmutableDictionary.CreateRange<string, (string, string)>(new[]
		{
			// Api group
			new KeyValuePair<string, (string, string)>("ApiVersion", ("Api", "Version")),
			new KeyValuePair<string, (string, string)>("ApiChannel", ("Api", "Channel")),
			new KeyValuePair<string, (string, string)>("Override", ("Api", "Override")),
			new KeyValuePair<string, (string, string)>("Locale", ("Api", "Locale")),
			new KeyValuePair<string, (string, string)>("Timezone", ("Api", "Timezone")),
			// Gateway group
			new KeyValuePair<string, (string, string)>("AutoReconnect", ("Gateway", "AutoReconnect")),
			new KeyValuePair<string, (string, string)>("ReconnectIndefinitely", ("Gateway", "ReconnectIndefinitely")),
			new KeyValuePair<string, (string, string)>("ShardId", ("Gateway", "ShardId")),
			new KeyValuePair<string, (string, string)>("ShardCount", ("Gateway", "ShardCount")),
			new KeyValuePair<string, (string, string)>("GatewayCompressionLevel", ("Gateway", "CompressionLevel")),
			new KeyValuePair<string, (string, string)>("LargeThreshold", ("Gateway", "LargeThreshold")),
			new KeyValuePair<string, (string, string)>("Capabilities", ("Gateway", "Capabilities")),
			new KeyValuePair<string, (string, string)>("MobileStatus", ("Gateway", "MobileStatus")),
			new KeyValuePair<string, (string, string)>("WebSocketClientFactory", ("Gateway", "WebSocketClientFactory")),
			new KeyValuePair<string, (string, string)>("UdpClientFactory", ("Gateway", "UdpClientFactory")),
			// Rest group
			new KeyValuePair<string, (string, string)>("HttpTimeout", ("Rest", "RequestTimeout")),
			new KeyValuePair<string, (string, string)>("UseRelativeRatelimit", ("Rest", "UseRelativeRatelimit")),
			new KeyValuePair<string, (string, string)>("Proxy", ("Rest", "Proxy")),
			// Cache group
			new KeyValuePair<string, (string, string)>("MessageCacheSize", ("Cache", "MessageCacheSize")),
			new KeyValuePair<string, (string, string)>("PresenceCacheSize", ("Cache", "PresenceCacheSize")),
			new KeyValuePair<string, (string, string)>("AlwaysCacheMembers", ("Cache", "AlwaysCacheMembers")),
			new KeyValuePair<string, (string, string)>("AutoRefreshChannelCache", ("Cache", "AutoRefreshChannelCache")),
			new KeyValuePair<string, (string, string)>("AutoFetchApplicationEmojis", ("Cache", "AutoFetchApplicationEmojis")),
			new KeyValuePair<string, (string, string)>("AutoFetchSkuIds", ("Cache", "AutoFetchSkuIds")),
			new KeyValuePair<string, (string, string)>("SkuId", ("Cache", "SkuId")),
			// Logging group
			new KeyValuePair<string, (string, string)>("MinimumLogLevel", ("Logging", "MinimumLogLevel")),
			new KeyValuePair<string, (string, string)>("LogTimestampFormat", ("Logging", "LogTimestampFormat")),
			new KeyValuePair<string, (string, string)>("LoggerFactory", ("Logging", "LoggerFactory")),
			new KeyValuePair<string, (string, string)>("HasShardLogger", ("Logging", "HasShardLogger")),
			// Diagnostics group
			new KeyValuePair<string, (string, string)>("EnablePayloadReceivedEvent", ("Diagnostics", "EnablePayloadReceivedEvent")),
			new KeyValuePair<string, (string, string)>("DisableUpdateCheck", ("Diagnostics.UpdateChecks", "Disabled")),
			new KeyValuePair<string, (string, string)>("UpdateCheckMode", ("Diagnostics.UpdateChecks", "Mode")),
			new KeyValuePair<string, (string, string)>("IncludePrereleaseInUpdateCheck", ("Diagnostics.UpdateChecks", "IncludePrerelease")),
			new KeyValuePair<string, (string, string)>("UpdateCheckGitHubToken", ("Diagnostics.UpdateChecks", "GitHubToken")),
			new KeyValuePair<string, (string, string)>("ShowReleaseNotesInUpdateCheck", ("Diagnostics.UpdateChecks", "ShowReleaseNotes")),
			// Telemetry group
			new KeyValuePair<string, (string, string)>("EnableSentry", ("Telemetry", "EnableSentry")),
			new KeyValuePair<string, (string, string)>("AttachRecentLogEntries", ("Telemetry", "AttachRecentLogEntries")),
			new KeyValuePair<string, (string, string)>("AttachUserInfo", ("Telemetry", "AttachUserInfo")),
			new KeyValuePair<string, (string, string)>("FeedbackEmail", ("Telemetry", "FeedbackEmail")),
			new KeyValuePair<string, (string, string)>("DeveloperUserId", ("Telemetry", "DeveloperUserId")),
			new KeyValuePair<string, (string, string)>("EnableDiscordIdScrubber", ("Telemetry", "EnableDiscordIdScrubber")),
			new KeyValuePair<string, (string, string)>("TrackExceptions", ("Telemetry", "TrackExceptions")),
			new KeyValuePair<string, (string, string)>("EnableLibraryDeveloperMode", ("Telemetry", "EnableLibraryDeveloperMode")),
			new KeyValuePair<string, (string, string)>("DisableScrubber", ("Telemetry", "DisableScrubber")),
			new KeyValuePair<string, (string, string)>("SentryDebug", ("Telemetry", "SentryDebug")),
			new KeyValuePair<string, (string, string)>("DisableExceptionFilter", ("Telemetry", "DisableExceptionFilter")),
			new KeyValuePair<string, (string, string)>("CustomSentryDsn", ("Telemetry", "CustomSentryDsn")),
		});

	internal static bool TryGetMigration(
		SemanticModel semanticModel,
		MemberAccessExpressionSyntax memberAccess,
		CancellationToken cancellationToken,
		out string oldName,
		out string nestedPath,
		out string newName)
	{
		oldName = memberAccess.Name.Identifier.Text;
		nestedPath = null!;
		newName = null!;

		if (!s_propertyMigrations.TryGetValue(oldName, out var migration))
			return false;

		// Verify the containing type is DiscordConfiguration
		var symbolInfo = semanticModel.GetSymbolInfo(memberAccess, cancellationToken);
		var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();
		if (symbol is not IPropertySymbol propertySymbol)
			return false;

		if (propertySymbol.ContainingType?.ToDisplayString() != DiscordConfigurationTypeName)
			return false;

		nestedPath = migration.NestedPath;
		newName = migration.NewName;
		return true;
	}

	internal static bool TryGetMigrationFromInitializer(
		SemanticModel semanticModel,
		IdentifierNameSyntax identifier,
		InitializerExpressionSyntax initializer,
		CancellationToken cancellationToken,
		out string oldName,
		out string nestedPath,
		out string newName)
	{
		oldName = identifier.Identifier.Text;
		nestedPath = null!;
		newName = null!;

		if (!s_propertyMigrations.TryGetValue(oldName, out var migration))
			return false;

		// Verify the initializer belongs to a DiscordConfiguration creation
		if (initializer.Parent is not BaseObjectCreationExpressionSyntax objectCreation)
			return false;

		var typeInfo = semanticModel.GetTypeInfo(objectCreation, cancellationToken);
		if (typeInfo.Type?.ToDisplayString() != DiscordConfigurationTypeName)
			return false;

		nestedPath = migration.NestedPath;
		newName = migration.NewName;
		return true;
	}
}
