using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace DisCatSharp.Analyzer.Tests;

internal static class RoslynTestDocumentFactory
{
	private static readonly ImmutableArray<MetadataReference> s_metadataReferences = CreateMetadataReferences();

	public static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
	{
		var compilation = CSharpCompilation.Create(
			"DisCatSharp.Analyzer.Tests.Dynamic",
			[CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
			s_metadataReferences,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var diagnostics = await compilation
			.WithAnalyzers([new DisCatSharpAnalyzer()])
			.GetAnalyzerDiagnosticsAsync()
			.ConfigureAwait(false);

		return diagnostics.Where(x => x.Id.StartsWith(DisCatSharpAnalyzer.DIAGNOSTIC_ID_PREFIX)).ToImmutableArray();
	}

	public static async Task<string> ApplyRequiresOverrideFixAsync(string source, string overrideValue)
	{
		var fixedSources = await ApplyRequiresOverrideFixAsync(
			ImmutableDictionary<string, string>.Empty.Add("Test.cs", source),
			"Test.cs",
			overrideValue).ConfigureAwait(false);
		return fixedSources["Test.cs"];
	}

	public static async Task<ImmutableDictionary<string, string>> ApplyRequiresOverrideFixAsync(
		ImmutableDictionary<string, string> sources,
		string diagnosticDocumentName,
		string overrideValue)
	{
		using var workspace = new AdhocWorkspace();
		var (solution, documentIds) = CreateProjectSolution(workspace, sources);
		var diagnosticDocumentId = documentIds[diagnosticDocumentName];
		var diagnosticDocument = solution.GetDocument(diagnosticDocumentId)!;
		var fixedSolution = await DisCatSharpRequiresOverrideCodeFix.ApplyFixToProjectAsync(diagnosticDocument.Project, overrideValue, CancellationToken.None).ConfigureAwait(false);

		var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
		foreach (var (documentName, documentId) in documentIds)
		{
			var document = fixedSolution.GetDocument(documentId)!;
			var text = await document.GetTextAsync().ConfigureAwait(false);
			builder[documentName] = text.ToString();
		}

		return builder.ToImmutable();
	}

	public static async Task<string> ApplyApplicationCommandChecksFailedMigrationFixAsync(string source)
	{
		using var workspace = new AdhocWorkspace();
		var (solution, documentIds) = CreateProjectSolution(workspace, ImmutableDictionary<string, string>.Empty.Add("Test.cs", source));
		var documentId = documentIds["Test.cs"];

		var document = solution.GetDocument(documentId)!;
		var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
		var diagnostics = await compilation!
			.WithAnalyzers([new DisCatSharpAnalyzer()])
			.GetAnalyzerDiagnosticsAsync()
			.ConfigureAwait(false);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration);

		var fixedDocument = await DisCatSharpApplicationCommandChecksFailedMigrationCodeFix.ApplyFixToDocumentAsync(document, diagnostic, CancellationToken.None).ConfigureAwait(false);
		var text = await fixedDocument.GetTextAsync().ConfigureAwait(false);
		return text.ToString();
	}

	public static async Task<string> ApplyPresenceAccessMigrationFixAsync(string source)
	{
		using var workspace = new AdhocWorkspace();
		var (solution, documentIds) = CreateProjectSolution(workspace, ImmutableDictionary<string, string>.Empty.Add("Test.cs", source));
		var documentId = documentIds["Test.cs"];

		var document = solution.GetDocument(documentId)!;
		var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
		var diagnostics = await compilation!
			.WithAnalyzers([new DisCatSharpAnalyzer()])
			.GetAnalyzerDiagnosticsAsync()
			.ConfigureAwait(false);
		var diagnostic = Assert.Single(diagnostics, x => x.Id == DisCatSharpDiagnosticIds.PresenceAccessMigration);

		var fixedDocument = await DisCatSharpPresenceAccessMigrationCodeFix.ApplyFixToDocumentAsync(document, diagnostic, CancellationToken.None).ConfigureAwait(false);
		var text = await fixedDocument.GetTextAsync().ConfigureAwait(false);
		return text.ToString();
	}

	private static (Solution Solution, ImmutableDictionary<string, DocumentId> DocumentIds) CreateProjectSolution(
		AdhocWorkspace workspace,
		ImmutableDictionary<string, string> sources)
	{
		var projectId = ProjectId.CreateNewId();
		var solution = workspace.CurrentSolution
			.AddProject(projectId, "DisCatSharp.Analyzer.Tests.Dynamic", "DisCatSharp.Analyzer.Tests.Dynamic", LanguageNames.CSharp)
			.WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Latest))
			.WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.WithProjectMetadataReferences(projectId, s_metadataReferences);

		var builder = ImmutableDictionary.CreateBuilder<string, DocumentId>(StringComparer.Ordinal);
		foreach (var (documentName, source) in sources)
		{
			var documentId = DocumentId.CreateNewId(projectId);
			solution = solution.AddDocument(documentId, documentName, SourceText.From(source));
			builder[documentName] = documentId;
		}

		return (solution, builder.ToImmutable());
	}

	private static ImmutableArray<MetadataReference> CreateMetadataReferences()
	{
		var trustedPlatformAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Select(path => MetadataReference.CreateFromFile(path));

		return trustedPlatformAssemblies
			.Append(MetadataReference.CreateFromFile(typeof(DiscordClient).Assembly.Location))
			.Append(MetadataReference.CreateFromFile(typeof(ApplicationCommandsExtension).Assembly.Location))
			.Append(MetadataReference.CreateFromFile(typeof(ExperimentalAttribute).Assembly.Location))
			.GroupBy(x => x.Display, StringComparer.OrdinalIgnoreCase)
			.Select(x => x.First())
			.Select(x => (MetadataReference)x)
			.ToImmutableArray();
	}
}
