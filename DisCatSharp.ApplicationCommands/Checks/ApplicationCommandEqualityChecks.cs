using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Checks;

/// <summary>
///     The application command equality checks.
/// </summary>
internal static class ApplicationCommandEqualityChecks
{
	/// <summary>
	///     Whether two application commands are equal.
	/// </summary>
	/// <param name="ac1">Source command.</param>
	/// <param name="targetApplicationCommand">Command to check against.</param>
	/// <param name="client">The discord client.</param>
	/// <param name="isGuild">Whether the equal check is performed for a guild command.</param>
	internal static bool IsEqualTo(
		this DiscordApplicationCommand? ac1,
		DiscordApplicationCommand? targetApplicationCommand,
		DiscordClient client,
		bool isGuild
	)
	{
		if (targetApplicationCommand is null || ac1 is null)
			return false;

		DiscordApplicationCommand sourceApplicationCommand = new(
			ac1.Name, ac1.Description, ac1.Options,
			ac1.Type,
			ac1.NameLocalizations, ac1.DescriptionLocalizations,
			ac1.DefaultMemberPermissions,
			ac1.IsNsfw, ac1.AllowedContexts, ac1.IntegrationTypes, ac1.HandlerType
		);

		// C1: Create a defensive copy of the target to avoid mutating the caller's object
		DiscordApplicationCommand targetCopy = new(
			targetApplicationCommand.Name, targetApplicationCommand.Description, targetApplicationCommand.Options,
			targetApplicationCommand.Type,
			targetApplicationCommand.NameLocalizations, targetApplicationCommand.DescriptionLocalizations,
			targetApplicationCommand.DefaultMemberPermissions,
			targetApplicationCommand.IsNsfw, targetApplicationCommand.AllowedContexts, targetApplicationCommand.IntegrationTypes, targetApplicationCommand.HandlerType
		);

		if (sourceApplicationCommand.DefaultMemberPermissions is Permissions.None &&
			targetCopy.DefaultMemberPermissions is null)
			sourceApplicationCommand.DefaultMemberPermissions = null;

		if (isGuild)
		{
			sourceApplicationCommand.IntegrationTypes = null;
			targetCopy.IntegrationTypes = null;
			sourceApplicationCommand.AllowedContexts = null;
			targetCopy.AllowedContexts = null;
		}
		else
		{
			sourceApplicationCommand.IntegrationTypes ??= [ApplicationCommandIntegrationTypes.GuildInstall];
			targetCopy.IntegrationTypes ??= [ApplicationCommandIntegrationTypes.GuildInstall];
		}

		if (sourceApplicationCommand.Type is not ApplicationCommandType.PrimaryEntryPoint)
		{
			sourceApplicationCommand.HandlerType = null;
			targetCopy.HandlerType = null;
		}

		client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel,
			"[AC Change Check] Command {name}\n\n[{jsonOne},{jsontwo}]\n\n", ac1.Name,
			JsonConvert.SerializeObject(sourceApplicationCommand),
			JsonConvert.SerializeObject(targetCopy));

