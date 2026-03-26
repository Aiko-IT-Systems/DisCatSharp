namespace DisCatSharp.Analyzer.Tests;

public sealed class PresenceAccessMigrationTests
{
	[Fact]
	public async Task Reports_diagnostic_for_values_where_userid_pattern()
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

		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresenceAccessMigration);
		Assert.Equal("Presences", diagnostic.Location.SourceTree?.GetRoot().FindNode(diagnostic.Location.SourceSpan).ToString());
	}

	[Fact]
	public async Task Reports_diagnostic_for_keyvalue_select_pattern()
	{
		const string source =
			"""
			using System.Linq;
			using DisCatSharp;

			public sealed class Consumer
			{
			    public object GetPresences(DiscordClient client, ulong userId)
			        => client.Presences.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Value);
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		Assert.Contains(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresenceAccessMigration);
	}

	[Fact]
	public async Task Does_not_report_diagnostic_for_key_based_filter()
	{
		const string source =
			"""
			using System.Linq;
			using DisCatSharp;

			public sealed class Consumer
			{
			    public object GetPresences(DiscordClient client, ulong guildId)
			        => client.Presences.Where(kvp => kvp.Key == guildId).Select(kvp => kvp.Value);
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresenceAccessMigration);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_rewrites_values_where_pattern()
	{
		const string source =
			"""
			using System.Linq;
			using DisCatSharp;

			public sealed class Consumer
			{
			    public object GetPresences(DiscordClient client, ulong userId)
			        => client.Presences.Values.Where(p => p.UserId == userId).ToList();
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyPresenceAccessMigrationFixAsync(source);

		Assert.Contains("client.GetPresences(userId).Values.ToList()", fixedSource);
		Assert.DoesNotContain("Presences.Values.Where", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_rewrites_keyvalue_select_pattern()
	{
		const string source =
			"""
			using System.Linq;
			using DisCatSharp;

			public sealed class Consumer
			{
			    public object GetPresences(DiscordClient client, ulong userId)
			        => client.Presences.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Value);
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyPresenceAccessMigrationFixAsync(source);

		Assert.Contains("client.GetPresences(userId).Values", fixedSource);
		Assert.DoesNotContain(".Where(kvp => kvp.Value.UserId == userId)", fixedSource);
		Assert.DoesNotContain(".Select(kvp => kvp.Value)", fixedSource);
	}
}
