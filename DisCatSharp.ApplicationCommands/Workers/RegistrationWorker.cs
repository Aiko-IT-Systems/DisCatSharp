using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Checks;
using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.Exceptions;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.ApplicationCommands.Workers;

/// <summary>
/// Represents a <see cref="RegistrationWorker"/>.
/// </summary>
internal class RegistrationWorker
{
	/// <summary>
	/// Registers the global commands.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="commands">The command list.</param>
	/// <returns>A list of registered commands.</returns>
	internal static async Task<List<DiscordApplicationCommand>?> RegisterGlobalCommandsAsync(DiscordClient client, List<DiscordApplicationCommand> commands)
	{
		var (changedCommands, unchangedCommands) = BuildGlobalOverwriteList(client, commands);
		var globalCommandsCreateList = BuildGlobalCreateList(client, commands);
		var globalCommandsDeleteList = BuildGlobalDeleteList(client, commands);

		if (globalCommandsCreateList!.NotEmptyAndNotNull() && unchangedCommands!.NotEmptyAndNotNull() && changedCommands!.NotEmptyAndNotNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GLOBAL] Creating, re-using and overwriting application commands");

			foreach (var cmd in globalCommandsCreateList)
			{
				var discordBackendCommand = await client.CreateGlobalApplicationCommandAsync(cmd).ConfigureAwait(false);
				commands.Add(discordBackendCommand);
			}

			foreach (var (key, command) in changedCommands)
			{
				var discordBackendCommand = await client.EditGlobalApplicationCommandAsync(key, action =>
				{
					action.Name = command.Name;
					action.NameLocalizations = command.NameLocalizations;
					action.Description = command.Description;
					action.DescriptionLocalizations = command.DescriptionLocalizations;
					if (command.Options is not null && command.Options.Count is not 0)
						action.Options = Optional.FromNullable(command.Options);
					action.DefaultMemberPermissions = command.DefaultMemberPermissions;
					action.DmPermission = command.DmPermission ?? true;
					action.IsNsfw = command.IsNsfw;
					action.AllowedContexts = command.AllowedContexts;
					action.IntegrationTypes = command.IntegrationTypes;
				}).ConfigureAwait(false);

				commands.Add(discordBackendCommand);
			}

			commands.AddRange(unchangedCommands);
		}
		else if (globalCommandsCreateList!.NotEmptyAndNotNull() && (unchangedCommands!.NotEmptyAndNotNull() || changedCommands!.NotEmptyAndNotNull()))
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GLOBAL] Creating, re-using and overwriting application commands");

			foreach (var cmd in globalCommandsCreateList)
			{
				var discordBackendCommand = await client.CreateGlobalApplicationCommandAsync(cmd).ConfigureAwait(false);
				commands.Add(discordBackendCommand);
			}

			if (changedCommands!.NotEmptyAndNotNull())
				foreach (var (key, command) in changedCommands)
				{
					var discordBackendCommand = await client.EditGlobalApplicationCommandAsync(key, action =>
					{
						action.Name = command.Name;
						action.NameLocalizations = command.NameLocalizations;
						action.Description = command.Description;
						action.DescriptionLocalizations = command.DescriptionLocalizations;
						if (command.Options != null && command.Options.Count != 0)
							action.Options = Optional.FromNullable(command.Options);
						action.DefaultMemberPermissions = command.DefaultMemberPermissions;
						action.DmPermission = command.DmPermission ?? true;
						action.IsNsfw = command.IsNsfw;
						action.AllowedContexts = command.AllowedContexts;
						action.IntegrationTypes = command.IntegrationTypes;
					}).ConfigureAwait(false);

					commands.Add(discordBackendCommand);
				}

			if (unchangedCommands!.NotEmptyAndNotNull())
				commands.AddRange(unchangedCommands);
		}
		else if (globalCommandsCreateList!.EmptyOrNull() && unchangedCommands!.NotEmptyAndNotNull() && changedCommands!.NotEmptyAndNotNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GLOBAL] Editing & re-using application commands");

			foreach (var (key, command) in changedCommands)
			{
				var discordBackendCommand = await client.EditGlobalApplicationCommandAsync(key, action =>
				{
					action.Name = command.Name;
					action.NameLocalizations = command.NameLocalizations;
					action.Description = command.Description;
					action.DescriptionLocalizations = command.DescriptionLocalizations;
					if (command.Options != null && command.Options.Count != 0)
						action.Options = Optional.FromNullable(command.Options);
					action.DefaultMemberPermissions = command.DefaultMemberPermissions;
					action.DmPermission = command.DmPermission ?? true;
					action.IsNsfw = command.IsNsfw;
					action.AllowedContexts = command.AllowedContexts;
					action.IntegrationTypes = command.IntegrationTypes;
				}).ConfigureAwait(false);

				commands.Add(discordBackendCommand);
			}

			commands.AddRange(unchangedCommands);
		}
		else if (globalCommandsCreateList!.EmptyOrNull() && changedCommands!.NotEmptyAndNotNull() && unchangedCommands!.EmptyOrNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GLOBAL] Overwriting all application commands");

			List<DiscordApplicationCommand> overwriteList = [];
			foreach (var (key, cmd) in changedCommands)
			{
				cmd.Id = key;
				overwriteList.Add(cmd);
			}

			var discordBackendCommands = await client.BulkOverwriteGlobalApplicationCommandsAsync(overwriteList).ConfigureAwait(false);
			commands.AddRange(discordBackendCommands);
		}
		else if (globalCommandsCreateList!.NotEmptyAndNotNull() && changedCommands!.EmptyOrNull() && unchangedCommands!.EmptyOrNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GLOBAL] Creating all application commands");

			var cmds = await client.BulkOverwriteGlobalApplicationCommandsAsync(globalCommandsCreateList).ConfigureAwait(false);
			commands.AddRange(cmds);
		}
		else if (globalCommandsCreateList!.EmptyOrNull() && changedCommands!.EmptyOrNull() && unchangedCommands!.NotEmptyAndNotNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GLOBAL] Re-using all application commands");

			commands.AddRange(unchangedCommands);
		}

		if (!globalCommandsDeleteList.NotEmptyAndNotNull())
			return commands.NotEmpty() ? commands : null;

		client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GLOBAL] Deleting missing application commands");

		foreach (var cmdId in globalCommandsDeleteList)
			try
			{
				await client.DeleteGlobalApplicationCommandAsync(cmdId).ConfigureAwait(false);
			}
			catch (NotFoundException)
			{
				client.Logger.LogError("Could not delete global command {cmd}. Please clean up manually", cmdId);
			}

		return commands.NotEmpty() ? commands : null;
	}

	/// <summary>
	/// Registers the guild commands.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="guildId">The target guild id.</param>
	/// <param name="commands">The command list.</param>
	/// <returns>A list of registered commands.</returns>
	internal static async Task<List<DiscordApplicationCommand>?> RegisterGuildCommandsAsync(DiscordClient client, ulong guildId, List<DiscordApplicationCommand> commands)
	{
		var (changedCommands, unchangedCommands) = BuildGuildOverwriteList(client, guildId, commands);
		var guildCommandsCreateList = BuildGuildCreateList(client, guildId, commands);
		var guildCommandsDeleteList = BuildGuildDeleteList(client, guildId, commands);

		if (guildCommandsCreateList!.NotEmptyAndNotNull() && unchangedCommands!.NotEmptyAndNotNull() && changedCommands!.NotEmptyAndNotNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GUILD] Creating, re-using and overwriting application commands. Guild ID: {guild}", guildId);

			foreach (var cmd in guildCommandsCreateList)
			{
				var discordBackendCommand = await client.CreateGuildApplicationCommandAsync(guildId, cmd).ConfigureAwait(false);
				commands.Add(discordBackendCommand);
			}

			foreach (var (key, command) in changedCommands)
			{
				var discordBackendCommand = await client.EditGuildApplicationCommandAsync(guildId, key, action =>
				{
					action.Name = command.Name;
					action.NameLocalizations = command.NameLocalizations;
					action.Description = command.Description;
					action.DescriptionLocalizations = command.DescriptionLocalizations;
					if (command.Options != null && command.Options.Count != 0)
						action.Options = Optional.FromNullable(command.Options);
					action.DefaultMemberPermissions = command.DefaultMemberPermissions;
					action.DmPermission = command.DmPermission ?? true;
					action.IsNsfw = command.IsNsfw;
					action.AllowedContexts = command.AllowedContexts;
					action.IntegrationTypes = command.IntegrationTypes;
				}).ConfigureAwait(false);

				commands.Add(discordBackendCommand);
			}

			commands.AddRange(unchangedCommands);
		}
		else if (guildCommandsCreateList!.NotEmptyAndNotNull() && (unchangedCommands!.NotEmptyAndNotNull() || changedCommands!.NotEmptyAndNotNull()))
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GUILD] Creating, re-using and overwriting application commands. Guild ID: {guild}", guildId);

			foreach (var cmd in guildCommandsCreateList)
			{
				var discordBackendCommand = await client.CreateGuildApplicationCommandAsync(guildId, cmd).ConfigureAwait(false);
				commands.Add(discordBackendCommand);
			}

			if (changedCommands!.NotEmptyAndNotNull())
				foreach (var (key, command) in changedCommands)
				{
					var discordBackendCommand = await client.EditGuildApplicationCommandAsync(guildId, key, action =>
					{
						action.Name = command.Name;
						action.NameLocalizations = command.NameLocalizations;
						action.Description = command.Description;
						action.DescriptionLocalizations = command.DescriptionLocalizations;
						if (command.Options != null && command.Options.Count != 0)
							action.Options = Optional.FromNullable(command.Options);
						action.DefaultMemberPermissions = command.DefaultMemberPermissions;
						action.DmPermission = command.DmPermission ?? true;
						action.IsNsfw = command.IsNsfw;
						action.AllowedContexts = command.AllowedContexts;
						action.IntegrationTypes = command.IntegrationTypes;
					}).ConfigureAwait(false);

					commands.Add(discordBackendCommand);
				}

			if (unchangedCommands!.NotEmptyAndNotNull())
				commands.AddRange(unchangedCommands);
		}
		else if (guildCommandsCreateList!.EmptyOrNull() && unchangedCommands!.NotEmptyAndNotNull() && changedCommands!.NotEmptyAndNotNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GUILD] Editing & re-using application commands. Guild ID: {guild}", guildId);

			foreach (var (key, command) in changedCommands)
			{
				var discordBackendCommand = await client.EditGuildApplicationCommandAsync(guildId, key, action =>
				{
					action.Name = command.Name;
					action.NameLocalizations = command.NameLocalizations;
					action.Description = command.Description;
					action.DescriptionLocalizations = command.DescriptionLocalizations;
					if (command.Options != null && command.Options.Count != 0)
						action.Options = Optional.FromNullable(command.Options);
					if (command.DefaultMemberPermissions.HasValue)
						action.DefaultMemberPermissions = command.DefaultMemberPermissions.Value;
					if (command.DmPermission.HasValue)
						action.DmPermission = command.DmPermission.Value;
					action.IsNsfw = command.IsNsfw;
					action.AllowedContexts = command.AllowedContexts;
					action.IntegrationTypes = command.IntegrationTypes;
				}).ConfigureAwait(false);

				commands.Add(discordBackendCommand);
			}

			commands.AddRange(unchangedCommands);
		}
		else if (guildCommandsCreateList!.EmptyOrNull() && changedCommands!.NotEmptyAndNotNull() && unchangedCommands!.EmptyOrNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GUILD] Overwriting all application commands. Guild ID: {guild}", guildId);

			List<DiscordApplicationCommand> overwriteList = [];
			foreach (var (key, cmd) in changedCommands)
			{
				cmd.Id = key;
				overwriteList.Add(cmd);
			}

			var discordBackendCommands = await client.BulkOverwriteGuildApplicationCommandsAsync(guildId, overwriteList).ConfigureAwait(false);
			commands.AddRange(discordBackendCommands);
		}
		else if (guildCommandsCreateList!.NotEmptyAndNotNull() && changedCommands!.EmptyOrNull() && unchangedCommands!.EmptyOrNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GUILD] Creating all application commands. Guild ID: {guild}", guildId);

			var cmds = await client.BulkOverwriteGuildApplicationCommandsAsync(guildId, guildCommandsCreateList).ConfigureAwait(false);
			commands.AddRange(cmds);
		}
		else if (guildCommandsCreateList!.EmptyOrNull() && changedCommands!.EmptyOrNull() && unchangedCommands!.NotEmptyAndNotNull())
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GUILD] Re-using all application commands Guild ID: {guild}", guildId);

			commands.AddRange(unchangedCommands);
		}

		if (!guildCommandsDeleteList.NotEmptyAndNotNull())
			return commands.NotEmpty() ? commands : null;

		foreach (var cmdId in guildCommandsDeleteList)
		{
			client.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, "[AC GUILD] Deleting missing application commands. Guild ID: {guild}", guildId);
			try
			{
				await client.DeleteGuildApplicationCommandAsync(guildId, cmdId).ConfigureAwait(false);
			}
			catch (NotFoundException)
			{
				client.Logger.LogError("Could not delete guild command {cmd} in guild {guild}. Please clean up manually", cmdId, guildId);
			}
		}

		return commands.NotEmpty() ? commands : null;
	}

	/// <summary>
	/// Builds a list of guild command ids to be deleted on discords backend.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="guildId">The guild id these commands belong to.</param>
	/// <param name="updateList">The command list.</param>
	/// <returns>A list of command ids.</returns>
	private static List<ulong>? BuildGuildDeleteList(DiscordClient client, ulong guildId, List<DiscordApplicationCommand>? updateList = null)
	{
		if (ApplicationCommandsExtension.GuildDiscordCommands.Count is 0
		    || !ApplicationCommandsExtension.GuildDiscordCommands!.TryGetFirstValueByKey(guildId, out var discord)
		)
			return null;

		List<ulong> invalidCommandIds = [];

		if (discord is null)
			return null;

		if (updateList is null)
			invalidCommandIds.AddRange(discord.Select(cmd => cmd.Id));
		else
			invalidCommandIds.AddRange(from cmd in discord where updateList.All(ul => ul.Name != cmd.Name) select cmd.Id);

		return invalidCommandIds;
	}

	/// <summary>
	/// Builds a list of guild commands to be created on discords backend.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="guildId">The guild id these commands belong to.</param>
	/// <param name="updateList">The command list.</param>
	/// <returns></returns>
	private static List<DiscordApplicationCommand>? BuildGuildCreateList(DiscordClient client, ulong guildId, List<DiscordApplicationCommand>? updateList = null)
	{
		if (ApplicationCommandsExtension.GuildDiscordCommands.Count is 0
		    || updateList is null || !ApplicationCommandsExtension.GuildDiscordCommands!.TryGetFirstValueByKey(guildId, out var discord)
		)
			return null;

		List<DiscordApplicationCommand> newCommands = [];

		if (discord is null)
			return updateList;

		newCommands.AddRange(updateList.Where(cmd => discord.All(d => d.Name != cmd.Name)));

		return newCommands;
	}

	/// <summary>
	/// Builds a list of guild commands to be overwritten on discords backend.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="guildId">The guild id these commands belong to.</param>
	/// <param name="updateList">The command list.</param>
	/// <returns>A dictionary of command id and command.</returns>
	private static (
		Dictionary<ulong, DiscordApplicationCommand>? changedCommands,
		List<DiscordApplicationCommand>? unchangedCommands
		) BuildGuildOverwriteList(DiscordClient client, ulong guildId, List<DiscordApplicationCommand>? updateList = null)
	{
		if (ApplicationCommandsExtension.GuildDiscordCommands.Count is 0
		    || ApplicationCommandsExtension.GuildDiscordCommands.All(l => l.Key != guildId) || updateList is null
		    || !ApplicationCommandsExtension.GuildDiscordCommands!.TryGetFirstValueByKey(guildId, out var discord)
		)
			return (null, null);

		Dictionary<ulong, DiscordApplicationCommand> updateCommands = [];
		List<DiscordApplicationCommand> unchangedCommands = [];

		if (discord is null)
			return (null, null);

		foreach (var cmd in updateList)
			if (discord!.TryGetFirstValueWhere(d => d?.Name == cmd.Name, out var command))
				if (command.IsEqualTo(cmd, client, true))
				{
					if (ApplicationCommandsExtension.DebugEnabled)
						client.Logger.LogDebug("[AC] Command {cmdName} unchanged", cmd.Name);
					cmd.Id = command.Id;
					cmd.ApplicationId = command.ApplicationId;
					cmd.Version = command.Version;
					unchangedCommands.Add(cmd);
				}
				else
				{
					if (ApplicationCommandsExtension.DebugEnabled)
						client.Logger.LogDebug("[AC] Command {cmdName} changed", cmd.Name);
					updateCommands.Add(command.Id, cmd);
				}

		return (updateCommands, unchangedCommands);
	}

	/// <summary>
	/// Builds a list of global command ids to be deleted on discords backend.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="updateList">The command list.</param>
	/// <returns>A list of command ids.</returns>
	private static List<ulong>? BuildGlobalDeleteList(DiscordClient client, List<DiscordApplicationCommand>? updateList = null)
	{
		if (ApplicationCommandsExtension.GlobalDiscordCommands.Count is 0)
			return null;

		var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

		List<ulong> invalidCommandIds = [];

		if (discord is null)
			return null;

		if (updateList is null)
			invalidCommandIds.AddRange(discord.Select(cmd => cmd.Id));
		else
			invalidCommandIds.AddRange(from cmd in discord where updateList.All(ul => ul.Name != cmd.Name) select cmd.Id);

		return invalidCommandIds;
	}

	/// <summary>
	/// Builds a list of global commands to be created on discords backend.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="updateList">The command list.</param>
	/// <returns>A list of commands.</returns>
	private static List<DiscordApplicationCommand>? BuildGlobalCreateList(DiscordClient client, List<DiscordApplicationCommand>? updateList = null)
	{
		if (ApplicationCommandsExtension.GlobalDiscordCommands.Count is 0 || updateList is null)
			return updateList;

		var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

		List<DiscordApplicationCommand> newCommands = [];

		if (discord is null)
			return updateList;

		newCommands.AddRange(updateList.Where(cmd => discord.All(d => d.Name != cmd.Name)));

		return newCommands;
	}

	/// <summary>
	/// Builds a list of global commands to be overwritten on discords backend.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="updateList">The command list.</param>
	/// <returns>A dictionary of command ids and commands.</returns>
	private static (
		Dictionary<ulong, DiscordApplicationCommand>? changedCommands,
		List<DiscordApplicationCommand>? unchangedCommands
		) BuildGlobalOverwriteList(DiscordClient client, List<DiscordApplicationCommand>? updateList = null)
	{
		if (ApplicationCommandsExtension.GlobalDiscordCommands.Count is 0 || updateList is null)
			return (null, null);

		var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

		if (discord is null)
			return (null, null);

		Dictionary<ulong, DiscordApplicationCommand> changedCommands = [];
		List<DiscordApplicationCommand> unchangedCommands = [];
		foreach (var cmd in updateList)
			if (discord!.TryGetFirstValueWhere(d => d?.Name == cmd.Name, out var command))
				if (command.IsEqualTo(cmd, client, false))
				{
					if (ApplicationCommandsExtension.DebugEnabled)
						client.Logger.LogDebug("[AC] Command {cmdName} unchanged", cmd.Name);
					cmd.Id = command.Id;
					cmd.ApplicationId = command.ApplicationId;
					cmd.Version = command.Version;
					unchangedCommands.Add(cmd);
				}
				else
				{
					if (ApplicationCommandsExtension.DebugEnabled)
						client.Logger.LogDebug("[AC] Command {cmdName} changed", cmd.Name);
					changedCommands.Add(command.Id, cmd);
				}

		return (changedCommands, unchangedCommands);
	}
}
