// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using DisCatSharp.Experimental;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AnalyzerAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "DcsExp";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Informations";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
			context.RegisterSyntaxNodeAction(AnalyzerInvocation, SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

			// Find just those named type symbols with names containing lowercase letters.
			if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
			{
				// For all such symbols, produce a diagnostic.
				var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}

		private static void AnalyzerInvocation(SyntaxNodeAnalysisContext context)
		{
			var invocation = (InvocationExpressionSyntax)context.Node;

			var methodDeclaration = (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol);

			//There are several reason why this may be null e.g invoking a delegate
			if (null == methodDeclaration)
			{
				return;
			}

			var methodAttributes = methodDeclaration.GetAttributes();
			var attributeData = methodAttributes.FirstOrDefault(attr => IsExperimentalAttribute(context.SemanticModel, attr, typeof(ExperimentalAttribute)));
			if (null == attributeData)
			{
				return;
			}

			var message = GetMessage(attributeData);
			var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodDeclaration.Name, message);
			context.ReportDiagnostic(diagnostic);
		}

		static bool IsExperimentalAttribute(SemanticModel semanticModel, AttributeData attribute, Type desiredAttributeType)
		{
			var desiredTypeNamedSymbol = semanticModel.Compilation.GetTypeByMetadataName(desiredAttributeType.FullName);

			var result = attribute.AttributeClass.Equals(desiredTypeNamedSymbol);
			return result;
		}

		static string GetMessage(AttributeData attribute)
		{
			if (attribute.ConstructorArguments.Length < 1)
			{
				return "Do not use in production";
			}

			return (attribute.ConstructorArguments[0].Value as string);
		}
	}
}
