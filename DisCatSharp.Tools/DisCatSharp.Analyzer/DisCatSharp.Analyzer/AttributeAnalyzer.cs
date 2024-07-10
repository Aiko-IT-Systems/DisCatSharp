using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using DisCatSharp.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DisCatSharp.Analyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AttributeAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>
		/// The diagnostic ID prefix.
		/// </summary>
		public const string DIAGNOSTIC_ID_PREFIX = "DCS";

		/// <summary>
		/// The diagnostic category.
		/// </summary>
		public const string CATEGORY = "Usage";

		private static readonly LocalizableString s_titleExperimental = new LocalizableResourceString(nameof(Resources.AnalyzerTitleExperimental), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_messageFormatExperimental =
			new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatExperimental), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_descriptionExperimental =
			new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionExperimental), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_titleDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDeprecated), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_messageFormatDeprecated =
			new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDeprecated), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_descriptionDeprecated =
			new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDeprecated), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_titleDiscordInExperiment =
			new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordInExperiment), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_messageFormatDiscordInExperiment =
			new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordInExperiment), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_descriptionDiscordInExperiment =
			new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordInExperiment), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_titleDiscordDeprecated =
			new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordDeprecated), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_messageFormatDiscordDeprecated =
			new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordDeprecated), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_descriptionDiscordDeprecated =
			new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordDeprecated), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_titleDiscordUnreleased =
			new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordUnreleased), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_messageFormatDiscordUnreleased =
			new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordUnreleased), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_descriptionDiscordUnreleased =
			new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordUnreleased), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_titleRequiresFeature =
			new LocalizableResourceString(nameof(Resources.AnalyzerTitleRequiresFeature), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_messageFormatRequiresFeature =
			new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatRequiresFeature), Resources.ResourceManager, typeof(Resources));

		private static readonly LocalizableString s_descriptionRequiresFeature =
			new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionRequiresFeature), Resources.ResourceManager, typeof(Resources));

		private static readonly DiagnosticDescriptor s_experimentalRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0001", s_titleExperimental, s_messageFormatExperimental, CATEGORY,
			DiagnosticSeverity.Warning, true, s_descriptionExperimental, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0001");

		private static readonly DiagnosticDescriptor s_deprecatedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0002", s_titleDeprecated, s_messageFormatDeprecated, CATEGORY,
			DiagnosticSeverity.Warning, true, s_descriptionDeprecated, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0002");

		private static readonly DiagnosticDescriptor s_discordInExperimentRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0101", s_titleDiscordInExperiment,
			s_messageFormatDiscordInExperiment, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordInExperiment, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0101");

		private static readonly DiagnosticDescriptor s_discordDeprecatedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0102", s_titleDiscordDeprecated, s_messageFormatDiscordDeprecated,
			CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordDeprecated, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0102");

		private static readonly DiagnosticDescriptor s_discordUnreleasedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0103", s_titleDiscordUnreleased, s_messageFormatDiscordUnreleased,
			CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordUnreleased, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0103");

		private static readonly DiagnosticDescriptor s_requiresFeatureRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0200", s_titleRequiresFeature, s_messageFormatRequiresFeature,
			CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionRequiresFeature, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0200");

		/// <summary>
		/// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
		/// </summary>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(s_experimentalRule, s_deprecatedRule, s_discordInExperimentRule, s_discordDeprecatedRule, s_discordUnreleasedRule, s_requiresFeatureRule);

		/// <summary>
		/// Called once at session start to register actions in the analysis context.
		/// </summary>
		/// <param name="context">The analysis context.</param>
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
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

		/// <summary>
		/// Analyzes the status of various components.
		/// </summary>
		/// <param name="context">The syntac node analysis context.</param>
		private static void StatusAnalyzer(SyntaxNodeAnalysisContext context)
		{
			try
			{
				var invocation = context.Node;
				var declaration = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol;
				if (null == declaration)
				{
					Console.WriteLine(new LocalizableResourceString(nameof(Resources.Faulty), Resources.ResourceManager, typeof(Resources)));
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
				var requiresFeatureAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(RequiresFeatureAttribute)));

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

				if (requiresFeatureAttributeData != null)
				{
					var message = GetFeatureMessage(requiresFeatureAttributeData);
					context.ReportDiagnostic(Diagnostic.Create(s_requiresFeatureRule, invocation.GetLocation(), kind, name, message));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"StatusAnalyzer threw an exception: {ex.Message}");
				if (!string.IsNullOrEmpty(ex.StackTrace))
					Console.WriteLine(ex.StackTrace);
			}

			return;
		}

		/// <summary>
		/// Analyzes the experimental state of various components.
		/// </summary>
		/// <param name="context">The symbol analysis context.</param>
		private static void ExperimentalAnalyzer(SymbolAnalysisContext context)
		{
			try
			{
				var syntaxTrees = from x in context.Symbol.Locations
				                  where x.IsInSource
				                  select x.SourceTree;
				var declaration = context.Symbol;

				// ReSharper disable HeuristicUnreachableCode
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (null == declaration)
				{
					Console.WriteLine(new LocalizableResourceString(nameof(Resources.Faulty), Resources.ResourceManager, typeof(Resources)));
					return;
				}
				// ReSharper enable HeuristicUnreachableCode

				var attributes = declaration.GetAttributes();

				var name = declaration.Name;
				var kind = declaration.Kind.ToString();
				if (name == ".ctor")
				{
					name = declaration.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
					kind = "Constructor";
				}
				else if (kind == "NamedType")
					kind = "Class";

				var model = context.Compilation.GetSemanticModel(syntaxTrees.First(), true);

				var experimentalAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(ExperimentalAttribute)));
				var deprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DeprecatedAttribute)));
				var discordInExperimentAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DiscordInExperimentAttribute)));
				var discordDeprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DiscordDeprecatedAttribute)));
				var discordUnreleasedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DiscordUnreleasedAttribute)));
				var requiresFeatureAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(RequiresFeatureAttribute)));

				if (experimentalAttributeData != null)
				{
					var message = GetMessage(experimentalAttributeData);
					context.ReportDiagnostic(Diagnostic.Create(s_experimentalRule, context.Symbol.Locations.FirstOrDefault(x => x.IsInSource), kind, name, message));
				}

				if (deprecatedAttributeData != null)
				{
					var message = GetMessage(deprecatedAttributeData);
					context.ReportDiagnostic(Diagnostic.Create(s_deprecatedRule, context.Symbol.Locations.FirstOrDefault(x => x.IsInSource), kind, name, message));
				}

				if (discordInExperimentAttributeData != null)
				{
					var message = GetMessage(discordInExperimentAttributeData);
					context.ReportDiagnostic(Diagnostic.Create(s_discordInExperimentRule, context.Symbol.Locations.FirstOrDefault(x => x.IsInSource), kind, name, message));
				}

				if (discordDeprecatedAttributeData != null)
				{
					var message = GetMessage(discordDeprecatedAttributeData);
					context.ReportDiagnostic(Diagnostic.Create(s_discordDeprecatedRule, context.Symbol.Locations.FirstOrDefault(x => x.IsInSource), kind, name, message));
				}

				if (discordUnreleasedAttributeData != null)
				{
					var message = GetMessage(discordUnreleasedAttributeData);
					context.ReportDiagnostic(Diagnostic.Create(s_discordUnreleasedRule, context.Symbol.Locations.FirstOrDefault(x => x.IsInSource), kind, name, message));
				}

				if (requiresFeatureAttributeData != null)
				{
					var message = GetFeatureMessage(requiresFeatureAttributeData);
					context.ReportDiagnostic(Diagnostic.Create(s_requiresFeatureRule, context.Symbol.Locations.FirstOrDefault(x => x.IsInSource), kind, name, message));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"StatusAnalyzer threw an exception: {ex.Message}");
				if (!string.IsNullOrEmpty(ex.StackTrace))
					Console.WriteLine(ex.StackTrace);
			}

			return;
		}

		/// <summary>
		/// Checks if the attribute is the desired attribute.
		/// </summary>
		/// <param name="semanticModel">The current semantic model.</param>
		/// <param name="attribute">>The current attribute data.</param>
		/// <param name="desiredAttributeType">The target attribute type to check for.</param>
		/// <returns>Whether the <paramref name="attribute"/> contains the <paramref name="desiredAttributeType"/>.</returns>
		private static bool IsRequiredAttribute(SemanticModel semanticModel, AttributeData attribute, Type desiredAttributeType)
		{
			if (desiredAttributeType.FullName is null)
				return false;

			var desiredTypeNamedSymbol = semanticModel.Compilation.GetTypeByMetadataName(desiredAttributeType.FullName);

			var result = attribute.AttributeClass?.Equals(desiredTypeNamedSymbol, SymbolEqualityComparer.Default) ?? false;
			return result;
		}

		/// <summary>
		/// Gets the message from the attribute.
		/// </summary>
		/// <param name="attribute">The current attribute data.</param>
		/// <returns>The message.</returns>
		private static string GetMessage(AttributeData attribute)
			=> attribute.ConstructorArguments.Length < 1
				? "Do not use in production."
				: attribute.ConstructorArguments[0].Value as string;

		/// <summary>
		/// Gets the feature message from the attribute.
		/// </summary>
		/// <param name="attribute">The current attribute data.</param>
		/// <returns>The message.</returns>
		private static string GetFeatureMessage(AttributeData attribute)
		{
			if (attribute is null)
				return string.Empty;

			var featureReqEnum = (Features)attribute.ConstructorArguments[0].Value!;
			var description = attribute.ConstructorArguments.Length > 1
				? attribute.ConstructorArguments[1].Value as string
				: "No additional information.";
			return $"{featureReqEnum.ToFeaturesString()} | {description}";
		}
	}

	/// <summary>
	/// Represents a various helper methods.
	/// </summary>
	internal static class Helpers
	{
		/// <summary>
		/// Gets the feature strings.
		/// </summary>
		internal static Dictionary<Features, string> FeaturesStrings { get; set; }

		/// <summary>
		/// Initializes the <see cref="Helpers"/> class.
		/// </summary>
		static Helpers()
		{
			FeaturesStrings = new Dictionary<Features, string>();
			var t = typeof(Features);
			var ti = t.GetTypeInfo();
			var vals = Enum.GetValues(t).Cast<Features>();

			foreach (var xv in vals)
			{
				var xsv = xv.ToString();
				var xmv = ti.DeclaredMembers.FirstOrDefault(xm => xm.Name == xsv);
				var xav = xmv?.GetCustomAttribute<FeatureDescriptionAttribute>();

				FeaturesStrings[xv] = xav?.Description ?? "No description given.";
			}
		}

		/// <summary>
		/// Converts a feature enum to a string.
		/// </summary>
		/// <param name="features">The feature enum to convert.</param>
		/// <returns>The string representation of the feature enum.</returns>
		public static string ToFeaturesString(this Features features)
		{
			var strs = FeaturesStrings
				.Where(xkvp => (features & xkvp.Key) == xkvp.Key)
				.Select(xkvp => xkvp.Value);

			return string.Join(", ", strs.OrderBy(xs => xs));
		}
	}
}
