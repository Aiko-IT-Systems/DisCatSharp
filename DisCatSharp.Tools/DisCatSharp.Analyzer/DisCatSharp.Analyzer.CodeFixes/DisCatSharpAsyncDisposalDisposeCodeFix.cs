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
///     Code fix for DCS1302: replaces <c>.Dispose()</c> with <c>await .DisposeAsync()</c>
///     on DisCatSharp async-disposable client types.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpAsyncDisposalDisposeCodeFix)), Shared]
public sealed class DisCatSharpAsyncDisposalDisposeCodeFix : SingleDiagnosticCodeFixProvider
{
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.AsyncDisposalDisposeMigration;

	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					"Use 'await DisposeAsync()' instead",
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
		var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation?.Expression is not MemberAccessExpressionSyntax memberAccess)
			return document;

		// Replace .Dispose() → .DisposeAsync()
		var newMemberAccess = memberAccess.WithName(
			SyntaxFactory.IdentifierName("DisposeAsync")
				.WithTriviaFrom(memberAccess.Name));
		var newInvocation = invocation.WithExpression(newMemberAccess);

		// Wrap in await: await client.DisposeAsync()
		var awaitExpr = SyntaxFactory.AwaitExpression(newInvocation)
			.WithTriviaFrom(invocation);

		var newRoot = root.ReplaceNode(invocation, awaitExpr);
		return document.WithSyntaxRoot(newRoot);
	}
}
