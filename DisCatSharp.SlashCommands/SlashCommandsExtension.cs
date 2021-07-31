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
using DisCatSharp.SlashCommands.EventArgs;
using DisCatSharp.Exceptions;

namespace DisCatSharp.SlashCommands
{
    /// <summary>
    /// A class that handles slash commands for a client.
    /// </summary>
    public sealed class SlashCommandsExtension : BaseExtension
    {
        /// <summary>
        /// Gets or sets the command methods.
        /// </summary>
        private static List<CommandMethod> CommandMethods { get; set; } = new List<CommandMethod>();
        /// <summary>
        /// Gets or sets the group commands.
        /// </summary>
        private static List<GroupCommand> GroupCommands { get; set; } = new List<GroupCommand>();
        /// <summary>
        /// Gets or sets the sub group commands.
        /// </summary>
        private static List<SubGroupCommand> SubGroupCommands { get; set; } = new List<SubGroupCommand>();

        /// <summary>
        /// Gets or sets the singleton modules.
        /// </summary>
        private static List<object> SingletonModules { get; set; } = new List<object>();

        /// <summary>
        /// Gets or sets the update list.
        /// </summary>
        private List<KeyValuePair<ulong?, Type>> UpdateList { get; set; } = new List<KeyValuePair<ulong?, Type>>();
        private readonly SlashCommandsConfiguration _configuration;
        /// <summary>
        /// Gets or sets a value indicating whether errored.
        /// </summary>
        private static bool Errored { get; set; } = false;

        /// <summary>
        /// Gets a list of registered commands. The key is the guild id (null if global).
        /// </summary>
        public IReadOnlyList<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> RegisteredCommands
            => _registeredCommands;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
        private static List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> _registeredCommands = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SlashCommandsExtension"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal SlashCommandsExtension(SlashCommandsConfiguration configuration)
        {
            _configuration = configuration;
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

            _error = new AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, null);
            _executed = new AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, null);

