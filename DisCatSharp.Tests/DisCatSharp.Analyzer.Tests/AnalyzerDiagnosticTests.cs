namespace DisCatSharp.Analyzer.Tests;

public sealed class AnalyzerDiagnosticTests
{
	public static TheoryData<string, string> AttributeDiagnosticSources
		=> new()
		{
			{
				"""
				using DisCatSharp.Attributes;

				public sealed class TestType
				{
				    [Experimental("This is experimental.")]
				    public void TestMethod() { }
				}
				""",
				"DCS0001"
			},
			{
				"""
				using DisCatSharp.Attributes;

				public sealed class TestType
				{
				    [Deprecated("Use something else.")]
				    public void TestMethod() { }
				}
				""",
				"DCS0002"
			},
			{
				"""
				using DisCatSharp.Attributes;

				public sealed class TestType
				{
				    [DiscordInExperiment("This is in experiment.")]
				    public int Value => 1;
				}
				""",
				"DCS0101"
			},
			{
				"""
				using DisCatSharp.Attributes;

				public sealed class TestType
				{
				    [DiscordDeprecated("This is deprecated by Discord.")]
				    public int Value => 1;
				}
				""",
				"DCS0102"
			},
			{
				"""
				using DisCatSharp.Attributes;

				public sealed class TestType
				{
				    [DiscordUnreleased("This is not released yet.")]
				    public int Value => 1;
				}
				""",
				"DCS0103"
			},
			{
				"""
				using DisCatSharp.Attributes;

				public sealed class TestType
				{
				    [RequiresFeature(Features.Community)]
				    public void TestMethod() { }
				}
				""",
				"DCS0200"
			}
		};

	[Theory]
	[MemberData(nameof(AttributeDiagnosticSources))]
	public async Task Reports_expected_attribute_diagnostic(string source, string diagnosticId)
	{
		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.Contains(diagnostics, x => x.Id == diagnosticId);
	}

	[Fact]
	public async Task Reports_requires_override_diagnostic_when_consuming_annotated_member()
	{
		const string source =
			"""
			using DisCatSharp;
			using DisCatSharp.Attributes;

			public sealed class OverrideDependency
			{
			    [RequiresOverride("new-override", "21/03/2025")]
			    public string Profile => "value";
			}

			public sealed class Consumer
			{
			    public void Test(OverrideDependency dependency)
			    {
			        var client = new DiscordClient(new DiscordConfiguration
			        {
			            Token = string.Empty
			        });

			        _ = dependency.Profile;
			    }
			}
			""";

		var diagnostics = await RoslynTestDocumentFactory.GetAnalyzerDiagnosticsAsync(source);
		Assert.Contains(diagnostics, x => x.Id == "DCS0201");
	}
}
