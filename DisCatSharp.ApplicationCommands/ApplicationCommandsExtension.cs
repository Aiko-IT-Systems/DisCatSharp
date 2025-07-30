using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.Entities;
using DisCatSharp.ApplicationCommands.Enums;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.ApplicationCommands.Exceptions;
using DisCatSharp.ApplicationCommands.Workers;
using DisCatSharp.Common;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Enums.Core;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

// ReSharper disable HeuristicUnreachableCode

namespace DisCatSharp.ApplicationCommands;

/// <summary>
///     A class that handles slash commands for a client.
/// </summary>
public sealed class ApplicationCommandsExtension : BaseExtension
{
	/// <summary>
	///     Configuration for Discord.
	/// </summary>
	internal static ApplicationCommandsConfiguration Configuration { get; set; }

	/// <summary>
	///     Sets a list of registered commands. The key is the guild id (null if global).
	/// </summary>
	private static readonly List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> s_registeredCommands = [];

	/// <summary>
	///     Sets a list of registered global commands.
	/// </summary>
	internal static readonly List<DiscordApplicationCommand> GlobalCommandsInternal = [];

	/// <summary>
	///     Sets a list of registered guild commands mapped by guild id.
	/// </summary>
	internal static readonly Dictionary<ulong, IReadOnlyList<DiscordApplicationCommand>> GuildCommandsInternal = [];

	/// <summary>
	///     Gets a list of handled interactions. Fix for double interaction execution bug.
	/// </summary>
	internal static readonly List<ulong> HandledInteractions = [];

	/// <summary>
	///     Fires the application command module ready event.
	///     <para>
	///         This is fired when the whole module for the <see cref="DiscordClient" /> finished the startup and
	///         registration.
	///     </para>
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, ApplicationCommandsModuleReadyEventArgs> _applicationCommandsModuleReady;

	/// <summary>
	///     Fires the application command module startup finished event.
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, ApplicationCommandsModuleStartupFinishedEventArgs> _applicationCommandsModuleStartupFinished;

	/// <summary>
	///     Fires the context menu error event.
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, ContextMenuErrorEventArgs> _contextMenuErrored;

	/// <summary>
	///     Fires the context menu executed event.
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, ContextMenuExecutedEventArgs> _contextMenuExecuted;

	/// <summary>
	///     Fires the global application command registered event.
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, GlobalApplicationCommandsRegisteredEventArgs> _globalApplicationCommandsRegistered;

	/// <summary>
	///     Fires the guild application command registered event.
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, GuildApplicationCommandsRegisteredEventArgs> _guildApplicationCommandsRegistered;

	/// <summary>
	///     Fires the slash command error event.
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, SlashCommandErrorEventArgs> _slashError;

	/// <summary>
	///     Fires the slash command executed event.
	/// </summary>
	private readonly AsyncEvent<ApplicationCommandsExtension, SlashCommandExecutedEventArgs> _slashExecuted;

	/// <summary>
	///     List of modules to register.
	/// </summary>
	private readonly List<KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>> _updateList = [];

	/// <summary>
	///     Initializes a new instance of the <see cref="ApplicationCommandsExtension" /> class.
	/// </summary>
	/// <param name="configuration">The configuration.</param>
	internal ApplicationCommandsExtension(ApplicationCommandsConfiguration? configuration = null)
	{
		configuration ??= new();
		Configuration = configuration;
		DebugEnabled = configuration?.DebugStartup ?? false;
		CheckAllGuilds = configuration?.CheckAllGuilds ?? false;
		AutoDeferEnabled = configuration?.AutoDefer ?? false;
		IsCalledByUnitTest = configuration?.UnitTestMode ?? false;

		this._slashError = new("SLASHCOMMAND_ERRORED", TimeSpan.Zero, null!);
		this._slashExecuted = new("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, null!);
		this._contextMenuErrored = new("CONTEXTMENU_ERRORED", TimeSpan.Zero, null!);
		this._contextMenuExecuted = new("CONTEXTMENU_EXECUTED", TimeSpan.Zero, null!);
		this._applicationCommandsModuleReady = new("APPLICATION_COMMANDS_MODULE_READY", TimeSpan.Zero, null!);
		this._applicationCommandsModuleStartupFinished = new("APPLICATION_COMMANDS_MODULE_STARTUP_FINISHED", TimeSpan.Zero, null!);
		this._globalApplicationCommandsRegistered = new("GLOBAL_COMMANDS_REGISTERED", TimeSpan.Zero, null!);
		this._guildApplicationCommandsRegistered = new("GUILD_COMMANDS_REGISTERED", TimeSpan.Zero, null!);
		this.ApplicationCommandsModuleStartupFinished += this.CheckStartupFinishAsync;
	}

	/// <summary>
	///     A list of methods for top level commands.
	/// </summary>
	internal static List<CommandMethod> CommandMethods { get; set; } = [];

	/// <summary>
	///     List of groups.
	/// </summary>
	internal static List<GroupCommand> GroupCommands { get; set; } = [];

	/// <summary>
	///     List of groups with subgroups.
	/// </summary>
	internal static List<SubGroupCommand> SubGroupCommands { get; set; } = [];

	/// <summary>
	///     List of context menus.
	/// </summary>
	internal static List<ContextMenuCommand> ContextMenuCommands { get; set; } = [];

	/// <summary>
	///     List of global commands on discords backend.
	/// </summary>
	internal static List<DiscordApplicationCommand> GlobalDiscordCommands { get; set; } = [];

	/// <summary>
	///     List of guild commands on discords backend.
	/// </summary>
	internal static Dictionary<ulong, List<DiscordApplicationCommand>> GuildDiscordCommands { get; set; } = [];

	/// <summary>
	///     Singleton modules.
	/// </summary>
	private static List<object> s_singletonModules { get; } = [];

	/// <summary>
	///     Set to true if anything fails when registering.
	/// </summary>
	private static bool s_errored { get; set; }

	/// <summary>
	///     Gets a list of registered commands. The key is the guild id (null if global).
	/// </summary>
	public IReadOnlyList<KeyValuePair<ulong?, IReadOnlyList<RegisteredDiscordApplicationCommand>>> RegisteredCommands
		=> s_registeredCommands.Select(guild =>
			new KeyValuePair<ulong?, IReadOnlyList<RegisteredDiscordApplicationCommand>>(guild.Key, guild.Value
				.Select(parent => new RegisteredDiscordApplicationCommand(parent)).ToList())).ToList().AsReadOnly();

	/// <summary>
	///     Gets a list of registered global commands.
	/// </summary>
	public IReadOnlyList<DiscordApplicationCommand> GlobalCommands
		=> GlobalCommandsInternal;

	/// <summary>
	///     Gets a list of registered guild commands mapped by guild id.
	/// </summary>
	public IReadOnlyDictionary<ulong, IReadOnlyList<DiscordApplicationCommand>> GuildCommands
		=> GuildCommandsInternal;

	/// <summary>
	///     Gets the guild ids where the applications.commands scope is missing.
	/// </summary>
	private List<ulong> MISSING_SCOPE_GUILD_IDS { get; set; } = [];

	/// <summary>
	///     Gets the guild ids where the applications.commands scope is missing.
	/// </summary>
	private static List<ulong> s_missingScopeGuildIdsGlobal { get; } = [];

	/// <summary>
	///     Gets whether debug is enabled.
	/// </summary>
	internal static bool DebugEnabled { get; set; }

	/// <summary>
	///     Gets the debug level for the logs.
	/// </summary>
	internal static LogLevel ApplicationCommandsLogLevel
		=> DebugEnabled ? LogLevel.Debug : LogLevel.Trace;

	/// <summary>
	///     Gets the logger.
	/// </summary>
	internal static ILogger Logger { get; set; }

	/// <summary>
	///     Gets whether check through all guilds is enabled.
	/// </summary>
	internal static bool CheckAllGuilds { get; set; }

	/// <summary>
	///     Gets whether the registration check should be manually overridden.
	/// </summary>
	internal static bool ManOr { get; set; }

	/// <summary>
	///     Gets whether interactions should be automatically deffered.
	/// </summary>
	internal static bool AutoDeferEnabled { get; set; }

	/// <summary>
	///     Whether this module finished the startup.
	/// </summary>
	internal bool ShardStartupFinished { get; set; } = false;

	/// <summary>
	///     Whether this module finished the startup.
	/// </summary>
	internal static bool StartupFinished { get; set; } = false;

	/// <summary>
	///     Gets the service provider this module was configured with.
	/// </summary>
	public IServiceProvider Services
		=> Configuration.ServiceProvider;

	/// <summary>
	///     Whether this module is called by an unit test.
	/// </summary>
	internal static bool IsCalledByUnitTest { get; set; } = false;

	/// <summary>
	///     Gets the shard count.
	/// </summary>
	internal static int ShardCount { get; set; } = 1;

	/// <summary>
	///     Gets the count of shards who finished initializing the module.
	/// </summary>
	internal static int FinishedShardCount { get; set; } = 0;

	/// <summary>
	///     Gets whether the finish event was fired.
	/// </summary>
	public static bool FinishFired { get; set; } = false;

	internal static DiscordApplicationCommand? EntryPointCommand { get; set; } = null;

	/// <summary>
	///     Runs setup.
	///     <note type="caution">DO NOT RUN THIS MANUALLY. DO NOT DO ANYTHING WITH THIS.</note>
	/// </summary>
	/// <param name="client">The client to setup on.</param>
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;
		ShardCount = client.ShardCount;
		Logger = client.Logger;

		this.Client.Ready += (c, e) =>
		{
			if (!this.ShardStartupFinished)
				_ = Task.Run(async () => await this.UpdateAsync().ConfigureAwait(false));
			return Task.CompletedTask;
		};
		this.Client.InteractionCreated += this.CatchInteractionsOnStartup;
		this.Client.ContextMenuInteractionCreated += this.CatchContextMenuInteractionsOnStartup;
	}

	/// <summary>
	///     Catches all interactions during the startup.
	/// </summary>
	/// <param name="sender">The client.</param>
	/// <param name="e">The interaction create event args.</param>
	/// <returns></returns>
	private async Task CatchInteractionsOnStartup(DiscordClient sender, InteractionCreateEventArgs e)
	{
		if (!this.ShardStartupFinished)
			await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Attention: This application is still starting up. Application commands are unavailable for now.")).ConfigureAwait(false);
		else
			await Task.Delay(1).ConfigureAwait(false);
	}

	/// <summary>
	///     Catches all context menu interactions during the startup.
	/// </summary>
	/// <param name="sender">The client.</param>
	/// <param name="e">The context menu interaction create event args.</param>
	private async Task CatchContextMenuInteractionsOnStartup(DiscordClient sender, ContextMenuInteractionCreateEventArgs e)
	{
		if (!this.ShardStartupFinished)
			await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Attention: This application is still starting up. Context menu commands are unavailable for now.")).ConfigureAwait(false);
		else
			await Task.Delay(1).ConfigureAwait(false);
	}

	/// <summary>
	///     Fired when the startup is completed.
	///     <para>
	///         Switches the interaction handling from <see cref="CatchInteractionsOnStartup" /> and
	///         <see cref="CatchContextMenuInteractionsOnStartup" /> to <see cref="InteractionHandler" /> and
	///         <see cref="ContextMenuHandler" />.
	///     </para>
	/// </summary>
	private void FinishedRegistration()
	{
		this.Client.InteractionCreated -= this.CatchInteractionsOnStartup;
		this.Client.ContextMenuInteractionCreated -= this.CatchContextMenuInteractionsOnStartup;

		this.Client.InteractionCreated += this.InteractionHandler;
		this.Client.ContextMenuInteractionCreated += this.ContextMenuHandler;
	}

