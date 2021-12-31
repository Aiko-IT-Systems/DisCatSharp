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
        /// <param name="Commands">The command list.</param>
        /// <returns>A list of registered commands.</returns>
        internal static async Task<List<DiscordApplicationCommand>> RegisterGlobalCommandsAsync(List<DiscordApplicationCommand> Commands)
        {
            var globalCommandsOverwriteList = BuildGlobalOverwriteList(Commands);
            var globalCommandsCreateList = BuildGlobalCreateList(Commands);
            var globalCommandsDeleteList = BuildGlobalDeleteList(Commands);

            if (globalCommandsCreateList.NotEmptyAndNotNull() && globalCommandsOverwriteList.EmptyOrNull())
            {
                var cmds = await ApplicationCommandsExtension._client.BulkOverwriteGlobalApplicationCommands(globalCommandsCreateList);
                Commands.AddRange(cmds);
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
                var discordBackendCommands = await ApplicationCommandsExtension._client.BulkOverwriteGlobalApplicationCommands(overwriteList);
                Commands.AddRange(discordBackendCommands);
            }
            else if (globalCommandsCreateList.NotEmptyAndNotNull() && globalCommandsOverwriteList.NotEmptyAndNotNull())
            {
                foreach (var cmd in globalCommandsCreateList)
                {
                    var discordBackendCommand = await ApplicationCommandsExtension._client.CreateGlobalApplicationCommand(cmd);
                    Commands.Add(discordBackendCommand);
                }

                foreach (var cmd in globalCommandsOverwriteList)
                {
                    var command = cmd.Value;

                    var discordBackendCommand = await ApplicationCommandsExtension._client.EditGlobalApplicationCommandAsync(cmd.Key, Action =>
                    {
                        Action.Name = command.Name;
                        Action.NameLocalizations = command.NameLocalizations;
                        Action.Description = command.Description;
                        Action.DescriptionLocalizations = command.DescriptionLocalizations;
                        if(command.Options != null && command.Options.Any())
                            Action.Options = Entities.Optional.FromValue(command.Options);
                        Action.DefaultPermission = command.DefaultPermission;
                    });
                    Commands.Add(discordBackendCommand);
                }
            }

            if (globalCommandsDeleteList.NotEmptyAndNotNull())
            {
                foreach (var cmdId in globalCommandsDeleteList)
                {
                    try
                    {
                        await ApplicationCommandsExtension._client.DeleteGlobalApplicationCommand(cmdId);
                    }
                    catch (NotFoundException)
                    {
                        ApplicationCommandsExtension._client.Logger.LogError($"Could not delete global command {cmdId}. Please clean up manually");
                    }
                }
            }

            return Commands.NotEmptyAndNotNull() ? Commands : null;
        }

        /// <summary>
        /// Registers the guild commands.
        /// </summary>
        /// <param name="GuildId">The target guild id.</param>
        /// <param name="Commands">The command list.</param>
        /// <returns>A list of registered commands.</returns>
        internal static async Task<List<DiscordApplicationCommand>> RegisterGuilldCommandsAsync(ulong GuildId, List<DiscordApplicationCommand> Commands)
        {
            var guildCommandsOverwriteList = BuildGuildOverwriteList(GuildId, Commands);
            var guildCommandsCreateList = BuildGuildCreateList(GuildId, Commands);
            var guildCommandsDeleteList = BuildGuildDeleteList(GuildId, Commands);

            if (guildCommandsCreateList.NotEmptyAndNotNull() && guildCommandsOverwriteList.EmptyOrNull())
            {
                var cmds = await ApplicationCommandsExtension._client.BulkOverwriteGuildApplicationCommands(GuildId, guildCommandsCreateList);
                Commands.AddRange(cmds);
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
                var discordBackendCommands = await ApplicationCommandsExtension._client.BulkOverwriteGuildApplicationCommands(GuildId, overwriteList);
                Commands.AddRange(discordBackendCommands);
            }
            else if (guildCommandsCreateList.NotEmptyAndNotNull() && guildCommandsOverwriteList.NotEmptyAndNotNull())
            {
                foreach (var cmd in guildCommandsCreateList)
                {
                    var discordBackendCommand = await ApplicationCommandsExtension._client.CreateGuildApplicationCommand(GuildId, cmd);
                    Commands.Add(discordBackendCommand);
                }

                foreach (var cmd in guildCommandsOverwriteList)
                {
                    var command = cmd.Value;
                    var discordBackendCommand = await ApplicationCommandsExtension._client.EditGuildApplicationCommandAsync(GuildId, cmd.Key, Action =>
                    {
                        Action.Name = command.Name;
                        Action.NameLocalizations = command.NameLocalizations;
                        Action.Description = command.Description;
                        Action.DescriptionLocalizations = command.DescriptionLocalizations;
                        if(command.Options != null && command.Options.Any())
                            Action.Options = Entities.Optional.FromValue(command.Options);
                        Action.DefaultPermission = command.DefaultPermission;
                    });

                    Commands.Add(discordBackendCommand);
                }
            }

            if (guildCommandsDeleteList.NotEmptyAndNotNull())
            {
                foreach (var cmdId in guildCommandsDeleteList)
                {
                    try
                    {
                        await ApplicationCommandsExtension._client.DeleteGuildApplicationCommand(GuildId, cmdId);
                    }
                    catch (NotFoundException)
                    {
                        ApplicationCommandsExtension._client.Logger.LogError($"Could not delete guild command {cmdId} in guild {GuildId}. Please clean up manually");
                    }
                }
            }

            return Commands.NotEmptyAndNotNull() ? Commands : null;
        }

        /// <summary>
        /// Builds a list of guild command ids to be deleted on discords backend.
        /// </summary>
        /// <param name="GuildId">The guild id these commands belong to.</param>
        /// <param name="UpdateList">The command list.</param>
        /// <returns>A list of command ids.</returns>
        private static List<ulong> BuildGuildDeleteList(ulong GuildId, List<DiscordApplicationCommand> UpdateList)
        {
            List<DiscordApplicationCommand> discord;

            if (ApplicationCommandsExtension.GuildDiscordCommands == null || !ApplicationCommandsExtension.GuildDiscordCommands.Any()
                || !ApplicationCommandsExtension.GuildDiscordCommands.GetFirstValueByKey(GuildId, out discord)
            )
                return null;

            List<ulong> invalidCommandIds = new();

            if (UpdateList == null)
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
                    if (!UpdateList.Any(Ul => Ul.Name == cmd.Name))
                        invalidCommandIds.Add(cmd.Id);
                }
            }

            return invalidCommandIds;
        }

        /// <summary>
        /// Builds a list of guild commands to be created on discords backend.
        /// </summary>
        /// <param name="GuildId">The guild id these commands belong to.</param>
        /// <param name="UpdateList">The command list.</param>
        /// <returns></returns>
        private static List<DiscordApplicationCommand> BuildGuildCreateList(ulong GuildId, List<DiscordApplicationCommand> UpdateList)
        {
            List<DiscordApplicationCommand> discord;

            if (ApplicationCommandsExtension.GuildDiscordCommands == null || !ApplicationCommandsExtension.GuildDiscordCommands.Any()
                || UpdateList == null || !ApplicationCommandsExtension.GuildDiscordCommands.GetFirstValueByKey(GuildId, out discord)
            )
                return UpdateList;

            List<DiscordApplicationCommand> newCommands = new();

            foreach (var cmd in UpdateList)
            {
                if (!discord.Any(D => D.Name == cmd.Name))
                {
                    newCommands.Add(cmd);
                }
            }

            return newCommands;
        }

        /// <summary>
        /// Builds a list of guild commands to be overwritten on discords backend.
        /// </summary>
        /// <param name="GuildId">The guild id these commands belong to.</param>
        /// <param name="UpdateList">The command list.</param>
        /// <returns>A dictionary of command id and command.</returns>
        private static Dictionary<ulong, DiscordApplicationCommand> BuildGuildOverwriteList(ulong GuildId, List<DiscordApplicationCommand> UpdateList)
        {
            List<DiscordApplicationCommand> discord;

            if (ApplicationCommandsExtension.GuildDiscordCommands == null || !ApplicationCommandsExtension.GuildDiscordCommands.Any()
                || !ApplicationCommandsExtension.GuildDiscordCommands.Any(L => L.Key == GuildId) || UpdateList == null
                || !ApplicationCommandsExtension.GuildDiscordCommands.GetFirstValueByKey(GuildId, out discord)
            )
                return null;

            Dictionary<ulong, DiscordApplicationCommand> updateCommands = new();

            foreach (var cmd in UpdateList)
            {
                if (discord.GetFirstValueWhere(D => D.Name == cmd.Name, out var command))
                    updateCommands.Add(command.Id, cmd);
            }

            return updateCommands;
        }

        /// <summary>
        /// Builds a list of global command ids to be deleted on discords backend.
        /// </summary>
        /// <param name="UpdateList">The command list.</param>
        /// <returns>A list of command ids.</returns>
        private static List<ulong> BuildGlobalDeleteList(List<DiscordApplicationCommand> UpdateList = null)
        {
            if (ApplicationCommandsExtension.GlobalDiscordCommands == null || !ApplicationCommandsExtension.GlobalDiscordCommands.Any()
                || ApplicationCommandsExtension.GlobalDiscordCommands == null
            )
                return null;

            var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

            List<ulong> invalidCommandIds = new();

            if (UpdateList == null)
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
                    if (!UpdateList.Any(Ul => Ul.Name == cmd.Name))
                        invalidCommandIds.Add(cmd.Id);
                }
            }

            return invalidCommandIds;
        }

        /// <summary>
        /// Builds a list of global commands to be created on discords backend.
        /// </summary>
        /// <param name="UpdateList">The command list.</param>
        /// <returns>A list of commands.</returns>
        private static List<DiscordApplicationCommand> BuildGlobalCreateList(List<DiscordApplicationCommand> UpdateList)
        {
            if (ApplicationCommandsExtension.GlobalDiscordCommands == null || !ApplicationCommandsExtension.GlobalDiscordCommands.Any() || UpdateList == null)
                return UpdateList;

            var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

            List<DiscordApplicationCommand> newCommands = new();

            foreach (var cmd in UpdateList)
            {
                if (discord.Any(D => D.Name == cmd.Name))
                {
                    newCommands.Add(cmd);
                }
            }

            return newCommands;
        }

        /// <summary>
        /// Builds a list of global commands to be overwritten on discords backend.
        /// </summary>
        /// <param name="UpdateList">The command list.</param>
        /// <returns>A dictionary of command ids and commands.</returns>
        private static Dictionary<ulong, DiscordApplicationCommand> BuildGlobalOverwriteList(List<DiscordApplicationCommand> UpdateList)
        {
            if (ApplicationCommandsExtension.GlobalDiscordCommands == null || !ApplicationCommandsExtension.GlobalDiscordCommands.Any()
                || UpdateList == null || ApplicationCommandsExtension.GlobalDiscordCommands == null
            )
                return null;

            var discord = ApplicationCommandsExtension.GlobalDiscordCommands;

            Dictionary<ulong, DiscordApplicationCommand> updateCommands = new();
            foreach (var cmd in UpdateList)
            {
                if (discord.GetFirstValueWhere(D => D.Name == cmd.Name, out var command))
                    updateCommands.Add(command.Id, cmd);
            }

            return updateCommands;
        }
    }
}
