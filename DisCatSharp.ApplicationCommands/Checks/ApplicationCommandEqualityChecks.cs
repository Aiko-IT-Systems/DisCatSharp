using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Checks;

/// <summary>
/// The application command equality checks.
/// </summary>
internal static class ApplicationCommandEqualityChecks
{
	/// <summary>
	/// Whether two application commands are equal.
	/// </summary>
	/// <param name="ac1">Source command.</param>
	/// <param name="targetApplicationCommand">Command to check against.</param>
	/// <param name="client">The discord client.</param>
	/// <param name="isGuild">Whether the equal check is performed for a guild command.</param>
	internal static bool IsEqualTo(
		this DiscordApplicationCommand? ac1,
		DiscordApplicationCommand? targetApplicationCommand, DiscordClient client, bool isGuild
	)
	{
		if (targetApplicationCommand is null || ac1 is null)
			return false;

		DiscordApplicationCommand sourceApplicationCommand = new(
			ac1.Name, ac1.Description, ac1.Options,
			ac1.Type,
			ac1.NameLocalizations, ac1.DescriptionLocalizations,
			ac1.DefaultMemberPermissions, ac1.DmPermission ?? true,
			ac1.IsNsfw, ac1.AllowedContexts, ac1.IntegrationTypes
		);

		if (sourceApplicationCommand.DefaultMemberPermissions == Permissions.None &&
		    targetApplicationCommand.DefaultMemberPermissions == null)
			sourceApplicationCommand.DefaultMemberPermissions = null;

		if (isGuild)
		{
			sourceApplicationCommand.DmPermission = null;
			targetApplicationCommand.DmPermission = null;
			sourceApplicationCommand.IntegrationTypes = null;
			targetApplicationCommand.IntegrationTypes = null;
			sourceApplicationCommand.AllowedContexts = null;
			targetApplicationCommand.AllowedContexts = null;
		}
		else
		{
			sourceApplicationCommand.IntegrationTypes ??= new()
			{
				ApplicationCommandIntegrationTypes.GuildInstall
			};
			targetApplicationCommand.IntegrationTypes ??= new()
			{
				ApplicationCommandIntegrationTypes.GuildInstall
			};
		}

		client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel,
			"[AC Change Check] Command {name}\n\n[{jsonOne},{jsontwo}]\n\n", ac1.Name,
			JsonConvert.SerializeObject(sourceApplicationCommand),
			JsonConvert.SerializeObject(targetApplicationCommand));

