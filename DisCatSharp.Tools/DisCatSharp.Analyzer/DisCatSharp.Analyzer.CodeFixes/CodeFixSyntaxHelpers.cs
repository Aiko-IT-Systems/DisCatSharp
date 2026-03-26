using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DisCatSharp.Analyzer;

internal static class CodeFixSyntaxHelpers
{
	public static bool IsAssignedProperty(ExpressionSyntax left, string propertyName)
		=> left switch
		{
			IdentifierNameSyntax identifier => identifier.Identifier.Text == propertyName,
			MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text == propertyName,
			_ => false
		};

	public static AssignmentExpressionSyntax CreateStringPropertyAssignment(string propertyName, string value)
		=> SyntaxFactory.AssignmentExpression(
			SyntaxKind.SimpleAssignmentExpression,
			SyntaxFactory.IdentifierName(propertyName),
			SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value)))
			.WithAdditionalAnnotations(Formatter.Annotation);

	public static bool TryAddObjectInitializerAssignment(InitializerExpressionSyntax? initializer, string propertyName, AssignmentExpressionSyntax assignment, out InitializerExpressionSyntax updatedInitializer)
	{
		if (initializer is null)
		{
			updatedInitializer = SyntaxFactory.InitializerExpression(
				SyntaxKind.ObjectInitializerExpression,
				SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(assignment));
			return true;
		}

		if (initializer.Expressions.OfType<AssignmentExpressionSyntax>().Any(expr => IsAssignedProperty(expr.Left, propertyName)))
		{
			updatedInitializer = initializer;
			return false;
		}

		updatedInitializer = initializer.WithExpressions(initializer.Expressions.Add(assignment));
		return true;
	}
}
