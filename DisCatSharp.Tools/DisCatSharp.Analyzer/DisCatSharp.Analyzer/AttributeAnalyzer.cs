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

using DisCatSharp.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System;
using System.Collections.Immutable;
using System.Linq;

namespace DisCatSharp.Analyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AttributeAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticIdPrefix = "DCS";

		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Information";

		private static readonly DiagnosticDescriptor ExperimentalRule = new DiagnosticDescriptor(DiagnosticIdPrefix + "0001", Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, "https://docs.discatsharp.tech/vs/analyzer/dcs/0001.html");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(ExperimentalRule); } }

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSymbolAction(ExperimentalAnalyzer, SymbolKind.Parameter);
			context.RegisterSymbolAction(ExperimentalAnalyzer, SymbolKind.Property);
			context.RegisterSymbolAction(ExperimentalAnalyzer, SymbolKind.NamedType);
			context.RegisterSymbolAction(ExperimentalAnalyzer, SymbolKind.Method);
			context.RegisterSymbolAction(ExperimentalAnalyzer, SymbolKind.Field);
			context.RegisterSymbolAction(ExperimentalAnalyzer, SymbolKind.Event);
			context.RegisterSyntaxNodeAction(ExperimentalAnalyzer, SyntaxKind.InvocationExpression);
			context.RegisterSyntaxNodeAction(ExperimentalAnalyzer, SyntaxKind.ObjectCreationExpression);
			context.RegisterSyntaxNodeAction(ExperimentalAnalyzer, SyntaxKind.FieldDeclaration); // Don't work
			context.RegisterSyntaxNodeAction(ExperimentalAnalyzer, SyntaxKind.PropertyDeclaration); // Don't work, one of the not working ones should create a report if a property is used. I.e.:
			/*
			 
			var test = new Test("test");
			test.Invoke();
			var str = test.TestString; <- Fire here for TestString
			 
			 */
		}
		private static void ExperimentalAnalyzer(SyntaxNodeAnalysisContext context)
		{
			var invocation = context.Node;
			var declaration = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol;
			if (null == declaration)
			{
				Console.WriteLine("Faulty");
				//context.ReportDiagnostic(Diagnostic.Create(ExperimentalRule, invocation.GetLocation(), "unknown", "unknown", "unknown"));
				return;
			}
			var attributes = declaration.GetAttributes();
			var attributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(ExperimentalAttribute)));
			if (null == attributeData)
			{
				Console.WriteLine("Faulty 2");
				return;
			}
			var name = declaration.Name;
			var kind = declaration.Kind.ToString();
			if (name == ".ctor")
			{
				name = declaration.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				kind = "Constructor";
			}
			var message = GetMessage(attributeData);
			var diagnostic = Diagnostic.Create(ExperimentalRule, invocation.GetLocation(), kind, name, message);
			context.ReportDiagnostic(diagnostic);

		}

		private static void ExperimentalAnalyzer(SymbolAnalysisContext context)
		{
			Console.WriteLine("Handling " + context.Symbol.Kind.ToString());
			var syntaxTrees = from x in context.Symbol.Locations
						  where x.IsInSource
						  select x.SourceTree;
			var declaration = context.Symbol;
			if (null == declaration)
			{
				Console.WriteLine("Faulty");
				return;
			}
			var attributes = declaration.GetAttributes();
			var attributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.Compilation.GetSemanticModel(syntaxTrees.First()), attr, typeof(ExperimentalAttribute)));
			if (null == attributeData)
			{
				Console.WriteLine("Faulty 2");
				return;
			}

			var message = GetMessage(attributeData);
			var name = declaration.Name;
			var kind = declaration.Kind.ToString();
			if (name == ".ctor")
			{
				name = declaration.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				kind = "Constructor";
			}
			else if (kind == "NamedType")
			{
				kind = "Class";
			}
			var diagnostic = Diagnostic.Create(ExperimentalRule, context.Symbol.Locations.First(x => x.IsInSource), kind, name, message);
			context.ReportDiagnostic(diagnostic);
		}

		static bool IsRequiredAttribute(SemanticModel semanticModel, AttributeData attribute, Type desiredAttributeType)
		{
			var desiredTypeNamedSymbol = semanticModel.Compilation.GetTypeByMetadataName(desiredAttributeType.FullName);

			var result = attribute.AttributeClass.Equals(desiredTypeNamedSymbol, SymbolEqualityComparer.Default);
			return result;
		}

		static string GetMessage(AttributeData attribute)
		{
			if (attribute.ConstructorArguments.Length < 1)
			{
				return "Do not use in production.";
			}
			return attribute.ConstructorArguments[0].Value as string;
		}
	}
}
