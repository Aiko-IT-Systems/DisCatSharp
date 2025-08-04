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

namespace DisCatSharp.CommandsNext;

/// <summary>
///     This is the class which handles command registration, management, and execution.
/// </summary>
public class CommandsNextExtension : BaseExtension
{
	/// <summary>
	///     Gets the config.
	/// </summary>
	private readonly CommandsNextConfiguration _config;

	/// <summary>
	///     Gets the convert generic.
	/// </summary>
	private readonly MethodInfo _convertGeneric;

	/// <summary>
	///     Gets the help formatter.
	/// </summary>
	private readonly HelpFormatterFactory _helpFormatter;

	/// <summary>
	///     Gets the user friendly type names.
	/// </summary>
	private readonly Dictionary<Type, string> _userFriendlyTypeNames;

	/// <summary>
	///     Initializes a new instance of the <see cref="CommandsNextExtension" /> class.
	/// </summary>
	/// <param name="cfg">The cfg.</param>
	internal CommandsNextExtension(CommandsNextConfiguration cfg)
	{
		this._config = new(cfg);
		this._topLevelCommands = [];
		this._registeredCommandsLazy = new(() => new ReadOnlyDictionary<string, Command>(this._topLevelCommands));
		this._helpFormatter = new();
		this._helpFormatter.SetFormatterType<DefaultHelpFormatter>();

		this.ArgumentConverters = new()
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
			[typeof(DiscordXThreadChannel)] = new DiscordThreadChannelConverter(),
			[typeof(DiscordInvite)] = new DiscordInviteConverter(),
			[typeof(DiscordColor)] = new DiscordColorConverter(),
			[typeof(DiscordScheduledEvent)] = new DiscordScheduledEventConverter()
		};

		this._userFriendlyTypeNames = new()
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
			[typeof(DiscordXThreadChannel)] = "thread",
			[typeof(DiscordInvite)] = "invite",
			[typeof(DiscordColor)] = "color",
			[typeof(DiscordScheduledEvent)] = "event"
		};

		foreach (var xt in this.ArgumentConverters.Keys.ToArray())
		{
			var xti = xt.GetTypeInfo();
			if (!xti.IsValueType)
				continue;

			var xcvt = typeof(NullableConverter<>).MakeGenericType(xt);
			var xnt = typeof(Nullable<>).MakeGenericType(xt);
			if (this.ArgumentConverters.ContainsKey(xcvt))
				continue;

			var xcv = Activator.CreateInstance(xcvt) as IArgumentConverter;
			this.ArgumentConverters[xnt] = xcv;
			this._userFriendlyTypeNames[xnt] = this._userFriendlyTypeNames[xt];
		}

		var t = this.GetType();
		var ms = t.GetTypeInfo().DeclaredMethods;
		var m = ms.FirstOrDefault(xm => xm is { Name: "ConvertArgumentToObj", ContainsGenericParameters: true } and { IsStatic: false, IsPrivate: true });
		this._convertGeneric = m;
	}

	/// <summary>
	///     Gets the argument converters.
	/// </summary>
	internal Dictionary<Type, IArgumentConverter> ArgumentConverters { get; }

	/// <summary>
	///     Gets the service provider this CommandsNext module was configured with.
	/// </summary>
	public IServiceProvider Services
		=> this._config.ServiceProvider;

	/// <summary>
	///     Sets the help formatter to use with the default help command.
	/// </summary>
	/// <typeparam name="T">Type of the formatter to use.</typeparam>
	public void SetHelpFormatter<T>() where T : BaseHelpFormatter => this._helpFormatter.SetFormatterType<T>();

