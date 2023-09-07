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
			ac1.IsNsfw, ac1.AllowedContexts
		);

		if (sourceApplicationCommand.DefaultMemberPermissions == Permissions.None && targetApplicationCommand.DefaultMemberPermissions == null)
			sourceApplicationCommand.DefaultMemberPermissions = null;

		if (isGuild)
		{
			sourceApplicationCommand.DmPermission = null;
			targetApplicationCommand.DmPermission = null;
		}

		client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC Change Check] Command {name}\n\n[{jsonOne},{jsontwo}]\n\n", ac1.Name, JsonConvert.SerializeObject(sourceApplicationCommand), JsonConvert.SerializeObject(targetApplicationCommand));

		return ac1.Type == targetApplicationCommand.Type && sourceApplicationCommand.SoftEqual(targetApplicationCommand, ac1.Type, ApplicationCommandsExtension.Configuration?.EnableLocalization ?? false, isGuild);
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
	internal static bool SoftEqual(this DiscordApplicationCommand source, DiscordApplicationCommand target, ApplicationCommandType type, bool localizationEnabled = false, bool guild = false)
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
				}
				: type switch
				{
					ApplicationCommandType.ChatInput => DeepEqual(source, target, false, sDmPerm, tDmPerm),
					_ => source.Name == target.Name
						 && source.Type == target.Type
						 && source.DefaultMemberPermissions == target.DefaultMemberPermissions
						 && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw
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
					&& sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw && source.AllowedContexts == target.AllowedContexts
			}
			: type switch
			{
				ApplicationCommandType.ChatInput => DeepEqual(source, target, false, sDmPerm, tDmPerm),
				_ => source.Name == target.Name
					&& source.Type == target.Type
					&& source.DefaultMemberPermissions == target.DefaultMemberPermissions
					&& sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw && source.AllowedContexts == target.AllowedContexts
			};
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
	internal static bool DeepEqual(DiscordApplicationCommand source, DiscordApplicationCommand target, bool localizationEnabled = false, bool? sDmPerm = null, bool? tDmPerm = null)
	{
		var rootCheck = true;
		rootCheck = source.Name == target.Name && source.Description == target.Description && source.Type == target.Type && source.DefaultMemberPermissions == target.DefaultMemberPermissions && sDmPerm == tDmPerm && source.IsNsfw == target.IsNsfw && source.AllowedContexts == target.AllowedContexts;

		if (localizationEnabled)
			rootCheck = rootCheck && source.NameLocalizations == target.NameLocalizations && source.DescriptionLocalizations == target.DescriptionLocalizations;

		if (source.Options == null && target.Options == null)
			return rootCheck;

		if ((source.Options != null && target.Options == null) || (source.Options == null && target.Options != null))
			return false;

		if (source.Options.Any(o => o.Type is ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType.SubCommand) && target.Options.Any(o => o.Type is ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType.SubCommand))
		{
			var eqCheck1 = true;
			var eqCheck2 = true;
			if (source.Options.Any(o => o.Type == ApplicationCommandOptionType.SubCommandGroup) &&
				target.Options.Any(o => o.Type == ApplicationCommandOptionType.SubCommandGroup))
			{
				List<DiscordApplicationCommandOption> minimalSourceOptions = new();

				foreach (var option in
						 source.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommandGroup))
				{
					List<DiscordApplicationCommandOption> minimalSubSourceOptions = new();
					if (option.Options != null)
					{
						minimalSubSourceOptions.AddRange(from subOption in option.Options
														 where subOption.Options != null
														 let minimalSubSubSourceOptions = subOption.Options.Select(subSubOption => new DiscordApplicationCommandOption(subSubOption.Name, subSubOption.Description, subSubOption.Type, subSubOption.Required, subSubOption.Choices, null, subSubOption.ChannelTypes?.OrderBy(x => x), subSubOption.AutoComplete, subSubOption.MinimumValue, subSubOption.MaximumValue, localizationEnabled ? subSubOption.NameLocalizations : null, localizationEnabled ? subSubOption.DescriptionLocalizations : null, subSubOption.MinimumLength, subSubOption.MaximumLength)).ToList()
														 select new DiscordApplicationCommandOption(subOption.Name, subOption.Description, subOption.Type, options: minimalSubSubSourceOptions, nameLocalizations: localizationEnabled ? subOption.NameLocalizations : null, descriptionLocalizations: localizationEnabled
															 ? subOption.DescriptionLocalizations
															 : null));
					}

					minimalSourceOptions.Add(new(
						option.Name, option.Description, option.Type,
						options: minimalSubSourceOptions,
						nameLocalizations: localizationEnabled ? option.NameLocalizations : null,
						descriptionLocalizations: localizationEnabled ? option.DescriptionLocalizations : null
					));
				}

				var minimalTargetOptions = (from option in target.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommandGroup)
											let minimalSubTargetOptions = (from subOption in option.Options
																		   where subOption.Options != null && subOption.Options.Any()
																		   let minimalSubSubTargetOptions = subOption.Options.Select(subSubOption => new DiscordApplicationCommandOption(subSubOption.Name, subSubOption.Description, subSubOption.Type, subSubOption.Required, subSubOption.Choices, null, subSubOption.ChannelTypes?.OrderBy(x => x), subSubOption.AutoComplete, subSubOption.MinimumValue, subSubOption.MaximumValue, localizationEnabled ? subSubOption.NameLocalizations : null, localizationEnabled ? subSubOption.DescriptionLocalizations : null, subSubOption.MinimumLength, subSubOption.MaximumLength)).ToList()
																		   select new DiscordApplicationCommandOption(subOption.Name, subOption.Description, subOption.Type, options: minimalSubSubTargetOptions, nameLocalizations: localizationEnabled ? subOption.NameLocalizations : null, descriptionLocalizations: localizationEnabled
							? subOption.DescriptionLocalizations
							: null)).ToList()
											select new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, options: minimalSubTargetOptions, nameLocalizations: localizationEnabled ? option.NameLocalizations : null, descriptionLocalizations: localizationEnabled ? option.DescriptionLocalizations : null)).ToList();

				var sOpt = JsonConvert.SerializeObject(minimalSourceOptions, Formatting.None);
				var tOpt = JsonConvert.SerializeObject(minimalTargetOptions, Formatting.None);

				eqCheck1 = rootCheck && sOpt == tOpt;
			}

			if (source.Options.All(o => o.Type != ApplicationCommandOptionType.SubCommand) || target.Options.All(o => o.Type != ApplicationCommandOptionType.SubCommand))
				return eqCheck1 && eqCheck2;

			{
				List<DiscordApplicationCommandOption> minimalSourceOptions = new();
				List<DiscordApplicationCommandOption> minimalTargetOptions = new();

				foreach (var option in source.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand))
				{
					List<DiscordApplicationCommandOption> minimalSubSourceOptions = null;

					if (option.Options != null)
						minimalSubSourceOptions = option.Options.Select(subOption => new DiscordApplicationCommandOption(subOption.Name, subOption.Description, subOption.Type, subOption.Required, subOption.Choices, null, subOption.ChannelTypes?.OrderBy(x => x), subOption.AutoComplete, subOption.MinimumValue, subOption.MaximumValue, localizationEnabled ? subOption.NameLocalizations : null, localizationEnabled ? subOption.DescriptionLocalizations : null, subOption.MinimumLength, subOption.MaximumLength)).ToList();

					minimalSourceOptions.Add(new(
						option.Name, option.Description, option.Type,
						options: minimalSubSourceOptions,
						nameLocalizations: localizationEnabled ? option.NameLocalizations : null,
						descriptionLocalizations: localizationEnabled ? option.DescriptionLocalizations : null
					));
				}

				foreach (var option in target.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand))
				{
					List<DiscordApplicationCommandOption> minimalSubTargetOptions = null;

					if (option.Options != null && option.Options.Any())
						minimalSubTargetOptions = option.Options.Select(subOption => new DiscordApplicationCommandOption(subOption.Name, subOption.Description, subOption.Type, subOption.Required, subOption.Choices, null, subOption.ChannelTypes?.OrderBy(x => x), subOption.AutoComplete, subOption.MinimumValue, subOption.MaximumValue, localizationEnabled ? subOption.NameLocalizations : null, localizationEnabled ? subOption.DescriptionLocalizations : null, subOption.MinimumLength, subOption.MaximumLength)).ToList();

					minimalTargetOptions.Add(new(
						option.Name, option.Description, option.Type,
						options: minimalSubTargetOptions,
						nameLocalizations: localizationEnabled ? option.NameLocalizations : null,
						descriptionLocalizations: localizationEnabled ? option.DescriptionLocalizations : null
					));
				}

				var sOpt = JsonConvert.SerializeObject(minimalSourceOptions, Formatting.None);
				var tOpt = JsonConvert.SerializeObject(minimalTargetOptions, Formatting.None);

				eqCheck2 = rootCheck && sOpt == tOpt;
			}

			return eqCheck1 && eqCheck2;
		}
		// ReSharper disable once RedundantIfElseBlock
		else
		{
			var minimalSourceOptions = source.Options.Select(option => new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required, option.Choices, null, option.ChannelTypes?.OrderBy(x => x), option.AutoComplete, option.MinimumValue, option.MaximumValue, localizationEnabled ? option.NameLocalizations : null, localizationEnabled ? option.DescriptionLocalizations : null, option.MinimumLength, option.MaximumLength)).ToList();

			var minimalTargetOptions = target.Options.Select(option => new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required, option.Choices, null, option.ChannelTypes?.OrderBy(x => x), option.AutoComplete, option.MinimumValue, option.MaximumValue, localizationEnabled ? option.NameLocalizations : null, localizationEnabled ? option.DescriptionLocalizations : null, option.MinimumLength, option.MaximumLength)).ToList();
			var sOpt = JsonConvert.SerializeObject(minimalSourceOptions, Formatting.None);
			var tOpt = JsonConvert.SerializeObject(minimalTargetOptions, Formatting.None);

			return rootCheck && sOpt == tOpt;
		}
	}
}
