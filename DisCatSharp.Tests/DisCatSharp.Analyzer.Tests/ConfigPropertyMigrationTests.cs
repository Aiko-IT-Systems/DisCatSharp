using Microsoft.CodeAnalysis;

namespace DisCatSharp.Analyzer.Tests;

public sealed class ConfigPropertyMigrationTests
{
	[Fact]
	public async Task Reports_diagnostic_on_direct_property_read()
	{
		const string source = """
			#pragma warning disable CS0618
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        var version = config.ApiVersion;
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Contains("Api.Version", diagnostic.GetMessage());
	}

	[Fact]
	public async Task Reports_diagnostic_on_property_assignment()
	{
		const string source = """
			#pragma warning disable CS0618
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        config.ShardCount = 4;
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
		Assert.Contains("Gateway.ShardCount", diagnostic.GetMessage());
	}

	[Fact]
	public async Task Reports_diagnostic_for_renamed_property()
	{
		const string source = """
			#pragma warning disable CS0618
			using System;
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        var timeout = config.HttpTimeout;
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
		Assert.Contains("Rest.RequestTimeout", diagnostic.GetMessage());
	}

	[Fact]
	public async Task Reports_diagnostic_for_deep_nested_property()
	{
		const string source = """
			#pragma warning disable CS0618
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        var mode = config.UpdateCheckMode;
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
		Assert.Contains("Diagnostics.UpdateChecks.Mode", diagnostic.GetMessage());
	}

	[Fact]
	public async Task Does_not_report_on_new_nested_path()
	{
		const string source = """
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        var version = config.Api.Version;
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
	}

	[Fact]
	public async Task Does_not_report_on_root_keepers()
	{
		const string source = """
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        var token = config.Token;
			        var intents = config.Intents;
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
	}

	[Fact]
	public async Task Code_fix_replaces_simple_property_access()
	{
		const string source = """
			#pragma warning disable CS0618
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        var version = config.ApiVersion;
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyConfigPropertyMigrationFixAsync(source);
		Assert.Contains("config.Api.Version", fixedSource);
		Assert.DoesNotContain("config.ApiVersion", fixedSource);
	}

	[Fact]
	public async Task Code_fix_replaces_deep_nested_property_access()
	{
		const string source = """
			#pragma warning disable CS0618
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test(DiscordConfiguration config)
			    {
			        var disabled = config.DisableUpdateCheck;
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyConfigPropertyMigrationFixAsync(source);
		Assert.Contains("config.Diagnostics.UpdateChecks.Disabled", fixedSource);
		Assert.DoesNotContain("config.DisableUpdateCheck", fixedSource);
	}

	[Fact]
	public async Task Reports_diagnostic_on_initializer_property()
	{
		const string source = """
			#pragma warning disable DCS0002
			using DisCatSharp;
			using DisCatSharp.Enums;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var cfg = new DiscordConfiguration
			        {
			            ApiChannel = ApiChannel.Canary,
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
		Assert.Contains("Api.Channel", diagnostic.GetMessage());
	}

	[Fact]
	public async Task Reports_diagnostic_on_implicit_new_initializer()
	{
		const string source = """
			#pragma warning disable DCS0002
			using DisCatSharp;

			public sealed class Consumer
			{
			    public DiscordConfiguration Create()
			    {
			        return new()
			        {
			            ReconnectIndefinitely = true,
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
		Assert.Contains("Gateway.ReconnectIndefinitely", diagnostic.GetMessage());
	}

	[Fact]
	public async Task Reports_diagnostic_on_deep_nested_initializer_property()
	{
		const string source = """
			#pragma warning disable DCS0002
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var cfg = new DiscordConfiguration
			        {
			            DisableUpdateCheck = true,
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
		Assert.Contains("Diagnostics.UpdateChecks.Disabled", diagnostic.GetMessage());
	}

	[Fact]
	public async Task Does_not_report_on_root_keeper_in_initializer()
	{
		const string source = """
			using DisCatSharp;
			using DisCatSharp.Enums;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var cfg = new DiscordConfiguration
			        {
			            Token = "test",
			            TokenType = TokenType.Bot,
			        };
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.DoesNotContain(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ConfigPropertyMigration);
	}

	[Fact]
	public async Task Code_fix_replaces_initializer_property()
	{
		const string source = """
			#pragma warning disable DCS0002
			using DisCatSharp;
			using DisCatSharp.Enums;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var cfg = new DiscordConfiguration
			        {
			            ApiChannel = ApiChannel.Canary,
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyConfigPropertyMigrationFixAsync(source);
		Assert.Contains("Api =", fixedSource);
		Assert.Contains("Channel = ApiChannel.Canary", fixedSource);
		Assert.DoesNotContain("ApiChannel = ApiChannel.Canary", fixedSource);
	}

	[Fact]
	public async Task Code_fix_replaces_deep_nested_initializer_property()
	{
		const string source = """
			#pragma warning disable DCS0002
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var cfg = new DiscordConfiguration
			        {
			            DisableUpdateCheck = true,
			        };
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyConfigPropertyMigrationFixAsync(source);
		Assert.Contains("Diagnostics =", fixedSource);
		Assert.Contains("UpdateChecks =", fixedSource);
		Assert.Contains("Disabled = true", fixedSource);
		Assert.DoesNotContain("DisableUpdateCheck", fixedSource);
	}
}
