using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using YamlDotNet.Serialization;

namespace DisCatSharp.Docs.SearchIndexer;

public sealed partial class SearchIndexBuilder(string repositoryRoot, string docsRoot, string siteRoot)
{
	private const int SchemaVersion = 1;
	private const int MaxChunkLines = 200;
	private const int MaxChunkBytes = 32 * 1024;
	private const int MaxRelatedMembers = 100;
	private readonly string _repositoryRoot = Path.GetFullPath(repositoryRoot);
	private readonly string _docsRoot = Path.GetFullPath(docsRoot);
	private readonly string _siteRoot = Path.GetFullPath(siteRoot);
	private readonly IDeserializer _yaml = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

	public async Task<SearchIndexArtifact> BuildAsync(string? sourceCommit = null, CancellationToken cancellationToken = default)
	{
		var manifest = await ReadJsonAsync<ManifestRoot>(Path.Combine(this._siteRoot, "manifest.json"), cancellationToken);
		var xrefs = await ReadYamlAsync<XrefMap>(Path.Combine(this._siteRoot, "xrefmap.yml"), cancellationToken);
		var hrefByUid = xrefs.References
			.Where(reference => !string.IsNullOrWhiteSpace(reference.Uid) && !string.IsNullOrWhiteSpace(reference.Href))
			.GroupBy(reference => reference.Uid!, StringComparer.Ordinal)
			.ToDictionary(group => group.Key, group => "/" + group.First().Href!.TrimStart('/'), StringComparer.Ordinal);

		var symbols = await this.BuildSymbolsAsync(manifest, hrefByUid, cancellationToken);
		var documents = await this.BuildDocumentsAsync(manifest, cancellationToken);
		var sourceChunks = await this.BuildSourceChunksAsync(symbols, cancellationToken);

		var modules = symbols.Select(symbol => symbol.Module)
			.Concat(documents.Select(document => document.Module))
			.Where(module => !string.IsNullOrWhiteSpace(module))
			.Select(module => module!)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Order(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		var types = symbols.Select(symbol => symbol.Kind)
			.Concat(documents.Select(document => document.Kind))
			.Append("conceptual")
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Order(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		var conceptualCount = manifest.Files.Count(file => string.Equals(file.Type, "Conceptual", StringComparison.Ordinal));

		return new SearchIndexArtifact(
			SchemaVersion,
			sourceCommit ?? ResolveCommit(this._repositoryRoot),
			DateTimeOffset.UtcNow,
			new SearchIndexCounts(symbols.Count, documents.Count, conceptualCount, sourceChunks.Count),
			modules,
			types,
			symbols,
			documents,
			sourceChunks);
	}

	private async Task<List<SearchSymbol>> BuildSymbolsAsync(ManifestRoot manifest, IReadOnlyDictionary<string, string> hrefByUid, CancellationToken cancellationToken)
	{
		var yamlPaths = manifest.Files
			.Where(file => string.Equals(file.Type, "ManagedReference", StringComparison.Ordinal))
			.Select(file => NormalizeRelativePath(file.SourceRelativePath ?? throw new InvalidDataException("ManagedReference manifest item has no source path.")))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Order(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		var pending = new List<(ManagedReferenceItem Item, string Url, SourceLocation? Source)>();
		foreach (var yamlPath in yamlPaths)
		{
			var document = await this.ReadYamlAsync<ManagedReferenceDocument>(ResolveWithin(this._docsRoot, yamlPath), cancellationToken);
			foreach (var item in document.Items)
			{
				if (string.IsNullOrWhiteSpace(item.Uid) || string.IsNullOrWhiteSpace(item.Type))
					continue;

				var url = hrefByUid.GetValueOrDefault(item.Uid!)
					?? throw new InvalidDataException($"No canonical DocFX xref URL found for symbol '{item.Uid}'.");
				pending.Add((item, url, this.CreateSourceLocation(item.Source)));
			}
		}

		var spans = await this.ResolveSourceSpansAsync(pending.Select(entry => entry.Source).OfType<SourceLocation>(), cancellationToken);
		var symbols = new List<SearchSymbol>(pending.Count);
		foreach (var (item, url, initialSource) in pending)
		{
			var source = initialSource is null ? null : initialSource with { EndLine = spans.GetValueOrDefault((initialSource.Path, initialSource.StartLine), initialSource.StartLine) };
			var displayName = item.Name ?? item.MemberId ?? item.Uid!;
			var simpleName = GetSimpleName(item);
			var qualifiedName = StripArguments(item.NameWithType ?? displayName);
			var fullName = StripArguments(item.FullName ?? item.Uid!);
			var summary = MarkdownParser.NormalizeInline(Flatten(item.Summary));
			var signature = item.Syntax?.Content?.Trim() ?? string.Empty;
			var content = BuildSymbolContent(item, summary, signature);
			var kind = item.Type!.ToLowerInvariant();
			var module = item.Assemblies.FirstOrDefault();
			var related = item.Children.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).Take(MaxRelatedMembers).ToArray();
			var hash = Hash(string.Join('\n', item.Uid, displayName, simpleName, qualifiedName, fullName, kind, item.Namespace, module, item.Parent, summary, signature, content, url, source?.Path, source?.StartLine, source?.EndLine, string.Join('\n', related)));

			symbols.Add(new SearchSymbol($"symbol:{item.Uid}", item.Uid!, simpleName, displayName, qualifiedName, fullName, kind, item.Namespace, module, item.Parent, summary, signature, content, url, source, related, hash));
		}

		return symbols.OrderBy(symbol => symbol.Uid, StringComparer.Ordinal).ToList();
	}

	private async Task<List<SearchDocument>> BuildDocumentsAsync(ManifestRoot manifest, CancellationToken cancellationToken)
	{
		var conceptual = manifest.Files.Where(file => string.Equals(file.Type, "Conceptual", StringComparison.Ordinal)).ToArray();
		var documents = new List<SearchDocument>(conceptual.Length);
		var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var file in conceptual.OrderBy(file => file.SourceRelativePath, StringComparer.OrdinalIgnoreCase))
		{
			var sourcePath = NormalizeRelativePath(file.SourceRelativePath ?? throw new InvalidDataException("Conceptual manifest item has no source path."));
			if (!string.Equals(Path.GetExtension(sourcePath), ".md", StringComparison.OrdinalIgnoreCase))
				throw new InvalidDataException($"Conceptual source '{sourcePath}' is not a Markdown file.");

			var url = GetOutputUrl(file);
			if (!seenUrls.Add(url))
				throw new InvalidDataException($"Multiple conceptual manifest items produce '{url}'.");

			var markdown = await File.ReadAllTextAsync(ResolveWithin(this._docsRoot, sourcePath), cancellationToken);
			var parsed = MarkdownParser.Parse(markdown, Path.GetFileNameWithoutExtension(sourcePath));
			var kind = ConceptualClassifier.Classify(sourcePath);
			var key = CreateDocumentKey(url);
			var module = GetConceptualModule(sourcePath);
			var hash = Hash(string.Join('\n', key, kind, parsed.Title, parsed.Description, parsed.Content, url, module, sourcePath));
			documents.Add(new SearchDocument($"document:{key}", key, "conceptual", kind, parsed.Title, parsed.Description, parsed.Content, url, module, sourcePath, hash));
		}

		var documentUrls = documents.Select(document => document.Url).ToHashSet(StringComparer.OrdinalIgnoreCase);
		if (!seenUrls.SetEquals(documentUrls) || documents.Count != conceptual.Length)
			throw new InvalidDataException("Conceptual index coverage does not exactly match the DocFX manifest.");

		return documents.OrderBy(document => document.Url, StringComparer.OrdinalIgnoreCase).ToList();
	}

	private async Task<List<SourceChunk>> BuildSourceChunksAsync(IReadOnlyCollection<SearchSymbol> symbols, CancellationToken cancellationToken)
	{
		var chunks = new List<SourceChunk>();
		foreach (var sourcePath in symbols.Select(symbol => symbol.Source?.Path).Where(path => path is not null).Select(path => path!).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase))
		{
			var content = await File.ReadAllTextAsync(ResolveWithin(this._repositoryRoot, sourcePath), cancellationToken);
			var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');
			var tree = CSharpSyntaxTree.ParseText(content, cancellationToken: cancellationToken);
			var root = await tree.GetRootAsync(cancellationToken);
			var declarationBoundaries = root.DescendantNodes()
				.Where(IsDeclaration)
				.SelectMany(node => new[]
				{
					tree.GetLineSpan(node.FullSpan).StartLinePosition.Line,
					tree.GetLineSpan(node.Span).EndLinePosition.Line + 1
				})
				.Where(line => line > 0 && line < lines.Length)
				.Distinct()
				.Order()
				.ToArray();
			var start = 0;
			while (start < lines.Length)
			{
				var end = start;
				var bytes = 0;
				while (end < lines.Length && end - start < MaxChunkLines)
				{
					var nextBytes = Encoding.UTF8.GetByteCount(lines[end]) + (end > start ? 1 : 0);
					if (nextBytes > MaxChunkBytes)
						throw new InvalidDataException($"Source '{sourcePath}' contains a line larger than {MaxChunkBytes:N0} bytes.");
					if (end > start && bytes + nextBytes > MaxChunkBytes)
						break;
					bytes += nextBytes;
					end++;
				}

				if (end == start)
					end++;

				// Prefer a Roslyn declaration boundary when one is available inside
				// the hard size cap. This keeps chunks readable without sacrificing
				// complete, non-overlapping source coverage.
				var preferredEnd = declarationBoundaries.LastOrDefault(line => line > start && line <= end);
				if (preferredEnd > start)
					end = preferredEnd;

				var chunkContent = string.Join('\n', lines[start..end]);
				var startLine = start + 1;
				var endLine = end;
				var id = $"source:{sourcePath}#L{startLine}-L{endLine}";
				chunks.Add(new SourceChunk(id, sourcePath, "csharp", startLine, endLine, chunkContent, Hash(string.Join('\n', sourcePath, startLine, endLine, chunkContent))));
				start = end;
			}
		}

		return chunks;
	}

