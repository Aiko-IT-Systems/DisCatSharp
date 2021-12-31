// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp.ApplicationCommands
{
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
		internal static Task<(List<DiscordApplicationCommand> applicationCommands, List<KeyValuePair<Type, Type>> commandTypeSources, List<ContextMenuCommand> contextMenuCommands)> ParseContextMenuCommands(Type type, IEnumerable<MethodInfo> methods, List<CommandTranslator> translator = null)
		{
			List<DiscordApplicationCommand> commands = new();
			List<KeyValuePair<Type, Type>> commandTypeSources = new();
			List<ContextMenuCommand> contextMenuCommands = new();


			foreach (var contextMethod in methods)
			{
				var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();

				DiscordApplicationCommandLocalization NameLocalizations = null;

				var command_translation = translator?.Single(c => c.Name == contextAttribute.Name && c.Type == contextAttribute.Type);
				if (command_translation != null)
					NameLocalizations = command_translation.NameTranslations;

				var command = new DiscordApplicationCommand(contextAttribute.Name, null, null, contextAttribute.DefaultPermission, contextAttribute.Type, NameLocalizations);

				var parameters = contextMethod.GetParameters();
				if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(ContextMenuContext)))
					throw new ArgumentException($"The first argument must be a ContextMenuContext!");
				if (parameters.Length > 1)
					throw new ArgumentException($"A context menu cannot have parameters!");

				contextMenuCommands.Add(new ContextMenuCommand { Method = contextMethod, Name = contextAttribute.Name });

				commands.Add(command);
				commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));
			}

			return Task.FromResult((commands, commandTypeSources, contextMenuCommands));
		}

		/// <summary>
		/// Parses single application commands.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="methods">List of method infos.</param>
		/// <param name="guildId">The optional guild id.</param>
		/// <param name="translator">The optional command translations.</param>
		/// <returns>Too much.</returns>
		internal static async Task<(List<DiscordApplicationCommand> applicationCommands, List<KeyValuePair<Type, Type>> commandTypeSources, List<CommandMethod> commandMethods)> ParseBasicSlashCommandsAsync(Type type, IEnumerable<MethodInfo> methods, ulong? guildId = null, List<CommandTranslator> translator = null)
		{
			List<DiscordApplicationCommand> commands = new();
			List<KeyValuePair<Type, Type>> commandTypeSources = new();
			List<CommandMethod> commandMethods = new();

			foreach (var method in methods)
			{
				var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

				var parameters = method.GetParameters();
				if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
					throw new ArgumentException($"The first argument must be an InteractionContext!");
				parameters = parameters.Skip(1).ToArray();
				var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters, guildId);

				commandMethods.Add(new CommandMethod { Method = method, Name = commandattribute.Name });

				DiscordApplicationCommandLocalization NameLocalizations = null;
				DiscordApplicationCommandLocalization DescriptionLocalizations = null;
				List<DiscordApplicationCommandOption> LocalizisedOptions = null;

				var command_translation = translator?.Single(c => c.Name == commandattribute.Name && c.Type == ApplicationCommandType.ChatInput);

				if (command_translation != null)
				{
					if (command_translation.Options != null)
					{
						LocalizisedOptions = new(options.Count);
						foreach (var option in options)
						{
							List<DiscordApplicationCommandOptionChoice> choices = option.Choices != null ? new(option.Choices.Count()) : null;
							if (option.Choices != null)
							{
								foreach (var choice in option.Choices)
								{
									choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, command_translation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations));
								}
							}

							LocalizisedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
								choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
								command_translation.Options.Single(o => o.Name == option.Name).NameTranslations, command_translation.Options.Single(o => o.Name == option.Name).DescriptionTranslations
							));
						}
					}

					NameLocalizations = command_translation.NameTranslations;
					DescriptionLocalizations = command_translation.DescriptionTranslations;
				}

				var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, LocalizisedOptions ?? options, commandattribute.DefaultPermission, ApplicationCommandType.ChatInput, NameLocalizations, DescriptionLocalizations);
				commands.Add(payload);
				commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));
			}

			return (commands, commandTypeSources, commandMethods);
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
				List<SubGroupCommand> subGroupCommands
			)
			> ParseSlashGroupsAsync(Type type, List<TypeInfo> types, ulong? guildId = null, List<GroupTranslator> translator = null)
		{
			List<DiscordApplicationCommand> commands = new();
			List<KeyValuePair<Type, Type>> commandTypeSources = new();
			List<GroupCommand> groupCommands = new();
			List<SubGroupCommand> subGroupCommands = new();
			List<object> singletonModules = new();

			//Handles groups
			foreach (var subclassinfo in types)
			{
				//Gets the attribute and methods in the group
				var groupAttribute = subclassinfo.GetCustomAttribute<SlashCommandGroupAttribute>();
				var submethods = subclassinfo.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null).ToList();
				var subclasses = subclassinfo.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
				if (subclasses.Any() && submethods.Any())
				{
					throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
				}

				DiscordApplicationCommandLocalization NameLocalizations = null;
				DiscordApplicationCommandLocalization DescriptionLocalizations = null;

				if (translator != null)
				{
					var command_translation = translator.Single(c => c.Name == groupAttribute.Name);
					if (command_translation != null)
					{
						NameLocalizations = command_translation.NameTranslations;
						DescriptionLocalizations = command_translation.DescriptionTranslations;
					}
				}

				//Initializes the command
				var payload = new DiscordApplicationCommand(groupAttribute.Name, groupAttribute.Description, default_permission: groupAttribute.DefaultPermission, nameLocalizations: NameLocalizations, descriptionLocalizations: DescriptionLocalizations);
				commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));

				var commandmethods = new List<KeyValuePair<string, MethodInfo>>();
				//Handles commands in the group
				foreach (var submethod in submethods)
				{
					var commandAttribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

					//Gets the paramaters and accounts for InteractionContext
					var parameters = submethod.GetParameters();
					if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
						throw new ArgumentException($"The first argument must be an InteractionContext!");
					parameters = parameters.Skip(1).ToArray();

					var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters, guildId);

					DiscordApplicationCommandLocalization SubNameLocalizations = null;
					DiscordApplicationCommandLocalization SubDescriptionLocalizations = null;
					List<DiscordApplicationCommandOption> LocalizisedOptions = null;

					var command_translation = translator?.Single(c => c.Name == payload.Name);

					if (command_translation?.Commands != null)
					{

						var sub_command_translation = command_translation.Commands.Single(sc => sc.Name == commandAttribute.Name);
						if (sub_command_translation.Options != null)
						{
							LocalizisedOptions = new(options.Count);
							foreach (var option in options)
							{
								List<DiscordApplicationCommandOptionChoice> choices = option.Choices != null ? new(option.Choices.Count()) : null;
								if (option.Choices != null)
								{
									foreach (var choice in option.Choices)
									{
										choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, sub_command_translation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations));
									}
								}

								LocalizisedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
									choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
									sub_command_translation.Options.Single(o => o.Name == option.Name).NameTranslations, sub_command_translation.Options.Single(o => o.Name == option.Name).DescriptionTranslations
								));
							}
						}

						SubNameLocalizations = sub_command_translation.NameTranslations;
						SubDescriptionLocalizations = sub_command_translation.DescriptionTranslations;
					}


					//Creates the subcommand and adds it to the main command
					var subpayload = new DiscordApplicationCommandOption(commandAttribute.Name, commandAttribute.Description, ApplicationCommandOptionType.SubCommand, null, null, LocalizisedOptions ?? options, nameLocalizations: SubNameLocalizations, descriptionLocalizations: SubDescriptionLocalizations);
					payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission, nameLocalizations: payload.NameLocalizations, descriptionLocalizations: payload.DescriptionLocalizations);
					commandTypeSources.Add(new KeyValuePair<Type, Type>(subclassinfo, type));

					//Adds it to the method lists
					commandmethods.Add(new KeyValuePair<string, MethodInfo>(commandAttribute.Name, submethod));
					groupCommands.Add(new GroupCommand { Name = groupAttribute.Name, Methods = commandmethods });
				}

				var command = new SubGroupCommand { Name = groupAttribute.Name };
				//Handles subgroups
				foreach (var subclass in subclasses)
				{
					var subGroupAttribute = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
					var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

					var options = new List<DiscordApplicationCommandOption>();

					var currentMethods = new List<KeyValuePair<string, MethodInfo>>();


					DiscordApplicationCommandLocalization SubNameLocalizations = null;
					DiscordApplicationCommandLocalization SubDescriptionLocalizations = null;

					if (translator != null)
					{
						var command_translation = translator.Single(c => c.Name == payload.Name);
						if (command_translation != null && command_translation.SubGroups != null)
						{

							var sub_command_translation = command_translation.SubGroups.Single(sc => sc.Name == subGroupAttribute.Name);

							if (sub_command_translation != null)
							{
								SubNameLocalizations = sub_command_translation.NameTranslations;
								SubDescriptionLocalizations = sub_command_translation.DescriptionTranslations;
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

						DiscordApplicationCommandLocalization SubSubNameLocalizations = null;
						DiscordApplicationCommandLocalization SubSubDescriptionLocalizations = null;
						List<DiscordApplicationCommandOption> LocalizisedOptions = null;

						var command_translation = translator?.Single(c => c.Name == payload.Name);

						var sub_command_translation = command_translation?.SubGroups?.Single(sc => sc.Name == commatt.Name);

						var sub_sub_command_translation = sub_command_translation?.Commands.Single(sc => sc.Name == commatt.Name);

						if (sub_sub_command_translation != null)
						{
							if (sub_sub_command_translation.Options != null)
							{
								LocalizisedOptions = new(suboptions.Count);
								foreach (var option in suboptions)
								{
									List<DiscordApplicationCommandOptionChoice> choices = option.Choices != null ? new(option.Choices.Count) : null;
									if (option.Choices != null)
									{
										foreach (var choice in option.Choices)
										{
											choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, sub_sub_command_translation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations));
										}
									}

									LocalizisedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
										choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
										sub_sub_command_translation.Options.Single(o => o.Name == option.Name).NameTranslations, sub_sub_command_translation.Options.Single(o => o.Name == option.Name).DescriptionTranslations
									));
								}
							}

							SubSubNameLocalizations = sub_sub_command_translation.NameTranslations;
							SubSubDescriptionLocalizations = sub_sub_command_translation.DescriptionTranslations;
						}

						var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, LocalizisedOptions ?? suboptions, nameLocalizations: SubSubNameLocalizations, descriptionLocalizations: SubSubDescriptionLocalizations);
						options.Add(subsubpayload);
						commandmethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
						currentMethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
					}

					//Adds the group to the command and method lists
					var subpayload = new DiscordApplicationCommandOption(subGroupAttribute.Name, subGroupAttribute.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options, nameLocalizations: SubNameLocalizations, descriptionLocalizations: SubDescriptionLocalizations);
					command.SubCommands.Add(new GroupCommand { Name = subGroupAttribute.Name, Methods = currentMethods });
					payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission, nameLocalizations: payload.NameLocalizations, descriptionLocalizations: payload.DescriptionLocalizations);
					commandTypeSources.Add(new KeyValuePair<Type, Type>(subclass, type));

					//Accounts for lifespans for the sub group
					if (subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null && subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
					{
						singletonModules.Add(ApplicationCommandsExtension.CreateInstance(subclass, ApplicationCommandsExtension._configuration?.ServiceProvider));
					}
				}
				if (command.SubCommands.Any()) subGroupCommands.Add(command);
				commands.Add(payload);

				//Accounts for lifespans
				if (subclassinfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null && subclassinfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
				{
					singletonModules.Add(ApplicationCommandsExtension.CreateInstance(subclassinfo, ApplicationCommandsExtension._configuration?.ServiceProvider));
				}
			}

			return (commands, commandTypeSources, singletonModules, groupCommands, subGroupCommands);
		}
	}
}