#region DiscordClient Registration

	/// <summary>
	///     DO NOT USE THIS MANUALLY.
	/// </summary>
	/// <param name="client">DO NOT USE THIS MANUALLY.</param>
	/// <exception cref="InvalidOperationException" />
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;

		this._executed = new("COMMAND_EXECUTED", TimeSpan.Zero, this.Client.EventErrorHandler);
		this._error = new("COMMAND_ERRORED", TimeSpan.Zero, this.Client.EventErrorHandler);

		if (this._config.UseDefaultCommandHandler)
			this.Client.MessageCreated += this.HandleCommandsAsync;
		else
			this.Client.Logger.LogWarning(CommandsNextEvents.Misc, "Not attaching default command handler - if this is intentional, you can ignore this message");

		if (this._config.EnableDefaultHelp)
		{
			this.RegisterCommands(typeof(DefaultHelpModule), null, null, out var tcmds);

			if (this._config.DefaultHelpChecks != null)
			{
				var checks = this._config.DefaultHelpChecks.ToArray();

				foreach (var cb in tcmds)
					cb.WithExecutionChecks(checks);
			}

			if (tcmds != null)
				foreach (var xc in tcmds)
					this.AddToCommandDictionary(xc.Build(null));
		}
	}

#endregion

#region Sudo

	/// <summary>
	///     Creates a fake command context to execute commands with.
	/// </summary>
	/// <param name="actor">The user or member to use as message author.</param>
	/// <param name="channel">The channel the message is supposed to appear from.</param>
	/// <param name="messageContents">Contents of the message.</param>
	/// <param name="prefix">Command prefix, used to execute commands.</param>
	/// <param name="cmd">Command to execute.</param>
	/// <param name="rawArguments">Raw arguments to pass to command.</param>
	/// <returns>Created fake context.</returns>
	public CommandContext CreateFakeContext(DiscordUser actor, DiscordChannel channel, string messageContents, string prefix, Command cmd, string rawArguments = null)
	{
		var epoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var now = DateTimeOffset.UtcNow;
		var timeSpan = (ulong)(now - epoch).TotalMilliseconds;

		// create fake message
		var msg = new DiscordMessage
		{
			Discord = this.Client,
			Author = actor,
			ChannelId = channel.Id,
			Content = messageContents,
			Id = timeSpan << 22,
			Pinned = false,
			MentionEveryone = messageContents.Contains("@everyone"),
			IsTts = false,
			AttachmentsInternal = [],
			EmbedsInternal = [],
			TimestampRaw = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
			ReactionsInternal = [],
			GuildId = channel.GuildId
		};

		var mentionedUsers = new List<DiscordUser>();
		var mentionedRoles = msg.Channel.Guild != null ? new List<DiscordRole>() : null;
		var mentionedChannels = msg.Channel.Guild != null ? new List<DiscordChannel>() : null;

		if (!string.IsNullOrWhiteSpace(msg.Content))
		{
			if (msg.Channel.Guild != null)
			{
				mentionedUsers = Utilities.GetUserMentions(msg.Content).Select(xid => msg.Channel.Guild.Members.GetValueOrDefault(xid)).Cast<DiscordUser>().ToList();
				mentionedRoles = Utilities.GetRoleMentions(msg.Content).Select(xid => msg.Channel.Guild.GetRole(xid)).ToList();
				mentionedChannels = Utilities.GetChannelMentions(msg.Content).Select(xid => msg.Channel.Guild.GetChannel(xid)).ToList();
			}
			else
				mentionedUsers = Utilities.GetUserMentions(msg.Content).Select(this.Client.GetCachedOrEmptyUserInternal).ToList();
		}

		msg.MentionedUsersInternal = mentionedUsers;
		msg.MentionedRolesInternal = mentionedRoles;
		msg.MentionedChannelsInternal = mentionedChannels;

		var ctx = new CommandContext
		{
			Client = this.Client,
			Command = cmd,
			Message = msg,
			Config = this._config,
			RawArgumentString = rawArguments ?? "",
			Prefix = prefix,
			CommandsNext = this,
			Services = this.Services,
			UserId = msg.Author.Id,
			GuildId = msg.GuildId,
			MemberId = msg.GuildId is not null ? msg.Author.Id : null,
			ChannelId = msg.ChannelId
		};

		if (cmd != null && cmd.Module is TransientCommandModule or null)
		{
			var scope = ctx.Services.CreateScope();
			ctx.ServiceScopeContext = new(ctx.Services, scope);
			ctx.Services = scope.ServiceProvider;
		}

		return ctx;
	}

#endregion

