using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DisCatSharp.Analyzer;

/// <summary>
///     Helpers for the DCS1301/DCS1302 async-disposal migration diagnostics.
/// </summary>
internal static class AsyncDisposalMigrationAnalysis
{
	private const string AsyncDisposableInterface = "System.IAsyncDisposable";
	private const string DisCatSharpNamespace = "DisCatSharp";

	/// <summary>
	///     Returns <see langword="true" /> when the given type is declared inside the
	///     <c>DisCatSharp</c> namespace tree and implements <c>System.IAsyncDisposable</c>.
	/// </summary>
	internal static bool IsDisCatSharpAsyncDisposable(ITypeSymbol type)
	{
		if (type is null)
			return false;

		// Check namespace — must start with "DisCatSharp"
		var ns = type.ContainingNamespace?.ToDisplayString();
		if (ns is null || !ns.StartsWith(DisCatSharpNamespace, StringComparison.Ordinal))
			return false;

		// Check interface implementation
		return type.AllInterfaces.Any(i => i.ToDisplayString() == AsyncDisposableInterface);
	}

	/// <summary>
	///     Checks whether a <c>using var x = …;</c> (local declaration) targets a
	///     DisCatSharp <c>IAsyncDisposable</c> type and is not already <c>await using</c>.
	/// </summary>
	internal static bool IsNonAwaitUsingLocalDeclaration(
		SyntaxNodeAnalysisContext context,
		LocalDeclarationStatementSyntax localDecl)
	{
		// Must be a using declaration (using var x = ...)
		if (localDecl.UsingKeyword == default)
			return false;

		// Already "await using"
		if (localDecl.AwaitKeyword != default)
			return false;

		// Resolve the declared type
		var variable = localDecl.Declaration.Variables.FirstOrDefault();
		if (variable is null)
			return false;

		var symbolInfo = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
		if (symbolInfo is not ILocalSymbol local)
			return false;

		return IsDisCatSharpAsyncDisposable(local.Type);
	}

	/// <summary>
	///     Checks whether a <c>using (var x = …) { }</c> (using statement) targets a
	///     DisCatSharp <c>IAsyncDisposable</c> type and is not already <c>await using</c>.
	/// </summary>
	internal static bool IsNonAwaitUsingStatement(
		SyntaxNodeAnalysisContext context,
		UsingStatementSyntax usingStmt)
	{
		// Already "await using"
		if (usingStmt.AwaitKeyword != default)
			return false;

		// If the using has a declaration
		if (usingStmt.Declaration is not null)
		{
			var variable = usingStmt.Declaration.Variables.FirstOrDefault();
			if (variable is null)
				return false;

			var symbolInfo = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
			if (symbolInfo is not ILocalSymbol local)
				return false;

			return IsDisCatSharpAsyncDisposable(local.Type);
		}

		// If the using has an expression (using (expr) { })
		if (usingStmt.Expression is not null)
		{
			var typeInfo = context.SemanticModel.GetTypeInfo(usingStmt.Expression, context.CancellationToken);
			return typeInfo.Type is not null && IsDisCatSharpAsyncDisposable(typeInfo.Type);
		}

		return false;
	}

	/// <summary>
	///     Checks whether an invocation is a <c>.Dispose()</c> call on a DisCatSharp
	///     <c>IAsyncDisposable</c> type.
	/// </summary>
	internal static bool IsSyncDisposeCallOnAsyncDisposable(
		SyntaxNodeAnalysisContext context,
		InvocationExpressionSyntax invocation)
	{
		// Must be a member access: something.Dispose()
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return false;

		if (memberAccess.Name.Identifier.Text != "Dispose")
			return false;

		// Must have zero arguments
		if (invocation.ArgumentList.Arguments.Count != 0)
			return false;

		// Resolve the method
		var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
		if (symbolInfo.Symbol is not IMethodSymbol method)
			return false;

		// Must be an IDisposable.Dispose() implementation
		if (method.Name != "Dispose" || method.Parameters.Length != 0)
			return false;

		return IsDisCatSharpAsyncDisposable(method.ContainingType);
	}
}
