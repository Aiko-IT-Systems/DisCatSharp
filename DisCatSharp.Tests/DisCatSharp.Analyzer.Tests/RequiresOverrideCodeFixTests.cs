using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace DisCatSharp.Analyzer.Tests;

public sealed class RequiresOverrideCodeFixTests
{
	private static readonly DiagnosticDescriptor s_descriptor = new(
		"TEST001",
		"Test",
		"Test",
		"Testing",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	[Fact]
	public void SelectLatestOverrideValue_prefers_newest_date()
	{
		var diagnostics = ImmutableArray.Create(
			CreateDiagnostic("older-override", "21/03/2025"),
			CreateDiagnostic("newer-override", "22/03/2025"));

		var overrideValue = DisCatSharpRequiresOverrideCodeFix.SelectLatestOverrideValue(diagnostics);

		Assert.Equal("newer-override", overrideValue);
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_adds_override_to_explicit_configuration()
	{
		const string source =
			"""
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var client = new DiscordClient(new DiscordConfiguration
			        {
			            Token = string.Empty
			        });
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyRequiresOverrideFixAsync(source, "explicit-override");

		Assert.Contains("Override = \"explicit-override\"", fixedSource);
		Assert.Equal(1, CountOccurrences(fixedSource, "Override = \"explicit-override\""));
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_adds_override_to_target_typed_configuration()
	{
		const string source =
			"""
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var client = new DiscordClient(new()
			        {
			            Token = string.Empty
			        });
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyRequiresOverrideFixAsync(source, "implicit-override");

		Assert.Contains("Override = \"implicit-override\"", fixedSource);
		Assert.Equal(1, CountOccurrences(fixedSource, "Override = \"implicit-override\""));
	}

	[Fact]
	public async Task ApplyFixToDocumentAsync_does_not_duplicate_existing_override()
	{
		const string source =
			"""
			using DisCatSharp;

			public sealed class Consumer
			{
			    public void Test()
			    {
			        var client = new DiscordClient(new DiscordConfiguration
			        {
			            Token = string.Empty,
			            Override = "existing-override"
			        });
			    }
			}
			""";

		var fixedSource = await RoslynTestDocumentFactory.ApplyRequiresOverrideFixAsync(source, "new-override");

		Assert.Contains("Override = \"existing-override\"", fixedSource);
		Assert.Equal(1, CountOccurrences(fixedSource, "Override = "));
	}

	[Fact]
	public async Task ApplyFixToProjectAsync_updates_configuration_in_different_document()
	{
		var sources = ImmutableDictionary<string, string>.Empty
			.Add(
				"Consumer.cs",
				"""
				using DisCatSharp.Attributes;

				public sealed class OverrideDependency
				{
				    [RequiresOverride("cross-file-override", "21/03/2025")]
				    public string Profile => "value";
				}

				public sealed class Consumer
				{
				    public void Test(OverrideDependency dependency)
				    {
				        _ = dependency.Profile;
				    }
				}
				""")
			.Add(
				"ConfigurationFactory.cs",
				"""
				using DisCatSharp;

				public static class ConfigurationFactory
				{
				    public static DiscordClient CreateClient()
				        => new(new DiscordConfiguration
				        {
				            Token = string.Empty
				        });
				}
				""");

		var fixedSources = await RoslynTestDocumentFactory.ApplyRequiresOverrideFixAsync(sources, "Consumer.cs", "cross-file-override");

		Assert.DoesNotContain("Override = ", fixedSources["Consumer.cs"]);
		Assert.Contains("Override = \"cross-file-override\"", fixedSources["ConfigurationFactory.cs"]);
		Assert.Equal(1, CountOccurrences(fixedSources["ConfigurationFactory.cs"], "Override = \"cross-file-override\""));
	}

	private static Diagnostic CreateDiagnostic(string overrideValue, string overrideDate)
	{
		var properties = ImmutableDictionary<string, string?>.Empty
			.Add("LastKnownOverride", overrideValue)
			.Add("OverrideDate", overrideDate);

		return Diagnostic.Create(s_descriptor, Location.None, properties);
	}

	private static int CountOccurrences(string source, string value)
	{
		var count = 0;
		var startIndex = 0;

		while ((startIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal)) >= 0)
		{
			count++;
			startIndex += value.Length;
		}

		return count;
	}
}