#region Default Help

	/// <summary>
	///     Represents the default help module.
	/// </summary>
	[ModuleLifespan(ModuleLifespan.Transient)]
	public class DefaultHelpModule : BaseCommandModule
	{
		/// <summary>
		///     Defaults the help async.
		/// </summary>
		/// <param name="ctx">The ctx.</param>
		/// <param name="command">The command.</param>
		/// <returns>A Task.</returns>
		[Command("help"), Description("Displays command help.")]
		public async Task DefaultHelpAsync(CommandContext ctx, [Description("Command to provide help for.")] params string[] command)
		{
			var topLevel = ctx.CommandsNext._topLevelCommands.Values.Distinct();
			var helpBuilder = ctx.CommandsNext._helpFormatter.Create(ctx);

			if (command != null && command.Length != 0)
			{
				Command cmd = null;
				var searchIn = topLevel;
				foreach (var c in command)
				{
					if (searchIn == null)
					{
						cmd = null;
						break;
					}

					cmd = ctx.Config.CaseSensitive
						? searchIn.FirstOrDefault(xc => xc.Name == c || (xc.Aliases != null && xc.Aliases.Contains(c)))
						: searchIn.FirstOrDefault(xc => string.Equals(xc.Name, c, StringComparison.InvariantCultureIgnoreCase) || (xc.Aliases != null && xc.Aliases.Select(xs => xs.ToLowerInvariant()).Contains(c.ToLowerInvariant())));

					if (cmd == null)
						break;

					var failedChecks = await cmd.RunChecksAsync(ctx, true).ConfigureAwait(false);
					if (failedChecks.Any())
						throw new ChecksFailedException(cmd, ctx, failedChecks);

					searchIn = cmd is CommandGroup ? (cmd as CommandGroup).Children : null;
				}

				if (cmd == null)
					throw new CommandNotFoundException(string.Join(" ", command));

				helpBuilder.WithCommand(cmd);

				if (cmd is CommandGroup group)
				{
					var commandsToSearch = group.Children.Where(xc => !xc.IsHidden);
					var eligibleCommands = new List<Command>();
					foreach (var candidateCommand in commandsToSearch)
					{
						if (candidateCommand.ExecutionChecks == null || !candidateCommand.ExecutionChecks.Any())
						{
							eligibleCommands.Add(candidateCommand);
							continue;
						}

						var candidateFailedChecks = await candidateCommand.RunChecksAsync(ctx, true).ConfigureAwait(false);
						if (!candidateFailedChecks.Any())
							eligibleCommands.Add(candidateCommand);
					}

					if (eligibleCommands.Count != 0)
						helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
				}
			}
			else
			{
				var commandsToSearch = topLevel.Where(xc => !xc.IsHidden);
				var eligibleCommands = new List<Command>();
				foreach (var sc in commandsToSearch)
				{
					if (sc.ExecutionChecks == null || !sc.ExecutionChecks.Any())
					{
						eligibleCommands.Add(sc);
						continue;
					}

					var candidateFailedChecks = await sc.RunChecksAsync(ctx, true).ConfigureAwait(false);
					if (!candidateFailedChecks.Any())
						eligibleCommands.Add(sc);
				}

				if (eligibleCommands.Count != 0)
					helpBuilder.WithSubcommands(eligibleCommands.OrderBy(xc => xc.Name));
			}

			var helpMessage = helpBuilder.Build();

			var builder = new DiscordMessageBuilder().WithContent(helpMessage.Content).AddEmbed(helpMessage.Embed);

			if (!ctx.Config.DmHelp || ctx.Channel is DiscordDmChannel || ctx.Guild == null)
				await ctx.RespondAsync(builder).ConfigureAwait(false);
			else
				await ctx.Member.SendMessageAsync(builder).ConfigureAwait(false);
		}
	}

#endregion

