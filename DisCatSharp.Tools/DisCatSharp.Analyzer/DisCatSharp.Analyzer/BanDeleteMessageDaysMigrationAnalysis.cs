using System;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Analyzer;

internal static class BanDeleteMessageDaysMigrationAnalysis
{
	private const string OldParameterName = "deleteMessageDays";

	private static readonly string[] s_banMethodNames =
	[
	"BanAsync",
	"BanMemberAsync",
	];

	private static readonly string[] s_containingTypeNames =
	[
	"DisCatSharp.Entities.DiscordMember",
	"DisCatSharp.DiscordGuild",
	];

	internal static bool TryGetCandidate(
	SemanticModel semanticModel,
	InvocationExpressionSyntax invocation,
	CancellationToken cancellationToken,
	out NameColonSyntax nameColon,
	out ArgumentSyntax argument)
	{
		nameColon = null!;
		argument = null!;

		foreach (var arg in invocation.ArgumentList.Arguments)
		{
			if (arg.NameColon?.Name.Identifier.ValueText != OldParameterName)
				continue;

			// Verify it resolves to one of our ban methods
			var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
			var method = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
			if (method is null)
				continue;

			if (!s_banMethodNames.Contains(method.Name, StringComparer.Ordinal))
				continue;

			var containingType = method.ContainingType.ToDisplayString();
			if (!s_containingTypeNames.Any(n => containingType.StartsWith(n, StringComparison.Ordinal)))
				continue;

			nameColon = arg.NameColon;
			argument = arg;
			return true;
		}

		return false;
	}
}
