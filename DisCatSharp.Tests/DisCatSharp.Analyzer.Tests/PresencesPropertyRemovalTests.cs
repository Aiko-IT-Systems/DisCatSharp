using Microsoft.CodeAnalysis;

namespace DisCatSharp.Analyzer.Tests;

public sealed class PresencesPropertyRemovalTests
{
	[Fact]
	public async Task Reports_diagnostic_for_bare_presences_access()
	{
		const string source =
			"""
			using DisCatSharp;

			public sealed class Consumer
			{
				public object GetAll(DiscordClient client)
					=> client.Presences;
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresencesPropertyRemoval);
		Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
	}

	[Fact]
	public async Task Reports_diagnostic_for_presences_indexer_access()
	{
		const string source =
			"""
			using DisCatSharp;

			public sealed class Consumer
			{
				public object GetOne(DiscordClient client, ulong userId)
					=> client.Presences[userId];
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		Assert.Contains(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresencesPropertyRemoval);
	}

	[Fact]
	public async Task Reports_diagnostic_for_presences_linq_pattern()
	{
		const string source =
			"""
			using System.Linq;
			using DisCatSharp;

			public sealed class Consumer
			{
				public object GetPresences(DiscordClient client, ulong userId)
					=> client.Presences.Values.Where(p => p.UserId == userId);
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		Assert.Contains(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresencesPropertyRemoval);
	}

	[Fact]
	public async Task Does_not_report_diagnostic_for_guild_presences()
	{
		const string source =
			"""
			using DisCatSharp.Entities;

			public sealed class Consumer
			{
				public object GetGuildPresences(DiscordGuild guild)
					=> guild.Presences;
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresencesPropertyRemoval);
	}
}
