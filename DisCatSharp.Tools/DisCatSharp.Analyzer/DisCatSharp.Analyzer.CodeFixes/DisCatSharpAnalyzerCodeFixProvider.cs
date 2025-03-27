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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisCatSharpAnalyzerCodeFixProvider)), Shared]
	public class DisCatSharpAnalyzerCodeFixProvider : CodeFixProvider
	{/// <summary>
	 ///     The best available override value.
	 /// </summary>
		public const string OVERRIDE_VALUE = "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRGlzY29yZCBDbGllbnQiLCJyZWxlYXNlX2NoYW5uZWwiOiJjYW5hcnkiLCJjbGllbnRfdmVyc2lvbiI6IjEuMC41NzgiLCJvc192ZXJzaW9uIjoiMTAuMC4yNjEyMCIsIm9zX2FyY2giOiJ4NjQiLCJhcHBfYXJjaCI6Ing2NCIsInN5c3RlbV9sb2NhbGUiOiJlbi1VUyIsImhhc19jbGllbnRfbW9kcyI6ZmFsc2UsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQpIEFwcGxlV2ViS2l0LzUzNy4zNiAoS0hUTUwsIGxpa2UgR2Vja28pIGRpc2NvcmQvMS4wLjU3OCBDaHJvbWUvMTM0LjAuNjk5OC40NCBFbGVjdHJvbi8zNS4wLjIgU2FmYXJpLzUzNy4zNiIsImJyb3dzZXJfdmVyc2lvbiI6IjM1LjAuMiIsIm9zX3Nka192ZXJzaW9uIjoiMjYxMjAiLCJjbGllbnRfYnVpbGRfbnVtYmVyIjozODA2NzUsIm5hdGl2ZV9idWlsZF9udW1iZXIiOjYwNjYzLCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==";

		/// <summary>
		///     Gets the formatting annotation.
		/// </summary>
		public static readonly SyntaxAnnotation FormattingAnnotation = new SyntaxAnnotation("Formatting");

		/// <summary>
		///    Gets the diagnostic ID for which this provider can provide fixes.
		/// </summary>
		private const string DiagnosticId = "DCS0201";

		/// <inheritdoc />
		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

		/// <inheritdoc />
		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		/// <inheritdoc />
		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var document = context.Document;
			var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var diagnostic = context.Diagnostics.First(x => x.Id == DiagnosticId);
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root?.FindNode(diagnosticSpan);
			if (node == null)
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Configure Override in DiscordConfiguration",
					c => ApplyFixToDocumentAsync(document, c),
					"Configure Override in DiscordConfiguration"),
				diagnostic);
		}

		/// <inheritdoc />
		public sealed class MultiDocumentFixAllProvider : FixAllProvider
		{
			/// <inheritdoc />
			public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
			{
				var documentsToFix = new List<Document>();
				switch (fixAllContext.Scope)
				{
					case FixAllScope.Document:
						documentsToFix.Add(fixAllContext.Document);
						break;
					case FixAllScope.Project:
						documentsToFix.AddRange(fixAllContext.Project.Documents);
						break;
					case FixAllScope.Solution:
						documentsToFix.AddRange(fixAllContext.Solution.Projects.SelectMany(p => p.Documents));
						break;
				}

				return CodeAction.Create(
					GetTitle(fixAllContext),
					cancellationToken => FixAllAsync(documentsToFix, fixAllContext.Solution, cancellationToken),
					GetTitle(fixAllContext));
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
					case FixAllScope.Document:
						return $"Fix all occurrences in document '{context.Document.Name}'";
					case FixAllScope.Project:
						return $"Fix all occurrences in project '{context.Project.Name}'";
					case FixAllScope.Solution:
						return "Fix all occurrences in solution";
					default:
						return "Fix all occurrences";
				}
			}

			/// <summary>
			///     Applies fixes to a collection of documents and updates the provided solution accordingly.
			/// </summary>
			/// <param name="documents">A collection of documents that need fixes applied to them.</param>
			/// <param name="solution">The initial solution that will be updated based on the fixed documents.</param>
			/// <param name="cancellationToken">Used to signal if the operation should be canceled before completion.</param>
			/// <returns>The updated solution after all documents have been processed.</returns>
			public static async Task<Solution> FixAllAsync(IEnumerable<Document> documents, Solution solution, CancellationToken cancellationToken)
			{
				foreach (var document in documents)
				{
					var updatedDocument = await ApplyFixToDocumentAsync(document, cancellationToken).ConfigureAwait(false);
					solution = updatedDocument.Project.Solution;
				}

				return solution;
			}
		}

		/// <summary>
		///     Applies the fix to a single document by locating all DiscordConfiguration initializations and adding the Override
		///     property if missing.
		/// </summary>
		public static async Task<Document> ApplyFixToDocumentAsync(Document document, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (root == null || semanticModel == null)
				return document;

			var configNodes = root.DescendantNodes()
				.OfType<ObjectCreationExpressionSyntax>()
				.Where(x =>
				{
					var typeInfo = semanticModel.GetTypeInfo(x, cancellationToken);
					return typeInfo.Type?.ToString() == "DisCatSharp.DiscordConfiguration";
				})
				.ToList();

			var clientNodes = root.DescendantNodes()
				.OfType<ObjectCreationExpressionSyntax>()
				.Where(x =>
				{
					var typeInfo = semanticModel.GetTypeInfo(x, cancellationToken);
					return typeInfo.Type?.ToString() == "DisCatSharp.DiscordClient" || typeInfo.Type?.ToString() == "DisCatSharp.DiscordShardedClient";
				})
				.Where(x => x.ArgumentList != null)
				.SelectMany(x => x.ArgumentList.Arguments)
				.Select(x => x.Expression)
				.OfType<ObjectCreationExpressionSyntax>()
				.Where(x =>
				{
					var typeInfo = semanticModel.GetTypeInfo(x, cancellationToken);
					return typeInfo.Type?.ToString() == "DisCatSharp.DiscordConfiguration";
				})
				.ToList();

			configNodes.AddRange(clientNodes);

			var currentRoot = root;
			foreach (var configNode in configNodes)
			{
				var initializer = configNode.Initializer;
				if (initializer == null)
					continue;

				var initializerProperties = initializer.Expressions.OfType<AssignmentExpressionSyntax>().ToList();
				var hasOverride = initializerProperties.Any(expr => IsOverrideProperty(expr.Left));
				if (hasOverride)
					continue;

				var newProperties = initializerProperties.ToList();
				var overrideAssignment = SyntaxFactory.AssignmentExpression(
					SyntaxKind.SimpleAssignmentExpression,
					SyntaxFactory.IdentifierName("Override"),
					SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(OVERRIDE_VALUE)));
				newProperties.Add(overrideAssignment);

				var newInitializer = SyntaxFactory.InitializerExpression(
					SyntaxKind.ObjectInitializerExpression,
					SyntaxFactory.SeparatedList<ExpressionSyntax>(
						newProperties.Select(x => x.WithAdditionalAnnotations(FormattingAnnotation))));

				var newObjectCreation = configNode.WithInitializer(newInitializer);
				currentRoot = currentRoot.ReplaceNode(configNode, newObjectCreation);
			}

			return currentRoot == root ? document : document.WithSyntaxRoot(currentRoot);
		}

		/// <summary>
		///     Checks if the given left-hand side expression represents an assignment to "Override".
		///     Supports both IdentifierNameSyntax and MemberAccessExpressionSyntax.
		/// </summary>
		public static bool IsOverrideProperty(ExpressionSyntax left)
		{
			if (left is IdentifierNameSyntax identifier)
			{
				return identifier.Identifier.Text == "Override";
			}
			else if (left is MemberAccessExpressionSyntax memberAccess)
			{
				return memberAccess.Name.Identifier.Text == "Override";
			}
			return false;
		}
	}
}
