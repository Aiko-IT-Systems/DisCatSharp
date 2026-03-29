using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DisCatSharp.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpConfigPropertyMigrationCodeFix))]
[Shared]
public sealed class DisCatSharpConfigPropertyMigrationCodeFix : SingleDiagnosticCodeFixProvider
{
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.ConfigPropertyMigration;

	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
		{
			if (!diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.ConfigNestedPath, out var nestedPath) ||
				!diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.ConfigNewName, out var newName) ||
				nestedPath is null || newName is null)
				continue;

			var fullPath = $"{nestedPath}.{newName}";

			context.RegisterCodeFix(
				CodeAction.Create(
					$"Use {fullPath} instead",
					c => ApplyFixToDocumentAsync(context.Document, diagnostic, nestedPath, newName, c),
					$"{this.FixableDiagnosticId}:{fullPath}"),
				diagnostic);
		}

		return Task.CompletedTask;
	}

	internal static async Task<Document> ApplyFixToDocumentAsync(
		Document document,
		Diagnostic diagnostic,
		string nestedPath,
		string newName,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
			return document;

		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var node = root.FindNode(diagnosticSpan);

		// Find the parent MemberAccessExpression (e.g., config.ApiVersion)
		var memberAccess = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
		if (memberAccess is null)
			return document;

		// Build the replacement: config.Api.Version (or config.Diagnostics.UpdateChecks.Disabled)
		var expression = memberAccess.Expression; // "config"

		// Build nested path: split "Diagnostics.UpdateChecks" into parts
		var pathParts = nestedPath.Split('.');
		ExpressionSyntax current = expression;
		foreach (var part in pathParts)
		{
			current = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				current,
				SyntaxFactory.IdentifierName(part));
		}

		// Add the final property name
		var replacement = SyntaxFactory.MemberAccessExpression(
			SyntaxKind.SimpleMemberAccessExpression,
			current,
			SyntaxFactory.IdentifierName(newName))
			.WithTriviaFrom(memberAccess);

		var newRoot = root.ReplaceNode(memberAccess, replacement);
		return await Formatter.FormatAsync(
			document.WithSyntaxRoot(newRoot),
			cancellationToken: cancellationToken).ConfigureAwait(false);
	}
}