#region Command Handling

	/// <summary>
	///     Handles the commands async.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private async Task HandleCommandsAsync(DiscordClient sender, MessageCreateEventArgs e)
	{
		if (e.Author.IsBot) // bad bot
			return;

		if (!this._config.EnableDms && e.Channel.IsPrivate)
			return;

		var mpos = -1;
		if (this._config.EnableMentionPrefix)
			mpos = e.Message.GetMentionPrefixLength(this.Client.CurrentUser);

		if (this._config.StringPrefixes?.Any() == true)
			foreach (var pfix in this._config.StringPrefixes.Where(pfix => mpos == -1 && !string.IsNullOrWhiteSpace(pfix)))
				mpos = e.Message.GetStringPrefixLength(pfix, this._config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

		if (mpos == -1 && this._config.PrefixResolver != null)
			mpos = await this._config.PrefixResolver(e.Message).ConfigureAwait(false);

		if (mpos == -1)
			return;

		var pfx = e.Message.Content[..mpos];
		var cnt = e.Message.Content[mpos..];

		var __ = 0;
		var fname = cnt.ExtractNextArgument(ref __);

		var cmd = this.FindCommand(cnt, out var args);
		var ctx = this.CreateContext(e.Message, pfx, cmd, args);
		if (cmd == null)
		{
			await this._error.InvokeAsync(this, new(this.Client.ServiceProvider)
			{
				Context = ctx,
				Exception = new CommandNotFoundException(fname)
			}).ConfigureAwait(false);
			return;
		}

		_ = Task.Run(async () => await this.ExecuteCommandAsync(ctx).ConfigureAwait(false));
	}

	/// <summary>
	///     Finds a specified command by its qualified name, then separates arguments.
	/// </summary>
	/// <param name="commandString">Qualified name of the command, optionally with arguments.</param>
	/// <param name="rawArguments">Separated arguments.</param>
	/// <returns>Found command or null if none was found.</returns>
	public Command FindCommand(string commandString, out string? rawArguments)
	{
		rawArguments = null;

		var ignoreCase = !this._config.CaseSensitive;
		var pos = 0;
		var next = commandString.ExtractNextArgument(ref pos);
		if (next == null)
			return null;

		if (!this.RegisteredCommands.TryGetValue(next, out var cmd))
		{
			if (!ignoreCase)
				return null;

			next = next.ToLowerInvariant();
			var cmdKvp = this.RegisteredCommands.FirstOrDefault(x => x.Key.ToLowerInvariant() == next);
			if (cmdKvp.Value == null)
				return null;

			cmd = cmdKvp.Value;
		}

		if (cmd is not CommandGroup)
		{
			rawArguments = commandString[pos..].Trim();
			return cmd;
		}

		while (cmd is CommandGroup)
		{
			var cm2 = cmd as CommandGroup;
			var oldPos = pos;
			next = commandString.ExtractNextArgument(ref pos);
			if (next == null)
				break;

			if (ignoreCase)
			{
				next = next.ToLowerInvariant();
				cmd = cm2.Children.FirstOrDefault(x => x.Name.ToLowerInvariant() == next || x.Aliases?.Any(xx => xx.ToLowerInvariant() == next) == true);
			}
			else
				cmd = cm2.Children.FirstOrDefault(x => x.Name == next || x.Aliases?.Contains(next) == true);

			if (cmd == null)
			{
				cmd = cm2;
				pos = oldPos;
				break;
			}
		}

		rawArguments = commandString[pos..].Trim();
		return cmd;
	}

	/// <summary>
	///     Creates a command execution context from specified arguments.
	/// </summary>
	/// <param name="msg">Message to use for context.</param>
	/// <param name="prefix">Command prefix, used to execute commands.</param>
	/// <param name="cmd">Command to execute.</param>
	/// <param name="rawArguments">Raw arguments to pass to command.</param>
	/// <returns>Created command execution context.</returns>
	public CommandContext CreateContext(DiscordMessage msg, string prefix, Command cmd, string? rawArguments = null)
	{
		var ctx = new CommandContext
		{
			Client = this.Client,
			Command = cmd,
			Message = msg,
			Config = this._config,
			RawArgumentString = rawArguments ?? "",
			Prefix = prefix,
			CommandsNext = this,
			Services = this.Services,
			UserId = msg.Author.Id,
			GuildId = msg.GuildId,
			MemberId = msg.GuildId is not null ? msg.Author.Id : null,
			ChannelId = msg.ChannelId
		};

		if (cmd != null && cmd.Module is TransientCommandModule or null)
		{
			var scope = ctx.Services.CreateScope();
			ctx.ServiceScopeContext = new(ctx.Services, scope);
			ctx.Services = scope.ServiceProvider;
		}

		return ctx;
	}

	/// <summary>
	///     Executes specified command from given context.
	/// </summary>
	/// <param name="ctx">Context to execute command from.</param>
	/// <returns></returns>
	public async Task ExecuteCommandAsync(CommandContext ctx)
	{
		try
		{
			var cmd = ctx.Command;
			await this.RunAllChecksAsync(cmd, ctx).ConfigureAwait(false);

			var res = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);

			if (res.IsSuccessful)
				await this._executed.InvokeAsync(this, new(this.Client.ServiceProvider)
				{
					Context = res.Context
				}).ConfigureAwait(false);
			else
				await this._error.InvokeAsync(this, new(this.Client.ServiceProvider)
				{
					Context = res.Context,
					Exception = res.Exception
				}).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			await this._error.InvokeAsync(this, new(this.Client.ServiceProvider)
			{
				Context = ctx,
				Exception = ex
			}).ConfigureAwait(false);
		}
		finally
		{
			if (ctx.ServiceScopeContext.IsInitialized)
				ctx.ServiceScopeContext.Dispose();
		}
	}

	/// <summary>
	///     Runs the all checks async.
	/// </summary>
	/// <param name="cmd">The cmd.</param>
	/// <param name="ctx">The ctx.</param>
	/// <returns>A Task.</returns>
	private async Task RunAllChecksAsync(Command cmd, CommandContext ctx)
	{
		if (cmd.Parent != null)
			await this.RunAllChecksAsync(cmd.Parent, ctx).ConfigureAwait(false);

		var fchecks = await cmd.RunChecksAsync(ctx, false).ConfigureAwait(false);
		if (fchecks.Any())
			throw new ChecksFailedException(cmd, ctx, fchecks);
	}

