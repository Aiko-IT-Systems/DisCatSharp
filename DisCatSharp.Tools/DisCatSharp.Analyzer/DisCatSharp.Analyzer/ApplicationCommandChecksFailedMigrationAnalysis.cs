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

	internal static bool TryGetCandidate(SemanticModel semanticModel, AssignmentExpressionSyntax assignment, CancellationToken cancellationToken, out ApplicationCommandChecksFailedMigrationCandidate candidate)
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

		if (!TryMatchGuardCondition(semanticModel, handlerInfo.ArgumentsIdentifier, handlerInfo.GuardIfStatement.Condition, mapping.ExpectedExceptionMetadataName, cancellationToken, out var failedExceptionIdentifier))
			return false;

		var protectedStatements = GetProtectedStatements(handlerInfo.GuardIfStatement, handlerInfo.HandlerBlock);
		if (!AreStatementsSupported(protectedStatements, handlerInfo.ArgumentsIdentifier, failedExceptionIdentifier))
			return false;

		candidate = new(
			assignment,
			eventAccess,
			handler,
			handlerInfo.HandlerBlock,
			handlerInfo.GuardIfStatement,
			handlerInfo.ArgumentsIdentifier,
			mapping.SourceEventName,
			mapping.TargetEventName,
			mapping.TargetEventArgsTypeName,
			failedExceptionIdentifier);

		return true;
	}

	internal static ImmutableArray<StatementSyntax> GetMigratedStatements(ApplicationCommandChecksFailedMigrationCandidate candidate)
	{
		var leadingStatements = candidate.GuardIfStatement.Statement switch
		{
			BlockSyntax guardedBlock => guardedBlock.Statements.ToImmutableArray(),
			StatementSyntax guardedStatement => [guardedStatement]
		};

		var trailingStatements = candidate.HandlerBlock.Statements
			.Skip(1)
			.ToImmutableArray();

		var statements = leadingStatements.AddRange(trailingStatements);
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

		return s_mappings.TryGetValue(eventSymbol.Name, out mapping) && mapping is not null;
	}

	private static bool TryGetHandlerInfo(AnonymousFunctionExpressionSyntax handler, out ApplicationCommandChecksFailedMigrationHandlerInfo handlerInfo)
	{
		handlerInfo = null!;

		switch (handler)
		{
			case ParenthesizedLambdaExpressionSyntax parenthesizedLambda
				when parenthesizedLambda.Body is BlockSyntax handlerBlock &&
				     parenthesizedLambda.ParameterList.Parameters.Count == 2 &&
				     parenthesizedLambda.ParameterList.Parameters[1].Identifier.ValueText is { Length: > 0 } argumentsIdentifier &&
				     handlerBlock.Statements.FirstOrDefault() is IfStatementSyntax guardIfStatement &&
				     guardIfStatement.Else is null:
				handlerInfo = new(handlerBlock, guardIfStatement, argumentsIdentifier);
				return true;

			case AnonymousMethodExpressionSyntax anonymousMethod
				when anonymousMethod.Block is { } anonymousHandlerBlock &&
				     anonymousMethod.ParameterList is { Parameters.Count: 2 } parameterList &&
				     parameterList.Parameters[1].Identifier.ValueText is { Length: > 0 } anonymousArgumentsIdentifier &&
				     anonymousHandlerBlock.Statements.FirstOrDefault() is IfStatementSyntax anonymousGuardIfStatement &&
				     anonymousGuardIfStatement.Else is null:
				handlerInfo = new(anonymousHandlerBlock, anonymousGuardIfStatement, anonymousArgumentsIdentifier);
				return true;

			default:
				return false;
		}
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

	private static ImmutableArray<StatementSyntax> GetProtectedStatements(IfStatementSyntax guardIfStatement, BlockSyntax handlerBlock)
	{
		var leadingStatements = guardIfStatement.Statement switch
		{
			BlockSyntax guardedBlock => guardedBlock.Statements.ToImmutableArray(),
			StatementSyntax guardedStatement => [guardedStatement]
		};

		return leadingStatements.AddRange(handlerBlock.Statements.Skip(1));
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
			string? failedExceptionIdentifier)
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
	}

	private sealed class ApplicationCommandChecksFailedMigrationHandlerInfo
	{
		internal ApplicationCommandChecksFailedMigrationHandlerInfo(BlockSyntax handlerBlock, IfStatementSyntax guardIfStatement, string argumentsIdentifier)
		{
			this.HandlerBlock = handlerBlock;
			this.GuardIfStatement = guardIfStatement;
			this.ArgumentsIdentifier = argumentsIdentifier;
		}

		internal BlockSyntax HandlerBlock { get; }

		internal IfStatementSyntax GuardIfStatement { get; }

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
