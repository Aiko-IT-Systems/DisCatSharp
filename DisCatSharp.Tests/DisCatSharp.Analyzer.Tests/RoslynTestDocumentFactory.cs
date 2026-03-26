using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
		using var workspace = new AdhocWorkspace();
		var projectId = ProjectId.CreateNewId();
		var documentId = DocumentId.CreateNewId(projectId);

		var solution = workspace.CurrentSolution
			.AddProject(projectId, "DisCatSharp.Analyzer.Tests.Dynamic", "DisCatSharp.Analyzer.Tests.Dynamic", LanguageNames.CSharp)
			.WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Latest))
			.WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.WithProjectMetadataReferences(projectId, s_metadataReferences)
			.AddDocument(documentId, "Test.cs", SourceText.From(source));

		var document = solution.GetDocument(documentId)!;
		var fixedDocument = await DisCatSharpRequiresOverrideCodeFix.ApplyFixToDocumentAsync(document, overrideValue, CancellationToken.None).ConfigureAwait(false);
		var text = await fixedDocument.GetTextAsync().ConfigureAwait(false);
		return text.ToString();
	}

	private static ImmutableArray<MetadataReference> CreateMetadataReferences()
	{
		var trustedPlatformAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Select(path => MetadataReference.CreateFromFile(path));

		return trustedPlatformAssemblies
			.Append(MetadataReference.CreateFromFile(typeof(DiscordClient).Assembly.Location))
			.Append(MetadataReference.CreateFromFile(typeof(ExperimentalAttribute).Assembly.Location))
			.GroupBy(x => x.Display, StringComparer.OrdinalIgnoreCase)
			.Select(x => x.First())
			.Select(x => (MetadataReference)x)
			.ToImmutableArray();
	}
}
