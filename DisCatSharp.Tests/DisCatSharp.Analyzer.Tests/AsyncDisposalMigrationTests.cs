using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DisCatSharp.Analyzer.Tests;

public sealed class AsyncDisposalMigrationTests
{
	#region DCS1301 — using → await using

	[Fact]
	public async Task DCS1301_Reports_on_using_var_with_DiscordClient()
	{
		const string source = """
			using DisCatSharp;
			using DisCatSharp.Enums;

			class Test
			{
				void Run()
				{
					using var client = new DiscordClient(new DiscordConfiguration
					{
						Token = "t",
						TokenType = TokenType.Bot
					});
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalUsingMigration);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Contains("DiscordClient", diagnostic.GetMessage());
	}

	[Fact]
	public async Task DCS1301_Reports_on_using_statement_with_DiscordClient()
	{
		const string source = """
			using DisCatSharp;
			using DisCatSharp.Enums;

			class Test
			{
				void Run()
				{
					using (var client = new DiscordClient(new DiscordConfiguration
					{
						Token = "t",
						TokenType = TokenType.Bot
					}))
					{
					}
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalUsingMigration);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
	}

	[Fact]
	public async Task DCS1301_No_diagnostic_on_await_using_var()
	{
		const string source = """
			using System.Threading.Tasks;
			using DisCatSharp;
			using DisCatSharp.Enums;

			class Test
			{
				async Task RunAsync()
				{
					await using var client = new DiscordClient(new DiscordConfiguration
					{
						Token = "t",
						TokenType = TokenType.Bot
					});
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.DoesNotContain(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalUsingMigration);
	}

	[Fact]
	public async Task DCS1301_No_diagnostic_on_non_discatsharp_type()
	{
		// System.IO.MemoryStream implements IDisposable but NOT IAsyncDisposable from DisCatSharp
		const string source = """
			using System.IO;

			class Test
			{
				void Run()
				{
					using var stream = new MemoryStream();
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.DoesNotContain(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalUsingMigration);
	}

	[Fact]
	public async Task DCS1301_Reports_on_DiscordWebhookClient()
	{
		const string source = """
			using DisCatSharp;

			class Test
			{
				void Run()
				{
					using var client = new DiscordWebhookClient();
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalUsingMigration);
		Assert.Contains("DiscordWebhookClient", diagnostic.GetMessage());
	}

	#endregion

	#region DCS1302 — Dispose() → DisposeAsync()

	[Fact]
	public async Task DCS1302_Reports_on_Dispose_call()
	{
		const string source = """
			using DisCatSharp;
			using DisCatSharp.Enums;

			class Test
			{
				void Run()
				{
					var client = new DiscordClient(new DiscordConfiguration
					{
						Token = "t",
						TokenType = TokenType.Bot
					});
					client.Dispose();
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalDisposeMigration);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Contains("client", diagnostic.GetMessage());
	}

	[Fact]
	public async Task DCS1302_No_diagnostic_on_non_discatsharp_Dispose()
	{
		const string source = """
			using System.IO;

			class Test
			{
				void Run()
				{
					var stream = new MemoryStream();
					stream.Dispose();
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.DoesNotContain(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalDisposeMigration);
	}

	[Fact]
	public async Task DCS1302_Reports_on_DiscordWebhookClient_Dispose()
	{
		const string source = """
			using DisCatSharp;

			class Test
			{
				void Run()
				{
					var client = new DiscordWebhookClient();
					client.Dispose();
				}
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, d => d.Id == DisCatSharpDiagnosticIds.AsyncDisposalDisposeMigration);
		Assert.Contains("client", diagnostic.GetMessage());
	}

	#endregion

	#region Code fix verification

	[Fact]
	public async Task DCS1301_Fix_converts_using_to_await_using()
	{
		const string source = """
			using System.Threading.Tasks;
			using DisCatSharp;
			using DisCatSharp.Enums;

			class Test
			{
				async Task RunAsync()
				{
					using var client = new DiscordClient(new DiscordConfiguration
					{
						Token = "t",
						TokenType = TokenType.Bot
					});
				}
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyAsyncDisposalUsingFixAsync(source);
		Assert.Contains("await using", fixedSource);
		Assert.DoesNotContain("using using", fixedSource);
	}

	[Fact]
	public async Task DCS1302_Fix_converts_Dispose_to_DisposeAsync()
	{
		const string source = """
			using System.Threading.Tasks;
			using DisCatSharp;
			using DisCatSharp.Enums;

			class Test
			{
				async Task RunAsync()
				{
					var client = new DiscordClient(new DiscordConfiguration
					{
						Token = "t",
						TokenType = TokenType.Bot
					});
					client.Dispose();
				}
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyAsyncDisposalDisposeFixAsync(source);
		Assert.Contains("DisposeAsync", fixedSource);
		Assert.Contains("await", fixedSource);
		Assert.DoesNotContain("client.Dispose()", fixedSource);
	}

	#endregion
}
