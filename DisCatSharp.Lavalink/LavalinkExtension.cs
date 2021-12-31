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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink.EventArgs;
using DisCatSharp.Net;

namespace DisCatSharp.Lavalink
{
    /// <summary>
    /// The lavalink extension.
    /// </summary>
    public sealed class LavalinkExtension : BaseExtension
    {
        /// <summary>
        /// Triggered whenever a node disconnects.
        /// </summary>
        public event AsyncEventHandler<LavalinkNodeConnection, NodeDisconnectedEventArgs> NodeDisconnected
        {
            add { this._nodeDisconnected.Register(value); }
            remove { this._nodeDisconnected.Unregister(value); }
        }
        private AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs> _nodeDisconnected;

        /// <summary>
        /// Gets a dictionary of connected Lavalink nodes for the extension.
        /// </summary>
        public IReadOnlyDictionary<ConnectionEndpoint, LavalinkNodeConnection> ConnectedNodes { get; }
        private readonly ConcurrentDictionary<ConnectionEndpoint, LavalinkNodeConnection> _connectedNodes = new();

        /// <summary>
        /// Creates a new instance of this Lavalink extension.
        /// </summary>
        internal LavalinkExtension()
        {
            this.ConnectedNodes = new ReadOnlyConcurrentDictionary<ConnectionEndpoint, LavalinkNodeConnection>(this._connectedNodes);
        }

        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="Client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="System.InvalidOperationException"/>
        protected internal override void Setup(DiscordClient Client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = Client;

            this._nodeDisconnected = new AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED", TimeSpan.Zero, this.Client.EventErrorHandler);
        }

        /// <summary>
        /// Connect to a Lavalink node.
        /// </summary>
        /// <param name="Config">Lavalink client configuration.</param>
        /// <returns>The established Lavalink connection.</returns>
        public async Task<LavalinkNodeConnection> ConnectAsync(LavalinkConfiguration Config)
        {
            if (this._connectedNodes.ContainsKey(Config.SocketEndpoint))
                return this._connectedNodes[Config.SocketEndpoint];

            var con = new LavalinkNodeConnection(this.Client, this, Config);
            con.NodeDisconnected += this.Con_NodeDisconnected;
            con.Disconnected += this.Con_Disconnected;
            this._connectedNodes[con.NodeEndpoint] = con;
            try
            {
                await con.StartAsync().ConfigureAwait(false);
            }
            catch
            {
                this.Con_NodeDisconnected(con);
                throw;
            }

            return con;
        }

        /// <summary>
        /// Gets the Lavalink node connection for the specified endpoint.
        /// </summary>
        /// <param name="Endpoint">Endpoint at which the node resides.</param>
        /// <returns>Lavalink node connection.</returns>
        public LavalinkNodeConnection GetNodeConnection(ConnectionEndpoint Endpoint)
            => this._connectedNodes.ContainsKey(Endpoint) ? this._connectedNodes[Endpoint] : null;

        /// <summary>
        /// Gets a Lavalink node connection based on load balancing and an optional voice region.
        /// </summary>
        /// <param name="Region">The region to compare with the node's <see cref="LavalinkConfiguration.Region"/>, if any.</param>
        /// <returns>The least load affected node connection, or null if no nodes are present.</returns>
        public LavalinkNodeConnection GetIdealNodeConnection(DiscordVoiceRegion Region = null)
        {
            if (this._connectedNodes.Count <= 1)
                return this._connectedNodes.Values.FirstOrDefault();

            var nodes = this._connectedNodes.Values.ToArray();

            if (Region != null)
            {
                var regionPredicate = new Func<LavalinkNodeConnection, bool>(X => X.Region == Region);

                if (nodes.Any(regionPredicate))
                    nodes = nodes.Where(regionPredicate).ToArray();

                if (nodes.Count() <= 1)
                    return nodes.FirstOrDefault();
            }

            return this.FilterByLoad(nodes);
        }

        /// <summary>
        /// Gets a Lavalink guild connection from a <see cref="DisCatSharp.Entities.DiscordGuild"/>.
        /// </summary>
        /// <param name="Guild">The guild the connection is on.</param>
        /// <returns>The found guild connection, or null if one could not be found.</returns>
        public LavalinkGuildConnection GetGuildConnection(DiscordGuild Guild)
        {
            var nodes = this._connectedNodes.Values;
            var node = nodes.FirstOrDefault(X => X._connectedGuilds.ContainsKey(Guild.Id));
            return node?.GetGuildConnection(Guild);
        }

        /// <summary>
        /// Filters the by load.
        /// </summary>
        /// <param name="Nodes">The nodes.</param>
        private LavalinkNodeConnection FilterByLoad(LavalinkNodeConnection[] Nodes)
        {
            Array.Sort(Nodes, (A, B) =>
            {
                if (!A.Statistics._updated || !B.Statistics._updated)
                    return 0;

                //https://github.com/FredBoat/Lavalink-Client/blob/48bc27784f57be5b95d2ff2eff6665451b9366f5/src/main/java/lavalink/client/io/LavalinkLoadBalancer.java#L122
                //https://github.com/briantanner/eris-lavalink/blob/master/src/PlayerManager.js#L329

                //player count
                var aPenaltyCount = A.Statistics.ActivePlayers;
                var bPenaltyCount = B.Statistics.ActivePlayers;

                //cpu load
                aPenaltyCount += (int)Math.Pow(1.05d, (100 * (A.Statistics.CpuSystemLoad / A.Statistics.CpuCoreCount) * 10) - 10);
                bPenaltyCount += (int)Math.Pow(1.05d, (100 * (B.Statistics.CpuSystemLoad / A.Statistics.CpuCoreCount) * 10) - 10);

                //frame load
                if (A.Statistics.AverageDeficitFramesPerMinute > 0)
                {
                    //deficit frame load
                    aPenaltyCount += (int)((Math.Pow(1.03d, 500f * (A.Statistics.AverageDeficitFramesPerMinute / 3000f)) * 600) - 600);

                    //null frame load
                    aPenaltyCount += (int)((Math.Pow(1.03d, 500f * (A.Statistics.AverageNulledFramesPerMinute / 3000f)) * 300) - 300);
                }

                //frame load
                if (B.Statistics.AverageDeficitFramesPerMinute > 0)
                {
                    //deficit frame load
                    bPenaltyCount += (int)((Math.Pow(1.03d, 500f * (B.Statistics.AverageDeficitFramesPerMinute / 3000f)) * 600) - 600);

                    //null frame load
                    bPenaltyCount += (int)((Math.Pow(1.03d, 500f * (B.Statistics.AverageNulledFramesPerMinute / 3000f)) * 300) - 300);
                }

                return aPenaltyCount - bPenaltyCount;
            });

            return Nodes[0];
        }

        /// <summary>
        /// Removes a node.
        /// </summary>
        /// <param name="Node">The node to be removed.</param>
        private void Con_NodeDisconnected(LavalinkNodeConnection Node)
            => this._connectedNodes.TryRemove(Node.NodeEndpoint, out _);

        /// <summary>
        /// Disconnects a node.
        /// </summary>
        /// <param name="Node">The affected node.</param>
        /// <param name="E">The node disconnected event args.</param>
        private Task Con_Disconnected(LavalinkNodeConnection Node, NodeDisconnectedEventArgs E)
            => this._nodeDisconnected.InvokeAsync(Node, E);
    }
}
