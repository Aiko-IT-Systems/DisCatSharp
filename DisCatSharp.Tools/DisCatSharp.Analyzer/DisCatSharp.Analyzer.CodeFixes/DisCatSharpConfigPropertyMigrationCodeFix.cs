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

			var fullPath = $"{nestedPath}.{newName}";

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
			foreach (var part in pathParts)
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
			identifier.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } assignment)
		{
			var replacement = BuildNestedInitializerAssignment(nestedPath, newName, assignment.Right);
			var newRoot = root.ReplaceNode(assignment, replacement.WithTriviaFrom(assignment));
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
			if (!ConfigPropertyMigrationAnalysis.s_propertyMigrations.TryGetValue(propertyName, out var migration))
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
			var segments = migration.NestedPath.Split('.');
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
			SyntaxFactory.SeparatedList(newExpressions));

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
		var pathParts = nestedPath.Split('.');

		ExpressionSyntax current = SyntaxFactory.AssignmentExpression(
			SyntaxKind.SimpleAssignmentExpression,
			SyntaxFactory.IdentifierName(newName),
			value);

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

	private sealed class InitializerGroupNode
	{
		public Dictionary<string, InitializerGroupNode> Children { get; } = new(StringComparer.Ordinal);
		public List<(string Name, ExpressionSyntax Value)> Assignments { get; } = [];
	}
}
