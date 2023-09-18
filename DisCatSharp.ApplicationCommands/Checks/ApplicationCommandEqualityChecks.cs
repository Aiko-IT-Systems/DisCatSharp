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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Checks;

internal static class ApplicationCommandEqualityChecks
{
	/// <summary>
	/// Whether two application commands are equal.
	/// </summary>
	/// <param name="ac1">Source command.</param>
	/// <param name="targetApplicationCommand">Command to check against.</param>
	/// <param name="client">The discord client.</param>
	/// <param name="isGuild">Whether the equal check is performed for a guild command.</param>
	internal static bool IsEqualTo(this DiscordApplicationCommand? ac1, DiscordApplicationCommand? targetApplicationCommand, DiscordClient client, bool isGuild)
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
		}
		else
		{
			sourceApplicationCommand.IntegrationTypes ??= new() { ApplicationCommandIntegrationTypes.InstalledToGuild };
			targetApplicationCommand.IntegrationTypes ??= new() { ApplicationCommandIntegrationTypes.InstalledToGuild };
		}

		client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel,
			"[AC Change Check] Command {name}\n\n[{jsonOne},{jsontwo}]\n\n", ac1.Name,
			JsonConvert.SerializeObject(sourceApplicationCommand),
			JsonConvert.SerializeObject(targetApplicationCommand));

		return ac1.Type == targetApplicationCommand.Type && sourceApplicationCommand.SoftEqual(targetApplicationCommand,
			ac1.Type, ApplicationCommandsExtension.Configuration?.EnableLocalization ?? false, isGuild);
	}

	/// <summary>
	/// Checks softly whether two <see cref="DiscordApplicationCommand"/>s are the same.
	/// Excluding id, application id and version here.
	/// </summary>
	/// <param name="source">Source application command.</param>
	/// <param name="target">Application command to check against.</param>
	/// <param name="type">The application command type.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	/// <param name="guild">Whether the equal check is performed for a guild command.</param>
	internal static bool SoftEqual(this DiscordApplicationCommand source, DiscordApplicationCommand target,
		ApplicationCommandType type, bool localizationEnabled = false, bool guild = false)
	{
		bool? sDmPerm = source.DmPermission ?? true;
		bool? tDmPerm = target.DmPermission ?? true;
		if (!guild)
			return localizationEnabled
				? type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, true, sDmPerm, tDmPerm),
					_ => source.Name == target.Name
					     && source.Type == target.Type && source.NameLocalizations == target.NameLocalizations
					     && source.DefaultMemberPermissions == target.DefaultMemberPermissions
					     && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
					     && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
					     source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
				}
				: type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, false, sDmPerm, tDmPerm),
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
				ApplicationCommandType.ChatInput => DeepEqual(source, target, true, sDmPerm, tDmPerm),
				_ => source.Name == target.Name
				     && source.Type == target.Type && source.NameLocalizations == target.NameLocalizations
				     && source.DefaultMemberPermissions == target.DefaultMemberPermissions
				     && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
				     && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
				     source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
			}
			: type switch
			{
				ApplicationCommandType.ChatInput => DeepEqual(source, target, false, sDmPerm, tDmPerm),
				_ => source.Name == target.Name
				     && source.Type == target.Type
				     && source.DefaultMemberPermissions == target.DefaultMemberPermissions
				     && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
				     && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
				     source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes)
			};
	}

	/// <summary>
	/// Performs a SequenceEqual on a list if both <paramref name="source"/> and <paramref name="target"/> is not null.
	/// </summary>
	/// <typeparam name="T">The containing type within the list.</typeparam>
	/// <param name="source">The source list.</param>
	/// <param name="target">The target list.</param>
	/// <returns>Whether both nullable lists are equal.</returns>
	internal static bool NullableSequenceEqual<T>(this List<T>? source, List<T>? target)
	{
		if (source is not null && target is not null)
			return source.OrderBy(x => x).SequenceEqual(target.OrderBy(x => x));

		if ((source is not null && target is null) ||
		    (source is null && target is not null))
			return false;

		return true;
	}

	/// <summary>
	/// Checks deeply whether two <see cref="DiscordApplicationCommand"/>s are the same.
	/// Excluding id, application id and version here.
	/// </summary>
	/// <param name="source">Source application command.</param>
	/// <param name="target">Application command to check against.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	/// <param name="sDmPerm">The source dm permission.</param>
	/// <param name="tDmPerm">The target dm permission.</param>
	internal static bool DeepEqual(DiscordApplicationCommand source, DiscordApplicationCommand target,
		bool localizationEnabled = false, bool? sDmPerm = null, bool? tDmPerm = null)
	{
		var rootCheck = source.Name == target.Name &&
		                source.Description == target.Description &&
		                source.Type == target.Type &&
		                source.DefaultMemberPermissions == target.DefaultMemberPermissions &&
		                sDmPerm == tDmPerm &&
		                source.IsNsfw == target.IsNsfw
		                && source.AllowedContexts.NullableSequenceEqual(target.AllowedContexts) &&
		                source.IntegrationTypes.NullableSequenceEqual(target.IntegrationTypes);

		if (localizationEnabled)
			rootCheck = rootCheck &&
			            source.NameLocalizations.Localizations.SequenceEqual(target.NameLocalizations.Localizations) &&
			            source.DescriptionLocalizations.Localizations.SequenceEqual(target.DescriptionLocalizations
				            .Localizations);

		// Compare the Options using recursion
		var optionsEqual = DeepEqualOptions(source.Options, target.Options, localizationEnabled);

		return rootCheck && optionsEqual;
	}

	/// <summary>
	/// Checks deeply whether <see cref="DiscordApplicationCommandOption"/>s are the same.
	/// </summary>
	/// <param name="sourceOptions">Source options.</param>
	/// <param name="targetOptions">Options to check against.</param>
	/// <param name="localizationEnabled">Whether localization is enabled.</param>
	private static bool DeepEqualOptions(IReadOnlyList<DiscordApplicationCommandOption>? sourceOptions,
		IReadOnlyList<DiscordApplicationCommandOption>? targetOptions, bool localizationEnabled)
	{
		if (sourceOptions == null && targetOptions == null)
			return true;

		if ((sourceOptions != null && targetOptions == null) || (sourceOptions == null && targetOptions != null))
			return false;

		if (sourceOptions!.Count != targetOptions!.Count)
			return false;

		for (var i = 0; i < sourceOptions.Count; i++)
		{
			var sourceOption = sourceOptions[i];
			var targetOption = targetOptions[i];

			var optionCheck = sourceOption.Name == targetOption.Name &&
			                  sourceOption.Description == targetOption.Description &&
			                  sourceOption.Type == targetOption.Type &&
			                  sourceOption.Required == targetOption.Required &&
			                  sourceOption.AutoComplete == targetOption.AutoComplete &&
			                  sourceOption.MinimumValue == targetOption.MinimumValue &&
			                  sourceOption.MaximumValue == targetOption.MaximumValue &&
			                  sourceOption.MinimumLength == targetOption.MinimumLength &&
			                  sourceOption.MaximumLength == targetOption.MaximumLength;

			if (localizationEnabled)
				optionCheck = optionCheck &&
				              sourceOption.NameLocalizations.Localizations.SequenceEqual(targetOption.NameLocalizations
					              .Localizations) &&
				              sourceOption.DescriptionLocalizations.Localizations.SequenceEqual(targetOption
					              .DescriptionLocalizations.Localizations);

			if ((sourceOption.Choices is null && targetOption.Choices is not null) ||
			    (sourceOption.Choices is not null && targetOption.Choices is null))
				return false;

			if (sourceOption.Choices is not null && targetOption.Choices is not null)
			{
				var j1 = JsonConvert.SerializeObject(sourceOption.Choices.OrderBy(x => x.Name), Formatting.None);
				var j2 = JsonConvert.SerializeObject(targetOption.Choices.OrderBy(x => x.Name), Formatting.None);
				if (j1 != j2)
					return false;
			}

			if ((sourceOption.ChannelTypes is null && targetOption.ChannelTypes is not null) ||
			    (sourceOption.ChannelTypes is not null && targetOption.ChannelTypes is null) ||
			    (sourceOption.ChannelTypes is not null && targetOption.ChannelTypes is not null &&
			     !sourceOption.ChannelTypes.OrderBy(x => x).All(targetOption.ChannelTypes.OrderBy(x => x).Contains)))
				return false;

			if (!DeepEqualOptions(sourceOption.Options, targetOption.Options, localizationEnabled))
				return false;

			if (!optionCheck)
				return false;
		}

		return true;
	}
}