	/// <summary>
	///     Cleans the module for a new start of the bot.
	///     DO NOT USE IF YOU DON'T KNOW WHAT IT DOES.
	/// </summary>
	public void CleanModule()
	{
		this._updateList.Clear();
		s_singletonModules.Clear();
		s_errored = false;
		ShardCount = 1;
		CommandMethods.Clear();
		GroupCommands.Clear();
		ContextMenuCommands.Clear();
		SubGroupCommands.Clear();
		s_singletonModules.Clear();
		s_registeredCommands.Clear();
		GlobalCommandsInternal.Clear();
		GuildCommandsInternal.Clear();
		s_missingScopeGuildIdsGlobal.Clear();
		this.MISSING_SCOPE_GUILD_IDS.Clear();
		HandledInteractions.Clear();
		FinishedShardCount = 0;
		FinishFired = false;
		StartupFinished = false;
		this.ShardStartupFinished = false;
		this.Client.InteractionCreated += this.CatchInteractionsOnStartup;
		this.Client.ContextMenuInteractionCreated += this.CatchContextMenuInteractionsOnStartup;
	}

	/// <summary>
	///     Cleans all guild application commands.
	///     <note type="caution">You normally don't need to execute it.</note>
	/// </summary>
	public async Task CleanGuildCommandsAsync()
	{
		foreach (var guild in this.Client.Guilds.Values)
			await this.Client.BulkOverwriteGuildApplicationCommandsAsync(guild.Id, Array.Empty<DiscordApplicationCommand>()).ConfigureAwait(false);
	}

