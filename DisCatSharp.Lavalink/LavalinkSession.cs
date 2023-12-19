using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Entities.Websocket;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.Enums.Websocket;
using DisCatSharp.Lavalink.EventArgs;
using DisCatSharp.Lavalink.Payloads;
using DisCatSharp.Net;
using DisCatSharp.Net.WebSocket;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Lavalink;

internal delegate void SessionDisconnectedEventHandler(LavalinkSession node);

/// <summary>
/// Represents a <see cref="LavalinkSession"/>.
/// </summary>
public sealed class LavalinkSession
{
	/// <summary>
	/// Triggered whenever Lavalink WebSocket throws an exception.
	/// </summary>
	public event AsyncEventHandler<LavalinkSession, SocketErrorEventArgs> LavalinkSocketErrored
	{
		add => this._lavalinkSocketError.Register(value);
		remove => this._lavalinkSocketError.Unregister(value);
	}

	private readonly AsyncEvent<LavalinkSession, SocketErrorEventArgs> _lavalinkSocketError;

	/// <summary>
	/// Triggered when this session disconnects.
	/// </summary>
	internal event AsyncEventHandler<LavalinkSession, LavalinkSessionDisconnectedEventArgs> LavalinkSessionDisconnected
	{
		add => this._lavalinkSessionDisconnected.Register(value);
		remove => this._lavalinkSessionDisconnected.Unregister(value);
	}

	private readonly AsyncEvent<LavalinkSession, LavalinkSessionDisconnectedEventArgs> _lavalinkSessionDisconnected;

	/// <summary>
	/// Triggered when this session connects.
	/// </summary>
	internal event AsyncEventHandler<LavalinkSession, LavalinkSessionConnectedEventArgs> LavalinkSessionConnected
	{
		add => this._lavalinkSessionConnected.Register(value);
		remove => this._lavalinkSessionConnected.Unregister(value);
	}

	private readonly AsyncEvent<LavalinkSession, LavalinkSessionConnectedEventArgs> _lavalinkSessionConnected;

	/// <summary>
	/// Triggered when a <see cref="LavalinkStats"/> are received.
	/// </summary>
	public event AsyncEventHandler<LavalinkSession, LavalinkStatsReceivedEventArgs> StatsReceived
	{
		add => this._statsReceived.Register(value);
		remove => this._statsReceived.Unregister(value);
	}

	private readonly AsyncEvent<LavalinkSession, LavalinkStatsReceivedEventArgs> _statsReceived;

	/// <summary>
	/// Triggered when a <see cref="LavalinkGuildPlayer"/> gets destroyed.
	/// </summary>
	public event AsyncEventHandler<LavalinkSession, GuildPlayerDestroyedEventArgs> GuildPlayerDestroyed
	{
		add => this.GuildPlayerDestroyedEvent.Register(value);
		remove => this.GuildPlayerDestroyedEvent.Unregister(value);
	}

	internal readonly AsyncEvent<LavalinkSession, GuildPlayerDestroyedEventArgs> GuildPlayerDestroyedEvent;

	/// <summary>
	/// Triggered when the websocket to discord gets closed.
	/// </summary>
	public event AsyncEventHandler<LavalinkSession, LavalinkWebsocketClosedEventArgs> WebsocketClosed
	{
		add => this._websocketClosed.Register(value);
		remove => this._websocketClosed.Unregister(value);
	}

	private readonly AsyncEvent<LavalinkSession, LavalinkWebsocketClosedEventArgs> _websocketClosed;

	/// <summary>
	/// Gets the remote endpoint of this Lavalink node connection.
	/// </summary>
	public ConnectionEndpoint NodeEndpoint
		=> this.Config.SocketEndpoint;

	/// <summary>
	/// Gets whether the client is connected to Lavalink.
	/// </summary>
	public bool IsConnected => !Volatile.Read(ref this._isDisposed);

	/// <summary>
	/// Whether this <see cref="LavalinkSession"/> is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	/// Gets the current backoff for reconnecting.
	/// </summary>
	private int _backoff;

