using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Analyzer;

/// <summary>
///     Code fix for DCS1301: replaces <c>using</c> with <c>await using</c> on
///     DisCatSharp async-disposable client declarations.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpAsyncDisposalUsingCodeFix)), Shared]
public sealed class DisCatSharpAsyncDisposalUsingCodeFix : SingleDiagnosticCodeFixProvider
{
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.AsyncDisposalUsingMigration;

	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
		{
			var root = context.Document.GetSyntaxRootAsync(context.CancellationToken).GetAwaiter().GetResult();
			if (root is null)
				continue;

			var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
			if (!AsyncDisposalMigrationAnalysis.IsInAsyncContext(node))
				continue;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Use 'await using' for async disposal",
					c => ApplyFixAsync(context.Document, diagnostic, c),
					this.FixableDiagnosticId),
				diagnostic);
		}

		return Task.CompletedTask;
	}

	internal static async Task<Document> ApplyFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
			return document;

		var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
		if (!AsyncDisposalMigrationAnalysis.IsInAsyncContext(node))
			return document;

		// Walk up to the owning statement
		var localDecl = node.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
		if (localDecl is not null && localDecl.UsingKeyword != default && localDecl.AwaitKeyword == default)
		{
			var newDecl = localDecl.WithAwaitKeyword(
				SyntaxFactory.Token(SyntaxKind.AwaitKeyword)
					.WithLeadingTrivia(localDecl.UsingKeyword.LeadingTrivia)
					.WithTrailingTrivia(SyntaxFactory.Space));

			// Move the using keyword's leading trivia to the new await keyword
			var newUsing = newDecl.UsingKeyword.WithLeadingTrivia(SyntaxTriviaList.Empty);
			newDecl = newDecl.WithUsingKeyword(newUsing);

			var newRoot = root.ReplaceNode(localDecl, newDecl);
			return document.WithSyntaxRoot(newRoot);
		}

		var usingStmt = node.FirstAncestorOrSelf<UsingStatementSyntax>();
		if (usingStmt is not null && usingStmt.AwaitKeyword == default)
		{
			var newStmt = usingStmt.WithAwaitKeyword(
				SyntaxFactory.Token(SyntaxKind.AwaitKeyword)
					.WithLeadingTrivia(usingStmt.UsingKeyword.LeadingTrivia)
					.WithTrailingTrivia(SyntaxFactory.Space));

			var newUsing = newStmt.UsingKeyword.WithLeadingTrivia(SyntaxTriviaList.Empty);
			newStmt = newStmt.WithUsingKeyword(newUsing);

			var newRoot = root.ReplaceNode(usingStmt, newStmt);
			return document.WithSyntaxRoot(newRoot);
		}

		return document;
	}
}
