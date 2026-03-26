using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Analyzer;

internal static class ApplicationCommandChecksFailedMigrationAnalysis
{
	private const string ApplicationCommandsExtensionMetadataName = "DisCatSharp.ApplicationCommands.ApplicationCommandsExtension";
	private const string SlashExecutionChecksFailedExceptionMetadataName = "DisCatSharp.ApplicationCommands.Exceptions.SlashExecutionChecksFailedException";
	private const string ContextMenuExecutionChecksFailedExceptionMetadataName = "DisCatSharp.ApplicationCommands.Exceptions.ContextMenuExecutionChecksFailedException";
	private const string SlashCommandChecksFailedEventArgsTypeName = "global::DisCatSharp.ApplicationCommands.EventArgs.SlashCommandChecksFailedEventArgs";
	private const string ContextMenuChecksFailedEventArgsTypeName = "global::DisCatSharp.ApplicationCommands.EventArgs.ContextMenuChecksFailedEventArgs";
	private const string FailedChecksPropertyName = "FailedChecks";
	private const string ExceptionPropertyName = "Exception";

	private static readonly ImmutableDictionary<string, ApplicationCommandChecksFailedMigrationMapping> s_mappings
		= ImmutableDictionary.CreateRange(
		[
			new KeyValuePair<string, ApplicationCommandChecksFailedMigrationMapping>(
				"SlashCommandErrored",
				new(
					"SlashCommandErrored",
					"SlashCommandChecksFailed",
					SlashExecutionChecksFailedExceptionMetadataName,
					SlashCommandChecksFailedEventArgsTypeName)),
			new KeyValuePair<string, ApplicationCommandChecksFailedMigrationMapping>(
				"ContextMenuErrored",
				new(
					"ContextMenuErrored",
					"ContextMenuChecksFailed",
					ContextMenuExecutionChecksFailedExceptionMetadataName,
					ContextMenuChecksFailedEventArgsTypeName))
		]);

	internal static bool TryGetDiagnosticCandidate(SemanticModel semanticModel, AssignmentExpressionSyntax assignment, CancellationToken cancellationToken, out ApplicationCommandChecksFailedMigrationCandidate candidate)
	{
		candidate = null!;

		if (assignment.Kind() != SyntaxKind.AddAssignmentExpression ||
		    assignment.Left is not MemberAccessExpressionSyntax eventAccess ||
		    assignment.Right is not AnonymousFunctionExpressionSyntax handler)
			return false;

		if (!TryGetEventMapping(semanticModel, eventAccess, cancellationToken, out var mapping))
			return false;

		if (!TryGetHandlerInfo(handler, out var handlerInfo))
			return false;

		if (!TryFindGuardIfStatement(semanticModel, handlerInfo.HandlerBlock, handlerInfo.ArgumentsIdentifier, mapping.ExpectedExceptionMetadataName, cancellationToken, out var guardIfStatement, out var failedExceptionIdentifier))
			return false;

		var protectedStatements = GetProtectedStatements(guardIfStatement);
		if (!AreStatementsSupported(protectedStatements, handlerInfo.ArgumentsIdentifier, failedExceptionIdentifier))
			return false;

		var canAutoFix = IsFullyGuardedHandler(handlerInfo.HandlerBlock, guardIfStatement) ||
		                 CanExtractGuardedBranch(handlerInfo.HandlerBlock, guardIfStatement, handlerInfo.ArgumentsIdentifier);

		candidate = new(
			assignment,
			eventAccess,
			handler,
			handlerInfo.HandlerBlock,
			guardIfStatement,
			handlerInfo.ArgumentsIdentifier,
			mapping.SourceEventName,
			mapping.TargetEventName,
			mapping.TargetEventArgsTypeName,
			failedExceptionIdentifier,
			canAutoFix);

		return true;
	}

	internal static bool TryGetCodeFixCandidate(SemanticModel semanticModel, AssignmentExpressionSyntax assignment, CancellationToken cancellationToken, out ApplicationCommandChecksFailedMigrationCandidate candidate)
	{
		if (TryGetDiagnosticCandidate(semanticModel, assignment, cancellationToken, out candidate) &&
		    candidate.CanAutoFix)
			return true;

		candidate = null!;
		return false;
	}

