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

		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration);
		Assert.Equal("SlashCommandErrored", diagnostic.Location.SourceTree?.GetRoot().FindNode(diagnostic.Location.SourceSpan).ToString());
		Assert.Equal("false", diagnostic.Properties[DisCatSharpDiagnosticProperties.MigrationCanAutoFix]);
		Assert.Equal(DisCatSharpDiagnosticProperties.MigrationFixKindManual, diagnostic.Properties[DisCatSharpDiagnosticProperties.MigrationFixKind]);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_extracts_mixed_handler_checks_failed_branch()
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
			        extension.SlashCommandErrored += (sender, args) =>
			        {
			            args.Handled = true;
			            _ = Task.Run(async () =>
			            {
			                if (args.Exception is SlashExecutionChecksFailedException failed)
			                {
			                    _ = failed.FailedChecks;
			                    _ = args.Context;
			                    await Task.CompletedTask;
			                    return;
			                }

			                _ = args.Exception;
			            });
			            return Task.CompletedTask;
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.Contains("extension.SlashCommandErrored +=", fixedSource);
		Assert.Contains("args.Handled = true;", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
		Assert.DoesNotContain("if (args.Exception is SlashExecutionChecksFailedException failed)", fixedSource);
	}

	[Fact]
	public async Task Reports_split_fix_kind_for_mixed_handler_branch_extraction()
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
			        extension.SlashCommandErrored += (sender, args) =>
			        {
			            args.Handled = true;
			            _ = Task.Run(async () =>
			            {
			                if (args.Exception is SlashExecutionChecksFailedException failed)
			                {
			                    _ = failed.FailedChecks;
			                    _ = args.Context;
			                    await Task.CompletedTask;
			                    return;
			                }

			                _ = args.Exception;
			            });
			            return Task.CompletedTask;
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);

		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration);
		Assert.Equal("true", diagnostic.Properties[DisCatSharpDiagnosticProperties.MigrationCanAutoFix]);
		Assert.Equal(DisCatSharpDiagnosticProperties.MigrationFixKindSplit, diagnostic.Properties[DisCatSharpDiagnosticProperties.MigrationFixKind]);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_extracts_realistic_mikusharp_style_branch()
	{
		const string source =
			"""
			using System;
			using System.Linq;
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Attributes;
			using DisCatSharp.ApplicationCommands.Context;
			using DisCatSharp.ApplicationCommands.Exceptions;
			using DisCatSharp.Entities;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += (sender, args) =>
			        {
			            args.Handled = true;
			            _ = Task.Run(async () =>
			            {
			                if (args.Exception is SlashExecutionChecksFailedException ex)
			                {
			                    if (ex.FailedChecks.Any(x => x is ApplicationCommandRequireTeamMemberAttribute))
			                    {
			                        await TryRespondToCommandErrorAsync(args.Context, "This command is limit to my team", DiscordColor.Orange);
			                        return;
			                    }

			                    sender.Client.Logger.LogDebug("Skipping generic slash error response because a command check already handled the interaction.");
			                    return;
			                }

			                if (args.Exception is GuildSafetyCheckException safetyEx)
			                {
			                    await TryRespondToCommandErrorAsync(args.Context, "safety", DiscordColor.Red, safetyEx.Message);
			                    sender.Client.Logger.LogWarning("blocked");
			                    return;
			                }

			                await TryRespondToCommandErrorAsync(args.Context, "generic", DiscordColor.Yellow, args.Exception.Message);
			            });
			            return Task.CompletedTask;
			        };
			    }

			    private static Task TryRespondToCommandErrorAsync(CommandContext context, string message, DiscordColor color, string? details = null)
			        => Task.CompletedTask;
			}

			public sealed class GuildSafetyCheckException : Exception
			{ }

			public sealed class ApplicationCommandRequireTeamMemberAttribute : ApplicationCommandCheckBaseAttribute
			{
			    public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
			        => Task.FromResult(true);
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.Contains("extension.SlashCommandErrored +=", fixedSource);
		Assert.Contains("args.FailedChecks.Any", fixedSource);
		Assert.Contains("GuildSafetyCheckException", fixedSource);
		Assert.DoesNotContain("if (args.Exception is SlashExecutionChecksFailedException ex)", fixedSource);
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
	public async Task Reports_rewrite_fix_kind_for_fully_guarded_handler()
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
		Assert.Equal("true", diagnostic.Properties[DisCatSharpDiagnosticProperties.MigrationCanAutoFix]);
		Assert.Equal(DisCatSharpDiagnosticProperties.MigrationFixKindRewrite, diagnostic.Properties[DisCatSharpDiagnosticProperties.MigrationFixKind]);
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

	[Fact]
	public async Task ApplyFixToDocumentAsync_extracts_context_menu_mixed_handler_branch()
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
			        extension.ContextMenuErrored += (sender, args) =>
			        {
			            args.Handled = true;
			            _ = Task.Run(async () =>
			            {
			                if (args.Exception is ContextMenuExecutionChecksFailedException failed)
			                {
			                    _ = failed.FailedChecks;
			                    _ = args.Context;
			                    await Task.CompletedTask;
			                    return;
			                }

			                _ = args.Exception;
			            });
			            return Task.CompletedTask;
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.ContextMenuChecksFailed +=", fixedSource);
		Assert.Contains("extension.ContextMenuErrored +=", fixedSource);
		Assert.Contains("args.Handled = true;", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
		Assert.DoesNotContain("if (args.Exception is ContextMenuExecutionChecksFailedException failed)", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_preserves_local_helper_calls()
	{
		const string source =
			"""
			using System.Collections.Generic;
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;
			using DisCatSharp.ApplicationCommands.Context;
			using DisCatSharp.ApplicationCommands.EventArgs;
			using DisCatSharp.ApplicationCommands.Attributes;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += async (_, args) =>
			        {
			            if (args.Exception is SlashExecutionChecksFailedException failed)
			            {
			                await HandleChecksAsync(args.Context, failed.FailedChecks);
			            }
			        };
			    }

			    private static Task HandleChecksAsync(CommandContext context, IReadOnlyList<ApplicationCommandCheckBaseAttribute> failedChecks)
			        => Task.CompletedTask;
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.Contains("HandleChecksAsync(args.Context, args.FailedChecks)", fixedSource);
		Assert.DoesNotContain("SlashExecutionChecksFailedException", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_rewrites_negative_guard_handler()
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
			            if (args.Exception is not SlashExecutionChecksFailedException failed)
			                return;

			            _ = args.Context;
			            _ = failed.FailedChecks;
			            await Task.CompletedTask;
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.DoesNotContain("SlashCommandErrored", fixedSource);
		Assert.DoesNotContain("is not SlashExecutionChecksFailedException", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_rewrites_filtered_logical_and_guard()
	{
		const string source =
			"""
			using System.Linq;
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Attributes;
			using DisCatSharp.ApplicationCommands.Context;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += async (_, args) =>
			        {
			            if (args.Exception is SlashExecutionChecksFailedException failed &&
			                failed.FailedChecks.Any(x => x is ApplicationCommandRequireDirectMessagesAttribute))
			            {
			                await HandleChecksAsync(args.Context);
			            }
			        };
			    }

			    private static Task HandleChecksAsync(CommandContext context)
			        => Task.CompletedTask;
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.Contains("if (args.FailedChecks.Any", fixedSource);
		Assert.Contains("HandleChecksAsync(args.Context)", fixedSource);
		Assert.DoesNotContain("SlashExecutionChecksFailedException failed &&", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_rewrites_top_level_switch_handler()
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
			            switch (args.Exception)
			            {
			                case SlashExecutionChecksFailedException failed:
			                    _ = args.Context;
			                    _ = failed.FailedChecks;
			                    await Task.CompletedTask;
			                    break;
			            }
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.DoesNotContain("SlashCommandErrored", fixedSource);
		Assert.DoesNotContain("switch (args.Exception)", fixedSource);
		Assert.DoesNotContain("break;", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_extracts_switch_case_from_mixed_handler()
	{
		const string source =
			"""
			using System;
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.ContextMenuErrored += async (_, args) =>
			        {
			            switch (args.Exception)
			            {
			                case ContextMenuExecutionChecksFailedException failed:
			                    _ = args.Context;
			                    _ = failed.FailedChecks;
			                    await Task.CompletedTask;
			                    break;
			                case InvalidOperationException:
			                    _ = args.Exception.Message;
			                    break;
			            }
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.ContextMenuChecksFailed +=", fixedSource);
		Assert.Contains("extension.ContextMenuErrored +=", fixedSource);
		Assert.Contains("switch (args.Exception)", fixedSource);
		Assert.DoesNotContain("case ContextMenuExecutionChecksFailedException failed:", fixedSource);
		Assert.Contains("case InvalidOperationException:", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_extracts_nested_switch_case_through_task_run_wrapper()
	{
		const string source =
			"""
			using System;
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.SlashCommandErrored += (sender, args) =>
			        {
			            args.Handled = true;
			            _ = Task.Run(async () =>
			            {
			                switch (args.Exception)
			                {
			                    case SlashExecutionChecksFailedException failed:
			                        _ = args.Context;
			                        _ = failed.FailedChecks;
			                        await Task.CompletedTask;
			                        break;
			                    case InvalidOperationException:
			                        _ = args.Exception.Message;
			                        break;
			                }
			            });
			            return Task.CompletedTask;
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.SlashCommandChecksFailed +=", fixedSource);
		Assert.Contains("extension.SlashCommandErrored +=", fixedSource);
		Assert.Contains("args.Handled = true;", fixedSource);
		Assert.Contains("_ = Task.Run(async () =>", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
		Assert.DoesNotContain("case SlashExecutionChecksFailedException failed:", fixedSource);
		Assert.Contains("case InvalidOperationException:", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_extracts_nested_context_menu_switch_case_through_task_run_wrapper()
	{
		const string source =
			"""
			using System;
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.ContextMenuErrored += (sender, args) =>
			        {
			            args.Handled = true;
			            _ = Task.Run(async () =>
			            {
			                switch (args.Exception)
			                {
			                    case ContextMenuExecutionChecksFailedException failed:
			                        _ = args.Context;
			                        _ = failed.FailedChecks;
			                        await Task.CompletedTask;
			                        break;
			                    case InvalidOperationException:
			                        _ = args.Exception.Message;
			                        break;
			                }
			            });
			            return Task.CompletedTask;
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.ContextMenuChecksFailed +=", fixedSource);
		Assert.Contains("extension.ContextMenuErrored +=", fixedSource);
		Assert.Contains("args.Handled = true;", fixedSource);
		Assert.Contains("_ = Task.Run(async () =>", fixedSource);
		Assert.Contains("args.FailedChecks", fixedSource);
		Assert.DoesNotContain("case ContextMenuExecutionChecksFailedException failed:", fixedSource);
		Assert.Contains("case InvalidOperationException:", fixedSource);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_rewrites_switch_when_failed_checks_filter()
	{
		const string source =
			"""
			using System.Linq;
			using System.Threading.Tasks;
			using DisCatSharp.ApplicationCommands;
			using DisCatSharp.ApplicationCommands.Attributes;
			using DisCatSharp.ApplicationCommands.Context;
			using DisCatSharp.ApplicationCommands.Exceptions;

			public sealed class Consumer
			{
			    public void Configure(ApplicationCommandsExtension extension)
			    {
			        extension.ContextMenuErrored += async (_, args) =>
			        {
			            switch (args.Exception)
			            {
			                case ContextMenuExecutionChecksFailedException failed when failed.FailedChecks.Any(x => x is ApplicationCommandRequireGuildAttribute):
			                    await HandleChecksAsync(args.Context);
			                    break;
			            }
			        };
			    }

			    private static Task HandleChecksAsync(ContextMenuContext context)
			        => Task.CompletedTask;
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyApplicationCommandChecksFailedMigrationFixAsync(source);

		Assert.Contains("extension.ContextMenuChecksFailed +=", fixedSource);
		Assert.Contains("if (args.FailedChecks.Any", fixedSource);
		Assert.Contains("HandleChecksAsync(args.Context)", fixedSource);
		Assert.DoesNotContain("when failed.FailedChecks.Any", fixedSource);
	}
}
