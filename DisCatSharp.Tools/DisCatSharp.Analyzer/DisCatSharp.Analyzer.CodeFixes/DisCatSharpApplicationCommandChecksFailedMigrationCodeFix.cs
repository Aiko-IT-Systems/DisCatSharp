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

/// <inheritdoc />
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpApplicationCommandChecksFailedMigrationCodeFix)), Shared]
public sealed class DisCatSharpApplicationCommandChecksFailedMigrationCodeFix : SingleDiagnosticCodeFixProvider
{
	/// <inheritdoc />
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.ApplicationCommandChecksFailedMigration;

	/// <inheritdoc />
	protected override Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
		{
			if (!diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.MigrationCanAutoFix, out var canAutoFix) ||
			    !bool.TryParse(canAutoFix, out var isAutoFixable) ||
			    !isAutoFixable)
				continue;

			var targetEventName = diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.MigrationTargetEventName, out var configuredTargetEventName) &&
			                      !string.IsNullOrWhiteSpace(configuredTargetEventName)
				? configuredTargetEventName
				: "the dedicated checks-failed event";

			context.RegisterCodeFix(
				CodeAction.Create(
					$"Migrate handler to '{targetEventName}'",
					c => ApplyFixToDocumentAsync(context.Document, diagnostic, c),
					$"{this.FixableDiagnosticId}:{targetEventName}"),
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
		var assignment = targetNode.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
		if (assignment is null ||
		    !ApplicationCommandChecksFailedMigrationAnalysis.TryGetCodeFixCandidate(semanticModel, assignment, cancellationToken, out var candidate))
			return document;

		if (candidate.HandlerBlock.Statements.Count == 1)
		{
			var updatedAssignment = UpdateAssignment(candidate);
			var replacedRoot = root.ReplaceNode(candidate.Assignment, updatedAssignment);
			return await Formatter.FormatAsync(document.WithSyntaxRoot(replacedRoot), cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		if (candidate.Assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>() is not { } assignmentStatement ||
		    !TryCreateSplitAssignments(candidate, out var extractedAssignment, out var updatedSourceAssignment))
			return document;

		var extractedStatement = SyntaxFactory.ExpressionStatement(extractedAssignment)
			.WithLeadingTrivia(assignmentStatement.GetLeadingTrivia())
			.WithAdditionalAnnotations(Formatter.Annotation);
		var updatedStatement = assignmentStatement
			.WithExpression(updatedSourceAssignment)
			.WithLeadingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)
			.WithAdditionalAnnotations(Formatter.Annotation);

		var newRoot = root.ReplaceNode(assignmentStatement, [extractedStatement, updatedStatement]);
		return await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot), cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	private static AssignmentExpressionSyntax UpdateAssignment(ApplicationCommandChecksFailedMigrationAnalysis.ApplicationCommandChecksFailedMigrationCandidate candidate)
	{
		var updatedHandler = UpdateHandler(candidate);
		var updatedEventAccess = candidate.EventAccess.WithName(
			SyntaxFactory.IdentifierName(candidate.TargetEventName)
				.WithTriviaFrom(candidate.EventAccess.Name));

		return candidate.Assignment
			.WithLeft(updatedEventAccess)
			.WithRight(updatedHandler)
			.WithAdditionalAnnotations(Formatter.Annotation);
	}

	private static ExpressionSyntax UpdateHandler(ApplicationCommandChecksFailedMigrationAnalysis.ApplicationCommandChecksFailedMigrationCandidate candidate)
	{
		var updatedStatements = ApplicationCommandChecksFailedMigrationAnalysis.GetMigratedStatements(candidate);
		var updatedBlock = SyntaxFactory.Block(updatedStatements)
			.WithTriviaFrom(candidate.HandlerBlock)
			.WithAdditionalAnnotations(Formatter.Annotation);

		return candidate.Handler switch
		{
			ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda
				.WithParameterList(UpdateParameterList(parenthesizedLambda.ParameterList, candidate.TargetEventArgsTypeName))
				.WithBody(updatedBlock)
				.WithAdditionalAnnotations(Formatter.Annotation),
			AnonymousMethodExpressionSyntax anonymousMethod when anonymousMethod.ParameterList is { } parameterList => anonymousMethod
				.WithParameterList(UpdateParameterList(parameterList, candidate.TargetEventArgsTypeName))
				.WithBlock(updatedBlock)
				.WithAdditionalAnnotations(Formatter.Annotation),
			AnonymousMethodExpressionSyntax anonymousMethod => anonymousMethod.WithBlock(updatedBlock).WithAdditionalAnnotations(Formatter.Annotation),
			_ => (ExpressionSyntax)candidate.Handler
		};
	}

	private static bool TryCreateSplitAssignments(
		ApplicationCommandChecksFailedMigrationAnalysis.ApplicationCommandChecksFailedMigrationCandidate candidate,
		out AssignmentExpressionSyntax extractedAssignment,
		out AssignmentExpressionSyntax updatedSourceAssignment)
	{
		extractedAssignment = null!;
		updatedSourceAssignment = null!;

		if (candidate.GuardIfStatement.Parent is not BlockSyntax guardContainerBlock)
			return false;

		if (!TryBuildExtractedHandlerBlock(candidate, guardContainerBlock, out var extractedHandlerBlock))
			return false;

		var updatedGuardContainerBlock = guardContainerBlock
			.WithStatements(guardContainerBlock.Statements.Remove(candidate.GuardIfStatement))
			.WithAdditionalAnnotations(Formatter.Annotation);
		var updatedSourceHandlerBlock = candidate.HandlerBlock
			.ReplaceNode(guardContainerBlock, updatedGuardContainerBlock)
			.WithAdditionalAnnotations(Formatter.Annotation);

		var extractedHandler = UpdateHandler(candidate, extractedHandlerBlock);
		var updatedSourceHandler = UpdateHandler(candidate, updatedSourceHandlerBlock, keepSourceEventArgs: true);

		extractedAssignment = CreateAssignment(candidate, extractedHandler, candidate.TargetEventName);
		updatedSourceAssignment = CreateAssignment(candidate, updatedSourceHandler, candidate.SourceEventName);
		return true;
	}

	private static bool TryBuildExtractedHandlerBlock(
		ApplicationCommandChecksFailedMigrationAnalysis.ApplicationCommandChecksFailedMigrationCandidate candidate,
		BlockSyntax guardContainerBlock,
		out BlockSyntax extractedHandlerBlock)
	{
		extractedHandlerBlock = null!;

		var currentStatements = ApplicationCommandChecksFailedMigrationAnalysis.GetMigratedStatements(candidate);
		var currentBlock = guardContainerBlock;
		StatementSyntax? topLevelPathStatement = null;
		StatementSyntax? updatedTopLevelPathStatement = null;

		while (!ReferenceEquals(currentBlock, candidate.HandlerBlock))
		{
			if (!ApplicationCommandChecksFailedMigrationAnalysis.TryGetContainingStatement(currentBlock, out var parentBlock, out var pathStatement))
				return false;

			var updatedCurrentBlock = SyntaxFactory.Block(currentStatements)
				.WithTriviaFrom(currentBlock)
				.WithAdditionalAnnotations(Formatter.Annotation);
			var updatedPathStatement = pathStatement.ReplaceNode(currentBlock, updatedCurrentBlock)
				.WithAdditionalAnnotations(Formatter.Annotation);

			if (ReferenceEquals(parentBlock, candidate.HandlerBlock))
			{
				topLevelPathStatement = pathStatement;
				updatedTopLevelPathStatement = updatedPathStatement;
			}

			currentStatements = [updatedPathStatement];
			currentBlock = parentBlock;
		}

		if (topLevelPathStatement is null || updatedTopLevelPathStatement is null)
			return false;

		var topLevelStatements = candidate.HandlerBlock.Statements
			.SelectMany(statement =>
			{
				if (ReferenceEquals(statement, topLevelPathStatement))
					return [updatedTopLevelPathStatement];

				return statement is ReturnStatementSyntax
					? [statement]
					: Enumerable.Empty<StatementSyntax>();
			})
			.ToList();

		extractedHandlerBlock = SyntaxFactory.Block(SyntaxFactory.List(topLevelStatements))
			.WithTriviaFrom(candidate.HandlerBlock)
			.WithAdditionalAnnotations(Formatter.Annotation);
		return true;
	}

	private static AssignmentExpressionSyntax CreateAssignment(
		ApplicationCommandChecksFailedMigrationAnalysis.ApplicationCommandChecksFailedMigrationCandidate candidate,
		ExpressionSyntax handler,
		string eventName)
	{
		var updatedEventAccess = candidate.EventAccess.WithName(
			SyntaxFactory.IdentifierName(eventName)
				.WithTriviaFrom(candidate.EventAccess.Name));

		return candidate.Assignment
			.WithLeft(updatedEventAccess)
			.WithRight(handler)
			.WithAdditionalAnnotations(Formatter.Annotation);
	}

	private static ExpressionSyntax UpdateHandler(
		ApplicationCommandChecksFailedMigrationAnalysis.ApplicationCommandChecksFailedMigrationCandidate candidate,
		BlockSyntax updatedBlock,
		bool keepSourceEventArgs = false)
	{
		return candidate.Handler switch
		{
			ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda
				.WithParameterList(keepSourceEventArgs
					? parenthesizedLambda.ParameterList
					: UpdateParameterList(parenthesizedLambda.ParameterList, candidate.TargetEventArgsTypeName))
				.WithBody(updatedBlock)
				.WithAdditionalAnnotations(Formatter.Annotation),
			AnonymousMethodExpressionSyntax anonymousMethod when anonymousMethod.ParameterList is { } parameterList => anonymousMethod
				.WithParameterList(keepSourceEventArgs
					? parameterList
					: UpdateParameterList(parameterList, candidate.TargetEventArgsTypeName))
				.WithBlock(updatedBlock)
				.WithAdditionalAnnotations(Formatter.Annotation),
			AnonymousMethodExpressionSyntax anonymousMethod => anonymousMethod.WithBlock(updatedBlock).WithAdditionalAnnotations(Formatter.Annotation),
			_ => (ExpressionSyntax)candidate.Handler
		};
	}

	private static ParameterListSyntax UpdateParameterList(ParameterListSyntax parameterList, string targetEventArgsTypeName)
	{
		if (parameterList.Parameters.Count < 2)
			return parameterList;

		var argumentsParameter = parameterList.Parameters[1];
		if (argumentsParameter.Type is null)
			return parameterList;

		var updatedArgumentsParameter = argumentsParameter.WithType(
			SyntaxFactory.ParseTypeName(targetEventArgsTypeName)
				.WithTriviaFrom(argumentsParameter.Type));

		return parameterList.WithParameters(parameterList.Parameters.Replace(argumentsParameter, updatedArgumentsParameter));
	}
}
