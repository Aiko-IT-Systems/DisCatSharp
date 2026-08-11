using System.Text.Json;
using DisCatSharp.Docs.SearchIndexer;
using Xunit;

namespace DisCatSharp.Docs.SearchIndexer.Tests;

public sealed class SearchIndexBuilderTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), $"dcs-search-indexer-{Guid.NewGuid():N}");

	[Fact]
	public async Task ConceptualCorpusComesFromManifestAndPreservesEveryClassification()
	{
		var docs = Path.Combine(this._root, "docs");
		var site = Path.Combine(docs, "_site");
		Directory.CreateDirectory(Path.Combine(docs, "future", "nested"));
		Directory.CreateDirectory(Path.Combine(docs, "changelogs"));
		Directory.CreateDirectory(Path.Combine(docs, "api", "DisCatSharp.Voice"));
		Directory.CreateDirectory(site);
		await File.WriteAllTextAsync(Path.Combine(docs, "index.md"), "---\ntitle: Home\n---\n\nRoot documentation.");
		await File.WriteAllTextAsync(Path.Combine(docs, "future", "nested", "page.md"), "# Future Page\n\nAutomatically discovered.");
		await File.WriteAllTextAsync(Path.Combine(docs, "changelogs", "v1.md"), "# Version One\n\nChanges.");
		await File.WriteAllTextAsync(Path.Combine(docs, "api", "index.md"), "# API\n\nAll modules.");
		await File.WriteAllTextAsync(Path.Combine(docs, "api", "DisCatSharp.Voice", "index.md"), "# Voice API\n\nVoice module.");
		await File.WriteAllTextAsync(Path.Combine(docs, "excluded.md"), "# Not built");
		await File.WriteAllTextAsync(Path.Combine(site, "xrefmap.yml"), "references: []\n");
		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new object[]
			{
				Conceptual("index.md", "index.html"),
				Conceptual("future/nested/page.md", "future/nested/page.html"),
				Conceptual("changelogs/v1.md", "changelogs/v1.html"),
				Conceptual("api/index.md", "api/index.html"),
				Conceptual("api/DisCatSharp.Voice/index.md", "api/DisCatSharp.Voice/index.html")
			}
		}));

		var artifact = await new SearchIndexBuilder(this._root, docs, site).BuildAsync("test");

		Assert.Equal(5, artifact.Counts.ConceptualManifestItems);
		Assert.Equal(5, artifact.Documents.Count);
		Assert.Equal(["/api/DisCatSharp.Voice/index.html", "/api/index.html", "/changelogs/v1.html", "/future/nested/page.html", "/index.html"], artifact.Documents.Select(document => document.Url).Order().ToArray());
		Assert.DoesNotContain(artifact.Documents, document => document.SourcePath == "excluded.md");
		Assert.Equal("article", artifact.Documents.Single(document => document.SourcePath == "future/nested/page.md").Kind);
		Assert.Equal("changelog", artifact.Documents.Single(document => document.SourcePath == "changelogs/v1.md").Kind);
		Assert.Equal("document:home", artifact.Documents.Single(document => document.SourcePath == "index.md").Id);
		Assert.Equal("document:future/nested/page", artifact.Documents.Single(document => document.SourcePath == "future/nested/page.md").Id);
		Assert.Equal("document:changelogs/v1", artifact.Documents.Single(document => document.SourcePath == "changelogs/v1.md").Id);
		Assert.Equal("DisCatSharp.Voice", artifact.Documents.Single(document => document.SourcePath == "api/DisCatSharp.Voice/index.md").Module);
		Assert.Null(artifact.Documents.Single(document => document.SourcePath == "api/index.md").Module);
		Assert.All(artifact.Documents, document =>
		{
			Assert.Equal("conceptual", document.Family);
			Assert.StartsWith("document:", document.Id);
		});

		var rebuilt = await new SearchIndexBuilder(this._root, docs, site).BuildAsync("test");
		Assert.Equal(artifact.Documents.Select(document => (document.Id, document.ContentHash)), rebuilt.Documents.Select(document => (document.Id, document.ContentHash)));
	}

	[Fact]
	public async Task ConceptualIdentityDependsOnCanonicalUrlAndNotClassification()
	{
		var docs = Path.Combine(this._root, "docs");
		var site = Path.Combine(docs, "_site");
		Directory.CreateDirectory(Path.Combine(docs, "articles"));
		Directory.CreateDirectory(Path.Combine(docs, "changelogs"));
		Directory.CreateDirectory(site);
		await File.WriteAllTextAsync(Path.Combine(docs, "articles", "release.md"), "# Release");
		await File.WriteAllTextAsync(Path.Combine(docs, "changelogs", "release.md"), "# Release");
		await File.WriteAllTextAsync(Path.Combine(site, "xrefmap.yml"), "references: []\n");

		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new[] { Conceptual("articles/release.md", "release/index.html") }
		}));
		var article = Assert.Single((await new SearchIndexBuilder(this._root, docs, site).BuildAsync("test")).Documents);

		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new[] { Conceptual("changelogs/release.md", "release/index.html") }
		}));
		var changelog = Assert.Single((await new SearchIndexBuilder(this._root, docs, site).BuildAsync("test")).Documents);

		Assert.Equal("article", article.Kind);
		Assert.Equal("changelog", changelog.Kind);
		Assert.Equal("document:release", article.Id);
		Assert.Equal(article.Id, changelog.Id);
		Assert.Equal(article.DocumentKey, changelog.DocumentKey);
		Assert.NotEqual(article.ContentHash, changelog.ContentHash);
	}

	[Fact]
	public async Task ManagedReferencesPreserveKindsOverloadsAndCanonicalXrefUrls()
	{
		var docs = Path.Combine(this._root, "docs");
		var site = Path.Combine(docs, "_site");
		var api = Path.Combine(docs, "api");
		Directory.CreateDirectory(site);
		Directory.CreateDirectory(api);
		await File.WriteAllTextAsync(Path.Combine(api, "Widget.yml"), """
			items:
			- uid: Library.Widget
			  id: Widget
			  name: Widget
			  nameWithType: Widget
			  fullName: Library.Widget
			  type: Class
			  assemblies: [Library]
			- uid: Library.Widget.#ctor
			  id: '#ctor'
			  parent: Library.Widget
			  name: Widget()
			  nameWithType: Widget.Widget()
			  fullName: Library.Widget.Widget()
			  type: Constructor
			  assemblies: [Library]
			- uid: Library.Widget.Run(System.String)
			  id: Run
			  parent: Library.Widget
			  name: Run(String)
			  nameWithType: Widget.Run(String)
			  fullName: Library.Widget.Run(System.String)
			  type: Method
			  assemblies: [Library]
			- uid: Library.Widget.Run(System.Int32)
			  id: Run
			  parent: Library.Widget
			  name: Run(Int32)
			  nameWithType: Widget.Run(Int32)
			  fullName: Library.Widget.Run(System.Int32)
			  type: Method
			  assemblies: [Library]
			- uid: Library.Widget.Name
			  id: Name
			  parent: Library.Widget
			  name: Name
			  nameWithType: Widget.Name
			  fullName: Library.Widget.Name
			  type: Property
			  assemblies: [Library]
			- uid: Library.Widget.Count
			  id: Count
			  parent: Library.Widget
			  name: Count
			  nameWithType: Widget.Count
			  fullName: Library.Widget.Count
			  type: Field
			  assemblies: [Library]
			""");
		await File.WriteAllTextAsync(Path.Combine(site, "xrefmap.yml"), """
			references:
			- uid: Library.Widget
			  href: api/Library.Widget.html
			- uid: Library.Widget.#ctor
			  href: api/Library.Widget.html#Library_Widget__ctor
			- uid: Library.Widget.Run(System.String)
			  href: api/Library.Widget.html#Library_Widget_Run_System_String_
			- uid: Library.Widget.Run(System.Int32)
			  href: api/Library.Widget.html#Library_Widget_Run_System_Int32_
			- uid: Library.Widget.Name
			  href: api/Library.Widget.html#Library_Widget_Name
			- uid: Library.Widget.Count
			  href: api/Library.Widget.html#Library_Widget_Count
			""");
		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new object[]
			{
				new { type = "ManagedReference", source_relative_path = "api/Widget.yml", Title = "Library.Widget", output = new Dictionary<string, object> { [".html"] = new { relative_path = "api/Library.Widget.html" } } }
			}
		}));

		var artifact = await new SearchIndexBuilder(this._root, docs, site).BuildAsync("test");

		Assert.Equal(6, artifact.Symbols.Count);
		Assert.Equal(["class", "constructor", "field", "method", "property"], artifact.Symbols.Select(symbol => symbol.Kind).Distinct().Order().ToArray());
		var overloads = artifact.Symbols.Where(symbol => symbol.Kind == "method").OrderBy(symbol => symbol.Uid).ToArray();
		Assert.Equal(2, overloads.Length);
		Assert.NotEqual(overloads[0].Id, overloads[1].Id);
		Assert.NotEqual(overloads[0].Url, overloads[1].Url);
		Assert.All(overloads, symbol => Assert.StartsWith("/api/Library.Widget.html#Library_Widget_Run_", symbol.Url));
	}

	[Fact]
	public async Task ConceptualCorpusFailsWhenManifestSourceCannotBeRead()
	{
		var docs = Path.Combine(this._root, "docs");
		var site = Path.Combine(docs, "_site");
		Directory.CreateDirectory(site);
		await File.WriteAllTextAsync(Path.Combine(site, "xrefmap.yml"), "references: []\n");
		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new[] { Conceptual("missing.md", "missing.html") }
		}));

		await Assert.ThrowsAsync<FileNotFoundException>(() => new SearchIndexBuilder(this._root, docs, site).BuildAsync("test"));
	}

	[Fact]
	public async Task ConceptualCorpusRejectsConflictingCanonicalUrls()
	{
		var docs = Path.Combine(this._root, "docs");
		var site = Path.Combine(docs, "_site");
		Directory.CreateDirectory(site);
		await File.WriteAllTextAsync(Path.Combine(docs, "one.md"), "# One");
		await File.WriteAllTextAsync(Path.Combine(docs, "two.md"), "# Two");
		await File.WriteAllTextAsync(Path.Combine(site, "xrefmap.yml"), "references: []\n");
		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new[] { Conceptual("one.md", "same.html"), Conceptual("two.md", "same.html") }
		}));

		var error = await Assert.ThrowsAsync<InvalidDataException>(() => new SearchIndexBuilder(this._root, docs, site).BuildAsync("test"));
		Assert.Contains("Multiple conceptual manifest items", error.Message);
	}

	[Theory]
	[InlineData("api/DisCatSharp/index.md", "api")]
	[InlineData("changelogs/10.md", "changelog")]
	[InlineData("native/voice.md", "native")]
	[InlineData("natives/voice.md", "native")]
	[InlineData("vs/setup.md", "vs")]
	[InlineData("new-section/page.md", "article")]
	[InlineData("index.md", "article")]
	public void ClassifierOnlyLabelsConceptualPages(string path, string expected) => Assert.Equal(expected, ConceptualClassifier.Classify(path));

	[Theory]
	[InlineData("../secret.md")]
	[InlineData("C:/secret.md")]
	[InlineData("C:secret.md")]
	[InlineData("folder\\secret.md")]
	public void ClassifierRejectsUnsafePaths(string path) => Assert.Throws<InvalidDataException>(() => ConceptualClassifier.Classify(path));

	[Fact]
	public async Task SourceEndLineUsesTheSmallestRoslynDeclarationIncludingDocumentationTrivia()
	{
		var docs = Path.Combine(this._root, "docs");
		var site = Path.Combine(docs, "_site");
		var api = Path.Combine(docs, "api");
		var sourceDirectory = Path.Combine(this._root, "Library");
		Directory.CreateDirectory(site);
		Directory.CreateDirectory(api);
		Directory.CreateDirectory(sourceDirectory);
		await File.WriteAllTextAsync(Path.Combine(sourceDirectory, "Widget.cs"), "namespace Library;\n\npublic sealed class Widget\n{\n    /// <summary>Runs.</summary>\n    public void Run()\n    {\n    }\n}\n");
		await File.WriteAllTextAsync(Path.Combine(api, "Widget.yml"), """
			items:
			- uid: Library.Widget.Run
			  id: Run
			  parent: Library.Widget
			  name: Run()
			  nameWithType: Widget.Run()
			  fullName: Library.Widget.Run()
			  type: Method
			  assemblies:
			  - Library
			  source:
			    remote:
			      path: Library/Widget.cs
			    startLine: 5
			  syntax:
			    content: public void Run()
			""");
		await File.WriteAllTextAsync(Path.Combine(site, "xrefmap.yml"), "references:\n- uid: Library.Widget.Run\n  href: api/Widget.Run.html\n");
		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new object[]
			{
				new { type = "ManagedReference", source_relative_path = "api/Widget.yml", Title = "Library.Widget.Run", output = new Dictionary<string, object> { [".html"] = new { relative_path = "api/Widget.Run.html" } } }
			}
		}));

		var artifact = await new SearchIndexBuilder(this._root, docs, site).BuildAsync("test");

		var source = Assert.Single(artifact.Symbols).Source;
		Assert.NotNull(source);
		Assert.Equal(5, source.StartLine);
		Assert.Equal(8, source.EndLine);
	}

	[Fact]
	public async Task SourceChunksPreferRoslynBoundariesAndRemainBoundedAndContinuous()
	{
		var docs = Path.Combine(this._root, "docs");
		var site = Path.Combine(docs, "_site");
		var api = Path.Combine(docs, "api");
		var sourceDirectory = Path.Combine(this._root, "Library");
		Directory.CreateDirectory(site);
		Directory.CreateDirectory(api);
		Directory.CreateDirectory(sourceDirectory);
		var sourceLines = new List<string> { "namespace Library;", "public sealed class Large", "{" , "    public void First()", "    {" };
		sourceLines.AddRange(Enumerable.Range(1, 170).Select(number => $"        // first {number}"));
		sourceLines.AddRange(["    }", "    public void Second()", "    {"]);
		sourceLines.AddRange(Enumerable.Range(1, 90).Select(number => $"        // second {number}"));
		sourceLines.AddRange(["    }", "}"]);
		await File.WriteAllTextAsync(Path.Combine(sourceDirectory, "Large.cs"), string.Join('\n', sourceLines));
		await File.WriteAllTextAsync(Path.Combine(api, "Large.yml"), """
			items:
			- uid: Library.Large.First
			  id: First
			  parent: Library.Large
			  name: First()
			  nameWithType: Large.First()
			  fullName: Library.Large.First()
			  type: Method
			  assemblies: [Library]
			  source:
			    remote:
			      path: Library/Large.cs
			    startLine: 4
			""");
		await File.WriteAllTextAsync(Path.Combine(site, "xrefmap.yml"), "references:\n- uid: Library.Large.First\n  href: api/Large.First.html\n");
		await File.WriteAllTextAsync(Path.Combine(site, "manifest.json"), JsonSerializer.Serialize(new
		{
			files = new object[] { new { type = "ManagedReference", source_relative_path = "api/Large.yml", Title = "Library.Large.First", output = new Dictionary<string, object> { [".html"] = new { relative_path = "api/Large.First.html" } } } }
		}));

		var chunks = (await new SearchIndexBuilder(this._root, docs, site).BuildAsync("test")).SourceChunks;

		Assert.True(chunks.Count >= 2);
		Assert.InRange(chunks[0].EndLine, 170, 199);
		Assert.Equal(1, chunks[0].StartLine);
		for (var index = 0; index < chunks.Count; index++)
		{
			Assert.InRange(chunks[index].EndLine - chunks[index].StartLine + 1, 1, 200);
			Assert.True(System.Text.Encoding.UTF8.GetByteCount(chunks[index].Content) <= 32 * 1024);
			if (index > 0)
				Assert.Equal(chunks[index - 1].EndLine + 1, chunks[index].StartLine);
		}
		Assert.Equal(sourceLines.Count, chunks[^1].EndLine);
	}

	public void Dispose()
	{
		if (Directory.Exists(this._root))
			Directory.Delete(this._root, true);
	}

	private static object Conceptual(string source, string output) => new
	{
		type = "Conceptual",
		source_relative_path = source,
		output = new Dictionary<string, object> { [".html"] = new { relative_path = output } }
	};
}