	/// <summary>
	/// Gets the minimum backoff.
	/// </summary>
	private const int MINIMUM_BACKOFF = 7500;

	/// <summary>
	/// Gets the maximum backoff.
	/// </summary>
	private const int MAXIMUM_BACKOFF = 120000;

	/// <summary>
	/// Gets a dictionary of Lavalink guild connections for this node.
	/// </summary>
	public IReadOnlyDictionary<ulong, LavalinkGuildPlayer> ConnectedPlayers
		=> this.ConnectedPlayersInternal;

	internal ConcurrentDictionary<ulong, LavalinkGuildPlayer> ConnectedPlayersInternal = new();

	/// <summary>
	/// Gets the REST client for this Lavalink connection.
	/// </summary>
	internal LavalinkRestClient Rest { get; }

	/// <summary>
	/// Gets the Discord client this node connection belongs to.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the parent <see cref="LavalinkExtension"/>.
	/// </summary>
	internal LavalinkExtension Extension { get; }

	/// <summary>
	/// Gets the <see cref="LavalinkConfiguration"/>.
	/// </summary>
	internal LavalinkConfiguration Config { get; }

	/// <summary>
	/// Gets the voice region.
	/// </summary>
	internal DiscordVoiceRegion Region { get; }

	/// <summary>
	/// Gets the current <see cref="LavalinkStats"/>.
	/// </summary>
	public LavalinkStats Statistics { get; internal set; }

	/// <summary>
	/// Gets the current <see cref="LavalinkSessionConfiguration"/>.
	/// </summary>
	public LavalinkSessionConfiguration Configuration { get; internal set; } = new()
	{
		Resuming = true,
		TimeoutSeconds = 60
	};

	/// <summary>
	/// Gets the web socket.
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
	/// Fires when a <see cref="LavalinkSession"/> disconnected.
	/// </summary>
	internal event SessionDisconnectedEventHandler SessionDisconnected;

