using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Analyzer;

internal static class CodeFixSemanticHelpers
{
	public static bool IsCreationOfType(SemanticModel semanticModel, ExpressionSyntax node, INamedTypeSymbol expectedType, CancellationToken cancellationToken)
	{
		var typeInfo = semanticModel.GetTypeInfo(node, cancellationToken);
		return SymbolEqualityComparer.Default.Equals(typeInfo.Type, expectedType);
	}
}
