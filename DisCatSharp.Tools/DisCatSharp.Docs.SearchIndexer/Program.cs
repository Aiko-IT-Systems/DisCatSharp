using System.Text.Json;
using DisCatSharp.Docs.SearchIndexer;

var options = CliOptions.Parse(args);
var builder = new SearchIndexBuilder(options.RepositoryRoot, options.DocsRoot, options.SiteRoot, new SearchCorpusOptions(
	options.Corpus,
	options.Repository,
	options.SiteBaseUrl,
	options.SourcePathPrefix));
var artifact = await builder.BuildAsync(options.SourceCommit);
var output = Path.GetFullPath(options.OutputPath);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
await using var stream = File.Create(output);
await JsonSerializer.SerializeAsync(stream, artifact, new JsonSerializerOptions
{
	PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	WriteIndented = true
});
Console.WriteLine($"Wrote {artifact.Counts.Symbols:N0} symbols, {artifact.Counts.Documents:N0} conceptual documents, and {artifact.Counts.SourceChunks:N0} source chunks to {output}.");

internal sealed record CliOptions(
	string RepositoryRoot,
	string DocsRoot,
	string SiteRoot,
	string OutputPath,
	string? SourceCommit,
	string Corpus,
	string Repository,
	string? SiteBaseUrl,
	string? SourcePathPrefix)
{
	public static CliOptions Parse(string[] args)
	{
		var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		for (var index = 0; index < args.Length; index += 2)
		{
			if (index + 1 >= args.Length || !args[index].StartsWith("--", StringComparison.Ordinal))
				throw new ArgumentException("Arguments must use --name value pairs.");
			values[args[index][2..]] = args[index + 1];
		}

		var repository = Path.GetFullPath(values.GetValueOrDefault("repo") ?? Directory.GetCurrentDirectory());
		var docs = Path.GetFullPath(values.GetValueOrDefault("docs") ?? Path.Combine(repository, "DisCatSharp.Docs"));
		return new CliOptions(
			repository,
			docs,
			Path.GetFullPath(values.GetValueOrDefault("site") ?? Path.Combine(docs, "_site")),
			Path.GetFullPath(values.GetValueOrDefault("output") ?? Path.Combine(docs, "obj", "search", "search-index.json")),
			values.GetValueOrDefault("commit"),
			values.GetValueOrDefault("corpus") ?? "main",
			values.GetValueOrDefault("repository") ?? "Aiko-IT-Systems/DisCatSharp",
			values.GetValueOrDefault("site-base-url"),
			values.GetValueOrDefault("source-prefix"));
	}
}
