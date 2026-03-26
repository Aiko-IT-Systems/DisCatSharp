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
		    !ApplicationCommandChecksFailedMigrationAnalysis.TryGetCandidate(semanticModel, assignment, cancellationToken, out var candidate))
			return document;

		var updatedAssignment = UpdateAssignment(candidate);
		var newRoot = root.ReplaceNode(candidate.Assignment, updatedAssignment);
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
