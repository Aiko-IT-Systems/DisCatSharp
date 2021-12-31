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

namespace DisCatSharp.Interactivity.Extensions
{
    /// <summary>
    /// Interactivity extension methods for <see cref="DiscordClient"/> and <see cref="DiscordShardedClient"/>.
    /// </summary>
    public static class ClientExtensions
    {
        /// <summary>
        /// Enables interactivity for this <see cref="DiscordClient"/> instance.
        /// </summary>
        /// <param name="Client">The client to enable interactivity for.</param>
        /// <param name="Configuration">A configuration instance. Default configuration values will be used if none is provided.</param>
        /// <returns>A brand new <see cref="InteractivityExtension"/> instance.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if interactivity has already been enabled for the client instance.</exception>
        public static InteractivityExtension UseInteractivity(this DiscordClient Client, InteractivityConfiguration Configuration = null)
        {
            if (Client.GetExtension<InteractivityExtension>() != null) throw new InvalidOperationException($"Interactivity is already enabled for this {(Client._isShard ? "shard" : "client")}.");

            Configuration ??= new InteractivityConfiguration();
            var extension = new InteractivityExtension(Configuration);
            Client.AddExtension(extension);

            return extension;
        }

        /// <summary>
        /// Enables interactivity for each shard.
        /// </summary>
        /// <param name="Client">The shard client to enable interactivity for.</param>
        /// <param name="Configuration">Configuration to use for all shards. If one isn't provided, default configuration values will be used.</param>
        /// <returns>A dictionary containing new <see cref="InteractivityExtension"/> instances for each shard.</returns>
        public static async Task<IReadOnlyDictionary<int, InteractivityExtension>> UseInteractivityAsync(this DiscordShardedClient Client, InteractivityConfiguration Configuration = null)
        {
            var extensions = new Dictionary<int, InteractivityExtension>();
            await Client.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in Client.ShardClients.Select(Xkvp => Xkvp.Value))
            {
                var extension = shard.GetExtension<InteractivityExtension>() ?? shard.UseInteractivity(Configuration);
                extensions.Add(shard.ShardId, extension);
            }

            return new ReadOnlyDictionary<int, InteractivityExtension>(extensions);
        }

        /// <summary>
        /// Retrieves the registered <see cref="InteractivityExtension"/> instance for this client.
        /// </summary>
        /// <param name="Client">The client to retrieve an <see cref="InteractivityExtension"/> instance from.</param>
        /// <returns>An existing <see cref="InteractivityExtension"/> instance, or <see langword="null"/> if interactivity is not enabled for the <see cref="DiscordClient"/> instance.</returns>
        public static InteractivityExtension GetInteractivity(this DiscordClient Client)
            => Client.GetExtension<InteractivityExtension>();

        /// <summary>
        /// Retrieves a <see cref="InteractivityExtension"/> instance for each shard.
        /// </summary>
        /// <param name="Client">The shard client to retrieve interactivity instances from.</param>
        /// <returns>A dictionary containing <see cref="InteractivityExtension"/> instances for each shard.</returns>
        public static async Task<ReadOnlyDictionary<int, InteractivityExtension>> GetInteractivityAsync(this DiscordShardedClient Client)
        {
            await Client.InitializeShardsAsync().ConfigureAwait(false);
            var extensions = new Dictionary<int, InteractivityExtension>();

            foreach (var shard in Client.ShardClients.Select(Xkvp => Xkvp.Value))
            {
                extensions.Add(shard.ShardId, shard.GetExtension<InteractivityExtension>());
            }

            return new ReadOnlyDictionary<int, InteractivityExtension>(extensions);
        }
    }
}