	/// <summary>
	///     Cleans all global application commands.
	///     <note type="caution">You normally don't need to execute it.</note>
	/// </summary>
	public async Task CleanGlobalCommandsAsync()
		=> await this.Client.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>()).ConfigureAwait(false);

	/// <summary>
	///     Registers all commands from a given assembly. The command classes need to be public to be considered for
	///     registration.
	/// </summary>
	/// <param name="assembly">Assembly to register commands from.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	public void RegisterGuildCommands(Assembly assembly, ulong guildId)
	{
		var types = assembly.GetTypes().Where(xt =>
		{
			var xti = xt.GetTypeInfo();
			return xti.IsModuleCandidateType() && !xti.IsNested;
		});
		foreach (var xt in types)
			this.RegisterGuildCommands(xt, guildId);
	}

	/// <summary>
	///     Registers all commands from a given assembly. The command classes need to be public to be considered for
	///     registration.
	/// </summary>
	/// <param name="assembly">Assembly to register commands from.</param>
	public void RegisterGlobalCommands(Assembly assembly)
	{
		var types = assembly.GetTypes().Where(xt =>
		{
			var xti = xt.GetTypeInfo();
			return xti.IsModuleCandidateType() && !xti.IsNested;
		});
		foreach (var xt in types)
			this.RegisterGlobalCommands(xt);
	}

	/// <summary>
	///     Registers a command class with optional translation setup for a guild.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGuildCommands<T>(ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : ApplicationCommandsModule
		=> this._updateList.Add(new(guildId, new(typeof(T), translationSetup)));

	/// <summary>
	///     Registers a command class with optional translation setup for a guild.
	/// </summary>
	/// <param name="type">The <see cref="System.Type" /> of the command class to register.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGuildCommands(Type type, ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));

		this._updateList.Add(new(guildId, new(type, translationSetup)));
	}

	/// <summary>
	///     Registers a command class with optional translation setup globally.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGlobalCommands<T>(Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : ApplicationCommandsModule
		=> this._updateList.Add(new(null, new(typeof(T), translationSetup)));

	/// <summary>
	///     Registers a command class with optional translation setup globally.
	/// </summary>
	/// <param name="type">The <see cref="System.Type" /> of the command class to register.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGlobalCommands(Type type, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));

		this._updateList.Add(new(null, new(type, translationSetup)));
	}

	/// <summary>
	///     Fired when the application commands module is ready.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ApplicationCommandsModuleReadyEventArgs> ApplicationCommandsModuleReady
	{
		add => this._applicationCommandsModuleReady.Register(value);
		remove => this._applicationCommandsModuleReady.Unregister(value);
	}

	/// <summary>
	///     Fired when the application commands modules startup is finished.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ApplicationCommandsModuleStartupFinishedEventArgs> ApplicationCommandsModuleStartupFinished
	{
		add => this._applicationCommandsModuleStartupFinished.Register(value);
		remove => this._applicationCommandsModuleStartupFinished.Unregister(value);
	}

	/// <summary>
	///     Fired when guild commands are registered on a guild.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, GuildApplicationCommandsRegisteredEventArgs> GuildApplicationCommandsRegistered
	{
		add => this._guildApplicationCommandsRegistered.Register(value);
		remove => this._guildApplicationCommandsRegistered.Unregister(value);
	}

	/// <summary>
	///     Fired when the global commands are registered.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, GlobalApplicationCommandsRegisteredEventArgs> GlobalApplicationCommandsRegistered
	{
		add => this._globalApplicationCommandsRegistered.Register(value);
		remove => this._globalApplicationCommandsRegistered.Unregister(value);
	}

	/// <summary>
	///     Used for RegisterCommands and the <see cref="DisCatSharp.DiscordClient.Ready" /> event.
	/// </summary>
	internal async Task UpdateAsync()
	{
		this.Client.Logger.Log(LogLevel.Information, "Request to register commands on shard {shard}", this.Client.ShardId);

		try
		{
			if (this.ShardStartupFinished)
			{
				this.Client.Logger.Log(LogLevel.Information, "Shard {shard} already setup, skipping", this.Client.ShardId);
				return;
			}

			GlobalDiscordCommands = [];
			GuildDiscordCommands = [];

			this.Client.Logger.Log(ApplicationCommandsLogLevel, "Shard {shard} has {guilds} guilds", this.Client.ShardId, this.Client.ReadyGuildIds.Count);
			List<ulong> failedGuilds = [];
			var globalCommands = IsCalledByUnitTest ? null : (await this.Client.GetGlobalApplicationCommandsAsync(Configuration?.EnableLocalization ?? false).ConfigureAwait(false))?.ToList() ?? null;

			var guilds = CheckAllGuilds ? this.Client.ReadyGuildIds : this._updateList.Where(x => x.Key is not null)?.Select(x => x.Key!.Value).Distinct().ToList();
			var wrongShards = guilds is not null && this.Client.ReadyGuildIds.Count is not 0 ? guilds.Where(x => !this.Client.ReadyGuildIds.Contains(x)).ToList() : [];
			if (wrongShards.Count is not 0)
			{
				this.Client.Logger.Log(ApplicationCommandsLogLevel, "Some guilds are not on the same shard as the client. Removing them from the update list");
				foreach (var guild in wrongShards)
				{
					this._updateList.RemoveAll(x => x.Key == guild);
					guilds?.Remove(guild);
				}
			}

			var commandsPending = this._updateList.Select(x => x.Key).Distinct().ToList();

			if (guilds is not null && guilds.Count != 0)
				foreach (var guild in guilds)
				{
					List<DiscordApplicationCommand>? commands = null;
					var unauthorized = false;
					try
					{
						commands = (await this.Client.GetGuildApplicationCommandsAsync(guild, Configuration?.EnableLocalization ?? false).ConfigureAwait(false)).ToList() ?? null;
					}
					catch (UnauthorizedException)
					{
						unauthorized = true;
					}
					finally
					{
						switch (unauthorized)
						{
							case false when commands is not null && commands.Count is not 0:
								GuildDiscordCommands.Add(guild, [.. commands]);
								break;
							case true:
								failedGuilds.Add(guild);
								break;
						}
					}
				}

			//Default should be to add the help and slash commands can be added without setting any configuration
			//so this should still add the default help
			if (Configuration is null || (Configuration is not null && Configuration.EnableDefaultHelp))
			{
				this._updateList.Add(new(null, new(typeof(DefaultHelpModule))));
				commandsPending = this._updateList.Select(x => x.Key).Distinct().ToList();
			}
			else if (Configuration is not null && Configuration.EnableDefaultUserAppsHelp)
			{
				this._updateList.Add(new(null, new(typeof(DefaultUserAppsHelpModule))));
				commandsPending = this._updateList.Select(x => x.Key).Distinct().ToList();
			}
			else
			{
				try
				{
					this._updateList.Remove(new(null, new(typeof(DefaultHelpModule))));
				}
				catch
				{ }

				commandsPending = this._updateList.Select(x => x.Key).Distinct().ToList();
			}

			if (globalCommands is not null && globalCommands.Count is not 0)
			{
				GlobalDiscordCommands.AddRange(globalCommands);
				if (this.Client.Configuration.HasActivitiesEnabled)
				{
					var entryPointCommand = globalCommands.First(command => command.Name == "launch");
					EntryPointCommand = entryPointCommand;
				}
			}

			foreach (var key in commandsPending)
			{
				this.Client.Logger.Log(ApplicationCommandsLogLevel, key.HasValue ? $"Registering commands in guild {key.Value}" : "Registering global commands");
				if (key.HasValue)
				{
					this.Client.Logger.Log(ApplicationCommandsLogLevel, "Found guild {guild} in shard {shard}!", key.Value, this.Client.ShardId);
					this.Client.Logger.Log(ApplicationCommandsLogLevel, "Registering");
				}

				await this.RegisterCommands(this._updateList.Where(x => x.Key == key).Select(x => x.Value).ToList(), key).ConfigureAwait(false);
			}

			this.MISSING_SCOPE_GUILD_IDS = [.. failedGuilds];
			s_missingScopeGuildIdsGlobal.AddRange(failedGuilds);
			this.ShardStartupFinished = true;
			FinishedShardCount++;

			StartupFinished = FinishedShardCount == ShardCount;

			this.Client.Logger.Log(LogLevel.Information, "Application command setup finished for shard {ShardId}, enabling receiving", this.Client.ShardId);
			await this._applicationCommandsModuleStartupFinished.InvokeAsync(this, new(Configuration.ServiceProvider)
			{
				RegisteredGlobalCommands = GlobalCommandsInternal,
				RegisteredGuildCommands = GuildCommandsInternal,
				GuildsWithoutScope = this.MISSING_SCOPE_GUILD_IDS,
				ShardId = this.Client.ShardId
			}).ConfigureAwait(false);
			this.FinishedRegistration();
		}
		catch (Exception ex)
		{
			this.Client.Logger.LogCritical(ex, "There was an error during the application commands setup");
			this.Client.Logger.LogError(ex.Message);
			this.Client.Logger.LogError(ex.StackTrace);
		}
	}

	/// <summary>
	///     Method for registering commands for a target from modules.
	/// </summary>
	/// <param name="types">The types.</param>
	/// <param name="guildId">The optional guild id.</param>
	private async Task RegisterCommands(List<ApplicationCommandsModuleConfiguration> types, ulong? guildId)
	{
		this.Client.Logger.Log(LogLevel.Information, "Registering commands on shard {shard}", this.Client.ShardId);
		//Initialize empty lists to be added to the global ones at the end
		var commandMethods = new List<CommandMethod>();
		var groupCommands = new List<GroupCommand>();
		var subGroupCommands = new List<SubGroupCommand>();
		var contextMenuCommands = new List<ContextMenuCommand>();
		var updateList = new List<DiscordApplicationCommand>();

		var commandTypeSources = new List<KeyValuePair<Type, Type>>();
		var groupTranslation = new List<GroupTranslator>();
		var translation = new List<CommandTranslator>();

		List<DiscordApplicationCommand> unitTestCommands = [];

		//Iterates over all the modules
		foreach (var config in types)
		{
			var type = config.Type;
			try
			{
				var module = type.GetTypeInfo();
				var classes = new List<TypeInfo>();

				var ctx = new ApplicationCommandsTranslationContext(type, module.FullName);
				config.Translations?.Invoke(ctx);

				//Add module to classes list if it's a group
				var extremeNestedGroup = false;
				if (module.GetCustomAttribute<SlashCommandGroupAttribute>() is not null)
					classes.Add(module);
				else if (module.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Any(x => x.IsDefined(typeof(SlashCommandGroupAttribute))))
				{
					//Otherwise add the extreme nested groups
					classes = module.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
						.Where(x => x.IsDefined(typeof(SlashCommandGroupAttribute)))
						.Select(x => module.GetNestedType(x.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).GetTypeInfo()).ToList();
					extremeNestedGroup = true;
				}
				else
					//Otherwise add the nested groups
					classes = module.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();

				if (module.GetCustomAttribute<SlashCommandGroupAttribute>() is not null || extremeNestedGroup)
				{
					List<GroupTranslator> groupTranslations = null;

					if (!string.IsNullOrEmpty(ctx.GroupTranslations))
						groupTranslations = JsonConvert.DeserializeObject<List<GroupTranslator>>(ctx.GroupTranslations)!;

					var slashGroupsTuple = await NestedCommandWorker.ParseSlashGroupsAsync(type, classes, guildId, groupTranslations).ConfigureAwait(false);

					if (slashGroupsTuple.applicationCommands is not null && slashGroupsTuple.applicationCommands.Count is not 0)
					{
						if (IsCalledByUnitTest)
							unitTestCommands.AddRange(slashGroupsTuple.applicationCommands);
						updateList.AddRange(slashGroupsTuple.applicationCommands);
						if (Configuration.GenerateTranslationFilesOnly)
						{
							var cgwsgs = new List<CommandGroupWithSubGroups>();
							foreach (var cmd in slashGroupsTuple.applicationCommands)
								if (cmd.Type is ApplicationCommandType.ChatInput)
								{
									var cgs = new List<CommandGroup>();
									var cs2 = new List<Command>();
									if (cmd.Options is not null)
									{
										foreach (var scg in cmd.Options.Where(x => x.Type is ApplicationCommandOptionType.SubCommandGroup))
										{
											var cs = new List<Command>();
											if (scg.Options is not null)
												foreach (var sc in scg.Options)
													if (sc.Options is null || sc.Options.Count is 0)
														cs.Add(new(sc.Name, sc.Description, null, null, sc.RawNameLocalizations, sc.RawDescriptionLocalizations));
													else
														cs.Add(new(sc.Name, sc.Description, [.. sc.Options], null, sc.RawNameLocalizations, sc.RawDescriptionLocalizations));
											cgs.Add(new(scg.Name, scg.Description, cs, null, scg.RawNameLocalizations, scg.RawDescriptionLocalizations));
										}

										foreach (var sc2 in cmd.Options.Where(x => x.Type is ApplicationCommandOptionType.SubCommand))
											if (sc2.Options == null || sc2.Options.Count == 0)
												cs2.Add(new(sc2.Name, sc2.Description, null, null, sc2.RawNameLocalizations, sc2.RawDescriptionLocalizations));
											else
												cs2.Add(new(sc2.Name, sc2.Description, [.. sc2.Options], null, sc2.RawNameLocalizations, sc2.RawDescriptionLocalizations));
									}

									cgwsgs.Add(new(cmd.Name, cmd.Description, cgs, cs2, cmd.Type, cmd.RawNameLocalizations, cmd.RawDescriptionLocalizations));
								}

							if (cgwsgs.Count is not 0)
								groupTranslation.AddRange(cgwsgs.Select(cgwsg => JsonConvert.DeserializeObject<GroupTranslator>(JsonConvert.SerializeObject(cgwsg))!));
						}
					}

					if (slashGroupsTuple.commandTypeSources is not null && slashGroupsTuple.commandTypeSources.Count is not 0)
						commandTypeSources.AddRange(slashGroupsTuple.commandTypeSources);

					if (slashGroupsTuple.singletonModules is not null && slashGroupsTuple.singletonModules.Count is not 0)
						s_singletonModules.AddRange(slashGroupsTuple.singletonModules);

					if (slashGroupsTuple.groupCommands is not null && slashGroupsTuple.groupCommands.Count is not 0)
						groupCommands.AddRange(slashGroupsTuple.groupCommands);

					if (slashGroupsTuple.subGroupCommands is not null && slashGroupsTuple.subGroupCommands.Count is not 0)
						subGroupCommands.AddRange(slashGroupsTuple.subGroupCommands);
				}

				//Handles methods and context menus, only if the module isn't a group itself
				if (module.GetCustomAttribute<SlashCommandGroupAttribute>() is null)
				{
					List<CommandTranslator>? commandTranslations = null;

					if (!string.IsNullOrEmpty(ctx.SingleTranslations))
						commandTranslations = JsonConvert.DeserializeObject<List<CommandTranslator>>(ctx.SingleTranslations);

					//Slash commands
					var methods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() is not null);

					var slashCommands = await CommandWorker.ParseBasicSlashCommandsAsync(type, methods, guildId, commandTranslations).ConfigureAwait(false);

					if (slashCommands.applicationCommands is not null && slashCommands.applicationCommands.Count is not 0)
					{
						if (IsCalledByUnitTest)
							unitTestCommands.AddRange(slashCommands.applicationCommands);
						updateList.AddRange(slashCommands.applicationCommands);
						if (Configuration.GenerateTranslationFilesOnly)
						{
							var cs = new List<Command>();
							foreach (var cmd in slashCommands.applicationCommands.Where(cmd => cmd.Type is ApplicationCommandType.ChatInput && (cmd.Options is null || !cmd.Options.Any(x => x.Type is ApplicationCommandOptionType.SubCommand or ApplicationCommandOptionType.SubCommandGroup))))
								if (cmd.Options == null || cmd.Options.Count == 0)
									cs.Add(new(cmd.Name, cmd.Description, null, ApplicationCommandType.ChatInput, cmd.RawNameLocalizations, cmd.RawDescriptionLocalizations));
								else
									cs.Add(new(cmd.Name, cmd.Description, [.. cmd.Options], ApplicationCommandType.ChatInput, cmd.RawNameLocalizations, cmd.RawDescriptionLocalizations));

							if (cs.Count is not 0)
								//translation.AddRange(cs.Select(c => JsonConvert.DeserializeObject<CommandTranslator>(JsonConvert.SerializeObject(c))!));
								foreach (var c in cs)
								{
									var json = JsonConvert.SerializeObject(c);
									var obj = JsonConvert.DeserializeObject<CommandTranslator>(json);
									translation.Add(obj!);
								}
						}
					}

					if (slashCommands.commandTypeSources is not null && slashCommands.commandTypeSources.Count is not 0)
						commandTypeSources.AddRange(slashCommands.commandTypeSources);

					if (slashCommands.commandMethods is not null && slashCommands.commandMethods.Count is not 0)
						commandMethods.AddRange(slashCommands.commandMethods);

					//Context Menus
					var contextMethods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() is not null);

					var contextCommands = await CommandWorker.ParseContextMenuCommands(type, contextMethods, commandTranslations).ConfigureAwait(false);

					if (contextCommands.applicationCommands is not null && contextCommands.applicationCommands.Count is not 0)
					{
						if (IsCalledByUnitTest)
							unitTestCommands.AddRange(contextCommands.applicationCommands);
						updateList.AddRange(contextCommands.applicationCommands);
						if (Configuration.GenerateTranslationFilesOnly)
						{
							var cs = new List<Command>();
							foreach (var cmd in contextCommands.applicationCommands)
								if (cmd.Type is ApplicationCommandType.Message or ApplicationCommandType.User)
									cs.Add(new(cmd.Name, null, null, cmd.Type));
							if (cs.Count != 0)
								translation.AddRange(cs.Select(c => JsonConvert.DeserializeObject<CommandTranslator>(JsonConvert.SerializeObject(c))!));
						}
					}

					if (contextCommands.commandTypeSources is not null && contextCommands.commandTypeSources.Count is not 0)
						commandTypeSources.AddRange(contextCommands.commandTypeSources);

					if (contextCommands.contextMenuCommands is not null && contextCommands.contextMenuCommands.Count is not 0)
						contextMenuCommands.AddRange(contextCommands.contextMenuCommands);

					//Accounts for lifespans
					if (module.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() is not null && module.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan is ApplicationCommandModuleLifespan.Singleton)
						s_singletonModules.Add(CreateInstance(module, Configuration.ServiceProvider));
				}
			}
			catch (NullReferenceException ex)
			{
				this.Client.Logger.LogCritical(ex, "NRE Exception thrown: {msg}\nStack: {stack}", ex.Message, ex.StackTrace);
			}
			catch (Exception ex)
			{
				if (ex is BadRequestException brex)
					this.Client.Logger.LogCritical(brex, @"There was an error registering application commands: {res}", brex.WebResponse.Response);
				else
				{
					if (ex.InnerException is BadRequestException brex1)
						this.Client.Logger.LogCritical(brex1, @"There was an error registering application commands: {res}", brex1.WebResponse.Response);
					else
						this.Client.Logger.LogCritical(ex, @"There was an error parsing the application commands");
				}

				s_errored = true;
			}
		}

		if (!s_errored && !IsCalledByUnitTest)
		{
			updateList = updateList.DistinctBy(x => x.Name).ToList();
			if (Configuration.GenerateTranslationFilesOnly)
				await this.CheckRegistrationStartup(translation, groupTranslation, guildId);
			else
				try
				{
					List<DiscordApplicationCommand> commands = [];

					try
					{
						if (guildId is null)
						{
							if (updateList.Count is not 0)
							{
								var regCommands = await RegistrationWorker.RegisterGlobalCommandsAsync(this.Client, updateList, EntryPointCommand).ConfigureAwait(false);
								if (regCommands is not null)
								{
									var actualCommands = regCommands.Distinct().ToList();
									commands.AddRange(actualCommands);
									GlobalCommandsInternal.AddRange(actualCommands);
								}
							}
							else if (GlobalDiscordCommands.Count is not 0)
								foreach (var cmd in GlobalDiscordCommands)
									try
									{
										if (EntryPointCommand is null || cmd.Name is not "launch")
											await this.Client.DeleteGlobalApplicationCommandAsync(cmd.Id).ConfigureAwait(false);
									}
									catch (NotFoundException)
									{
										this.Client.Logger.Log(ApplicationCommandsLogLevel, "Could not delete global command {cmdId}. Please clean up manually", cmd.Id);
									}
						}
						else
						{
							if (updateList.Count is not 0)
							{
								var regCommands = await RegistrationWorker.RegisterGuildCommandsAsync(this.Client, guildId.Value, updateList).ConfigureAwait(false);
								if (regCommands is not null)
								{
									var actualCommands = regCommands.Distinct().ToList();
									commands.AddRange(actualCommands);
									GuildCommandsInternal.Add(guildId.Value, actualCommands);
									try
									{
										if (this.Client.Guilds.TryGetValue(guildId.Value, out var guild))
											guild.InternalRegisteredApplicationCommands.AddRange(actualCommands);
									}
									catch (NullReferenceException)
									{ }
								}
							}
							else if (GuildDiscordCommands.Count is not 0)
								foreach (var cmd in GuildDiscordCommands.First(x => x.Key == guildId.Value).Value)
									try
									{
										await this.Client.DeleteGuildApplicationCommandAsync(guildId.Value, cmd.Id).ConfigureAwait(false);
									}
									catch (NotFoundException)
									{
										this.Client.Logger.Log(ApplicationCommandsLogLevel, "Could not delete guild command {cmdId} in guild {guildId}. Please clean up manually", cmd.Id, guildId.Value);
									}
						}
					}
					catch (UnauthorizedException ex)
					{
						this.Client.Logger.LogError("Could not register application commands for guild {guildId}.\nError: {exc}", guildId, ex.JsonMessage);
						return;
					}

					//Creates a guild command if a guild id is specified, otherwise global
					//Checks against the ids and adds them to the command method lists
					foreach (var command in commands)
					{
						if (commandMethods!.TryGetFirstValueWhere(x => x.Name == command.Name, out var com))
							com.CommandId = command.Id;
						if (groupCommands!.TryGetFirstValueWhere(x => x.Name == command.Name, out var groupCom))
							groupCom.CommandId = command.Id;
						if (subGroupCommands!.TryGetFirstValueWhere(x => x.Name == command.Name, out var subCom))
							subCom.CommandId = command.Id;
						if (contextMenuCommands!.TryGetFirstValueWhere(x => x.Name == command.Name, out var cmCom))
							cmCom.CommandId = command.Id;
					}

					//Adds to the global lists finally
					CommandMethods.AddRange(commandMethods.DistinctBy(x => x.Name));
					GroupCommands.AddRange(groupCommands.DistinctBy(x => x.Name));
					SubGroupCommands.AddRange(subGroupCommands.DistinctBy(x => x.Name));
					ContextMenuCommands.AddRange(contextMenuCommands.DistinctBy(x => x.Name));

					s_registeredCommands.Add(new(guildId, commands.ToList()));

					foreach (var app in commandMethods.Select(command => types.First(t => t.Type == command.Method.DeclaringType)))
					{ }

					if (guildId.HasValue)
						await this._guildApplicationCommandsRegistered.InvokeAsync(this, new(Configuration.ServiceProvider)
						{
							Handled = true,
							GuildId = guildId.Value,
							RegisteredCommands = GuildCommandsInternal.FirstOrDefault(c => c.Key == guildId.Value).Value ?? []
						}).ConfigureAwait(false);
					else
						await this._globalApplicationCommandsRegistered.InvokeAsync(this, new(Configuration.ServiceProvider)
						{
							Handled = true,
							RegisteredCommands = GlobalCommandsInternal
						}).ConfigureAwait(false);

					await this.CheckRegistrationStartup(translation, groupTranslation, guildId);
				}
				catch (NullReferenceException ex)
				{
					this.Client.Logger.LogCritical(ex, "NRE Exception thrown: {msg}\nStack: {stack}", ex.Message, ex.StackTrace);
				}
				catch (Exception ex)
				{
					if (ex is BadRequestException brex)
						this.Client.Logger.LogCritical(brex, @"There was an error registering application commands: {res}", brex.WebResponse.Response);
					else
					{
						if (ex.InnerException is BadRequestException brex1)
							this.Client.Logger.LogCritical(brex1, @"There was an error registering application commands: {res}", brex1.WebResponse.Response);
						else
							this.Client.Logger.LogCritical(ex, @"There was an general error registering application commands");
					}

					s_errored = true;
				}
		}
		else if (IsCalledByUnitTest)
		{
			CommandMethods.AddRange(commandMethods.DistinctBy(x => x.Name));
			GroupCommands.AddRange(groupCommands.DistinctBy(x => x.Name));
			SubGroupCommands.AddRange(subGroupCommands.DistinctBy(x => x.Name));
			ContextMenuCommands.AddRange(contextMenuCommands.DistinctBy(x => x.Name));

			s_registeredCommands.Add(new(guildId, unitTestCommands.ToList()));

			foreach (var app in commandMethods.Select(command => types.First(t => t.Type == command.Method.DeclaringType)))
			{ }

			if (guildId.HasValue)
				await this._guildApplicationCommandsRegistered.InvokeAsync(this, new(Configuration.ServiceProvider)
				{
					Handled = true,
					GuildId = guildId.Value,
					RegisteredCommands = GuildCommandsInternal.FirstOrDefault(c => c.Key == guildId.Value).Value ?? []
				}).ConfigureAwait(false);
			else
				await this._globalApplicationCommandsRegistered.InvokeAsync(this, new(Configuration.ServiceProvider)
				{
					Handled = true,
					RegisteredCommands = GlobalCommandsInternal
				}).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Checks the registration startup.
	/// </summary>
	/// <param name="translation">The optional translations.</param>
	/// <param name="groupTranslation">The optional group translations.</param>
	/// <param name="guildId">The optional guild id.</param>
	private async Task CheckRegistrationStartup(List<CommandTranslator>? translation = null, List<GroupTranslator>? groupTranslation = null, ulong? guildId = null)
	{
		if (Configuration.GenerateTranslationFilesOnly)
		{
			try
			{
				if (translation is not null && translation.Count is not 0)
				{
					var fileName = $"translation_generator_export-shard{this.Client.ShardId}-SINGLE-{(guildId.HasValue ? guildId.Value : "global")}.json";
					var fs = File.Create(fileName);
					var ms = new MemoryStream();
					var writer = new StreamWriter(ms);
					await writer.WriteAsync(JsonConvert.SerializeObject(translation.DistinctBy(x => x.Name), Formatting.Indented)).ConfigureAwait(false);
					await writer.FlushAsync().ConfigureAwait(false);
					ms.Position = 0;
					await ms.CopyToAsync(fs).ConfigureAwait(false);
					await fs.FlushAsync().ConfigureAwait(false);
					fs.Close();
					await fs.DisposeAsync().ConfigureAwait(false);
					ms.Close();
					await ms.DisposeAsync().ConfigureAwait(false);
					this.Client.Logger.LogInformation("Exported base translation to {exppath}", fileName);
				}

				if (groupTranslation is not null && groupTranslation.Count is not 0)
				{
					var fileName = $"translation_generator_export-shard{this.Client.ShardId}-GROUP-{(guildId.HasValue ? guildId.Value : "global")}.json";
					var fs = File.Create(fileName);
					var ms = new MemoryStream();
					var writer = new StreamWriter(ms);
					await writer.WriteAsync(JsonConvert.SerializeObject(groupTranslation.DistinctBy(x => x.Name), Formatting.Indented)).ConfigureAwait(false);
					await writer.FlushAsync().ConfigureAwait(false);
					ms.Position = 0;
					await ms.CopyToAsync(fs).ConfigureAwait(false);
					await fs.FlushAsync().ConfigureAwait(false);
					fs.Close();
					await fs.DisposeAsync().ConfigureAwait(false);
					ms.Close();
					await ms.DisposeAsync().ConfigureAwait(false);
					this.Client.Logger.LogInformation("Exported base translation to {exppath}", fileName);
				}
			}
			catch (Exception ex)
			{
				this.Client.Logger.LogError(@"{msg}", ex.Message);
				this.Client.Logger.LogError(@"{stack}", ex.StackTrace);
			}

			await this.Client.DisconnectAsync().ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Checks whether we finished up the complete startup.
	/// </summary>
	private async Task CheckStartupFinishAsync(ApplicationCommandsExtension sender, ApplicationCommandsModuleStartupFinishedEventArgs args)
	{
		if (StartupFinished)
			if (!FinishFired)
			{
				await this._applicationCommandsModuleReady.InvokeAsync(sender, new(Configuration.ServiceProvider)
				{
					GuildsWithoutScope = s_missingScopeGuildIdsGlobal
				}).ConfigureAwait(false);
				FinishFired = true;
				if (Configuration.GenerateTranslationFilesOnly)
					Environment.Exit(0);
			}

		args.Handled = false;
	}

	/// <summary>
	///     Interaction handler.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The event args.</param>
	private Task InteractionHandler(DiscordClient client, InteractionCreateEventArgs e)
	{
		this.Client.Logger.Log(ApplicationCommandsLogLevel, "Got slash interaction on shard {shard}", this.Client.ShardId);
		if (HandledInteractions.Contains(e.Interaction.Id) || (e.Interaction is { GuildId: not null, AuthorizingIntegrationOwners.GuildInstallKey: not null } && !client.Guilds.ContainsKey(e.Interaction.GuildId.Value)))
		{
			this.Client.Logger.Log(ApplicationCommandsLogLevel, "Ignoring, already received or wrong shard");
			return Task.FromResult(true);
		}

		HandledInteractions.Add(e.Interaction.Id);

		_ = Task.Run(async () =>
		{
			var type = GetInteractionType(e.Interaction.Data);

			switch (e.Interaction.Type)
			{
				case InteractionType.ApplicationCommand:
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
						Services = Configuration.ServiceProvider,
						ResolvedUserMentions = e.Interaction.Data.Resolved?.Users?.Values.ToList() ?? [],
						ResolvedRoleMentions = e.Interaction.Data.Resolved?.Roles?.Values.ToList() ?? [],
						ResolvedChannelMentions = e.Interaction.Data.Resolved?.Channels?.Values.ToList() ?? [],
						ResolvedAttachments = e.Interaction.Data.Resolved?.Attachments?.Values.ToList() ?? [],
						Type = ApplicationCommandType.ChatInput,
						Locale = e.Interaction.Locale,
						GuildLocale = e.Interaction.GuildLocale,
						AppPermissions = e.Interaction.AppPermissions,
						Entitlements = e.Interaction.Entitlements,
						UserId = e.Interaction.User.Id,
						GuildId = e.Interaction.GuildId,
						MemberId = e.Interaction.GuildId is not null ? e.Interaction.User.Id : null,
						ChannelId = e.Interaction.ChannelId,
						AttachmentSizeLimit = e.Interaction.AttachmentSizeLimit
					};

					try
					{
						if (s_errored)
						{
							await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Application commands failed to register properly on startup.").AsEphemeral()).ConfigureAwait(false);
							throw new InvalidOperationException("Application commands failed to register properly on startup.");
						}

						var methods = CommandMethods.Where(x => x.CommandId == e.Interaction.Data.Id).ToList();
						var groups = GroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id).ToList();
						var subgroups = SubGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id).ToList();
						if (methods.Count is 0 && groups.Count is 0 && subgroups.Count is 0)
						{
							await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("An application command was executed, but no command was registered for it.").AsEphemeral()).ConfigureAwait(false);
							throw new InvalidOperationException($"An application command was executed, but no command was registered for it.\n\tCommand name: {e.Interaction.Data.Name}\n\tCommand ID: {e.Interaction.Data.Id}");
						}

						switch (type)
						{
							case ApplicationCommandFinalType.Command when methods.Count is not 0:
							{
								var method = methods.First().Method;
								context.SubCommandName = null;
								context.SubSubCommandName = null;
								if (DebugEnabled)
									this.Client.Logger.LogDebug("Executing {cmd}", method.Name);
								var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options).ConfigureAwait(false);

								await this.RunCommandAsync(context, method, args).ConfigureAwait(false);
								break;
							}
							case ApplicationCommandFinalType.SubCommand when groups.Count is not 0:
							{
								var command = e.Interaction.Data.Options[0];
								var method = groups.First().Methods.First(x => x.Key == command.Name).Value;
								context.SubCommandName = command.Name;
								context.SubSubCommandName = null;
								if (DebugEnabled)
									this.Client.Logger.LogDebug("Executing {cmd}", method.Name);
								var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options[0].Options).ConfigureAwait(false);

								await this.RunCommandAsync(context, method, args).ConfigureAwait(false);
								break;
							}
							case ApplicationCommandFinalType.SubCommandGroup when subgroups.Count is not 0:
							{
								var command = e.Interaction.Data.Options[0];
								var group = subgroups.First().SubCommands.First(x => x.Name == command.Name);

								var method = group.Methods.First(x => x.Key == command.Options[0].Name).Value;
								context.SubCommandName = command.Name;
								context.SubSubCommandName = command.Options[0].Name;

								if (DebugEnabled)
									this.Client.Logger.LogDebug("Executing {cmd}", method.Name);
								var args = await this.ResolveInteractionCommandParameters(e, context, method, e.Interaction.Data.Options[0].Options[0].Options).ConfigureAwait(false);

								await this.RunCommandAsync(context, method, args).ConfigureAwait(false);
								break;
							}
							case ApplicationCommandFinalType.NotDetermined:
							default:
								throw new ArgumentOutOfRangeException(null, "Could not determine application command type");
						}

						await this._slashExecuted.InvokeAsync(this, new(this.Client.ServiceProvider)
						{
							Context = context
						}).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						await this._slashError.InvokeAsync(this, new(this.Client.ServiceProvider)
						{
							Context = context,
							Exception = ex
						}).ConfigureAwait(false);
						this.Client.Logger.LogError(ex, "Error in slash interaction");
					}

					break;
				}
				case InteractionType.AutoComplete when s_errored:
					throw new InvalidOperationException("Application commands failed to register properly on startup.");
				case InteractionType.AutoComplete:
				{
					var methods = CommandMethods.Where(x => x.CommandId == e.Interaction.Data.Id).ToList();
					var groups = GroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id).ToList();
					var subgroups = SubGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id).ToList();
					if (methods.Count is 0 && groups.Count is 0 && subgroups.Count is 0)
						throw new InvalidOperationException("An autocomplete interaction was created, but no command was registered for it");

					try
					{
						switch (type)
						{
							case ApplicationCommandFinalType.Command when methods.Count is not 0:
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
									Client = client,
									Services = Configuration.ServiceProvider,
									ApplicationCommandsExtension = this,
									GuildId = e.Interaction.GuildId,
									ChannelId = e.Interaction.ChannelId,
									MemberId = e.Interaction.GuildId is not null ? e.Interaction.User.Id : null,
									UserId = e.Interaction.User.Id,
									Guild = e.Interaction.Guild,
									Channel = e.Interaction.Channel,
									User = e.Interaction.User,
									Options = e.Interaction.Data.Options.ToList(),
									FocusedOption = focusedOption,
									Locale = e.Interaction.Locale,
									GuildLocale = e.Interaction.GuildLocale,
									AppPermissions = e.Interaction.AppPermissions,
									Entitlements = e.Interaction.Entitlements
								};

								var choices = await ((Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>)providerMethod.Invoke(providerInstance, [context])).ConfigureAwait(false);
								await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices)).ConfigureAwait(false);
								break;
							}
							case ApplicationCommandFinalType.SubCommand when groups.Count is not 0:
							{
								var command = e.Interaction.Data.Options[0];
								var group = groups.First().Methods.First(x => x.Key == command.Name).Value;

								var focusedOption = command.Options.First(o => o.Focused);
								var option = group.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
								var provider = option.GetCustomAttribute<AutocompleteAttribute>().ProviderType;
								var providerMethod = provider.GetMethod(nameof(IAutocompleteProvider.Provider));
								var providerInstance = Activator.CreateInstance(provider);

								var context = new AutocompleteContext
								{
									Client = client,
									Interaction = e.Interaction,
									Services = Configuration.ServiceProvider,
									ApplicationCommandsExtension = this,
									GuildId = e.Interaction.GuildId,
									ChannelId = e.Interaction.ChannelId,
									MemberId = e.Interaction.GuildId is not null ? e.Interaction.User.Id : null,
									UserId = e.Interaction.User.Id,
									Guild = e.Interaction.Guild,
									Channel = e.Interaction.Channel,
									User = e.Interaction.User,
									Options = command.Options.ToList(),
									FocusedOption = focusedOption,
									Locale = e.Interaction.Locale,
									GuildLocale = e.Interaction.GuildLocale,
									AppPermissions = e.Interaction.AppPermissions,
									Entitlements = e.Interaction.Entitlements,
								};

								var choices = await ((Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>)providerMethod.Invoke(providerInstance, [context])).ConfigureAwait(false);
								await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices)).ConfigureAwait(false);
								break;
							}
							case ApplicationCommandFinalType.SubCommandGroup when subgroups.Count is not 0:
							{
								var command = e.Interaction.Data.Options[0];
								var group = subgroups.First().SubCommands.First(x => x.Name == command.Name).Methods.First(x => x.Key == command.Options[0].Name).Value;

								var focusedOption = command.Options[0].Options.First(o => o.Focused);

								var option = group.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
								var provider = option.GetCustomAttribute<AutocompleteAttribute>().ProviderType;
								var providerMethod = provider.GetMethod(nameof(IAutocompleteProvider.Provider));
								var providerInstance = Activator.CreateInstance(provider);

								var context = new AutocompleteContext
								{
									Client = client,
									Interaction = e.Interaction,
									Services = Configuration.ServiceProvider,
									ApplicationCommandsExtension = this,
									GuildId = e.Interaction.GuildId,
									ChannelId = e.Interaction.ChannelId,
									MemberId = e.Interaction.GuildId is not null ? e.Interaction.User.Id : null,
									UserId = e.Interaction.User.Id,
									Guild = e.Interaction.Guild,
									Channel = e.Interaction.Channel,
									User = e.Interaction.User,
									Options = command.Options[0].Options.ToList(),
									FocusedOption = focusedOption,
									Locale = e.Interaction.Locale,
									GuildLocale = e.Interaction.GuildLocale,
									AppPermissions = e.Interaction.AppPermissions,
									Entitlements = e.Interaction.Entitlements,
								};

								var choices = await ((Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>)providerMethod.Invoke(providerInstance, [context])).ConfigureAwait(false);
								await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices)).ConfigureAwait(false);
								break;
							}
						}
					}
					catch (Exception ex)
					{
						this.Client.Logger.LogError(ex, "Error in autocomplete interaction");
					}

					break;
				}
				case InteractionType.Ping:
				case InteractionType.Component:
				case InteractionType.ModalSubmit:
					break;
				default:
					throw new ArgumentOutOfRangeException(null, "Received out unknown interaction type");
			}
		});
		return Task.CompletedTask;
	}

	/// <summary>
	///     Gets the interaction type from the interaction data.
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	private static ApplicationCommandFinalType GetInteractionType(DiscordInteractionData data)
	{
		var type = ApplicationCommandFinalType.NotDetermined;
		if (data.Options.Count is 0)
			return ApplicationCommandFinalType.Command;
		if (data.Options.All(x =>
			x.Type is not ApplicationCommandOptionType.SubCommand
				and not ApplicationCommandOptionType.SubCommandGroup))
			return ApplicationCommandFinalType.Command;

		if (data.Options.Any(x => x.Type is ApplicationCommandOptionType.SubCommandGroup))
			type = ApplicationCommandFinalType.SubCommandGroup;
		else if (data.Options.Any(x => x.Type is ApplicationCommandOptionType.SubCommand))
			type = ApplicationCommandFinalType.SubCommand;
		return type;
	}

	/// <summary>
	///     Context menu handler.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The event args.</param>
	private Task ContextMenuHandler(DiscordClient client, ContextMenuInteractionCreateEventArgs e)
	{
		this.Client.Logger.Log(ApplicationCommandsLogLevel, "Got context menu interaction on shard {shard}", this.Client.ShardId);
		if (HandledInteractions.Contains(e.Interaction.Id))
		{
			this.Client.Logger.Log(ApplicationCommandsLogLevel, "Ignoring, already received");
			return Task.FromResult(true);
		}

		HandledInteractions.Add(e.Interaction.Id);

		_ = Task.Run(async () =>
		{
			//Creates the context
			var context = new ContextMenuContext(e.Type switch
			{
				ApplicationCommandType.User => DisCatSharpCommandType.UserCommand,
				ApplicationCommandType.Message => DisCatSharpCommandType.MessageCommand,
				_ => throw new ArgumentOutOfRangeException(nameof(e.Type), "Unknown context menu type")
			})
			{
				Interaction = e.Interaction,
				Channel = e.Interaction.Channel,
				Client = client,
				Services = Configuration.ServiceProvider,
				CommandName = e.Interaction.Data.Name,
				ApplicationCommandsExtension = this,
				Guild = e.Interaction.Guild,
				InteractionId = e.Interaction.Id,
				User = e.Interaction.User,
				Token = e.Interaction.Token,
				TargetUser = e.TargetUser,
				TargetMessage = e.TargetMessage,
				Type = e.Type,
				Locale = e.Interaction.Locale,
				GuildLocale = e.Interaction.GuildLocale,
				AppPermissions = e.Interaction.AppPermissions,
				Entitlements = e.Interaction.Entitlements,
				UserId = e.Interaction.User.Id,
				GuildId = e.Interaction.GuildId,
				MemberId = e.Interaction.GuildId is not null ? e.Interaction.User.Id : null,
				ChannelId = e.Interaction.ChannelId,
				AttachmentSizeLimit = e.Interaction.AttachmentSizeLimit
			};

			try
			{
				if (s_errored)
				{
					await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Context menus failed to register properly on startup.").AsEphemeral()).ConfigureAwait(false);
					throw new InvalidOperationException("Context menus failed to register properly on startup.");
				}

				//Gets the method for the command
				var method = ContextMenuCommands.FirstOrDefault(x => x.CommandId == e.Interaction.Data.Id);

				if (method == null)
				{
					await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("A context menu command was executed, but no command was registered for it.").AsEphemeral()).ConfigureAwait(false);
					throw new InvalidOperationException("A context menu command was executed, but no command was registered for it.");
				}

				await this.RunCommandAsync(context, method.Method, new[] { context }).ConfigureAwait(false);

				await this._contextMenuExecuted.InvokeAsync(this, new(this.Client.ServiceProvider)
				{
					Context = context
				}).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await this._contextMenuErrored.InvokeAsync(this, new(this.Client.ServiceProvider)
				{
					Context = context,
					Exception = ex
				}).ConfigureAwait(false);
			}
		});

		return Task.CompletedTask;
	}

	/// <summary>
	///     Runs a command.
	/// </summary>
	/// <param name="context">The base context.</param>
	/// <param name="method">The method info.</param>
	/// <param name="args">The arguments.</param>
	internal async Task RunCommandAsync(BaseContext context, MethodInfo method, IEnumerable<object> args)
	{
		this.Client.Logger.Log(ApplicationCommandsLogLevel, "Executing {cmd}", method.Name);
		
		try
		{
			//Accounts for lifespans
			var moduleLifespan = (method.DeclaringType.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null ? method.DeclaringType.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>()?.Lifespan : ApplicationCommandModuleLifespan.Transient) ?? ApplicationCommandModuleLifespan.Transient;
			
			// Create service scope for scoped modules and store in context for disposal
			IServiceProvider serviceProvider;
			if (moduleLifespan == ApplicationCommandModuleLifespan.Scoped && Configuration?.ServiceProvider != null)
			{
				context.ServiceScope = Configuration.ServiceProvider.CreateScope();
				serviceProvider = context.ServiceScope.ServiceProvider;
			}
			else
			{
				serviceProvider = Configuration?.ServiceProvider;
			}
			
			var classInstance = moduleLifespan switch
			{
				ApplicationCommandModuleLifespan.Scoped =>
					//Accounts for static methods and adds DI with scoped service provider
					method.IsStatic ? ActivatorUtilities.CreateInstance(serviceProvider, method.DeclaringType) : CreateInstance(method.DeclaringType, serviceProvider),
				ApplicationCommandModuleLifespan.Transient =>
					//Accounts for static methods and adds DI
					method.IsStatic ? ActivatorUtilities.CreateInstance(Configuration?.ServiceProvider, method.DeclaringType) : CreateInstance(method.DeclaringType, Configuration?.ServiceProvider),
				//If singleton, gets it from the singleton list
				ApplicationCommandModuleLifespan.Singleton => s_singletonModules.First(x => ReferenceEquals(x.GetType(), method.DeclaringType)),
				_ => throw new($"An unknown {nameof(ApplicationCommandModuleLifespanAttribute)} scope was specified on command {context.CommandName}")
			};

			ApplicationCommandsModule module = null;
			if (classInstance is ApplicationCommandsModule mod)
				module = mod;

			switch (context)
			{
				// Slash commands
				case InteractionContext slashContext:
				{
					await RunPreexecutionChecksAsync(method, slashContext).ConfigureAwait(false);

					var shouldExecute = await (module?.BeforeSlashExecutionAsync(slashContext) ?? Task.FromResult(true)).ConfigureAwait(false);

					if (shouldExecute)
					{
						if (AutoDeferEnabled)
							await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
						await ((Task)method.Invoke(classInstance, args.ToArray())).ConfigureAwait(false);

						await (module?.AfterSlashExecutionAsync(slashContext) ?? Task.CompletedTask).ConfigureAwait(false);
					}

					break;
				}
				// Context menus
				case ContextMenuContext contextMenuContext:
				{
					await RunPreexecutionChecksAsync(method, contextMenuContext).ConfigureAwait(false);

					var shouldExecute = await (module?.BeforeContextMenuExecutionAsync(contextMenuContext) ?? Task.FromResult(true)).ConfigureAwait(false);

					if (shouldExecute)
					{
						if (AutoDeferEnabled)
							await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
						await ((Task)method.Invoke(classInstance, args.ToArray())).ConfigureAwait(false);

						await (module?.AfterContextMenuExecutionAsync(contextMenuContext) ?? Task.CompletedTask).ConfigureAwait(false);
					}

					break;
				}
			}
		}
		finally
		{
			// Ensure service scope is properly disposed
			context.ServiceScope?.Dispose();
			context.ServiceScope = null;
		}
	}

	/// <summary>
	///     Property injection
	/// </summary>
	/// <param name="t">The type.</param>
	/// <param name="services">The services.</param>
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
		var props = t.GetRuntimeProperties().Where(xp => xp.CanWrite && xp.SetMethod is not null && !xp.SetMethod.IsStatic && xp.SetMethod.IsPublic);
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
		var fields = t.GetRuntimeFields().Where(xf => !xf.IsInitOnly && xf is { IsStatic: false, IsPublic: true });
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
	///     Resolves the slash command parameters.
	/// </summary>
	/// <param name="e">The event arguments.</param>
	/// <param name="context">The interaction context.</param>
	/// <param name="method">The method info.</param>
	/// <param name="options">The options.</param>
	private async Task<List<object?>> ResolveInteractionCommandParameters(InteractionCreateEventArgs e, InteractionContext context, MethodBase method, IReadOnlyList<DiscordInteractionDataOption> options)
	{
		var args = new List<object?>
		{
			context
		};
		var parameters = method.GetParameters().Skip(1).ToList();

		foreach (var parameter in parameters)
			//Accounts for optional arguments without values given
			if (parameter.IsOptional && (options is null || (!options?.Any(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>()?.Name.ToLower()) ?? true)))
				args.Add(parameter.DefaultValue);
			else
			{
				var option = options.Single(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>()?.Name.ToLower());

				if (parameter.ParameterType == typeof(string))
					args.Add(option.Value.ToString());
				else if (parameter.ParameterType.IsEnum)
					args.Add(Enum.Parse(parameter.ParameterType, (string)option.Value));
				else if (parameter.ParameterType == typeof(ulong) || parameter.ParameterType == typeof(ulong?))
					args.Add((ulong?)option.Value);
				else if (parameter.ParameterType == typeof(int) || parameter.ParameterType == typeof(int?))
					args.Add((int?)option.Value);
				else if (parameter.ParameterType == typeof(long) || parameter.ParameterType == typeof(long?))
					if (option.Value is null)
						args.Add(null);
					else
						args.Add(Convert.ToInt64(option.Value));
				else if (parameter.ParameterType == typeof(bool) || parameter.ParameterType == typeof(bool?))
					args.Add((bool?)option.Value);
				else if (parameter.ParameterType == typeof(double) || parameter.ParameterType == typeof(double?))
					args.Add((double?)option.Value);
				else if (parameter.ParameterType == typeof(int) || parameter.ParameterType == typeof(int?))
					args.Add((int?)option.Value);
				else if (parameter.ParameterType == typeof(DiscordAttachment))
				{
					//Checks through resolved
					if (e.Interaction.Data.Resolved.Attachments != null &&
					    e.Interaction.Data.Resolved.Attachments.TryGetValue((ulong)option.Value, out var attachment))
						args.Add(attachment);
					else
						args.Add(new DiscordAttachment
						{
							Id = (ulong)option.Value,
							Discord = this.Client.ApiClient.Discord
						});
				}
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
						args.Add(await this.Client.GetUserAsync((ulong)option.Value).ConfigureAwait(false));
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
					if (e.Interaction.Data.Resolved.Channels != null && e.Interaction.Data.Resolved.Channels.TryGetValue((ulong)option.Value, out var channel))
						args.Add(channel);
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
					throw new ArgumentException("Error resolving interaction.");
			}

		return args;
	}

	/// <summary>
	///     Runs the pre-execution checks.
	/// </summary>
	/// <param name="method">The method info.</param>
	/// <param name="context">The base context.</param>
	private static async Task RunPreexecutionChecksAsync(MemberInfo method, BaseContext context)
	{
		switch (context)
		{
			case InteractionContext ctx:
			{
				//Gets all attributes from parent classes as well and stuff
				var attributes = new List<ApplicationCommandCheckBaseAttribute>();
				if (method.DeclaringType?.DeclaringType is not null)
				{
					if (method.DeclaringType.DeclaringType.DeclaringType is not null)
						attributes.AddRange(method.DeclaringType.DeclaringType.DeclaringType.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>());
					attributes.AddRange(method.DeclaringType.DeclaringType.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>());
				}

				attributes.AddRange(method.DeclaringType!.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>());
				attributes.AddRange(method.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>(true));

				var dict = new Dictionary<ApplicationCommandCheckBaseAttribute, bool>();
				foreach (var att in attributes)
				{
					//Runs the check and adds the result to a list
					var result = await att.ExecuteChecksAsync(ctx).ConfigureAwait(false);
					dict.Add(att, result);
				}

				//Checks if any failed, and throws an exception
				if (dict.Any(x => x.Value is false))
					throw new SlashExecutionChecksFailedException
					{
						FailedChecks = dict.Where(x => x.Value is false).Select(x => x.Key).ToList()
					};

				break;
			}
			case ContextMenuContext cMctx:
			{
				var attributes = new List<ApplicationCommandCheckBaseAttribute>();

				if (method.DeclaringType?.DeclaringType is not null)
				{
					if (method.DeclaringType.DeclaringType.DeclaringType is not null)
						attributes.AddRange(method.DeclaringType.DeclaringType.DeclaringType.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>());
					attributes.AddRange(method.DeclaringType.DeclaringType.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>());
				}

				attributes.AddRange(method.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>(true));
				attributes.AddRange(method.DeclaringType!.GetCustomAttributes<ApplicationCommandCheckBaseAttribute>());

				var dict = new Dictionary<ApplicationCommandCheckBaseAttribute, bool>();
				foreach (var att in attributes)
				{
					//Runs the check and adds the result to a list
					var result = await att.ExecuteChecksAsync(cMctx).ConfigureAwait(false);
					dict.Add(att, result);
				}

				//Checks if any failed, and throws an exception
				if (dict.Any(x => x.Value is false))
					throw new ContextMenuExecutionChecksFailedException
					{
						FailedChecks = dict.Where(x => x.Value is false).Select(x => x.Key).ToList()
					};

				break;
			}
		}
	}

	/// <summary>
	///     Gets the choice attributes from choice provider.
	/// </summary>
	/// <param name="customAttributes">The custom attributes.</param>
	/// <param name="guildId">The optional guild id</param>
	private static async Task<List<DiscordApplicationCommandOptionChoice>> GetChoiceAttributesFromProvider(List<ChoiceProviderAttribute> customAttributes, ulong? guildId = null)
	{
		var choices = new List<DiscordApplicationCommandOptionChoice>();
		foreach (var choiceProviderAttribute in customAttributes)
		{
			var method = choiceProviderAttribute.ProviderType.GetMethod(nameof(IChoiceProvider.Provider)) ?? throw new ArgumentException("ChoiceProviders must inherit from IChoiceProvider.");
			var instance = Activator.CreateInstance(choiceProviderAttribute.ProviderType);

			// Abstract class offers more properties that can be set
			if (choiceProviderAttribute.ProviderType.IsSubclassOf(typeof(ChoiceProvider)))
			{
				choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.GuildId))
					?.SetValue(instance, guildId);

				choiceProviderAttribute.ProviderType.GetProperty(nameof(ChoiceProvider.Services))
					?.SetValue(instance, Configuration.ServiceProvider);
			}

			//Gets the choices from the method
			var result = (await ((Task<IEnumerable<DiscordApplicationCommandOptionChoice>>)method.Invoke(instance, null)!).ConfigureAwait(false)).ToList();

			if (result.Count is not 0)
				choices.AddRange(result);
		}

		return choices;
	}

	/// <summary>
	///     Gets the choice attributes from enum parameter.
	/// </summary>
	/// <param name="enumParam">The enum parameter.</param>
	private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromEnumParameter(Type enumParam)
		=> (from Enum enumValue in Enum.GetValues(enumParam) select new DiscordApplicationCommandOptionChoice(enumValue.GetName(), enumValue.ToString())).ToList();

	/// <summary>
	///     Gets the parameter type.
	/// </summary>
	/// <param name="type">The type.</param>
	private static ApplicationCommandOptionType GetParameterType(Type type)
	{
		var parameterType = type == typeof(string)
			? ApplicationCommandOptionType.String
			: type == typeof(long) || type == typeof(long?) || type == typeof(int) || type == typeof(int?)
				? ApplicationCommandOptionType.Integer
				: type == typeof(bool) || type == typeof(bool?)
					? ApplicationCommandOptionType.Boolean
					: type == typeof(double) || type == typeof(double?)
						? ApplicationCommandOptionType.Number
						: type == typeof(DiscordAttachment)
							? ApplicationCommandOptionType.Attachment
							: type == typeof(DiscordChannel)
								? ApplicationCommandOptionType.Channel
								: type == typeof(DiscordUser)
									? ApplicationCommandOptionType.User
									: type == typeof(DiscordRole)
										? ApplicationCommandOptionType.Role
										: type == typeof(SnowflakeObject)
											? ApplicationCommandOptionType.Mentionable
											: type.IsEnum
												? ApplicationCommandOptionType.String
												: throw new ArgumentException("Cannot convert type! Argument types must be string, int, long, bool, double, DiscordChannel, DiscordUser, DiscordRole, SnowflakeObject, DiscordAttachment or an Enum.");
		return parameterType;
	}

	/// <summary>
	///     Gets the choice attributes from parameter.
	/// </summary>
	/// <param name="choiceAttributes">The choice attributes.</param>
	private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromParameter(List<ChoiceAttribute> choiceAttributes) =>
		choiceAttributes.Count is 0
			? []
			: choiceAttributes.Select(att => new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToList();

	/// <summary>
	///     Parses the parameters.
	/// </summary>
	/// <param name="parameters">The parameters.</param>
	/// <param name="commandName">The command name.</param>
	/// <param name="guildId">The optional guild id.</param>
	internal static async Task<List<DiscordApplicationCommandOption>> ParseParametersAsync(IEnumerable<ParameterInfo> parameters, string commandName, ulong? guildId)
	{
		var options = new List<DiscordApplicationCommandOption>();
		foreach (var parameter in parameters)
		{
			//Gets the attribute
			var optionAttribute = parameter.GetCustomAttribute<OptionAttribute>() ?? throw new ArgumentException($"One or more arguments of the command '{commandName}' are missing the Option attribute!");
			var minimumValue = parameter.GetCustomAttribute<MinimumValueAttribute>()?.Value ?? null;
			var maximumValue = parameter.GetCustomAttribute<MaximumValueAttribute>()?.Value ?? null;
			var minimumLength = parameter.GetCustomAttribute<MinimumLengthAttribute>()?.Value ?? null;
			var maximumLength = parameter.GetCustomAttribute<MaximumLengthAttribute>()?.Value ?? null;
			var channelTypes = parameter.GetCustomAttribute<ChannelTypesAttribute>()?.ChannelTypes ?? null;

			var autocompleteAttribute = parameter.GetCustomAttribute<AutocompleteAttribute>();
			switch (optionAttribute.Autocomplete)
			{
				case true when autocompleteAttribute == null:
					throw new ArgumentException($"The command '{commandName}' has autocomplete enabled but is missing an autocomplete attribute!");
				case false when autocompleteAttribute != null:
					throw new ArgumentException($"The command '{commandName}' has an autocomplete provider but the option to have autocomplete set to false!");
			}

			//Sets the type
			var type = parameter.ParameterType;
			var parameterType = GetParameterType(type);

			switch (parameterType)
			{
				case ApplicationCommandOptionType.String:
					minimumValue = null;
					maximumValue = null;
					break;
				case ApplicationCommandOptionType.Integer:
				case ApplicationCommandOptionType.Number:
					minimumLength = null;
					maximumLength = null;
					break;
				case not ApplicationCommandOptionType.Channel:
					channelTypes = null;
					break;
			}

			//Handles choices
			//From attributes
			var choices = GetChoiceAttributesFromParameter(parameter.GetCustomAttributes<ChoiceAttribute>().ToList());
			//From enums
			if (parameter.ParameterType.IsEnum)
				choices = GetChoiceAttributesFromEnumParameter(parameter.ParameterType);
			//From choice provider
			var choiceProviders = parameter.GetCustomAttributes<ChoiceProviderAttribute>().ToList();
			if (choiceProviders.Count is not 0)
				choices = await GetChoiceAttributesFromProvider(choiceProviders, guildId).ConfigureAwait(false);

			options.Add(new(optionAttribute.Name, optionAttribute.Description, parameterType, !parameter.IsOptional, choices, null, channelTypes, optionAttribute.Autocomplete, minimumValue, maximumValue, minimumLength: minimumLength, maximumLength: maximumLength));
		}

		return options;
	}
	/*
	/// <summary>
	/// <para>Refreshes your commands, used for refreshing choice providers or applying commands registered after the ready event on the discord client.</para>
	/// <para>Not recommended and should be avoided since it can make slash commands be unresponsive for a while.</para>
	/// </summary>
	public async Task RefreshCommandsAsync()
	{
		s_commandMethods.Clear();
		s_groupCommands.Clear();
		s_subGroupCommands.Clear();
		_registeredCommands.Clear();
		s_contextMenuCommands.Clear();
		GlobalDiscordCommands.Clear();
		GuildDiscordCommands.Clear();
		GuildCommandsInternal.Clear();
		GlobalCommandsInternal.Clear();
		GlobalDiscordCommands = null;
		GuildDiscordCommands = null;
		s_errored = false;

		/*if (Configuration != null && Configuration.EnableDefaultHelp)
		{
			this._updateList.RemoveAll(x => x.Value.Type == typeof(DefaultHelpModule));
		}*/
	/*
		await this.UpdateAsync();
	}*/

	/// <summary>
	///     Fires when the execution of a slash command fails.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
	{
		add => this._slashError.Register(value);
		remove => this._slashError.Unregister(value);
	}

	/// <summary>
	///     Fires when the execution of a slash command is successful.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
	{
		add => this._slashExecuted.Register(value);
		remove => this._slashExecuted.Unregister(value);
	}

	/// <summary>
	///     Fires when the execution of a context menu fails.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ContextMenuErrorEventArgs> ContextMenuErrored
	{
		add => this._contextMenuErrored.Register(value);
		remove => this._contextMenuErrored.Unregister(value);
	}

	/// <summary>
	///     Fire when the execution of a context menu is successful.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ContextMenuExecutedEventArgs> ContextMenuExecuted
	{
		add => this._contextMenuExecuted.Register(value);
		remove => this._contextMenuExecuted.Unregister(value);
	}
}

