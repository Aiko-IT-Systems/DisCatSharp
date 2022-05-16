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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands;

/// <summary>
/// Represents a <see cref="CommandWorker"/>.
/// </summary>
internal class CommandWorker
{
	/// <summary>
	/// Parses context menu application commands.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="methods">List of method infos.</param>
	/// <param name="translator">The optional command translations.</param>
	/// <returns>Too much.</returns>
	internal static Task<
		(
			List<DiscordApplicationCommand> applicationCommands,
			List<KeyValuePair<Type, Type>> commandTypeSources,
			List<ContextMenuCommand> contextMenuCommands,
			bool withLocalization
		)
		> ParseContextMenuCommands(Type type, IEnumerable<MethodInfo> methods, List<CommandTranslator> translator = null)
	{
		List<DiscordApplicationCommand> commands = new();
		List<KeyValuePair<Type, Type>> commandTypeSources = new();
		List<ContextMenuCommand> contextMenuCommands = new();


		foreach (var contextMethod in methods)
		{
			var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();

			DiscordApplicationCommandLocalization nameLocalizations = null;

			var commandTranslation = translator?.Single(c => c.Name == contextAttribute.Name && c.Type == contextAttribute.Type);
			if (commandTranslation != null)
				nameLocalizations = commandTranslation.NameTranslations;

			var command = new DiscordApplicationCommand(contextAttribute.Name, null, null, contextAttribute.Type, nameLocalizations, null, contextAttribute.DefaultMemberPermissions, contextAttribute.DmPermission);

			var parameters = contextMethod.GetParameters();
			if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(ContextMenuContext)))
				throw new ArgumentException($"The first argument must be a ContextMenuContext!");
			if (parameters.Length > 1)
				throw new ArgumentException($"A context menu cannot have parameters!");

			contextMenuCommands.Add(new ContextMenuCommand { Method = contextMethod, Name = contextAttribute.Name });

			commands.Add(command);
			commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));
		}

		return Task.FromResult((commands, commandTypeSources, contextMenuCommands, translator != null));
	}

	/// <summary>
	/// Parses single application commands.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="methods">List of method infos.</param>
	/// <param name="guildId">The optional guild id.</param>
	/// <param name="translator">The optional command translations.</param>
	/// <returns>Too much.</returns>
	internal static async Task<
		(
			List<DiscordApplicationCommand> applicationCommands,
			List<KeyValuePair<Type, Type>> commandTypeSources,
			List<CommandMethod> commandMethods,
			bool withLocalization
		)
		> ParseBasicSlashCommandsAsync(Type type, IEnumerable<MethodInfo> methods, ulong? guildId = null, List<CommandTranslator> translator = null)
	{
		List<DiscordApplicationCommand> commands = new();
		List<KeyValuePair<Type, Type>> commandTypeSources = new();
		List<CommandMethod> commandMethods = new();

		foreach (var method in methods)
		{
			var commandAttribute = method.GetCustomAttribute<SlashCommandAttribute>();

			var parameters = method.GetParameters();
			if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
				throw new ArgumentException($"The first argument must be an InteractionContext!");
			parameters = parameters.Skip(1).ToArray();
			var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters, guildId);

			commandMethods.Add(new CommandMethod { Method = method, Name = commandAttribute.Name });

			DiscordApplicationCommandLocalization nameLocalizations = null;
			DiscordApplicationCommandLocalization descriptionLocalizations = null;
			List<DiscordApplicationCommandOption> localizedOptions = null;

			var commandTranslation = translator?.Single(c => c.Name == commandAttribute.Name && c.Type == ApplicationCommandType.ChatInput);

			if (commandTranslation != null && commandTranslation.Options != null)
			{
				localizedOptions = new List<DiscordApplicationCommandOption>(options.Count);
				foreach (var option in options)
				{
					var choices = option.Choices != null ? new List<DiscordApplicationCommandOptionChoice>(option.Choices.Count) : null;
					if (option.Choices != null)
					{
						foreach (var choice in option.Choices)
						{
							choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, commandTranslation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations));
						}
					}

					localizedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
						choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
						commandTranslation.Options.Single(o => o.Name == option.Name).NameTranslations, commandTranslation.Options.Single(o => o.Name == option.Name).DescriptionTranslations
					));
				}

				nameLocalizations = commandTranslation.NameTranslations;
				descriptionLocalizations = commandTranslation.DescriptionTranslations;
			}

			var payload = new DiscordApplicationCommand(commandAttribute.Name, commandAttribute.Description, (localizedOptions != null && localizedOptions.Any() ? localizedOptions : null) ?? (options != null && options.Any() ? options : null), ApplicationCommandType.ChatInput, nameLocalizations, descriptionLocalizations, commandAttribute.DefaultMemberPermissions, commandAttribute.DmPermission);
			commands.Add(payload);
			commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));
		}

		return (commands, commandTypeSources, commandMethods, translator != null);
	}
}

