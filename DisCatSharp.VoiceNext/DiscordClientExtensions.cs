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

namespace DisCatSharp.VoiceNext
{
    /// <summary>
    /// The discord client extensions.
    /// </summary>
    public static class DiscordClientExtensions
    {
        /// <summary>
        /// Creates a new VoiceNext client with default settings.
        /// </summary>
        /// <param name="Client">Discord client to create VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextExtension UseVoiceNext(this DiscordClient Client)
            => UseVoiceNext(Client, new VoiceNextConfiguration());

        /// <summary>
        /// Creates a new VoiceNext client with specified settings.
        /// </summary>
        /// <param name="Client">Discord client to create VoiceNext instance for.</param>
        /// <param name="Config">Configuration for the VoiceNext client.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextExtension UseVoiceNext(this DiscordClient Client, VoiceNextConfiguration Config)
        {
            if (Client.GetExtension<VoiceNextExtension>() != null)
                throw new InvalidOperationException("VoiceNext is already enabled for that client.");

            var vnext = new VoiceNextExtension(Config);
            Client.AddExtension(vnext);
            return vnext;
        }

        /// <summary>
        /// Creates new VoiceNext clients on all shards in a given sharded client.
        /// </summary>
        /// <param name="Client">Discord sharded client to create VoiceNext instances for.</param>
        /// <param name="Config">Configuration for the VoiceNext clients.</param>
        /// <returns>A dictionary of created VoiceNext clients.</returns>
        public static async Task<IReadOnlyDictionary<int, VoiceNextExtension>> UseVoiceNextAsync(this DiscordShardedClient Client, VoiceNextConfiguration Config)
        {
            var modules = new Dictionary<int, VoiceNextExtension>();
            await Client.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in Client.ShardClients.Select(Xkvp => Xkvp.Value))
            {
                var vnext = shard.GetExtension<VoiceNextExtension>();
                if (vnext == null)
                    vnext = shard.UseVoiceNext(Config);

                modules[shard.ShardId] = vnext;
            }

            return new ReadOnlyDictionary<int, VoiceNextExtension>(modules);
        }

        /// <summary>
        /// Gets the active instance of VoiceNext client for the DiscordClient.
        /// </summary>
        /// <param name="Client">Discord client to get VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextExtension GetVoiceNext(this DiscordClient Client)
            => Client.GetExtension<VoiceNextExtension>();

        /// <summary>
        /// Retrieves a <see cref="VoiceNextExtension"/> instance for each shard.
        /// </summary>
        /// <param name="Client">The shard client to retrieve <see cref="VoiceNextExtension"/> instances from.</param>
        /// <returns>A dictionary containing <see cref="VoiceNextExtension"/> instances for each shard.</returns>
        public static async Task<IReadOnlyDictionary<int, VoiceNextExtension>> GetVoiceNextAsync(this DiscordShardedClient Client)
        {
            await Client.InitializeShardsAsync().ConfigureAwait(false);
            var extensions = new Dictionary<int, VoiceNextExtension>();

            foreach (var shard in Client.ShardClients.Values)
            {
                extensions.Add(shard.ShardId, shard.GetExtension<VoiceNextExtension>());
            }

            return new ReadOnlyDictionary<int, VoiceNextExtension>(extensions);
        }

        /// <summary>
        /// Connects to this voice channel using VoiceNext.
        /// </summary>
        /// <param name="Channel">Channel to connect to.</param>
        /// <returns>If successful, the VoiceNext connection.</returns>
        public static Task<VoiceNextConnection> Connect(this DiscordChannel Channel)
        {
            if (Channel == null)
                throw new NullReferenceException();

            if (Channel.Guild == null)
                throw new InvalidOperationException("VoiceNext can only be used with guild channels.");

            if (Channel.Type != ChannelType.Voice && Channel.Type != ChannelType.Stage)
                throw new InvalidOperationException("You can only connect to voice or stage channels.");

            if (Channel.Discord is not DiscordClient discord || discord == null)
                throw new NullReferenceException();

            var vnext = discord.GetVoiceNext();
            if (vnext == null)
                throw new InvalidOperationException("VoiceNext is not initialized for this Discord client.");

            var vnc = vnext.GetConnection(Channel.Guild);
            return vnc != null
                ? throw new InvalidOperationException("VoiceNext is already connected in this guild.")
                : vnext.ConnectAsync(Channel);
        }
    }
}
