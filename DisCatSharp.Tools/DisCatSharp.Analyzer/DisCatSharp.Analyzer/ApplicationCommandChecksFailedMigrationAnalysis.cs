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

		if (TryFindGuardIfStatement(semanticModel, handlerInfo.HandlerBlock, handlerInfo.ArgumentsIdentifier, mapping.ExpectedExceptionMetadataName, cancellationToken, out var guardIfStatement, out var failedExceptionIdentifier, out var guardFilterExpression))
		{
			var protectedStatements = GetProtectedStatements(guardIfStatement, guardIfStatement.Parent as BlockSyntax);
			if (!AreStatementsSupported(protectedStatements, handlerInfo.ArgumentsIdentifier, failedExceptionIdentifier))
				return false;

			var fixKind = GetFixKind(handlerInfo.HandlerBlock, guardIfStatement, handlerInfo.ArgumentsIdentifier);
			var canAutoFix = fixKind != ApplicationCommandChecksFailedMigrationFixKind.Manual;

			candidate = new(
				assignment,
				eventAccess,
				handler,
				handlerInfo.HandlerBlock,
				guardIfStatement,
				null,
				null,
				handlerInfo.ArgumentsIdentifier,
				mapping.SourceEventName,
				mapping.TargetEventName,
				mapping.TargetEventArgsTypeName,
				failedExceptionIdentifier,
				guardFilterExpression,
				ApplicationCommandChecksFailedMigrationBranchKind.IfStatement,
				fixKind,
				canAutoFix);

			return true;
		}

		if (!TryFindGuardSwitchSection(semanticModel, handlerInfo.HandlerBlock, handlerInfo.ArgumentsIdentifier, mapping.ExpectedExceptionMetadataName, cancellationToken, out var guardSwitchStatement, out var guardSwitchSection, out failedExceptionIdentifier, out guardFilterExpression))
			return false;

		var switchStatements = GetProtectedStatements(guardSwitchSection);
		if (!AreStatementsSupported(switchStatements, handlerInfo.ArgumentsIdentifier, failedExceptionIdentifier))
			return false;

		var switchFixKind = GetSwitchFixKind(handlerInfo.HandlerBlock, guardSwitchStatement, guardSwitchSection, handlerInfo.ArgumentsIdentifier);
		var canAutoFixSwitch = switchFixKind != ApplicationCommandChecksFailedMigrationFixKind.Manual;

		candidate = new(
			assignment,
			eventAccess,
			handler,
			handlerInfo.HandlerBlock,
			null,
			guardSwitchStatement,
			guardSwitchSection,
			handlerInfo.ArgumentsIdentifier,
			mapping.SourceEventName,
			mapping.TargetEventName,
			mapping.TargetEventArgsTypeName,
			failedExceptionIdentifier,
			guardFilterExpression,
			ApplicationCommandChecksFailedMigrationBranchKind.SwitchSection,
			switchFixKind,
			canAutoFixSwitch);

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
		var statements = GetProtectedStatements(candidate);
		if (candidate.FailedExceptionIdentifier is null)
			return statements;

		var rewriter = new FailedChecksAccessRewriter(candidate.FailedExceptionIdentifier, candidate.ArgumentsIdentifier);
		var migratedStatements = statements
			.Select(statement => (StatementSyntax)rewriter.Visit(statement)!)
			.ToImmutableArray();

		if (candidate.GuardFilterExpression is null)
			return migratedStatements;

		var migratedFilter = (ExpressionSyntax)rewriter.Visit(candidate.GuardFilterExpression)!;
		return
		[
			SyntaxFactory.IfStatement(
				migratedFilter,
				SyntaxFactory.Block(migratedStatements))
		];
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

	private static bool TryFindGuardIfStatement(SemanticModel semanticModel, BlockSyntax handlerBlock, string argumentsIdentifier, string expectedExceptionMetadataName, CancellationToken cancellationToken, out IfStatementSyntax guardIfStatement, out string? failedExceptionIdentifier, out ExpressionSyntax? guardFilterExpression)
	{
		guardIfStatement = null!;
		failedExceptionIdentifier = null;
		guardFilterExpression = null;

		foreach (var candidateGuard in handlerBlock.DescendantNodes().OfType<IfStatementSyntax>())
		{
			if (candidateGuard.Else is not null)
				continue;

			if (!TryMatchGuardCondition(semanticModel, argumentsIdentifier, candidateGuard.Condition, expectedExceptionMetadataName, allowNegatedPattern: false, cancellationToken, out failedExceptionIdentifier, out guardFilterExpression))
				continue;

			guardIfStatement = candidateGuard;
			return true;
		}

		foreach (var candidateGuard in handlerBlock.DescendantNodes().OfType<IfStatementSyntax>())
		{
			if (candidateGuard.Else is not null ||
			    !IsEarlyReturnStatement(candidateGuard.Statement))
				continue;

			if (!TryMatchGuardCondition(semanticModel, argumentsIdentifier, candidateGuard.Condition, expectedExceptionMetadataName, allowNegatedPattern: true, cancellationToken, out failedExceptionIdentifier, out guardFilterExpression))
				continue;

			if (candidateGuard.Parent is not BlockSyntax guardContainerBlock)
				continue;

			var protectedStatements = GetProtectedStatements(candidateGuard, guardContainerBlock);
			if (protectedStatements.IsDefaultOrEmpty)
				continue;

			guardIfStatement = candidateGuard;
			return true;
		}

		return false;
	}

	private static bool TryFindGuardSwitchSection(SemanticModel semanticModel, BlockSyntax handlerBlock, string argumentsIdentifier, string expectedExceptionMetadataName, CancellationToken cancellationToken, out SwitchStatementSyntax guardSwitchStatement, out SwitchSectionSyntax guardSwitchSection, out string? failedExceptionIdentifier, out ExpressionSyntax? guardFilterExpression)
	{
		guardSwitchStatement = null!;
		guardSwitchSection = null!;
		failedExceptionIdentifier = null;
		guardFilterExpression = null;

		foreach (var candidateSwitchStatement in handlerBlock.DescendantNodes().OfType<SwitchStatementSyntax>())
		{
			if (!IsExceptionSwitch(candidateSwitchStatement, argumentsIdentifier))
				continue;

			foreach (var candidateSection in candidateSwitchStatement.Sections)
			{
				if (!TryMatchSwitchSection(semanticModel, candidateSection, expectedExceptionMetadataName, cancellationToken, out failedExceptionIdentifier, out guardFilterExpression))
					continue;

				var protectedStatements = GetProtectedStatements(candidateSection);
				if (protectedStatements.IsDefaultOrEmpty)
					continue;

				guardSwitchStatement = candidateSwitchStatement;
				guardSwitchSection = candidateSection;
				return true;
			}
		}

		return false;
	}

	private static bool TryMatchGuardCondition(SemanticModel semanticModel, string argumentsIdentifier, ExpressionSyntax condition, string expectedExceptionMetadataName, bool allowNegatedPattern, CancellationToken cancellationToken, out string? failedExceptionIdentifier, out ExpressionSyntax? guardFilterExpression)
	{
		failedExceptionIdentifier = null;
		guardFilterExpression = null;

		if (!allowNegatedPattern &&
		    condition is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalAndExpression } logicalAnd)
		{
			if (TryMatchGuardCondition(semanticModel, argumentsIdentifier, logicalAnd.Left, expectedExceptionMetadataName, allowNegatedPattern: false, cancellationToken, out failedExceptionIdentifier, out _) &&
			    failedExceptionIdentifier is not null &&
			    IsSupportedFailedChecksFilter(logicalAnd.Right, failedExceptionIdentifier))
			{
				guardFilterExpression = logicalAnd.Right;
				return true;
			}

			if (TryMatchGuardCondition(semanticModel, argumentsIdentifier, logicalAnd.Right, expectedExceptionMetadataName, allowNegatedPattern: false, cancellationToken, out failedExceptionIdentifier, out _) &&
			    failedExceptionIdentifier is not null &&
			    IsSupportedFailedChecksFilter(logicalAnd.Left, failedExceptionIdentifier))
			{
				guardFilterExpression = logicalAnd.Left;
				return true;
			}
		}

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

		var pattern = isPattern.Pattern;
		if (allowNegatedPattern)
		{
			if (pattern is not UnaryPatternSyntax { Pattern: var innerPattern })
				return false;

			pattern = innerPattern;
		}

		switch (pattern)
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

	private static bool IsExceptionSwitch(SwitchStatementSyntax switchStatement, string argumentsIdentifier)
		=> switchStatement.Expression is MemberAccessExpressionSyntax
		{
			Expression: IdentifierNameSyntax { Identifier.ValueText: var identifierName },
			Name.Identifier.ValueText: ExceptionPropertyName
		} && identifierName == argumentsIdentifier;

	private static bool TryMatchSwitchSection(SemanticModel semanticModel, SwitchSectionSyntax switchSection, string expectedExceptionMetadataName, CancellationToken cancellationToken, out string? failedExceptionIdentifier, out ExpressionSyntax? guardFilterExpression)
	{
		failedExceptionIdentifier = null;
		guardFilterExpression = null;

		if (switchSection.Labels.Count != 1 ||
		    switchSection.Labels[0] is not CasePatternSwitchLabelSyntax { Pattern: var pattern } caseLabel)
			return false;

		var expectedExceptionType = semanticModel.Compilation.GetTypeByMetadataName(expectedExceptionMetadataName);
		if (expectedExceptionType is null)
			return false;

		switch (pattern)
		{
			case DeclarationPatternSyntax { Type: { } declarationType, Designation: SingleVariableDesignationSyntax designation }:
				if (!IsExpectedExceptionType(semanticModel, declarationType, expectedExceptionType, cancellationToken))
					return false;

				failedExceptionIdentifier = designation.Identifier.ValueText;
				if (caseLabel.WhenClause is not null)
				{
					if (!IsSupportedFailedChecksFilter(caseLabel.WhenClause.Condition, failedExceptionIdentifier))
						return false;

					guardFilterExpression = caseLabel.WhenClause.Condition;
				}
				return true;

			case TypePatternSyntax { Type: { } typePattern }:
				if (caseLabel.WhenClause is not null)
					return false;

				return IsExpectedExceptionType(semanticModel, typePattern, expectedExceptionType, cancellationToken);

			default:
				return false;
		}
	}

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
		if (guardIfStatement.Parent is BlockSyntax guardContainerBlock &&
		    ReferenceEquals(guardContainerBlock, handlerBlock) &&
		    IsEarlyReturnStatement(guardIfStatement.Statement) &&
		    ReferenceEquals(handlerBlock.Statements.FirstOrDefault(), guardIfStatement) &&
		    GetProtectedStatements(guardIfStatement, guardContainerBlock).Length > 0)
			return true;

		return handlerBlock.Statements.Count == 1 &&
		       ReferenceEquals(handlerBlock.Statements[0], guardIfStatement) &&
		       guardIfStatement.Else is null;
	}

	private static ApplicationCommandChecksFailedMigrationFixKind GetFixKind(BlockSyntax handlerBlock, IfStatementSyntax guardIfStatement, string argumentsIdentifier)
	{
		if (IsFullyGuardedHandler(handlerBlock, guardIfStatement))
			return ApplicationCommandChecksFailedMigrationFixKind.Rewrite;

		return CanExtractGuardedBranch(handlerBlock, guardIfStatement, argumentsIdentifier)
			? ApplicationCommandChecksFailedMigrationFixKind.Split
			: ApplicationCommandChecksFailedMigrationFixKind.Manual;
	}

	private static ApplicationCommandChecksFailedMigrationFixKind GetSwitchFixKind(BlockSyntax handlerBlock, SwitchStatementSyntax guardSwitchStatement, SwitchSectionSyntax guardSwitchSection, string argumentsIdentifier)
	{
		if (handlerBlock.Statements.Count == 1 && ReferenceEquals(handlerBlock.Statements[0], guardSwitchStatement))
		{
			return guardSwitchStatement.Sections.Count == 1 && ReferenceEquals(guardSwitchStatement.Sections[0], guardSwitchSection)
				? ApplicationCommandChecksFailedMigrationFixKind.Rewrite
				: ApplicationCommandChecksFailedMigrationFixKind.Split;
		}

		return CanExtractGuardedSwitchBranch(handlerBlock, guardSwitchStatement, guardSwitchSection, argumentsIdentifier)
			? ApplicationCommandChecksFailedMigrationFixKind.Split
			: ApplicationCommandChecksFailedMigrationFixKind.Manual;
	}

	private static bool CanExtractGuardedSwitchBranch(BlockSyntax handlerBlock, SwitchStatementSyntax guardSwitchStatement, SwitchSectionSyntax guardSwitchSection, string argumentsIdentifier)
	{
		if (guardSwitchStatement.Parent is not BlockSyntax currentBlock || currentBlock == handlerBlock)
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

	private static ImmutableArray<StatementSyntax> GetProtectedStatements(ApplicationCommandChecksFailedMigrationCandidate candidate)
		=> candidate.BranchKind switch
		{
			ApplicationCommandChecksFailedMigrationBranchKind.IfStatement => GetProtectedStatements(candidate.GuardIfStatement!, candidate.GuardIfStatement!.Parent as BlockSyntax),
			ApplicationCommandChecksFailedMigrationBranchKind.SwitchSection => GetProtectedStatements(candidate.GuardSwitchSection!),
			_ => []
		};

	private static ImmutableArray<StatementSyntax> GetProtectedStatements(IfStatementSyntax guardIfStatement, BlockSyntax? guardContainerBlock)
		=> guardContainerBlock is not null && IsEarlyReturnStatement(guardIfStatement.Statement)
			? GetStatementsAfterGuard(guardIfStatement, guardContainerBlock)
			: GetProtectedStatements(guardIfStatement);

	private static ImmutableArray<StatementSyntax> GetProtectedStatements(IfStatementSyntax guardIfStatement)
	{
		return guardIfStatement.Statement switch
		{
			BlockSyntax guardedBlock => guardedBlock.Statements.ToImmutableArray(),
			StatementSyntax guardedStatement => [guardedStatement]
		};
	}

	private static ImmutableArray<StatementSyntax> GetStatementsAfterGuard(IfStatementSyntax guardIfStatement, BlockSyntax guardContainerBlock)
	{
		var guardIndex = guardContainerBlock.Statements.IndexOf(guardIfStatement);
		return guardIndex < 0
			? []
			: [.. guardContainerBlock.Statements.Skip(guardIndex + 1)];
	}

	private static ImmutableArray<StatementSyntax> GetProtectedStatements(SwitchSectionSyntax switchSection)
	{
		var statements = switchSection.Statements;
		if (statements.Count > 0 && statements[statements.Count - 1] is BreakStatementSyntax)
			return [.. statements.Take(statements.Count - 1)];

		return [.. statements];
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

	private static bool IsSupportedFailedChecksFilter(ExpressionSyntax expression, string failedExceptionIdentifier)
	{
		foreach (var identifier in expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(x => x.Identifier.ValueText == failedExceptionIdentifier))
		{
			if (identifier.Parent is MemberAccessExpressionSyntax memberAccess &&
			    ReferenceEquals(memberAccess.Expression, identifier) &&
			    memberAccess.Name.Identifier.ValueText == FailedChecksPropertyName)
				continue;

			return false;
		}

		return true;
	}

	private static bool IsEarlyReturnStatement(StatementSyntax statement)
		=> statement is ReturnStatementSyntax ||
		   statement is BlockSyntax { Statements.Count: 1 } block && block.Statements[0] is ReturnStatementSyntax;

	internal sealed class ApplicationCommandChecksFailedMigrationCandidate
	{
		internal ApplicationCommandChecksFailedMigrationCandidate(
			AssignmentExpressionSyntax assignment,
			MemberAccessExpressionSyntax eventAccess,
			AnonymousFunctionExpressionSyntax handler,
			BlockSyntax handlerBlock,
			IfStatementSyntax? guardIfStatement,
			SwitchStatementSyntax? guardSwitchStatement,
			SwitchSectionSyntax? guardSwitchSection,
			string argumentsIdentifier,
			string sourceEventName,
			string targetEventName,
			string targetEventArgsTypeName,
			string? failedExceptionIdentifier,
			ExpressionSyntax? guardFilterExpression,
			ApplicationCommandChecksFailedMigrationBranchKind branchKind,
			ApplicationCommandChecksFailedMigrationFixKind fixKind,
			bool canAutoFix)
		{
			this.Assignment = assignment;
			this.EventAccess = eventAccess;
			this.Handler = handler;
			this.HandlerBlock = handlerBlock;
			this.GuardIfStatement = guardIfStatement;
			this.GuardSwitchStatement = guardSwitchStatement;
			this.GuardSwitchSection = guardSwitchSection;
			this.ArgumentsIdentifier = argumentsIdentifier;
			this.SourceEventName = sourceEventName;
			this.TargetEventName = targetEventName;
			this.TargetEventArgsTypeName = targetEventArgsTypeName;
			this.FailedExceptionIdentifier = failedExceptionIdentifier;
			this.GuardFilterExpression = guardFilterExpression;
			this.BranchKind = branchKind;
			this.FixKind = fixKind;
			this.CanAutoFix = canAutoFix;
		}

		internal AssignmentExpressionSyntax Assignment { get; }

		internal MemberAccessExpressionSyntax EventAccess { get; }

		internal AnonymousFunctionExpressionSyntax Handler { get; }

		internal BlockSyntax HandlerBlock { get; }

		internal IfStatementSyntax? GuardIfStatement { get; }

		internal SwitchStatementSyntax? GuardSwitchStatement { get; }

		internal SwitchSectionSyntax? GuardSwitchSection { get; }

		internal string ArgumentsIdentifier { get; }

		internal string SourceEventName { get; }

		internal string TargetEventName { get; }

		internal string TargetEventArgsTypeName { get; }

		internal string? FailedExceptionIdentifier { get; }

		internal ExpressionSyntax? GuardFilterExpression { get; }

		internal ApplicationCommandChecksFailedMigrationBranchKind BranchKind { get; }

		internal ApplicationCommandChecksFailedMigrationFixKind FixKind { get; }

		internal bool CanAutoFix { get; }
	}

	internal enum ApplicationCommandChecksFailedMigrationFixKind
	{
		Rewrite,
		Split,
		Manual
	}

	internal enum ApplicationCommandChecksFailedMigrationBranchKind
	{
		IfStatement,
		SwitchSection
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