/// <summary>
/// Represents a <see cref="NestedCommandWorker"/>.
/// </summary>
internal class NestedCommandWorker
{
	/// <summary>
	/// Parses application command groups.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="types">List of type infos.</param>
	/// <param name="guildId">The optional guild id.</param>
	/// <param name="translator">The optional group translations.</param>
	/// <returns>Too much.</returns>
	internal static async Task<
		(
			List<DiscordApplicationCommand> applicationCommands,
			List<KeyValuePair<Type, Type>> commandTypeSources,
			List<object> singletonModules,
			List<GroupCommand> groupCommands,
			List<SubGroupCommand> subGroupCommands,
			bool withLocalization
		)
		> ParseSlashGroupsAsync(Type type, List<TypeInfo> types, ulong? guildId = null, List<GroupTranslator> translator = null)
	{
		List<DiscordApplicationCommand> commands = new();
		List<KeyValuePair<Type, Type>> commandTypeSources = new();
		List<GroupCommand> groupCommands = new();
		List<SubGroupCommand> subGroupCommands = new();
		List<object> singletonModules = new();

		//Handles groups
		foreach (var subclassInfo in types)
		{
			//Gets the attribute and methods in the group
			var groupAttribute = subclassInfo.GetCustomAttribute<SlashCommandGroupAttribute>();
			var submethods = subclassInfo.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null).ToList();
			var subclasses = subclassInfo.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
			if (subclasses.Any() && submethods.Any())
			{
				throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
			}

			DiscordApplicationCommandLocalization nameLocalizations = null;
			DiscordApplicationCommandLocalization descriptionLocalizations = null;

			if (translator != null)
			{
				var commandTranslation = translator.Single(c => c.Name == groupAttribute.Name);
				if (commandTranslation != null)
				{
					nameLocalizations = commandTranslation.NameTranslations;
					descriptionLocalizations = commandTranslation.DescriptionTranslations;
				}
			}

			//Initializes the command
			var payload = new DiscordApplicationCommand(groupAttribute.Name, groupAttribute.Description, nameLocalizations: nameLocalizations, descriptionLocalizations: descriptionLocalizations, defaultMemberPermissions: groupAttribute.DefaultMemberPermissions, dmPermission: groupAttribute.DmPermission);
			commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));

			var commandMethods = new List<KeyValuePair<string, MethodInfo>>();
			//Handles commands in the group
			foreach (var submethod in submethods)
			{
				var commandAttribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

				//Gets the parameters and accounts for InteractionContext
				var parameters = submethod.GetParameters();
				if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
					throw new ArgumentException($"The first argument must be an InteractionContext!");
				parameters = parameters.Skip(1).ToArray();

				var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters, guildId);

				DiscordApplicationCommandLocalization subNameLocalizations = null;
				DiscordApplicationCommandLocalization subDescriptionLocalizations = null;
				List<DiscordApplicationCommandOption> localizedOptions = null;

				var commandTranslation = translator?.Single(c => c.Name == payload.Name);

				if (commandTranslation?.Commands != null)
				{

					var subCommandTranslation = commandTranslation.Commands.Single(sc => sc.Name == commandAttribute.Name);
					if (subCommandTranslation.Options != null)
					{
						localizedOptions = new List<DiscordApplicationCommandOption>(options.Count);
						foreach (var option in options)
						{
							var choices = option.Choices != null ? new List<DiscordApplicationCommandOptionChoice>(option.Choices.Count) : null;
							if (option.Choices != null)
							{
								foreach (var choice in option.Choices)
								{
									choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, subCommandTranslation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations));
								}
							}

							localizedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
								choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
								subCommandTranslation.Options.Single(o => o.Name == option.Name).NameTranslations, subCommandTranslation.Options.Single(o => o.Name == option.Name).DescriptionTranslations
							));
						}
					}

					subNameLocalizations = subCommandTranslation.NameTranslations;
					subDescriptionLocalizations = subCommandTranslation.DescriptionTranslations;
				}


				//Creates the subcommand and adds it to the main command
				var subpayload = new DiscordApplicationCommandOption(commandAttribute.Name, commandAttribute.Description, ApplicationCommandOptionType.SubCommand, null, null, localizedOptions ?? options, nameLocalizations: subNameLocalizations, descriptionLocalizations: subDescriptionLocalizations);
				payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, nameLocalizations: payload.NameLocalizations, descriptionLocalizations: payload.DescriptionLocalizations, defaultMemberPermissions: payload.DefaultMemberPermissions, dmPermission: payload.DmPermission);
				commandTypeSources.Add(new KeyValuePair<Type, Type>(subclassInfo, type));

				//Adds it to the method lists
				commandMethods.Add(new KeyValuePair<string, MethodInfo>(commandAttribute.Name, submethod));
				groupCommands.Add(new GroupCommand { Name = groupAttribute.Name, Methods = commandMethods });
			}

			var command = new SubGroupCommand { Name = groupAttribute.Name };
			//Handles subgroups
			foreach (var subclass in subclasses)
			{
				var subgroupAttribute = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
				var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

				var options = new List<DiscordApplicationCommandOption>();

				var currentMethods = new List<KeyValuePair<string, MethodInfo>>();


				DiscordApplicationCommandLocalization subNameLocalizations = null;
				DiscordApplicationCommandLocalization subDescriptionLocalizations = null;

				if (translator != null)
				{
					var commandTranslation = translator.Single(c => c.Name == payload.Name);
					if (commandTranslation != null && commandTranslation.SubGroups != null)
					{

						var subCommandTranslation = commandTranslation.SubGroups.Single(sc => sc.Name == subgroupAttribute.Name);

						if (subCommandTranslation != null)
						{
							subNameLocalizations = subCommandTranslation.NameTranslations;
							subDescriptionLocalizations = subCommandTranslation.DescriptionTranslations;
						}
					}
				}

				//Similar to the one for regular groups
				foreach (var subsubmethod in subsubmethods)
				{
					var suboptions = new List<DiscordApplicationCommandOption>();
					var commatt = subsubmethod.GetCustomAttribute<SlashCommandAttribute>();
					var parameters = subsubmethod.GetParameters();
					if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
						throw new ArgumentException($"The first argument must be an InteractionContext!");

					parameters = parameters.Skip(1).ToArray();
					suboptions = suboptions.Concat(await ApplicationCommandsExtension.ParseParametersAsync(parameters, guildId)).ToList();

					DiscordApplicationCommandLocalization subSubNameLocalizations = null;
					DiscordApplicationCommandLocalization subSubDescriptionLocalizations = null;
					List<DiscordApplicationCommandOption> localizedOptions = null;

					var commandTranslation = translator?.Single(c => c.Name == payload.Name);

					var subCommandTranslation = commandTranslation?.SubGroups?.Single(sc => sc.Name == commatt.Name);

					var subSubCommandTranslation = subCommandTranslation?.Commands.Single(sc => sc.Name == commatt.Name);

					if (subSubCommandTranslation != null && subSubCommandTranslation.Options != null)
					{
						localizedOptions = new List<DiscordApplicationCommandOption>(suboptions.Count);
						foreach (var option in suboptions)
						{
							var choices = option.Choices != null ? new List<DiscordApplicationCommandOptionChoice>(option.Choices.Count) : null;
							if (option.Choices != null)
							{
								foreach (var choice in option.Choices)
								{
									choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, subSubCommandTranslation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations));
								}
							}

							localizedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
								choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
								subSubCommandTranslation.Options.Single(o => o.Name == option.Name).NameTranslations, subSubCommandTranslation.Options.Single(o => o.Name == option.Name).DescriptionTranslations
							));
						}

						subSubNameLocalizations = subSubCommandTranslation.NameTranslations;
						subSubDescriptionLocalizations = subSubCommandTranslation.DescriptionTranslations;
					}

					var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, (localizedOptions != null && localizedOptions.Any() ? localizedOptions : null) ?? (suboptions != null && suboptions.Any() ? suboptions : null), nameLocalizations: subSubNameLocalizations, descriptionLocalizations: subSubDescriptionLocalizations);
					options.Add(subsubpayload);
					commandMethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
					currentMethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
				}

				//Adds the group to the command and method lists
				var subpayload = new DiscordApplicationCommandOption(subgroupAttribute.Name, subgroupAttribute.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options, nameLocalizations: subNameLocalizations, descriptionLocalizations: subDescriptionLocalizations);
				command.SubCommands.Add(new GroupCommand { Name = subgroupAttribute.Name, Methods = currentMethods });
				payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, nameLocalizations: payload.NameLocalizations, descriptionLocalizations: payload.DescriptionLocalizations, defaultMemberPermissions: payload.DefaultMemberPermissions, dmPermission: payload.DmPermission);
				commandTypeSources.Add(new KeyValuePair<Type, Type>(subclass, type));

				//Accounts for lifespans for the sub group
				if (subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null && subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
				{
					singletonModules.Add(ApplicationCommandsExtension.CreateInstance(subclass, ApplicationCommandsExtension.Configuration?.ServiceProvider));
				}
			}
			if (command.SubCommands.Any()) subGroupCommands.Add(command);
			commands.Add(payload);

			//Accounts for lifespans
			if (subclassInfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null && subclassInfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
			{
				singletonModules.Add(ApplicationCommandsExtension.CreateInstance(subclassInfo, ApplicationCommandsExtension.Configuration?.ServiceProvider));
			}
		}

		return (commands, commandTypeSources, singletonModules, groupCommands, subGroupCommands, translator != null);
	}
}
