// This file is part of the DisCatSharp project, a fork of DSharpPlus.
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
        /// <param name="Type">The type.</param>
        /// <param name="Methods">List of method infos.</param>
        /// <param name="Guildid">The optional guild id.</param>
        /// <param name="Translator">The optional command translations.</param>
        /// <returns>Too much.</returns>
        internal static Task<Tuple<List<DiscordApplicationCommand>, List<KeyValuePair<Type, Type>>, List<ContextMenuCommand>>> ParseContextMenuCommands(Type Type, IEnumerable<MethodInfo> Methods, ulong? Guildid = null, List<CommandTranslator> Translator = null)
        {
            List<DiscordApplicationCommand> commands = new();
            List<KeyValuePair<Type, Type>> commandTypeSources = new();
            List<ContextMenuCommand> contextMenuCommands = new();


            foreach (var contextMethod in Methods)
            {
                var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();

                DiscordApplicationCommandLocalization nameLocalizations = null;

                if (Translator != null)
                {
                    var commandTranslation = Translator.Single(C => C.Name == contextAttribute.Name && C.Type == contextAttribute.Type);
                    if (commandTranslation != null)
                        nameLocalizations = commandTranslation.NameTranslations;
                }

                var command = new DiscordApplicationCommand(contextAttribute.Name, null, null, contextAttribute.DefaultPermission, contextAttribute.Type, nameLocalizations);

                var parameters = contextMethod.GetParameters();
                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(ContextMenuContext)))
                    throw new ArgumentException($"The first argument must be a ContextMenuContext!");
                if (parameters.Length > 1)
                    throw new ArgumentException($"A context menu cannot have parameters!");

                contextMenuCommands.Add(new ContextMenuCommand { Method = contextMethod, Name = contextAttribute.Name });

                commands.Add(command);
                commandTypeSources.Add(new KeyValuePair<Type, Type>(Type, Type));
            }

            return Task.FromResult(Tuple.Create(commands, commandTypeSources, contextMenuCommands));
        }

        /// <summary>
        /// Parses single application commands.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="Methods">List of method infos.</param>
        /// <param name="Guildid">The optional guild id.</param>
        /// <param name="Translator">The optional command translations.</param>
        /// <returns>Too much.</returns>
        internal static async Task<Tuple<List<DiscordApplicationCommand>, List<KeyValuePair<Type, Type>>, List<CommandMethod>>> ParseBasicSlashCommandsAsync(Type Type, IEnumerable<MethodInfo> Methods, ulong? Guildid = null, List<CommandTranslator> Translator = null)
        {
            List<DiscordApplicationCommand> commands = new();
            List<KeyValuePair<Type, Type>> commandTypeSources = new();
            List<CommandMethod> commandMethods = new();

            foreach (var method in Methods)
            {
                var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

                var parameters = method.GetParameters();
                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                parameters = parameters.Skip(1).ToArray();
                var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters, Guildid);

                commandMethods.Add(new CommandMethod { Method = method, Name = commandattribute.Name });

                DiscordApplicationCommandLocalization nameLocalizations = null;
                DiscordApplicationCommandLocalization descriptionLocalizations = null;
                List<DiscordApplicationCommandOption> localizisedOptions = null;

                if (Translator != null)
                {
                    var commandTranslation = Translator.Single(C => C.Name == commandattribute.Name && C.Type == ApplicationCommandType.ChatInput);

                    if (commandTranslation != null)
                    {
                        if (commandTranslation.Options != null)
                        {
                            localizisedOptions = new(options.Count);
                            foreach (var option in options)
                            {
                                List<DiscordApplicationCommandOptionChoice> choices = option.Choices != null ? new(option.Choices.Count()) : null;
                                if (option.Choices != null)
                                {
                                    foreach (var choice in option.Choices)
                                    {
                                        choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, commandTranslation.Options.Single(O => O.Name == option.Name).Choices.Single(C => C.Name == choice.Name).NameTranslations));
                                    }
                                }

                                localizisedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
                                    choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
                                    commandTranslation.Options.Single(O => O.Name == option.Name).NameTranslations, commandTranslation.Options.Single(O => O.Name == option.Name).DescriptionTranslations
                                ));
                            }
                        }

                        nameLocalizations = commandTranslation.NameTranslations;
                        descriptionLocalizations = commandTranslation.DescriptionTranslations;
                    }
                }

                var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, localizisedOptions ?? options, commandattribute.DefaultPermission, ApplicationCommandType.ChatInput, nameLocalizations, descriptionLocalizations);
                commands.Add(payload);
                commandTypeSources.Add(new KeyValuePair<Type, Type>(Type, Type));
            }

            return Tuple.Create(commands, commandTypeSources, commandMethods);
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
        /// <param name="Type">The type.</param>
        /// <param name="Types">List of type infos.</param>
        /// <param name="Guildid">The optional guild id.</param>
        /// <param name="Translator">The optional group translations.</param>
        /// <returns>Too much.</returns>
        internal static async Task<
            Tuple<
                List<DiscordApplicationCommand>,
                List<KeyValuePair<Type, Type>>,
                List<object>,
                List<GroupCommand>,
                List<SubGroupCommand>
                >
            > ParseSlashGroupsAsync(Type Type, List<TypeInfo> Types, ulong? Guildid = null, List<GroupTranslator> Translator = null)
        {
            List<DiscordApplicationCommand> commands = new();
            List<KeyValuePair<Type, Type>> commandTypeSources = new();
            List<GroupCommand> groupCommands = new();
            List<SubGroupCommand> subGroupCommands = new();
            List<object> singletonModules = new();

            //Handles groups
            foreach (var subclassinfo in Types)
            {
                //Gets the attribute and methods in the group
                var groupAttribute = subclassinfo.GetCustomAttribute<SlashCommandGroupAttribute>();
                var submethods = subclassinfo.DeclaredMethods.Where(X => X.GetCustomAttribute<SlashCommandAttribute>() != null);
                var subclasses = subclassinfo.DeclaredNestedTypes.Where(X => X.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
                if (subclasses.Any() && submethods.Any())
                {
                    throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
                }

                DiscordApplicationCommandLocalization nameLocalizations = null;
                DiscordApplicationCommandLocalization descriptionLocalizations = null;

                if (Translator != null)
                {
                    var commandTranslation = Translator.Single(C => C.Name == groupAttribute.Name);
                    if (commandTranslation != null)
                    {
                        nameLocalizations = commandTranslation.NameTranslations;
                        descriptionLocalizations = commandTranslation.DescriptionTranslations;
                    }
                }

                //Initializes the command
                var payload = new DiscordApplicationCommand(groupAttribute.Name, groupAttribute.Description, DefaultPermission: groupAttribute.DefaultPermission, NameLocalizations: nameLocalizations, DescriptionLocalizations: descriptionLocalizations);
                commandTypeSources.Add(new KeyValuePair<Type, Type>(Type, Type));

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

                    var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters, Guildid);

                    DiscordApplicationCommandLocalization subNameLocalizations = null;
                    DiscordApplicationCommandLocalization subDescriptionLocalizations = null;
                    List<DiscordApplicationCommandOption> localizisedOptions = null;

                    if (Translator != null)
                    {
                        var commandTranslation = Translator.Single(C => C.Name == payload.Name);

                        if (commandTranslation.Commands != null)
                        {

                            var subCommandTranslation = commandTranslation.Commands.Single(Sc => Sc.Name == commandAttribute.Name);
                            if (subCommandTranslation.Options != null)
                            {
                                localizisedOptions = new(options.Count);
                                foreach (var option in options)
                                {
                                    List<DiscordApplicationCommandOptionChoice> choices = option.Choices != null ? new(option.Choices.Count()) : null;
                                    if (option.Choices != null)
                                    {
                                        foreach (var choice in option.Choices)
                                        {
                                            choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, subCommandTranslation.Options.Single(O => O.Name == option.Name).Choices.Single(C => C.Name == choice.Name).NameTranslations));
                                        }
                                    }

                                    localizisedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
                                        choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
                                        subCommandTranslation.Options.Single(O => O.Name == option.Name).NameTranslations, subCommandTranslation.Options.Single(O => O.Name == option.Name).DescriptionTranslations
                                    ));
                                }
                            }

                            subNameLocalizations = subCommandTranslation.NameTranslations;
                            subDescriptionLocalizations = subCommandTranslation.DescriptionTranslations;
                        }
                    }


                    //Creates the subcommand and adds it to the main command
                    var subpayload = new DiscordApplicationCommandOption(commandAttribute.Name, commandAttribute.Description, ApplicationCommandOptionType.SubCommand, null, null, localizisedOptions ?? options, NameLocalizations: subNameLocalizations, DescriptionLocalizations: subDescriptionLocalizations);
                    payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission, NameLocalizations: payload.NameLocalizations, DescriptionLocalizations: payload.DescriptionLocalizations);
                    commandTypeSources.Add(new KeyValuePair<Type, Type>(subclassinfo, Type));

                    //Adds it to the method lists
                    commandmethods.Add(new KeyValuePair<string, MethodInfo>(commandAttribute.Name, submethod));
                    groupCommands.Add(new GroupCommand { Name = groupAttribute.Name, Methods = commandmethods });
                }

                var command = new SubGroupCommand { Name = groupAttribute.Name };
                //Handles subgroups
                foreach (var subclass in subclasses)
                {
                    var subGroupAttribute = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
                    var subsubmethods = subclass.DeclaredMethods.Where(X => X.GetCustomAttribute<SlashCommandAttribute>() != null);

                    var options = new List<DiscordApplicationCommandOption>();

                    var currentMethods = new List<KeyValuePair<string, MethodInfo>>();


                    DiscordApplicationCommandLocalization subNameLocalizations = null;
                    DiscordApplicationCommandLocalization subDescriptionLocalizations = null;

                    if (Translator != null)
                    {
                        var commandTranslation = Translator.Single(C => C.Name == payload.Name);
                        if (commandTranslation != null && commandTranslation.SubGroups != null)
                        {

                            var subCommandTranslation = commandTranslation.SubGroups.Single(Sc => Sc.Name == subGroupAttribute.Name);

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
                        suboptions = suboptions.Concat(await ApplicationCommandsExtension.ParseParametersAsync(parameters, Guildid)).ToList();

                        DiscordApplicationCommandLocalization subSubNameLocalizations = null;
                        DiscordApplicationCommandLocalization subSubDescriptionLocalizations = null;
                        List<DiscordApplicationCommandOption> localizisedOptions = null;

                        if (Translator != null)
                        {
                            var commandTranslation = Translator.Single(C => C.Name == payload.Name);

                            if (commandTranslation.SubGroups != null)
                            {
                                var subCommandTranslation = commandTranslation.SubGroups.Single(Sc => Sc.Name == commatt.Name);

                                if (subCommandTranslation != null)
                                {
                                    var subSubCommandTranslation = subCommandTranslation.Commands.Single(Sc => Sc.Name == commatt.Name);

                                    if (subSubCommandTranslation != null)
                                    {
                                        if (subSubCommandTranslation.Options != null)
                                        {
                                            localizisedOptions = new(suboptions.Count);
                                            foreach (var option in suboptions)
                                            {
                                                List<DiscordApplicationCommandOptionChoice> choices = option.Choices != null ? new(option.Choices.Count) : null;
                                                if (option.Choices != null)
                                                {
                                                    foreach (var choice in option.Choices)
                                                    {
                                                        choices.Add(new DiscordApplicationCommandOptionChoice(choice.Name, choice.Value, subSubCommandTranslation.Options.Single(O => O.Name == option.Name).Choices.Single(C => C.Name == choice.Name).NameTranslations));
                                                    }
                                                }

                                                localizisedOptions.Add(new DiscordApplicationCommandOption(option.Name, option.Description, option.Type, option.Required,
                                                    choices, option.Options, option.ChannelTypes, option.AutoComplete, option.MinimumValue, option.MaximumValue,
                                                    subSubCommandTranslation.Options.Single(O => O.Name == option.Name).NameTranslations, subSubCommandTranslation.Options.Single(O => O.Name == option.Name).DescriptionTranslations
                                                ));
                                            }
                                        }

                                        subSubNameLocalizations = subSubCommandTranslation.NameTranslations;
                                        subSubDescriptionLocalizations = subSubCommandTranslation.DescriptionTranslations;
                                    }
                                }
                            }
                        }

                        var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, localizisedOptions ?? suboptions, NameLocalizations: subSubNameLocalizations, DescriptionLocalizations: subSubDescriptionLocalizations);
                        options.Add(subsubpayload);
                        commandmethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                        currentMethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                    }

                    //Adds the group to the command and method lists
                    var subpayload = new DiscordApplicationCommandOption(subGroupAttribute.Name, subGroupAttribute.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options, NameLocalizations: subNameLocalizations, DescriptionLocalizations: subDescriptionLocalizations);
                    command.SubCommands.Add(new GroupCommand { Name = subGroupAttribute.Name, Methods = currentMethods });
                    payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission, NameLocalizations: payload.NameLocalizations, DescriptionLocalizations: payload.DescriptionLocalizations);
                    commandTypeSources.Add(new KeyValuePair<Type, Type>(subclass, Type));

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

            return Tuple.Create(commands, commandTypeSources, singletonModules, groupCommands, subGroupCommands);
        }
    }
}
