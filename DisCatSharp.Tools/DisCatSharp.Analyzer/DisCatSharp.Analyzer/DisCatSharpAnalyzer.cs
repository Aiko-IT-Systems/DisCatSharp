using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using DisCatSharp.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DisCatSharp.Analyzer
{
	/// <inheritdoc />
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DisCatSharpAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>
		///     The diagnostic ID prefix.
		/// </summary>
		public const string DIAGNOSTIC_ID_PREFIX = "DCS";

		/// <summary>
		///     The diagnostic category.
		/// </summary>
		public const string CATEGORY = "Usage";

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_titleExperimental = new LocalizableResourceString(nameof(Resources.AnalyzerTitleExperimental), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_messageFormatExperimental = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatExperimental), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_descriptionExperimental = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionExperimental), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_titleDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDeprecated), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_messageFormatDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDeprecated), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_descriptionDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDeprecated), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_titleDiscordInExperiment = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordInExperiment), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_messageFormatDiscordInExperiment = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordInExperiment), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_descriptionDiscordInExperiment = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordInExperiment), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_titleDiscordDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordDeprecated), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_messageFormatDiscordDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordDeprecated), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_descriptionDiscordDeprecated = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordDeprecated), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_titleDiscordUnreleased = new LocalizableResourceString(nameof(Resources.AnalyzerTitleDiscordUnreleased), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_messageFormatDiscordUnreleased = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatDiscordUnreleased), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_descriptionDiscordUnreleased = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionDiscordUnreleased), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_titleRequiresFeature = new LocalizableResourceString(nameof(Resources.AnalyzerTitleRequiresFeature), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_messageFormatRequiresFeature = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatRequiresFeature), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_descriptionRequiresFeature = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionRequiresFeature), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_titleRequiresOverride = new LocalizableResourceString(nameof(Resources.AnalyzerTitleRequiresOverride), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_messageFormatRequiresOverride = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatRequiresOverride), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="LocalizableString" />
		private static readonly LocalizableString s_descriptionRequiresOverride = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionRequiresOverride), Resources.ResourceManager, typeof(Resources));

		/// <inheritdoc cref="DiagnosticDescriptor" />
		private static readonly DiagnosticDescriptor s_experimentalRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0001", s_titleExperimental, s_messageFormatExperimental, CATEGORY, DiagnosticSeverity.Info, true, s_descriptionExperimental, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0001");

		/// <inheritdoc cref="DiagnosticDescriptor" />
		private static readonly DiagnosticDescriptor s_deprecatedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0002", s_titleDeprecated, s_messageFormatDeprecated, CATEGORY, DiagnosticSeverity.Error, true, s_descriptionDeprecated, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0002");

		/// <inheritdoc cref="DiagnosticDescriptor" />
		private static readonly DiagnosticDescriptor s_discordInExperimentRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0101", s_titleDiscordInExperiment, s_messageFormatDiscordInExperiment, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordInExperiment, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0101");

		/// <inheritdoc cref="DiagnosticDescriptor" />
		private static readonly DiagnosticDescriptor s_discordDeprecatedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0102", s_titleDiscordDeprecated, s_messageFormatDiscordDeprecated, CATEGORY, DiagnosticSeverity.Error, true, s_descriptionDiscordDeprecated, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0102");

		/// <inheritdoc cref="DiagnosticDescriptor" />
		private static readonly DiagnosticDescriptor s_discordUnreleasedRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0103", s_titleDiscordUnreleased, s_messageFormatDiscordUnreleased, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionDiscordUnreleased, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0103");

		/// <inheritdoc cref="DiagnosticDescriptor" />
		private static readonly DiagnosticDescriptor s_requiresFeatureRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0200", s_titleRequiresFeature, s_messageFormatRequiresFeature, CATEGORY, DiagnosticSeverity.Info, true, s_descriptionRequiresFeature, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0200");

		/// <inheritdoc cref="DiagnosticDescriptor" />
		private static readonly DiagnosticDescriptor s_requiresOverrideRule = new DiagnosticDescriptor(DIAGNOSTIC_ID_PREFIX + "0201", s_titleRequiresOverride, s_messageFormatRequiresOverride, CATEGORY, DiagnosticSeverity.Warning, true, s_descriptionRequiresOverride, "https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0201");

		/// <inheritdoc />
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(s_experimentalRule, s_deprecatedRule, s_discordInExperimentRule, s_discordDeprecatedRule, s_discordUnreleasedRule, s_requiresFeatureRule, s_requiresOverrideRule);

		/// <inheritdoc />
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();

			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Parameter);
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Event);
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ObjectCreationExpression);
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ElementAccessExpression);
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.SimpleMemberAccessExpression);
		}

		/// <summary>
		///     Analyzes the symbols.
		/// </summary>
		/// <param name="context">The symbol analysis context.</param>
		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var syntaxTrees = from x in context.Symbol.Locations
			                  where x.IsInSource
			                  select x.SourceTree;
			var declaration = context.Symbol;

			if (declaration == null)
				return;

			var attributes = declaration.GetAttributes();

			var name = declaration.Name;
			var kind = declaration.Kind;

			var model = context.Compilation.GetSemanticModel(syntaxTrees.First(), true);

			var experimentalAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(ExperimentalAttribute)));
			var deprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DeprecatedAttribute)));
			var discordInExperimentAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DiscordInExperimentAttribute)));
			var discordDeprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DiscordDeprecatedAttribute)));
			var discordUnreleasedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(DiscordUnreleasedAttribute)));
			var requiresFeatureAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(RequiresFeatureAttribute)));
			var requiresOverrideAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(model, attr, typeof(RequiresOverrideAttribute)));

			if (experimentalAttributeData != null)
			{
				var message = GetMessage(experimentalAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_experimentalRule, context.Symbol.Locations.FirstOrDefault(), kind, name, message));
			}

			if (deprecatedAttributeData != null)
			{
				var message = GetMessage(deprecatedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_deprecatedRule, context.Symbol.Locations.FirstOrDefault(), kind, name, message));
			}

			if (discordInExperimentAttributeData != null)
			{
				var message = GetMessage(discordInExperimentAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordInExperimentRule, context.Symbol.Locations.FirstOrDefault(), kind, name, message));
			}

			if (discordDeprecatedAttributeData != null)
			{
				var message = GetMessage(discordDeprecatedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordDeprecatedRule, context.Symbol.Locations.FirstOrDefault(), kind, name, message));
			}

			if (discordUnreleasedAttributeData != null)
			{
				var message = GetMessage(discordUnreleasedAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_discordUnreleasedRule, context.Symbol.Locations.FirstOrDefault(), kind, name, message));
			}

			if (requiresFeatureAttributeData != null)
			{
				var message = GetFeatureMessage(requiresFeatureAttributeData);
				context.ReportDiagnostic(Diagnostic.Create(s_requiresFeatureRule, context.Symbol.Locations.FirstOrDefault(), kind, name, message));
			}

			if (requiresOverrideAttributeData != null)
			{
				var message = GetOverrideMessage(requiresOverrideAttributeData);
				var properties = ImmutableDictionary.Create<string, string>().Add("LastKnownOverride", GetOverrideValue(requiresOverrideAttributeData)).Add("OverrideDate", GetOverrideDate(requiresOverrideAttributeData));
				context.ReportDiagnostic(Diagnostic.Create(s_requiresOverrideRule, context.Symbol.Locations.FirstOrDefault(), properties, kind, name, message));
			}
		}

		/// <summary>
		///     Analyzes the syntax nodes.
		/// </summary>
		/// <param name="context">The syntac node analysis context.</param>
		private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			var invocation = context.Node;
			var declaration = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol;
			if (declaration == null)
				return;

			var attributes = declaration.GetAttributes();

			var name = declaration.Name;
			var kind = declaration.Kind;

			var experimentalAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(ExperimentalAttribute)));
			var deprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DeprecatedAttribute)));
			var discordInExperimentAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DiscordInExperimentAttribute)));
			var discordDeprecatedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DiscordDeprecatedAttribute)));
			var discordUnreleasedAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(DiscordUnreleasedAttribute)));
			var requiresFeatureAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(RequiresFeatureAttribute)));
			var requiresOverrideAttributeData = attributes.FirstOrDefault(attr => IsRequiredAttribute(context.SemanticModel, attr, typeof(RequiresOverrideAttribute)));

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

			if (requiresOverrideAttributeData != null)
			{
				var message = GetOverrideMessage(requiresOverrideAttributeData);
				var properties = ImmutableDictionary.Create<string, string>().Add("LastKnownOverride", GetOverrideValue(requiresOverrideAttributeData)).Add("OverrideDate", GetOverrideDate(requiresOverrideAttributeData));
				context.ReportDiagnostic(Diagnostic.Create(s_requiresOverrideRule, invocation.GetLocation(), properties, kind, name, message));
			}
		}

		/// <summary>
		///     Checks if the attribute is the desired attribute.
		/// </summary>
		/// <param name="semanticModel">The current semantic model.</param>
		/// <param name="attribute">>The current attribute data.</param>
		/// <param name="desiredAttributeType">The target attribute type to check for.</param>
		/// <returns>Whether the <paramref name="attribute" /> contains the <paramref name="desiredAttributeType" />.</returns>
		private static bool IsRequiredAttribute(SemanticModel semanticModel, AttributeData attribute, Type desiredAttributeType)
		{
			if (desiredAttributeType.FullName == null)
				return false;

			var desiredTypeNamedSymbol = semanticModel.Compilation.GetTypeByMetadataName(desiredAttributeType.FullName);

			var result = attribute.AttributeClass?.Equals(desiredTypeNamedSymbol, SymbolEqualityComparer.Default) ?? false;
			return result;
		}

		/// <summary>
		///     Gets the message from the attribute.
		/// </summary>
		/// <param name="attributeData">The current attribute data.</param>
		/// <returns>The message.</returns>
		private static string GetMessage(AttributeData attributeData)
			=> attributeData.ConstructorArguments.Length < 1
				? "Do not use in production."
				: attributeData.ConstructorArguments[0].Value as string;

		/// <summary>
		///     Retrieves a formatted message indicating the required override and additional information based on the provided
		///     attribute data.
		/// </summary>
		/// <param name="attributeData">
		///     An object containing data about an attribute, which may include constructor arguments for
		///     processing.
		/// </param>
		/// <returns>
		///     A formatted string detailing the required override and any additional information or a default message if none is
		///     provided.
		/// </returns>
		private static string GetOverrideMessage(AttributeData attributeData)
		{
			if (attributeData == null)
				return string.Empty;

			var lastKnownOverride = GetOverrideValue(attributeData, true);
			var overrideDate = GetOverrideDate(attributeData, true);
			var description = attributeData.ConstructorArguments.Length > 2
				? attributeData.ConstructorArguments[2].Value as string
				: "No additional information.";

			return $"The following override is required (or newer if created after {overrideDate}): {lastKnownOverride} | {description}";
		}

		/// <summary>
		///     Retrieves the override value from the provided attribute data, returning a default value if the attribute is <see langword="null" />.
		/// </summary>
		/// <param name="attributeData">The attribute data from which to extract the override value.</param>
		/// <param name="fallback">Whether to return a default value if the attribute is <see langword="null" />.</param>
		/// <returns>Returns the override value as a string or an empty string if the attribute is <see langword="null" />.</returns>
		private static string GetOverrideValue(AttributeData attributeData, bool fallback = false)
			=> attributeData == null ? fallback ? "No known Override" : string.Empty : attributeData.ConstructorArguments[0].Value as string;

		/// <summary>
		///     Retrieves the override date from an attribute, returning a default value if the attribute is <see langword="null" />.
		/// </summary>
		/// <param name="attributeData">The attribute from which to extract the override date.</param>
		/// <param name="fallback">Whether to return a default value if the attribute is <see langword="null" />.</param>
		/// <returns>Returns the override date as a string or an empty string if the attribute is <see langword="null" />.</returns>
		private static string GetOverrideDate(AttributeData attributeData, bool fallback = false)
			=> attributeData == null ? fallback ? "No known date" : string.Empty : attributeData.ConstructorArguments[1].Value as string;

		/// <summary>
		///     Gets the feature message from the attribute.
		/// </summary>
		/// <param name="attributeData">The current attribute data.</param>
		/// <returns>The message.</returns>
		private static string GetFeatureMessage(AttributeData attributeData)
		{
			if (attributeData == null)
				return string.Empty;

			var featureReqEnum = (Features)attributeData.ConstructorArguments[0].Value;
			var description = attributeData.ConstructorArguments.Length > 1
				? attributeData.ConstructorArguments[1].Value as string
				: "No additional information.";

			return $"{featureReqEnum.ToFeaturesString()} | {description}";
		}
	}

	/// <summary>
	///     Represents a various helper methods.
	/// </summary>
	internal static class Helpers
	{
		/// <summary>
		///     Initializes the <see cref="Helpers" /> class.
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
		///     Gets the feature strings.
		/// </summary>
		internal static Dictionary<Features, string> FeaturesStrings { get; set; }

		/// <summary>
		///     Converts a feature enum to a string.
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