#endregion

#region Command Registration

	/// <summary>
	///     Gets a dictionary of registered top-level commands.
	/// </summary>
	public IReadOnlyDictionary<string, Command> RegisteredCommands
		=> this._registeredCommandsLazy.Value;

	/// <summary>
	///     Gets or sets the top level commands.
	/// </summary>
	private readonly Dictionary<string, Command> _topLevelCommands;

	private readonly Lazy<IReadOnlyDictionary<string, Command>> _registeredCommandsLazy;

	/// <summary>
	///     Registers all commands from a given assembly. The command classes need to be public to be considered for
	///     registration.
	/// </summary>
	/// <param name="assembly">Assembly to register commands from.</param>
	public void RegisterCommands(Assembly assembly)
	{
		var types = assembly.ExportedTypes.Where(xt =>
		{
			var xti = xt.GetTypeInfo();
			return xti.IsModuleCandidateType() && !xti.IsNested;
		});
		foreach (var xt in types)
			this.RegisterCommands(xt);
	}

	/// <summary>
	///     Registers all commands from a given command class.
	/// </summary>
	/// <typeparam name="T">Class which holds commands to register.</typeparam>
	public void RegisterCommands<T>() where T : BaseCommandModule
	{
		var t = typeof(T);
		this.RegisterCommands(t);
	}

	/// <summary>
	///     Registers all commands from a given command class.
	/// </summary>
	/// <param name="t">Type of the class which holds commands to register.</param>
	public void RegisterCommands(Type t)
	{
		if (t == null)
			throw new ArgumentNullException(nameof(t), "Type cannot be null.");

		if (!t.IsModuleCandidateType())
			throw new ArgumentNullException(nameof(t), "Type must be a class, which cannot be abstract or static.");

		this.RegisterCommands(t, null, null, out var tempCommands);

		if (tempCommands != null)
			foreach (var command in tempCommands)
				this.AddToCommandDictionary(command.Build(null));
	}

	/// <summary>
	///     Registers the commands.
	/// </summary>
	/// <param name="t">The type.</param>
	/// <param name="currentParent">The current parent.</param>
	/// <param name="inheritedChecks">The inherited checks.</param>
	/// <param name="foundCommands">The found commands.</param>
	private void RegisterCommands(Type t, CommandGroupBuilder currentParent, IEnumerable<CheckBaseAttribute> inheritedChecks, out List<CommandBuilder> foundCommands)
	{
		var ti = t.GetTypeInfo();

		var lifespan = ti.GetCustomAttribute<ModuleLifespanAttribute>();
		var moduleLifespan = lifespan != null ? lifespan.Lifespan : ModuleLifespan.Singleton;

		var module = new CommandModuleBuilder()
			.WithType(t)
			.WithLifespan(moduleLifespan)
			.Build(this.Services);

		// restrict parent lifespan to more or equally restrictive
		if (currentParent?.Module is TransientCommandModule && moduleLifespan != ModuleLifespan.Transient)
			throw new InvalidOperationException("In a transient module, child modules can only be transient.");

		// check if we are anything
		var groupBuilder = new CommandGroupBuilder(module);
		var isModule = false;
		var moduleAttributes = ti.GetCustomAttributes();
		var moduleHidden = false;
		var moduleChecks = new List<CheckBaseAttribute>();

		foreach (var xa in moduleAttributes)
			switch (xa)
			{
				case GroupAttribute g:
					isModule = true;
					var moduleName = g.Name;
					if (moduleName == null)
					{
						moduleName = ti.Name;

						if (moduleName.EndsWith("Group", StringComparison.Ordinal) && moduleName != "Group")
							moduleName = moduleName[..^5];
						else if (moduleName.EndsWith("Module", StringComparison.Ordinal) && moduleName != "Module")
							moduleName = moduleName[..^6];
						else if (moduleName.EndsWith("Commands", StringComparison.Ordinal) && moduleName != "Commands")
							moduleName = moduleName[..^8];
					}

					if (!this._config.CaseSensitive)
						moduleName = moduleName.ToLowerInvariant();

					groupBuilder.WithName(moduleName);

					if (inheritedChecks != null)
						foreach (var chk in inheritedChecks)
							groupBuilder.WithExecutionCheck(chk);

					foreach (var mi in ti.DeclaredMethods.Where(x => x.IsCommandCandidate(out _) && x.GetCustomAttribute<GroupCommandAttribute>() != null))
						groupBuilder.WithOverload(new(mi));
					break;

				case AliasesAttribute a:
					foreach (var xalias in a.Aliases)
						groupBuilder.WithAlias(this._config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
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

		if (!isModule)
		{
			groupBuilder = null;
			if (inheritedChecks != null)
				moduleChecks.AddRange(inheritedChecks);
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
			if (attrs.FirstOrDefault(xa => xa is CommandAttribute) is not CommandAttribute cattr)
				continue;

			var commandName = cattr.Name;
			if (commandName == null)
			{
				commandName = m.Name;
				if (commandName.EndsWith("Async", StringComparison.Ordinal) && commandName != "Async")
					commandName = commandName[..^5];
			}

			if (!this._config.CaseSensitive)
				commandName = commandName.ToLowerInvariant();

			if (!commandBuilders.TryGetValue(commandName, out var commandBuilder))
			{
				commandBuilders.Add(commandName, commandBuilder = new CommandBuilder(module).WithName(commandName));

				if (!isModule)
					if (currentParent != null)
						currentParent.WithChild(commandBuilder);
					else
						commands.Add(commandBuilder);
				else
					groupBuilder.WithChild(commandBuilder);
			}

			commandBuilder.WithOverload(new(m));

			if (!isModule && moduleChecks.Count != 0)
				foreach (var chk in moduleChecks)
					commandBuilder.WithExecutionCheck(chk);

			foreach (var xa in attrs)
				switch (xa)
				{
					case AliasesAttribute a:
						foreach (var xalias in a.Aliases)
							commandBuilder.WithAlias(this._config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
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

			if (!isModule && moduleHidden)
				commandBuilder.WithHiddenStatus(true);
		}

		// candidate types
		var types = ti.DeclaredNestedTypes
			.Where(xt => xt.IsModuleCandidateType() && xt.DeclaredConstructors.Any(xc => xc.IsPublic));
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

		if (isModule && currentParent == null)
			commands.Add(groupBuilder);
		else if (isModule)
			currentParent.WithChild(groupBuilder);
		foundCommands = commands;
	}

	/// <summary>
	///     Builds and registers all supplied commands.
	/// </summary>
	/// <param name="cmds">Commands to build and register.</param>
	public void RegisterCommands(params CommandBuilder[] cmds)
	{
		foreach (var cmd in cmds)
			this.AddToCommandDictionary(cmd.Build(null));
	}

	/// <summary>
	///     Unregister specified commands from CommandsNext.
	/// </summary>
	/// <param name="cmds">Commands to unregister.</param>
	public void UnregisterCommands(params Command[] cmds)
	{
		if (cmds.Any(x => x.Parent != null))
			throw new InvalidOperationException("Cannot unregister nested commands.");

		var keys = this.RegisteredCommands.Where(x => cmds.Contains(x.Value)).Select(x => x.Key).ToList();
		foreach (var key in keys)
			this._topLevelCommands.Remove(key);
	}

	/// <summary>
	///     Adds the to command dictionary.
	/// </summary>
	/// <param name="cmd">The cmd.</param>
	private void AddToCommandDictionary(Command cmd)
	{
		if (cmd.Parent != null)
			return;

		if (this._topLevelCommands.ContainsKey(cmd.Name) || (cmd.Aliases != null && cmd.Aliases.Any(xs => this._topLevelCommands.ContainsKey(xs))))
			throw new DuplicateCommandException(cmd.QualifiedName);

		this._topLevelCommands[cmd.Name] = cmd;
		if (cmd.Aliases != null)
			foreach (var xs in cmd.Aliases)
				this._topLevelCommands[xs] = cmd;
	}

#endregion

#region Type Conversion

	/// <summary>
	///     Converts a string to specified type.
	/// </summary>
	/// <typeparam name="T">Type to convert to.</typeparam>
	/// <param name="value">Value to convert.</param>
	/// <param name="ctx">Context in which to convert to.</param>
	/// <returns>Converted object.</returns>
	public async Task<T> ConvertArgument<T>(string value, CommandContext ctx)
	{
		var t = typeof(T);
		if (!this.ArgumentConverters.ContainsKey(t))
			throw new ArgumentException("There is no converter specified for given type.", nameof(T));

		if (this.ArgumentConverters[t] is not IArgumentConverter<T> cv)
			throw new ArgumentException("Invalid converter registered for this type.", nameof(T));

		var cvr = await cv.ConvertAsync(value, ctx).ConfigureAwait(false);
		return !cvr.HasValue ? throw new ArgumentException("Could not convert specified value to given type.", nameof(value)) : cvr.Value;
	}

	/// <summary>
	///     Converts a string to specified type.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	/// <param name="ctx">Context in which to convert to.</param>
	/// <param name="type">Type to convert to.</param>
	/// <returns>Converted object.</returns>
	public async Task<object> ConvertArgument(string value, CommandContext ctx, Type type)
	{
		var m = this._convertGeneric.MakeGenericMethod(type);
		try
		{
			return await (m.Invoke(this, [value, ctx]) as Task<object>).ConfigureAwait(false);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException;
		}
	}

	/// <summary>
	///     Registers an argument converter for specified type.
	/// </summary>
	/// <typeparam name="T">Type for which to register the converter.</typeparam>
	/// <param name="converter">Converter to register.</param>
	public void RegisterConverter<T>(IArgumentConverter<T> converter)
	{
		if (converter == null)
			throw new ArgumentNullException(nameof(converter), "Converter cannot be null.");

		var t = typeof(T);
		var ti = t.GetTypeInfo();
		this.ArgumentConverters[t] = converter;

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
	///     Unregister an argument converter for specified type.
	/// </summary>
	/// <typeparam name="T">Type for which to unregister the converter.</typeparam>
	public void UnregisterConverter<T>()
	{
		var t = typeof(T);
		var ti = t.GetTypeInfo();
		this.ArgumentConverters.Remove(t);

		this._userFriendlyTypeNames.Remove(t);

		if (!ti.IsValueType)
			return;

		var nullableType = typeof(Nullable<>).MakeGenericType(t);
		if (!this.ArgumentConverters.ContainsKey(nullableType))
			return;

		this.ArgumentConverters.Remove(nullableType);
		this._userFriendlyTypeNames.Remove(nullableType);
	}

	/// <summary>
	///     Registers a user-friendly type name.
	/// </summary>
	/// <typeparam name="T">Type to register the name for.</typeparam>
	/// <param name="value">Name to register.</param>
	public void RegisterUserFriendlyTypeName<T>(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentNullException(nameof(value), "Name cannot be null or empty.");

		var t = typeof(T);
		var ti = t.GetTypeInfo();
		if (!this.ArgumentConverters.ContainsKey(t))
			throw new InvalidOperationException("Cannot register a friendly name for a type which has no associated converter.");

		this._userFriendlyTypeNames[t] = value;

		if (!ti.IsValueType)
			return;

		var nullableType = typeof(Nullable<>).MakeGenericType(t);
		this._userFriendlyTypeNames[nullableType] = value;
	}

	/// <summary>
	///     Converts a type into user-friendly type name.
	/// </summary>
	/// <param name="t">Type to convert.</param>
	/// <returns>User-friendly type name.</returns>
	public string GetUserFriendlyTypeName(Type t)
	{
		if (this._userFriendlyTypeNames.TryGetValue(t, out var userFriendlyTypeName))
			return userFriendlyTypeName;

		var ti = t.GetTypeInfo();
		if (!ti.IsGenericTypeDefinition || t.GetGenericTypeDefinition() != typeof(Nullable<>)) return t.Name;

		var tn = ti.GenericTypeArguments[0];
		return this._userFriendlyTypeNames.TryGetValue(tn, out var name) ? name : tn.Name;
	}

#endregion

#region Helpers

	/// <summary>
	///     Allows easier interoperability with reflection by turning the <see cref="Task{T}" /> returned by
	///     <see cref="ConvertArgument" />
	///     into a task containing <see cref="object" />, using the provided generic type information.
	/// </summary>
	private async Task<object> ConvertArgumentToObj<T>(string value, CommandContext ctx)
		=> await this.ConvertArgument<T>(value, ctx).ConfigureAwait(false);

	/// <summary>
	///     Gets the configuration-specific string comparer. This returns <see cref="StringComparer.Ordinal" /> or
	///     <see cref="StringComparer.OrdinalIgnoreCase" />,
	///     depending on whether <see cref="CommandsNextConfiguration.CaseSensitive" /> is set to <see langword="true" /> or
	///     <see langword="false" />.
	/// </summary>
	/// <returns>A string comparer.</returns>
	internal IEqualityComparer<string> GetStringComparer()
		=> this._config.CaseSensitive
			? StringComparer.Ordinal
			: StringComparer.OrdinalIgnoreCase;

#endregion

#region Events

	/// <summary>
	///     Triggered whenever a command executes successfully.
	/// </summary>
	public event AsyncEventHandler<CommandsNextExtension, CommandExecutionEventArgs> CommandExecuted
	{
		add => this._executed.Register(value);
		remove => this._executed.Unregister(value);
	}

	private AsyncEvent<CommandsNextExtension, CommandExecutionEventArgs> _executed;

	/// <summary>
	///     Triggered whenever a command throws an exception during execution.
	/// </summary>
	public event AsyncEventHandler<CommandsNextExtension, CommandErrorEventArgs> CommandErrored
	{
		add => this._error.Register(value);
		remove => this._error.Unregister(value);
	}

	private AsyncEvent<CommandsNextExtension, CommandErrorEventArgs> _error;

	/// <summary>
	///     Fires when a command gets executed.
	/// </summary>
	/// <param name="e">The command execution event arguments.</param>
	private Task OnCommandExecuted(CommandExecutionEventArgs e)
		=> this._executed.InvokeAsync(this, e);

	/// <summary>
	///     Fires when a command fails.
	/// </summary>
	/// <param name="e">The command error event arguments.</param>
	private Task OnCommandErrored(CommandErrorEventArgs e)
		=> this._error.InvokeAsync(this, e);

#endregion
}
