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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.EventArgs;
using DisCatSharp.Net;
using DisCatSharp.Net.WebSocket;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Lavalink;

internal delegate void NodeDisconnectedEventHandler(LavalinkNodeConnection node);

/// <summary>
/// Represents a connection to a Lavalink node.
/// </summary>
public sealed class LavalinkNodeConnection
{
	/// <summary>
	/// Triggered whenever Lavalink WebSocket throws an exception.
	/// </summary>
	public event AsyncEventHandler<LavalinkNodeConnection, SocketErrorEventArgs> LavalinkSocketErrored
	{
		add => this._lavalinkSocketError.Register(value);
		remove => this._lavalinkSocketError.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs> _lavalinkSocketError;

	/// <summary>
	/// Triggered when this node disconnects.
	/// </summary>
	public event AsyncEventHandler<LavalinkNodeConnection, NodeDisconnectedEventArgs> Disconnected
	{
		add => this._disconnected.Register(value);
		remove => this._disconnected.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs> _disconnected;

	/// <summary>
	/// Triggered when this node receives a statistics update.
	/// </summary>
	public event AsyncEventHandler<LavalinkNodeConnection, StatisticsReceivedEventArgs> StatisticsReceived
	{
		add => this._statsReceived.Register(value);
		remove => this._statsReceived.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs> _statsReceived;

	/// <summary>
	/// Triggered whenever any of the players on this node is updated.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> PlayerUpdated
	{
		add => this._playerUpdated.Register(value);
		remove => this._playerUpdated.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> _playerUpdated;

	/// <summary>
	/// Triggered whenever playback of a track starts.
	/// <para>This is only available for version 3.3.1 and greater.</para>
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildConnection, TrackStartEventArgs> PlaybackStarted
	{
		add => this._playbackStarted.Register(value);
		remove => this._playbackStarted.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs> _playbackStarted;

	/// <summary>
	/// Triggered whenever playback of a track finishes.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> PlaybackFinished
	{
		add => this._playbackFinished.Register(value);
		remove => this._playbackFinished.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs> _playbackFinished;

	/// <summary>
	/// Triggered whenever playback of a track gets stuck.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildConnection, TrackStuckEventArgs> TrackStuck
	{
		add => this._trackStuck.Register(value);
		remove => this._trackStuck.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs> _trackStuck;

	/// <summary>
	/// Triggered whenever playback of a track encounters an error.
	/// </summary>
	public event AsyncEventHandler<LavalinkGuildConnection, TrackExceptionEventArgs> TrackException
	{
		add => this._trackException.Register(value);
		remove => this._trackException.Unregister(value);
	}
	private readonly AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs> _trackException;

	/// <summary>
	/// Gets the remote endpoint of this Lavalink node connection.
	/// </summary>
	public ConnectionEndpoint NodeEndpoint => this.Configuration.SocketEndpoint;

	/// <summary>
	/// Gets whether the client is connected to Lavalink.
	/// </summary>
	public bool IsConnected => !Volatile.Read(ref this._isDisposed);
	private bool _isDisposed;
	private int _backoff;
	/// <summary>
	/// The minimum backoff.
	/// </summary>
	private const int MINIMUM_BACKOFF = 7500;
	/// <summary>
	/// The maximum backoff.
	/// </summary>
	private const int MAXIMUM_BACKOFF = 120000;

	/// <summary>
	/// Gets the current resource usage statistics.
	/// </summary>
	public LavalinkStatistics Statistics { get; }

	/// <summary>
	/// Gets a dictionary of Lavalink guild connections for this node.
	/// </summary>
	public IReadOnlyDictionary<ulong, LavalinkGuildConnection> ConnectedGuilds { get; }
	internal ConcurrentDictionary<ulong, LavalinkGuildConnection> ConnectedGuildsInternal = new();

	/// <summary>
	/// Gets the REST client for this Lavalink connection.
	/// </summary>
	public LavalinkRestClient Rest { get; }

	/// <summary>
	/// Gets the parent extension which this node connection belongs to.
	/// </summary>
	public LavalinkExtension Parent { get; }

	/// <summary>
	/// Gets the Discord client this node connection belongs to.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the configuration.
	/// </summary>
	internal LavalinkConfiguration Configuration { get; }
	/// <summary>
	/// Gets the region.
	/// </summary>
	internal DiscordVoiceRegion Region { get; }

	/// <summary>
	/// Gets or sets the web socket.
	/// </summary>
	private IWebSocketClient _webSocket;

	/// <summary>
	/// Gets the voice state updates.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> _voiceStateUpdates;

	/// <summary>
	/// Gets the voice server updates.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> _voiceServerUpdates;

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkNodeConnection"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="extension">the event.tension.</param>
	/// <param name="config">The config.</param>
	internal LavalinkNodeConnection(DiscordClient client, LavalinkExtension extension, LavalinkConfiguration config)
	{
		this.Discord = client;
		this.Parent = extension;
		this.Configuration = new LavalinkConfiguration(config);

		if (config.Region != null && this.Discord.VoiceRegions.Values.Contains(config.Region))
			this.Region = config.Region;

		this.ConnectedGuilds = new ReadOnlyConcurrentDictionary<ulong, LavalinkGuildConnection>(this.ConnectedGuildsInternal);
		this.Statistics = new LavalinkStatistics();

		this._lavalinkSocketError = new AsyncEvent<LavalinkNodeConnection, SocketErrorEventArgs>("LAVALINK_SOCKET_ERROR", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._disconnected = new AsyncEvent<LavalinkNodeConnection, NodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._statsReceived = new AsyncEvent<LavalinkNodeConnection, StatisticsReceivedEventArgs>("LAVALINK_STATS_RECEIVED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._playerUpdated = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("LAVALINK_PLAYER_UPDATED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._playbackStarted = new AsyncEvent<LavalinkGuildConnection, TrackStartEventArgs>("LAVALINK_PLAYBACK_STARTED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._playbackFinished = new AsyncEvent<LavalinkGuildConnection, TrackFinishEventArgs>("LAVALINK_PLAYBACK_FINISHED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._trackStuck = new AsyncEvent<LavalinkGuildConnection, TrackStuckEventArgs>("LAVALINK_TRACK_STUCK", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._trackException = new AsyncEvent<LavalinkGuildConnection, TrackExceptionEventArgs>("LAVALINK_TRACK_EXCEPTION", TimeSpan.Zero, this.Discord.EventErrorHandler);

		this._voiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
		this._voiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
		this.Discord.VoiceStateUpdated += this.Discord_VoiceStateUpdated;
		this.Discord.VoiceServerUpdated += this.Discord_VoiceServerUpdated;

		this.Rest = new LavalinkRestClient(this.Configuration, this.Discord);

		Volatile.Write(ref this._isDisposed, false);
	}

	/// <summary>
	/// Establishes a connection to the Lavalink node.
	/// </summary>
	/// <returns></returns>
	internal async Task StartAsync()
	{
		if (this.Discord?.CurrentUser?.Id == null || this.Discord?.ShardCount == null)
			throw new InvalidOperationException("This operation requires the Discord client to be fully initialized.");

		this._webSocket = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy, this.Discord.ServiceProvider);
		this._webSocket.Connected += this.WebSocket_OnConnect;
		this._webSocket.Disconnected += this.WebSocket_OnDisconnect;
		this._webSocket.ExceptionThrown += this.WebSocket_OnException;
		this._webSocket.MessageReceived += this.WebSocket_OnMessage;

		this._webSocket.AddDefaultHeader("Authorization", this.Configuration.Password);
		this._webSocket.AddDefaultHeader("Num-Shards", this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture));
		this._webSocket.AddDefaultHeader("User-Id", this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
		this._webSocket.AddDefaultHeader("Client-Name", $"DisCatSharp.Lavalink version {this.Discord.VersionString}");
		if (this.Configuration.ResumeKey != null)
			this._webSocket.AddDefaultHeader("Resume-Key", this.Configuration.ResumeKey);

		do
		{
			try
			{
				if (this._backoff != 0)
				{
					await Task.Delay(this._backoff).ConfigureAwait(false);
					this._backoff = Math.Min(this._backoff * 2, MAXIMUM_BACKOFF);
				}
				else
				{
					this._backoff = MINIMUM_BACKOFF;
				}

				await this._webSocket.ConnectAsync(new Uri(this.Configuration.SocketEndpoint.ToWebSocketString())).ConfigureAwait(false);
				break;
			}
			catch (PlatformNotSupportedException)
			{ throw; }
			catch (NotImplementedException)
			{ throw; }
			catch (Exception ex)
			{
				if (!this.Configuration.SocketAutoReconnect || this._backoff == MAXIMUM_BACKOFF)
				{
					this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, "Failed to connect to Lavalink.");
					throw;
				}
				else
				{
					this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, $"Failed to connect to Lavalink, retrying in {this._backoff} ms.");
				}
			}
		}
		while (this.Configuration.SocketAutoReconnect);

		Volatile.Write(ref this._isDisposed, false);
	}

	/// <summary>
	/// Stops this Lavalink node connection and frees resources.
	/// </summary>
	/// <returns></returns>
	public async Task StopAsync()
	{
		foreach (var kvp in this.ConnectedGuildsInternal)
			await kvp.Value.DisconnectAsync().ConfigureAwait(false);

		this.NodeDisconnected?.Invoke(this);

		Volatile.Write(ref this._isDisposed, true);
		await this._webSocket.DisconnectAsync().ConfigureAwait(false);
		// this should not be here, no?
		//await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this)).ConfigureAwait(false);
	}

	/// <summary>
	/// Connects this Lavalink node to specified Discord channel.
	/// </summary>
	/// <param name="channel">Voice channel to connect to.</param>
	/// <returns>Channel connection, which allows for playback control.</returns>
	public async Task<LavalinkGuildConnection> ConnectAsync(DiscordChannel channel)
	{
		if (this.ConnectedGuildsInternal.ContainsKey(channel.Guild.Id))
			return this.ConnectedGuildsInternal[channel.Guild.Id];

		if (channel.Guild == null || (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage))
			throw new ArgumentException("Invalid channel specified.", nameof(channel));

		var vstut = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
		var vsrut = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
		this._voiceStateUpdates[channel.Guild.Id] = vstut;
		this._voiceServerUpdates[channel.Guild.Id] = vsrut;

		var vsd = new VoiceDispatch
		{
			OpCode = 4,
			Payload = new VoiceStateUpdatePayload
			{
				GuildId = channel.Guild.Id,
				ChannelId = channel.Id,
				Deafened = false,
				Muted = false
			}
		};
		var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
		await (channel.Discord as DiscordClient).WsSendAsync(vsj).ConfigureAwait(false);
		var vstu = await vstut.Task.ConfigureAwait(false);
		var vsru = await vsrut.Task.ConfigureAwait(false);
		await this.SendPayloadAsync(new LavalinkVoiceUpdate(vstu, vsru)).ConfigureAwait(false);

		var con = new LavalinkGuildConnection(this, channel, vstu);
		con.ChannelDisconnected += this.Con_ChannelDisconnected;
		con.PlayerUpdated += (s, e) => this._playerUpdated.InvokeAsync(s, e);
		con.PlaybackStarted += (s, e) => this._playbackStarted.InvokeAsync(s, e);
		con.PlaybackFinished += (s, e) => this._playbackFinished.InvokeAsync(s, e);
		con.TrackStuck += (s, e) => this._trackStuck.InvokeAsync(s, e);
		con.TrackException += (s, e) => this._trackException.InvokeAsync(s, e);
		this.ConnectedGuildsInternal[channel.Guild.Id] = con;

		return con;
	}

	/// <summary>
	/// Gets a Lavalink connection to specified Discord channel.
	/// </summary>
	/// <param name="guild">Guild to get connection for.</param>
	/// <returns>Channel connection, which allows for playback control.</returns>
	public LavalinkGuildConnection GetGuildConnection(DiscordGuild guild)
		=> this.ConnectedGuildsInternal.TryGetValue(guild.Id, out var lgc) && lgc.IsConnected ? lgc : null;

	/// <summary>
	/// Sends the payload async.
	/// </summary>
	/// <param name="payload">The payload.</param>
	internal async Task SendPayloadAsync(LavalinkPayload payload)
		=> await this.WsSendAsync(JsonConvert.SerializeObject(payload, Formatting.None)).ConfigureAwait(false);

	/// <summary>
	/// Webs the socket_ on message.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">the event.ent.</param>
	private async Task WebSocket_OnMessage(IWebSocketClient client, SocketMessageEventArgs e)
	{
		if (e is not SocketTextMessageEventArgs et)
		{
			this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, "Lavalink sent binary data - unable to process");
			return;
		}

		this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, et.Message);

		var json = et.Message;
		var jsonData = JObject.Parse(json);
		switch (jsonData["op"].ToString())
		{
			case "playerUpdate":
				var gid = (ulong)jsonData["guildId"];
				var state = jsonData["state"].ToObject<LavalinkState>();
				if (this.ConnectedGuildsInternal.TryGetValue(gid, out var lvl))
					await lvl.InternalUpdatePlayerStateAsync(state).ConfigureAwait(false);
				break;

			case "stats":
				var statsRaw = jsonData.ToObject<LavalinkStats>();
				this.Statistics.Update(statsRaw);
				await this._statsReceived.InvokeAsync(this, new StatisticsReceivedEventArgs(this.Discord.ServiceProvider, this.Statistics)).ConfigureAwait(false);
				break;

			case "event":
				var evtype = jsonData["type"].ToObject<EventType>();
				var guildId = (ulong)jsonData["guildId"];
				switch (evtype)
				{
					case EventType.TrackStartEvent:
						if (this.ConnectedGuildsInternal.TryGetValue(guildId, out var lvlEvtst))
							await lvlEvtst.InternalPlaybackStartedAsync(jsonData["track"].ToString()).ConfigureAwait(false);
						break;

					case EventType.TrackEndEvent:
						var reason = TrackEndReason.Cleanup;
						switch (jsonData["reason"].ToString())
						{
							case "FINISHED":
								reason = TrackEndReason.Finished;
								break;
							case "LOAD_FAILED":
								reason = TrackEndReason.LoadFailed;
								break;
							case "STOPPED":
								reason = TrackEndReason.Stopped;
								break;
							case "REPLACED":
								reason = TrackEndReason.Replaced;
								break;
							case "CLEANUP":
								reason = TrackEndReason.Cleanup;
								break;
						}
						if (this.ConnectedGuildsInternal.TryGetValue(guildId, out var lvlEvtf))
							await lvlEvtf.InternalPlaybackFinishedAsync(new TrackFinishData { Track = jsonData["track"].ToString(), Reason = reason }).ConfigureAwait(false);
						break;

					case EventType.TrackStuckEvent:
						if (this.ConnectedGuildsInternal.TryGetValue(guildId, out var lvlEvts))
							await lvlEvts.InternalTrackStuckAsync(new TrackStuckData { Track = jsonData["track"].ToString(), Threshold = (long)jsonData["thresholdMs"] }).ConfigureAwait(false);
						break;

					case EventType.TrackExceptionEvent:
						var severity = LoadFailedSeverity.Common;

						switch (jsonData["severity"].ToString())
						{
							case "COMMON":
								severity = LoadFailedSeverity.Common;
								break;

							case "SUSPICIOUS":
								severity = LoadFailedSeverity.Suspicious;
								break;

							case "FAULT":
								severity = LoadFailedSeverity.Fault;
								break;
						}
						if (this.ConnectedGuildsInternal.TryGetValue(guildId, out var lvlEvte))
							await lvlEvte.InternalTrackExceptionAsync(new LavalinkLoadFailedInfo { Message = jsonData["message"].ToString(), Severity = severity }, jsonData["track"].ToString()).ConfigureAwait(false);
						break;

					case EventType.WebSocketClosedEvent:
						if (this.ConnectedGuildsInternal.TryGetValue(guildId, out var lvlEwsce))
						{
							lvlEwsce.VoiceWsDisconnectTcs.SetResult(true);
							await lvlEwsce.InternalWebSocketClosedAsync(new WebSocketCloseEventArgs(jsonData["code"].ToObject<int>(), jsonData["reason"].ToString(), jsonData["byRemote"].ToObject<bool>(), this.Discord.ServiceProvider)).ConfigureAwait(false);
						}
						break;
				}
				break;
		}
	}

	/// <summary>
	/// Webs the socket_ on exception.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">the event.</param>
	private Task WebSocket_OnException(IWebSocketClient client, SocketErrorEventArgs e)
		=> this._lavalinkSocketError.InvokeAsync(this, new SocketErrorEventArgs(client.ServiceProvider) { Exception = e.Exception });

	/// <summary>
	/// Webs the socket_ on disconnect.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">the event.</param>
	private async Task WebSocket_OnDisconnect(IWebSocketClient client, SocketCloseEventArgs e)
	{
		if (this.IsConnected && e.CloseCode != 1001 && e.CloseCode != -1)
		{
			this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Connection broken ({0}, '{1}'), reconnecting", e.CloseCode, e.CloseMessage);
			await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false)).ConfigureAwait(false);

			if (this.Configuration.SocketAutoReconnect)
				await this.StartAsync().ConfigureAwait(false);
		}
		else if (e.CloseCode != 1001 && e.CloseCode != -1)
		{
			this.Discord.Logger.LogInformation(LavalinkEvents.LavalinkConnectionClosed, "Connection closed ({0}, '{1}')", e.CloseCode, e.CloseMessage);
			this.NodeDisconnected?.Invoke(this);
			await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, true)).ConfigureAwait(false);
		}
		else
		{
			Volatile.Write(ref this._isDisposed, true);
			this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkConnectionClosed, "Lavalink died");
			foreach (var kvp in this.ConnectedGuildsInternal)
			{
				await kvp.Value.SendVoiceUpdateAsync().ConfigureAwait(false);
				_ = this.ConnectedGuildsInternal.TryRemove(kvp.Key, out _);
			}
			this.NodeDisconnected?.Invoke(this);
			await this._disconnected.InvokeAsync(this, new NodeDisconnectedEventArgs(this, false)).ConfigureAwait(false);

			if (this.Configuration.SocketAutoReconnect)
				await this.StartAsync().ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Webs the socket_ on connect.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="ea">the event..</param>
	private async Task WebSocket_OnConnect(IWebSocketClient client, SocketEventArgs ea)
	{
		this.Discord.Logger.LogDebug(LavalinkEvents.LavalinkConnected, "Connection to Lavalink node established");
		this._backoff = 0;

		if (this.Configuration.ResumeKey != null)
			await this.SendPayloadAsync(new LavalinkConfigureResume(this.Configuration.ResumeKey, this.Configuration.ResumeTimeout)).ConfigureAwait(false);
	}

	/// <summary>
	/// Con_S the channel disconnected.
	/// </summary>
	/// <param name="con">The con.</param>
	private void Con_ChannelDisconnected(LavalinkGuildConnection con)
		=> this.ConnectedGuildsInternal.TryRemove(con.GuildId, out _);

	/// <summary>
	/// Discord voice state updated.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">the event.</param>
	private Task Discord_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
	{
		var gld = e.Guild;
		if (gld == null)
			return Task.CompletedTask;

		if (e.User == null)
			return Task.CompletedTask;

		if (e.User.Id == this.Discord.CurrentUser.Id)
		{
			if (this.ConnectedGuildsInternal.TryGetValue(e.Guild.Id, out var lvlgc))
				lvlgc.VoiceStateUpdate = e;

			if (e.After.Channel == null && this.IsConnected && this.ConnectedGuildsInternal.ContainsKey(gld.Id))
			{
				_ = Task.Run(async () =>
				{
					var delayTask = Task.Delay(this.Configuration.WebSocketCloseTimeout);
					var tcs = lvlgc.VoiceWsDisconnectTcs.Task;
					_ = await Task.WhenAny(delayTask, tcs).ConfigureAwait(false);

					await lvlgc.DisconnectInternalAsync(false, true).ConfigureAwait(false);
					_ = this.ConnectedGuildsInternal.TryRemove(gld.Id, out _);
				});
			}

			if (!string.IsNullOrWhiteSpace(e.SessionId) && e.Channel != null && this._voiceStateUpdates.TryRemove(gld.Id, out var xe))
				xe.SetResult(e);
		}

		return Task.CompletedTask;
	}

	/// <summary>
	/// Discord voice server updated.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">the event.</param>
	private Task Discord_VoiceServerUpdated(DiscordClient client, VoiceServerUpdateEventArgs e)
	{
		var gld = e.Guild;
		if (gld == null)
			return Task.CompletedTask;

		if (this.ConnectedGuildsInternal.TryGetValue(e.Guild.Id, out var lvlgc))
		{
			var lvlp = new LavalinkVoiceUpdate(lvlgc.VoiceStateUpdate, e);
			_ = Task.Run(() => this.WsSendAsync(JsonConvert.SerializeObject(lvlp)));
		}

		if (this._voiceServerUpdates.TryRemove(gld.Id, out var xe))
			xe.SetResult(e);

		return Task.CompletedTask;
	}
	/// <summary>
	/// Ws the send async.
	/// </summary>
	/// <param name="payload">The payload.</param>

	private async Task WsSendAsync(string payload)
	{
		this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsTx, payload);
		await this._webSocket.SendMessageAsync(payload).ConfigureAwait(false);
	}

	internal event NodeDisconnectedEventHandler NodeDisconnected;
}
