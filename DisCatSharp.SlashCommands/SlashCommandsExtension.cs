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
    /// A class that handles slash commands for a client
    /// </summary>
    public class SlashCommandsExtension : BaseExtension
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
        /// Gets or sets the update list.
        /// </summary>
        private List<KeyValuePair<ulong?, Type>> UpdateList { get; set; } = new List<KeyValuePair<ulong?, Type>>();

        private readonly SlashCommandsConfiguration _configuration;
        /// <summary>
        /// Gets or sets a value indicating whether errored.
        /// </summary>
        private static bool Errored { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlashCommandsExtension"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal SlashCommandsExtension(SlashCommandsConfiguration configuration)
        {
            this._configuration = configuration;
        }

        /// <summary>
        /// Runs setup. DO NOT RUN THIS MANUALLY. DO NOT DO ANYTHING WITH THIS.
        /// I could've broken it..
        /// </summary>
        /// <param name="client">The client to setup on.</param>
        /// <changed>Lala Sabathil,06.07.2021</changed>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            this._error = new AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, null);
            this._executed = new AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, null);

            this.Client.Ready += this.Update;
            this.Client.InteractionCreated += this.InteractionHandler;
        }

        //Register

        /// <summary>
        /// Registers a command class
        /// </summary>
        /// <typeparam name="T">The command class to register</typeparam>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands<T>(ulong? guildId = null) where T : SlashCommandModule
        {
            if (this.Client.ShardId == 0)
                this.UpdateList.Add(new KeyValuePair<ulong?, Type>(guildId, typeof(T)));
        }

        /// <summary>
        /// Registers a command class
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the command class to register</param>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public void RegisterCommands(Type type, ulong? guildId = null)
        {
            if (!typeof(SlashCommandModule).IsAssignableFrom(type))
                throw new ArgumentException("Command classes have to inherit from SlashCommandModule", nameof(type));
            if (this.Client.ShardId == 0)
                this.UpdateList.Add(new KeyValuePair<ulong?, Type>(guildId, type));
        }

        /// <summary>
        /// Updates the.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="e">The event.</param>

        internal Task Update(DiscordClient client, ReadyEventArgs e)
        {
            if (client.ShardId == 0)
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
        /// <param name="guildid">The guildid.</param>
        private void RegisterCommands(IEnumerable<Type> types, ulong? guildid)
        {
            var InternalCommandMethods = new List<CommandMethod>();
            var InternalGroupCommands = new List<GroupCommand>();
            var InternalSubGroupCommands = new List<SubGroupCommand>();
            var ToUpdate = new List<DiscordApplicationCommand>();

            _ = Task.Run(async () =>
            {
                foreach (var t in types)
                {
                    try
                    {
                        var ti = t.GetTypeInfo();

                        var classes = ti.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
                        foreach (var tti in classes)
                        {
                            var groupatt = tti.GetCustomAttribute<SlashCommandGroupAttribute>();
                            var submethods = tti.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null).ToList();
                            var subclasses = tti.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
                            if (subclasses.Any() && submethods.Any())
                            {
                                throw new ArgumentException("Slash command groups cannot have both subcommands and subgroups!");
                            }
                            var payload = new DiscordApplicationCommand(groupatt.Name, groupatt.Description);


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

                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload });

                                InternalGroupCommands.Add(new GroupCommand { Name = groupatt.Name, ParentClass = tti, Methods = commandmethods });
                            }
                            foreach (var subclass in subclasses)
                            {
                                var subgroupatt = subclass.GetCustomAttribute<SlashCommandGroupAttribute>();
                                var subsubmethods = subclass.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

                                var command = new SubGroupCommand { Name = groupatt.Name };

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
                                command.SubCommands.Add(new GroupCommand { Name = subgroupatt.Name, ParentClass = subclass, Methods = currentMethods });
                                InternalSubGroupCommands.Add(command);
                                payload = new DiscordApplicationCommand(payload.Name, payload.Description, payload.Options?.Append(subpayload) ?? new[] { subpayload });
                            }
                            ToUpdate.Add(payload);
                        }

                        var methods = ti.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);
                        foreach (var method in methods)
                        {
                            var commandattribute = method.GetCustomAttribute<SlashCommandAttribute>();

                            var options = new List<DiscordApplicationCommandOption>();

                            var parameters = method.GetParameters();
                            if (parameters.Length == 0 || parameters == null || !ReferenceEquals(parameters.FirstOrDefault()?.ParameterType, typeof(InteractionContext)))
                                throw new ArgumentException($"The first argument must be an InteractionContext!");
                            parameters = parameters.Skip(1).ToArray();
                            options = options.Concat(await this.ParseParameters(parameters)).ToList();

                            InternalCommandMethods.Add(new CommandMethod { Method = method, Name = commandattribute.Name, ParentClass = t });

                            var payload = new DiscordApplicationCommand(commandattribute.Name, commandattribute.Description, options);
                            ToUpdate.Add(payload);
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
                        if (guildid == null)
                        {
                            commands = await this.Client.BulkOverwriteGlobalApplicationCommandsAsync(ToUpdate);
                        }
                        else
                        {
                            commands = await this.Client.BulkOverwriteGuildApplicationCommandsAsync(guildid.Value, ToUpdate);
                        }
                        foreach (var command in commands)
                        {
                            if (InternalCommandMethods.Any(x => x.Name == command.Name))
                                InternalCommandMethods.First(x => x.Name == command.Name).Id = command.Id;

                            else if (InternalGroupCommands.Any(x => x.Name == command.Name))
                                InternalGroupCommands.First(x => x.Name == command.Name).Id = command.Id;

                            else if (InternalSubGroupCommands.Any(x => x.Name == command.Name))
                                InternalSubGroupCommands.First(x => x.Name == command.Name).Id = command.Id;
                        }
                        CommandMethods.AddRange(InternalCommandMethods);
                        GroupCommands.AddRange(InternalGroupCommands);
                        SubGroupCommands.AddRange(InternalSubGroupCommands);
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
        /// <returns>A list of DiscordApplicationCommandOptionChoices.</returns>
        private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromEnumParameter(Type enumParam)
        {
            var choices = new List<DiscordApplicationCommandOptionChoice>();
            foreach (Enum foo in Enum.GetValues(enumParam))
            {
                choices.Add(new DiscordApplicationCommandOptionChoice(foo.GetName(), foo.ToString()));
            }
            return choices;
        }

        //Handler
        /// <summary>
        /// Interactions the handler.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="e">The event.</param>

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
                        Services = this._configuration?.Services
                    };

                    try
                    {
                        if (Errored)
                            throw new InvalidOperationException("Slash commands failed to register properly on startup.");
                        var methods = CommandMethods.Where(x => x.Id == e.Interaction.Data.Id);
                        var groups = GroupCommands.Where(x => x.Id == e.Interaction.Data.Id);
                        var subgroups = SubGroupCommands.Where(x => x.Id == e.Interaction.Data.Id);
                        if (!methods.Any() && !groups.Any() && !subgroups.Any())
                            throw new InvalidOperationException("A slash command was executed, but no command was registered for it.");

                        if (methods.Any())
                        {
                            var method = methods.First();

                            var args = await this.ResolveInteractionCommandParameters(e, context, method.Method, e.Interaction.Data.Options);

                            object classinstance = method.Method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.Services, method.ParentClass) : CreateInstance(method.Method.DeclaringType, this._configuration?.Services);

                            await this.RunPreexecutionChecksAsync(method.Method, context);

                            await ((SlashCommandModule)classinstance).BeforeExecutionAsync(context);

                            var task = (Task)method.Method.Invoke(classinstance, args.ToArray());
                            await task;

                            await ((SlashCommandModule)classinstance).AfterExecutionAsync(context);
                        }
                        else if (groups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();
                            var method = groups.First().Methods.First(x => x.Key == command.Name).Value;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options);
                            object classinstance = method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.Services, groups.First().ParentClass) : CreateInstance(groups.First().ParentClass, this._configuration?.Services);

                            SlashCommandModule module = null;
                            if (classinstance is SlashCommandModule _module)
                                module = _module;

                            await this.RunPreexecutionChecksAsync(method, context);

                            await (module?.BeforeExecutionAsync(context) ?? Task.CompletedTask);

                            var task = (Task)method.Invoke(classinstance, args.ToArray());
                            await task;

                            await (module?.AfterExecutionAsync(context) ?? Task.CompletedTask);
                        }
                        else if (subgroups.Any())
                        {
                            var command = e.Interaction.Data.Options.First();

                            var subgroup = subgroups.First();

                            var group = subgroup.SubCommands.First(x => x.Name == command.Name);

                            var method = group.Methods.First(x => x.Key == command.Options.First().Name).Value;

                            var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options.First().Options.First().Options);
                            object classinstance = method.IsStatic ? ActivatorUtilities.CreateInstance(this._configuration?.Services, group.ParentClass) : CreateInstance(group.ParentClass, this._configuration?.Services);

                            SlashCommandModule module = null;
                            if (classinstance is SlashCommandModule _module)
                                module = _module;

                            await this.RunPreexecutionChecksAsync(method, context);

                            await (module?.BeforeExecutionAsync(context) ?? Task.CompletedTask);

                            var task = (Task)method.Invoke(classinstance, args.ToArray());
                            await task;

                            await (module?.AfterExecutionAsync(context) ?? Task.CompletedTask);
                        }

                        await this._executed.InvokeAsync(this, new SlashCommandExecutedEventArgs { Context = context });
                    }
                    catch (Exception ex)
                    {
                        await this._error.InvokeAsync(this, new SlashCommandErrorEventArgs { Context = context, Exception = ex });
                    }
                }
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="services">The services.</param>
        /// <returns>An object.</returns>
        internal static object CreateInstance(Type t, IServiceProvider services)
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
        /// <param name="e">The event.</param>
        /// <param name="context">The context.</param>
        /// <param name="method">The method.</param>
        /// <param name="options">The options.</param>

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
                    else if (ReferenceEquals(parameter.ParameterType, typeof(double)))
                        args.Add((double)option.Value);
                    else if (ReferenceEquals(parameter.ParameterType, typeof(bool)))
                        args.Add((bool)option.Value);
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
                    else
                        throw new ArgumentException($"Error resolving interaction.");
                }
            }

            return args;
        }

        /// <summary>
        /// Runs the preexecution checks async.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="ctx">The ctx.</param>

        private async Task RunPreexecutionChecksAsync(MethodInfo method, InteractionContext ctx)
        {
            var attributes = method.GetCustomAttributes<SlashCheckBaseAttribute>(true);
            var dict = new Dictionary<SlashCheckBaseAttribute, bool>();
            foreach (var att in attributes)
            {
                var result = await att.ExecuteChecksAsync(ctx);
                dict.Add(att, result);
            }

            if (dict.Any(x => x.Value == false))
                throw new SlashExecutionChecksFailedException(dict.Where(x => x.Value == false).Select(x => x.Key).ToList());
        }

        //Events

        /// <summary>
        /// Fires whenver the execution of a slash command fails
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandErrorEventArgs> _error;

        /// <summary>
        /// Fires when the execution of a slash command is successful
        /// </summary>
        public event AsyncEventHandler<SlashCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
        {
            add { this._executed.Register(value); }
            remove { this._executed.Unregister(value); }
        }
        private AsyncEvent<SlashCommandsExtension, SlashCommandExecutedEventArgs> _executed;

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An ApplicationCommandOptionType.</returns>
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
            else if (type.IsEnum)
                parametertype = ApplicationCommandOptionType.String;

            else
                throw new ArgumentException("Cannot convert type! Argument types must be string, long, bool, double, DiscordChannel, DiscordUser, DiscordRole or an Enum.");

            return parametertype;
        }

        /// <summary>
        /// Gets the choice attributes from parameter.
        /// </summary>
        /// <param name="choiceattributes">The choiceattributes.</param>
        /// <returns>A list of DiscordApplicationCommandOptionChoices.</returns>
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

    }

    /// <summary>
    /// Represents a command method.
    /// </summary>
    internal class CommandMethod
    {
        public ulong Id;

        public string Name;
        public MethodInfo Method;
        public Type ParentClass;
    }

    /// <summary>
    /// Represents a group command.
    /// </summary>
    internal class GroupCommand
    {
        public ulong Id;

        public string Name;
        public List<KeyValuePair<string, MethodInfo>> Methods = null;
        public Type ParentClass;
    }

    /// <summary>
    /// Represents a sub group command.
    /// </summary>
    internal class SubGroupCommand
    {
        public ulong Id;

        public string Name;
        public List<GroupCommand> SubCommands = new List<GroupCommand>();
    }
}
