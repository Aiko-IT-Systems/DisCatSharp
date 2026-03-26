using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DisCatSharp.Analyzer;

/// <inheritdoc />
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpRequiresOverrideCodeFix)), Shared]
public sealed class DisCatSharpRequiresOverrideCodeFix : SingleDiagnosticCodeFixProvider
{
	private const string DiscordConfigurationMetadataName = "DisCatSharp.DiscordConfiguration";
	private const string OverridePropertyName = "Override";
	internal const string DefaultOverrideValue = "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRGlzY29yZCBDbGllbnQiLCJyZWxlYXNlX2NoYW5uZWwiOiJjYW5hcnkiLCJjbGllbnRfdmVyc2lvbiI6IjEuMC41NzgiLCJvc192ZXJzaW9uIjoiMTAuMC4yNjEyMCIsIm9zX2FyY2giOiJ4NjQiLCJhcHBfYXJjaCI6Ing2NCIsInN5c3RlbV9sb2NhbGUiOiJlbi1VUyIsImhhc19jbGllbnRfbW9kcyI6ZmFsc2UsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQpIEFwcGxlV2ViS2l0LzUzNy4zNiAoS0hUTUwsIGxpa2UgR2Vja28pIGRpc2NvcmQvMS4wLjU3OCBDaHJvbWUvMTM0LjAuNjk5OC40NCBFbGVjdHJvbi8zNS4wLjIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQpIEFwcGxlV2ViS2l0LzUzNy4zNiAoS0hUTUwsIGxpa2UgR2Vja28pIGRpc2NvcmQvMS4wLjU3OCBDaHJvbWUvMTM0LjAuNjk5OC40NCBFbGVjdHJvbi8zNS4wLjIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6IjM1LjAuMiIsIm9zX3Nka192ZXJzaW9uIjoiMjYxMjAiLCJjbGllbnRfYnVpbGRfbnVtYmVyIjozODA2NzUsIm5hdGl2ZV9idWlsZF9udW1iZXIiOjYwNjYzLCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==";

	private static readonly string[] s_supportedOverrideDateFormats =
	[
		"dd/MM/yyyy",
		"d/M/yyyy",
		"yyyy-MM-dd",
		"O",
		"o"
	];

	/// <inheritdoc />
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.RequiresOverride;

	/// <inheritdoc />
	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		var overrideValue = SelectLatestOverrideValue(diagnostics);
		context.RegisterCodeFix(
			CodeAction.Create(
				"Add required Override to this DiscordConfiguration",
				c => ApplyFixToDocumentAsync(context.Document, overrideValue, c),
				this.FixableDiagnosticId),
			diagnostics);