/// <summary>
///     Holds configuration data for setting up an application command.
/// </summary>
internal sealed class ApplicationCommandsModuleConfiguration
{
	/// <summary>
	///     Creates a new command configuration.
	/// </summary>
	/// <param name="type">The type of the command module.</param>
	/// <param name="translations">The translation setup callback.</param>
	public ApplicationCommandsModuleConfiguration(Type type, Action<ApplicationCommandsTranslationContext>? translations = null)
	{
		this.Type = type;
		this.Translations = translations;
	}

	/// <summary>
	///     The type of the command module.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	///     The translation setup.
	/// </summary>
	public Action<ApplicationCommandsTranslationContext>? Translations { get; }
}

/// <summary>
///     Links a command to its original command module.
/// </summary>
internal class ApplicationCommandSourceLink
{
	/// <summary>
	///     The command.
	/// </summary>
	public DiscordApplicationCommand ApplicationCommand { get; set; }

	/// <summary>
	///     The base/root module the command is contained in.
	/// </summary>
	public Type RootCommandContainerType { get; set; }

	/// <summary>
	///     The direct group the command is contained in.
	/// </summary>
	public Type CommandContainerType { get; set; }
}

/// <summary>
///     The command method.
/// </summary>
internal sealed class CommandMethod
{
	/// <summary>
	///     Gets or sets the command id.
	/// </summary>
	public ulong CommandId { get; set; }

	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	///     Gets or sets the method.
	/// </summary>
	public MethodInfo Method { get; init; }
}

