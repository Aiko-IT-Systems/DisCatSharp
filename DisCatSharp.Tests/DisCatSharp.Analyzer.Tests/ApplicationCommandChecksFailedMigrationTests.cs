namespace DisCatSharp.Analyzer.Tests;

public sealed class ApplicationCommandChecksFailedMigrationTests
{
	[Fact]
	public async Task Reports_diagnostic_for_slash_command_check_failure_guard()
	{
		const string source =
			"""
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += async (_, args) =>
			        {
			            if (args.Exception is SlashExecutionChecksFailedException failed)
			            {
			                _ = args.Context;
			                _ = failed.FailedChecks;
			            }
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration);
		Assert.Equal("SlashCommandErrored", diagnostic.Location.SourceTree?.GetRoot().FindNode(diagnostic.Location.SourceSpan).ToString());
	}

	[Fact]
	public async Task Does_not_report_diagnostic_when_handler_uses_exception_directly()
	{
		const string source =
			"""
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += async (_, args) =>
			        {
			            if (args.Exception is SlashExecutionChecksFailedException failed)
			            {
			                _ = failed.Message;
			            }
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration);
	}

	[Fact]
	public async Task Does_not_report_diagnostic_when_handler_has_trailing_statements_outside_guard()
	{
		const string source =
			"""
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += async (_, args) =>
			        {
			            if (args.Exception is SlashExecutionChecksFailedException failed)
			            {
			                _ = args.Context;
			                _ = failed.FailedChecks;
			            }

			            _ = args.Exception;
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_rewrites_slash_command_handler_to_checks_failed_event()
	{
		const string source =
			"""
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += async (_, args) =>
			        {
			            if (args.Exception is SlashExecutionChecksFailedException failed)
			            {
			                _ = args.Context;
			                _ = failed.FailedChecks;
			            }
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.DoesNotContain("SlashCommandErrored", fixedSource);
		Assert.DoesNotContain("SlashExecutionChecksFailedException", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(fixedSource);
		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_updates_typed_context_menu_event_args()
	{
		const string source =
			"""
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.EventArgs;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.ContextMenuErrored += async (ApplicationCommandsExtension _, ContextMenuErrorEventArgs args) =>
			        {
			            if (args.Exception is ContextMenuExecutionChecksFailedException failed)
			            {
			                _ = args.Context;
			                _ = failed.FailedChecks;
			                await Task.CompletedTask;
			            }
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.ContextMenuChecksFailed +=", fixedSource);
		Assert.Contains("global::DisCatSharp.ApplicationCommands.EventArgs.ContextMenuChecksFailedEventArgs args", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
		Assert.DoesNotContain("ContextMenuExecutionChecksFailedException", fixedSource);
	}
}
