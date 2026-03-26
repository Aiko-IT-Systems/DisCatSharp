namespace DisCatSharp.Analyzer;

/// <summary>
///     Centralized identifiers for DisCatSharp analyzer diagnostics.
/// </summary>
internal static class DisCatSharpDiagnosticIds
{
	public const string Experimental = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "0001";
	public const string Deprecated = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "0002";
	public const string DiscordInExperiment = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "0101";
	public const string DiscordDeprecated = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "0102";
	public const string DiscordUnreleased = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "0103";
	public const string PresenceAccessMigration = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "1101";
	public const string RequiresFeature = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "0200";
	public const string RequiresOverride = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "0201";
	public const string ApplicationCommandChecksFailedMigration = DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX + "2101";
}
