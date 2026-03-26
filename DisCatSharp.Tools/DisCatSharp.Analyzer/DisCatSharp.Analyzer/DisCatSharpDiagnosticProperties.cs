using System.Collections.Immutable;

namespace DisCatSharp.Analyzer;

/// <summary>
///     Shared diagnostic property names and builders used by analyzers and code fixes.
/// </summary>
internal static class DisCatSharpDiagnosticProperties
{
	public const string LastKnownOverride = "LastKnownOverride";
	public const string OverrideDate = "OverrideDate";

	public static ImmutableDictionary<string, string?> CreateRequiresOverrideProperties(string overrideValue, string overrideDate)
		=> ImmutableDictionary<string, string?>.Empty
			.Add(LastKnownOverride, overrideValue)
			.Add(OverrideDate, overrideDate);
}