	internal static ImmutableArray<StatementSyntax> GetMigratedStatements(ApplicationCommandChecksFailedMigrationCandidate candidate)
	{
		var statements = candidate.GuardIfStatement.Statement switch
		{
			BlockSyntax guardedBlock => guardedBlock.Statements.ToImmutableArray(),
			StatementSyntax guardedStatement => [guardedStatement]
		};
		if (candidate.FailedExceptionIdentifier is null)
			return statements;

		var rewriter = new FailedChecksAccessRewriter(candidate.FailedExceptionIdentifier, candidate.ArgumentsIdentifier);
		return statements
			.Select(statement => (StatementSyntax)rewriter.Visit(statement)!)
			.ToImmutableArray();
	}

	private static bool TryGetEventMapping(SemanticModel semanticModel, MemberAccessExpressionSyntax eventAccess, CancellationToken cancellationToken, out ApplicationCommandChecksFailedMigrationMapping mapping)
	{
		mapping = null!;

		if (semanticModel.GetSymbolInfo(eventAccess, cancellationToken).Symbol is not IEventSymbol eventSymbol ||
		    eventSymbol.ContainingType.ToDisplayString() != ApplicationCommandsExtensionMetadataName)
			return false;

		if (!s_mappings.TryGetValue(eventSymbol.Name, out var resolvedMapping) || resolvedMapping is null)
			return false;

		mapping = resolvedMapping;
		return true;
	}

	private static bool TryGetHandlerInfo(AnonymousFunctionExpressionSyntax handler, out ApplicationCommandChecksFailedMigrationHandlerInfo handlerInfo)
	{
		handlerInfo = null!;

		switch (handler)
		{
			case ParenthesizedLambdaExpressionSyntax parenthesizedLambda
				when parenthesizedLambda.Body is BlockSyntax handlerBlock &&
				     parenthesizedLambda.ParameterList.Parameters.Count == 2 &&
				     parenthesizedLambda.ParameterList.Parameters[1].Identifier.ValueText is { Length: > 0 } argumentsIdentifier:
				handlerInfo = new(handlerBlock, argumentsIdentifier);
				return true;

			case AnonymousMethodExpressionSyntax anonymousMethod
				when anonymousMethod.Block is { } anonymousHandlerBlock &&
				     anonymousMethod.ParameterList is { Parameters.Count: 2 } parameterList &&
				     parameterList.Parameters[1].Identifier.ValueText is { Length: > 0 } anonymousArgumentsIdentifier:
				handlerInfo = new(anonymousHandlerBlock, anonymousArgumentsIdentifier);
				return true;

			default:
				return false;
		}
	}

	private static bool TryFindGuardIfStatement(SemanticModel semanticModel, BlockSyntax handlerBlock, string argumentsIdentifier, string expectedExceptionMetadataName, CancellationToken cancellationToken, out IfStatementSyntax guardIfStatement, out string? failedExceptionIdentifier)
	{
		guardIfStatement = null!;
		failedExceptionIdentifier = null;

		foreach (var candidateGuard in handlerBlock.DescendantNodes().OfType<IfStatementSyntax>())
		{
			if (candidateGuard.Else is not null)
				continue;

			if (!TryMatchGuardCondition(semanticModel, argumentsIdentifier, candidateGuard.Condition, expectedExceptionMetadataName, cancellationToken, out failedExceptionIdentifier))
				continue;

			guardIfStatement = candidateGuard;
			return true;
		}

		return false;
	}

	private static bool TryMatchGuardCondition(SemanticModel semanticModel, string argumentsIdentifier, ExpressionSyntax condition, string expectedExceptionMetadataName, CancellationToken cancellationToken, out string? failedExceptionIdentifier)
	{
		failedExceptionIdentifier = null;

		if (condition is not IsPatternExpressionSyntax
		    {
			    Expression: MemberAccessExpressionSyntax
			    {
				    Expression: IdentifierNameSyntax { Identifier.ValueText: var identifierName },
				    Name.Identifier.ValueText: ExceptionPropertyName
			    }
		    } isPattern ||
		    identifierName != argumentsIdentifier)
			return false;

		var expectedExceptionType = semanticModel.Compilation.GetTypeByMetadataName(expectedExceptionMetadataName);
		if (expectedExceptionType is null)
			return false;

		switch (isPattern.Pattern)
		{
			case DeclarationPatternSyntax { Type: { } declarationType, Designation: SingleVariableDesignationSyntax designation }:
				if (!IsExpectedExceptionType(semanticModel, declarationType, expectedExceptionType, cancellationToken))
					return false;

				failedExceptionIdentifier = designation.Identifier.ValueText;
				return true;

			case TypePatternSyntax { Type: { } typePattern }:
				return IsExpectedExceptionType(semanticModel, typePattern, expectedExceptionType, cancellationToken);

			default:
				return false;
		}
	}