	private async Task<Dictionary<(string Path, int StartLine), int>> ResolveSourceSpansAsync(IEnumerable<SourceLocation> locations, CancellationToken cancellationToken)
	{
		var result = new Dictionary<(string Path, int StartLine), int>();
		foreach (var group in locations.Distinct().GroupBy(location => location.Path, StringComparer.OrdinalIgnoreCase))
		{
			var sourceText = await File.ReadAllTextAsync(ResolveWithin(this._repositoryRoot, group.Key), cancellationToken);
			var tree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: cancellationToken);
			var root = await tree.GetRootAsync(cancellationToken);
			var declarationSpans = root.DescendantNodes()
				.Where(IsDeclaration)
				.Select(node => (
					Start: tree.GetLineSpan(node.FullSpan).StartLinePosition.Line + 1,
					End: tree.GetLineSpan(node.Span).EndLinePosition.Line + 1))
				.ToArray();
			var orderedStarts = group.Select(location => location.StartLine).Distinct().Order().ToArray();

			foreach (var startLine in orderedStarts)
			{
				var exact = declarationSpans.Where(span => span.Start == startLine).OrderBy(span => span.End - span.Start).FirstOrDefault();
				var containing = declarationSpans.Where(span => span.Start <= startLine && span.End >= startLine).OrderBy(span => span.End - span.Start).FirstOrDefault();
				var nextSymbol = orderedStarts.FirstOrDefault(line => line > startLine);
				var fallback = nextSymbol > 0 ? nextSymbol - 1 : startLine;
				result[(group.Key, startLine)] = Math.Max(startLine, exact != default ? exact.End : containing != default ? containing.End : fallback);
			}
		}

