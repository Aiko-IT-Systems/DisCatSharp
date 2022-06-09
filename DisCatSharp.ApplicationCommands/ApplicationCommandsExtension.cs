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
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Common;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands;

/// <summary>
/// A class that handles slash commands for a client.
/// </summary>
public sealed class ApplicationCommandsExtension : BaseExtension
{
	/// <summary>
	/// A list of methods for top level commands.
	/// </summary>
	private static List<CommandMethod> s_commandMethods { get; set; } = new();

	/// <summary>
	/// List of groups.
	/// </summary>
	private static List<GroupCommand> s_groupCommands { get; set; } = new();

	/// <summary>
	/// List of groups with subgroups.
	/// </summary>
	private static List<SubGroupCommand> s_subGroupCommands { get; set; } = new();

	/// <summary>
	/// List of context menus.
	/// </summary>
	private static List<ContextMenuCommand> s_contextMenuCommands { get; set; } = new();

	/// <summary>
	/// List of global commands on discords backend.
	/// </summary>
	internal static List<DiscordApplicationCommand> GlobalDiscordCommands { get; set; }

	/// <summary>
	/// List of guild commands on discords backend.
	/// </summary>
	internal static Dictionary<ulong, List<DiscordApplicationCommand>> GuildDiscordCommands { get; set; }

	/// <summary>
	/// Singleton modules.
	/// </summary>
	private static List<object> s_singletonModules { get; set; } = new();

	/// <summary>
	/// List of modules to register.
	/// </summary>
	private readonly List<KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>> _updateList = new();

	/// <summary>
	/// Configuration for Discord.
	/// </summary>
	internal static ApplicationCommandsConfiguration Configuration;

	/// <summary>
	/// Discord client.
	/// </summary>
	internal static DiscordClient ClientInternal;

	/// <summary>
	/// Set to true if anything fails when registering.
	/// </summary>
	private static bool s_errored { get; set; }

	/// <summary>
	/// Gets a list of registered commands. The key is the guild id (null if global).
	/// </summary>
	public IReadOnlyList<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> RegisteredCommands
		=> s_registeredCommands;
	private static readonly List<KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>> s_registeredCommands = new();

	/// <summary>
	/// Gets a list of registered global commands.
	/// </summary>
	public IReadOnlyList<DiscordApplicationCommand> GlobalCommands
		=> GlobalCommandsInternal;
	internal static readonly List<DiscordApplicationCommand> GlobalCommandsInternal = new();

	/// <summary>
	/// Gets a list of registered guild commands mapped by guild id.
	/// </summary>
	public IReadOnlyDictionary<ulong, IReadOnlyList<DiscordApplicationCommand>> GuildCommands
	=> GuildCommandsInternal;
	internal static readonly Dictionary<ulong, IReadOnlyList<DiscordApplicationCommand>> GuildCommandsInternal = new();

	/// <summary>
	/// Gets the registration count.
	/// </summary>
	private static int s_registrationCount { get; set; }

	/// <summary>
	/// Gets the expected count.
	/// </summary>
	private static int s_expectedCount { get; set; }

	/// <summary>
	/// Gets the guild ids where the applications.commands scope is missing.
	/// </summary>
	private IReadOnlyList<ulong> _missingScopeGuildIds;

	/// <summary>
	/// Gets whether debug is enabled.
	/// </summary>
	internal static bool DebugEnabled { get; set; }

	internal static LogLevel ApplicationCommandsLogLevel
		=> DebugEnabled ? LogLevel.Debug : LogLevel.Trace;

	/// <summary>
	/// Gets whether check through all guilds is enabled.
	/// </summary>
	internal static bool CheckAllGuilds { get; set; }

	/// <summary>
	/// Gets whether the registration check should be manually overridden.
	/// </summary>
	internal static bool ManOr { get; set; }

	/// <summary>
	/// Gets whether interactions should be automatically deffered.
	/// </summary>
	internal static bool AutoDeferEnabled { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationCommandsExtension"/> class.
	/// </summary>
	/// <param name="configuration">The configuration.</param>
	internal ApplicationCommandsExtension(ApplicationCommandsConfiguration configuration = null)
	{
		Configuration = configuration;
		DebugEnabled = configuration?.DebugStartup ?? false;
		CheckAllGuilds = configuration?.CheckAllGuilds ?? false;
		ManOr = configuration?.ManualOverride ?? false;
		AutoDeferEnabled = configuration?.AutoDefer ?? false;
	}

	/// <summary>
	/// Runs setup.
	/// <note type="caution">DO NOT RUN THIS MANUALLY. DO NOT DO ANYTHING WITH THIS.</note>
	/// </summary>
	/// <param name="client">The client to setup on.</param>
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;
		ClientInternal = client;

		this._slashError = new AsyncEvent<ApplicationCommandsExtension, SlashCommandErrorEventArgs>("SLASHCOMMAND_ERRORED", TimeSpan.Zero, null);
		this._slashExecuted = new AsyncEvent<ApplicationCommandsExtension, SlashCommandExecutedEventArgs>("SLASHCOMMAND_EXECUTED", TimeSpan.Zero, null);
		this._contextMenuErrored = new AsyncEvent<ApplicationCommandsExtension, ContextMenuErrorEventArgs>("CONTEXTMENU_ERRORED", TimeSpan.Zero, null);
		this._contextMenuExecuted = new AsyncEvent<ApplicationCommandsExtension, ContextMenuExecutedEventArgs>("CONTEXTMENU_EXECUTED", TimeSpan.Zero, null);
		this._applicationCommandsModuleReady = new AsyncEvent<ApplicationCommandsExtension, ApplicationCommandsModuleReadyEventArgs>("APPLICATION_COMMANDS_MODULE_READY", TimeSpan.Zero, null);
		this._applicationCommandsModuleStartupFinished = new AsyncEvent<ApplicationCommandsExtension, ApplicationCommandsModuleStartupFinishedEventArgs>("APPLICATION_COMMANDS_MODULE_STARTUP_FINISHED", TimeSpan.Zero, null);
		this._globalApplicationCommandsRegistered = new AsyncEvent<ApplicationCommandsExtension, GlobalApplicationCommandsRegisteredEventArgs>("GLOBAL_COMMANDS_REGISTERED", TimeSpan.Zero, null);
		this._guildApplicationCommandsRegistered = new AsyncEvent<ApplicationCommandsExtension, GuildApplicationCommandsRegisteredEventArgs>("GUILD_COMMANDS_REGISTERED", TimeSpan.Zero, null);

		this.Client.GuildDownloadCompleted += async (c, e) => await this.UpdateAsync();
		this.Client.InteractionCreated += this.CatchInteractionsOnStartup;
		this.Client.ContextMenuInteractionCreated += this.CatchContextMenuInteractionsOnStartup;
	}

