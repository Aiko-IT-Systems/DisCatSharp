using Microsoft.CodeAnalysis;

namespace DisCatSharp.Analyzer.Tests;

public sealed class BanDeleteMessageDaysMigrationTests
{
	[Fact]
	public async Task Reports_diagnostic_on_named_argument()
	{
		const string source =
		"""
using System.Threading.Tasks;
using DisCatSharp.Entities;

public sealed class Consumer
{
    public async Task BanUser(DiscordMember member)
        => await member.BanAsync(deleteMessageDays: 7);
}
""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.BanDeleteMessageDaysMigration);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
	}

	[Fact]
	public async Task Fix_converts_literal_days_to_seconds()
	{
		const string source =
		"""
using System.Threading.Tasks;
using DisCatSharp.Entities;

public sealed class Consumer
{
    public async Task BanUser(DiscordMember member)
        => await member.BanAsync(deleteMessageDays: 7);
}
""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyBanDeleteMessageDaysMigrationFixAsync(source);

		Assert.Contains("deleteMessageSeconds: 604800", fixedSource);
		Assert.DoesNotContain("deleteMessageDays", fixedSource);
	}

	[Fact]
	public async Task Fix_renames_non_literal_argument()
	{
		const string source =
		"""
using System.Threading.Tasks;
using DisCatSharp.Entities;

public sealed class Consumer
{
    public async Task BanUser(DiscordMember member, int days)
        => await member.BanAsync(deleteMessageDays: days);
}
""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyBanDeleteMessageDaysMigrationFixAsync(source);

		Assert.Contains("deleteMessageSeconds:", fixedSource);
		Assert.Contains("TODO", fixedSource);
		Assert.DoesNotContain("deleteMessageDays:", fixedSource);
	}
}