		return Task.CompletedTask;
	}

	internal static string SelectLatestOverrideValue(ImmutableArray<Diagnostic> diagnostics)
	{
		var newestDateTime = DateTime.MinValue;
		var overrideValue = DefaultOverrideValue;

		foreach (var diagnostic in diagnostics)
		{
			if (!TryGetOverrideMetadata(diagnostic, out var currentOverrideValue, out var currentOverrideDate))
				continue;

			if (currentOverrideDate <= newestDateTime)
				continue;

			newestDateTime = currentOverrideDate;
			overrideValue = currentOverrideValue;
		}

		return overrideValue;
	}

	internal static bool TryGetOverrideMetadata(Diagnostic diagnostic, out string overrideValue, out DateTime overrideDate)
	{
		overrideValue = string.Empty;
		overrideDate = DateTime.MinValue;

		if (!diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.LastKnownOverride, out var rawOverrideValue) ||
		    string.IsNullOrWhiteSpace(rawOverrideValue))
			return false;

		if (!diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.OverrideDate, out var rawOverrideDate) ||
		    !TryParseOverrideDate(rawOverrideDate, out overrideDate))
			return false;

		overrideValue = rawOverrideValue!;
		return true;
	}

	internal static bool TryParseOverrideDate(string? rawOverrideDate, out DateTime overrideDate)
	{
		if (DateTime.TryParseExact(rawOverrideDate, s_supportedOverrideDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out overrideDate))
			return true;

		return DateTime.TryParse(rawOverrideDate, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out overrideDate);
	}

	/// <summary>
	///     Applies modifications to a document's syntax tree, specifically targeting DiscordConfiguration object creation
	///     expressions.
	/// </summary>
	/// <param name="document">The document to be modified based on the specified criteria.</param>
	/// <param name="overrideValue">The override value to apply to matching DiscordConfiguration initializers.</param>
	/// <param name="cancellationToken">Used to signal cancellation of the operation if needed.</param>
	/// <returns>Returns the modified document if changes were made; otherwise, returns the original document.</returns>
	public static async Task<Document> ApplyFixToDocumentAsync(Document document, string overrideValue, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (root is null || semanticModel is null)
			return document;

		var discordConfigurationSymbol = semanticModel.Compilation.GetTypeByMetadataName(DiscordConfigurationMetadataName);
		if (discordConfigurationSymbol is null)
			return document;

		var configNodes = root.DescendantNodes()
			.OfType<ExpressionSyntax>()
			.Where(node => node is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax)
			.Where(node => IsDiscordConfigurationCreation(semanticModel, discordConfigurationSymbol, node, cancellationToken))
			.ToImmutableArray();

		if (configNodes.IsDefaultOrEmpty)
			return document;

		var replacementNodes = configNodes
			.Select(node => (Original: (SyntaxNode)node, Updated: TryCreateUpdatedNode(node, overrideValue)))
			.Where(x => x.Updated is not null)
			.ToImmutableDictionary(x => x.Original, x => x.Updated!);

		if (replacementNodes.Count == 0)
			return document;

		var newRoot = root.ReplaceNodes(replacementNodes.Keys, (original, _) => replacementNodes[original]);
		return await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot), cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	private static bool IsDiscordConfigurationCreation(SemanticModel semanticModel, INamedTypeSymbol discordConfigurationSymbol, ExpressionSyntax node, CancellationToken cancellationToken)
		=> CodeFixSemanticHelpers.IsCreationOfType(semanticModel, node, discordConfigurationSymbol, cancellationToken);

	private static SyntaxNode? TryCreateUpdatedNode(ExpressionSyntax node, string overrideValue)
		=> node switch
		{
			ObjectCreationExpressionSyntax objectCreation when TryCreateUpdatedInitializer(objectCreation.Initializer, overrideValue, out var updatedObjectInitializer)
				=> objectCreation.WithInitializer(updatedObjectInitializer).WithAdditionalAnnotations(Formatter.Annotation),
			ImplicitObjectCreationExpressionSyntax implicitObjectCreation when TryCreateUpdatedInitializer(implicitObjectCreation.Initializer, overrideValue, out var updatedImplicitInitializer)
				=> implicitObjectCreation.WithInitializer(updatedImplicitInitializer).WithAdditionalAnnotations(Formatter.Annotation),
			_ => null
		};

	private static bool TryCreateUpdatedInitializer(InitializerExpressionSyntax? initializer, string overrideValue, out InitializerExpressionSyntax updatedInitializer)
		=> CodeFixSyntaxHelpers.TryAddObjectInitializerAssignment(initializer, OverridePropertyName, CreateOverrideAssignment(overrideValue), out updatedInitializer);

	private static AssignmentExpressionSyntax CreateOverrideAssignment(string overrideValue)
		=> CodeFixSyntaxHelpers.CreateStringPropertyAssignment(OverridePropertyName, overrideValue);

	/// <summary>
	///     Checks if the given left-hand side expression represents an assignment to "Override".
	///     Supports both IdentifierNameSyntax and MemberAccessExpressionSyntax.
	/// </summary>
	internal static bool IsOverrideProperty(ExpressionSyntax left)
		=> CodeFixSyntaxHelpers.IsAssignedProperty(left, OverridePropertyName);
}
