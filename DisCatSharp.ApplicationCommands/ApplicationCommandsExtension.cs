// This file is part of the DisCatSharp project.
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
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using DisCatSharp.Entities;
using System.Linq;
using DisCatSharp.EventArgs;
using Microsoft.Extensions.Logging;
using DisCatSharp.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Exceptions;
using DisCatSharp.Enums;
using DisCatSharp.ApplicationCommands.Attributes;
using System.Text.RegularExpressions;
using DisCatSharp.Common;

namespace DisCatSharp.ApplicationCommands
{
    /// <summary>
    /// A class that handles slash commands for a client.
    /// </summary>
    public sealed class ApplicationCommandsExtension : BaseExtension
    {
        /// <summary>
        /// A list of methods for top level commands.
        /// </summary>
        private static List<CommandMethod> _commandMethods { get; set; } = new List<CommandMethod>();

        /// <summary>
        /// List of groups.
        /// </summary>
        private static List<GroupCommand> _groupCommands { get; set; } = new List<GroupCommand>();

        /// <summary>
        /// List of groups with subgroups.
        /// </summary>
        private static List<SubGroupCommand> _subGroupCommands { get; set; } = new List<SubGroupCommand>();

        /// <summary>
        /// List of context menus.
        /// </summary>
        private static List<ContextMenuCommand> _contextMenuCommands { get; set; } = new List<ContextMenuCommand>();

        /// <summary>
        /// Singleton modules.
        /// </summary>
        private static List<object> _singletonModules { get; set; } = new List<object>();

        /// <summary>
        /// List of modules to register.
        /// </summary>
        private List<KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>> _updateList { get; set; } = new List<KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>>();

        /// <summary>
        /// Configuration for Discord.
        /// </summary>
        private readonly ApplicationCommandsConfiguration _configuration;

        /// <summary>
        /// Set to true if anything fails when registering.
        /// </summary>
        private static bool _errored { get; set; } = false;

        /// <summary>
        /// Gets a list of registered commands. The key is the guild id (null if global).
        /// </summary>
        public IReadOnlyList<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> RegisteredCommands
            => _registeredCommands;
        private static List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> _registeredCommands = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationCommandsExtension"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal ApplicationCommandsExtension(ApplicationCommandsConfiguration configuration)
        {
            this._configuration = configuration;
        }

        /// <summary>
        /// Runs setup. DO NOT RUN THIS MANUALLY. DO NOT DO ANYTHING WITH THIS.
        /// </summary>
        /// <param name="client">The client to setup on.</param>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            this._slashError = new AsyncEvent<ApplicationCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, null);
            this._slashExecuted = new AsyncEvent<ApplicationCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, null);
            this._contextMenuErrored = new AsyncEvent<ApplicationCommandsExtension, ContextMenuErrorEventArgs>("CONTEXTMENU_ERRORED", TimeSpan.Zero, null);
            this._contextMenuExecuted = new AsyncEvent<ApplicationCommandsExtension, ContextMenuExecutedEventArgs>("CONTEXTMENU_EXECUTED", TimeSpan.Zero, null);

