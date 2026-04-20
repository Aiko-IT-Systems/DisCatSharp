using System;
using System.Collections.Generic;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpConfigPropertyMigrationCodeFix))]
[Shared]
public sealed class DisCatSharpConfigPropertyMigrationCodeFix : SingleDiagnosticCodeFixProvider
{
	protected override string FixableDiagnosticId
		=> DisCatSharpDiagnosticIds.ConfigPropertyMigration;

	protected override async Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		foreach (var diagnostic in diagnostics)
		{
			if (!diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.ConfigNestedPath, out var nestedPath) ||
				!diagnostic.Properties.TryGetValue(DisCatSharpDiagnosticProperties.ConfigNewName, out var newName) ||
				nestedPath is null || newName is null)
				continue;

			var fullPath = string.IsNullOrEmpty(nestedPath) ? newName : $"{nestedPath}.{newName}";

			// Single-property fix
			context.RegisterCodeFix(
				CodeAction.Create(
					$"Use {fullPath} instead",
					c => ApplyFixToDocumentAsync(context.Document, diagnostic, nestedPath, newName, c),
					$"{this.FixableDiagnosticId}:{fullPath}"),
				diagnostic);

			// Batch fix for initializers: "Migrate all configuration properties"
			var node = root.FindNode(diagnostic.Location.SourceSpan);
			if (node is IdentifierNameSyntax { Parent: AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax initializer } })
			{
				context.RegisterCodeFix(
					CodeAction.Create(
						"Migrate all configuration properties in this initializer",
						c => ApplyInitializerBatchFixAsync(context.Document, initializer, c),
						$"{this.FixableDiagnosticId}:batch"),
					diagnostic);
				break; // Only register the batch fix once
			}
		}
	}

	internal static async Task<Document> ApplyFixToDocumentAsync(
		Document document,
		Diagnostic diagnostic,
		string nestedPath,
		string newName,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
			return document;

		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var node = root.FindNode(diagnosticSpan);

		// Case 1: MemberAccessExpression (e.g., config.ApiVersion)
		var memberAccess = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
		if (memberAccess is not null)
		{
			var expression = memberAccess.Expression;
			var pathParts = nestedPath.Split('.');
			ExpressionSyntax current = expression;
			foreach (var part in pathParts.Where(p => p.Length > 0))
			{
				current = SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					current,
					SyntaxFactory.IdentifierName(part));
			}

			var replacement = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				current,
				SyntaxFactory.IdentifierName(newName))
				.WithTriviaFrom(memberAccess);

			var newRoot = root.ReplaceNode(memberAccess, replacement);
			return await Formatter.FormatAsync(
				document.WithSyntaxRoot(newRoot),
				cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		// Case 2: Object initializer assignment (e.g., ApiChannel = value inside new DiscordConfiguration { ... })
		if (node is IdentifierNameSyntax identifier &&
			identifier.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax initializer } assignment)
		{
			var replacement = BuildNestedInitializerAssignment(nestedPath, newName, assignment.Right)
				.WithLeadingTrivia(assignment.GetLeadingTrivia())
				.WithTrailingTrivia(assignment.GetTrailingTrivia());

			var newInitializer = ReplaceExpressionInSeparatedList(initializer, assignment, replacement);
			var newRoot = root.ReplaceNode(initializer, newInitializer);
			return await Formatter.FormatAsync(
				document.WithSyntaxRoot(newRoot),
				cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		return document;
	}

	internal static async Task<Document> ApplyInitializerBatchFixAsync(
		Document document,
		InitializerExpressionSyntax initializer,
		CancellationToken cancellationToken)
	{
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel is null || root is null)
			return document;

		var kept = new List<ExpressionSyntax>();
		var tree = new InitializerGroupNode();

		foreach (var expression in initializer.Expressions)
		{
			if (expression is not AssignmentExpressionSyntax { Left: IdentifierNameSyntax identifier } assignment)
			{
				kept.Add(expression);
				continue;
			}

			var propertyName = identifier.Identifier.Text;
			if (!ConfigPropertyMigrationAnalysis.PropertyMigrations.TryGetValue(propertyName, out var migration))
			{
				kept.Add(expression);
				continue;
			}

			// Verify it's actually on DiscordConfiguration
			var symbolInfo = semanticModel.GetSymbolInfo(identifier, cancellationToken);
			if (symbolInfo.Symbol is not IPropertySymbol prop ||
				prop.ContainingType?.ToDisplayString() != "DisCatSharp.DiscordConfiguration")
			{
				kept.Add(expression);
				continue;
			}

			// Add to tree: split full path into segments
			var segments = migration.NestedPath.Split('.').Where(s => s.Length > 0);
			var node = tree;
			foreach (var segment in segments)
			{
				if (!node.Children.TryGetValue(segment, out var child))
				{
					child = new();
					node.Children[segment] = child;
				}

				node = child;
			}

			node.Assignments.Add((migration.NewName, assignment.Right));
		}

		if (tree.Children.Count == 0)
			return document;

		// Generate merged nested initializer expressions
		var newExpressions = new List<ExpressionSyntax>(kept);
		foreach (var group in tree.Children.OrderBy(x => x.Key, StringComparer.Ordinal))
		{
			var groupInitializer = BuildGroupInitializer(group.Key, group.Value);
			newExpressions.Add(groupInitializer);
		}

		var newInitializer = initializer.WithExpressions(
			BuildSeparatedListWithTrailingCommas(newExpressions));

		var newRoot = root.ReplaceNode(initializer, newInitializer);
		return await Formatter.FormatAsync(
			document.WithSyntaxRoot(newRoot),
			cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	private static ExpressionSyntax BuildGroupInitializer(string name, InitializerGroupNode node)
	{
		var members = new List<ExpressionSyntax>();

		// Direct assignments at this level
		foreach (var (propName, value) in node.Assignments)
		{
			members.Add(SyntaxFactory.AssignmentExpression(
				SyntaxKind.SimpleAssignmentExpression,
				SyntaxFactory.IdentifierName(propName),
				value));
		}

		// Child groups (recursively)
		foreach (var child in node.Children.OrderBy(x => x.Key, StringComparer.Ordinal))
			members.Add(BuildGroupInitializer(child.Key, child.Value));

		return SyntaxFactory.AssignmentExpression(
			SyntaxKind.SimpleAssignmentExpression,
			SyntaxFactory.IdentifierName(name),
			SyntaxFactory.InitializerExpression(
				SyntaxKind.ObjectInitializerExpression,
				SyntaxFactory.SeparatedList(members)));
	}

	private static ExpressionSyntax BuildNestedInitializerAssignment(string nestedPath, string newName, ExpressionSyntax value)
	{
		var pathParts = nestedPath.Split('.').Where(p => p.Length > 0).ToArray();

		ExpressionSyntax current = SyntaxFactory.AssignmentExpression(
			SyntaxKind.SimpleAssignmentExpression,
			SyntaxFactory.IdentifierName(newName),
			value.WithoutTrivia());

		for (var i = pathParts.Length - 1; i >= 0; i--)
		{
			current = SyntaxFactory.AssignmentExpression(
				SyntaxKind.SimpleAssignmentExpression,
				SyntaxFactory.IdentifierName(pathParts[i]),
				SyntaxFactory.InitializerExpression(
					SyntaxKind.ObjectInitializerExpression,
					SyntaxFactory.SingletonSeparatedList(current)));
		}

		return current;
	}

	private static InitializerExpressionSyntax ReplaceExpressionInSeparatedList(
		InitializerExpressionSyntax initializer,
		ExpressionSyntax oldExpression,
		ExpressionSyntax newExpression)
	{
		var expressions = initializer.Expressions;
		var index = expressions.IndexOf(oldExpression);
		if (index < 0)
			return initializer;

		var newExpressions = new List<ExpressionSyntax>();
		for (var i = 0; i < expressions.Count; i++)
			newExpressions.Add(i == index ? newExpression : expressions[i]);

		return initializer.WithExpressions(BuildSeparatedListWithTrailingCommas(newExpressions));
	}

	private static SeparatedSyntaxList<ExpressionSyntax> BuildSeparatedListWithTrailingCommas(List<ExpressionSyntax> expressions)
	{
		if (expressions.Count == 0)
			return SyntaxFactory.SeparatedList<ExpressionSyntax>();

		var nodesAndTokens = new List<SyntaxNodeOrToken>();
		for (var i = 0; i < expressions.Count; i++)
		{
			nodesAndTokens.Add(expressions[i]);
			// Always add trailing comma (C# allows trailing comma in initializers)
			nodesAndTokens.Add(SyntaxFactory.Token(SyntaxKind.CommaToken)
				.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed));
		}

		// Remove the very last comma — let the formatter decide
		nodesAndTokens.RemoveAt(nodesAndTokens.Count - 1);

		return SyntaxFactory.SeparatedList<ExpressionSyntax>(nodesAndTokens);
	}

	private sealed class InitializerGroupNode
	{
		public Dictionary<string, InitializerGroupNode> Children { get; } = new(StringComparer.Ordinal);
		public List<(string Name, ExpressionSyntax Value)> Assignments { get; } = [];
	}
}