		return ac1.Type == targetApplicationCommand.Type && sourceApplicationCommand.SoftEqual(targetApplicationCommand,
			ac1.Type, client, ApplicationCommandsExtension.Configuration?.EnableLocalization ?? false, isGuild);
	}

	/// <summary>
	/// Checks softly whether two <see cref="DiscordApplicationCommand"/>s are the same.
	/// Excluding id, application id and version here.
	/// </summary>
	/// <param name="source">Source application command.</param>
	/// <param name="target">Application command to check against.</param>
	/// <param name="type">The application command type.</param>
	/// <param name="client">The discord client.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	/// <param name="guild">Whether the equal check is performed for a guild command.</param>
	internal static bool SoftEqual(
		this DiscordApplicationCommand source, DiscordApplicationCommand target,
		ApplicationCommandType type, BaseDiscordClient client, bool localizationEnabled = false, bool guild = false
	)
	{
		bool? sDmPerm = source.DmPermission ?? true;
		bool? tDmPerm = target.DmPermission ?? true;
		if (!guild)
			return localizationEnabled
				? type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, client, true, sDmPerm, tDmPerm),
					_ => source.Name == target.Name
					     && source.Type == target.Type && source.NameLocalizations == target.NameLocalizations
					     && source.DefaultMemberPermissions == target.DefaultMemberPermissions
					     && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
					     && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
					     source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes) &&
					     source.RawNameLocalizations.AreDictionariesEqual(target.RawNameLocalizations)
				}
				: type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, client, false, sDmPerm, tDmPerm),
					_ => source.Name == target.Name
					     && source.Type == target.Type
					     && source.DefaultMemberPermissions == target.DefaultMemberPermissions
					     && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
					     && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
					     source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
				};

		sDmPerm = null;
		tDmPerm = null;
		return localizationEnabled
			? type switch
			{
				ApplicationCommandType.ChatInput => DeepEqual(source, target, client, true, sDmPerm, tDmPerm),
				_ => source.Name == target.Name
				     && source.Type == target.Type && source.NameLocalizations == target.NameLocalizations
				     && source.DefaultMemberPermissions == target.DefaultMemberPermissions
				     && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
				     && source.RawNameLocalizations.AreDictionariesEqual(target.RawNameLocalizations)
			}
			: type switch
			{
				ApplicationCommandType.ChatInput => DeepEqual(source, target, client, false, sDmPerm, tDmPerm),
				_ => source.Name == target.Name
				     && source.Type == target.Type
				     && source.DefaultMemberPermissions == target.DefaultMemberPermissions
				     && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
			};
	}

	/// <summary>
	/// Performs a SequenceEqual on an enumerable if both <paramref name="source"/> and <paramref name="target"/> is not null.
	/// </summary>
	/// <typeparam name="T">The containing type within the list.</typeparam>
	/// <param name="source">The source enumerable.</param>
	/// <param name="target">The target enumerable.</param>
	/// <returns>Whether both nullable enumerable are equal.</returns>
	internal static bool NullableSequenceEqual<T>(this List<T>? source, List<T>? target)
	{
		if (source is not null && target is not null)
			return source.All(target.Contains) && source.Count == target.Count;

		return source is null && target is null;
	}

	/// <summary>
	/// Checks whether two dictionaries are equal.
	/// </summary>
	/// <param name="sourceDictionary">The source dictionary.</param>
	/// <param name="targetDictionary">The target dictionary.</param>
	/// <returns>Whether both dictionaries are equal.</returns>
	internal static bool AreDictionariesEqual(this Dictionary<string, string> sourceDictionary, Dictionary<string, string> targetDictionary)
	{
		if (sourceDictionary?.Count != targetDictionary?.Count)
			return false;

		if (sourceDictionary is null && targetDictionary is null)
			return true;

		foreach (var kvp in sourceDictionary)
			if (!targetDictionary.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
				return false;

		return true;
	}

	/// <summary>
	/// Checks deeply whether two <see cref="DiscordApplicationCommand"/>s are the same.
	/// Excluding id, application id and version here.
	/// </summary>
	/// <param name="source">Source application command.</param>
	/// <param name="target">Application command to check against.</param>
	/// <param name="client">The discord client.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	/// <param name="sDmPerm">The source dm permission.</param>
	/// <param name="tDmPerm">The target dm permission.</param>
	internal static bool DeepEqual(
		DiscordApplicationCommand source, DiscordApplicationCommand target, BaseDiscordClient client,
		bool localizationEnabled = false, bool? sDmPerm = null, bool? tDmPerm = null
	)
	{
		var name = source.Name;
		var rootCheck = source.Name == target.Name &&
		                source.Description == target.Description &&
		                source.Type == target.Type &&
		                source.DefaultMemberPermissions == target.DefaultMemberPermissions &&
		                sDmPerm == tDmPerm &&
		                source.IsNsfw == target.IsNsfw &&
		                source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
		                source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes);

		if (localizationEnabled)
			rootCheck = rootCheck &&
			            source.RawNameLocalizations.AreDictionariesEqual(target.RawNameLocalizations) &&
			            source.RawDescriptionLocalizations.AreDictionariesEqual(target.RawDescriptionLocalizations);

		var optionsEqual = DeepEqualOptions(source.Options, target.Options, localizationEnabled);
		if (optionsEqual.Reason is not null)
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "Inequality found in options of {name} - {reason}", name, optionsEqual.Reason);
		if (!rootCheck)
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "Inequality found in root of {name}", name);

		return rootCheck && optionsEqual.Equal;
	}

	/// <summary>
	/// Checks deeply whether <see cref="DiscordApplicationCommandOption"/>s are the same.
	/// </summary>
	/// <param name="sourceOptions">Source options.</param>
	/// <param name="targetOptions">Options to check against.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	private static (bool Equal, string? Reason) DeepEqualOptions(
		IReadOnlyList<DiscordApplicationCommandOption>? sourceOptions,
		IReadOnlyList<DiscordApplicationCommandOption>? targetOptions, bool localizationEnabled
	)
	{
		if (sourceOptions == null && targetOptions == null)
			return (true, null);

		if ((sourceOptions != null && targetOptions == null) || (sourceOptions == null && targetOptions != null))
			return (false, "source or target option null, but not both");

		if (sourceOptions!.Count != targetOptions!.Count)
			return (false, "source option count mismatches target option count");

		for (var i = 0; i < sourceOptions.Count; i++)
		{
			var sourceOption = sourceOptions[i];
			var targetOption = targetOptions[i];

			var optionCheck = sourceOption.Name == targetOption.Name &&
			                  sourceOption.Description == targetOption.Description &&
			                  sourceOption.Type == targetOption.Type &&
			                  sourceOption.Required == targetOption.Required &&
			                  sourceOption.AutoComplete == targetOption.AutoComplete &&
			                  sourceOption.MinimumValue?.ToString() == targetOption.MinimumValue?.ToString() &&
			                  sourceOption.MaximumValue?.ToString() == targetOption.MaximumValue?.ToString() &&
			                  sourceOption.MinimumLength == targetOption.MinimumLength &&
			                  sourceOption.MaximumLength == targetOption.MaximumLength;

			if (localizationEnabled)
				optionCheck = optionCheck &&
				              sourceOption.RawNameLocalizations.AreDictionariesEqual(targetOption.RawNameLocalizations) &&
				              sourceOption.RawDescriptionLocalizations.AreDictionariesEqual(targetOption
					              .RawDescriptionLocalizations);

			if ((sourceOption.Choices is null && targetOption.Choices is not null) ||
			    (sourceOption.Choices is not null && targetOption.Choices is null))
				return (false, "source or target choices null, but not both - " + sourceOption.Name);

			if (sourceOption.Choices is not null && targetOption.Choices is not null)
			{
				var j1 = JsonConvert.SerializeObject(sourceOption.Choices.OrderBy(x => x.Name), Formatting.None);
				var j2 = JsonConvert.SerializeObject(targetOption.Choices.OrderBy(x => x.Name), Formatting.None);
				if (j1 != j2)
					return (false, "choice mismatch - " + sourceOption.Name);
			}

			if ((sourceOption.ChannelTypes is null && targetOption.ChannelTypes is not null) ||
			    (sourceOption.ChannelTypes is not null && targetOption.ChannelTypes is null) ||
			    (sourceOption.ChannelTypes is not null && targetOption.ChannelTypes is not null &&
			     !sourceOption.ChannelTypes.OrderBy(x => x).All(targetOption.ChannelTypes.OrderBy(x => x).Contains)))
				return (false, "channel type mismatch - " + sourceOption.Name);

			var rec = DeepEqualOptions(sourceOption.Options, targetOption.Options, localizationEnabled);
			if (!rec.Equal)
				return (false, "Options Recursive - " + sourceOption.Name + " -- " + rec.Reason);

			if (!optionCheck)
				return (false, "OptionCheck - " + sourceOption.Name);
		}

		return (true, null);
	}
}
