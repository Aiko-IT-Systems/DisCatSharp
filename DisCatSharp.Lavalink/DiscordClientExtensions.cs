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
using System.Threading.Tasks;
using DisCatSharp.Entities;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Lavalink
{
    /// <summary>
    /// The discord client extensions.
    /// </summary>
    public static class DiscordClientExtensions
    {
        /// <summary>
        /// Creates a new Lavalink client with specified settings.
        /// </summary>
        /// <param name="Client">Discord client to create Lavalink instance for.</param>
        /// <returns>Lavalink client instance.</returns>
        public static LavalinkExtension UseLavalink(this DiscordClient Client)
        {
            if (Client.GetExtension<LavalinkExtension>() != null)
                throw new InvalidOperationException("Lavalink is already enabled for that client.");

            if (!Client.Configuration.Intents.HasIntent(DiscordIntents.GuildVoiceStates))
                Client.Logger.LogCritical(LavalinkEvents.Intents, "The Lavalink extension is registered but the guild voice states intent is not enabled. It is highly recommended to enable it.");

            var lava = new LavalinkExtension();
            Client.AddExtension(lava);
            return lava;
        }

        /// <summary>
        /// Creates new Lavalink clients on all shards in a given sharded client.
        /// </summary>
        /// <param name="Client">Discord sharded client to create Lavalink instances for.</param>
        /// <returns>A dictionary of created Lavalink clients.</returns>
        public static async Task<IReadOnlyDictionary<int, LavalinkExtension>> UseLavalinkAsync(this DiscordShardedClient Client)
        {
            var modules = new Dictionary<int, LavalinkExtension>();
            await Client.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in Client.ShardClients.Select(Xkvp => Xkvp.Value))
            {
                var lava = shard.GetExtension<LavalinkExtension>();
                if (lava == null)
                    lava = shard.UseLavalink();

                modules[shard.ShardId] = lava;
            }

            return new ReadOnlyDictionary<int, LavalinkExtension>(modules);
        }

        /// <summary>
        /// Gets the active instance of the Lavalink client for the DiscordClient.
        /// </summary>
        /// <param name="Client">Discord client to get Lavalink instance for.</param>
        /// <returns>Lavalink client instance.</returns>
        public static LavalinkExtension GetLavalink(this DiscordClient Client)
            => Client.GetExtension<LavalinkExtension>();

        /// <summary>
        /// Retrieves a <see cref="LavalinkExtension"/> instance for each shard.
        /// </summary>
        /// <param name="Client">The shard client to retrieve <see cref="LavalinkExtension"/> instances from.</param>
        /// <returns>A dictionary containing <see cref="LavalinkExtension"/> instances for each shard.</returns>
        public static async Task<IReadOnlyDictionary<int, LavalinkExtension>> GetLavalinkAsync(this DiscordShardedClient Client)
        {
            await Client.InitializeShardsAsync().ConfigureAwait(false);
            var extensions = new Dictionary<int, LavalinkExtension>();

            foreach (var shard in Client.ShardClients.Values)
            {
                extensions.Add(shard.ShardId, shard.GetExtension<LavalinkExtension>());
            }

            return new ReadOnlyDictionary<int, LavalinkExtension>(extensions);
        }

        /// <summary>
        /// Connects to this voice channel using Lavalink.
        /// </summary>
        /// <param name="Channel">Channel to connect to.</param>
        /// <param name="Node">Lavalink node to connect through.</param>
        /// <returns>If successful, the Lavalink client.</returns>
        public static Task Connect(this DiscordChannel Channel, LavalinkNodeConnection Node)
        {
            if (Channel == null)
                throw new NullReferenceException();

            if (Channel.Guild == null)
                throw new InvalidOperationException("Lavalink can only be used with guild channels.");

            if (Channel.Type != ChannelType.Voice && Channel.Type != ChannelType.Stage)
                throw new InvalidOperationException("You can only connect to voice and stage channels.");

            if (Channel.Discord is not DiscordClient discord || discord == null)
                throw new NullReferenceException();

            var lava = discord.GetLavalink();
            return lava == null
                ? throw new InvalidOperationException("Lavalink is not initialized for this Discord client.")
                : Node.ConnectAsync(Channel);
        }
    }
}
