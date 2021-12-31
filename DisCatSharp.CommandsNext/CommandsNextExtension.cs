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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.CommandsNext.Builders;
using DisCatSharp.CommandsNext.Converters;
using DisCatSharp.CommandsNext.Entities;
using DisCatSharp.CommandsNext.Exceptions;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.CommandsNext
{
    /// <summary>
    /// This is the class which handles command registration, management, and execution.
    /// </summary>
    public class CommandsNextExtension : BaseExtension
    {
        /// <summary>
        /// Gets the config.
        /// </summary>
        private CommandsNextConfiguration Config { get; }
        /// <summary>
        /// Gets the help formatter.
        /// </summary>
        private HelpFormatterFactory HelpFormatter { get; }

        /// <summary>
        /// Gets the convert generic.
        /// </summary>
        private MethodInfo ConvertGeneric { get; }
        /// <summary>
        /// Gets the user friendly type names.
        /// </summary>
        private Dictionary<Type, string> UserFriendlyTypeNames { get; }
        /// <summary>
        /// Gets the argument converters.
        /// </summary>
        internal Dictionary<Type, IArgumentConverter> ArgumentConverters { get; }

        /// <summary>
        /// Gets the service provider this CommandsNext module was configured with.
        /// </summary>
        public IServiceProvider Services
            => this.Config.ServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandsNextExtension"/> class.
        /// </summary>
        /// <param name="Cfg">The cfg.</param>
        internal CommandsNextExtension(CommandsNextConfiguration Cfg)
        {
            this.Config = new CommandsNextConfiguration(Cfg);
            this.TopLevelCommands = new Dictionary<string, Command>();
            this._registeredCommandsLazy = new Lazy<IReadOnlyDictionary<string, Command>>(() => new ReadOnlyDictionary<string, Command>(this.TopLevelCommands));
            this.HelpFormatter = new HelpFormatterFactory();
            this.HelpFormatter.SetFormatterType<DefaultHelpFormatter>();

            this.ArgumentConverters = new Dictionary<Type, IArgumentConverter>
            {
                [typeof(string)] = new StringConverter(),
                [typeof(bool)] = new BoolConverter(),
                [typeof(sbyte)] = new Int8Converter(),
                [typeof(byte)] = new Uint8Converter(),
                [typeof(short)] = new Int16Converter(),
                [typeof(ushort)] = new Uint16Converter(),
                [typeof(int)] = new Int32Converter(),
                [typeof(uint)] = new Uint32Converter(),
                [typeof(long)] = new Int64Converter(),
                [typeof(ulong)] = new Uint64Converter(),
                [typeof(float)] = new Float32Converter(),
                [typeof(double)] = new Float64Converter(),
                [typeof(decimal)] = new Float128Converter(),
                [typeof(DateTime)] = new DateTimeConverter(),
                [typeof(DateTimeOffset)] = new DateTimeOffsetConverter(),
                [typeof(TimeSpan)] = new TimeSpanConverter(),
                [typeof(Uri)] = new UriConverter(),
                [typeof(DiscordUser)] = new DiscordUserConverter(),
                [typeof(DiscordMember)] = new DiscordMemberConverter(),
                [typeof(DiscordRole)] = new DiscordRoleConverter(),
                [typeof(DiscordChannel)] = new DiscordChannelConverter(),
                [typeof(DiscordGuild)] = new DiscordGuildConverter(),
                [typeof(DiscordMessage)] = new DiscordMessageConverter(),
                [typeof(DiscordEmoji)] = new DiscordEmojiConverter(),
                [typeof(DiscordThreadChannel)] = new DiscordThreadChannelConverter(),
                [typeof(DiscordInvite)] = new DiscordInviteConverter(),
                [typeof(DiscordColor)] = new DiscordColorConverter(),
                [typeof(DiscordScheduledEvent)] = new DiscordScheduledEventConverter(),
            };

            this.UserFriendlyTypeNames = new Dictionary<Type, string>()
            {
                [typeof(string)] = "string",
                [typeof(bool)] = "boolean",
                [typeof(sbyte)] = "signed byte",
                [typeof(byte)] = "byte",
                [typeof(short)] = "short",
                [typeof(ushort)] = "unsigned short",
                [typeof(int)] = "int",
                [typeof(uint)] = "unsigned int",
                [typeof(long)] = "long",
                [typeof(ulong)] = "unsigned long",
                [typeof(float)] = "float",
                [typeof(double)] = "double",
                [typeof(decimal)] = "decimal",
                [typeof(DateTime)] = "date and time",
                [typeof(DateTimeOffset)] = "date and time",
                [typeof(TimeSpan)] = "time span",
                [typeof(Uri)] = "URL",
                [typeof(DiscordUser)] = "user",
                [typeof(DiscordMember)] = "member",
                [typeof(DiscordRole)] = "role",
                [typeof(DiscordChannel)] = "channel",
                [typeof(DiscordGuild)] = "guild",
                [typeof(DiscordMessage)] = "message",
                [typeof(DiscordEmoji)] = "emoji",
                [typeof(DiscordThreadChannel)] = "thread",
                [typeof(DiscordInvite)] = "invite",
                [typeof(DiscordColor)] = "color",
                [typeof(DiscordScheduledEvent)] = "event"
            };

            var ncvt = typeof(NullableConverter<>);
            var nt = typeof(Nullable<>);
            var cvts = this.ArgumentConverters.Keys.ToArray();
            foreach (var xt in cvts)
            {
                var xti = xt.GetTypeInfo();
                if (!xti.IsValueType)
                    continue;

                var xcvt = ncvt.MakeGenericType(xt);
                var xnt = nt.MakeGenericType(xt);
                if (this.ArgumentConverters.ContainsKey(xcvt))
                    continue;

                var xcv = Activator.CreateInstance(xcvt) as IArgumentConverter;
                this.ArgumentConverters[xnt] = xcv;
                this.UserFriendlyTypeNames[xnt] = this.UserFriendlyTypeNames[xt];
            }

            var t = typeof(CommandsNextExtension);
            var ms = t.GetTypeInfo().DeclaredMethods;
            var m = ms.FirstOrDefault(Xm => Xm.Name == "ConvertArgument" && Xm.ContainsGenericParameters && !Xm.IsStatic && Xm.IsPublic);
            this.ConvertGeneric = m;
        }

        /// <summary>
        /// Sets the help formatter to use with the default help command.
        /// </summary>
        /// <typeparam name="T">Type of the formatter to use.</typeparam>
        public void SetHelpFormatter<T>() where T : BaseHelpFormatter => this.HelpFormatter.SetFormatterType<T>();

        #region DiscordClient Registration
        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="Client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="System.InvalidOperationException"/>
        protected internal override void Setup(DiscordClient Client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = Client;

            this._executed = new AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs>("COMMAND_EXECUTED", TimeSpan.Zero, this.Client.EventErrorHandler);
            this._error = new AsyncEvent<CommandsNextExtension, CommandErrorEventArgs>("COMMAND_ERRORED", TimeSpan.Zero, this.Client.EventErrorHandler);

            if (this.Config.UseDefaultCommandHandler)
                this.Client.MessageCreated += this.HandleCommands;
            else
                this.Client.Logger.LogWarning(CommandsNextEvents.Misc, "Not attaching default command handler - if this is intentional, you can ignore this message");

            if (this.Config.EnableDefaultHelp)
            {
                this.RegisterCommands(typeof(DefaultHelpModule), null, null, out var tcmds);

                if (this.Config.DefaultHelpChecks != null)
                {
                    var checks = this.Config.DefaultHelpChecks.ToArray();

                    for (var i = 0; i < tcmds.Count; i++)
                        tcmds[i].WithExecutionChecks(checks);
                }

                if (tcmds != null)
                    foreach (var xc in tcmds)
                        this.AddToCommandDictionary(xc.Build(null));
            }

        }
        #endregion

        #region Command Handling
        /// <summary>
        /// Handles the commands async.
        /// </summary>
        /// <param name="Sender">The sender.</param>
        /// <param name="E">The e.</param>
        /// <returns>A Task.</returns>
        private async Task HandleCommands(DiscordClient Sender, MessageCreateEventArgs E)
        {
            if (E.Author.IsBot) // bad bot
                return;

            if (!this.Config.EnableDms && E.Channel.IsPrivate)
                return;

            var mpos = -1;
            if (this.Config.EnableMentionPrefix)
                mpos = E.Message.GetMentionPrefixLength(this.Client.CurrentUser);

            if (this.Config.StringPrefixes?.Any() == true)
                foreach (var pfix in this.Config.StringPrefixes)
                    if (mpos == -1 && !string.IsNullOrWhiteSpace(pfix))
                        mpos = E.Message.GetStringPrefixLength(pfix, this.Config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

            if (mpos == -1 && this.Config.PrefixResolver != null)
                mpos = await this.Config.PrefixResolver(E.Message).ConfigureAwait(false);

            if (mpos == -1)
                return;

            var pfx = E.Message.Content[..mpos];
            var cnt = E.Message.Content[mpos..];

            var __ = 0;
            var fname = cnt.ExtractNextArgument(ref __);

            var cmd = this.FindCommand(cnt, out var args);
            var ctx = this.CreateContext(E.Message, pfx, cmd, args);
            if (cmd == null)
            {
                await this._error.InvokeAsync(this, new CommandErrorEventArgs(this.Client.ServiceProvider) { Context = ctx, Exception = new CommandNotFoundException(fname) }).ConfigureAwait(false);
                return;
            }

            _ = Task.Run(async () => await this.ExecuteCommandAsync(ctx).ConfigureAwait(false));
        }

        /// <summary>
        /// Finds a specified command by its qualified name, then separates arguments.
        /// </summary>
        /// <param name="CommandString">Qualified name of the command, optionally with arguments.</param>
        /// <param name="RawArguments">Separated arguments.</param>
        /// <returns>Found command or null if none was found.</returns>
        public Command FindCommand(string CommandString, out string RawArguments)
        {
            RawArguments = null;

            var ignoreCase = !this.Config.CaseSensitive;
            var pos = 0;
            var next = CommandString.ExtractNextArgument(ref pos);
            if (next == null)
                return null;

            if (!this.RegisteredCommands.TryGetValue(next, out var cmd))
            {
                if (!ignoreCase)
                    return null;

                next = next.ToLowerInvariant();
                var cmdKvp = this.RegisteredCommands.FirstOrDefault(X => X.Key.ToLowerInvariant() == next);
                if (cmdKvp.Value == null)
                    return null;

                cmd = cmdKvp.Value;
            }

            if (cmd is not CommandGroup)
            {
                RawArguments = CommandString[pos..].Trim();
                return cmd;
            }

            while (cmd is CommandGroup)
            {
                var cm2 = cmd as CommandGroup;
                var oldPos = pos;
                next = CommandString.ExtractNextArgument(ref pos);
                if (next == null)
                    break;

                if (ignoreCase)
                {
                    next = next.ToLowerInvariant();
                    cmd = cm2.Children.FirstOrDefault(X => X.Name.ToLowerInvariant() == next || X.Aliases?.Any(Xx => Xx.ToLowerInvariant() == next) == true);
                }
                else
                {
                    cmd = cm2.Children.FirstOrDefault(X => X.Name == next || X.Aliases?.Contains(next) == true);
                }

                if (cmd == null)
                {
                    cmd = cm2;
                    pos = oldPos;
                    break;
                }
            }

            RawArguments = CommandString[pos..].Trim();
            return cmd;
        }

        /// <summary>
        /// Creates a command execution context from specified arguments.
        /// </summary>
        /// <param name="Msg">Message to use for context.</param>
        /// <param name="Prefix">Command prefix, used to execute commands.</param>
        /// <param name="Cmd">Command to execute.</param>
        /// <param name="RawArguments">Raw arguments to pass to command.</param>
        /// <returns>Created command execution context.</returns>
        public CommandContext CreateContext(DiscordMessage Msg, string Prefix, Command Cmd, string RawArguments = null)
        {
            var ctx = new CommandContext
            {
                Client = this.Client,
                Command = Cmd,
                Message = Msg,
                Config = this.Config,
                RawArgumentString = RawArguments ?? "",
                Prefix = Prefix,
                CommandsNext = this,
                Services = this.Services
            };

            if (Cmd != null && (Cmd.Module is TransientCommandModule || Cmd.Module == null))
            {
                var scope = ctx.Services.CreateScope();
                ctx.ServiceScopeContext = new CommandContext.ServiceContext(ctx.Services, scope);
                ctx.Services = scope.ServiceProvider;
            }

            return ctx;
        }

        /// <summary>
        /// Executes specified command from given context.
        /// </summary>
        /// <param name="Ctx">Context to execute command from.</param>
        /// <returns></returns>
        public async Task ExecuteCommandAsync(CommandContext Ctx)
        {
            try
            {
                var cmd = Ctx.Command;
                await this.RunAllChecks(cmd, Ctx).ConfigureAwait(false);

                var res = await cmd.ExecuteAsync(Ctx).ConfigureAwait(false);

                if (res.IsSuccessful)
                    await this._executed.InvokeAsync(this, new CommandExecutionEventArgs(this.Client.ServiceProvider) { Context = res.Context }).ConfigureAwait(false);
                else
                    await this._error.InvokeAsync(this, new CommandErrorEventArgs(this.Client.ServiceProvider) { Context = res.Context, Exception = res.Exception }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this._error.InvokeAsync(this, new CommandErrorEventArgs(this.Client.ServiceProvider) { Context = Ctx, Exception = ex }).ConfigureAwait(false);
            }
            finally
            {
                if (Ctx.ServiceScopeContext.IsInitialized)
                    Ctx.ServiceScopeContext.Dispose();
            }
        }

        /// <summary>
        /// Runs the all checks async.
        /// </summary>
        /// <param name="Cmd">The cmd.</param>
        /// <param name="Ctx">The ctx.</param>
        /// <returns>A Task.</returns>
        private async Task RunAllChecks(Command Cmd, CommandContext Ctx)
        {
            if (Cmd.Parent != null)
                await this.RunAllChecks(Cmd.Parent, Ctx).ConfigureAwait(false);

            var fchecks = await Cmd.RunChecksAsync(Ctx, false).ConfigureAwait(false);
            if (fchecks.Any())
                throw new ChecksFailedException(Cmd, Ctx, fchecks);
        }
        #endregion

        #region Command Registration
        /// <summary>
        /// Gets a dictionary of registered top-level commands.
        /// </summary>
        public IReadOnlyDictionary<string, Command> RegisteredCommands
            => this._registeredCommandsLazy.Value;

        /// <summary>
        /// Gets or sets the top level commands.
        /// </summary>
        private Dictionary<string, Command> TopLevelCommands { get; set; }
        private readonly Lazy<IReadOnlyDictionary<string, Command>> _registeredCommandsLazy;

        /// <summary>
        /// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
        /// </summary>
        /// <param name="Assembly">Assembly to register commands from.</param>
        public void RegisterCommands(Assembly Assembly)
        {
            var types = Assembly.ExportedTypes.Where(Xt =>
            {
                var xti = Xt.GetTypeInfo();
                return xti.IsModuleCandidateType() && !xti.IsNested;
            });
            foreach (var xt in types)
                this.RegisterCommands(xt);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <typeparam name="T">Class which holds commands to register.</typeparam>
        public void RegisterCommands<T>() where T : BaseCommandModule
        {
            var t = typeof(T);
            this.RegisterCommands(t);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <param name="T">Type of the class which holds commands to register.</param>
        public void RegisterCommands(Type T)
        {
            if (T == null)
                throw new ArgumentNullException(nameof(T), "Type cannot be null.");

            if (!T.IsModuleCandidateType())
                throw new ArgumentNullException(nameof(T), "Type must be a class, which cannot be abstract or static.");

            this.RegisterCommands(T, null, null, out var tempCommands);

            if (tempCommands != null)
                foreach (var command in tempCommands)
                    this.AddToCommandDictionary(command.Build(null));
        }

        /// <summary>
        /// Registers the commands.
        /// </summary>
        /// <param name="T">The type.</param>
        /// <param name="CurrentParent">The current parent.</param>
        /// <param name="InheritedChecks">The inherited checks.</param>
        /// <param name="FoundCommands">The found commands.</param>
        private void RegisterCommands(Type T, CommandGroupBuilder CurrentParent, IEnumerable<CheckBaseAttribute> InheritedChecks, out List<CommandBuilder> FoundCommands)
        {
            var ti = T.GetTypeInfo();

            var lifespan = ti.GetCustomAttribute<ModuleLifespanAttribute>();
            var moduleLifespan = lifespan != null ? lifespan.Lifespan : ModuleLifespan.Singleton;

            var module = new CommandModuleBuilder()
                .WithType(T)
                .WithLifespan(moduleLifespan)
                .Build(this.Services);

            // restrict parent lifespan to more or equally restrictive
            if (CurrentParent?.Module is TransientCommandModule && moduleLifespan != ModuleLifespan.Transient)
                throw new InvalidOperationException("In a transient module, child modules can only be transient.");

            // check if we are anything
            var groupBuilder = new CommandGroupBuilder(module);
            var isModule = false;
            var moduleAttributes = ti.GetCustomAttributes();
            var moduleHidden = false;
            var moduleChecks = new List<CheckBaseAttribute>();

            foreach (var xa in moduleAttributes)
            {
                switch (xa)
                {
                    case GroupAttribute g:
                        isModule = true;
                        var moduleName = g.Name;
                        if (moduleName == null)
                        {
                            moduleName = ti.Name;

                            if (moduleName.EndsWith("Group") && moduleName != "Group")
                                moduleName = moduleName[0..^5];
                            else if (moduleName.EndsWith("Module") && moduleName != "Module")
                                moduleName = moduleName[0..^6];
                            else if (moduleName.EndsWith("Commands") && moduleName != "Commands")
                                moduleName = moduleName[0..^8];
                        }

                        if (!this.Config.CaseSensitive)
                            moduleName = moduleName.ToLowerInvariant();

                        groupBuilder.WithName(moduleName);

                        if (InheritedChecks != null)
                            foreach (var chk in InheritedChecks)
                                groupBuilder.WithExecutionCheck(chk);

                        foreach (var mi in ti.DeclaredMethods.Where(X => X.IsCommandCandidate(out _) && X.GetCustomAttribute<GroupCommandAttribute>() != null))
                            groupBuilder.WithOverload(new CommandOverloadBuilder(mi));
                        break;

                    case AliasesAttribute a:
                        foreach (var xalias in a.Aliases)
                            groupBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                        break;

                    case HiddenAttribute h:
                        groupBuilder.WithHiddenStatus(true);
                        moduleHidden = true;
                        break;

                    case DescriptionAttribute d:
                        groupBuilder.WithDescription(d.Description);
                        break;

                    case CheckBaseAttribute c:
                        moduleChecks.Add(c);
                        groupBuilder.WithExecutionCheck(c);
                        break;

                    default:
                        groupBuilder.WithCustomAttribute(xa);
                        break;
                }
            }

            if (!isModule)
            {
                groupBuilder = null;
                if (InheritedChecks != null)
                    moduleChecks.AddRange(InheritedChecks);
            }

            // candidate methods
            var methods = ti.DeclaredMethods;
            var commands = new List<CommandBuilder>();
            var commandBuilders = new Dictionary<string, CommandBuilder>();
            foreach (var m in methods)
            {
                if (!m.IsCommandCandidate(out _))
                    continue;

                var attrs = m.GetCustomAttributes();
                if (attrs.FirstOrDefault(Xa => Xa is CommandAttribute) is not CommandAttribute cattr)
                    continue;

                var commandName = cattr.Name;
                if (commandName == null)
                {
                    commandName = m.Name;
                    if (commandName.EndsWith("Async") && commandName != "Async")
                        commandName = commandName[0..^5];
                }

                if (!this.Config.CaseSensitive)
                    commandName = commandName.ToLowerInvariant();

                if (!commandBuilders.TryGetValue(commandName, out var commandBuilder))
                {
                    commandBuilders.Add(commandName, commandBuilder = new CommandBuilder(module).WithName(commandName));

                    if (!isModule)
                        if (CurrentParent != null)
                            CurrentParent.WithChild(commandBuilder);
                        else
                            commands.Add(commandBuilder);
                    else
                        groupBuilder.WithChild(commandBuilder);
                }

                commandBuilder.WithOverload(new CommandOverloadBuilder(m));

                if (!isModule && moduleChecks.Any())
                    foreach (var chk in moduleChecks)
                        commandBuilder.WithExecutionCheck(chk);

                foreach (var xa in attrs)
                {
                    switch (xa)
                    {
                        case AliasesAttribute a:
                            foreach (var xalias in a.Aliases)
                                commandBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                            break;

                        case CheckBaseAttribute p:
                            commandBuilder.WithExecutionCheck(p);
                            break;

                        case DescriptionAttribute d:
                            commandBuilder.WithDescription(d.Description);
                            break;

                        case HiddenAttribute h:
                            commandBuilder.WithHiddenStatus(true);
                            break;

                        default:
                            commandBuilder.WithCustomAttribute(xa);
                            break;
                    }
                }

                if (!isModule && moduleHidden)
                    commandBuilder.WithHiddenStatus(true);
            }

            // candidate types
            var types = ti.DeclaredNestedTypes
                .Where(Xt => Xt.IsModuleCandidateType() && Xt.DeclaredConstructors.Any(Xc => Xc.IsPublic));
            foreach (var type in types)
            {
                this.RegisterCommands(type.AsType(),
                    groupBuilder,
                    !isModule ? moduleChecks : null,
                    out var tempCommands);

                if (isModule)
                    foreach (var chk in moduleChecks)
                        groupBuilder.WithExecutionCheck(chk);

                if (isModule && tempCommands != null)
                    foreach (var xtcmd in tempCommands)
                        groupBuilder.WithChild(xtcmd);
                else if (tempCommands != null)
                    commands.AddRange(tempCommands);
            }

            if (isModule && CurrentParent == null)
                commands.Add(groupBuilder);
            else if (isModule)
                CurrentParent.WithChild(groupBuilder);
            FoundCommands = commands;
        }

        /// <summary>
        /// Builds and registers all supplied commands.
        /// </summary>
        /// <param name="Cmds">Commands to build and register.</param>
        public void RegisterCommands(params CommandBuilder[] Cmds)
        {
            foreach (var cmd in Cmds)
                this.AddToCommandDictionary(cmd.Build(null));
        }

        /// <summary>
        /// Unregisters specified commands from CommandsNext.
        /// </summary>
        /// <param name="Cmds">Commands to unregister.</param>
        public void UnregisterCommands(params Command[] Cmds)
        {
            if (Cmds.Any(X => X.Parent != null))
                throw new InvalidOperationException("Cannot unregister nested commands.");

            var keys = this.RegisteredCommands.Where(X => Cmds.Contains(X.Value)).Select(X => X.Key).ToList();
            foreach (var key in keys)
                this.TopLevelCommands.Remove(key);
        }

        /// <summary>
        /// Adds the to command dictionary.
        /// </summary>
        /// <param name="Cmd">The cmd.</param>
        private void AddToCommandDictionary(Command Cmd)
        {
            if (Cmd.Parent != null)
                return;

            if (this.TopLevelCommands.ContainsKey(Cmd.Name) || (Cmd.Aliases != null && Cmd.Aliases.Any(Xs => this.TopLevelCommands.ContainsKey(Xs))))
                throw new DuplicateCommandException(Cmd.QualifiedName);

            this.TopLevelCommands[Cmd.Name] = Cmd;
            if (Cmd.Aliases != null)
                foreach (var xs in Cmd.Aliases)
                    this.TopLevelCommands[xs] = Cmd;
        }
        #endregion

        #region Default Help
        /// <summary>
        /// Represents the default help module.
        /// </summary>
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DefaultHelpModule : BaseCommandModule
        {
            /// <summary>
            /// Defaults the help async.
            /// </summary>
            /// <param name="Ctx">The ctx.</param>
            /// <param name="Command">The command.</param>
            /// <returns>A Task.</returns>
            [Command("help"), Description("Displays command help.")]
            public async Task DefaultHelpAsync(CommandContext Ctx, [Description("Command to provide help for.")] params string[] Command)
            {
                var topLevel = Ctx.CommandsNext.TopLevelCommands.Values.Distinct();
                var helpBuilder = Ctx.CommandsNext.HelpFormatter.Create(Ctx);

                if (Command != null && Command.Any())
                {
                    Command cmd = null;
                    var searchIn = topLevel;
                    foreach (var c in Command)
                    {
                        if (searchIn == null)
                        {
                            cmd = null;
                            break;
                        }

                        cmd = Ctx.Config.CaseSensitive
                            ? searchIn.FirstOrDefault(Xc => Xc.Name == c || (Xc.Aliases != null && Xc.Aliases.Contains(c)))
                            : searchIn.FirstOrDefault(Xc => Xc.Name.ToLowerInvariant() == c.ToLowerInvariant() || (Xc.Aliases != null && Xc.Aliases.Select(Xs => Xs.ToLowerInvariant()).Contains(c.ToLowerInvariant())));

                        if (cmd == null)
                            break;

                        var failedChecks = await cmd.RunChecksAsync(Ctx, true).ConfigureAwait(false);
                        if (failedChecks.Any())
                            throw new ChecksFailedException(cmd, Ctx, failedChecks);

                        searchIn = cmd is CommandGroup ? (cmd as CommandGroup).Children : null;
                    }

                    if (cmd == null)
                        throw new CommandNotFoundException(string.Join(" ", Command));

                    helpBuilder.WithCommand(cmd);

                    if (cmd is CommandGroup group)
                    {
                        var commandsToSearch = group.Children.Where(Xc => !Xc.IsHidden);
                        var eligibleCommands = new List<Command>();
                        foreach (var candidateCommand in commandsToSearch)
                        {
                            if (candidateCommand.ExecutionChecks == null || !candidateCommand.ExecutionChecks.Any())
                            {
                                eligibleCommands.Add(candidateCommand);
                                continue;
                            }

                            var candidateFailedChecks = await candidateCommand.RunChecksAsync(Ctx, true).ConfigureAwait(false);
                            if (!candidateFailedChecks.Any())
                                eligibleCommands.Add(candidateCommand);
                        }

                        if (eligibleCommands.Any())
                            helpBuilder.WithSubcommands(eligibleCommands.OrderBy(Xc => Xc.Name));
                    }
                }
                else
                {
                    var commandsToSearch = topLevel.Where(Xc => !Xc.IsHidden);
                    var eligibleCommands = new List<Command>();
                    foreach (var sc in commandsToSearch)
                    {
                        if (sc.ExecutionChecks == null || !sc.ExecutionChecks.Any())
                        {
                            eligibleCommands.Add(sc);
                            continue;
                        }

                        var candidateFailedChecks = await sc.RunChecksAsync(Ctx, true).ConfigureAwait(false);
                        if (!candidateFailedChecks.Any())
                            eligibleCommands.Add(sc);
                    }

                    if (eligibleCommands.Any())
                        helpBuilder.WithSubcommands(eligibleCommands.OrderBy(Xc => Xc.Name));
                }

                var helpMessage = helpBuilder.Build();

                var builder = new DiscordMessageBuilder().WithContent(helpMessage.Content).WithEmbed(helpMessage.Embed);

                if (!Ctx.Config.DmHelp || Ctx.Channel is DiscordDmChannel || Ctx.Guild == null)
                    await Ctx.Respond(builder).ConfigureAwait(false);
                else
                    await Ctx.Member.SendMessageAsync(builder).ConfigureAwait(false);

            }
        }
        #endregion

        #region Sudo
        /// <summary>
        /// Creates a fake command context to execute commands with.
        /// </summary>
        /// <param name="Actor">The user or member to use as message author.</param>
        /// <param name="Channel">The channel the message is supposed to appear from.</param>
        /// <param name="MessageContents">Contents of the message.</param>
        /// <param name="Prefix">Command prefix, used to execute commands.</param>
        /// <param name="Cmd">Command to execute.</param>
        /// <param name="RawArguments">Raw arguments to pass to command.</param>
        /// <returns>Created fake context.</returns>
        public CommandContext CreateFakeContext(DiscordUser Actor, DiscordChannel Channel, string MessageContents, string Prefix, Command Cmd, string RawArguments = null)
        {
            var epoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var now = DateTimeOffset.UtcNow;
            var timeSpan = (ulong)(now - epoch).TotalMilliseconds;

            // create fake message
            var msg = new DiscordMessage
            {
                Discord = this.Client,
                Author = Actor,
                ChannelId = Channel.Id,
                Content = MessageContents,
                Id = timeSpan << 22,
                Pinned = false,
                MentionEveryone = MessageContents.Contains("@everyone"),
                IsTts = false,
                _attachments = new List<DiscordAttachment>(),
                _embeds = new List<DiscordEmbed>(),
                TimestampRaw = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                _reactions = new List<DiscordReaction>()
            };

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = msg.Channel.Guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = msg.Channel.Guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(msg.Content))
            {
                if (msg.Channel.Guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(msg).Select(Xid => msg.Channel.Guild._members.TryGetValue(Xid, out var member) ? member : null).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(msg).Select(Xid => msg.Channel.Guild.GetRole(Xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(msg).Select(Xid => msg.Channel.Guild.GetChannel(Xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(msg).Select(this.Client.GetCachedOrEmptyUserInternal).ToList();
                }
            }

            msg._mentionedUsers = mentionedUsers;
            msg._mentionedRoles = mentionedRoles;
            msg._mentionedChannels = mentionedChannels;

            var ctx = new CommandContext
            {
                Client = this.Client,
                Command = Cmd,
                Message = msg,
                Config = this.Config,
                RawArgumentString = RawArguments ?? "",
                Prefix = Prefix,
                CommandsNext = this,
                Services = this.Services
            };

            if (Cmd != null && (Cmd.Module is TransientCommandModule || Cmd.Module == null))
            {
                var scope = ctx.Services.CreateScope();
                ctx.ServiceScopeContext = new CommandContext.ServiceContext(ctx.Services, scope);
                ctx.Services = scope.ServiceProvider;
            }

            return ctx;
        }
        #endregion

        #region Type Conversion
        /// <summary>
        /// Converts a string to specified type.
        /// </summary>
        /// <typeparam name="T">Type to convert to.</typeparam>
        /// <param name="Value">Value to convert.</param>
        /// <param name="Ctx">Context in which to convert to.</param>
        /// <returns>Converted object.</returns>
#pragma warning disable IDE1006 // Naming Styles
        public async Task<object> ConvertArgumentAsync<T>(string Value, CommandContext Ctx)
#pragma warning restore IDE1006 // Naming Styles
        {
            var t = typeof(T);
            if (!this.ArgumentConverters.ContainsKey(t))
                throw new ArgumentException("There is no converter specified for given type.", nameof(T));

            if (this.ArgumentConverters[t] is not IArgumentConverter<T> cv)
                throw new ArgumentException("Invalid converter registered for this type.", nameof(T));

            var cvr = await cv.Convert(Value, Ctx).ConfigureAwait(false);
            return !cvr.HasValue ? throw new ArgumentException("Could not convert specified value to given type.", nameof(Value)) : cvr.Value;
        }

        /// <summary>
        /// Converts a string to specified type.
        /// </summary>
        /// <param name="Value">Value to convert.</param>
        /// <param name="Ctx">Context in which to convert to.</param>
        /// <param name="Type">Type to convert to.</param>
        /// <returns>Converted object.</returns>
#pragma warning disable IDE1006 // Naming Styles
        public async Task<object> ConvertArgumentAsync(string Value, CommandContext Ctx, Type Type)
#pragma warning restore IDE1006 // Naming Styles
        {
            var m = this.ConvertGeneric.MakeGenericMethod(Type);
            try
            {
                return await (m.Invoke(this, new object[] { Value, Ctx }) as Task<object>).ConfigureAwait(false);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Registers an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to register the converter.</typeparam>
        /// <param name="Converter">Converter to register.</param>
        public void RegisterConverter<T>(IArgumentConverter<T> Converter)
        {
            if (Converter == null)
                throw new ArgumentNullException(nameof(Converter), "Converter cannot be null.");

            var t = typeof(T);
            var ti = t.GetTypeInfo();
            this.ArgumentConverters[t] = Converter;

            if (!ti.IsValueType)
                return;

            var nullableConverterType = typeof(NullableConverter<>).MakeGenericType(t);
            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            if (this.ArgumentConverters.ContainsKey(nullableType))
                return;

            var nullableConverter = Activator.CreateInstance(nullableConverterType) as IArgumentConverter;
            this.ArgumentConverters[nullableType] = nullableConverter;
        }

        /// <summary>
        /// Unregisters an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to unregister the converter.</typeparam>
        public void UnregisterConverter<T>()
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (this.ArgumentConverters.ContainsKey(t))
                this.ArgumentConverters.Remove(t);

            if (this.UserFriendlyTypeNames.ContainsKey(t))
                this.UserFriendlyTypeNames.Remove(t);

            if (!ti.IsValueType)
                return;

            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            if (!this.ArgumentConverters.ContainsKey(nullableType))
                return;

            this.ArgumentConverters.Remove(nullableType);
            this.UserFriendlyTypeNames.Remove(nullableType);
        }

        /// <summary>
        /// Registers a user-friendly type name.
        /// </summary>
        /// <typeparam name="T">Type to register the name for.</typeparam>
        /// <param name="Value">Name to register.</param>
        public void RegisterUserFriendlyTypeName<T>(string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
                throw new ArgumentNullException(nameof(Value), "Name cannot be null or empty.");

            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!this.ArgumentConverters.ContainsKey(t))
                throw new InvalidOperationException("Cannot register a friendly name for a type which has no associated converter.");

            this.UserFriendlyTypeNames[t] = Value;

            if (!ti.IsValueType)
                return;

            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            this.UserFriendlyTypeNames[nullableType] = Value;
        }

        /// <summary>
        /// Converts a type into user-friendly type name.
        /// </summary>
        /// <param name="T">Type to convert.</param>
        /// <returns>User-friendly type name.</returns>
        public string GetUserFriendlyTypeName(Type T)
        {
            if (this.UserFriendlyTypeNames.ContainsKey(T))
                return this.UserFriendlyTypeNames[T];

            var ti = T.GetTypeInfo();
            if (ti.IsGenericTypeDefinition && T.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var tn = ti.GenericTypeArguments[0];
                return this.UserFriendlyTypeNames.ContainsKey(tn) ? this.UserFriendlyTypeNames[tn] : tn.Name;
            }

            return T.Name;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Gets the configuration-specific string comparer. This returns <see cref="StringComparer.Ordinal"/> or <see cref="StringComparer.OrdinalIgnoreCase"/>,
        /// depending on whether <see cref="CommandsNextConfiguration.CaseSensitive"/> is set to <see langword="true"/> or <see langword="false"/>.
        /// </summary>
        /// <returns>A string comparer.</returns>
        internal IEqualityComparer<string> GetStringComparer()
            => this.Config.CaseSensitive
                ? StringComparer.Ordinal
                : StringComparer.OrdinalIgnoreCase;
        #endregion

        #region Events
        /// <summary>
        /// Triggered whenever a command executes successfully.
        /// </summary>
        public event AsyncEventHandler<CommandsNextExtension, CommandExecutionEventArgs> CommandExecuted
        {
            add { this._executed.Register(value); }
            remove { this._executed.Unregister(value); }
        }
        private AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs> _executed;

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event AsyncEventHandler<CommandsNextExtension, CommandErrorEventArgs> CommandErrored
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<CommandsNextExtension, CommandErrorEventArgs> _error;

        /// <summary>
        /// Ons the command executed.
        /// </summary>
        /// <param name="E">The e.</param>
        /// <returns>A Task.</returns>
        private Task OnCommandExecuted(CommandExecutionEventArgs E)
            => this._executed.InvokeAsync(this, E);

        /// <summary>
        /// Ons the command errored.
        /// </summary>
        /// <param name="E">The e.</param>
        /// <returns>A Task.</returns>
        private Task OnCommandErrored(CommandErrorEventArgs E)
            => this._error.InvokeAsync(this, E);
        #endregion
    }
}
