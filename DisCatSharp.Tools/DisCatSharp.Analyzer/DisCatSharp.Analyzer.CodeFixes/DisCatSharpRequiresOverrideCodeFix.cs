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

namespace DisCatSharp.Analyzer
{
	/// <inheritdoc />
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpRequiresOverrideCodeFix)), Shared]
	public class DisCatSharpRequiresOverrideCodeFix : CodeFixProvider
	{
		/// <summary>
		///     Gets the diagnostic ID for which this provider can provide fixes.
		/// </summary>
		private const string DIAGNOSTIC_ID = "DCS0201";

		/// <summary>
		///     Gets the best available override value.
		/// </summary>
		public static string OverrideValue = "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRGlzY29yZCBDbGllbnQiLCJyZWxlYXNlX2NoYW5uZWwiOiJjYW5hcnkiLCJjbGllbnRfdmVyc2lvbiI6IjEuMC41NzgiLCJvc192ZXJzaW9uIjoiMTAuMC4yNjEyMCIsIm9zX2FyY2giOiJ4NjQiLCJhcHBfYXJjaCI6Ing2NCIsInN5c3RlbV9sb2NhbGUiOiJlbi1VUyIsImhhc19jbGllbnRfbW9kcyI6ZmFsc2UsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQpIEFwcGxlV2ViS2l0LzUzNy4zNiAoS0hUTUwsIGxpa2UgR2Vja28pIGRpc2NvcmQvMS4wLjU3OCBDaHJvbWUvMTM0LjAuNjk5OC40NCBFbGVjdHJvbi8zNS4wLjIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6IjM1LjAuMiIsIm9zX3Nka192ZXJzaW9uIjoiMjYxMjAiLCJjbGllbnRfYnVpbGRfbnVtYmVyIjozODA2NzUsIm5hdGl2ZV9idWlsZF9udW1iZXIiOjYwNjYzLCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==";

		/// <summary>
		///     Gets the formatting annotation.
		/// </summary>
		public static readonly SyntaxAnnotation FormattingAnnotation = new SyntaxAnnotation("Formatting");

		/// <inheritdoc />
		public sealed override ImmutableArray<string> FixableDiagnosticIds
			=> ImmutableArray.Create(DIAGNOSTIC_ID);

		/// <inheritdoc />
		public sealed override FixAllProvider GetFixAllProvider()
			=> new MultiDocumentFixAllProvider();

		/// <inheritdoc />
		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var document = context.Document;
			var diagnostics = context.Diagnostics.Where(x => x.Id == DIAGNOSTIC_ID).ToList();

			if (diagnostics.Count == 0)
				return;

			var newestDateTime = DateTime.MinValue;
			var diagnostic = diagnostics.First();
			foreach (var diag in diagnostics)
			{
				if (!diag.Properties.TryGetValue("OverrideDate", out var date))
					continue;
				if (!diag.Properties.TryGetValue("LastKnownOverride", out var prop))
					continue;

				var parsedDate = DateTime.TryParse(date, out var dt) ? dt : DateTime.MinValue;
				if (parsedDate > newestDateTime)
					continue;

				newestDateTime = parsedDate;
				OverrideValue = prop;
				diagnostic = diag;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					"Add required Override to all DiscordConfigurations",
					c => ApplyFixToProjectAsync(document.Project, c),
					diagnostic.Id),
				diagnostics);
		}

		/// <summary>
		///     Applies modifications to a document's syntax tree, specifically targeting object creation expressions for certain
		///     types.
		/// </summary>
		/// <param name="document">The document to be modified based on the specified criteria.</param>
		/// <param name="cancellationToken">Used to signal cancellation of the operation if needed.</param>
		/// <returns>Returns the modified document if changes were made; otherwise, returns the original document.</returns>
		public static async Task<Document> ApplyFixToDocumentAsync(Document document, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (root == null || semanticModel == null)
				return document;

			var configNodes = root.DescendantNodes()
				.Where(node =>
				{
					if (!(node is ObjectCreationExpressionSyntax || node is AnonymousObjectCreationExpressionSyntax))
						return false;

					var typeInfo = semanticModel.GetTypeInfo(node, cancellationToken);
					return typeInfo.Type?.ToString() == "DisCatSharp.DiscordConfiguration";
				})
				.ToList();

			var clientNodes = root.DescendantNodes()
				.Where(node => node is ObjectCreationExpressionSyntax || node is AnonymousObjectCreationExpressionSyntax)
				.Where(node =>
				{
					var typeInfo = semanticModel.GetTypeInfo(node, cancellationToken);
					return typeInfo.Type?.ToString() == "DisCatSharp.DiscordClient" ||
					       typeInfo.Type?.ToString() == "DisCatSharp.DiscordShardedClient";
				})
				.SelectMany(node =>
				{
					SeparatedSyntaxList<ArgumentSyntax>? arguments;
					switch (node)
					{
						case ObjectCreationExpressionSyntax explicitNode:
							arguments = explicitNode.ArgumentList?.Arguments;
							break;
						case AnonymousObjectCreationExpressionSyntax implicitNode:
							arguments = (implicitNode.Parent as ArgumentListSyntax)?.Arguments;
							break;
						default:
							arguments = null;
							break;
					}

					return arguments ?? Enumerable.Empty<ArgumentSyntax>();
				})
				.Select(arg => arg.Expression)
				.Where(expr => expr is ObjectCreationExpressionSyntax ||
				               expr is AnonymousObjectCreationExpressionSyntax)
				.Where(expr =>
				{
					var typeInfo = semanticModel.GetTypeInfo(expr, cancellationToken);
					return typeInfo.Type?.ToString() == "DisCatSharp.DiscordConfiguration";
				})
				.ToList();

			configNodes.AddRange(clientNodes);

			var currentRoot = root;
			foreach (var node in configNodes)
				switch (node)
				{
					case ObjectCreationExpressionSyntax objectCreation:
					{
						var initializer = objectCreation.Initializer;
						if (initializer == null)
						{
							var overrideAssignment = SyntaxFactory.AssignmentExpression(
								SyntaxKind.SimpleAssignmentExpression,
								SyntaxFactory.IdentifierName("Override"),
								SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
									SyntaxFactory.Literal(OverrideValue)));

							var newInitializer = SyntaxFactory.InitializerExpression(
								SyntaxKind.ObjectInitializerExpression,
								SyntaxFactory.SeparatedList<ExpressionSyntax>(
									new[] { overrideAssignment }));

							currentRoot = currentRoot.ReplaceNode(
								objectCreation,
								objectCreation.WithInitializer(newInitializer));
						}
						else
						{
							var initializerProperties = initializer.Expressions.OfType<AssignmentExpressionSyntax>().ToList();
							if (initializerProperties.Any(expr => IsOverrideProperty(expr.Left)))
								continue;

							var newProperties = initializerProperties.ToList();
							var overrideAssignment = SyntaxFactory.AssignmentExpression(
								SyntaxKind.SimpleAssignmentExpression,
								SyntaxFactory.IdentifierName("Override"),
								SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
									SyntaxFactory.Literal(OverrideValue)));

							newProperties.Add(overrideAssignment);

							var expressions = newProperties.Select(x => (ExpressionSyntax)x.WithAdditionalAnnotations(FormattingAnnotation));

							var newInitializer = SyntaxFactory.InitializerExpression(
								SyntaxKind.ObjectInitializerExpression,
								SyntaxFactory.SeparatedList(expressions));

							currentRoot = currentRoot.ReplaceNode(
								objectCreation,
								objectCreation.WithInitializer(newInitializer));
						}

						break;
					}
					case AnonymousObjectCreationExpressionSyntax anonymousCreation:
					{
						var initializers = anonymousCreation.Initializers;
						if (initializers.Any(init =>
							init.NameEquals?.Name.Identifier.Text == "Override" ||
							(init.Expression is AssignmentExpressionSyntax assign &&
							 IsOverrideProperty(assign.Left))))
							continue;

						var overrideAssignment = SyntaxFactory.AnonymousObjectMemberDeclarator(
							SyntaxFactory.NameEquals("Override"),
							SyntaxFactory.LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								SyntaxFactory.Literal(OverrideValue)));

						var newInitializers = initializers.Add(overrideAssignment);
						currentRoot = currentRoot.ReplaceNode(
							anonymousCreation,
							anonymousCreation.WithInitializers(newInitializers));
						break;
					}
				}

			return document.WithSyntaxRoot(currentRoot);
		}

		/// <summary>
		///     Applies the fix to a single project by locating all DiscordConfiguration initializations and adding the Override
		///     property if missing.
		/// </summary>
		/// <param name="project">The project to be modified based on the specified criteria.</param>
		/// <param name="cancellationToken">Used to signal cancellation of the operation if needed.</param>
		/// <returns>Returns the modified project if changes were made; otherwise, returns the original project.</returns>
		public static async Task<Solution> ApplyFixToProjectAsync(Project project, CancellationToken cancellationToken)
		{
			var solution = project.Solution;
			foreach (var document in project.Documents)
			{
				var fixedDocument = await ApplyFixToDocumentAsync(document, cancellationToken).ConfigureAwait(false);
				solution = fixedDocument.Project.Solution;
			}

			return solution;
		}

		/// <summary>
		///     Checks if the given left-hand side expression represents an assignment to "Override".
		///     Supports both IdentifierNameSyntax and MemberAccessExpressionSyntax.
		/// </summary>
		public static bool IsOverrideProperty(ExpressionSyntax left)
		{
			switch (left)
			{
				case IdentifierNameSyntax identifier:
					return identifier.Identifier.Text == "Override";
				case MemberAccessExpressionSyntax memberAccess:
					return memberAccess.Name.Identifier.Text == "Override";
				default:
					return false;
			}
		}

		/// <inheritdoc />
		public sealed class MultiDocumentFixAllProvider : FixAllProvider
		{
			/// <inheritdoc />
			public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
				=> ImmutableArray.Create(FixAllScope.Project);

			/// <inheritdoc />
			public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
			{
				var projectsToFix = new List<Project>();
				switch (fixAllContext.Scope)
				{
					case FixAllScope.Project:
						projectsToFix.Add(fixAllContext.Project);
						break;
					case FixAllScope.Document:
					case FixAllScope.Solution:
					case FixAllScope.Custom:
						throw new NotSupportedException("This scope is not supported.");
					default:
						throw new ArgumentOutOfRangeException();
				}

				return Task.FromResult(CodeAction.Create(
					GetTitle(fixAllContext),
					cancellationToken => FixAllAsync(projectsToFix, fixAllContext.Solution, cancellationToken),
					GetTitle(fixAllContext)));
			}

			/// <summary>
			///     Gets the title for the codefix.
			/// </summary>
			/// <param name="context">The fix all context.</param>
			/// <returns>The generated title.</returns>
			public static string GetTitle(FixAllContext context)
			{
				switch (context.Scope)
				{
					case FixAllScope.Project:
						return $"Fix all occurrences in project '{context.Project.Name}'";
					case FixAllScope.Document:
					case FixAllScope.Solution:
					case FixAllScope.Custom:
						throw new NotSupportedException("This scope is not supported.");
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			/// <summary>
			///     Applies fixes to a collection of projects and updates the provided solution accordingly.
			/// </summary>
			/// <param name="projects">A collection of projects that need fixes applied to them.</param>
			/// <param name="solution">The initial solution that will be updated based on the fixed projects.</param>
			/// <param name="cancellationToken">Used to signal if the operation should be canceled before completion.</param>
			/// <returns>The updated solution after all projects have been processed.</returns>
			public static async Task<Solution> FixAllAsync(IEnumerable<Project> projects, Solution solution, CancellationToken cancellationToken)
			{
				foreach (var project in projects)
				{
					var fixedProject = await ApplyFixToProjectAsync(project, cancellationToken).ConfigureAwait(false);
					solution = fixedProject;
				}

				return solution;
			}
		}
	}
}