		return ac1.Type == targetApplicationCommand.Type && sourceApplicationCommand.SoftEqual(targetCopy,
			ac1.Type, client, ApplicationCommandsExtension.Configuration?.EnableLocalization ?? false, isGuild);
	}

	/// <summary>
	///     Checks softly whether two <see cref="DiscordApplicationCommand" />s are the same.
	///     Excluding id, application id and version here.
	/// </summary>
	/// <param name="source">Source application command.</param>
	/// <param name="target">Application command to check against.</param>
	/// <param name="type">The application command type.</param>
	/// <param name="client">The discord client.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	/// <param name="guild">Whether the equal check is performed for a guild command.</param>
	internal static bool SoftEqual(
		this DiscordApplicationCommand source,
		DiscordApplicationCommand target,
		ApplicationCommandType type,
		BaseDiscordClient client,
		bool localizationEnabled = false,
		bool guild = false
	)
	{
		return !guild
			? localizationEnabled
				? type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, client, true),
					ApplicationCommandType.PrimaryEntryPoint => source.Name == target.Name
						 && source.Type == target.Type
						 && source.Description == target.Description
						 && source.DefaultMemberPermissions == target.DefaultMemberPermissions
						 && source.IsNsfw == target.IsNsfw
						 && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts)
						 && source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
						 && source.HandlerType == target.HandlerType
						 && source.RawNameLocalizations.AreDictionariesEqual(target.RawNameLocalizations)
						 && source.RawDescriptionLocalizations.AreDictionariesEqual(target.RawDescriptionLocalizations),
					_ => source.Name == target.Name
						 && source.Type == target.Type
						 && source.DefaultMemberPermissions == target.DefaultMemberPermissions
						 && source.IsNsfw == target.IsNsfw
						 && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts)
						 && source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
						 && source.RawNameLocalizations.AreDictionariesEqual(target.RawNameLocalizations)
				}
				: type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, client),
					ApplicationCommandType.PrimaryEntryPoint => source.Name == target.Name
						 && source.Type == target.Type
						 && source.Description == target.Description
						 && source.DefaultMemberPermissions == target.DefaultMemberPermissions
						 && source.IsNsfw == target.IsNsfw
						 && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts)
						 && source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
						 && source.HandlerType == target.HandlerType,
					_ => source.Name == target.Name
						 && source.Type == target.Type
						 && source.DefaultMemberPermissions == target.DefaultMemberPermissions
						 && source.IsNsfw == target.IsNsfw
						 && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts)
						 && source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
				}
			: localizationEnabled
				? type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, client, true),
					_ => source.Name == target.Name
						 && source.Type == target.Type
						 && source.DefaultMemberPermissions == target.DefaultMemberPermissions
						 && source.IsNsfw == target.IsNsfw
						 && source.RawNameLocalizations.AreDictionariesEqual(target.RawNameLocalizations)
				}
				: type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, client),
					_ => source.Name == target.Name
						 && source.Type == target.Type
						 && source.DefaultMemberPermissions == target.DefaultMemberPermissions
						 && source.IsNsfw == target.IsNsfw
				};
	}

	/// <summary>
	///     Performs a SequenceEqual on an enumerable if both <paramref name="source" /> and <paramref name="target" /> is not
	///     null.
	/// </summary>
	/// <typeparam name="T">The containing type within the list.</typeparam>
	/// <param name="source">The source enumerable.</param>
	/// <param name="target">The target enumerable.</param>
	/// <returns>Whether both nullable enumerable are equal.</returns>
	internal static bool NullableSequenceEqual<T>(this List<T>? source, List<T>? target)
		=> source is not null && target is not null
			? source.OrderBy(x => x).SequenceEqual(target.OrderBy(x => x))
			: source is null && target is null;

	/// <summary>
	///     Checks whether two dictionaries are equal.
	/// </summary>
	/// <param name="sourceDictionary">The source dictionary.</param>
	/// <param name="targetDictionary">The target dictionary.</param>
	/// <returns>Whether both dictionaries are equal.</returns>
	internal static bool AreDictionariesEqual(this Dictionary<string, string>? sourceDictionary, Dictionary<string, string>? targetDictionary)
	{
		if (sourceDictionary?.Count != targetDictionary?.Count)
			return false;

		if (sourceDictionary is null && targetDictionary is null)
			return true;

		if (sourceDictionary is null || targetDictionary is null)
			return false;

		foreach (var kvp in sourceDictionary)
			if (!targetDictionary.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
				return false;

		return true;
	}

	/// <summary>
	///     Checks deeply whether two <see cref="DiscordApplicationCommand" />s are the same.
	///     Excluding id, application id and version here.
	/// </summary>
	/// <param name="source">Source application command.</param>
	/// <param name="target">Application command to check against.</param>
	/// <param name="client">The discord client.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	internal static bool DeepEqual(
		DiscordApplicationCommand source,
		DiscordApplicationCommand target,
		BaseDiscordClient client,
		bool localizationEnabled = false
	)
	{
		var name = source.Name;
		var rootCheck = source.Name == target.Name &&
						source.Description == target.Description &&
						source.Type == target.Type &&
						source.DefaultMemberPermissions == target.DefaultMemberPermissions &&
						source.IsNsfw == target.IsNsfw &&
						source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
						source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes);

		if (localizationEnabled)
			rootCheck = rootCheck &&
						source.RawNameLocalizations.AreDictionariesEqual(target.RawNameLocalizations) &&
						source.RawDescriptionLocalizations.AreDictionariesEqual(target.RawDescriptionLocalizations);

		var (equal, reason) = DeepEqualOptions(source.Options, target.Options, localizationEnabled);
		if (reason is not null)
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "Inequality found in options of {name} - {reason}", name, reason);
		if (!rootCheck)
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "Inequality found in root of {name}", name);

		return rootCheck && equal;
	}

	/// <summary>
	///     Checks deeply whether <see cref="DiscordApplicationCommandOption" />s are the same.
	/// </summary>
	/// <param name="sourceOptions">Source options.</param>
	/// <param name="targetOptions">Options to check against.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	private static (bool Equal, string? Reason) DeepEqualOptions(
		List<DiscordApplicationCommandOption>? sourceOptions,
		List<DiscordApplicationCommandOption>? targetOptions,
		bool localizationEnabled
	)
	{
		if (sourceOptions is null && targetOptions is null)
			return (true, null);

		if ((sourceOptions is not null && targetOptions is null) || (sourceOptions is null && targetOptions is not null))
			return (false, "source or target option null, but not both");

		if (sourceOptions!.Count != targetOptions!.Count)
			return (false, $"option count mismatch ({sourceOptions.Count} vs {targetOptions.Count})");

		List<string> reasons = [];

		for (var i = 0; i < sourceOptions.Count; i++)
		{
			var sourceOption = sourceOptions[i];
			var targetOption = targetOptions[i];
			var optName = sourceOption.Name ?? targetOption.Name ?? $"[{i}]";

			if (sourceOption.Name != targetOption.Name)
				reasons.Add($"{optName}: name mismatch");

			if (sourceOption.Description != targetOption.Description)
				reasons.Add($"{optName}: description mismatch");

			if (sourceOption.Type != targetOption.Type)
				reasons.Add($"{optName}: type mismatch");

			if (sourceOption.Required != targetOption.Required)
				reasons.Add($"{optName}: required mismatch");

			if (sourceOption.AutoComplete != targetOption.AutoComplete)
				reasons.Add($"{optName}: autocomplete mismatch");

			if (System.Convert.ToString(sourceOption.MinimumValue, System.Globalization.CultureInfo.InvariantCulture) != System.Convert.ToString(targetOption.MinimumValue, System.Globalization.CultureInfo.InvariantCulture))
				reasons.Add($"{optName}: minimum value mismatch");

			if (System.Convert.ToString(sourceOption.MaximumValue, System.Globalization.CultureInfo.InvariantCulture) != System.Convert.ToString(targetOption.MaximumValue, System.Globalization.CultureInfo.InvariantCulture))
				reasons.Add($"{optName}: maximum value mismatch");

			if (sourceOption.MinimumLength != targetOption.MinimumLength)
				reasons.Add($"{optName}: minimum length mismatch");

			if (sourceOption.MaximumLength != targetOption.MaximumLength)
				reasons.Add($"{optName}: maximum length mismatch");

			if (localizationEnabled)
			{
				if (!sourceOption.RawNameLocalizations.AreDictionariesEqual(targetOption.RawNameLocalizations))
					reasons.Add($"{optName}: name localizations mismatch");

				if (!sourceOption.RawDescriptionLocalizations.AreDictionariesEqual(targetOption.RawDescriptionLocalizations))
					reasons.Add($"{optName}: description localizations mismatch");
			}

			if (sourceOption.Choices is null != targetOption.Choices is null)
			{
				reasons.Add($"{optName}: choices null mismatch");
			}
			else if (sourceOption.Choices is not null && targetOption.Choices is not null)
			{
				var sourceChoices = sourceOption.Choices.OrderBy(x => x.Name).ToList();
				var targetChoices = targetOption.Choices.OrderBy(x => x.Name).ToList();

				if (sourceChoices.Count != targetChoices.Count)
				{
					reasons.Add($"{optName}: choice count mismatch ({sourceChoices.Count} vs {targetChoices.Count})");
				}
				else
				{
					for (var ci = 0; ci < sourceChoices.Count; ci++)
					{
						if (sourceChoices[ci].Name != targetChoices[ci].Name)
							reasons.Add($"{optName}: choice name mismatch at [{ci}]");

						if (!AreChoiceValuesEqual(sourceChoices[ci].Value, targetChoices[ci].Value))
							reasons.Add($"{optName}: choice value mismatch at [{ci}]");
					}
				}
			}

			if (sourceOption.ChannelTypes is null != targetOption.ChannelTypes is null)
			{
				reasons.Add($"{optName}: channel types null mismatch");
			}
			else if (sourceOption.ChannelTypes is not null && targetOption.ChannelTypes is not null &&
					 (sourceOption.ChannelTypes.Count != targetOption.ChannelTypes.Count ||
					  !sourceOption.ChannelTypes.OrderBy(x => x).SequenceEqual(targetOption.ChannelTypes.OrderBy(x => x))))
			{
				reasons.Add($"{optName}: channel types mismatch");
			}

			var (equal, reason) = DeepEqualOptions(sourceOption.Options, targetOption.Options, localizationEnabled);
			if (!equal)
				reasons.Add($"{optName} -> {reason}");
		}

		return reasons.Count > 0
			? (false, string.Join("; ", reasons))
			: (true, null);
	}

	/// <summary>
	///     Compares two choice values for equality, normalizing numeric types so that
	///     <c>42</c> (int), <c>42L</c> (long), and <c>42.0</c> (double) are considered equal
	///     when they represent the same logical value.
	/// </summary>
	/// <param name="a">The first value.</param>
	/// <param name="b">The second value.</param>
	/// <returns>Whether the two values are considered equal.</returns>
	private static bool AreChoiceValuesEqual(object? a, object? b)
	{
		if (ReferenceEquals(a, b))
			return true;
		if (a is null || b is null)
			return false;

		// String values use ordinal comparison — do NOT coerce through numeric conversion
		// (e.g. "01" and "1" must remain distinct)
		if (a is string sa && b is string sb)
			return string.Equals(sa, sb, StringComparison.Ordinal);

		// Normalize numeric types to a common representation for comparison
		if (a is IConvertible ca && b is IConvertible cb && a is not string && b is not string)
		{
			try
			{
				// If both can be represented as long without loss, compare as long
				var la = ca.ToInt64(System.Globalization.CultureInfo.InvariantCulture);
				var lb = cb.ToInt64(System.Globalization.CultureInfo.InvariantCulture);

				// Verify no precision was lost by round-tripping
				var da = ca.ToDouble(System.Globalization.CultureInfo.InvariantCulture);
				var db = cb.ToDouble(System.Globalization.CultureInfo.InvariantCulture);

				if (da == la && db == lb)
					return la == lb;

				// Fall back to double comparison for fractional values
				return da == db;
			}
			catch
			{
				// Not numeric — fall through to string comparison
			}
		}

		return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
	}
}
