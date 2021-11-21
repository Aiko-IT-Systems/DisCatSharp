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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DisCatSharp.ApplicationCommands
{
    /// <summary>
    /// Defines various extension methods for application commands.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Enables application commands on this <see cref="DiscordClient"/>.
        /// </summary>
        /// <param name="client">Client to enable application commands for.</param>
        /// <param name="config">Configuration to use.</param>
        /// <returns>Created <see cref="ApplicationCommandsExtension"/>.</returns>
        public static ApplicationCommandsExtension UseApplicationCommands(this DiscordClient client,
            ApplicationCommandsConfiguration config = null)
        {
            if (client.GetExtension<ApplicationCommandsExtension>() != null)
                throw new InvalidOperationException("Application commands are already enabled for that client.");

            var scomm = new ApplicationCommandsExtension(config);
            client.AddExtension(scomm);
            return scomm;
        }

        /// <summary>
        /// Gets the application commands module for this client.
        /// </summary>
        /// <param name="client">Client to get application commands for.</param>
        /// <returns>The module, or null if not activated.</returns>
        public static ApplicationCommandsExtension GetApplicationCommands(this DiscordClient client)
            => client.GetExtension<ApplicationCommandsExtension>();

        /// <summary>
        /// Enables application commands on this <see cref="DiscordShardedClient"/>.
        /// </summary>
        /// <param name="client">Client to enable application commands on.</param>
        /// <param name="config">Configuration to use.</param>
        /// <returns>A dictionary of created <see cref="ApplicationCommandsExtension"/> with the key being the shard id.</returns>
        public static async Task<IReadOnlyDictionary<int, ApplicationCommandsExtension>> UseApplicationCommandsAsync(this DiscordShardedClient client, ApplicationCommandsConfiguration config = null)
        {
            var modules = new Dictionary<int, ApplicationCommandsExtension>();
            await (Task)client.GetType().GetMethod("InitializeShardsAsync", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(client, null);
            foreach (var shard in client.ShardClients.Values)
            {
                var scomm = shard.GetApplicationCommands();
                if (scomm == null)
                    scomm = shard.UseApplicationCommands(config);

                modules[shard.ShardId] = scomm;
            }

            return modules;
        }

        /// <summary>
        /// Registers a commands class.
        /// </summary>
        /// <typeparam name="T">The command class to register.</typeparam>
        /// <param name="modules">The modules to register it on.</param>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public static void RegisterCommands<T>(this IReadOnlyDictionary<int, ApplicationCommandsExtension> modules, ulong? guildId = null) where T : ApplicationCommandsModule
        {
            foreach (var module in modules.Values)
                module.RegisterCommands<T>(guildId);
        }

        /// <summary>
        /// Registers a command class.
        /// </summary>
        /// <param name="modules">The modules to register it on.</param>
        /// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
        /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
        public static void RegisterCommands(this IReadOnlyDictionary<int, ApplicationCommandsExtension> modules, Type type, ulong? guildId = null)
        {
            foreach (var module in modules.Values)
                module.RegisterCommands(type, guildId);
        }

        /// <summary>
        /// Gets the name from the <see cref="ChoiceNameAttribute"/> for this enum value.
        /// </summary>
        /// <returns>The name.</returns>
        public static string GetName<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                var type = e.GetType();
                var values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));

                        return memInfo[0]
                            .GetCustomAttributes(typeof(ChoiceNameAttribute), false)
                            .FirstOrDefault() is ChoiceNameAttribute nameAttribute ? nameAttribute.Name : type.GetEnumName(val);
                    }
                }
            }
            return null;
        }
    }
}
