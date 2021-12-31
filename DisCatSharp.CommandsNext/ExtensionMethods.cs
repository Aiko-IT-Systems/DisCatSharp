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
using DisCatSharp.CommandsNext.Builders;
using DisCatSharp.CommandsNext.Converters;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.CommandsNext
{
    /// <summary>
    /// Defines various extensions specific to CommandsNext.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Enables CommandsNext module on this <see cref="DiscordClient"/>.
        /// </summary>
        /// <param name="Client">Client to enable CommandsNext for.</param>
        /// <param name="Cfg">CommandsNext configuration to use.</param>
        /// <returns>Created <see cref="CommandsNextExtension"/>.</returns>
        public static CommandsNextExtension UseCommandsNext(this DiscordClient Client, CommandsNextConfiguration Cfg)
        {
            if (Client.GetExtension<CommandsNextExtension>() != null)
                throw new InvalidOperationException("CommandsNext is already enabled for that client.");

            if (!Utilities.HasMessageIntents(Client.Configuration.Intents))
                Client.Logger.LogCritical(CommandsNextEvents.Intents, "The CommandsNext extension is registered but there are no message intents enabled. It is highly recommended to enable them.");

            if (!Client.Configuration.Intents.HasIntent(DiscordIntents.Guilds))
                Client.Logger.LogCritical(CommandsNextEvents.Intents, "The CommandsNext extension is registered but the guilds intent is not enabled. It is highly recommended to enable it.");

            var cnext = new CommandsNextExtension(Cfg);
            Client.AddExtension(cnext);
            return cnext;
        }

        /// <summary>
        /// Enables CommandsNext module on all shards in this <see cref="DiscordShardedClient"/>.
        /// </summary>
        /// <param name="Client">Client to enable CommandsNext for.</param>
        /// <param name="Cfg">CommandsNext configuration to use.</param>
        /// <returns>A dictionary of created <see cref="CommandsNextExtension"/>, indexed by shard id.</returns>
        public static async Task<IReadOnlyDictionary<int, CommandsNextExtension>> UseCommandsNextAsync(this DiscordShardedClient Client, CommandsNextConfiguration Cfg)
        {
            var modules = new Dictionary<int, CommandsNextExtension>();
            await Client.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in Client.ShardClients.Select(Xkvp => Xkvp.Value))
            {
                var cnext = shard.GetExtension<CommandsNextExtension>();
                if (cnext == null)
                    cnext = shard.UseCommandsNext(Cfg);

                modules[shard.ShardId] = cnext;
            }

            return new ReadOnlyDictionary<int, CommandsNextExtension>(modules);
        }

        /// <summary>
        /// Gets the active CommandsNext module for this client.
        /// </summary>
        /// <param name="Client">Client to get CommandsNext module from.</param>
        /// <returns>The module, or null if not activated.</returns>
        public static CommandsNextExtension GetCommandsNext(this DiscordClient Client)
            => Client.GetExtension<CommandsNextExtension>();


        /// <summary>
        /// Gets the active CommandsNext modules for all shards in this client.
        /// </summary>
        /// <param name="Client">Client to get CommandsNext instances from.</param>
        /// <returns>A dictionary of the modules, indexed by shard id.</returns>
        public static async Task<IReadOnlyDictionary<int, CommandsNextExtension>> GetCommandsNextAsync(this DiscordShardedClient Client)
        {
            await Client.InitializeShardsAsync().ConfigureAwait(false);
            var extensions = new Dictionary<int, CommandsNextExtension>();

            foreach (var shard in Client.ShardClients.Select(Xkvp => Xkvp.Value))
            {
                extensions.Add(shard.ShardId, shard.GetExtension<CommandsNextExtension>());
            }

            return new ReadOnlyDictionary<int, CommandsNextExtension>(extensions);
        }

        /// <summary>
        /// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
        /// </summary>
        /// <param name="Extensions">Extensions to register commands on.</param>
        /// <param name="Assembly">Assembly to register commands from.</param>
        public static void RegisterCommands(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions, Assembly Assembly)
        {
            foreach (var extension in Extensions.Values)
                extension.RegisterCommands(Assembly);
        }
        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <typeparam name="T">Class which holds commands to register.</typeparam>
        /// <param name="Extensions">Extensions to register commands on.</param>
        public static void RegisterCommands<T>(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions) where T : BaseCommandModule
        {
            foreach (var extension in Extensions.Values)
                extension.RegisterCommands<T>();
        }
        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <param name="Extensions">Extensions to register commands on.</param>
        /// <param name="T">Type of the class which holds commands to register.</param>
        public static void RegisterCommands(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions, Type T)
        {
            foreach (var extension in Extensions.Values)
                extension.RegisterCommands(T);
        }
        /// <summary>
        /// Builds and registers all supplied commands.
        /// </summary>
        /// <param name="Extensions">Extensions to register commands on.</param>
        /// <param name="Cmds">Commands to build and register.</param>
        public static void RegisterCommands(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions, params CommandBuilder[] Cmds)
        {
            foreach (var extension in Extensions.Values)
                extension.RegisterCommands(Cmds);
        }

        /// <summary>
        /// Unregisters specified commands from CommandsNext.
        /// </summary>
        /// <param name="Extensions">Extensions to unregister commands on.</param>
        /// <param name="Cmds">Commands to unregister.</param>
        public static void UnregisterCommands(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions, params Command[] Cmds)
        {
            foreach (var extension in Extensions.Values)
                extension.UnregisterCommands(Cmds);
        }

        /// <summary>
        /// Registers an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to register the converter.</typeparam>
        /// <param name="Extensions">Extensions to register the converter on.</param>
        /// <param name="Converter">Converter to register.</param>
        public static void RegisterConverter<T>(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions, IArgumentConverter<T> Converter)
        {
            foreach (var extension in Extensions.Values)
                extension.RegisterConverter(Converter);
        }

        /// <summary>
        /// Unregisters an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to unregister the converter.</typeparam>
        /// <param name="Extensions">Extensions to unregister the converter on.</param>
        public static void UnregisterConverter<T>(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions)
        {
            foreach (var extension in Extensions.Values)
                extension.UnregisterConverter<T>();
        }

        /// <summary>
        /// Registers a user-friendly type name.
        /// </summary>
        /// <typeparam name="T">Type to register the name for.</typeparam>
        /// <param name="Extensions">Extensions to register the name on.</param>
        /// <param name="Value">Name to register.</param>
        public static void RegisterUserFriendlyTypeName<T>(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions, string Value)
        {
            foreach (var extension in Extensions.Values)
                extension.RegisterUserFriendlyTypeName<T>(Value);
        }

        /// <summary>
        /// Sets the help formatter to use with the default help command.
        /// </summary>
        /// <typeparam name="T">Type of the formatter to use.</typeparam>
        /// <param name="Extensions">Extensions to set the help formatter on.</param>
        public static void SetHelpFormatter<T>(this IReadOnlyDictionary<int, CommandsNextExtension> Extensions) where T : BaseHelpFormatter
        {
            foreach (var extension in Extensions.Values)
                extension.SetHelpFormatter<T>();
        }
    }
}
