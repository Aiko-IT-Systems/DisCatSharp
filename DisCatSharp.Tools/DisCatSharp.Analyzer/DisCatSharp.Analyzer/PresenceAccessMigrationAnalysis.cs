using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Analyzer;

internal static class PresenceAccessMigrationAnalysis
{
	private const string DiscordClientMetadataName = "DisCatSharp.DiscordClient";
	private const string PresencesPropertyName = "Presences";
	private const string UserIdPropertyName = "UserId";
	private const string ValuesPropertyName = "Values";
	private const string ValuePropertyName = "Value";

	internal static bool TryGetCandidate(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken, out PresenceAccessMigrationCandidate candidate)
	{
		candidate = null!;

		if (TryGetValuesWhereCandidate(semanticModel, invocation, cancellationToken, out candidate))
			return true;

		return TryGetKeyValueSelectCandidate(semanticModel, invocation, cancellationToken, out candidate);
	}

	private static bool TryGetValuesWhereCandidate(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken, out PresenceAccessMigrationCandidate candidate)
	{
		candidate = null!;

		if (invocation.Expression is not MemberAccessExpressionSyntax
		    {
			    Name.Identifier.ValueText: "Where",
			    Expression: MemberAccessExpressionSyntax
			    {
				    Name.Identifier.ValueText: ValuesPropertyName,
				    Expression: MemberAccessExpressionSyntax presencesAccess
			    }
		    } whereAccess)
			return false;

		if (!TryGetDiscordClientPresencesAccess(semanticModel, presencesAccess, cancellationToken, out var clientExpression))
			return false;

		if (!TryGetSingleLambda(invocation, out var parameterName, out var predicateBody) ||
		    !TryMatchEqualityPredicate(predicateBody, parameterName, $"{parameterName}.{UserIdPropertyName}", out var userExpression))
			return false;

		candidate = new(invocation, presencesAccess, clientExpression, userExpression);
		return true;
	}

	private static bool TryGetKeyValueSelectCandidate(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken, out PresenceAccessMigrationCandidate candidate)
	{
		candidate = null!;

		if (invocation.Expression is not MemberAccessExpressionSyntax
		    {
			    Name.Identifier.ValueText: "Select",
			    Expression: InvocationExpressionSyntax whereInvocation
		    } ||
		    whereInvocation.Expression is not MemberAccessExpressionSyntax
		    {
			    Name.Identifier.ValueText: "Where",
			    Expression: MemberAccessExpressionSyntax presencesAccess
		    })
			return false;

		if (!TryGetDiscordClientPresencesAccess(semanticModel, presencesAccess, cancellationToken, out var clientExpression))
			return false;

		if (!TryGetSingleLambda(invocation, out var selectParameterName, out var selectorBody) ||
		    !IsMemberAccess(selectorBody, selectParameterName, ValuePropertyName) ||
		    !TryGetSingleLambda(whereInvocation, out var whereParameterName, out var predicateBody))
			return false;

		if (!TryMatchEqualityPredicate(predicateBody, whereParameterName, $"{whereParameterName}.{ValuePropertyName}.{UserIdPropertyName}", out var userExpression))
			return false;

		candidate = new(invocation, presencesAccess, clientExpression, userExpression);
		return true;
	}

	private static bool TryGetDiscordClientPresencesAccess(SemanticModel semanticModel, MemberAccessExpressionSyntax presencesAccess, CancellationToken cancellationToken, out ExpressionSyntax clientExpression)
	{
		clientExpression = null!;

		if (presencesAccess.Name.Identifier.ValueText != PresencesPropertyName)
			return false;

		var discordClientType = semanticModel.Compilation.GetTypeByMetadataName(DiscordClientMetadataName);
		if (discordClientType is null)
			return false;

		if (semanticModel.GetSymbolInfo(presencesAccess, cancellationToken).Symbol is not IPropertySymbol propertySymbol ||
		    propertySymbol.Name != PresencesPropertyName ||
		    !SymbolEqualityComparer.Default.Equals(propertySymbol.ContainingType, discordClientType))
			return false;

		clientExpression = presencesAccess.Expression;
		return true;
	}

	private static bool TryGetSingleLambda(InvocationExpressionSyntax invocation, out string parameterName, out ExpressionSyntax body)
	{
		parameterName = string.Empty;
		body = null!;

		if (invocation.ArgumentList.Arguments.Count != 1)
			return false;

		switch (invocation.ArgumentList.Arguments[0].Expression)
		{
			case SimpleLambdaExpressionSyntax { Parameter.Identifier.ValueText: { Length: > 0 } lambdaParameterName, Body: ExpressionSyntax lambdaBody }:
				parameterName = lambdaParameterName;
				body = lambdaBody;
				return true;

			case ParenthesizedLambdaExpressionSyntax
			    {
				    ParameterList.Parameters.Count: 1
			    } parenthesizedLambda
			    when parenthesizedLambda.ParameterList.Parameters[0].Identifier.ValueText is { Length: > 0 } parenthesizedParameterName &&
			         parenthesizedLambda.Body is ExpressionSyntax parenthesizedBody:
				parameterName = parenthesizedParameterName;
				body = parenthesizedBody;
				return true;

			default:
				return false;
		}
	}

	private static bool TryMatchEqualityPredicate(ExpressionSyntax predicateBody, string parameterName, string expectedMemberAccess, out ExpressionSyntax userExpression)
	{
		userExpression = null!;

		if (predicateBody is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.EqualsExpression } equality)
			return false;

		if (IsMemberAccess(equality.Left, expectedMemberAccess))
		{
			userExpression = equality.Right;
			return true;
		}

		if (IsMemberAccess(equality.Right, expectedMemberAccess))
		{
			userExpression = equality.Left;
			return true;
		}

		return false;
	}

	private static bool IsMemberAccess(ExpressionSyntax expression, string expectedText)
		=> expression is MemberAccessExpressionSyntax && expression.ToString() == expectedText;

	private static bool IsMemberAccess(ExpressionSyntax expression, string parameterName, string memberName)
		=> expression is MemberAccessExpressionSyntax
		{
			Expression: IdentifierNameSyntax { Identifier.ValueText: var identifierName },
			Name.Identifier.ValueText: var memberAccessName
		}
		   && identifierName == parameterName
		   && memberAccessName == memberName;

	internal sealed class PresenceAccessMigrationCandidate(
		InvocationExpressionSyntax invocationToReplace,
		MemberAccessExpressionSyntax presencesAccess,
		ExpressionSyntax clientExpression,
		ExpressionSyntax userExpression)
	{
		internal InvocationExpressionSyntax InvocationToReplace { get; } = invocationToReplace;

		internal MemberAccessExpressionSyntax PresencesAccess { get; } = presencesAccess;

		internal ExpressionSyntax ClientExpression { get; } = clientExpression;

		internal ExpressionSyntax UserExpression { get; } = userExpression;
	}
}
