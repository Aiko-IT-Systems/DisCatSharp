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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpPresencesPropertyRemovalCodeFix)), Shared]
public sealed class DisCatSharpPresencesPropertyRemovalCodeFix : SingleDiagnosticCodeFixProvider
{
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.PresencesPropertyRemoval;

	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					"Replace with GetPresences(userId)",
					c => ApplyFixAsync(context.Document, diagnostic, c),
					this.FixableDiagnosticId),
				diagnostic);
		}

		return Task.CompletedTask;
	}

	private static async Task<Document> ApplyFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
			return document;

		var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
		var memberAccess = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
		if (memberAccess is null)
			return document;

		var clientExpression = memberAccess.Expression;

		// Check if this is client.Presences[key] → replace with client.GetPresences(key)
		if (memberAccess.Parent is ElementAccessExpressionSyntax elementAccess &&
		    elementAccess.ArgumentList.Arguments.Count == 1)
		{
			var keyArg = elementAccess.ArgumentList.Arguments[0].Expression;

			var replacement = SyntaxFactory.InvocationExpression(
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						clientExpression.WithoutTrivia(),
						SyntaxFactory.IdentifierName("GetPresences")),
					SyntaxFactory.ArgumentList(
						SyntaxFactory.SeparatedList(new[]
						{
							SyntaxFactory.Argument(keyArg.WithoutTrivia())
						})))
				.WithTriviaFrom(elementAccess)
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newRoot = root.ReplaceNode(elementAccess, replacement);
			return document.WithSyntaxRoot(newRoot);
		}

		// For bare client.Presences access, replace with client.GetPresences(/* userId */)
		var bareReplacement = SyntaxFactory.InvocationExpression(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					clientExpression.WithoutTrivia(),
					SyntaxFactory.IdentifierName("GetPresences")),
				SyntaxFactory.ArgumentList(
					SyntaxFactory.SeparatedList(new[]
					{
						SyntaxFactory.Argument(
							SyntaxFactory.IdentifierName("userId")
								.WithLeadingTrivia(SyntaxFactory.Comment("/* TODO: pass the target user ID */ ")))
					})))
			.WithTriviaFrom(memberAccess)
			.WithAdditionalAnnotations(Formatter.Annotation);

		var newRoot2 = root.ReplaceNode(memberAccess, bareReplacement);
		return document.WithSyntaxRoot(newRoot2);
	}
}