	private static bool IsExpectedExceptionType(SemanticModel semanticModel, TypeSyntax typeSyntax, INamedTypeSymbol expectedExceptionType, CancellationToken cancellationToken)
		=> SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(typeSyntax, cancellationToken).Type, expectedExceptionType);

	private static bool CanExtractGuardedBranch(BlockSyntax handlerBlock, IfStatementSyntax guardIfStatement, string argumentsIdentifier)
	{
		if (guardIfStatement.Parent is not BlockSyntax currentBlock || currentBlock == handlerBlock)
			return false;

		StatementSyntax? topLevelPathStatement = null;
		while (currentBlock != handlerBlock)
		{
			if (!TryGetContainingStatement(currentBlock, out var parentBlock, out var pathStatement))
				return false;

			if (parentBlock != handlerBlock && !ReferenceEquals(parentBlock.Statements.FirstOrDefault(), pathStatement))
				return false;

			if (parentBlock == handlerBlock)
				topLevelPathStatement = pathStatement;

			currentBlock = parentBlock;
		}

		if (topLevelPathStatement is null)
			return false;

		foreach (var statement in handlerBlock.Statements)
		{
			if (ReferenceEquals(statement, topLevelPathStatement) ||
			    IsHandledAssignment(statement, argumentsIdentifier) ||
			    IsSafeCompletionStatement(statement))
				continue;

			return false;
		}

		return true;
	}

	private static bool IsFullyGuardedHandler(BlockSyntax handlerBlock, IfStatementSyntax guardIfStatement)
	{
		return handlerBlock.Statements.Count == 1 &&
		       ReferenceEquals(handlerBlock.Statements[0], guardIfStatement) &&
		       guardIfStatement.Else is null;
	}

	internal static bool TryGetContainingStatement(BlockSyntax currentBlock, out BlockSyntax parentBlock, out StatementSyntax pathStatement)
	{
		parentBlock = null!;
		pathStatement = null!;

		pathStatement = currentBlock
			.Ancestors()
			.OfType<StatementSyntax>()
			.FirstOrDefault(statement => statement.Parent is BlockSyntax block && block.Span.Contains(currentBlock.Span))!;
		if (pathStatement is null || pathStatement.Parent is not BlockSyntax containingBlock)
			return false;

		parentBlock = containingBlock;
		return true;
	}

	private static ImmutableArray<StatementSyntax> GetProtectedStatements(IfStatementSyntax guardIfStatement)
	{
		return guardIfStatement.Statement switch
		{
			BlockSyntax guardedBlock => guardedBlock.Statements.ToImmutableArray(),
			StatementSyntax guardedStatement => [guardedStatement]
		};
	}

	private static bool AreStatementsSupported(ImmutableArray<StatementSyntax> statements, string argumentsIdentifier, string? failedExceptionIdentifier)
	{
		foreach (var statement in statements)
		{
			if (statement.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>().Any(x => IsExceptionAccess(x, argumentsIdentifier)))
				return false;

			if (failedExceptionIdentifier is null)
				continue;

			foreach (var identifier in statement.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(x => x.Identifier.ValueText == failedExceptionIdentifier))
			{
				if (identifier.Parent is MemberAccessExpressionSyntax memberAccess &&
				    ReferenceEquals(memberAccess.Expression, identifier) &&
				    memberAccess.Name.Identifier.ValueText == FailedChecksPropertyName)
					continue;

				return false;
			}
		}

		return true;
	}

	private static bool IsExceptionAccess(MemberAccessExpressionSyntax memberAccess, string argumentsIdentifier)
		=> memberAccess.Expression is IdentifierNameSyntax { Identifier.ValueText: var identifierName } &&
		   identifierName == argumentsIdentifier &&
		   memberAccess.Name.Identifier.ValueText == ExceptionPropertyName;

	private static bool IsHandledAssignment(StatementSyntax statement, string argumentsIdentifier)
		=> statement is ExpressionStatementSyntax
		{
			Expression: AssignmentExpressionSyntax
			{
				Left: MemberAccessExpressionSyntax
				{
					Expression: IdentifierNameSyntax { Identifier.ValueText: var identifierName },
					Name.Identifier.ValueText: "Handled"
				}
			}
		} && identifierName == argumentsIdentifier;

	private static bool IsSafeCompletionStatement(StatementSyntax statement)
		=> statement is ReturnStatementSyntax;

	internal sealed class ApplicationCommandChecksFailedMigrationCandidate
	{
		internal ApplicationCommandChecksFailedMigrationCandidate(
			AssignmentExpressionSyntax assignment,
			MemberAccessExpressionSyntax eventAccess,
			AnonymousFunctionExpressionSyntax handler,
			BlockSyntax handlerBlock,
			IfStatementSyntax guardIfStatement,
			string argumentsIdentifier,
			string sourceEventName,
			string targetEventName,
			string targetEventArgsTypeName,
			string? failedExceptionIdentifier,
			bool canAutoFix)
		{
			this.Assignment = assignment;
			this.EventAccess = eventAccess;
			this.Handler = handler;
			this.HandlerBlock = handlerBlock;
			this.GuardIfStatement = guardIfStatement;
			this.ArgumentsIdentifier = argumentsIdentifier;
			this.SourceEventName = sourceEventName;
			this.TargetEventName = targetEventName;
			this.TargetEventArgsTypeName = targetEventArgsTypeName;
			this.FailedExceptionIdentifier = failedExceptionIdentifier;
			this.CanAutoFix = canAutoFix;
		}

		internal AssignmentExpressionSyntax Assignment { get; }

		internal MemberAccessExpressionSyntax EventAccess { get; }

		internal AnonymousFunctionExpressionSyntax Handler { get; }

		internal BlockSyntax HandlerBlock { get; }

		internal IfStatementSyntax GuardIfStatement { get; }

		internal string ArgumentsIdentifier { get; }

		internal string SourceEventName { get; }

		internal string TargetEventName { get; }

		internal string TargetEventArgsTypeName { get; }

		internal string? FailedExceptionIdentifier { get; }

		internal bool CanAutoFix { get; }
	}

	private sealed class ApplicationCommandChecksFailedMigrationHandlerInfo
	{
		internal ApplicationCommandChecksFailedMigrationHandlerInfo(BlockSyntax handlerBlock, string argumentsIdentifier)
		{
			this.HandlerBlock = handlerBlock;
			this.ArgumentsIdentifier = argumentsIdentifier;
		}

		internal BlockSyntax HandlerBlock { get; }

		internal string ArgumentsIdentifier { get; }
	}

	private sealed class ApplicationCommandChecksFailedMigrationMapping
	{
		internal ApplicationCommandChecksFailedMigrationMapping(string sourceEventName, string targetEventName, string expectedExceptionMetadataName, string targetEventArgsTypeName)
		{
			this.SourceEventName = sourceEventName;
			this.TargetEventName = targetEventName;
			this.ExpectedExceptionMetadataName = expectedExceptionMetadataName;
			this.TargetEventArgsTypeName = targetEventArgsTypeName;
		}

		internal string SourceEventName { get; }

		internal string TargetEventName { get; }

		internal string ExpectedExceptionMetadataName { get; }

		internal string TargetEventArgsTypeName { get; }
	}

	private sealed class FailedChecksAccessRewriter(string failedExceptionIdentifier, string argumentsIdentifier) : CSharpSyntaxRewriter
	{
		public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			if (node.Expression is IdentifierNameSyntax { Identifier.ValueText: var identifierName } &&
			    identifierName == failedExceptionIdentifier &&
			    node.Name.Identifier.ValueText == FailedChecksPropertyName)
			{
				return SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.IdentifierName(argumentsIdentifier),
						SyntaxFactory.IdentifierName(FailedChecksPropertyName))
					.WithTriviaFrom(node);
			}

			return base.VisitMemberAccessExpression(node);
		}
	}
}
