using System.Collections.Immutable;
using System.Composition;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpPresenceAccessMigrationCodeFix)), Shared]
public sealed class DisCatSharpPresenceAccessMigrationCodeFix : SingleDiagnosticCodeFixProvider
{
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.PresenceAccessMigration;

	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
		{
			var userExpression = diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.PresenceUserExpression, out var configuredUserExpression) &&
			                     !string.IsNullOrWhiteSpace(configuredUserExpression)
				? configuredUserExpression
				: "the user id";

			context.RegisterCodeFix(
				CodeAction.Create(
					$"Use GetPresences({userExpression})",
					c => ApplyFixToDocumentAsync(context.Document, diagnostic, c),
					$"{this.FixableDiagnosticId}:{userExpression}"),
				diagnostic);
		}

		return Task.CompletedTask;
	}

	internal static async Task<Document> ApplyFixToDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (root is null || semanticModel is null)
			return document;

		var targetNode = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
		var invocation = targetNode.AncestorsAndSelf().OfType<InvocationExpressionSyntax>()
			.FirstOrDefault(x => PresenceAccessMigrationAnalysis.TryGetCandidate(semanticModel, x, cancellationToken, out _));
		if (invocation is null ||
		    !PresenceAccessMigrationAnalysis.TryGetCandidate(semanticModel, invocation, cancellationToken, out var candidate))
			return document;

		var replacement = CreateReplacement(candidate);
		var newRoot = root.ReplaceNode(candidate.InvocationToReplace, replacement);
		return await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot), cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	private static ExpressionSyntax CreateReplacement(PresenceAccessMigrationAnalysis.PresenceAccessMigrationCandidate candidate)
	{
		var getPresencesInvocation = SyntaxFactory.InvocationExpression(
			SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				candidate.ClientExpression.WithoutTrivia(),
				SyntaxFactory.IdentifierName("GetPresences")),
			SyntaxFactory.ArgumentList(
			[
				SyntaxFactory.Argument(candidate.UserExpression.WithoutTrivia())
			]));

		return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				getPresencesInvocation,
				SyntaxFactory.IdentifierName("Values"))
			.WithTriviaFrom(candidate.InvocationToReplace)
			.WithAdditionalAnnotations(Formatter.Annotation);
	}
}