            this.Client.Ready += this.Update;
            this.Client.InteractionCreated += this.InteractionHandler;
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <typeparam name="T">The command class to register.</typeparam>
        /// <param name="guild_id">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands<T>(ulong? guild_id = null) where T : SlashCommandModule
        {
            if (this.Client.ShardId == 0)
                this.UpdateList.Add(new KeyValuePair<ulong?, Type>(guild_id, typeof(T)));
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the command class to register.</param>
        /// <param name="guild_id">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands(Type type, ulong? guild_id = null)
        {
            if (!typeof(SlashCommandModule).IsAssignableFrom(type))
                throw new ArgumentException("Command classes have to inherit from SlashCommandModule", nameof(type));
            if (this.Client.ShardId == 0)
                this.UpdateList.Add(new KeyValuePair<ulong?, Type>(guild_id, type));
        }

        /// <summary>
        /// Updates the slashcommands.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="e">The event args.</param>
        internal Task Update(DiscordClient client, ReadyEventArgs e)
            => this.Update();

        /// <summary>
        /// Updates the slash commands.
        /// </summary>
        internal Task Update()
        {
            if (this.Client.ShardId == 0)
            {
                foreach (var key in this.UpdateList.Select(x => x.Key).Distinct())
                {
                    this.RegisterCommands(this.UpdateList.Where(x => x.Key == key).Select(x => x.Value), key);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Registers the commands.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="guild_id">The guild id.</param>
        private void RegisterCommands(IEnumerable<Type> types, ulong? guild_id)
        {
            var commandMethods = new List<CommandMethod>();
            var groupCommands = new List<GroupCommand>();
            var subGroupCommands = new List<SubGroupCommand>();
            var updateList = new List<DiscordApplicationCommand>();

            _ = Task.Run(async () =>
            {
                foreach (var type in types)
                {
                    try
                    {
                        var module = type.GetTypeInfo();

                        var classes = new List<TypeInfo>();

                        //Add module to classes list only if it's a group
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() != null)
                        {
                            classes.Add(module);
                        }
                        else
                        {
                            classes = module.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
                        }

                        //Handles groups
                        foreach (var subclassinfo in classes)
                        {
                            var groupatt = subclassinfo.GetCustomAttribute<SlashCommandGroupAttribute>();
                            var submethods = subclassinfo.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            var subclasses = subclassinfo.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
                            if (subclasses.Any() && submethods.Any())
                            {
                                throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
                            }
                            var payload = new DiscordApplicationCommand(groupatt.Name, groupatt.Description, default_permission: groupatt.DefaultPermission);

                            var commandmethods = new List<KeyValuePair<string, MethodInfo>>();
                            foreach (var submethod in submethods)
                            {
                                var commandattribute = submethod.GetCustomAttribute<SlashCommandAttribute>();

                                var parameters = submethod.GetParameters();
                                if (!ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();

                                var options = await this.ParseParameters(parameters);

                                var subpayload = new DiscordApplicationCommandOption(commandattribute.Name, commandattribute.Description, ApplicationCommandOptionType.SubCommand, null, null, options);

                                commandmethods.Add(new KeyValuePair<string, MethodInfo>(commandattribute.Name, submethod));

                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission);

                                groupCommands.Add(new GroupCommand { Name = groupatt.Name, Methods = commandmethods });
                            }
                            var command = new SubGroupCommand { Name = groupatt.Name };
                            foreach (var subclass in subclasses)
                            {
                                var subgroupatt = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
                                var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                                var options = new List<DiscordApplicationCommandOption>();

                                var currentMethods = new List<KeyValuePair<string, MethodInfo>>();

                                foreach (var subsubmethod in subsubmethods)
                                {
                                    var suboptions = new List<DiscordApplicationCommandOption>();
                                    var commatt = subsubmethod.GetCustomAttribute<SlashCommandAttribute>();
                                    var parameters = subsubmethod.GetParameters();
                                    if (!ReferenceEquals(parameters.First().ParameterType, typeof(InteractionContext)))
                                        throw new ArgumentException($"The first argument must be an InteractionContext!");
                                    parameters = parameters.Skip(1).ToArray();
                                    suboptions = suboptions.Concat(await this.ParseParameters(parameters)).ToList();

                                    var subsubpayload = new DiscordApplicationCommandOption(commatt.Name, commatt.Description, ApplicationCommandOptionType.SubCommand, null, null, suboptions);
                                    options.Add(subsubpayload);
                                    commandmethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                                    currentMethods.Add(new KeyValuePair<string, MethodInfo>(commatt.Name, subsubmethod));
                                }

                                var subpayload = new DiscordApplicationCommandOption(subgroupatt.Name, subgroupatt.Description, ApplicationCommandOptionType.SubCommandGroup, null, null, options);
                                command.SubCommands.Add(new GroupCommand { Name = subgroupatt.Name, Methods = currentMethods });
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload }, payload.DefaultPermission);

                                if (subclass.GetCustomAttribute<SlashModuleLifespanAttribute>() != null)
                                {
                                    if (subclass.GetCustomAttribute<SlashModuleLifespanAttribute>().Lifespan == SlashModuleLifespan.Singleton)
                                    {
                                        SingletonModules.Add(this.CreateInstance(subclass, _configuration?.Services));
                                    }
                                }
                            }
                            if (command.SubCommands.Any()) subGroupCommands.Add(command);
                            updateList.Add(payload);


                            if (subclassinfo.GetCustomAttribute<SlashModuleLifespanAttribute>() != null)
                            {
                                if (subclassinfo.GetCustomAttribute<SlashModuleLifespanAttribute>().Lifespan == SlashModuleLifespan.Singleton)
                                {
                                    SingletonModules.Add(this.CreateInstance(subclassinfo, _configuration?.Services));
                                }
                            }
                        }

                        //Handles methods, only if the module isn't a group itself.
                        if (module.GetCustomAttribute<SlashCommandGroupAttribute>() == null)
                        {
                            var methods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                            foreach (var method in methods)
                            {
                                var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

                                var options = new List<DiscordApplicationCommandOption>();

                                var parameters = method.GetParameters();
                                if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
                                    throw new ArgumentException($"The first argument must be an InteractionContext!");
                                parameters = parameters.Skip(1).ToArray();
                                options = options.Concat(await this.ParseParameters(parameters)).ToList();

                                commandMethods.Add(new CommandMethod { Method = method, Name = commandattribute.Name });

                                var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, options, commandattribute.DefaultPermission);
                                updateList.Add(payload);
                            }

                            if (module.GetCustomAttribute<SlashModuleLifespanAttribute>() != null)
                            {
                                if (module.GetCustomAttribute<SlashModuleLifespanAttribute>().Lifespan == SlashModuleLifespan.Singleton)
                                {
                                    SingletonModules.Add(this.CreateInstance(module, _configuration?.Services));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is BadRequestException brex)
                            this.Client.Logger.LogCritical(brex, $"There was an error registering slash commands: {brex.JsonMessage}");
                        else
                            this.Client.Logger.LogCritical(ex, $"There was an error registering slash commands");
                        Errored = true;
                    }
                }
                if (!Errored)
                {
                    try
                    {
                        IEnumerable<DiscordApplicationCommand> commands;
                        if (guild_id == null)
                        {
                            commands = await this.Client.BulkOverwriteGlobalApplicationCommandsAsync(updateList);
                        }
                        else
                        {
                            commands = await this.Client.BulkOverwriteGuildApplicationCommandsAsync(guild_id.Value, updateList);
                        }
                        foreach (var command in commands)
                        {
                            if (commandMethods.Any(x => x.Name == command.Name))
                                commandMethods.First(x => x.Name == command.Name).CommandId = command.Id;

                            else if (groupCommands.Any(x => x.Name == command.Name))
                                groupCommands.First(x => x.Name == command.Name).CommandId = command.Id;

                            else if (subGroupCommands.Any(x => x.Name == command.Name))
                                subGroupCommands.First(x => x.Name == command.Name).CommandId = command.Id;
                        }
                        CommandMethods.AddRange(commandMethods);
                        GroupCommands.AddRange(groupCommands);
                        SubGroupCommands.AddRange(subGroupCommands);

                        _registeredCommands.Add(new KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>(guild_id, commands.ToList()));
                    }
                    catch (Exception ex)
                    {
                        if (ex is BadRequestException brex)
                            this.Client.Logger.LogCritical(brex, $"There was an error registering slash commands: {brex.JsonMessage}");
                        else
                            this.Client.Logger.LogCritical(ex, $"There was an error registering slash commands");
                        Errored = true;
                    }
                }
            });
        }

        /// <summary>
        /// Interactions the handler.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="e">The event args.</param>
        private Task InteractionHandler(DiscordClient client, InteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Interaction.Type == InteractionType.ApplicationCommand)
                {
                    InteractionContext context = new InteractionContext
                    {
                        Interaction = e.Interaction,
                        Channel = e.Interaction.Channel,
                        Guild = e.Interaction.Guild,
                        User = e.Interaction.User,
                        Client = client,
                        SlashCommandsExtension = this,
                        CommandName = e.Interaction.Data.Name,
                        InteractionId = e.Interaction.Id,
                        Token = e.Interaction.Token,
                        Services = _configuration?.Services
                    };

                    try
                    {
                        if (Errored)
                            throw new InvalidOperationException("Slash commands failed to register properly on startup.");
                        var methods = CommandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var groups = GroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
                        var subgroups = SubGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
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

                        await _executed.InvokeAsync(this, new SlashCommandExecutedEventArgs { Context = context });
                    }
                    catch (Exception ex)
                    {
                        await _error.InvokeAsync(this, new SlashCommandErrorEventArgs { Context = context, Exception = ex });
                    }
                }
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="method">The method.</param>
        /// <param name="args">The arguments.</param>
        internal async Task RunCommandAsync(InteractionContext context, MethodInfo method, IEnumerable<object> args)
        {
            var classinstance = method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>() != null && method.DeclaringType.GetCustomAttribute<SlashModuleLifespanAttribute>()?.Lifespan == SlashModuleLifespan.Singleton
                ? SingletonModules.First(x => ReferenceEquals(x.GetType(), method.DeclaringType))
                : method.IsStatic ? ActivatorUtilities.CreateInstance(_configuration?.Services, method.DeclaringType) : this.CreateInstance(method.DeclaringType, _configuration?.Services);
            SlashCommandModule module = null;
            if (classinstance is SlashCommandModule mod)
                module = mod;

            await this.RunPreexecutionChecksAsync(method, context);

            await (module?.BeforeExecutionAsync(context) ?? Task.CompletedTask);

            await (Task)method.Invoke(classinstance, args.ToArray());

            await (module?.AfterExecutionAsync(context) ?? Task.CompletedTask);
        }

        /// <summary>
        /// Creates the instance.
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
        /// Resolves the interaction command parameters.
        /// </summary>
        /// <param name="e">The event args.</param>
        /// <param name="context">The context.</param>
        /// <param name="method">The method.</param>
        /// <param name="options">The options.</param>
        /// <returns>A Task.</returns>
        private async Task<List<object>> ResolveInteractionCommandParameters(InteractionCreateEventArgs e, InteractionContext context, MethodInfo method, IEnumerable<DiscordInteractionDataOption> options)
        {
            var args = new List<object> { context };
            var parameters = method.GetParameters().Skip(1);

            for (int i = 0; i < parameters.Count(); i++)
            {
                var parameter = parameters.ElementAt(i);
                if (parameter.IsOptional && (options == null ||
                                             (!options?.Any(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower()) ?? true)))
                    args.Add(parameter.DefaultValue);
                else
                {
                    var option = options.Single(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower());

                    if (ReferenceEquals(parameter.ParameterType, typeof(string)))
                        args.Add(option.Value.ToString());
                    else if (parameter.ParameterType.IsEnum)
                        args.Add(Enum.Parse(parameter.ParameterType, (string)option.Value));
                    else if (ReferenceEquals(parameter.ParameterType, typeof(long)))
                        args.Add((long)option.Value);
                    else if (ReferenceEquals(parameter.ParameterType, typeof(bool)))
                        args.Add((bool)option.Value);
                    else if (ReferenceEquals(parameter.ParameterType, typeof(double)))
                        args.Add((double)option.Value);
                    else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordUser)))
                    {
                        if (e.Interaction.Data.Resolved.Members != null &&
                            e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                        {
                            args.Add(member);
                        }
                        else if (e.Interaction.Data.Resolved.Users != null &&
                                 e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                        {
                            args.Add(user);
                        }
                        else
                        {
                            args.Add(await this.Client.GetUserAsync((ulong)option.Value));
                        }
                    }
                    else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordChannel)))
                    {
                        if (e.Interaction.Data.Resolved.Channels != null &&
                            e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
                        {
                            args.Add(channel);
                        }
                        else
                        {
                            args.Add(e.Interaction.Guild.GetChannel((ulong)option.Value));
                        }
                    }
                    else if (ReferenceEquals(parameter.ParameterType, typeof(DiscordRole)))
                    {
                        if (e.Interaction.Data.Resolved.Roles != null &&
                            e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                        {
                            args.Add(role);
                        }
                        else
                        {
                            args.Add(e.Interaction.Guild.GetRole((ulong)option.Value));
                        }
                    }
                    else if (ReferenceEquals(parameter.ParameterType, typeof(SnowflakeObject)))
                    {
                        if (e.Interaction.Data.Resolved.Roles != null && e.Interaction.Data.Resolved.Roles.TryGetValue((ulong)option.Value, out var role))
                        {
                            args.Add(role);
                        }
                        else if (e.Interaction.Data.Resolved.Members != null && e.Interaction.Data.Resolved.Members.TryGetValue((ulong)option.Value, out var member))
                        {
                            args.Add(member);
                        }
                        else if (e.Interaction.Data.Resolved.Users != null && e.Interaction.Data.Resolved.Users.TryGetValue((ulong)option.Value, out var user))
                        {
                            args.Add(user);
                        }
                        else
                        {
                            throw new ArgumentException("Error resolving mentionable option.");
                        }
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
        /// <param name="method">The method.</param>
        /// <param name="ctx">The command context.</param>
        private async Task RunPreexecutionChecksAsync(MethodInfo method, InteractionContext ctx)
        {
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
                var result = await att.ExecuteChecksAsync(ctx);
                dict.Add(att, result);
            }

            if (dict.Any(x => x.Value == false))
                throw new SlashExecutionChecksFailedException(dict.Where(x => x.Value == false).Select(x => x.Key).ToList());
        }

        /// <summary>
        /// Gets the choice attributes from provider.
        /// </summary>
        /// <param name="customAttributes">The custom attributes.</param>
        private async Task<List<DiscordApplicationCommandOptionChoice>> GetChoiceAttributesFromProvider(IEnumerable<ChoiceProviderAttribute> customAttributes)
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
        /// <param name="enumParam">The enum param.</param>
        private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromEnumParameter(Type enumParam)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            foreach (Enum foo in Enum.GetValues(enumParam))
            {
                choices.Add(new DiscordApplicationCommandOptionChoice(foo.GetName(), foo.ToString()));
            }
            return choices;
        }

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        /// <param name="type">The type.</param>
        private ApplicationCommandOptionType GetParameterType(Type type)
        {
            ApplicationCommandOptionType parametertype;
            if (ReferenceEquals(type, typeof(string)))
                parametertype = ApplicationCommandOptionType.String;
            else if (ReferenceEquals(type, typeof(long)))
                parametertype = ApplicationCommandOptionType.Integer;
            else if (ReferenceEquals(type, typeof(bool)))
                parametertype = ApplicationCommandOptionType.Boolean;
            else if (ReferenceEquals(type, typeof(double)))
                parametertype = ApplicationCommandOptionType.Number;
            else if (ReferenceEquals(type, typeof(DiscordChannel)))
                parametertype = ApplicationCommandOptionType.Channel;
            else if (ReferenceEquals(type, typeof(DiscordUser)))
                parametertype = ApplicationCommandOptionType.User;
            else if (ReferenceEquals(type, typeof(DiscordRole)))
                parametertype = ApplicationCommandOptionType.Role;
            else if (ReferenceEquals(type, typeof(SnowflakeObject)))
                parametertype = ApplicationCommandOptionType.Mentionable;
            else if (type.IsEnum)
                parametertype = ApplicationCommandOptionType.String;

            else
                throw new ArgumentException("Cannot convert type! Argument types must be string, long, bool, double, DiscordChannel, DiscordUser, DiscordRole, SnowflakeObject or an Enum.");

            return parametertype;
        }

        /// <summary>
        /// Gets the choice attributes from parameter.
        /// </summary>
        /// <param name="choiceattributes">The choiceattributes.</param>
        private List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromParameter(IEnumerable<ChoiceAttribute> choiceattributes)
        {
            if (!choiceattributes.Any())
            {
                return null;
            }

            return choiceattributes.Select(att => new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToList();
        }

        /// <summary>
        /// Parses the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        private async Task<List<DiscordApplicationCommandOption>> ParseParameters(ParameterInfo[] parameters)
        {
            var options = new List<DiscordApplicationCommandOption>();
            foreach (var parameter in parameters)
            {
                var optionattribute = parameter.GetCustomAttribute<OptionAttribute>();
                if (optionattribute == null)
                    throw new ArgumentException("Arguments must have the Option attribute!");

                var type = parameter.ParameterType;
                var parametertype = this.GetParameterType(type);

                var choices = this.GetChoiceAttributesFromParameter(parameter.GetCustomAttributes<ChoiceAttribute>());

                if (parameter.ParameterType.IsEnum)
                {
                    choices = GetChoiceAttributesFromEnumParameter(parameter.ParameterType);
                }

                var choiceProviders = parameter.GetCustomAttributes<ChoiceProviderAttribute>();

                if (choiceProviders.Any())
                {
                    choices = await this.GetChoiceAttributesFromProvider(choiceProviders);
                }

                options.Add(new DiscordApplicationCommandOption(optionattribute.Name, optionattribute.Description, parametertype, !parameter.IsOptional, choices));
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
            CommandMethods.Clear();
            GroupCommands.Clear();
            SubGroupCommands.Clear();
            _registeredCommands.Clear();

            await this.Update();
        }

        /// <summary>
        /// Fires whenver the execution of a slash command fails.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
        {
            add { _error.Register(value); }
            remove { _error.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs> _error;

        /// <summary>
        /// Fires when the execution of a slash command is successful.
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
        {
            add { _executed.Register(value); }
            remove { _executed.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs> _executed;
    }

    /// <summary>
    /// The command method.
    /// </summary>
    internal class CommandMethod
    {
        public ulong CommandId;
        public string Name;
        public MethodInfo Method;
    }

    /// <summary>
    /// The group command.
    /// </summary>
    internal class GroupCommand
    {
        public ulong CommandId;
        public string Name;
        public List<KeyValuePair<string, MethodInfo>> Methods = null;
    }

    /// <summary>
    /// The sub group command.
    /// </summary>
    internal class SubGroupCommand
    {
        public ulong CommandId;
        public string Name;
        public List<GroupCommand> SubCommands = new List<GroupCommand>();
    }
}
