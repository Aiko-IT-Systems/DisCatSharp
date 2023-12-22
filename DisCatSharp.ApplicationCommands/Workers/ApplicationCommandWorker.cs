using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.Entities;
using DisCatSharp.ApplicationCommands.Enums;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Workers;

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
	> ParseContextMenuCommands(Type type, IEnumerable<MethodInfo> methods, List<CommandTranslator>? translator = null)
	{
		List<DiscordApplicationCommand> commands = [];
		List<KeyValuePair<Type, Type>> commandTypeSources = [];
		List<ContextMenuCommand> contextMenuCommands = [];

		foreach (var contextMethod in methods)
		{
			var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();

			DiscordApplicationCommandLocalization? nameLocalizations = null;

			var commandTranslation = translator?.Single(c => c.Name == contextAttribute.Name && c.Type == contextAttribute.Type);
			if (commandTranslation is not null)
				nameLocalizations = commandTranslation.NameTranslations;

			var command = new DiscordApplicationCommand(contextAttribute.Name, null, null, contextAttribute.Type, nameLocalizations, null, contextAttribute.DefaultMemberPermissions, contextAttribute.DmPermission ?? true, contextAttribute.IsNsfw, contextAttribute.AllowedContexts, contextAttribute.IntegrationTypes);

			var parameters = contextMethod.GetParameters();
			if (parameters.Length is 0 || parameters is null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(ContextMenuContext)))
				throw new ArgumentException($"The first argument of the command '{contextAttribute.Name}' has to be an ContextMenuContext!");
			if (parameters.Length > 1)
				throw new ArgumentException($"The context menu command '{contextAttribute.Name}' cannot have parameters!");

			contextMenuCommands.Add(new()
			{
				Method = contextMethod,
				Name = contextAttribute.Name
			});

			commands.Add(command);
			commandTypeSources.Add(new(type, type));
		}

		return Task.FromResult((commands, commandTypeSources, contextMenuCommands, translator is not null));
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
	> ParseBasicSlashCommandsAsync(Type type, IEnumerable<MethodInfo> methods, ulong? guildId = null, List<CommandTranslator>? translator = null)
	{
		List<DiscordApplicationCommand> commands = [];
		List<KeyValuePair<Type, Type>> commandTypeSources = [];
		List<CommandMethod> commandMethods = [];

		foreach (var method in methods)
			try
			{
				var commandAttribute = method.GetCustomAttribute<SlashCommandAttribute>();

				var parameters = method.GetParameters();
				if (parameters.Length is 0 || parameters is null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
					throw new ArgumentException($"The first argument of the command '{commandAttribute.Name}' has to be an InteractionContext!");

				var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters.Skip(1), commandAttribute.Name, guildId).ConfigureAwait(false);

				commandMethods.Add(new()
				{
					Method = method,
					Name = commandAttribute.Name
				});

				DiscordApplicationCommandLocalization? nameLocalizations = null;
				DiscordApplicationCommandLocalization? descriptionLocalizations = null;
				List<DiscordApplicationCommandOption>? localizedOptions = null;

				var commandTranslation = translator?.Single(c => c.Name == commandAttribute.Name && c.Type is ApplicationCommandType.ChatInput);

				if (commandTranslation is { Options: not null })
				{
					localizedOptions = new(options.Count);
					foreach (var option in options)
						try
						{
							var choices = option.Choices is not null ? new List<DiscordApplicationCommandOptionChoice>(option.Choices.Count) : null;
							if (option.Choices is not null && choices is not null)
								foreach (var choice in option.Choices)
									try
									{
										choices.Add(new(choice.Name, choice.Value, commandTranslation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations));
									}
									catch (Exception ex)
									{
										throw new AggregateException($"Failed to register choice '{choice.Name}' in command '{commandAttribute.Name}'", ex);
									}

							localizedOptions.Add(new(option.Name, option.Description, option.Type, option.Required,
								choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
								commandTranslation.Options.Single(o => o.Name == option.Name).NameTranslations, commandTranslation.Options.Single(o => o.Name == option.Name).DescriptionTranslations,
								option.MinimumLength, option.MaximumLength
							));
						}
						catch (Exception ex)
						{
							throw new AggregateException($"Failed to register option '{option.Name}' in command '{commandAttribute.Name}'", ex);
						}

					nameLocalizations = commandTranslation.NameTranslations;
					descriptionLocalizations = commandTranslation.DescriptionTranslations;
				}

				var payload = new DiscordApplicationCommand(commandAttribute.Name, commandAttribute.Description, (localizedOptions is not null && localizedOptions.Count is not 0 ? localizedOptions : null) ?? (options is not null && options.Count is not 0 ? options : null), ApplicationCommandType.ChatInput, nameLocalizations, descriptionLocalizations, commandAttribute.DefaultMemberPermissions, commandAttribute.DmPermission ?? true,
					commandAttribute.IsNsfw, commandAttribute.AllowedContexts, commandAttribute.IntegrationTypes);
				commands.Add(payload);
				commandTypeSources.Add(new(type, type));
			}
			catch (Exception ex)
			{
				throw new AggregateException($"Failed to register command with method name '{method.Name}'", ex);
			}

		return (commands, commandTypeSources, commandMethods, translator is not null);
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
	> ParseSlashGroupsAsync(Type type, List<TypeInfo> types, ulong? guildId = null, List<GroupTranslator>? translator = null)
	{
		List<DiscordApplicationCommand> commands = [];
		List<KeyValuePair<Type, Type>> commandTypeSources = [];
		List<GroupCommand> groupCommands = [];
		List<SubGroupCommand> subGroupCommands = [];
		List<object> singletonModules = [];

		//Handles groups
		foreach (var subclassInfo in types)
		{
			//Gets the attribute and methods in the group
			var groupAttribute = subclassInfo.GetCustomAttribute<SlashCommandGroupAttribute>();
			var submethods = subclassInfo.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() is not null).ToList();
			var subclasses = subclassInfo.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() is not null).ToList();

			DiscordApplicationCommandLocalization? nameLocalizations = null;
			DiscordApplicationCommandLocalization? descriptionLocalizations = null;

			var groupCommandTranslation = translator?.Single(c => c.Name == groupAttribute.Name);
			if (groupCommandTranslation is not null)
			{
				nameLocalizations = groupCommandTranslation.NameTranslations;
				descriptionLocalizations = groupCommandTranslation.DescriptionTranslations;
			}

			//Initializes the command
			var payload = new DiscordApplicationCommand(groupAttribute.Name, groupAttribute.Description, nameLocalizations: nameLocalizations, descriptionLocalizations: descriptionLocalizations, defaultMemberPermissions: groupAttribute.DefaultMemberPermissions, dmPermission: groupAttribute.DmPermission ?? true, isNsfw: groupAttribute.IsNsfw, allowedContexts: groupAttribute.AllowedContexts,
				integrationTypes: groupAttribute.IntegrationTypes);
			commandTypeSources.Add(new(type, type));

			var commandMethods = new List<KeyValuePair<string, MethodInfo>>();
			//Handles commands in the group
			foreach (var submethod in submethods)
			{
				var commandAttribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

				//Gets the parameters and accounts for InteractionContext
				var parameters = submethod.GetParameters();
				if (parameters.Length is 0 || parameters is null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
					throw new ArgumentException($"The first argument of the command '{commandAttribute.Name}' has to be an InteractionContext!");

				var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters.Skip(1), commandAttribute.Name, guildId).ConfigureAwait(false);

				DiscordApplicationCommandLocalization? subNameLocalizations = null;
				DiscordApplicationCommandLocalization? subDescriptionLocalizations = null;
				List<DiscordApplicationCommandOption>? localizedOptions = null;

				var commandTranslation = translator?.Single(c => c.Name == payload.Name);

				var subCommandTranslation = commandTranslation?.Commands?.Single(sc => sc.Name == commandAttribute.Name);
				if (subCommandTranslation?.Options is not null)
				{
					localizedOptions = new(options.Count);
					foreach (var option in options)
					{
						var choices = option.Choices is not null ? new List<DiscordApplicationCommandOptionChoice>(option.Choices.Count) : null;
						if (option.Choices is not null && choices is not null)
							choices.AddRange(option.Choices.Select(choice => new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, subCommandTranslation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations)));

						localizedOptions.Add(new(option.Name, option.Description, option.Type, option.Required,
							choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
							subCommandTranslation.Options.Single(o => o.Name == option.Name).NameTranslations, subCommandTranslation.Options.Single(o => o.Name == option.Name).DescriptionTranslations,
							option.MinimumLength, option.MaximumLength
						));
					}

					subNameLocalizations = subCommandTranslation.NameTranslations;
					subDescriptionLocalizations = subCommandTranslation.DescriptionTranslations;
				}

				//Creates the subcommand and adds it to the main command
				var subpayload = new DiscordApplicationCommandOption(commandAttribute.Name, commandAttribute.Description, ApplicationCommandOptionType.SubCommand, false, null, localizedOptions ?? options, nameLocalizations: subNameLocalizations, descriptionLocalizations: subDescriptionLocalizations);
				payload = new(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, nameLocalizations: payload.NameLocalizations, descriptionLocalizations: payload.DescriptionLocalizations, defaultMemberPermissions: payload.DefaultMemberPermissions, dmPermission: payload.DmPermission ?? true, isNsfw: payload.IsNsfw, allowedContexts: payload.AllowedContexts, integrationTypes: payload.IntegrationTypes);
				commandTypeSources.Add(new(subclassInfo, type));

				//Adds it to the method lists
				commandMethods.Add(new(commandAttribute.Name, submethod));
				groupCommands.Add(new()
				{
					Name = groupAttribute.Name,
					Methods = commandMethods
				});
			}

			var command = new SubGroupCommand
			{
				Name = groupAttribute.Name
			};
			//Handles subgroups
			foreach (var subclass in subclasses)
			{
				var subgroupAttribute = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
				var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() is not null);

				var options = new List<DiscordApplicationCommandOption>();

				var currentMethods = new List<KeyValuePair<string, MethodInfo>>();

				DiscordApplicationCommandLocalization? subNameLocalizations = null;
				DiscordApplicationCommandLocalization? subDescriptionLocalizations = null;

				var subGroupCommandTranslation = translator?.Single(c => c.Name == payload.Name);
				if (subGroupCommandTranslation is { SubGroups: not null })
				{
					var subCommandTranslation = subGroupCommandTranslation?.SubGroups.Single(sc => sc.Name == subgroupAttribute.Name);

					if (subCommandTranslation is not null)
					{
						subNameLocalizations = subCommandTranslation.NameTranslations;
						subDescriptionLocalizations = subCommandTranslation.DescriptionTranslations;
					}
				}

				//Similar to the one for regular groups
				foreach (var subsubmethod in subsubmethods)
				{
					var suboptions = new List<DiscordApplicationCommandOption>();
					var commatt = subsubmethod.GetCustomAttribute<SlashCommandAttribute>();
					var parameters = subsubmethod.GetParameters();
					if (parameters.Length is 0 || parameters is null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
						throw new ArgumentException($"The first argument of the command '{subgroupAttribute.Name}' has to be an InteractionContext!");

					suboptions = [.. suboptions, .. await ApplicationCommandsExtension.ParseParametersAsync(parameters.Skip(1), subgroupAttribute.Name, guildId).ConfigureAwait(false)];

					DiscordApplicationCommandLocalization? subSubNameLocalizations = null;
					DiscordApplicationCommandLocalization? subSubDescriptionLocalizations = null;
					List<DiscordApplicationCommandOption>? localizedOptions = null;

					var commandTranslation = translator?.Single(c => c.Name == payload.Name);

					var subCommandTranslation = commandTranslation?.SubGroups?.Single(sc => sc.Name == commatt.Name);

					var subSubCommandTranslation = subCommandTranslation?.Commands.Single(sc => sc.Name == commatt.Name);

					if (subSubCommandTranslation is { Options: not null })
					{
						localizedOptions = new(suboptions.Count);
						foreach (var option in suboptions)
						{
							var choices = option.Choices is not null ? new List<DiscordApplicationCommandOptionChoice>(option.Choices.Count) : null;
							if (option.Choices is not null && choices is not null)
								choices.AddRange(option.Choices.Select(choice => new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, subSubCommandTranslation.Options.Single(o => o.Name == option.Name).Choices.Single(c => c.Name == choice.Name).NameTranslations)));

							localizedOptions.Add(new(option.Name, option.Description, option.Type, option.Required,
								choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
								subSubCommandTranslation.Options.Single(o => o.Name == option.Name).NameTranslations, subSubCommandTranslation.Options.Single(o => o.Name == option.Name).DescriptionTranslations,
								option.MinimumLength, option.MaximumLength
							));
						}

						subSubNameLocalizations = subSubCommandTranslation.NameTranslations;
						subSubDescriptionLocalizations = subSubCommandTranslation.DescriptionTranslations;
					}

					var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, false, null, (localizedOptions is not null && localizedOptions.Count is not 0 ? localizedOptions : null) ?? (suboptions is not null && suboptions.Count is not 0 ? suboptions : null), nameLocalizations: subSubNameLocalizations, descriptionLocalizations: subSubDescriptionLocalizations);
					options.Add(subsubpayload);
					commandMethods.Add(new(commatt.Name, subsubmethod));
					currentMethods.Add(new(commatt.Name, subsubmethod));
				}

				//Adds the group to the command and method lists
				var subpayload = new DiscordApplicationCommandOption(subgroupAttribute.Name, subgroupAttribute.Description, ApplicationCommandOptionType.SubCommandGroup, false, null, options, nameLocalizations: subNameLocalizations, descriptionLocalizations: subDescriptionLocalizations);
				command.SubCommands.Add(new()
				{
					Name = subgroupAttribute.Name,
					Methods = currentMethods
				});
				payload = new(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, nameLocalizations: payload.NameLocalizations, descriptionLocalizations: payload.DescriptionLocalizations, defaultMemberPermissions: payload.DefaultMemberPermissions, dmPermission: payload.DmPermission ?? true, isNsfw: payload.IsNsfw, allowedContexts: payload.AllowedContexts, integrationTypes: payload.IntegrationTypes);
				commandTypeSources.Add(new(subclass, type));

				//Accounts for lifespans for the sub group
				if (subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() is not null && subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan is ApplicationCommandModuleLifespan.Singleton)
					singletonModules.Add(ApplicationCommandsExtension.CreateInstance(subclass, ApplicationCommandsExtension.Configuration?.ServiceProvider));
			}

			if (command.SubCommands.Count is not 0) subGroupCommands.Add(command);
			commands.Add(payload);

			//Accounts for lifespans
			if (subclassInfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() is not null && subclassInfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan is ApplicationCommandModuleLifespan.Singleton)
				singletonModules.Add(ApplicationCommandsExtension.CreateInstance(subclassInfo, ApplicationCommandsExtension.Configuration?.ServiceProvider));
		}

		return (commands, commandTypeSources, singletonModules, groupCommands, subGroupCommands, translator is not null);
	}
}
