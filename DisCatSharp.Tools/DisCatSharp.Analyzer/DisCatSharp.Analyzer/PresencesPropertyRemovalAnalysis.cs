using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Analyzer;

internal static class PresencesPropertyRemovalAnalysis
{
	private const string DiscordClientMetadataName = "DisCatSharp.DiscordClient";
	private const string PresencesPropertyName = "Presences";

	internal static bool TryGetCandidate(
		SemanticModel semanticModel,
		MemberAccessExpressionSyntax memberAccess,
		CancellationToken cancellationToken,
		out ExpressionSyntax clientExpression)
	{
		clientExpression = null!;

		if (memberAccess.Name.Identifier.ValueText != PresencesPropertyName)
			return false;

		var discordClientType = semanticModel.Compilation.GetTypeByMetadataName(DiscordClientMetadataName);
		if (discordClientType is null)
			return false;

		var expressionType = semanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken).Type;
		if (expressionType is null || !SymbolEqualityComparer.Default.Equals(expressionType, discordClientType))
			return false;

		clientExpression = memberAccess.Expression;
		return true;
	}
}
