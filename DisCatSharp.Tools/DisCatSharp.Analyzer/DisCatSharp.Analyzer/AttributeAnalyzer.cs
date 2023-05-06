// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2023 AITSYS
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
using System.Collections.Immutable;
using System.Linq;

using DisCatSharp.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DisCatSharp.Analyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AttributeAnalyzer : DiagnosticAnalyzer
	{
		public const string DIAGNOSTIC_ID_PREFIX = "DCS";
		public const string CATEGORY = "Information";

		private static readonly LocalizableString s_titleExperimental = new LocalizableResourceString(nameof(Resources.AnalyzerTitleExperimental), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_messageFormatExperimental = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatExperimental), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_descriptionExperimental = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionExperimental), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_titleDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDeprecated), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_messageFormatDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDeprecated), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_descriptionDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDeprecated), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_titleDiscordInExperiment = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordInExperiment), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_messageFormatDiscordInExperiment = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordInExperiment), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_descriptionDiscordInExperiment = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordInExperiment), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_titleDiscordDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordDeprecated), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_messageFormatDiscordDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordDeprecated), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_descriptionDiscordDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordDeprecated), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_titleDiscordUnreleased = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordUnreleased), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_messageFormatDiscordUnreleased = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordUnreleased), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString s_descriptionDiscordUnreleased = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordUnreleased), Resources.ResourceManager, typeof(Resources));

		private static readonly DiagnosticDescriptor s_experimentalRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0001", s_titleExperimental, s_messageFormatExperimental, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionExperimental, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0001.html");
		private static readonly DiagnosticDescriptor s_deprecatedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0002", s_titleDeprecated, s_messageFormatDeprecated, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDeprecated, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0002.html");
		private static readonly DiagnosticDescriptor s_discordInExperimentRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0101", s_titleDiscordInExperiment, s_messageFormatDiscordInExperiment, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordInExperiment, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0101.html");
		private static readonly DiagnosticDescriptor s_discordDeprecatedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0102", s_titleDiscordDeprecated, s_messageFormatDiscordDeprecated, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordDeprecated, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0102.html");
		private static readonly DiagnosticDescriptor s_discordUnreleasedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0103", s_titleDiscordUnreleased, s_messageFormatDiscordUnreleased, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordUnreleased, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0103.html");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(s_experimentalRule, s_discordInExperimentRule, s_discordDeprecatedRule, s_discordUnreleasedRule);

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
			context.RegisterSyntaxNodeAction(StatusAnalyzer, SyntaxKind.InvocationExpression);
			context.RegisterSyntaxNodeAction(StatusAnalyzer, SyntaxKind.ObjectCreationExpression);
			context.RegisterSyntaxNodeAction(StatusAnalyzer, SyntaxKind.ElementAccessExpression);
			context.RegisterSyntaxNodeAction(StatusAnalyzer, SyntaxKind.SimpleMemberAccessExpression);
		}
		private static void StatusAnalyzer(SyntaxNodeAnalysisContext context)
		{
			var invocation = context.Node;
			var declaration = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol;
			if (null == declaration)
			{
				Console.WriteLine("Faulty");
				return;
			}
			var attributes = declaration.GetAttributes();

			var name = declaration.Name;
			var kind = declaration.Kind.ToString();
			if (name == ".ctor")
			{
				name = declaration.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				kind = "Constructor";
			}
			var experimentalAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(ExperimentalAttribute)));
			var deprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DeprecatedAttribute)));
			var discordInExperimentAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DiscordInExperimentAttribute)));
			var discordDeprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DiscordDeprecatedAttribute)));
			var discordUnreleasedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DiscordUnreleasedAttribute)));

			if (experimentalAttributeData != null)
			{
				var message = GetMessage(experimentalAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_experimentalRule, invocation.GetLocation(), kind, name, message));
			}
			if (deprecatedAttributeData != null)
			{
				var message = GetMessage(deprecatedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_deprecatedRule, invocation.GetLocation(), kind, name, message));
			}
			if (discordInExperimentAttributeData != null)
			{
				var message = GetMessage(discordInExperimentAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordInExperimentRule, invocation.GetLocation(), kind, name, message));
			}
			if (discordDeprecatedAttributeData != null)
			{
				var message = GetMessage(discordDeprecatedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordDeprecatedRule, invocation.GetLocation(), kind, name, message));
			}
			if (discordUnreleasedAttributeData != null)
			{
				var message = GetMessage(discordUnreleasedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordUnreleasedRule, invocation.GetLocation(), kind, name, message));
			}
			return;
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
			var experimentalAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.Compilation.GetSemanticModel(syntaxTrees.First()), attr, typeof(ExperimentalAttribute)));
			var deprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.Compilation.GetSemanticModel(syntaxTrees.First()), attr, typeof(DeprecatedAttribute)));
			var discordInExperimentAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.Compilation.GetSemanticModel(syntaxTrees.First()), attr, typeof(DiscordInExperimentAttribute)));
			var discordDeprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.Compilation.GetSemanticModel(syntaxTrees.First()), attr, typeof(DiscordDeprecatedAttribute)));
			var discordUnreleasedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.Compilation.GetSemanticModel(syntaxTrees.First()), attr, typeof(DiscordUnreleasedAttribute)));

			if (experimentalAttributeData != null)
			{
				var message = GetMessage(experimentalAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_experimentalRule, context.Symbol.Locations.First(x => x.IsInSource), kind, name, message));
			}
			if (deprecatedAttributeData != null)
			{
				var message = GetMessage(deprecatedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_deprecatedRule, context.Symbol.Locations.First(x => x.IsInSource), kind, name, message));
			}
			if (discordInExperimentAttributeData != null)
			{
				var message = GetMessage(discordInExperimentAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordInExperimentRule, context.Symbol.Locations.First(x => x.IsInSource), kind, name, message));
			}
			if (discordDeprecatedAttributeData != null)
			{
				var message = GetMessage(discordDeprecatedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordDeprecatedRule, context.Symbol.Locations.First(x => x.IsInSource), kind, name, message));
			}
			if (discordUnreleasedAttributeData != null)
			{
				var message = GetMessage(discordUnreleasedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordUnreleasedRule, context.Symbol.Locations.First(x => x.IsInSource), kind, name, message));
			}
			return;
		}

		static bool IsRequiredAttribute(SemanticModel semanticModel, AttributeData attribute, Type desiredAttributeType)
		{
			var desiredTypeNamedSymbol = semanticModel.Compilation.GetTypeByMetadataName(desiredAttributeType.FullName);

			var result = attribute.AttributeClass.Equals(desiredTypeNamedSymbol, SymbolEqualityComparer.Default);
			return result;
		}

		static string GetMessage(AttributeData attribute)
			=> attribute.ConstructorArguments.Length < 1 ? "Do not use in production." : attribute.ConstructorArguments[0].Value as string;
	}
}
