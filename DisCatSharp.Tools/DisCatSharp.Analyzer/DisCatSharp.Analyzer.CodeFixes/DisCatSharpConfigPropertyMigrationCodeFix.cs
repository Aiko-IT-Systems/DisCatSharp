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

		// Case 1: MemberAccessExpression (e.g., config.ApiVersion)
		var memberAccess = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
		if (memberAccess is not null)
		{
			var expression = memberAccess.Expression;
			var pathParts = nestedPath.Split('.');
			ExpressionSyntax current = expression;
			foreach (var part in pathParts)
			{
				current = SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					current,
					SyntaxFactory.IdentifierName(part));
			}

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

		// Case 2: Object initializer assignment (e.g., ApiChannel = value inside new DiscordConfiguration { ... })
		if (node is IdentifierNameSyntax identifier &&
			identifier.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } assignment)
		{
			var replacement = BuildNestedInitializerAssignment(nestedPath, newName, assignment.Right);
			var newRoot = root.ReplaceNode(assignment, replacement.WithTriviaFrom(assignment));
			return await Formatter.FormatAsync(
				document.WithSyntaxRoot(newRoot),
				cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		return document;
	}

	private static ExpressionSyntax BuildNestedInitializerAssignment(string nestedPath, string newName, ExpressionSyntax value)
	{
		var pathParts = nestedPath.Split('.');

		// Build innermost: NewName = value
		ExpressionSyntax current = SyntaxFactory.AssignmentExpression(
			SyntaxKind.SimpleAssignmentExpression,
			SyntaxFactory.IdentifierName(newName),
			value);

		// Wrap in nested initializers from innermost to outermost
		for (var i = pathParts.Length - 1; i >= 0; i--)
		{
			current = SyntaxFactory.AssignmentExpression(
				SyntaxKind.SimpleAssignmentExpression,
				SyntaxFactory.IdentifierName(pathParts[i]),
				SyntaxFactory.InitializerExpression(
					SyntaxKind.ObjectInitializerExpression,
					SyntaxFactory.SingletonSeparatedList(current)));
		}

		return current;
	}
}