            this.Client.Ready += this.Update;
            this.Client.InteractionCreated += this.InteractionHandler;
            this.Client.ContextMenuInteractionCreated += this.ContextMenuHandler;
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <typeparam name="T">The command class to register.</typeparam>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands<T>(ulong? guildId = null) where T : ApplicationCommandsModule
        {
            if (this.Client.ShardId == 0)
                this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(guildId, new ApplicationCommandsModuleConfiguration(typeof(T))));
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands(Type type, ulong? guildId = null)
        {
            if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
                throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));
            //If sharding, only register for shard 0
            if (this.Client.ShardId == 0)
                this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(guildId, new ApplicationCommandsModuleConfiguration(type)));
        }

        /// <summary>
        /// Registers a command class with permission setup.
        /// </summary>
        /// <typeparam name="T">The command class to register.</typeparam>
        /// <param name="guildId">The guild id to register it on.</param>
        /// <param name="permissionSetup">A callback to setup permissions with.</param>
        public void RegisterCommands<T>(ulong guildId, Action<ApplicationCommandsPermissionContext> permissionSetup = null) where T : ApplicationCommandsModule
        {
            if (this.Client.ShardId == 0)
                this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(guildId, new ApplicationCommandsModuleConfiguration(typeof(T), permissionSetup)));
        }

        /// <summary>
        /// Registers a command class with permission setup.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
        /// <param name="guildId">The guild id to register it on.</param>
        /// <param name="permissionSetup">A callback to setup permissions with.</param>
        public void RegisterCommands(Type type, ulong guildId, Action<ApplicationCommandsPermissionContext> permissionSetup = null)
        {
            if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
                throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));
            //If sharding, only register for shard 0
            if (this.Client.ShardId == 0)
                this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(guildId, new ApplicationCommandsModuleConfiguration(type, permissionSetup)));
        }
        /*
        /// <summary>
        /// Registers a command class with permission setup but without a guild id.
        /// </summary>
        /// <typeparam name="T">The command class to register.</typeparam>
        /// <param name="permissionSetup">A callback to setup permissions with.</param>
        public void RegisterCommands<T>(Action<ApplicationCommandsPermissionContext> permissionSetup = null) where T : ApplicationCommandsModule
        {
            if (this.Client.ShardId == 0)
                this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(null, new ApplicationCommandsModuleConfiguration(typeof(T), permissionSetup)));
        }

        /// <summary>
        /// Registers a command class with permission setup but without a guild id.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
        /// <param name="permissionSetup">A callback to setup permissions with.</param>
        public void RegisterCommands(Type type, Action<ApplicationCommandsPermissionContext> permissionSetup = null)
        {
            if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
                throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));
            //If sharding, only register for shard 0
            if (this.Client.ShardId == 0)
                this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(null, new ApplicationCommandsModuleConfiguration(type, permissionSetup)));
        }
        */
        /// <summary>
        /// To be run on ready.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="e">The ready event args.</param>
        internal Task Update(DiscordClient client, ReadyEventArgs e)
            => this.Update();

        /// <summary>
        /// Actual method for registering, used for RegisterCommands and on Ready.
        /// </summary>
        internal Task Update()
        {
            //Only update for shard 0
            if (this.Client.ShardId == 0)
            {
                //Groups commands by guild id or global
                foreach (var key in this._updateList.Select(x => x.Key).Distinct())
                {
                    this.RegisterCommands(this._updateList.Where(x => x.Key == key).Select(x => x.Value), key);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method for registering commands for a target from modules.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="guildid">The optional guild id.</param>
        private void RegisterCommands(IEnumerable<ApplicationCommandsModuleConfiguration> types, ulong? guildid)
        {
            //Initialize empty lists to be added to the global ones at the end
            var commandMethods = new List<CommandMethod>();
            var groupCommands = new List<GroupCommand>();
            var subGroupCommands = new List<SubGroupCommand>();
            var contextMenuCommands = new List<ContextMenuCommand>();
            var updateList = new List<DiscordApplicationCommand>();

            var commandTypeSources = new List<KeyValuePair<Type, Type>>();

            _ = Task.Run(async () =>
            {
                //Iterates over all the modules
                foreach (var config in types)
                {
                    var type = config.Type;
                    try
                    {
                        var module = type.GetTypeInfo();
                        var classes = new List<TypeInfo>();

                        //Add module to classes list if it's a group
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() != null)
                        {
                            classes.Add(module);
                        }
                        else
                        {
                            //Otherwise add the nested groups
                            classes = module.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
                        }

                        //Handles groups
                        foreach (var subclassinfo in classes)
                        {
                            //Gets the attribute and methods in the group
                            var groupAttribute = subclassinfo.GetCustomAttribute<SlashCommandGroupAttribute>();
                            var submethods = subclassinfo.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            var subclasses = subclassinfo.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
                            if (subclasses.Any() && submethods.Any())
                            {
                                throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
                            }

                            //Initializes the command
                            var payload = new DiscordApplicationCommand(groupAttribute.Name, groupAttribute.Description, default_permission: groupAttribute.DefaultPermission);
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

                                var options = await this.ParseParameters(parameters, guildid);

                                //Creates the subcommand and adds it to the main command
                                var subpayload = new DiscordApplicationCommandOption(commandAttribute.Name, commandAttribute.Description, ApplicationCommandOptionType.SubCommand, null, null, options);
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission);
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
                                //I couldn't think of more creative naming
                                var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                                var options = new List<DiscordApplicationCommandOption>();

                                var currentMethods = new List<KeyValuePair<string, MethodInfo>>();

                                //Similar to the one for regular groups
                                foreach (var subsubmethod in subsubmethods)
                                {
                                    var suboptions = new List<DiscordApplicationCommandOption>();
                                    var commatt = subsubmethod.GetCustomAttribute<SlashCommandAttribute>();
                                    var parameters = subsubmethod.GetParameters();
                                    if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                        throw new ArgumentException($"The first argument must be an InteractionContext!");
                                    parameters = parameters.Skip(1).ToArray();
                                    suboptions = suboptions.Concat(await this.ParseParameters(parameters, guildid)).ToList();

                                    var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, suboptions);
                                    options.Add(subsubpayload);
                                    commandmethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                                    currentMethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                                }

                                //Adds the group to the command and method lists
                                var subpayload = new DiscordApplicationCommandOption(subGroupAttribute.Name, subGroupAttribute.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options);
                                command.SubCommands.Add(new GroupCommand { Name = subGroupAttribute.Name, Methods = currentMethods });
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission);
                                commandTypeSources.Add(new KeyValuePair<Type, Type>(subclass, type));

                                //Accounts for lifespans for the sub group
                                if (subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null)
                                {
                                    if (subclass.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
                                    {
                                        _singletonModules.Add(this.CreateInstance(subclass, this._configuration?.ServiceProvider));
                                    }
                                }
                            }
                            if (command.SubCommands.Any()) subGroupCommands.Add(command);
                            updateList.Add(payload);

                            //Accounts for lifespans
                            if (subclassinfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null)
                            {
                                if (subclassinfo.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
                                {
                                    _singletonModules.Add(this.CreateInstance(subclassinfo, this._configuration?.ServiceProvider));
                                }
                            }
                        }

                        //Handles methods and context menus, only if the module isn't a group itself
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() == null)
                        {
                            //Slash commands (again, similar to the one for groups)
                            var methods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            foreach (var method in methods)
                            {
                                var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

                                var parameters = method.GetParameters();
                                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();
                                var options = await this.ParseParameters(parameters, guildid);

                                commandMethods.Add(new CommandMethod { Method = method, Name = commandattribute.Name });

                                var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, options, commandattribute.DefaultPermission);
                                updateList.Add(payload);
                                commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));
                            }

                            //Context Menus
                            var contextMethods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);
                            foreach (var contextMethod in contextMethods)
                            {
                                var contextAttribute = contextMethod.GetCustomAttribute<ContextMenuAttribute>();
                                var command = new DiscordApplicationCommand(contextAttribute.Name, null, type: contextAttribute.Type, default_permission: contextAttribute.DefaultPermission);

                                var parameters = contextMethod.GetParameters();
                                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(ContextMenuContext)))
                                    throw new ArgumentException($"The first argument must be a ContextMenuContext!");
                                if (parameters.Length > 1)
                                    throw new ArgumentException($"A context menu cannot have parameters!");

                                contextMenuCommands.Add(new ContextMenuCommand { Method = contextMethod, Name = contextAttribute.Name });

                                updateList.Add(command);
                                commandTypeSources.Add(new KeyValuePair<Type, Type>(type, type));
                            }

                            //Accounts for lifespans
                            if (module.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null)
                            {
                                if (module.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
                                {
                                    _singletonModules.Add(this.CreateInstance(module, this._configuration?.ServiceProvider));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //This isn't really much more descriptive but I added a separate case for it anyway
                        if (ex is BadRequestException brex)
                            this.Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.JsonMessage}");
                        else
                            this.Client.Logger.LogCritical(ex, $"There was an error registering application commands");
                        _errored = true;
                    }
                }
                if (!_errored)
                {
                    try
                    {
                        async Task UpdateCommandPermission(ulong commandId, string commandName, Type commandDeclaringType, Type commandRootType)
                        {
                            if (guildid == null)
                            {
                                //throw new NotImplementedException("You can't set global permissions till yet. See https://discord.com/developers/docs/interactions/application-commands#permissions");
                            }
                            else
                            {
                                var ctx = new ApplicationCommandsPermissionContext(commandDeclaringType, commandName);
                                var conf = types.First(t => t.Type == commandRootType);
                                conf.Setup?.Invoke(ctx);

                                if (ctx.Permissions.Count == 0)
                                    return;

                                await this.Client.OverwriteGuildApplicationCommandPermissionsAsync(guildid.Value, commandId, ctx.Permissions);
                            }
                        }

                        async Task UpdateCommandPermissionGroup(GroupCommand groupCommand)
                        {
                            foreach (var com in groupCommand.Methods)
                            {
                                var source = commandTypeSources.FirstOrDefault(f => f.Key == com.Value.DeclaringType);

                                await UpdateCommandPermission(groupCommand.CommandId, com.Key, source.Key, source.Value);
                            }
                        }

                        var commands = guildid == null
                            ? await this.Client.BulkOverwriteGlobalApplicationCommandsAsync(updateList)
                            : (IEnumerable<DiscordApplicationCommand>)await this.Client.BulkOverwriteGuildApplicationCommandsAsync(guildid.Value, updateList);

                        //Creates a guild command if a guild id is specified, otherwise global
                        //Checks against the ids and adds them to the command method lists
                        foreach (var command in commands)
                        {
                            if (commandMethods.Any(x => x.Name == command.Name))
                            {
                                var com = commandMethods.First(x => x.Name == command.Name);
                                com.CommandId = command.Id;

                                var source = commandTypeSources.FirstOrDefault(f => f.Key == com.Method.DeclaringType);
                                await UpdateCommandPermission(command.Id, com.Name, source.Value, source.Key);
                            }

                            else if (groupCommands.Any(x => x.Name == command.Name))
                            {
                                var com = groupCommands.First(x => x.Name == command.Name);
                                com.CommandId = command.Id;

                                await UpdateCommandPermissionGroup(com);
                            }

                            else if (subGroupCommands.Any(x => x.Name == command.Name))
                            {
                                var com = subGroupCommands.First(x => x.Name == command.Name);
                                com.CommandId = command.Id;

                                foreach (var groupComs in com.SubCommands)
                                    await UpdateCommandPermissionGroup(groupComs);
                            }

                            else if (contextMenuCommands.Any(x => x.Name == command.Name))
                            {
                                var com = contextMenuCommands.First(x => x.Name == command.Name);
                                com.CommandId = command.Id;

                                var source = commandTypeSources.First(f => f.Key == com.Method.DeclaringType);
                                await UpdateCommandPermission(command.Id, com.Name, source.Value, source.Key);
                            }
                        }

                        //Adds to the global lists finally
                        _commandMethods.AddRange(commandMethods);
                        _groupCommands.AddRange(groupCommands);
                        _subGroupCommands.AddRange(subGroupCommands);
                        _contextMenuCommands.AddRange(contextMenuCommands);

                        _registeredCommands.Add(new KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>(guildid, commands.ToList()));

                        foreach (var command in commandMethods)
                        {
                            var app = types.First(t => t.Type == command.Method.DeclaringType);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is BadRequestException brex)
                            this.Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.JsonMessage}");
                        else
                            this.Client.Logger.LogCritical(ex, $"There was an error registering application commands");
                        _errored = true;
                    }
                }
            });
        }

        /// <summary>
        /// Interaction handler.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="e">The event args.</param>
        private Task InteractionHandler(DiscordClient client, InteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Interaction.Type == InteractionType.ApplicationCommand)
                {
                    //Creates the context
                    var context = new InteractionContext
                    {
                        Interaction = e.Interaction,
                        Channel = e.Interaction.Channel,
                        Guild = e.Interaction.Guild,
                        User = e.Interaction.User,
                        Client = client,
                        ApplicationCommandsExtension = this,
                        CommandName = e.Interaction.Data.Name,
                        InteractionId = e.Interaction.Id,
                        Token = e.Interaction.Token,
                        Services = this._configuration?.ServiceProvider,
                        ResolvedUserMentions = e.Interaction.Data.Resolved?.Users?.Values.ToList(),
                        ResolvedRoleMentions = e.Interaction.Data.Resolved?.Roles?.Values.ToList(),
                        ResolvedChannelMentions = e.Interaction.Data.Resolved?.Channels?.Values.ToList(),
                        Type = ApplicationCommandType.ChatInput
                    };

                    try
                    {
                        if (_errored)
                            throw new InvalidOperationException("Slash commands failed to register properly on startup.");

                        var methods = _commandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var groups = _groupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var subgroups = _subGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                        if (!methods.Any() && !groups.Any() && !subgroups.Any())
                            throw new InvalidOperationException("A slash command was executed, but no command was registered for it.");

                        if (methods.Any())
                        {
                            var method = methods.First().Method;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options);

                            await this.RunCommandAsync(context, method, args);
                        }
                        else if (groups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options);

                            await this.RunCommandAsync(context, method, args);
                        }
                        else if (subgroups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);

                            var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options.First().Options);

                            await this.RunCommandAsync(context, method, args);
                        }

                        await this._slashExecuted.InvokeAsync(this, new SlashCommandExecutedEventArgs(this.Client.ServiceProvider) { Context = context });
                    }
                    catch (Exception ex)
                    {
                        await this._slashError.InvokeAsync(this, new SlashCommandErrorEventArgs(this.Client.ServiceProvider) { Context = context, Exception = ex });
                    }
                }
                else if (e.Interaction.Type == InteractionType.AutoComplete)
                {
                    if (_errored)
                        throw new InvalidOperationException("Slash commands failed to register properly on startup.");

                    var methods = _commandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
                    var groups = _groupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                    var subgroups = _subGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                    if (!methods.Any() && !groups.Any() && !subgroups.Any())
                        throw new InvalidOperationException("An autocomplete interaction was created, but no command was registered for it.");

                    try
                    {
                        if (methods.Any())
                        {
                            var focusedOption = e.Interaction.Data.Options.First(o => o.Focused);
                            var method = methods.First().Method;

                            var option = method.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                            var provider = option.GetCustomAttribute<AutocompleteAttribute>().ProviderType;
                            var providerMethod = provider.GetMethod(nameof(IAutocompleteProvider.Provider));
                            var providerInstance = Activator.CreateInstance(provider);

                            var context = new AutocompleteContext
                            {
                                Interaction = e.Interaction,
                                Client = this.Client,
                                Services = this._configuration?.ServiceProvider,
                                ApplicationCommandsExtension = this,
                                Guild = e.Interaction.Guild,
                                Channel = e.Interaction.Channel,
                                User = e.Interaction.User,
                                Options = e.Interaction.Data.Options.ToList(),
                                FocusedOption = focusedOption
                            };

                            var choices = await (Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>) providerMethod.Invoke(providerInstance, new[] { context });
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
                        }
                        else if (groups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var group = groups.First().Methods.First(x => x.Key == command.Name).Value;

                            var focusedOption = command.Options.First(o => o.Focused);
                            var option = group.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
                            var provider = option.GetCustomAttribute<AutocompleteAttribute>().ProviderType;
                            var providerMethod = provider.GetMethod(nameof(IAutocompleteProvider.Provider));
                            var providerInstance = Activator.CreateInstance(provider);

                            var context = new AutocompleteContext
                            {
                                Interaction = e.Interaction,
                                Services = this._configuration?.ServiceProvider,
                                ApplicationCommandsExtension = this,
                                Guild = e.Interaction.Guild,
                                Channel = e.Interaction.Channel,
                                User = e.Interaction.User,
                                Options = command.Options.ToList(),
                                FocusedOption = focusedOption
                            };

                            var choices = await (Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>) providerMethod.Invoke(providerInstance, new[] { context });
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
                        }
                        /*else if (subgroups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var method = methods.First().Method;
                            var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);

                            var focusedOption = command.Options.First(x => x.Name == group.Name).Options.First(o => o.Focused);
                            this.Client.Logger.LogDebug("SUBGROUP::" + focusedOption.Name + ": " + focusedOption.RawValue);

                            var option = group.Methods.First(p => p.Value.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name).Value;
                            var provider = option.GetCustomAttribute<AutocompleteAttribute>().ProviderType;
                            var providerMethod = provider.GetMethod(nameof(IAutocompleteProvider.Provider));
                            var providerInstance = Activator.CreateInstance(provider);

                            var context = new AutocompleteContext
                            {
                                Interaction = e.Interaction,
                                Services = this._configuration?.Services,
                                ApplicationCommandsExtension = this,
                                Guild = e.Interaction.Guild,
                                Channel = e.Interaction.Channel,
                                User = e.Interaction.User,
                                Options = command.Options.First(x => x.Name == group.Name).Options.ToList(),
                                FocusedOption = focusedOption
                            };

                            var choices = await (Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>) providerMethod.Invoke(providerInstance, new[] { context });
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
                        }*/
                    }
                    catch (Exception ex)
                    {
                        this.Client.Logger.LogError(ex, "Error in autocomplete interaction");
                    }
                }
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="e">The event args.</param>
        private Task ContextMenuHandler(DiscordClient client, ContextMenuInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                //Creates the context
                var context = new ContextMenuContext
                {
                    Interaction = e.Interaction,
                    Channel = e.Interaction.Channel,
                    Client = client,
                    Services = this._configuration?.ServiceProvider,
                    CommandName = e.Interaction.Data.Name,
                    ApplicationCommandsExtension = this,
                    Guild = e.Interaction.Guild,
                    InteractionId = e.Interaction.Id,
                    User = e.Interaction.User,
                    Token = e.Interaction.Token,
                    TargetUser = e.TargetUser,
                    TargetMessage = e.TargetMessage,
                    Type = e.Type
                };

                try
                {
                    if (_errored)
                        throw new InvalidOperationException("Context menus failed to register properly on startup.");

                    //Gets the method for the command
                    var method = _contextMenuCommands.FirstOrDefault(x => x.CommandId == e.Interaction.Data.Id);

                    if (method == null)
                        throw new InvalidOperationException("A context menu was executed, but no command was registered for it.");

                    await this.RunCommandAsync(context, method.Method, new[] { context });

                    await this._contextMenuExecuted.InvokeAsync(this, new ContextMenuExecutedEventArgs(this.Client.ServiceProvider) { Context = context });
                }
                catch (Exception ex)
                {
                    await this._contextMenuErrored.InvokeAsync(this, new ContextMenuErrorEventArgs(this.Client.ServiceProvider) { Context = context, Exception = ex });
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs a command.
        /// </summary>
        /// <param name="context">The base context.</param>
        /// <param name="method">The method info.</param>
        /// <param name="args">The arguments.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        internal async Task RunCommandAsync(BaseContext context, MethodInfo method, IEnumerable<object> args)
        {
            object classInstance;

            //Accounts for lifespans
            var moduleLifespan = (method.DeclaringType.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null ? method.DeclaringType.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>()?.Lifespan : ApplicationCommandModuleLifespan.Transient) ?? ApplicationCommandModuleLifespan.Transient;
            switch (moduleLifespan)
            {
                case ApplicationCommandModuleLifespan.Scoped:
                    //Accounts for static methods and adds DI
                    classInstance = method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.ServiceProvider.CreateScope().ServiceProvider, method.DeclaringType) : this.CreateInstance(method.DeclaringType, this._configuration?.ServiceProvider.CreateScope().ServiceProvider);
                    break;

                case ApplicationCommandModuleLifespan.Transient:
                    //Accounts for static methods and adds DI
                    classInstance = method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.ServiceProvider, method.DeclaringType) : this.CreateInstance(method.DeclaringType, this._configuration?.ServiceProvider);
                    break;

                //If singleton, gets it from the singleton list
                case ApplicationCommandModuleLifespan.Singleton:
                    classInstance = _singletonModules.First(x => ReferenceEquals(x.GetType(), method.DeclaringType));
                    break;

                default:
                    throw new Exception($"An unknown {nameof(ApplicationCommandModuleLifespanAttribute)} scope was specified on command {context.CommandName}");
            }

            ApplicationCommandsModule module = null;
            if (classInstance is ApplicationCommandsModule mod)
                module = mod;

            // Slash commands
            if (context is InteractionContext slashContext)
            {
                await this.RunPreexecutionChecksAsync(method, slashContext);

                var shouldExecute = await (module?.BeforeSlashExecutionAsync(slashContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classInstance, args.ToArray());

                    await (module?.AfterSlashExecutionAsync(slashContext) ?? Task.CompletedTask);
                }
            }
            // Context menus
            if (context is ContextMenuContext contextMenuContext)
            {
                await this.RunPreexecutionChecksAsync(method, contextMenuContext);

                var shouldExecute = await (module?.BeforeContextMenuExecutionAsync(contextMenuContext) ?? Task.FromResult(true));

                if (shouldExecute)
                {
                    await (Task)method.Invoke(classInstance, args.ToArray());

                    await (module?.AfterContextMenuExecutionAsync(contextMenuContext) ?? Task.CompletedTask);
                }
            }
        }

        /// <summary>
        /// Property injection copied over from CommandsNext
        /// </summary>
        /// <param name="t">The type.</param>
        /// <param name="services">The services.</param>
        internal object CreateInstance(Type t, IServiceProvider services)
        {
            var ti = t.GetTypeInfo();
            var constructors = ti.DeclaredConstructors
                .Where(xci => xci.IsPublic)
                .ToArray();

            if (constructors.Length != 1)
                throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");

            var constructor = constructors[0];
            var constructorArgs = constructor.GetParameters();
            var args = new object[constructorArgs.Length];

            if (constructorArgs.Length != 0 && services == null)
                throw new InvalidOperationException("Dependency collection needs to be specified for parameterized constructors.");

            // inject via constructor
            if (constructorArgs.Length != 0)
                for (var i = 0; i < args.Length; i++)
                    args[i] = services.GetRequiredService(constructorArgs[i].ParameterType);

            var moduleInstance = Activator.CreateInstance(t, args);

            // inject into properties
            var props = t.GetRuntimeProperties().Where(xp => xp.CanWrite && xp.SetMethod != null && !xp.SetMethod.IsStatic && xp.SetMethod.IsPublic);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = services.GetService(prop.PropertyType);
                if (service == null)
                    continue;

                prop.SetValue(moduleInstance, service);
            }

            // inject into fields
            var fields = t.GetRuntimeFields().Where(xf => !xf.IsInitOnly && !xf.IsStatic && xf.IsPublic);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = services.GetService(field.FieldType);
                if (service == null)
                    continue;

                field.SetValue(moduleInstance, service);
            }

            return moduleInstance;
        }

        /// <summary>
        /// Resolves the slash command parameters.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        /// <param name="context">The interaction context.</param>
        /// <param name="method">The method info.</param>
        /// <param name="options">The options.</param>
        private async Task<List<object>> ResolveInteractionCommandParameters(InteractionCreateEventArgs e, InteractionContext context, MethodInfo method, IEnumerable<DiscordInteractionDataOption> options)
        {
            var args = new List<object> { context };
            var parameters = method.GetParameters().Skip(1);

            for (var i = 0; i < parameters.Count(); i++)
            {
                var parameter = parameters.ElementAt(i);

                //Accounts for optional arguments without values given
                if (parameter.IsOptional && (options == null ||
                                             (!options?.Any(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower()) ?? true)))
                    args.Add(parameter.DefaultValue);
                else
                {
                    var option = options.Single(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower());

                    //Checks the type and casts/references resolved and adds the value to the list
                    //This can probably reference the slash command's type property that didn't exist when I wrote this and it could use a cleaner switch instead, but if it works it works
                    if (parameter.ParameterType == typeof(string))
                        args.Add(option.Value.ToString());
                    else if (parameter.ParameterType.IsEnum)
                        args.Add(Enum.Parse(parameter.ParameterType, (string)option.Value));
                    else if (parameter.ParameterType == typeof(long) || parameter.ParameterType == typeof(long?))
                        args.Add((long?)option.Value);
                    else if (parameter.ParameterType == typeof(bool) || parameter.ParameterType == typeof(bool?))
                        args.Add((bool?)option.Value);
                    else if (parameter.ParameterType == typeof(double) || parameter.ParameterType == typeof(double?))
                        args.Add((double?)option.Value);
                    else if (parameter.ParameterType == typeof(DiscordUser))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Members != null &&
                            e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                            args.Add(member);
                        else if (e.Interaction.Data.Resolved.Users != null &&
                                 e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                            args.Add(user);
                        else
                            args.Add(await this.Client.GetUserAsync((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(DiscordChannel))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Channels != null &&
                            e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
                            args.Add(channel);
                        else
                            args.Add(e.Interaction.Guild.GetChannel((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(DiscordRole))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Roles != null &&
                            e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                            args.Add(role);
                        else
                            args.Add(e.Interaction.Guild.GetRole((ulong)option.Value));
                    }
                    else if (parameter.ParameterType == typeof(SnowflakeObject))
                    {
                        //Checks through resolved
                        if (e.Interaction.Data.Resolved.Roles != null && e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                            args.Add(role);
                        else if (e.Interaction.Data.Resolved.Members != null && e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                            args.Add(member);
                        else if (e.Interaction.Data.Resolved.Users != null && e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                            args.Add(user);
                        else
                            throw new ArgumentException("Error resolving mentionable option.");
                    }
                    else
                        throw new ArgumentException($"Error resolving interaction.");
                }
            }

            return args;
        }

        /// <summary>
        /// Runs the preexecution checks.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="context">The basecontext.</param>
        private async Task RunPreexecutionChecksAsync(MethodInfo method, BaseContext context)
        {
            if (context is InteractionContext ctx)
            {
                //Gets all attributes from parent classes as well and stuff
                var attributes = new List<SlashCheckBaseAttribute>();
                attributes.AddRange(method.GetCustomAttributes<SlashCheckBaseAttribute>(true));
                attributes.AddRange(method.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
                if (method.DeclaringType.DeclaringType != null)
                {
                    attributes.AddRange(method.DeclaringType.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
                    if (method.DeclaringType.DeclaringType.DeclaringType != null)
                    {
                        attributes.AddRange(method.DeclaringType.DeclaringType.DeclaringType.GetCustomAttributes<SlashCheckBaseAttribute>());
                    }
                }

                var dict = new Dictionary<SlashCheckBaseAttribute, bool>();
                foreach (var att in attributes)
                {
                    //Runs the check and adds the result to a list
                    var result = await att.ExecuteChecksAsync(ctx);
                    dict.Add(att, result);
                }

                //Checks if any failed, and throws an exception
                if (dict.Any(x => x.Value == false))
                    throw new SlashExecutionChecksFailedException { FailedChecks = dict.Where(x => x.Value == false).Select(x => x.Key).ToList() };
            }
            if (context is ContextMenuContext CMctx)
            {
                var attributes = new List<ContextMenuCheckBaseAttribute>();
                attributes.AddRange(method.GetCustomAttributes<ContextMenuCheckBaseAttribute>(true));
                attributes.AddRange(method.DeclaringType.GetCustomAttributes<ContextMenuCheckBaseAttribute>());
                if (method.DeclaringType.DeclaringType != null)
                {
                    attributes.AddRange(method.DeclaringType.DeclaringType.GetCustomAttributes<ContextMenuCheckBaseAttribute>());
                    if (method.DeclaringType.DeclaringType.DeclaringType != null)
                    {
                        attributes.AddRange(method.DeclaringType.DeclaringType.DeclaringType.GetCustomAttributes<ContextMenuCheckBaseAttribute>());
                    }
                }

                var dict = new Dictionary<ContextMenuCheckBaseAttribute, bool>();
                foreach (var att in attributes)
                {
                    //Runs the check and adds the result to a list
                    var result = await att.ExecuteChecksAsync(CMctx);
                    dict.Add(att, result);
                }

                //Checks if any failed, and throws an exception
                if (dict.Any(x => x.Value == false))
                    throw new ContextMenuExecutionChecksFailedException { FailedChecks = dict.Where(x => x.Value == false).Select(x => x.Key).ToList() };
            }
        }

        /// <summary>
        /// Gets the choice attributes from choice provider.
        /// </summary>
        /// <param name="customAttributes">The custom attributes.</param>
        /// <param name="guildId"></param>
        private async Task<List<DiscordApplicationCommandOptionChoice>> GetChoiceAttributesFromProvider(IEnumerable<ChoiceProviderAttribute> customAttributes, ulong? guildId = null)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            foreach (var choiceProviderAttribute in customAttributes)
            {
                var method = choiceProviderAttribute.ProviderType.GetMethod(nameof(IChoiceProvider.Provider));

                if (method == null)
                    throw new ArgumentException("ChoiceProviders must inherit from IChoiceProvider.");
                else
                {
                    var instance = Activator.CreateInstance(choiceProviderAttribute.ProviderType);

                    // Abstract class offers more properties that can be set
                    if (choiceProviderAttribute.ProviderType.IsSubclassOf(typeof(ChoiceProvider)))
                    {
                        choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.GuildId))
                            ?.SetValue(instance, guildId);

                        choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.Services))
                            ?.SetValue(instance, _configuration.ServiceProvider);
                    }

                    //Gets the choices from the method
                    var result = await (Task<IEnumerable<DiscordApplicationCommandOptionChoice>>)method.Invoke(instance, null);

                    if (result.Any())
                    {
                        choices.AddRange(result);
                    }
                }
            }

            return choices;
        }
        /// <summary>
        /// Gets the choice attributes from enum parameter.
        /// </summary>
        /// <param name="enumParam">The enum parameter.</param>
        private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromEnumParameter(Type enumParam)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            foreach (Enum enumValue in Enum.GetValues(enumParam))
            {
                choices.Add(new DiscordApplicationCommandOptionChoice(enumValue.GetName(), enumValue.ToString()));
            }
            return choices;
        }

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        /// <param name="type">The type.</param>
        private ApplicationCommandOptionType GetParameterType(Type type)
        {
            var parametertype = type == typeof(string)
                ? ApplicationCommandOptionType.String
                : type == typeof(long) || type == typeof(long?)
                ? ApplicationCommandOptionType.Integer
                : type == typeof(bool) || type == typeof(bool?)
                ? ApplicationCommandOptionType.Boolean
                : type == typeof(double) || type == typeof(double?)
                ? ApplicationCommandOptionType.Number
                : type == typeof(DiscordChannel)
                ? ApplicationCommandOptionType.Channel
                : type == typeof(DiscordUser)
                ? ApplicationCommandOptionType.User
                : type == typeof(DiscordRole)
                ? ApplicationCommandOptionType.Role
                : type == typeof(SnowflakeObject)
                ? ApplicationCommandOptionType.Mentionable
                : type == typeof(DiscordAttachment)
                ? ApplicationCommandOptionType.Attachment
                : type.IsEnum
                ? ApplicationCommandOptionType.String
                : throw new ArgumentException("Cannot convert type! Argument types must be string, long, bool, double, DiscordChannel, DiscordUser, DiscordRole, SnowflakeObject, DiscordAttachment or an Enum.");
            return parametertype;
        }

        /// <summary>
        /// Gets the choice attributes from parameter.
        /// </summary>
        /// <param name="choiceattributes">The choice attributes.</param>
        private List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromParameter(IEnumerable<ChoiceAttribute> choiceattributes)
        {
            return !choiceattributes.Any()
                ? null
                : choiceattributes.Select(att => new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToList();
        }

        /// <summary>
        /// Parses the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="guildId">The guild id.</param>
        /// <returns>A Task.</returns>
        private async Task<List<DiscordApplicationCommandOption>> ParseParameters(ParameterInfo[] parameters, ulong? guildId)
        {
            var options = new List<DiscordApplicationCommandOption>();
            foreach (var parameter in parameters)
            {
                //Gets the attribute
                var optionattribute = parameter.GetCustomAttribute<OptionAttribute>();
                if (optionattribute == null)
                    throw new ArgumentException("Arguments must have the Option attribute!");

                var minimumValue = parameter.GetCustomAttribute<MinimumAttribute>()?.Value ?? null;
                var maximumValue = parameter.GetCustomAttribute<MaximumAttribute>()?.Value ?? null;


                var autocompleteAttribute = parameter.GetCustomAttribute<AutocompleteAttribute>();
                if (optionattribute.Autocomplete && autocompleteAttribute == null)
                    throw new ArgumentException("Autocomplete options must have the Autocomplete attribute!");
                if (!optionattribute.Autocomplete && autocompleteAttribute != null)
                    throw new ArgumentException("Setting an autocomplete provider requires the option to have autocomplete set to true!");

                //Sets the type
                var type = parameter.ParameterType;
                var parametertype = this.GetParameterType(type);

                //Handles choices
                //From attributes
                var choices = this.GetChoiceAttributesFromParameter(parameter.GetCustomAttributes<ChoiceAttribute>());
                //From enums
                if (parameter.ParameterType.IsEnum)
                {
                    choices = GetChoiceAttributesFromEnumParameter(parameter.ParameterType);
                }
                //From choice provider
                var choiceProviders = parameter.GetCustomAttributes<ChoiceProviderAttribute>();
                if (choiceProviders.Any())
                {
                    choices = await this.GetChoiceAttributesFromProvider(choiceProviders, guildId);
                }

                var channelTypes = parameter.GetCustomAttribute<ChannelTypesAttribute>()?.ChannelTypes ?? null;

                options.Add(new DiscordApplicationCommandOption(optionattribute.Name, optionattribute.Description, parametertype, !parameter.IsOptional, choices, null, channelTypes, optionattribute.Autocomplete, minimumValue, maximumValue));
            }

            return options;
        }

        /// <summary>
        /// <para>Refreshes your commands, used for refreshing choice providers or applying commands registered after the ready event on the discord client.
        /// Should only be run on the slash command extension linked to shard 0 if sharding.</para>
        /// <para>Not recommended and should be avoided since it can make slash commands be unresponsive for a while.</para>
        /// </summary>
        public async Task RefreshCommandsAsync()
        {
            _commandMethods.Clear();
            _groupCommands.Clear();
            _subGroupCommands.Clear();
            _registeredCommands.Clear();
            _contextMenuCommands.Clear();

            await this.Update();
        }

        /// <summary>
        /// Fires when the execution of a slash command fails.
        /// </summary>
        public event AsyncEventHandler<ApplicationCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
        {
            add { this._slashError.Register(value); }
            remove { this._slashError.Unregister(value); }
        }
        private AsyncEvent<ApplicationCommandsExtension, SlashCommandErrorEventArgs> _slashError;

        /// <summary>
        /// Fires when the execution of a slash command is successful.
        /// </summary>
        public event AsyncEventHandler<ApplicationCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
        {
            add { this._slashExecuted.Register(value); }
            remove { this._slashExecuted.Unregister(value); }
        }
        private AsyncEvent<ApplicationCommandsExtension, SlashCommandExecutedEventArgs> _slashExecuted;

        /// <summary>
        /// Fires when the execution of a context menu fails.
        /// </summary>
        public event AsyncEventHandler<ApplicationCommandsExtension, ContextMenuErrorEventArgs> ContextMenuErrored
        {
            add { this._contextMenuErrored.Register(value); }
            remove { this._contextMenuErrored.Unregister(value); }
        }
        private AsyncEvent<ApplicationCommandsExtension, ContextMenuErrorEventArgs> _contextMenuErrored;

        /// <summary>
        /// Fire when the execution of a context menu is successful.
        /// </summary>
        public event AsyncEventHandler<ApplicationCommandsExtension, ContextMenuExecutedEventArgs> ContextMenuExecuted
        {
            add { this._contextMenuExecuted.Register(value); }
            remove { this._contextMenuExecuted.Unregister(value); }
        }
        private AsyncEvent<ApplicationCommandsExtension, ContextMenuExecutedEventArgs> _contextMenuExecuted;
    }

    /// <summary>
    /// Holds configuration data for setting up an application command.
    /// </summary>
    internal class ApplicationCommandsModuleConfiguration
    {
        /// <summary>
        /// The type of the command module.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The permission setup.
        /// </summary>
        public Action<ApplicationCommandsPermissionContext> Setup { get; }

        /// <summary>
        /// Creates a new command configuration.
        /// </summary>
        /// <param name="type">The type of the command module.</param>
        /// <param name="setup">The permission setup callback.</param>
        public ApplicationCommandsModuleConfiguration(Type type, Action<ApplicationCommandsPermissionContext> setup = null)
        {
            this.Type = type;
            this.Setup = setup;
        }
    }

    /// <summary>
    /// Links a command to its original command module.
    /// </summary>
    internal class ApplicationCommandSourceLink
    {
        /// <summary>
        /// The command.
        /// </summary>
        public DiscordApplicationCommand ApplicationCommand { get; set; }

        /// <summary>
        /// The base/root module the command is contained in.
        /// </summary>
        public Type RootCommandContainerType { get; set; }

        /// <summary>
        /// The direct group the command is contained in.
        /// </summary>
        public Type CommandContainerType { get; set; }
    }

    /// <summary>
    /// The command method.
    /// </summary>
    internal class CommandMethod
    {
        /// <summary>
        /// Gets or sets the command id.
        /// </summary>
        public ulong CommandId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        public MethodInfo Method { get; set; }
    }

    /// <summary>
    /// The group command.
    /// </summary>
    internal class GroupCommand
    {
        /// <summary>
        /// Gets or sets the command id.
        /// </summary>
        public ulong CommandId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the methods.
        /// </summary>
        public List<KeyValuePair<string, MethodInfo>> Methods { get; set; } = null;
    }

    /// <summary>
    /// The sub group command.
    /// </summary>
    internal class SubGroupCommand
    {
        /// <summary>
        /// Gets or sets the command id.
        /// </summary>
        public ulong CommandId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sub commands.
        /// </summary>
        public List<GroupCommand> SubCommands { get; set; } = new List<GroupCommand>();
    }

    /// <summary>
    /// The context menu command.
    /// </summary>
    internal class ContextMenuCommand
    {
        /// <summary>
        /// Gets or sets the command id.
        /// </summary>
        public ulong CommandId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        public MethodInfo Method { get; set; }
    }
}
