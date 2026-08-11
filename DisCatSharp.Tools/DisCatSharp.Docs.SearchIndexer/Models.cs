using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace DisCatSharp.Docs.SearchIndexer;

public sealed record SearchIndexArtifact(
	int SchemaVersion,
	string Corpus,
	string Repository,
	string? SiteBaseUrl,
	string SourceCommit,
	DateTimeOffset GeneratedAt,
	SearchIndexCounts Counts,
	IReadOnlyList<string> Modules,
	IReadOnlyList<string> Types,
	IReadOnlyList<SearchSymbol> Symbols,
	IReadOnlyList<SearchDocument> Documents,
	IReadOnlyList<SourceChunk> SourceChunks
);

public sealed record SearchCorpusOptions(string Name, string Repository, string? SiteBaseUrl = null, string? SourcePathPrefix = null)
{
	public static SearchCorpusOptions Main { get; } = new("main", "Aiko-IT-Systems/DisCatSharp");
}

public sealed record SearchIndexCounts(int Symbols, int Documents, int ConceptualManifestItems, int SourceChunks);

public sealed record SearchSymbol(
	string Id,
	string Uid,
	string Name,
	string DisplayName,
	string QualifiedName,
	string FullName,
	string Kind,
	string? Namespace,
	string? Module,
	string? ParentUid,
	string Summary,
	string Signature,
	string Content,
	string Url,
	SourceLocation? Source,
	IReadOnlyList<string> RelatedUids,
	string ContentHash
);

public sealed record SearchDocument(
	string Id,
	string DocumentKey,
	string Family,
	string Kind,
	string Title,
	string Description,
	string Content,
	string Url,
	string? Module,
	string SourcePath,
	string ContentHash
);

public sealed record SourceLocation(string Path, int StartLine, int EndLine);

public sealed record SourceChunk(
	string Id,
	string Path,
	string Language,
	int StartLine,
	int EndLine,
	string Content,
	string ContentHash
);

internal sealed class ManifestRoot
{
	[JsonPropertyName("files")]
	public List<ManifestFile> Files { get; init; } = [];
}

internal sealed class ManifestFile
{
	[JsonPropertyName("type")]
	public string? Type { get; init; }

	[JsonPropertyName("source_relative_path")]
	public string? SourceRelativePath { get; init; }

	[JsonPropertyName("output")]
	public Dictionary<string, ManifestOutput> Output { get; init; } = [];

	[JsonPropertyName("Title")]
	public string? Title { get; init; }
}

internal sealed class ManifestOutput
{
	[JsonPropertyName("relative_path")]
	public string? RelativePath { get; init; }
}

internal sealed class ManagedReferenceDocument
{
	[YamlMember(Alias = "items")]
	public List<ManagedReferenceItem> Items { get; init; } = [];
}

internal sealed class ManagedReferenceItem
{
	[YamlMember(Alias = "uid")]
	public string? Uid { get; init; }

	[YamlMember(Alias = "id")]
	public string? MemberId { get; init; }

	[YamlMember(Alias = "parent")]
	public string? Parent { get; init; }

	[YamlMember(Alias = "children")]
	public List<string> Children { get; init; } = [];

	[YamlMember(Alias = "name")]
	public string? Name { get; init; }

	[YamlMember(Alias = "nameWithType")]
	public string? NameWithType { get; init; }

	[YamlMember(Alias = "fullName")]
	public string? FullName { get; init; }

	[YamlMember(Alias = "type")]
	public string? Type { get; init; }

	[YamlMember(Alias = "namespace")]
	public string? Namespace { get; init; }

	[YamlMember(Alias = "assemblies")]
	public List<string> Assemblies { get; init; } = [];

	[YamlMember(Alias = "summary")]
	public object? Summary { get; init; }

	[YamlMember(Alias = "remarks")]
	public object? Remarks { get; init; }

	[YamlMember(Alias = "example")]
	public object? Example { get; init; }

	[YamlMember(Alias = "syntax")]
	public ManagedReferenceSyntax? Syntax { get; init; }

	[YamlMember(Alias = "source")]
	public ManagedReferenceSource? Source { get; init; }

	[YamlMember(Alias = "exceptions")]
	public List<ManagedReferenceDescription> Exceptions { get; init; } = [];

	[YamlMember(Alias = "seealso")]
	public object? SeeAlso { get; init; }
}

internal sealed class ManagedReferenceSyntax
{
	[YamlMember(Alias = "content")]
	public string? Content { get; init; }

	[YamlMember(Alias = "parameters")]
	public List<ManagedReferenceParameter> Parameters { get; init; } = [];

	[YamlMember(Alias = "return")]
	public ManagedReferenceDescription? Return { get; init; }
}

internal sealed class ManagedReferenceParameter
{
	[YamlMember(Alias = "id")]
	public string? Id { get; init; }

	[YamlMember(Alias = "type")]
	public string? Type { get; init; }

	[YamlMember(Alias = "description")]
	public object? Description { get; init; }
}

internal sealed class ManagedReferenceDescription
{
	[YamlMember(Alias = "type")]
	public string? Type { get; init; }

	[YamlMember(Alias = "description")]
	public object? Description { get; init; }
}

internal sealed class ManagedReferenceSource
{
	[YamlMember(Alias = "path")]
	public string? Path { get; init; }

	[YamlMember(Alias = "startLine")]
	public int StartLine { get; init; }

	[YamlMember(Alias = "remote")]
	public ManagedReferenceRemote? Remote { get; init; }
}

internal sealed class ManagedReferenceRemote
{
	[YamlMember(Alias = "path")]
	public string? Path { get; init; }
}

internal sealed class XrefMap
{
	[YamlMember(Alias = "references")]
	public List<XrefReference> References { get; init; } = [];
}

internal sealed class XrefReference
{
	[YamlMember(Alias = "uid")]
	public string? Uid { get; init; }

	[YamlMember(Alias = "href")]
	public string? Href { get; init; }
}

internal sealed record ParsedMarkdown(string Title, string Description, string Content);