		return result;
	}

	private SourceLocation? CreateSourceLocation(ManagedReferenceSource? source)
	{
		var path = source?.Remote?.Path;
		if (string.IsNullOrWhiteSpace(path) || source!.StartLine <= 0)
			return null;

		path = NormalizeRelativePath(path);
		try
		{
			_ = ResolveWithin(this._repositoryRoot, path);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
		return new SourceLocation(path, source.StartLine, source.StartLine);
	}

	private async Task<T> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
	{
		await using var stream = File.OpenRead(path);
		return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken) ?? throw new InvalidDataException($"Unable to parse '{path}'.");
	}

	private async Task<T> ReadYamlAsync<T>(string path, CancellationToken cancellationToken)
	{
		var text = await File.ReadAllTextAsync(path, cancellationToken);
		return this._yaml.Deserialize<T>(StripYamlMimeHeader(text)) ?? throw new InvalidDataException($"Unable to parse '{path}'.");
	}

	private static string StripYamlMimeHeader(string value)
	{
		var newline = value.IndexOf('\n');
		return value.StartsWith("### YamlMime:", StringComparison.Ordinal) && newline >= 0 ? value[(newline + 1)..] : value;
	}

	private static bool IsDeclaration(SyntaxNode node) => node is MemberDeclarationSyntax or VariableDeclaratorSyntax or EnumMemberDeclarationSyntax or AccessorDeclarationSyntax;

	private static string BuildSymbolContent(ManagedReferenceItem item, string summary, string signature)
	{
		var parts = new List<string> { summary, MarkdownParser.NormalizeInline(Flatten(item.Remarks)), MarkdownParser.NormalizeInline(Flatten(item.Example)), signature };
		parts.AddRange(item.Syntax?.Parameters.Select(parameter => $"Parameter {parameter.Id} ({parameter.Type}): {MarkdownParser.NormalizeInline(Flatten(parameter.Description))}") ?? []);
		if (item.Syntax?.Return is { } returns)
			parts.Add($"Returns {returns.Type}: {MarkdownParser.NormalizeInline(Flatten(returns.Description))}");
		parts.AddRange(item.Exceptions.Select(exception => $"Throws {exception.Type}: {MarkdownParser.NormalizeInline(Flatten(exception.Description))}"));
		parts.Add(MarkdownParser.NormalizeInline(Flatten(item.SeeAlso)));
		return string.Join('\n', parts.Where(part => !string.IsNullOrWhiteSpace(part))).Trim();
	}

	private static string GetSimpleName(ManagedReferenceItem item)
	{
		var value = item.MemberId ?? item.Name ?? item.Uid ?? string.Empty;
		value = StripArguments(value);
		if (value is "#ctor" or ".ctor")
			return item.Parent?.Split('.').LastOrDefault() ?? value;
		return value.Split('.').LastOrDefault() ?? value;
	}

	private static string StripArguments(string value)
	{
		var index = value.IndexOf('(');
		return (index >= 0 ? value[..index] : value).TrimEnd('*');
	}

	private static string Flatten(object? value)
	{
		return value switch
		{
			null => string.Empty,
			string text => text,
			IEnumerable<object> sequence => string.Join(' ', sequence.Select(Flatten)),
			IDictionary<object, object> dictionary => string.Join(' ', dictionary.Values.Select(Flatten)),
			_ => Convert.ToString(value) ?? string.Empty
		};
	}

	private static string GetOutputUrl(ManifestFile file)
	{
		if (!file.Output.TryGetValue(".html", out var output) || string.IsNullOrWhiteSpace(output.RelativePath))
			throw new InvalidDataException($"Manifest item '{file.SourceRelativePath}' has no HTML output.");
		return "/" + NormalizeRelativePath(output.RelativePath);
	}

	private static string CreateDocumentKey(string url)
	{
		var key = url.Trim('/');
		if (key.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
			key = key[..^5];
		if (key.EndsWith("/index", StringComparison.OrdinalIgnoreCase))
			key = key[..^6];
		return string.IsNullOrWhiteSpace(key) || string.Equals(key, "index", StringComparison.OrdinalIgnoreCase) ? "home" : key;
	}

	private static string? GetConceptualModule(string sourcePath)
	{
		var segments = sourcePath.Split('/');
		return segments.Length > 2 && string.Equals(segments[0], "api", StringComparison.OrdinalIgnoreCase) ? segments[1] : null;
	}

	internal static string NormalizeRelativePath(string path)
	{
		var hasWindowsDrivePrefix = path is { Length: >= 2 } && char.IsAsciiLetter(path[0]) && path[1] == ':';
		if (string.IsNullOrWhiteSpace(path) || hasWindowsDrivePrefix || Path.IsPathFullyQualified(path) || path.Contains('\\') || path.Contains('\0'))
			throw new InvalidDataException($"Unsafe relative path '{path}'.");
		var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
			throw new InvalidDataException($"Unsafe relative path '{path}'.");
		return string.Join('/', segments);
	}

	private static string ResolveWithin(string root, string relativePath)
	{
		var normalized = NormalizeRelativePath(relativePath);
		var resolved = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
		var prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		if (!resolved.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			throw new InvalidDataException($"Path '{relativePath}' escapes '{root}'.");
		if (!File.Exists(resolved))
			throw new FileNotFoundException($"Indexed source '{relativePath}' does not exist.", resolved);
		return resolved;
	}

	private static string ResolveCommit(string root)
	{
		var configured = Environment.GetEnvironmentVariable("GITHUB_SHA");
		if (!string.IsNullOrWhiteSpace(configured))
			return configured;
		using var process = Process.Start(new ProcessStartInfo("git", $"-C \"{root}\" rev-parse HEAD") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true });
		process?.WaitForExit(5000);
		return process is { ExitCode: 0 } ? process.StandardOutput.ReadToEnd().Trim() : "unknown";
	}

	private static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

public static class ConceptualClassifier
{
	public static string Classify(string sourcePath)
	{
		var path = SearchIndexBuilder.NormalizeRelativePath(sourcePath);
		if (path.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
			return "api";
		if (path.StartsWith("changelogs/", StringComparison.OrdinalIgnoreCase))
			return "changelog";
		if (path.StartsWith("native/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("natives/", StringComparison.OrdinalIgnoreCase))
			return "native";
		if (path.StartsWith("vs/", StringComparison.OrdinalIgnoreCase))
			return "vs";
		return "article";
	}
}