	/// <summary>
	/// <see cref="TaskCompletionSource"/> for the <see cref="LavalinkConfiguration.SessionId"/>.
	/// </summary>
	private TaskCompletionSource<string> _sessionIdReceived = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkSession"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="extension">The extension.</param>
	/// <param name="config">The lavalink configuration.</param>
	internal LavalinkSession(DiscordClient client, LavalinkExtension extension, LavalinkConfiguration config)
	{
		this.Discord = client;
		this.Extension = extension;
		this.Config = new(config);

		if (config.Region != null! && this.Discord.VoiceRegions.Values.Contains(config.Region))
			this.Region = config.Region;

		this._lavalinkSocketError = new("LAVALINK_SOCKET_ERROR", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._lavalinkSessionDisconnected = new("LAVALINK_SESSION_DISCONNECTED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._lavalinkSessionConnected = new("LAVALINK_SESSION_CONNECTED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this.GuildPlayerDestroyedEvent = new("LAVALINK_PLAYER_DESTROYED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._statsReceived = new("LAVALINK_STATS_RECEIVED", TimeSpan.Zero, this.Discord.EventErrorHandler);
		this._websocketClosed = new("LAVALINK_WEBSOCKET_CLOSED", TimeSpan.Zero, this.Discord.EventErrorHandler);

		this._voiceServerUpdates = new();
		this._voiceStateUpdates = new();
		this.Discord.VoiceStateUpdated += this.Discord_VoiceStateUpdated;
		this.Discord.VoiceServerUpdated += this.Discord_VoiceServerUpdated;
		this.GuildPlayerDestroyed += this.LavalinkGuildPlayerDestroyed;

		this.Rest = new(this.Config, this.Discord);

		Volatile.Write(ref this._isDisposed, false);
	}

	/// <summary>
	/// Gets the lavalink server information.
	/// </summary>
	/// <returns>A <see cref="LavalinkInfo"/> object.</returns>
	public async Task<LavalinkInfo> GetLavalinkInfoAsync()
		=> await this.Rest.GetInfoAsync().ConfigureAwait(false);

	/// <summary>
	/// Gets the lavalink server version
	/// </summary>
	/// <returns>The version <see langword="string"/>.</returns>
	public async Task<string> GetLavalinkVersionAsync()
	{
		var versionInfo = await this.Rest.GetVersionAsync().ConfigureAwait(false);
		return versionInfo.Headers.TryGetValues("Lavalink-Api-Version", out var headerValues)
			? headerValues.First()
			: versionInfo.Response!;
	}

	/// <summary>
	/// Gets the lavalink server statistics.
	/// </summary>
	/// <returns>A <see cref="LavalinkStats"/> object.</returns>
	public async Task<LavalinkStats> GetLavalinkStatsAsync()
	{
		var stats = await this.Rest.GetStatsAsync().ConfigureAwait(false);
		this.Statistics = new(stats, this.Statistics);
		return this.Statistics;
	}

	/// <summary>
	/// Destroys the current session, disconnecting all players.
	/// </summary>
	public async Task DestroyAsync()
	{
		await this.DestroyGuildPlayersAsync().ConfigureAwait(false);
		Volatile.Write(ref this._isDisposed, true);
		await this._webSocket.DisconnectAsync(1000, "Shutting down Lavalink Session").ConfigureAwait(false);
	}

	/// <summary>
	/// Destroys all players.
	/// </summary>
	public async Task DestroyGuildPlayersAsync()
	{
		if (!this.ConnectedPlayersInternal.IsEmpty)
			foreach (var player in this.ConnectedPlayersInternal.Values)
				await player.DisconnectAsync().ConfigureAwait(false);
		this.ConnectedPlayersInternal.Clear();
	}

	/// <summary>
	/// Fires when a <see cref="LavalinkGuildPlayer"/> was destroyed.
	/// </summary>
	/// <param name="sender">The lavalink session.</param>
	/// <param name="args">The guild player destroyed event args containing the destroyed <see cref="LavalinkGuildPlayer"/>.</param>
	private Task LavalinkGuildPlayerDestroyed(LavalinkSession sender, GuildPlayerDestroyedEventArgs args)
	{
		this.ConnectedPlayersInternal.Remove(args.Player.GuildId, out _);
		args.Handled = false;
		return Task.CompletedTask;
	}

	/// <summary>
	/// Connects to a <see cref="DiscordChannel"/>.
	/// </summary>
	/// <param name="channel">The channel to join.</param>
	/// <param name="deafened">Whether to join the channel deafened.</param>
	/// <returns>The created <see cref="LavalinkGuildPlayer"/>.</returns>
	/// <exception cref="ArgumentException"></exception>
	public async Task<LavalinkGuildPlayer> ConnectAsync(DiscordChannel channel, bool deafened = true)
	{
		if (this.ConnectedPlayersInternal.TryGetValue(channel.Guild.Id, out var connectedGuild))
			return connectedGuild;

		if (channel.Guild == null! || (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage))
			throw new ArgumentException("Invalid channel specified.", nameof(channel));

		var vstut = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
		var vsrut = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
		this._voiceStateUpdates[channel.Guild.Id] = vstut;
		this._voiceServerUpdates[channel.Guild.Id] = vsrut;

		var vsd = new DiscordDispatchPayload
		{
			OpCode = 4,
			Payload = new VoiceStateUpdatePayload()
			{
				GuildId = channel.Guild.Id,
				ChannelId = channel.Id,
				Deafened = deafened,
				Muted = false
			}
		};
		await this.Rest.CreatePlayerAsync(this.Config.SessionId!, channel.Guild.Id, this.Config.DefaultVolume).ConfigureAwait(false);
		await this.Discord.WsSendAsync(LavalinkJson.SerializeObject(vsd)).ConfigureAwait(false); // Send voice dispatch to trigger voice state & voice server update
		var vst = await vstut.Task.ConfigureAwait(false); // Wait for voice state update to get session_id
		var vsr = await vsrut.Task.ConfigureAwait(false); // Wait for voice server update to get token, guild_id & endpoint
		await this.Rest.UpdatePlayerVoiceStateAsync(this.Config.SessionId!, channel.Guild.Id, new()
			{
				Endpoint = vsr.Endpoint,
				Token = vsr.VoiceToken,
				SessionId = vst.SessionId
			})
			.ConfigureAwait(false);
		var player = await this.Rest.GetPlayerAsync(this.Config.SessionId!, channel.Guild.Id).ConfigureAwait(false);

		var con = new LavalinkGuildPlayer(this, channel.Guild.Id, player)
		{
			ChannelId = channel.Id
		};
		this.ConnectedPlayersInternal[channel.Guild.Id] = con;

		return con;
	}

	/// <summary>
	/// Configures the current lavalink session.
	/// </summary>
	/// <param name="config">The config update to set.</param>
	/// <returns>The updated session.</returns>
	public async Task<LavalinkSession> ConfigureAsync(LavalinkSessionConfiguration config)
	{
		var newConfig = await this.Rest.UpdateSessionAsync(this.Config.SessionId!, config).ConfigureAwait(false);
		this.Configuration = newConfig;
		return this;
	}

	/// <summary>
	/// Gets the guild player attached to <paramref name="guild"/>.
	/// </summary>
	/// <param name="guild">The guild to get the player for.</param>
	/// <returns>The found player or <see langword="null"/>.</returns>
	public LavalinkGuildPlayer? GetGuildPlayer(DiscordGuild guild)
		=> this.ConnectedPlayersInternal.TryGetValue(guild.Id, out var lgp) && lgp.IsConnected
			? lgp
			: null;

	/// <summary>
	/// Gets all guild players.
	/// </summary>
	/// <returns>The found players or <see langword="null"/>.</returns>
	public IReadOnlyList<LavalinkGuildPlayer>? GetGuildPlayersAsync()
		=> !this.ConnectedPlayersInternal.IsEmpty
			? this.ConnectedPlayersInternal.Values.ToList()
			: null;

	/// <summary>
	/// Gets the lavalink player attached to <paramref name="guild"/>.
	/// <para>Use <see cref="GetGuildPlayer"/> if you want to interact with the actual player.</para>
	/// </summary>
	/// <param name="guild">The guild to get the player for.</param>
	/// <returns>The found player or <see langword="null"/>.</returns>
	public async Task<LavalinkPlayer> GetPlayerAsync(DiscordGuild guild)
		=> await this.Rest.GetPlayerAsync(this.Config.SessionId!, guild.Id).ConfigureAwait(false);

	/// <summary>
	/// Gets all lavalink players.
	/// <para>Use <see cref="GetGuildPlayersAsync"/> if you want to interact with the actual players.</para>
	/// </summary>
	/// <returns>The found players or <see langword="null"/>.</returns>
	public async Task<IReadOnlyList<LavalinkPlayer>> GetPlayersAsync()
		=> await this.Rest.GetPlayersAsync(this.Config.SessionId!).ConfigureAwait(false);

	/// <summary>
	/// Decodes encoded <see cref="LavalinkTrack"/>s.
	/// <para>Might not work with pre 3.0 tracks.</para>
	/// </summary>
	/// <param name="tracks">The tracks to decode.</param>
	/// <returns>A <see cref="List{T}"/> of decoded <see cref="LavalinkTrack"/>s.</returns>
	public async Task<IReadOnlyList<LavalinkTrack>> DecodeTracksAsync(IEnumerable<string> tracks)
		=> await this.Rest.DecodeTracksAsync(tracks).ConfigureAwait(false);

	/// <summary>
	/// Decodes an encoded <see cref="LavalinkTrack"/>.
	/// <para>Might not work with pre 3.0 tracks.</para>
	/// </summary>
	/// <param name="track">The track to decode.</param>
	/// <returns>The decoded <see cref="LavalinkTrack"/>.</returns>
	public async Task<LavalinkTrack> DecodeTrackAsync(string track)
		=> await this.Rest.DecodeTrackAsync(track).ConfigureAwait(false);

	/// <summary>
	/// Loads tracks by <paramref name="identifier"/>.
	/// Returns a dynamic object you have to parse with (Type)Result.
	/// </summary>
	/// <param name="identifier">The identifier to load.</param>
	/// <returns>A track loading result.</returns>
	public async Task<LavalinkTrackLoadingResult> LoadTracksAsync(string identifier)
		=> await this.Rest.LoadTracksAsync(identifier).ConfigureAwait(false);

	/// <summary>
	/// Loads tracks by <paramref name="identifier"/>.
	/// Returns a dynamic object you have to parse with (Type)Result.
	/// </summary>
	/// <param name="searchType">The search type to use. Some types need additional setup.</param>
	/// <param name="identifier">The identifier to load.</param>
	/// <returns>A track loading result.</returns>
	public async Task<LavalinkTrackLoadingResult> LoadTracksAsync(LavalinkSearchType searchType, string identifier)
	{
		var type = searchType switch
		{
			LavalinkSearchType.Youtube => "ytsearch:",
			LavalinkSearchType.SoundCloud => "scsearch:",
			LavalinkSearchType.AppleMusic => "amsearch:",
			LavalinkSearchType.Deezer => "dzsearch:",
			LavalinkSearchType.DeezerISrc => "dzisrc:",
			LavalinkSearchType.YandexMusic => "ymsearch:",
			LavalinkSearchType.Spotify => "spsearch:",
			LavalinkSearchType.SpotifyRec => "sprec:",
			LavalinkSearchType.Plain => string.Empty,
			_ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "Invalid search type.")
		};
		return await this.LoadTracksAsync($"{type}{identifier}").ConfigureAwait(false);
	}

	/// <summary>
	/// Establishes a connection to the lavalink server.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when the <see cref="DiscordClient"/> is not fully initialized.</exception>
	internal async Task EstablishConnectionAsync()
	{
		if (this.Discord.CurrentUser?.Id == null)
			throw new InvalidOperationException("This operation requires the Discord client to be fully initialized.");

		this._webSocket = this.Discord.Configuration.WebSocketClientFactory(this.Discord.Configuration.Proxy, this.Discord.ServiceProvider);
		this._webSocket.Connected += this.Lavalink_WebSocket_Connected;
		this._webSocket.Disconnected += this.Lavalink_WebSocket_Disconnected;
		this._webSocket.ExceptionThrown += this.Lavalink_WebSocket_ExceptionThrown;
		this._webSocket.MessageReceived += this.Lavalink_WebSocket_MessageReceived;

		this._webSocket.AddDefaultHeader("Authorization", this.Config.Password);
		this._webSocket.AddDefaultHeader("Num-Shards", this.Discord.ShardCount.ToString(CultureInfo.InvariantCulture));
		this._webSocket.AddDefaultHeader("User-Id", this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
		this._webSocket.AddDefaultHeader("Client-Name", $"DisCatSharp.Lavalink/{this.Discord.VersionString}");

		do
		{
			if (this.Config.SessionId != null && !this._webSocket.DefaultHeaders.ContainsKey("Session-Id"))
				this._webSocket.AddDefaultHeader("Session-Id", this.Config.SessionId);
			else if (this.Config.SessionId != null)
			{
				this._webSocket.RemoveDefaultHeader("Session-Id");
				this._webSocket.AddDefaultHeader("Session-Id", this.Config.SessionId);
			}

			try
			{
				if (this._backoff != 0)
				{
					await Task.Delay(this._backoff).ConfigureAwait(false);
					this._backoff = Math.Min(this._backoff * 2, MAXIMUM_BACKOFF);
				}
				else
					this._backoff = MINIMUM_BACKOFF;

				await this._webSocket.ConnectAsync(new($"{this.Config.SocketEndpoint.ToWebSocketString()}{Enums.Endpoints.V4}{Enums.Endpoints.WEBSOCKET}")).ConfigureAwait(false);

				this._sessionIdReceived = new();
				var sessionId = await this._sessionIdReceived.Task.ConfigureAwait(false);
				this.Config.SessionId = sessionId;
				this._sessionIdReceived = null!;
				await this._lavalinkSessionConnected.InvokeAsync(this, new(this)).ConfigureAwait(false);
				break;
			}
			catch (PlatformNotSupportedException)
			{
				throw;
			}
			catch (NotImplementedException)
			{
				throw;
			}
			catch (Exception ex)
			{
				if (!this.Config.SocketAutoReconnect || this._backoff == MAXIMUM_BACKOFF)
				{
					this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, "Failed to connect to Lavalink .-.");
					throw;
				}

				this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, ex, "Failed to connect to Lavalink, retrying in {count} ms.", this._backoff);
			}
		}
		while (this.Config.SocketAutoReconnect);

		Volatile.Write(ref this._isDisposed, false);
	}

	/// <summary>
	/// Handles WebSocket messages from Lavalink.
	/// </summary>
	/// <param name="client">The websocket client.</param>
	/// <param name="args">The event args.</param>
	private Task Lavalink_WebSocket_MessageReceived(IWebSocketClient client, SocketMessageEventArgs args)
	{
		if (args is not SocketTextMessageEventArgs et)
		{
			this.Discord.Logger.LogCritical(LavalinkEvents.LavalinkConnectionError, "Lavalink sent binary data O.o - unable to process");
			return Task.CompletedTask;
		}

		_ = Task.Run(async () =>
		{
			try
			{
				var json = et.Message;
				var jsonData = JObject.Parse(json);
				var op = jsonData["op"]!.ToObject<OpType>();

				switch (op)
				{
					case OpType.Ready:
					{
						this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, null,
							"Received Lavalink Ready OP: {data}", json);
						var ready = LavalinkJson.DeserializeObject<ReadyOp>(json)!;
						if (this._sessionIdReceived != null!)
							this._sessionIdReceived.SetResult(ready.SessionId);
						else
							this.Config.SessionId = ready.SessionId;
						if (ready.Resumed)
							this.Discord.Logger.LogInformation(LavalinkEvents.LavalinkSessionConnected, null,
								"Lavalink session {sessionId} resumed", ready.SessionId);
						break;
					}
					case OpType.PlayerUpdate:
					{
						this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, null,
							"Received Lavalink Player Update OP: {data}", json);
						var playerUpdate = LavalinkJson.DeserializeObject<PlayerUpdateOp>(json)!;
						if (this.ConnectedPlayersInternal.TryGetValue(playerUpdate.GuildId, out var value))
						{
							value.Player.PlayerState = playerUpdate.State;
							await value.StateUpdatedEvent.InvokeAsync(value, new(this.Discord, playerUpdate.State)).ConfigureAwait(false);
						}

						break;
					}
					case OpType.Stats:
						this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, null,
							"Received Lavalink Stats OP: {data}", json);
						var stats = LavalinkJson.DeserializeObject<StatsOp>(json!)!;
						this.Statistics = new(stats);
						await this._statsReceived.InvokeAsync(this, new(this.Discord, this.Statistics)).ConfigureAwait(false);
						break;
					case OpType.Event:
						this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsRx, null,
							"Received Lavalink Event OP: {data}", json);
						var eventOp = LavalinkJson.DeserializeObject<EventOp>(json!)!;

						LavalinkGuildPlayer? player = null;

						if (!string.IsNullOrEmpty(eventOp.GuildId) &&
						    this.ConnectedPlayersInternal.TryGetValue(Convert.ToUInt64(eventOp.GuildId),
							    out var eventPlayer))
							player = eventPlayer;
						switch (eventOp.Type)
						{
							case EventOpType.TrackStartEvent:
								var startEvent = LavalinkJson.DeserializeObject<TrackStartEvent>(json!)!;
								if (player != null)
									await player.TrackStartedEvent.InvokeAsync(player, new(this.Discord, startEvent)).ConfigureAwait(false);
								break;
							case EventOpType.TrackEndEvent:
								var endEvent = LavalinkJson.DeserializeObject<TrackEndEvent>(json!)!;
								if (player != null)
									await player.TrackEndedEvent.InvokeAsync(player, new(this.Discord, endEvent)).ConfigureAwait(false);
								break;
							case EventOpType.TrackStuckEvent:
								var stuckEvent = LavalinkJson.DeserializeObject<TrackStuckEvent>(json!)!;
								if (player != null)
									await player.TrackStuckEvent.InvokeAsync(player, new(this.Discord, stuckEvent)).ConfigureAwait(false);
								break;
							case EventOpType.TrackExceptionEvent:
								var exceptionEvent = LavalinkJson.DeserializeObject<TrackExceptionEvent>(json!)!;
								if (player != null)
									await player.TrackExceptionEvent.InvokeAsync(player,
										new(this.Discord, exceptionEvent)).ConfigureAwait(false);
								break;
							case EventOpType.WebsocketClosedEvent:
								var websocketClosedEvent = LavalinkJson.DeserializeObject<WebSocketClosedEvent>(json!)!;
								await this._websocketClosed.InvokeAsync(this, new(this.Discord, websocketClosedEvent)).ConfigureAwait(false);
								break;
							default:
								var ex = new InvalidDataException("Lavalink send an unknown up");
								this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsException, ex,
									"Wtf QwQ? Received unknown Lavalink Event OP type: {type}", eventOp.Type);
								throw ex;
						}

						break;
					default:
					{
						var ex = new InvalidDataException("Lavalink send an unknown up");
						this.Discord.Logger.LogTrace(LavalinkEvents.LavalinkWsException, ex,
							"Tf O.o? Received unknown Lavalink OP: {data}", json);
						throw ex;
					}
				}
			}
			catch (Exception ex)
			{
				this.Discord.Logger.LogDebug("{message}", ex.Message);
				this.Discord.Logger.LogDebug("{stacktrace}", ex.StackTrace);
			}
		});
		args.Handled = true;
		return Task.CompletedTask;
	}

	/// <summary>
	/// Handles exceptions thrown by the websocket.
	/// </summary>
	/// <param name="client">The websocket client.</param>
	/// <param name="args">The event args.</param>
	private Task Lavalink_WebSocket_ExceptionThrown(IWebSocketClient client, SocketErrorEventArgs args)
		=> this._lavalinkSocketError.InvokeAsync(this, new(client.ServiceProvider)
		{
			Exception = args.Exception
		});

	/// <summary>
	/// Handles the event when the websocket disconnected.
	/// </summary>
	/// <param name="client">The websocket client.</param>
	/// <param name="args">The event args.</param>
	private async Task Lavalink_WebSocket_Disconnected(IWebSocketClient client, SocketCloseEventArgs args)
	{
		if (this.IsConnected && args.CloseCode != 1001 && args.CloseCode != -1)
		{
			this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkSessionConnectionClosed, "Connection broken :/ ({code}, '{message}'), reconnecting", args.CloseCode, args.CloseMessage);
			await this._lavalinkSessionDisconnected.InvokeAsync(this, new(this, false)).ConfigureAwait(false);

			if (this.Config.SocketAutoReconnect)
				_ = Task.Run(this.EstablishConnectionAsync);
		}
		else if (args.CloseCode != 1001 && args.CloseCode != -1)
		{
			this.Discord.Logger.LogInformation(LavalinkEvents.LavalinkSessionConnectionClosed, "Connection closed ({code}, '{message}')", args.CloseCode, args.CloseMessage);
			this.SessionDisconnected?.Invoke(this);
			await this._lavalinkSessionDisconnected.InvokeAsync(this, new(this, true)).ConfigureAwait(false);
		}
		else
		{
			Volatile.Write(ref this._isDisposed, true);
			this.Discord.Logger.LogWarning(LavalinkEvents.LavalinkSessionConnectionClosed, "Lavalink died QwQ");
			foreach (var kvp in this.ConnectedPlayersInternal)
			{
				await kvp.Value.DisconnectVoiceAsync().ConfigureAwait(false);
				_ = Task.Run(async () => await this.GuildPlayerDestroyedEvent.InvokeAsync(this, new(kvp.Value)).ConfigureAwait(false));
				_ = this.ConnectedPlayersInternal.TryRemove(kvp.Key, out _);
			}

			this.SessionDisconnected?.Invoke(this);
			await this._lavalinkSessionDisconnected.InvokeAsync(this, new(this, false)).ConfigureAwait(false);

			if (this.Config.SocketAutoReconnect)
				await this.EstablishConnectionAsync().ConfigureAwait(false);
		}

		args.Handled = true;
	}

	/// <summary>
	/// Handles the event when the websocket connected.
	/// </summary>
	/// <param name="client">The websocket client.</param>
	/// <param name="args">The event args.</param>
	private Task Lavalink_WebSocket_Connected(IWebSocketClient client, SocketEventArgs args)
	{
		this.Discord.Logger.LogDebug(LavalinkEvents.LavalinkSessionConnected, "Connection to Lavalink established UwU");
		this._backoff = 0;
		args.Handled = true;
		return Task.CompletedTask;
	}

	/// <summary>
	/// Handles when discord fires a voice state update.
	/// </summary>
	/// <param name="client">The websocket client.</param>
	/// <param name="args">The event args.</param>
	private Task Discord_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs args)
	{
		var gld = args.Guild;
		if (gld == null!)
			return Task.CompletedTask;

		if (args.User == null!)
			return Task.CompletedTask;

		if (args.User.Id != this.Discord.CurrentUser.Id)
			return Task.CompletedTask;

		if (args.After.Channel == null! && this.IsConnected && this.ConnectedPlayersInternal.TryGetValue(gld.Id, out var dGuildPlayer))
			_ = Task.Run(async () =>
			{
				await Task.Delay(this.Config.WebSocketCloseTimeout).ConfigureAwait(false);
				await this.Rest.DestroyPlayerAsync(this.Config.SessionId!, dGuildPlayer.GuildId).ConfigureAwait(false);
				_ = Task.Run(async () =>
				{
					if (this.ConnectedPlayersInternal.TryRemove(gld.Id, out _))
						await this.GuildPlayerDestroyedEvent.InvokeAsync(this, new(dGuildPlayer)).ConfigureAwait(false);
				});
			});
		else if (!string.IsNullOrWhiteSpace(args.SessionId) && this.ConnectedPlayersInternal.TryGetValue(gld.Id, out var guildPlayer))
			_ = Task.Run(async () =>
			{
				var state = new LavalinkVoiceState()
				{
					Endpoint = guildPlayer.Player.VoiceState.Endpoint,
					Token = guildPlayer.Player.VoiceState.Token,
					SessionId = args.After.SessionId
				};
				guildPlayer.UpdateVoiceState(state);
				await this.Rest.UpdatePlayerVoiceStateAsync(this.Config.SessionId!, guildPlayer.GuildId, state).ConfigureAwait(false);
				this.ConnectedPlayersInternal[gld.Id].ChannelId = args.After?.ChannelId ?? guildPlayer.ChannelId;
			});

		if (!string.IsNullOrWhiteSpace(args.SessionId) && args.Channel != null! && this._voiceStateUpdates.TryRemove(gld.Id, out var xe))
			xe.SetResult(args);

		return Task.CompletedTask;
	}

	/// <summary>
	/// Handles when discord fires a voice server update.
	/// </summary>
	/// <param name="client">The websocket client.</param>
	/// <param name="args">The event args.</param>
	private Task Discord_VoiceServerUpdated(DiscordClient client, VoiceServerUpdateEventArgs args)
	{
		var gld = args.Guild;
		if (gld == null!)
			return Task.CompletedTask;

		if (this.ConnectedPlayersInternal.TryGetValue(args.Guild.Id, out var guildPlayer))
			_ = Task.Run(async () =>
			{
				var state = new LavalinkVoiceState()
				{
					Endpoint = args.Endpoint,
					Token = args.VoiceToken,
					SessionId = guildPlayer.Player.VoiceState.SessionId
				};
				await this.Rest.UpdatePlayerVoiceStateAsync(this.Config.SessionId!, guildPlayer.GuildId, state).ConfigureAwait(false);
				guildPlayer.UpdateVoiceState(state);
			});

		if (this._voiceServerUpdates.TryRemove(gld.Id, out var xe))
			xe.SetResult(args);

		return Task.CompletedTask;
	}
}