	private async Task CatchInteractionsOnStartup(DiscordClient sender, InteractionCreateEventArgs e)
		=> await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("Attention: This application is still starting up. Application commands are unavailable for now."));

	private async Task CatchContextMenuInteractionsOnStartup(DiscordClient sender, ContextMenuInteractionCreateEventArgs e)
		=> await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("Attention: This application is still starting up. Context menu commands are unavailable for now."));

	private void FinishedRegistration()
	{
		this.Client.InteractionCreated -= this.CatchInteractionsOnStartup;
		this.Client.ContextMenuInteractionCreated -= this.CatchContextMenuInteractionsOnStartup;

		this.Client.InteractionCreated += this.InteractionHandler;
		this.Client.ContextMenuInteractionCreated += this.ContextMenuHandler;
	}
	/// <summary>
	/// Cleans the module for a new start of the bot.
	/// DO NOT USE IF YOU DON'T KNOW WHAT IT DOES.
	/// </summary>
	public void CleanModule()
	{
		this._updateList.Clear();
		s_singletonModules.Clear();
		s_errored = false;
		s_expectedCount = 0;
		s_registrationCount = 0;
		s_commandMethods.Clear();
		s_groupCommands.Clear();
		s_contextMenuCommands.Clear();
		s_subGroupCommands.Clear();
		s_singletonModules.Clear();
		s_registeredCommands.Clear();
		GlobalCommandsInternal.Clear();
		GuildCommandsInternal.Clear();
	}

	/// <summary>
	/// Registers a command class.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	public void RegisterGlobalCommands<T>() where T : ApplicationCommandsModule
	{
		if (this.Client.ShardId == 0)
			this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(null, new ApplicationCommandsModuleConfiguration(typeof(T))));
	}
	/// <summary>
	/// Registers a command class.
	/// </summary>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	public void RegisterGlobalCommands(Type type)
	{
		if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));
		//If sharding, only register for shard 0
		if (this.Client.ShardId == 0)
			this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(null, new ApplicationCommandsModuleConfiguration(type)));
	}

	/// <summary>
	/// Cleans all guild application commands.
	/// <note type="caution">You normally don't need to execute it.</note>
	/// </summary>
	public async Task CleanGuildCommandsAsync()
	{
		foreach (var guild in this.Client.Guilds.Values)
		{
			await this.Client.BulkOverwriteGuildApplicationCommandsAsync(guild.Id, Array.Empty<DiscordApplicationCommand>());
		}
	}

	/// <summary>
	/// Cleans the global application commands.
	/// <note type="caution">You normally don't need to execute it.</note>
	/// </summary>
	public async Task CleanGlobalCommandsAsync()
		=> await this.Client.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());

	/// <summary>
	/// Registers a command class with permission and translation setup.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGuildCommands<T>(ulong guildId, Action<ApplicationCommandsTranslationContext> translationSetup = null) where T : ApplicationCommandsModule
	{
		if (this.Client.ShardId == 0)
			this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(guildId, new ApplicationCommandsModuleConfiguration(typeof(T), translationSetup)));
	}

	/// <summary>
	/// Registers a command class with permission and translation setup.
	/// </summary>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGuildCommands(Type type, ulong guildId, Action<ApplicationCommandsTranslationContext> translationSetup = null)
	{
		if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));
		//If sharding, only register for shard 0
		if (this.Client.ShardId == 0)
			this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(guildId, new ApplicationCommandsModuleConfiguration(type, translationSetup)));
	}

	/// <summary>
	/// Registers a command class with permission setup but without a guild id.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGlobalCommands<T>(Action<ApplicationCommandsTranslationContext> translationSetup = null) where T : ApplicationCommandsModule
	{
		if (this.Client.ShardId == 0)
			this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(null, new ApplicationCommandsModuleConfiguration(typeof(T), translationSetup)));
	}

	/// <summary>
	/// Registers a command class with permission setup but without a guild id.
	/// </summary>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGlobalCommands(Type type, Action<ApplicationCommandsTranslationContext> translationSetup = null)
	{
		if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));
		//If sharding, only register for shard 0
		if (this.Client.ShardId == 0)
			this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>(null, new ApplicationCommandsModuleConfiguration(type, translationSetup)));
	}

	/// <summary>
	/// Fired when the application commands module is ready.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ApplicationCommandsModuleReadyEventArgs> ApplicationCommandsModuleReady
	{
		add => this._applicationCommandsModuleReady.Register(value);
		remove => this._applicationCommandsModuleReady.Unregister(value);
	}
	private AsyncEvent<ApplicationCommandsExtension, ApplicationCommandsModuleReadyEventArgs> _applicationCommandsModuleReady;

	/// <summary>
	/// Fired when the application commands modules startup is finished.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ApplicationCommandsModuleStartupFinishedEventArgs> ApplicationCommandsModuleStartupFinished
	{
		add => this._applicationCommandsModuleStartupFinished.Register(value);
		remove => this._applicationCommandsModuleStartupFinished.Unregister(value);
	}
	private AsyncEvent<ApplicationCommandsExtension, ApplicationCommandsModuleStartupFinishedEventArgs> _applicationCommandsModuleStartupFinished;


	/// <summary>
	/// Fired when guild commands are registered on a guild.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, GuildApplicationCommandsRegisteredEventArgs> GuildApplicationCommandsRegistered
	{
		add => this._guildApplicationCommandsRegistered.Register(value);
		remove => this._guildApplicationCommandsRegistered.Unregister(value);
	}
	private AsyncEvent<ApplicationCommandsExtension, GuildApplicationCommandsRegisteredEventArgs> _guildApplicationCommandsRegistered;

	/// <summary>
	/// Fired when the global commands are registered.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, GlobalApplicationCommandsRegisteredEventArgs> GlobalApplicationCommandsRegistered
	{
		add => this._globalApplicationCommandsRegistered.Register(value);
		remove => this._globalApplicationCommandsRegistered.Unregister(value);
	}
	private AsyncEvent<ApplicationCommandsExtension, GlobalApplicationCommandsRegisteredEventArgs> _globalApplicationCommandsRegistered;

	/// <summary>
	/// Used for RegisterCommands and the <see cref="DisCatSharp.DiscordClient.GuildDownloadCompleted"/> event.
	/// </summary>
	internal async Task UpdateAsync()
	{
		//Only update for shard 0
		if (this.Client.ShardId == 0)
		{
			GlobalDiscordCommands = new();
			GuildDiscordCommands = new();

			var commandsPending = this._updateList.Select(x => x.Key).Distinct();
			s_expectedCount = commandsPending.Count();

			this.Client.Logger.Log(ApplicationCommandsLogLevel, $"Expected Count: {s_expectedCount}");

			List<ulong> failedGuilds = new();
			IEnumerable<DiscordApplicationCommand> globalCommands = null;
			globalCommands = await this.Client.GetGlobalApplicationCommandsAsync(Configuration?.EnableLocalization ?? false) ?? null;
			var guilds = CheckAllGuilds ? this.Client.Guilds?.Keys : this._updateList.Select(x => x.Key)?.Distinct().Where(x => x != null)?.Select(x => x.Value);

			foreach (var guild in guilds)
			{
				IEnumerable<DiscordApplicationCommand> commands = null;
				var unauthorized = false;
				try
				{
					commands = await this.Client.GetGuildApplicationCommandsAsync(guild, Configuration?.EnableLocalization ?? false) ?? null;
				}
				catch (UnauthorizedException)
				{
					unauthorized = true;
				}
				finally
				{
					if (!unauthorized && commands != null && commands.Any())
						GuildDiscordCommands.Add(guild, commands.ToList());
					else if (!unauthorized)
						GuildDiscordCommands.Add(guild, null);
					else
						failedGuilds.Add(guild);
				}
			}

			//Default should be to add the help and slash commands can be added without setting any configuration
			//so this should still add the default help
			if (Configuration is null || (Configuration is not null && Configuration.EnableDefaultHelp))
			{
				this._updateList.Add(new KeyValuePair<ulong?, ApplicationCommandsModuleConfiguration>
					(null, new ApplicationCommandsModuleConfiguration(typeof(DefaultHelpModule))));
				commandsPending = this._updateList.Select(x => x.Key).Distinct();
			}

			if (globalCommands != null && globalCommands.Any())
				GlobalDiscordCommands.AddRange(globalCommands);

			foreach (var key in commandsPending.ToList())
			{
				this.Client.Logger.LogInformation(key.HasValue ? $"Registering commands in guild {key.Value}" : "Registering global commands.");
				await this.RegisterCommands(this._updateList.Where(x => x.Key == key).Select(x => x.Value), key);
			}

			this._missingScopeGuildIds = failedGuilds;

			await this._applicationCommandsModuleReady.InvokeAsync(this, new ApplicationCommandsModuleReadyEventArgs(Configuration?.ServiceProvider)
			{
				Handled = true,
				GuildsWithoutScope = failedGuilds
			});
		}
	}

	/// <summary>
	/// Method for registering commands for a target from modules.
	/// </summary>
	/// <param name="types">The types.</param>
	/// <param name="guildId">The optional guild id.</param>
	private async Task RegisterCommands(IEnumerable<ApplicationCommandsModuleConfiguration> types, ulong? guildId)
	{
		//Initialize empty lists to be added to the global ones at the end
		var commandMethods = new List<CommandMethod>();
		var groupCommands = new List<GroupCommand>();
		var subGroupCommands = new List<SubGroupCommand>();
		var contextMenuCommands = new List<ContextMenuCommand>();
		var updateList = new List<DiscordApplicationCommand>();

		var commandTypeSources = new List<KeyValuePair<Type, Type>>();

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
				if (module.GetCustomAttribute<SlashCommandGroupAttribute>() != null)
				{
					classes.Add(module);
				}
				else
				{
					//Otherwise add the nested groups
					classes = module.DeclaredNestedTypes.Where(x => x.GetCustomAttribute<SlashCommandGroupAttribute>() != null).ToList();
				}

				List<GroupTranslator> groupTranslations = null;

				if (!string.IsNullOrEmpty(ctx.Translations))
				{
					groupTranslations = JsonConvert.DeserializeObject<List<GroupTranslator>>(ctx.Translations);
				}

				var slashGroupsTuple = NestedCommandWorker.ParseSlashGroupsAsync(type, classes, guildId, groupTranslations).Result;

				if (slashGroupsTuple.applicationCommands != null && slashGroupsTuple.applicationCommands.Any())
					updateList.AddRange(slashGroupsTuple.applicationCommands);

				if (slashGroupsTuple.commandTypeSources != null && slashGroupsTuple.commandTypeSources.Any())
					commandTypeSources.AddRange(slashGroupsTuple.commandTypeSources);

				if (slashGroupsTuple.singletonModules != null && slashGroupsTuple.singletonModules.Any())
					s_singletonModules.AddRange(slashGroupsTuple.singletonModules);

				if (slashGroupsTuple.groupCommands != null && slashGroupsTuple.groupCommands.Any())
					groupCommands.AddRange(slashGroupsTuple.groupCommands);

				if (slashGroupsTuple.subGroupCommands != null && slashGroupsTuple.subGroupCommands.Any())
					subGroupCommands.AddRange(slashGroupsTuple.subGroupCommands);

				//Handles methods and context menus, only if the module isn't a group itself
				if (module.GetCustomAttribute<SlashCommandGroupAttribute>() == null)
				{
					List<CommandTranslator> commandTranslations = null;

					if (!string.IsNullOrEmpty(ctx.Translations))
					{
						commandTranslations = JsonConvert.DeserializeObject<List<CommandTranslator>>(ctx.Translations);
					}

					//Slash commands
					var methods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<SlashCommandAttribute>() != null);

					var slashCommands = CommandWorker.ParseBasicSlashCommandsAsync(type, methods, guildId, commandTranslations).Result;

					if (slashCommands.applicationCommands != null && slashCommands.applicationCommands.Any())
						updateList.AddRange(slashCommands.applicationCommands);

					if (slashCommands.commandTypeSources != null && slashCommands.commandTypeSources.Any())
						commandTypeSources.AddRange(slashCommands.commandTypeSources);

					if (slashCommands.commandMethods != null && slashCommands.commandMethods.Any())
						commandMethods.AddRange(slashCommands.commandMethods);

					//Context Menus
					var contextMethods = module.DeclaredMethods.Where(x => x.GetCustomAttribute<ContextMenuAttribute>() != null);

					var contextCommands = CommandWorker.ParseContextMenuCommands(type, contextMethods, commandTranslations).Result;

					if (contextCommands.applicationCommands != null && contextCommands.applicationCommands.Any())
						updateList.AddRange(contextCommands.applicationCommands);

					if (contextCommands.commandTypeSources != null && contextCommands.commandTypeSources.Any())
						commandTypeSources.AddRange(contextCommands.commandTypeSources);

					if (contextCommands.contextMenuCommands != null && contextCommands.contextMenuCommands.Any())
						contextMenuCommands.AddRange(contextCommands.contextMenuCommands);

					//Accounts for lifespans
					if (module.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>() != null && module.GetCustomAttribute<ApplicationCommandModuleLifespanAttribute>().Lifespan == ApplicationCommandModuleLifespan.Singleton)
					{
						s_singletonModules.Add(CreateInstance(module, Configuration?.ServiceProvider));
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is BadRequestException brex)
				{
					this.Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.WebResponse.Response}");
				}
				else
				{
					if (ex.InnerException is not null && ex.InnerException is BadRequestException brex1)
						this.Client.Logger.LogCritical(brex1, $"There was an error registering application commands: {brex1.WebResponse.Response}");
					else
						this.Client.Logger.LogCritical(ex, $"There was an error parsing the application commands");
				}
				s_errored = true;
			}
		}
		if (!s_errored)
		{
			try
			{
				List<DiscordApplicationCommand> commands = new();

				try
				{
					if (guildId == null)
					{
						if (updateList != null && updateList.Any())
						{
							var regCommands = RegistrationWorker.RegisterGlobalCommandsAsync(updateList).Result;
							var actualCommands = regCommands.Distinct().ToList();
							commands.AddRange(actualCommands);
							GlobalCommandsInternal.AddRange(actualCommands);
						}
						else
						{
							foreach (var cmd in GlobalDiscordCommands)
							{
								try
								{
									await this.Client.DeleteGlobalApplicationCommandAsync(cmd.Id);
								}
								catch (NotFoundException)
								{
									this.Client.Logger.Log(ApplicationCommandsLogLevel, $"Could not delete global command {cmd.Id}. Please clean up manually");
								}
							}
						}
					}
					else
					{
						if (updateList != null && updateList.Any())
						{
							var regCommands = RegistrationWorker.RegisterGuildCommandsAsync(guildId.Value, updateList).Result;
							var actualCommands = regCommands.Distinct().ToList();
							commands.AddRange(actualCommands);
							GuildCommandsInternal.Add(guildId.Value, actualCommands);
							if (this.Client.Guilds.TryGetValue(guildId.Value, out var guild))
							{
								guild.InternalRegisteredApplicationCommands = new();
								guild.InternalRegisteredApplicationCommands.AddRange(actualCommands);
							}

						}
						else
						{
							foreach (var cmd in GuildDiscordCommands[guildId.Value])
							{
								try
								{
									await this.Client.DeleteGuildApplicationCommandAsync(guildId.Value, cmd.Id);
								}
								catch (NotFoundException)
								{
									this.Client.Logger.Log(ApplicationCommandsLogLevel, $"Could not delete guild command {cmd.Id} in guild {guildId.Value}. Please clean up manually");
								}
							}
						}
					}
				}
				catch (UnauthorizedException ex)
				{
					this.Client.Logger.LogError($"Could not register application commands for guild {guildId}.\nError: {ex.JsonMessage}");
					return;
				}
				//Creates a guild command if a guild id is specified, otherwise global
				//Checks against the ids and adds them to the command method lists
				foreach (var command in commands)
				{
					if (commandMethods.GetFirstValueWhere(x => x.Name == command.Name, out var com))
						com.CommandId = command.Id;
					else if (groupCommands.GetFirstValueWhere(x => x.Name == command.Name, out var groupCom))
						groupCom.CommandId = command.Id;
					else if (subGroupCommands.GetFirstValueWhere(x => x.Name == command.Name, out var subCom))
						subCom.CommandId = command.Id;
					else if (contextMenuCommands.GetFirstValueWhere(x => x.Name == command.Name, out var cmCom))
						cmCom.CommandId = command.Id;
				}

				//Adds to the global lists finally
				s_commandMethods.AddRange(commandMethods);
				s_groupCommands.AddRange(groupCommands);
				s_subGroupCommands.AddRange(subGroupCommands);
				s_contextMenuCommands.AddRange(contextMenuCommands);

				s_registeredCommands.Add(new KeyValuePair<ulong?, IReadOnlyList<DiscordApplicationCommand>>(guildId, commands.ToList()));

				foreach (var command in commandMethods)
				{
					var app = types.First(t => t.Type == command.Method.DeclaringType);
				}

				this.Client.Logger.Log(ApplicationCommandsLogLevel, $"Expected Count: {s_expectedCount}\nCurrent Count: {s_registrationCount}");

				if (guildId.HasValue)
				{
					await this._guildApplicationCommandsRegistered.InvokeAsync(this, new GuildApplicationCommandsRegisteredEventArgs(Configuration?.ServiceProvider)
					{
						Handled = true,
						GuildId = guildId.Value,
						RegisteredCommands = GuildCommandsInternal.Any(c => c.Key == guildId.Value) ? GuildCommandsInternal.FirstOrDefault(c => c.Key == guildId.Value).Value : null
					});
				}
				else
				{
					await this._globalApplicationCommandsRegistered.InvokeAsync(this, new GlobalApplicationCommandsRegisteredEventArgs(Configuration?.ServiceProvider)
					{
						Handled = true,
						RegisteredCommands = GlobalCommandsInternal
					});
				}

				s_registrationCount++;
				this.CheckRegistrationStartup(ManOr);
			}
			catch (Exception ex)
			{
				if (ex is BadRequestException brex)
				{
					this.Client.Logger.LogCritical(brex, $"There was an error registering application commands: {brex.WebResponse.Response}");
				}
				else
				{
					if (ex.InnerException is not null && ex.InnerException is BadRequestException brex1)
						this.Client.Logger.LogCritical(brex1, $"There was an error registering application commands: {brex1.WebResponse.Response}");
					else
						this.Client.Logger.LogCritical(ex, $"There was an general error registering application commands");
				}
				s_errored = true;
			}
		}
	}

	private async void CheckRegistrationStartup(bool man = false)
	{
		this.Client.Logger.Log(ApplicationCommandsLogLevel, $"Checking counts...\n\nExpected Count: {s_expectedCount}\nCurrent Count: {s_registrationCount}");

		if ((s_registrationCount == s_expectedCount) || man)
		{
			await this._applicationCommandsModuleStartupFinished.InvokeAsync(this, new ApplicationCommandsModuleStartupFinishedEventArgs(Configuration?.ServiceProvider)
			{
				Handled = true,
				RegisteredGlobalCommands = GlobalCommandsInternal,
				RegisteredGuildCommands = GuildCommandsInternal,
				GuildsWithoutScope = this._missingScopeGuildIds
			});

			this.FinishedRegistration();
		}
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
					Services = Configuration?.ServiceProvider,
					ResolvedUserMentions = e.Interaction.Data.Resolved?.Users?.Values.ToList(),
					ResolvedRoleMentions = e.Interaction.Data.Resolved?.Roles?.Values.ToList(),
					ResolvedChannelMentions = e.Interaction.Data.Resolved?.Channels?.Values.ToList(),
					ResolvedAttachments = e.Interaction.Data.Resolved?.Attachments?.Values.ToList(),
					Type = ApplicationCommandType.ChatInput,
					Locale = e.Interaction.Locale,
					GuildLocale = e.Interaction.GuildLocale
				};

				try
				{
					if (s_errored)
						throw new InvalidOperationException("Slash commands failed to register properly on startup.");

					var methods = s_commandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
					var groups = s_groupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
					var subgroups = s_subGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
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
				if (s_errored)
					throw new InvalidOperationException("Slash commands failed to register properly on startup.");

				var methods = s_commandMethods.Where(x => x.CommandId == e.Interaction.Data.Id);
				var groups = s_groupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
				var subgroups = s_subGroupCommands.Where(x => x.CommandId == e.Interaction.Data.Id);
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
							Services = Configuration?.ServiceProvider,
							ApplicationCommandsExtension = this,
							Guild = e.Interaction.Guild,
							Channel = e.Interaction.Channel,
							User = e.Interaction.User,
							Options = e.Interaction.Data.Options.ToList(),
							FocusedOption = focusedOption,
							Locale = e.Interaction.Locale,
							GuildLocale = e.Interaction.GuildLocale
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
							Services = Configuration?.ServiceProvider,
							ApplicationCommandsExtension = this,
							Guild = e.Interaction.Guild,
							Channel = e.Interaction.Channel,
							User = e.Interaction.User,
							Options = command.Options.ToList(),
							FocusedOption = focusedOption,
							Locale = e.Interaction.Locale,
							GuildLocale = e.Interaction.GuildLocale
						};

						var choices = await (Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>) providerMethod.Invoke(providerInstance, new[] { context });
						await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
					}
					else if (subgroups.Any())
					{
						var command = e.Interaction.Data.Options.First();
						var group = subgroups.First().SubCommands.First(x => x.Name == command.Name).Methods.First(x => x.Key == command.Options.First().Name).Value;

						var focusedOption = command.Options.First().Options.First(o => o.Focused);

						var option = group.GetParameters().Skip(1).First(p => p.GetCustomAttribute<OptionAttribute>().Name == focusedOption.Name);
						var provider = option.GetCustomAttribute<AutocompleteAttribute>().ProviderType;
						var providerMethod = provider.GetMethod(nameof(IAutocompleteProvider.Provider));
						var providerInstance = Activator.CreateInstance(provider);

						var context = new AutocompleteContext
						{
							Interaction = e.Interaction,
							Services = Configuration?.ServiceProvider,
							ApplicationCommandsExtension = this,
							Guild = e.Interaction.Guild,
							Channel = e.Interaction.Channel,
							User = e.Interaction.User,
							Options = command.Options.First().Options.ToList(),
							FocusedOption = focusedOption,
							Locale = e.Interaction.Locale,
							GuildLocale = e.Interaction.GuildLocale
						};

						var choices = await (Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>>) providerMethod.Invoke(providerInstance, new[] { context });
						await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
					}
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
				Services = Configuration?.ServiceProvider,
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
				GuildLocale = e.Interaction.GuildLocale
			};

			try
			{
				if (s_errored)
					throw new InvalidOperationException("Context menus failed to register properly on startup.");

				//Gets the method for the command
				var method = s_contextMenuCommands.FirstOrDefault(x => x.CommandId == e.Interaction.Data.Id);

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
				classInstance = method.IsStatic ? ActivatorUtilities.CreateInstance(Configuration?.ServiceProvider.CreateScope().ServiceProvider, method.DeclaringType) : CreateInstance(method.DeclaringType, Configuration?.ServiceProvider.CreateScope().ServiceProvider);
				break;

			case ApplicationCommandModuleLifespan.Transient:
				//Accounts for static methods and adds DI
				classInstance = method.IsStatic ? ActivatorUtilities.CreateInstance(Configuration?.ServiceProvider, method.DeclaringType) : CreateInstance(method.DeclaringType, Configuration?.ServiceProvider);
				break;

			//If singleton, gets it from the singleton list
			case ApplicationCommandModuleLifespan.Singleton:
				classInstance = s_singletonModules.First(x => ReferenceEquals(x.GetType(), method.DeclaringType));
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
				if (AutoDeferEnabled)
					await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
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
				if (AutoDeferEnabled)
					await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
				await (Task)method.Invoke(classInstance, args.ToArray());

				await (module?.AfterContextMenuExecutionAsync(contextMenuContext) ?? Task.CompletedTask);
			}
		}
	}

	/// <summary>
	/// Property injection
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

		foreach (var parameter in parameters)
		{
			//Accounts for optional arguments without values given
			if (parameter.IsOptional && (options == null || (!options?.Any(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower()) ?? true)))
				args.Add(parameter.DefaultValue);
			else
			{
				var option = options.Single(x => x.Name == parameter.GetCustomAttribute<OptionAttribute>().Name.ToLower());

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
				else if (parameter.ParameterType == typeof(int) || parameter.ParameterType == typeof(int?))
					args.Add((int?)option.Value);
				else if (parameter.ParameterType == typeof(DiscordAttachment))
				{
					//Checks through resolved
					if (e.Interaction.Data.Resolved.Attachments != null &&
						e.Interaction.Data.Resolved.Attachments.TryGetValue((ulong)option.Value, out var attachment))
						args.Add(attachment);
					else
						args.Add(new DiscordAttachment() { Id = (ulong)option.Value, Discord = this.Client.ApiClient.Discord });
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
	/// Runs the pre-execution checks.
	/// </summary>
	/// <param name="method">The method info.</param>
	/// <param name="context">The base context.</param>
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
		if (context is ContextMenuContext cMctx)
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
				var result = await att.ExecuteChecksAsync(cMctx);
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
	/// <param name="guildId">The optional guild id</param>
	private static async Task<List<DiscordApplicationCommandOptionChoice>> GetChoiceAttributesFromProvider(IEnumerable<ChoiceProviderAttribute> customAttributes, ulong? guildId = null)
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
						?.SetValue(instance, Configuration.ServiceProvider);
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
	/// Gets the choice attributes from parameter.
	/// </summary>
	/// <param name="choiceAttributes">The choice attributes.</param>
	private static List<DiscordApplicationCommandOptionChoice> GetChoiceAttributesFromParameter(IEnumerable<ChoiceAttribute> choiceAttributes) =>
		!choiceAttributes.Any()
			? null
			: choiceAttributes.Select(att => new DiscordApplicationCommandOptionChoice(att.Name, att.Value)).ToList();

	/// <summary>
	/// Parses the parameters.
	/// </summary>
	/// <param name="parameters">The parameters.</param>
	/// <param name="guildId">The optional guild id.</param>
	internal static async Task<List<DiscordApplicationCommandOption>> ParseParametersAsync(IEnumerable<ParameterInfo> parameters, ulong? guildId)
	{
		var options = new List<DiscordApplicationCommandOption>();
		foreach (var parameter in parameters)
		{
			//Gets the attribute
			var optionAttribute = parameter.GetCustomAttribute<OptionAttribute>();
			if (optionAttribute == null)
				throw new ArgumentException("Arguments must have the Option attribute!");

			var minimumValue = parameter.GetCustomAttribute<MinimumAttribute>()?.Value ?? null;
			var maximumValue = parameter.GetCustomAttribute<MaximumAttribute>()?.Value ?? null;


			var autocompleteAttribute = parameter.GetCustomAttribute<AutocompleteAttribute>();
			if (optionAttribute.Autocomplete && autocompleteAttribute == null)
				throw new ArgumentException("Autocomplete options must have the Autocomplete attribute!");
			if (!optionAttribute.Autocomplete && autocompleteAttribute != null)
				throw new ArgumentException("Setting an autocomplete provider requires the option to have autocomplete set to true!");

			//Sets the type
			var type = parameter.ParameterType;
			var parameterType = GetParameterType(type);

			//Handles choices
			//From attributes
			var choices = GetChoiceAttributesFromParameter(parameter.GetCustomAttributes<ChoiceAttribute>());
			//From enums
			if (parameter.ParameterType.IsEnum)
			{
				choices = GetChoiceAttributesFromEnumParameter(parameter.ParameterType);
			}
			//From choice provider
			var choiceProviders = parameter.GetCustomAttributes<ChoiceProviderAttribute>();
			if (choiceProviders.Any())
			{
				choices = await GetChoiceAttributesFromProvider(choiceProviders, guildId);
			}

			var channelTypes = parameter.GetCustomAttribute<ChannelTypesAttribute>()?.ChannelTypes ?? null;

			options.Add(new DiscordApplicationCommandOption(optionAttribute.Name, optionAttribute.Description, parameterType, !parameter.IsOptional, choices, null, channelTypes, optionAttribute.Autocomplete, minimumValue, maximumValue));
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
		s_commandMethods.Clear();
		s_groupCommands.Clear();
		s_subGroupCommands.Clear();
		s_registeredCommands.Clear();
		s_contextMenuCommands.Clear();
		GlobalDiscordCommands.Clear();
		GuildDiscordCommands.Clear();
		GuildCommandsInternal.Clear();
		GlobalCommandsInternal.Clear();
		GlobalDiscordCommands = null;
		GuildDiscordCommands = null;
		s_errored = false;

		if (Configuration != null && Configuration.EnableDefaultHelp)
		{
			this._updateList.RemoveAll(x => x.Value.Type == typeof(DefaultHelpModule));
		}

		await this.UpdateAsync();
	}

	/// <summary>
	/// Fires when the execution of a slash command fails.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, SlashCommandErrorEventArgs> SlashCommandErrored
	{
		add => this._slashError.Register(value);
		remove => this._slashError.Unregister(value);
	}
	private AsyncEvent<ApplicationCommandsExtension, SlashCommandErrorEventArgs> _slashError;

	/// <summary>
	/// Fires when the execution of a slash command is successful.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, SlashCommandExecutedEventArgs> SlashCommandExecuted
	{
		add => this._slashExecuted.Register(value);
		remove => this._slashExecuted.Unregister(value);
	}
	private AsyncEvent<ApplicationCommandsExtension, SlashCommandExecutedEventArgs> _slashExecuted;

	/// <summary>
	/// Fires when the execution of a context menu fails.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ContextMenuErrorEventArgs> ContextMenuErrored
	{
		add => this._contextMenuErrored.Register(value);
		remove => this._contextMenuErrored.Unregister(value);
	}
	private AsyncEvent<ApplicationCommandsExtension, ContextMenuErrorEventArgs> _contextMenuErrored;

	/// <summary>
	/// Fire when the execution of a context menu is successful.
	/// </summary>
	public event AsyncEventHandler<ApplicationCommandsExtension, ContextMenuExecutedEventArgs> ContextMenuExecuted
	{
		add => this._contextMenuExecuted.Register(value);
		remove => this._contextMenuExecuted.Unregister(value);
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
	/// The translation setup.
	/// </summary>
	public Action<ApplicationCommandsTranslationContext> Translations { get; }

	/// <summary>
	/// Creates a new command configuration.
	/// </summary>
	/// <param name="type">The type of the command module.</param>
	/// <param name="translations">The translation setup callback.</param>
	public ApplicationCommandsModuleConfiguration(Type type, Action<ApplicationCommandsTranslationContext> translations = null)
	{
		this.Type = type;
		this.Translations = translations;
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
	public List<GroupCommand> SubCommands { get; set; } = new();
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

#region Default Help
/// <summary>
/// Represents the default help module.
/// </summary>
internal class DefaultHelpModule : ApplicationCommandsModule
{
	public class DefaultHelpAutoCompleteProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			var options = new List<DiscordApplicationCommandAutocompleteChoice>();

			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First())
					.Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase))
					.ToList();
			}
			else
			{
				await Task.WhenAll(globalCommandsTask);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First())
					.Where(ac => ac.Name.StartsWith(context.Options[0].Value.ToString(), StringComparison.OrdinalIgnoreCase))
					.ToList();
			}

			foreach (var sc in slashCommands.Take(25))
			{
				options.Add(new DiscordApplicationCommandAutocompleteChoice(sc.Name, sc.Name.Trim()));
			}
			return options.AsEnumerable();
		}
	}

	public class DefaultHelpAutoCompleteLevelOneProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			var options = new List<DiscordApplicationCommandAutocompleteChoice>();
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}
			else
			{
				await Task.WhenAll(globalCommandsTask);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}

			var command = slashCommands.FirstOrDefault(ac =>
				ac.Name.Equals(context.Options[0].Value.ToString().Trim(),StringComparison.OrdinalIgnoreCase));
			if (command is null || command.Options is null)
			{
				options.Add(new DiscordApplicationCommandAutocompleteChoice("no_options_for_this_command", "no_options_for_this_command"));
			}
			else
			{
				var opt = command.Options.Where(c => c.Type is ApplicationCommandOptionType.SubCommandGroup or ApplicationCommandOptionType.SubCommand
													 && c.Name.StartsWith(context.Options[1].Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).ToList();
				foreach (var option in opt.Take(25))
				{
					options.Add(new DiscordApplicationCommandAutocompleteChoice(option.Name, option.Name.Trim()));
				}
			}
			return options.AsEnumerable();
		}
	}

	public class DefaultHelpAutoCompleteLevelTwoProvider : IAutocompleteProvider
	{
		public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context)
		{
			var options = new List<DiscordApplicationCommandAutocompleteChoice>();
			IEnumerable<DiscordApplicationCommand> slashCommands = null;
			var globalCommandsTask = context.Client.GetGlobalApplicationCommandsAsync();
			if (context.Guild != null)
			{
				var guildCommandsTask = context.Client.GetGuildApplicationCommandsAsync(context.Guild.Id);
				await Task.WhenAll(globalCommandsTask, guildCommandsTask);
				slashCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}
			else
			{
				await Task.WhenAll(globalCommandsTask);
				slashCommands = globalCommandsTask.Result
					.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
					.GroupBy(ac => ac.Name).Select(x => x.First());
			}

			var command = slashCommands.FirstOrDefault(ac =>
				ac.Name.Equals(context.Options[0].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (command.Options is null)
			{
				options.Add(new DiscordApplicationCommandAutocompleteChoice("no_options_for_this_command", "no_options_for_this_command"));
				return options.AsEnumerable();
			}
			var foundCommand = command.Options.FirstOrDefault(op => op.Name.Equals(context.Options[1].Value.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
			if (foundCommand is null || foundCommand.Options is null)
			{
				options.Add(new DiscordApplicationCommandAutocompleteChoice("no_options_for_this_command", "no_options_for_this_command"));
			}
			else
			{
				var opt = foundCommand.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand &&
														  x.Name.StartsWith(context.Options[2].Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
				foreach (var option in opt.Take(25))
				{
					options.Add(new DiscordApplicationCommandAutocompleteChoice(option.Name, option.Name.Trim()));
				}
			}
			return options.AsEnumerable();
		}
	}

	[SlashCommand("help", "Displays command help")]
	internal async Task DefaultHelpAsync(InteractionContext ctx,
		[Autocomplete(typeof(DefaultHelpAutoCompleteProvider))]
		[Option("option_one", "top level command to provide help for", true)] string commandName,
		[Autocomplete(typeof(DefaultHelpAutoCompleteLevelOneProvider))]
		[Option("option_two", "subgroup or command to provide help for", true)] string commandOneName = null,
		[Autocomplete(typeof(DefaultHelpAutoCompleteLevelTwoProvider))]
		[Option("option_three", "command to provide help for", true)] string commandTwoName = null)
	{
		var globalCommandsTask = ctx.Client.GetGlobalApplicationCommandsAsync();
		var guildCommandsTask= ctx.Client.GetGuildApplicationCommandsAsync(ctx.Guild.Id);

		await Task.WhenAll(globalCommandsTask, guildCommandsTask);

		var applicationCommands = globalCommandsTask.Result.Concat(guildCommandsTask.Result)
			.Where(ac => !ac.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
			.GroupBy(ac => ac.Name).Select(x => x.First())
			.ToList();
		if (applicationCommands.Count < 1)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
				.WithContent($"There are no slash commands for guild {ctx.Guild.Name}").AsEphemeral(true));
			return;
		}
		if (commandTwoName is not null && !commandTwoName.Equals("no_options_for_this_command"))
		{
			var commandsWithSubCommands = applicationCommands.FindAll(ac => ac.Options is not null && ac.Options.Any(op => op.Type == ApplicationCommandOptionType.SubCommandGroup));
			var cmdParent = commandsWithSubCommands.FirstOrDefault(cm => cm.Options.Any(op => op.Name.Equals(commandOneName))).Options
					.FirstOrDefault(opt => opt.Name.Equals(commandOneName,StringComparison.OrdinalIgnoreCase));
			var cmd = cmdParent.Options.FirstOrDefault(op => op.Name.Equals(commandTwoName,StringComparison.OrdinalIgnoreCase));
			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{Formatter.InlineCode(cmd.Name)}: {cmd.Description ?? "No description provided."}"
			};
			if (cmd.Options is not null)
			{
				var commandOptions = cmd.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append(" (").Append(")`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new DiscordEmbedField("Arguments", sb.ToString().Trim()));
			}
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral(true));
		}
		else if (commandOneName is not null && commandTwoName is null && !commandOneName.Equals("no_options_for_this_command"))
		{
			var commandsWithOptions = applicationCommands.FindAll(ac => ac.Options is not null && ac.Options.All(op => op.Type == ApplicationCommandOptionType.SubCommand));
			var subCommandParent = commandsWithOptions.FirstOrDefault(cm => cm.Name.Equals(commandName,StringComparison.OrdinalIgnoreCase));
			var subCommand = subCommandParent.Options.FirstOrDefault(op => op.Name.Equals(commandOneName,StringComparison.OrdinalIgnoreCase));
			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{Formatter.InlineCode(subCommand.Name)}: {subCommand.Description ?? "No description provided."}"
			};
			if (subCommand.Options is not null)
			{
				var commandOptions = subCommand.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append(" (").Append(")`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new DiscordEmbedField("Arguments", sb.ToString().Trim()));
			}
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral(true));
		}
		else
		{
			var command = applicationCommands.FirstOrDefault(cm => cm.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			if (command is null)
			{
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
						.WithContent($"No command called {commandName} in guild {ctx.Guild.Name}").AsEphemeral(true));
				return;
			}
			var discordEmbed = new DiscordEmbedBuilder
			{
				Title = "Help",
				Description = $"{Formatter.InlineCode(command.Name)}: {command.Description ?? "No description provided."}"
			}.AddField(new DiscordEmbedField("Command is NSFW", command.IsNsfw.ToString()));
			if (command.Options is not null)
			{
				var commandOptions = command.Options.ToList();
				var sb = new StringBuilder();

				foreach (var option in commandOptions)
					sb.Append('`').Append(option.Name).Append(" (").Append(")`: ").Append(option.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
				discordEmbed.AddField(new DiscordEmbedField("Arguments", sb.ToString().Trim()));
			}
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral(true));
		}
	}
}
#endregion