/// <summary>
///     The group command.
/// </summary>
internal sealed class GroupCommand
{
	/// <summary>
	///     Gets or sets the command id.
	/// </summary>
	public ulong CommandId { get; set; }

	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	///     Gets or sets the methods.
	/// </summary>
	public List<KeyValuePair<string, MethodInfo>> Methods { get; init; } = [];
}

/// <summary>
///     The sub group command.
/// </summary>
internal sealed class SubGroupCommand
{
	/// <summary>
	///     Gets or sets the command id.
	/// </summary>
	public ulong CommandId { get; set; }

	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	///     Gets or sets the sub commands.
	/// </summary>
	public List<GroupCommand> SubCommands { get; set; } = [];
}

/// <summary>
///     The context menu command.
/// </summary>
internal sealed class ContextMenuCommand
{
	/// <summary>
	///     Gets or sets the command id.
	/// </summary>
	public ulong CommandId { get; set; }

	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	///     Gets or sets the method.
	/// </summary>
	public MethodInfo Method { get; init; }
}

#region Default Help

/// <summary>
///     Represents the default help module.
/// </summary>
internal sealed class DefaultHelpModule : ApplicationCommandsModule
{
	[SlashCommand("help", "Displays command help")]
	internal async Task DefaultHelpAsync(
		InteractionContext ctx,
		[Autocomplete(typeof(DefaultHelpAutoCompleteProvider)), Option("option_one", "top level command to provide help for", true)]
		string commandName,
		[Autocomplete(typeof(DefaultHelpAutoCompleteLevelOneProvider)), Option("option_two", "subgroup or command to provide help for", true)]
		string commandOneName = null,
		[Autocomplete(typeof(DefaultHelpAutoCompleteLevelTwoProvider)), Option("option_three", "command to provide help for", true)]
		string commandTwoName = null
	)
	{
		List<DiscordApplicationCommand> applicationCommands = null;
		var globalCommandsTask = ctx.Client.GetGlobalApplicationCommandsAsync();
		if (ctx.Guild != null)
		{
			var guildCommandsTask = ctx.Client.GetGuildApplicationCommandsAsync(ctx.Guild.Id);
			await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
			applicationCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
				.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
				.GroupBy(ac => ac.Name).Select(x => x.First())
				.ToList();
		}
		else
		{
			await Task.WhenAll(globalCommandsTask).ConfigureAwait(false);
			applicationCommands = globalCommandsTask.Result
				.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
				.GroupBy(ac => ac.Name).Select(x => x.First())
				.ToList();
		}

		if (applicationCommands.Count < 1)
		{
			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.WithContent("There are no slash commands")).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
					.WithContent("There are no slash commands").AsEphemeral()).ConfigureAwait(false);
			return;
		}

		if (commandTwoName is not null && !commandTwoName.Equals("no_options_for_this_command"))
		{
			var commandsWithSubCommands = applicationCommands.FindAll(ac => ac.Options is not null && ac.Options.Any(op => op.Type == ApplicationCommandOptionType.SubCommandGroup));
			var subCommandParent = commandsWithSubCommands.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			var cmdParent = commandsWithSubCommands.FirstOrDefault(cm => cm.Options.Any(op => op.Name.Equals(commandOneName))).Options
				.FirstOrDefault(opt => opt.Name.Equals(commandOneName, StringComparison.OrdinalIgnoreCase));
			var cmd = cmdParent.Options.FirstOrDefault(op => op.Name.Equals(commandTwoName, StringComparison.OrdinalIgnoreCase));
			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{subCommandParent.Mention.Replace(subCommandParent.Name, $"{subCommandParent.Name} {cmdParent.Name} {cmd.Name}")}: {cmd.Description ?? "No description provided."}"
			};
			if (cmd.Options is not null)
			{
				var commandOptions = cmd.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new("Arguments", sb.ToString().Trim()));
			}

			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(discordEmbed)).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral()).ConfigureAwait(false);
		}
		else if (commandOneName is not null && commandTwoName is null && !commandOneName.Equals("no_options_for_this_command"))
		{
			var commandsWithOptions = applicationCommands.FindAll(ac => ac.Options is not null && ac.Options.All(op => op.Type == ApplicationCommandOptionType.SubCommand));
			var subCommandParent = commandsWithOptions.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			var subCommand = subCommandParent.Options.FirstOrDefault(op => op.Name.Equals(commandOneName, StringComparison.OrdinalIgnoreCase));
			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{subCommandParent.Mention.Replace(subCommandParent.Name, $"{subCommandParent.Name} {subCommand.Name}")}: {subCommand.Description ?? "No description provided."}"
			};
			if (subCommand.Options is not null)
			{
				var commandOptions = subCommand.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new("Arguments", sb.ToString().Trim()));
			}

			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbed)).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral()).ConfigureAwait(false);
		}
		else
		{
			var command = applicationCommands.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			if (command is null)
			{
				if (ApplicationCommandsExtension.Configuration.AutoDefer)
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
						.WithContent($"No command called {commandName} in guild {ctx.Guild.Name}")).ConfigureAwait(false);
				else
					await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
						.WithContent($"No command called {commandName} in guild {ctx.Guild.Name}").AsEphemeral()).ConfigureAwait(false);
				return;
			}

			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{command.Mention}: {command.Description ?? "No description provided."}"
			}.AddField(new("Command is NSFW", command.IsNsfw.ToString()));
			if (command.Options is not null)
			{
				var commandOptions = command.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new("Arguments", sb.ToString().Trim()));
			}

			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbed)).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral()).ConfigureAwait(false);
		}
	}

	public sealed class DefaultHelpAutoCompleteProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First())
					.ToList();

				if (context.Options.Count > 0 && !string.IsNullOrEmpty(context.Options[0].Value?.ToString()))
					slashCommands = slashCommands.Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
			}
			else
			{
				await Task.WhenAll(globalCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First())
					.ToList();
				
				if (context.Options.Count > 0 && !string.IsNullOrEmpty(context.Options[0].Value?.ToString()))
					slashCommands = slashCommands.Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
			}

			var options = slashCommands.Take(25).Select(sc => new DiscordApplicationCommandAutocompleteChoice(sc.Name, sc.Name.Trim())).ToList();
			return options.AsEnumerable();
		}
	}

	public sealed class DefaultHelpAutoCompleteLevelOneProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			var options = new List<DiscordApplicationCommandAutocompleteChoice>();
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}
			else
			{
				await Task.WhenAll(globalCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}

			var command = slashCommands.FirstOrDefault(ac =>
				ac.Name.Equals(context.Options[0].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (command?.Options is null)
				options.Add(new("no_options_for_this_command", "no_options_for_this_command"));
			else
			{
				var opt = command.Options.Where(c => c.Type is ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType.SubCommand
				                                     && c.Name.StartsWith(context.Options[1].Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).ToList();
				options.AddRange(opt.Take(25).Select(option => new DiscordApplicationCommandAutocompleteChoice(option.Name, option.Name.Trim())));
			}

			return options.AsEnumerable();
		}
	}

	public sealed class DefaultHelpAutoCompleteLevelTwoProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			var options = new List<DiscordApplicationCommandAutocompleteChoice>();
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}
			else
			{
				await Task.WhenAll(globalCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}

			var command = slashCommands.FirstOrDefault(ac =>
				ac.Name.Equals(context.Options[0].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (command.Options is null)
			{
				options.Add(new("no_options_for_this_command", "no_options_for_this_command"));
				return options.AsEnumerable();
			}

			var foundCommand = command.Options.FirstOrDefault(op => op.Name.Equals(context.Options[1].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (foundCommand?.Options is null)
				options.Add(new("no_options_for_this_command", "no_options_for_this_command"));
			else
			{
				var opt = foundCommand.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand &&
				                                          x.Name.StartsWith(context.Options[2].Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
				options.AddRange(opt.Take(25).Select(option => new DiscordApplicationCommandAutocompleteChoice(option.Name, option.Name.Trim())));
			}

			return options.AsEnumerable();
		}
	}
}

#endregion

#region Default User Apps Help

/// <summary>
///     Represents the default user apps help module.
/// </summary>
internal sealed class DefaultUserAppsHelpModule : ApplicationCommandsModule
{
	[SlashCommand("help", "Displays command help", false, [InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel], [ApplicationCommandIntegrationTypes.GuildInstall, ApplicationCommandIntegrationTypes.UserInstall])]
	internal async Task DefaulUserAppstHelpAsync(
		InteractionContext ctx,
		[Autocomplete(typeof(DefaultUserAppsHelpAutoCompleteProvider)), Option("option_one", "top level command to provide help for", true)]
		string commandName,
		[Autocomplete(typeof(DefaultUserAppsHelpAutoCompleteLevelOneProvider)), Option("option_two", "subgroup or command to provide help for", true)]
		string commandOneName = null,
		[Autocomplete(typeof(DefaultUserAppsHelpAutoCompleteLevelTwoProvider)), Option("option_three", "command to provide help for", true)]
		string commandTwoName = null
	)
	{
		List<DiscordApplicationCommand> applicationCommands = null;
		var globalCommandsTask = ctx.Client.GetGlobalApplicationCommandsAsync();
		if (ctx.Guild != null)
		{
			var guildCommandsTask = ctx.Client.GetGuildApplicationCommandsAsync(ctx.Guild.Id);
			await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
			applicationCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
				.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
				.GroupBy(ac => ac.Name).Select(x => x.First())
				.ToList();
		}
		else
		{
			await globalCommandsTask.ConfigureAwait(false);
			applicationCommands = globalCommandsTask.Result
				.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
				.GroupBy(ac => ac.Name).Select(x => x.First())
				.ToList();
		}

		if (applicationCommands.Count < 1)
		{
			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.WithContent("There are no slash commands")).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
					.WithContent("There are no slash commands").AsEphemeral()).ConfigureAwait(false);
			return;
		}

		if (commandTwoName is not null && !commandTwoName.Equals("no_options_for_this_command"))
		{
			var commandsWithSubCommands = applicationCommands.FindAll(ac => ac.Options is not null && ac.Options.Any(op => op.Type == ApplicationCommandOptionType.SubCommandGroup));
			var subCommandParent = commandsWithSubCommands.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			var cmdParent = commandsWithSubCommands.FirstOrDefault(cm => cm.Options.Any(op => op.Name.Equals(commandOneName))).Options
				.FirstOrDefault(opt => opt.Name.Equals(commandOneName, StringComparison.OrdinalIgnoreCase));
			var cmd = cmdParent.Options.FirstOrDefault(op => op.Name.Equals(commandTwoName, StringComparison.OrdinalIgnoreCase));
			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{subCommandParent.Mention.Replace(subCommandParent.Name, $"{subCommandParent.Name} {cmdParent.Name} {cmd.Name}")}: {cmd.Description ?? "No description provided."}"
			};
			if (cmd.Options is not null)
			{
				var commandOptions = cmd.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new("Arguments", sb.ToString().Trim()));
			}

			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder()
					.AddEmbed(discordEmbed)).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral()).ConfigureAwait(false);
		}
		else if (commandOneName is not null && commandTwoName is null && !commandOneName.Equals("no_options_for_this_command"))
		{
			var commandsWithOptions = applicationCommands.FindAll(ac => ac.Options is not null && ac.Options.All(op => op.Type == ApplicationCommandOptionType.SubCommand));
			var subCommandParent = commandsWithOptions.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			var subCommand = subCommandParent.Options.FirstOrDefault(op => op.Name.Equals(commandOneName, StringComparison.OrdinalIgnoreCase));
			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{subCommandParent.Mention.Replace(subCommandParent.Name, $"{subCommandParent.Name} {subCommand.Name}")}: {subCommand.Description ?? "No description provided."}"
			};
			if (subCommand.Options is not null)
			{
				var commandOptions = subCommand.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new("Arguments", sb.ToString().Trim()));
			}

			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbed)).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral()).ConfigureAwait(false);
		}
		else
		{
			var command = applicationCommands.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			if (command is null)
			{
				if (ApplicationCommandsExtension.Configuration.AutoDefer)
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
						.WithContent($"No command called {commandName} in guild {ctx.Guild.Name}")).ConfigureAwait(false);
				else
					await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
						.WithContent($"No command called {commandName} in guild {ctx.Guild.Name}").AsEphemeral()).ConfigureAwait(false);
				return;
			}

			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{command.Mention}: {command.Description ?? "No description provided."}"
			}.AddField(new("Command is NSFW", command.IsNsfw.ToString()));
			if (command.Options is not null)
			{
				var commandOptions = command.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append("`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new("Arguments", sb.ToString().Trim()));
			}

			if (ApplicationCommandsExtension.Configuration.AutoDefer)
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbed)).ConfigureAwait(false);
			else
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral()).ConfigureAwait(false);
		}
	}

	public sealed class DefaultUserAppsHelpAutoCompleteProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First())
					.ToList();
				
				if (context.Options.Count > 0 && !string.IsNullOrEmpty(context.Options[0].Value?.ToString()))
					slashCommands = slashCommands.Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
			}
			else
			{
				await globalCommandsTask.ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First())
					.ToList();
				
				if (context.Options.Count > 0 && !string.IsNullOrEmpty(context.Options[0].Value?.ToString()))
					slashCommands = slashCommands.Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
			}

			var options = slashCommands.Take(25).Select(sc => new DiscordApplicationCommandAutocompleteChoice(sc.Name, sc.Name.Trim())).ToList();
			return options.AsEnumerable();
		}
	}

	public sealed class DefaultUserAppsHelpAutoCompleteLevelOneProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			var options = new List<DiscordApplicationCommandAutocompleteChoice>();
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}
			else
			{
				await globalCommandsTask.ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}

			var command = slashCommands.FirstOrDefault(ac =>
				ac.Name.Equals(context.Options[0].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (command?.Options is null)
				options.Add(new("no_options_for_this_command", "no_options_for_this_command"));
			else
			{
				var opt = command.Options.Where(c => c.Type is ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType.SubCommand
				                                     && c.Name.StartsWith(context.Options[1].Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).ToList();
				options.AddRange(opt.Take(25).Select(option => new DiscordApplicationCommandAutocompleteChoice(option.Name, option.Name.Trim())));
			}

			return options.AsEnumerable();
		}
	}

	public sealed class DefaultUserAppsHelpAutoCompleteLevelTwoProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			var options = new List<DiscordApplicationCommandAutocompleteChoice>();
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask).ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}
			else
			{
				await globalCommandsTask.ConfigureAwait(false);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}

			var command = slashCommands.FirstOrDefault(ac =>
				ac.Name.Equals(context.Options[0].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (command.Options is null)
			{
				options.Add(new("no_options_for_this_command", "no_options_for_this_command"));
				return options.AsEnumerable();
			}

			var foundCommand = command.Options.FirstOrDefault(op => op.Name.Equals(context.Options[1].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (foundCommand?.Options is null)
				options.Add(new("no_options_for_this_command", "no_options_for_this_command"));
			else
			{
				var opt = foundCommand.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand &&
				                                          x.Name.StartsWith(context.Options[2].Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
				options.AddRange(opt.Take(25).Select(option => new DiscordApplicationCommandAutocompleteChoice(option.Name, option.Name.Trim())));
			}

			return options.AsEnumerable();
		}
	}
}

#endregion
