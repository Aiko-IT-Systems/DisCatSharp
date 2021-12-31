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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.EventArgs;
using Newtonsoft.Json;

namespace DisCatSharp.Lavalink
{
    internal delegate void ChannelDisconnectedEventHandler(LavalinkGuildConnection Node);

    /// <summary>
    /// Represents a Lavalink connection to a channel.
    /// </summary>
    public sealed class LavalinkGuildConnection
    {
        /// <summary>
        /// Triggered whenever Lavalink updates player status.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
        {
            add { this._playerUpdated.Register(value); }
            remove { this._playerUpdated.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> _playerUpdated;

        /// <summary>
        /// Triggered whenever playback of a track starts.
        /// <para>This is only available for version 3.3.1 and greater.</para>
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
        {
            add { this._playbackStarted.Register(value); }
            remove { this._playbackStarted.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> _playbackStarted;

        /// <summary>
        /// Triggered whenever playback of a track finishes.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
        {
            add { this._playbackFinished.Register(value); }
            remove { this._playbackFinished.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> _playbackFinished;

        /// <summary>
        /// Triggered whenever playback of a track gets stuck.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
        {
            add { this._trackStuck.Register(value); }
            remove { this._trackStuck.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> _trackStuck;

        /// <summary>
        /// Triggered whenever playback of a track encounters an error.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
        {
            add { this._trackException.Register(value); }
            remove { this._trackException.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> _trackException;

        /// <summary>
        /// Triggered whenever Discord Voice WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<LavalinkGuildConnection, WebSocketCloseEventArgs> DiscordWebSocketClosed
        {
            add { this._webSocketClosed.Register(value); }
            remove { this._webSocketClosed.Unregister(value); }
        }
        private readonly AsyncEvent<LavalinkGuildConnection, WebSocketCloseEventArgs> _webSocketClosed;

        /// <summary>
        /// Gets whether this channel is still connected.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref this._isDisposed) && this.Channel != null;
        private bool _isDisposed = false;

        /// <summary>
        /// Gets the current player state.
        /// </summary>
        public LavalinkPlayerState CurrentState { get; }

        /// <summary>
        /// Gets the voice channel associated with this connection.
        /// </summary>
        public DiscordChannel Channel => this.VoiceStateUpdate.Channel;

        /// <summary>
        /// Gets the guild associated with this connection.
        /// </summary>
        public DiscordGuild Guild => this.Channel.Guild;

        /// <summary>
        /// Gets the Lavalink node associated with this connection.
        /// </summary>
        public LavalinkNodeConnection Node { get; }

        /// <summary>
        /// Gets the guild id string.
        /// </summary>
        internal string GuildIdString => this.GuildId.ToString(CultureInfo.InvariantCulture);
        /// <summary>
        /// Gets the guild id.
        /// </summary>
        internal ulong GuildId => this.Channel.Guild.Id;
        /// <summary>
        /// Gets or sets the voice state update.
        /// </summary>
        internal VoiceStateUpdateEventArgs VoiceStateUpdate { get; set; }
        /// <summary>
        /// Gets or sets the voice ws disconnect tcs.
        /// </summary>
        internal TaskCompletionSource<bool> VoiceWsDisconnectTcs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkGuildConnection"/> class.
        /// </summary>
        /// <param name="Node">The node.</param>
        /// <param name="Channel">The channel.</param>
        /// <param name="Vstu">The vstu.</param>
        internal LavalinkGuildConnection(LavalinkNodeConnection Node, DiscordChannel Channel, VoiceStateUpdateEventArgs Vstu)
        {
            this.Node = Node;
            this.VoiceStateUpdate = Vstu;
            this.CurrentState = new LavalinkPlayerState();
            this.VoiceWsDisconnectTcs = new TaskCompletionSource<bool>();

            Volatile.Write(ref this._isDisposed, false);

            this._playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("LAVALINK_PLAYER_UPDATE", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>("LAVALINK_PLAYBACK_STARTED", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>("LAVALINK_PLAYBACK_FINISHED", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>("LAVALINK_TRACK_STUCK", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_TRACK_EXCEPTION", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
            this._webSocketClosed = new AsyncEvent<LavalinkGuildConnection, WebSocketCloseEventArgs>("LAVALINK_DISCORD_WEBSOCKET_CLOSED", TimeSpan.Zero, this.Node.Discord.EventErrorHandler);
        }

        /// <summary>
        /// Disconnects the connection from the voice channel.
        /// </summary>
        /// <param name="ShouldDestroy">Whether the connection should be destroyed on the Lavalink server when leaving.</param>

        public Task Disconnect(bool ShouldDestroy = true)
            => this.DisconnectInternalAsync(ShouldDestroy);

        /// <summary>
        /// Disconnects the internal async.
        /// </summary>
        /// <param name="ShouldDestroy">If true, should destroy.</param>
        /// <param name="IsManualDisconnection">If true, is manual disconnection.</param>

        internal async Task DisconnectInternalAsync(bool ShouldDestroy, bool IsManualDisconnection = false)
        {
            if (!this.IsConnected && !IsManualDisconnection)
                throw new InvalidOperationException("This connection is not valid.");

            Volatile.Write(ref this._isDisposed, true);

            if (ShouldDestroy)
                await this.Node.SendPayloadAsync(new LavalinkDestroy(this)).ConfigureAwait(false);

            if (!IsManualDisconnection)
            {
                await this.SendVoiceUpdateAsync().ConfigureAwait(false);
                this.ChannelDisconnected?.Invoke(this);
            }
        }

        /// <summary>
        /// Sends the voice update async.
        /// </summary>

        internal async Task SendVoiceUpdateAsync()
        {
            var vsd = new VoiceDispatch
            {
                OpCode = 4,
                Payload = new VoiceStateUpdatePayload
                {
                    GuildId = this.GuildId,
                    ChannelId = null,
                    Deafened = false,
                    Muted = false
                }
            };
            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            await (this.Channel.Discord as DiscordClient).WsSendAsync(vsj).ConfigureAwait(false);
        }

        /// <summary>
        /// Searches for specified terms.
        /// </summary>
        /// <param name="SearchQuery">What to search for.</param>
        /// <param name="Type">What platform will search for.</param>
        /// <returns>A collection of tracks matching the criteria.</returns>
        public Task<LavalinkLoadResult> GetTracks(string SearchQuery, LavalinkSearchType Type = LavalinkSearchType.Youtube)
            => this.Node.Rest.GetTracks(SearchQuery, Type);

        /// <summary>
        /// Loads tracks from specified URL.
        /// </summary>
        /// <param name="Uri">URL to load tracks from.</param>
        /// <returns>A collection of tracks from the URL.</returns>
        public Task<LavalinkLoadResult> GetTracks(Uri Uri)
            => this.Node.Rest.GetTracks(Uri);

        /// <summary>
        /// Loads tracks from a local file.
        /// </summary>
        /// <param name="File">File to load tracks from.</param>
        /// <returns>A collection of tracks from the file.</returns>
        public Task<LavalinkLoadResult> GetTracks(FileInfo File)
            => this.Node.Rest.GetTracks(File);

        /// <summary>
        /// Queues the specified track for playback.
        /// </summary>
        /// <param name="Track">Track to play.</param>
        public async Task PlayAsync(LavalinkTrack Track)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            this.CurrentState.CurrentTrack = Track;
            await this.Node.SendPayloadAsync(new LavalinkPlay(this, Track)).ConfigureAwait(false);
        }

        /// <summary>
        /// Queues the specified track for playback. The track will be played from specified start timestamp to specified end timestamp.
        /// </summary>
        /// <param name="Track">Track to play.</param>
        /// <param name="Start">Timestamp to start playback at.</param>
        /// <param name="End">Timestamp to stop playback at.</param>
        public async Task PlayPartialAsync(LavalinkTrack Track, TimeSpan Start, TimeSpan End)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (Start.TotalMilliseconds < 0 || End <= Start)
                throw new ArgumentException("Both start and end timestamps need to be greater or equal to zero, and the end timestamp needs to be greater than start timestamp.");

            this.CurrentState.CurrentTrack = Track;
            await this.Node.SendPayloadAsync(new LavalinkPlayPartial(this, Track, Start, End)).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the player completely.
        /// </summary>
        public async Task StopAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkStop(this)).ConfigureAwait(false);
        }

        /// <summary>
        /// Pauses the player.
        /// </summary>
        public async Task PauseAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkPause(this, true)).ConfigureAwait(false);
        }

        /// <summary>
        /// Resumes playback.
        /// </summary>
        public async Task ResumeAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkPause(this, false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Seeks the current track to specified position.
        /// </summary>
        /// <param name="Position">Position to seek to.</param>
        public async Task SeekAsync(TimeSpan Position)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkSeek(this, Position)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the playback volume. This might incur a lot of CPU usage.
        /// </summary>
        /// <param name="Volume">Volume to set. Needs to be greater or equal to 0, and less than or equal to 100. 100 means 100% and is the default value.</param>
        public async Task SetVolumeAsync(int Volume)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (Volume < 0 || Volume > 1000)
                throw new ArgumentOutOfRangeException(nameof(Volume), "Volume needs to range from 0 to 1000.");

            await this.Node.SendPayloadAsync(new LavalinkVolume(this, Volume)).ConfigureAwait(false);
        }

        /// <summary>
        /// Adjusts the specified bands in the audio equalizer. This will alter the sound output, and might incur a lot of CPU usage.
        /// </summary>
        /// <param name="Bands">Bands adjustments to make. You must specify one adjustment per band at most.</param>
        public async Task AdjustEqualizerAsync(params LavalinkBandAdjustment[] Bands)
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            if (Bands?.Any() != true)
                return;

            if (Bands.Distinct(new LavalinkBandAdjustmentComparer()).Count() != Bands.Count())
                throw new InvalidOperationException("You cannot specify multiple modifiers for the same band.");

            await this.Node.SendPayloadAsync(new LavalinkEqualizer(this, Bands)).ConfigureAwait(false);
        }

        /// <summary>
        /// Resets the audio equalizer to default values.
        /// </summary>
        public async Task ResetEqualizerAsync()
        {
            if (!this.IsConnected)
                throw new InvalidOperationException("This connection is not valid.");

            await this.Node.SendPayloadAsync(new LavalinkEqualizer(this, Enumerable.Range(0, 15).Select(X => new LavalinkBandAdjustment(X, 0)))).ConfigureAwait(false);
        }

        /// <summary>
        /// Internals the update player state async.
        /// </summary>
        /// <param name="NewState">The new state.</param>

        internal Task InternalUpdatePlayerState(LavalinkState NewState)
        {
            this.CurrentState.LastUpdate = NewState.Time;
            this.CurrentState.PlaybackPosition = NewState.Position;

            return this._playerUpdated.InvokeAsync(this, new PlayerUpdateEventArgs(this, NewState.Time, NewState.Position));
        }

        /// <summary>
        /// Internals the playback started async.
        /// </summary>
        /// <param name="Track">The track.</param>

        internal Task InternalPlaybackStarted(string Track)
        {
            var ea = new TrackStartEventArgs(this, LavalinkUtilities.DecodeTrack(Track));
            return this._playbackStarted.InvokeAsync(this, ea);
        }

        /// <summary>
        /// Internals the playback finished async.
        /// </summary>
        /// <param name="E">The e.</param>

        internal Task InternalPlaybackFinished(TrackFinishData E)
        {
            if (E.Reason != TrackEndReason.Replaced)
                this.CurrentState.CurrentTrack = default;

            var ea = new TrackFinishEventArgs(this, LavalinkUtilities.DecodeTrack(E.Track), E.Reason);
            return this._playbackFinished.InvokeAsync(this, ea);
        }

        /// <summary>
        /// Internals the track stuck async.
        /// </summary>
        /// <param name="E">The e.</param>

        internal Task InternalTrackStuck(TrackStuckData E)
        {
            var ea = new TrackStuckEventArgs(this, E.Threshold, LavalinkUtilities.DecodeTrack(E.Track));
            return this._trackStuck.InvokeAsync(this, ea);
        }

        /// <summary>
        /// Internals the track exception async.
        /// </summary>
        /// <param name="E">The e.</param>

        internal Task InternalTrackException(TrackExceptionData E)
        {
            var ea = new TrackExceptionEventArgs(this, E.Error, LavalinkUtilities.DecodeTrack(E.Track));
            return this._trackException.InvokeAsync(this, ea);
        }

        /// <summary>
        /// Internals the web socket closed async.
        /// </summary>
        /// <param name="E">The e.</param>

        internal Task InternalWebSocketClosed(WebSocketCloseEventArgs E)
            => this._webSocketClosed.InvokeAsync(this, E);

        internal event ChannelDisconnectedEventHandler ChannelDisconnected;
    }
}
