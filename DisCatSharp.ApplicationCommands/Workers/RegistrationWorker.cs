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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.Exceptions;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.ApplicationCommands
{
	/// <summary>
	/// Represents a <see cref="RegistrationWorker"/>.
	/// </summary>
	internal class RegistrationWorker
	{

		/// <summary>
		/// Registers the global commands.
		/// </summary>
		/// <param name="commands">The command list.</param>
		/// <returns>A list of registered commands.</returns>
		internal static async Task<List<DiscordApplicationCommand>> RegisterGlobalCommandsAsync(List<DiscordApplicationCommand> commands)
		{
			var globalCommandsOverwriteList = BuildGlobalOverwriteList(commands);
			var globalCommandsCreateList = BuildGlobalCreateList(commands);
			var globalCommandsDeleteList = BuildGlobalDeleteList(commands);

			if (globalCommandsCreateList.NotEmptyAndNotNull() && globalCommandsOverwriteList.EmptyOrNull())
			{
				var cmds = await ApplicationCommandsExtension.ClientInternal.BulkOverwriteGlobalApplicationCommandsAsync(globalCommandsCreateList);
				commands.AddRange(cmds);
			}
			else if (globalCommandsCreateList.EmptyOrNull() && globalCommandsOverwriteList.NotEmptyAndNotNull())
			{
				List<DiscordApplicationCommand> overwriteList = new();
				foreach (var overwrite in globalCommandsOverwriteList)
				{
					var cmd = overwrite.Value;
					cmd.Id = overwrite.Key;
					overwriteList.Add(cmd);
				}
				var discordBackendCommands = await ApplicationCommandsExtension.ClientInternal.BulkOverwriteGlobalApplicationCommandsAsync(overwriteList);
				commands.AddRange(discordBackendCommands);
			}
			else if (globalCommandsCreateList.NotEmptyAndNotNull() && globalCommandsOverwriteList.NotEmptyAndNotNull())
			{
				foreach (var cmd in globalCommandsCreateList)
				{
					var discordBackendCommand = await ApplicationCommandsExtension.ClientInternal.CreateGlobalApplicationCommandAsync(cmd);
					commands.Add(discordBackendCommand);
				}

				foreach (var cmd in globalCommandsOverwriteList)
				{
					var command = cmd.Value;

					var discordBackendCommand = await ApplicationCommandsExtension.ClientInternal.EditGlobalApplicationCommandAsync(cmd.Key, action =>
					{
						action.Name = command.Name;
						action.NameLocalizations = command.NameLocalizations;
						action.Description = command.Description;
						action.DescriptionLocalizations = command.DescriptionLocalizations;
						if(command.Options != null && command.Options.Any())
							action.Options = Entities.Optional.FromValue(command.Options);
						action.DefaultPermission = command.DefaultPermission;
					});
					commands.Add(discordBackendCommand);
				}
			}

			if (globalCommandsDeleteList.NotEmptyAndNotNull())
			{
				foreach (var cmdId in globalCommandsDeleteList)
				{
					try
					{
						await ApplicationCommandsExtension.ClientInternal.DeleteGlobalApplicationCommandAsync(cmdId);
					}
					catch (NotFoundException)
					{
						ApplicationCommandsExtension.ClientInternal.Logger.LogError($"Could not delete global command {cmdId}. Please clean up manually");
					}
				}
			}

			return commands.NotEmptyAndNotNull() ? commands : null;
		}

		/// <summary>
		/// Registers the guild commands.
		/// </summary>
		/// <param name="guildId">The target guild id.</param>
		/// <param name="commands">The command list.</param>
		/// <returns>A list of registered commands.</returns>
		internal static async Task<List<DiscordApplicationCommand>> RegisterGuilldCommandsAsync(ulong guildId, List<DiscordApplicationCommand> commands)
		{
			var guildCommandsOverwriteList = BuildGuildOverwriteList(guildId, commands);
			var guildCommandsCreateList = BuildGuildCreateList(guildId, commands);
			var guildCommandsDeleteList = BuildGuildDeleteList(guildId, commands);

			if (guildCommandsCreateList.NotEmptyAndNotNull() && guildCommandsOverwriteList.EmptyOrNull())
			{
				var cmds = await ApplicationCommandsExtension.ClientInternal.BulkOverwriteGuildApplicationCommandsAsync(guildId, guildCommandsCreateList);
				commands.AddRange(cmds);
			}
			else if (guildCommandsCreateList.EmptyOrNull() && guildCommandsOverwriteList.NotEmptyAndNotNull())
			{
				List<DiscordApplicationCommand> overwriteList = new();
				foreach (var overwrite in guildCommandsOverwriteList)
				{
					var cmd = overwrite.Value;
					cmd.Id = overwrite.Key;
					overwriteList.Add(cmd);
				}
				var discordBackendCommands = await ApplicationCommandsExtension.ClientInternal.BulkOverwriteGuildApplicationCommandsAsync(guildId, overwriteList);
				commands.AddRange(discordBackendCommands);
			}
			else if (guildCommandsCreateList.NotEmptyAndNotNull() && guildCommandsOverwriteList.NotEmptyAndNotNull())
			{
				foreach (var cmd in guildCommandsCreateList)
				{
					var discordBackendCommand = await ApplicationCommandsExtension.ClientInternal.CreateGuildApplicationCommandAsync(guildId, cmd);
					commands.Add(discordBackendCommand);
				}

				foreach (var cmd in guildCommandsOverwriteList)
				{
					var command = cmd.Value;
					var discordBackendCommand = await ApplicationCommandsExtension.ClientInternal.EditGuildApplicationCommandAsync(guildId, cmd.Key, action =>
					{
						action.Name = command.Name;
						action.NameLocalizations = command.NameLocalizations;
						action.Description = command.Description;
						action.DescriptionLocalizations = command.DescriptionLocalizations;
						if(command.Options != null && command.Options.Any())
							action.Options = Entities.Optional.FromValue(command.Options);
						action.DefaultPermission = command.DefaultPermission;
					});

					commands.Add(discordBackendCommand);
				}
			}

			if (guildCommandsDeleteList.NotEmptyAndNotNull())
			{
				foreach (var cmdId in guildCommandsDeleteList)
				{
					try
					{
						await ApplicationCommandsExtension.ClientInternal.DeleteGuildApplicationCommandAsync(guildId, cmdId);
					}
					catch (NotFoundException)
					{
						ApplicationCommandsExtension.ClientInternal.Logger.LogError($"Could not delete guild command {cmdId} in guild {guildId}. Please clean up manually");
					}
				}
			}

			return commands.NotEmptyAndNotNull() ? commands : null;
		}

		/// <summary>
		/// Builds a list of guild command ids to be deleted on discords backend.
		/// </summary>
		/// <param name="guildId">The guild id these commands belong to.</param>
		/// <param name="updateList">The command list.</param>
		/// <returns>A list of command ids.</returns>
		private static List<ulong> BuildGuildDeleteList(ulong guildId, List<DiscordApplicationCommand> updateList)
		{
			List<DiscordApplicationCommand> discord;

			if (ApplicationCommandsExtension.GuildDiscordCommands == null || !ApplicationCommandsExtension.GuildDiscordCommands.Any()
				|| !ApplicationCommandsExtension.GuildDiscordCommands.GetFirstValueByKey(guildId, out discord)
			)
				return null;

			List<ulong> invalidCommandIds = new();

			if (updateList == null)
			{
				foreach (var cmd in discord)
				{
					invalidCommandIds.Add(cmd.Id);
				}
			}
			else
			{
				foreach (var cmd in discord)
				{
					if (!updateList.Any(ul => ul.Name == cmd.Name))
						invalidCommandIds.Add(cmd.Id);
				}
			}

			return invalidCommandIds;
		}

		/// <summary>
		/// Builds a list of guild commands to be created on discords backend.
		/// </summary>
		/// <param name="guildId">The guild id these commands belong to.</param>
		/// <param name="updateList">The command list.</param>
		/// <returns></returns>
		private static List<DiscordApplicationCommand> BuildGuildCreateList(ulong guildId, List<DiscordApplicationCommand> updateList)
		{
			List<DiscordApplicationCommand> discord;

			if (ApplicationCommandsExtension.GuildDiscordCommands == null || !ApplicationCommandsExtension.GuildDiscordCommands.Any()
				|| updateList == null || !ApplicationCommandsExtension.GuildDiscordCommands.GetFirstValueByKey(guildId, out discord)
			)
				return updateList;

			List<DiscordApplicationCommand> newCommands = new();

			foreach (var cmd in updateList)
			{
				if (!discord.Any(d => d.Name == cmd.Name))
				{
					newCommands.Add(cmd);
				}
			}

			return newCommands;
		}

		/// <summary>
		/// Builds a list of guild commands to be overwritten on discords backend.
		/// </summary>
		/// <param name="guildId">The guild id these commands belong to.</param>
		/// <param name="updateList">The command list.</param>
		/// <returns>A dictionary of command id and command.</returns>
		private static Dictionary<ulong, DiscordApplicationCommand> BuildGuildOverwriteList(ulong guildId, List<DiscordApplicationCommand> updateList)
		{
			List<DiscordApplicationCommand> discord;

			if (ApplicationCommandsExtension.GuildDiscordCommands == null || !ApplicationCommandsExtension.GuildDiscordCommands.Any()
				|| !ApplicationCommandsExtension.GuildDiscordCommands.Any(l => l.Key == guildId) || updateList == null
				|| !ApplicationCommandsExtension.GuildDiscordCommands.GetFirstValueByKey(guildId, out discord)
			)
				return null;

			Dictionary<ulong, DiscordApplicationCommand> updateCommands = new();

			foreach (var cmd in updateList)
			{
				if (discord.GetFirstValueWhere(d => d.Name == cmd.Name, out var command))
					updateCommands.Add(command.Id, cmd);
			}

			return updateCommands;
		}

		/// <summary>
		/// Builds a list of global command ids to be deleted on discords backend.
		/// </summary>
		/// <param name="updateList">The command list.</param>
		/// <returns>A list of command ids.</returns>
		private static List<ulong> BuildGlobalDeleteList(List<DiscordApplicationCommand> updateList = null)
		{
			if (ApplicationCommandsExtension.GlobalDiscordCommands == null || !ApplicationCommandsExtension.GlobalDiscordCommands.Any()
				|| ApplicationCommandsExtension.GlobalDiscordCommands == null
			)
				return null;

			var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

			List<ulong> invalidCommandIds = new();

			if (updateList == null)
			{
				foreach (var cmd in discord)
				{
					invalidCommandIds.Add(cmd.Id);
				}
			}
			else
			{
				foreach (var cmd in discord)
				{
					if (!updateList.Any(ul => ul.Name == cmd.Name))
						invalidCommandIds.Add(cmd.Id);
				}
			}

			return invalidCommandIds;
		}

		/// <summary>
		/// Builds a list of global commands to be created on discords backend.
		/// </summary>
		/// <param name="updateList">The command list.</param>
		/// <returns>A list of commands.</returns>
		private static List<DiscordApplicationCommand> BuildGlobalCreateList(List<DiscordApplicationCommand> updateList)
		{
			if (ApplicationCommandsExtension.GlobalDiscordCommands == null || !ApplicationCommandsExtension.GlobalDiscordCommands.Any() || updateList == null)
				return updateList;

			var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

			List<DiscordApplicationCommand> newCommands = new();

			foreach (var cmd in updateList)
			{
				if (discord.Any(d => d.Name == cmd.Name))
				{
					newCommands.Add(cmd);
				}
			}

			return newCommands;
		}

		/// <summary>
		/// Builds a list of global commands to be overwritten on discords backend.
		/// </summary>
		/// <param name="updateList">The command list.</param>
		/// <returns>A dictionary of command ids and commands.</returns>
		private static Dictionary<ulong, DiscordApplicationCommand> BuildGlobalOverwriteList(List<DiscordApplicationCommand> updateList)
		{
			if (ApplicationCommandsExtension.GlobalDiscordCommands == null || !ApplicationCommandsExtension.GlobalDiscordCommands.Any()
				|| updateList == null || ApplicationCommandsExtension.GlobalDiscordCommands == null
			)
				return null;

			var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

			Dictionary<ulong, DiscordApplicationCommand> updateCommands = new();
			foreach (var cmd in updateList)
			{
				if (discord.GetFirstValueWhere(d => d.Name == cmd.Name, out var command))
					updateCommands.Add(command.Id, cmd);
			}

			return updateCommands;
		}
	}
}
