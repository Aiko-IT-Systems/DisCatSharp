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

namespace DisCatSharp.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpBanDeleteMessageDaysMigrationCodeFix))]
[Shared]
public sealed class DisCatSharpBanDeleteMessageDaysMigrationCodeFix : SingleDiagnosticCodeFixProvider
{
	protected override string FixableDiagnosticId => DisCatSharpDiagnosticIds.BanDeleteMessageDaysMigration;

	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		context.RegisterCodeFix(
		CodeAction.Create(
		"Rename to 'deleteMessageSeconds' (convert literal days -> seconds)",
		ct => ApplyFixAsync(context.Document, diagnostics[0], ct),
		equivalenceKey: nameof(DisCatSharpBanDeleteMessageDaysMigrationCodeFix)),
		diagnostics[0]);

		return Task.CompletedTask;
	}

	internal static async Task<Document> ApplyFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
			return document;

		var nameColonNode = root.FindNode(diagnostic.Location.SourceSpan) as NameColonSyntax;
		if (nameColonNode is null)
			return document;

		var argument = nameColonNode.Parent as ArgumentSyntax;
		if (argument is null)
			return document;

		// New name colon: deleteMessageSeconds:
		var newNameColon = SyntaxFactory.NameColon(
		SyntaxFactory.IdentifierName("deleteMessageSeconds"))
		.WithTriviaFrom(nameColonNode);

		// If the argument expression is a numeric literal, multiply by 86400
		ArgumentSyntax newArgument;
		if (argument.Expression is LiteralExpressionSyntax { Token.Value: int days })
		{
			var seconds = days * 86400;
			var newLiteral = SyntaxFactory.LiteralExpression(
			SyntaxKind.NumericLiteralExpression,
			SyntaxFactory.Literal(seconds))
			.WithTriviaFrom(argument.Expression);
			newArgument = argument.WithNameColon(newNameColon).WithExpression(newLiteral);
		}
		else
		{
			// Non-literal: rename only, add TODO comment
			var todoComment = SyntaxFactory.Comment("/* TODO: was days, now seconds - multiply by 86400 if needed */");
			var newExpression = argument.Expression.WithLeadingTrivia(
			argument.Expression.GetLeadingTrivia().Add(todoComment).Add(SyntaxFactory.Space));
			newArgument = argument.WithNameColon(newNameColon).WithExpression(newExpression);
		}

		var newRoot = root.ReplaceNode(argument, newArgument);
		return document.WithSyntaxRoot(newRoot);
	}
}
