using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;
using DisCatSharp.Net.WebSocket;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Sentry;

namespace DisCatSharp;

/// <summary>
/// Represents a discord websocket client.
/// </summary>
public sealed partial class DiscordClient
{
#region Private Fields

	/// <summary>
	/// Gets the heartbeat interval.
	/// </summary>
	private int _heartbeatInterval;

	/// <summary>
	/// Gets the datetime offset of the last heartbeat.
	/// </summary>
	private DateTimeOffset _lastHeartbeat;

	/// <summary>
	/// Gets the heartbeat task.
	/// Currently unused.
	/// </summary>
	private Task? _heartbeatTask;

	/// <summary>
	/// Gets the default discord epoch for its snowflakes.
	/// </summary>
	internal static DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

	/// <summary>
	/// Gets the count of skipped heartbeats.
	/// </summary>
	private int _skippedHeartbeats;

	/// <summary>
	/// Gets the last sequence number.
	/// </summary>
	private long _lastSequence;

	/// <summary>
	/// Gets the websocket client.
	/// </summary>
	internal IWebSocketClient? WebSocketClient;

	/// <summary>
	/// Gets the payload decompressor.
	/// </summary>
	private PayloadDecompressor? _payloadDecompressor;

	/// <summary>
	/// Gets the cancellation token source.
	/// </summary>
	private CancellationTokenSource _cancelTokenSource;

	/// <summary>
	/// Gets the cancellation token.
	/// </summary>
	private CancellationToken _cancelToken;

#endregion

#region Connection Semaphore

	/// <summary>
	/// Gets the socket locks.
	/// </summary>
	private static ConcurrentDictionary<ulong, SocketLock> s_socketLocks { get; } = new();

	/// <summary>
	/// Gets the session lock.
	/// </summary>
	private readonly ManualResetEventSlim _sessionLock = new(true);

#endregion

#region Internal Connection Methods

	/// <summary>
	/// Reconnects the websocket client.
	/// </summary>
	/// <param name="startNewSession">Whether to start a new session.</param>
	/// <param name="code">The reconnect code.</param>
	/// <param name="message">The reconnect message.</param>
	private Task InternalReconnectAsync(bool startNewSession = false, int code = 1000, string message = "")
	{
		if (startNewSession)
			this._sessionId = null;

		_ = this.WebSocketClient?.DisconnectAsync(code, message);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Connects the websocket client.
	/// </summary>
	internal async Task InternalConnectAsync()
	{
		SocketLock socketLock = null;
		try
		{
			if (this.GatewayInfo == null)
				await this.InternalUpdateGatewayAsync().ConfigureAwait(false);
			await this.InitializeAsync().ConfigureAwait(false);

			socketLock = this.GetSocketLock();
			await socketLock.LockAsync().ConfigureAwait(false);
		}
		catch
		{
			socketLock?.UnlockAfter(TimeSpan.Zero);
			throw;
		}

		if (!this.Presences.ContainsKey(this.CurrentUser!.Id))
			this.PresencesInternal[this.CurrentUser.Id] = new()
			{
				Discord = this,
				Status = UserStatus.Online,
				InternalUser = new()
				{
					Id = this.CurrentUser.Id
				}
			};
		else
		{
			var pr = this.PresencesInternal[this.CurrentUser.Id];
			pr.RawActivity = null;
			pr.Activity = null;
			pr.Status = UserStatus.Online;
		}

		Volatile.Write(ref this._skippedHeartbeats, 0);

		this.WebSocketClient =
			this.Configuration.WebSocketClientFactory(this.Configuration.Proxy, this.ServiceProvider);
		this._payloadDecompressor = this.Configuration.GatewayCompressionLevel != GatewayCompressionLevel.None
			? new PayloadDecompressor(this.Configuration.GatewayCompressionLevel)
			: null;

		this._cancelTokenSource = new();
		this._cancelToken = this._cancelTokenSource.Token;

		this.WebSocketClient.Connected += SocketOnConnect;
		this.WebSocketClient.Disconnected += SocketOnDisconnect;
		this.WebSocketClient!.MessageReceived += SocketOnMessage;
		this.WebSocketClient!.ExceptionThrown += SocketOnException;

		var gwuri = new QueryUriBuilder(this.GatewayUri)
			.AddParameter("v", this.Configuration.ApiVersion)
			.AddParameter("encoding", "json");

		if (this.Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Stream)
			gwuri.AddParameter("compress", "zlib-stream");

		this.Logger.LogDebug(LoggerEvents.Startup, "Connecting to {gw}", this.GatewayUri.AbsoluteUri);

		await this.WebSocketClient.ConnectAsync(gwuri.Build()).ConfigureAwait(false);
		return;

		Task SocketOnConnect(IWebSocketClient sender, SocketEventArgs e)
			=> this._socketOpened.InvokeAsync(this, e);

		async Task SocketOnMessage(IWebSocketClient sender, SocketMessageEventArgs e)
		{
			string msg = null!;
			switch (e)
			{
				case SocketTextMessageEventArgs socketTextMessage:
					msg = socketTextMessage.Message;
					break;
				case SocketBinaryMessageEventArgs socketBinaryMessage:
				{
					using var ms = new MemoryStream();
					if (!this._payloadDecompressor!.TryDecompress(new(socketBinaryMessage.Message), ms))
					{
						this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, "Payload decompression failed");
						return;
					}

					ms.Position = 0;
					using var sr = new StreamReader(ms, Utilities.UTF8);
					msg = await sr.ReadToEndAsync().ConfigureAwait(false);
					break;
				}
			}

			try
			{
				this.Logger.LogTrace(LoggerEvents.GatewayWsRx, "{message}", msg);
				await this.HandleSocketMessageAsync(msg).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, ex,
					"Socket handler suppressed an exception");
				if (this.Configuration.EnableSentry)
					this.Sentry.CaptureException(ex);
			}
		}

		Task SocketOnException(IWebSocketClient sender, SocketErrorEventArgs e)
			=> this._socketErrored.InvokeAsync(this, e);

		async Task SocketOnDisconnect(IWebSocketClient sender, SocketCloseEventArgs e)
		{
			// release session and connection
			this._connectionLock.Set();
			this._sessionLock.Set();

			if (!this._disposed)
				this._cancelTokenSource.Cancel();

			this.Logger.LogDebug(LoggerEvents.ConnectionClose, "Connection closed ({code}, '{message}')", e.CloseCode,
				e.CloseMessage);
			await this._socketClosed.InvokeAsync(this, e).ConfigureAwait(false);

			// TODO: We might need to include more 400X codes
			if (this.Configuration.AutoReconnect && e.CloseCode is < 4001 or >= 5000)
			{
				this.Logger.LogCritical(LoggerEvents.ConnectionClose,
					"Connection terminated ({code}, '{message}'), reconnecting", e.CloseCode,
					e.CloseMessage);

				if (this._status == null)
					await this.ConnectAsync().ConfigureAwait(false);
				else if (this._status.IdleSince.HasValue)
					await this.ConnectAsync(this._status.ActivityInternal, this._status.Status,
							Utilities.GetDateTimeOffsetFromMilliseconds(this._status.IdleSince.Value))
						.ConfigureAwait(false);
				else
					await this.ConnectAsync(this._status.ActivityInternal, this._status.Status).ConfigureAwait(false);
			}
			else
				this.Logger.LogCritical(LoggerEvents.ConnectionClose, "Connection terminated ({code}, '{message}')",
					e.CloseCode, e.CloseMessage);
		}
	}

#endregion

#region WebSocket (Events)

	/// <summary>
	/// Handles the socket message.
	/// </summary>
	/// <param name="data">The data.</param>
	internal async Task HandleSocketMessageAsync(string? data)
	{
		if (data == null)
			return;

		var payload = DiscordJson.DeserializeObject<GatewayPayload>(data, this);
		this._lastSequence = payload.Sequence ?? this._lastSequence;
		switch (payload.OpCode)
		{
			case GatewayOpCode.Dispatch:
				await Task.Run(() => this.HandleDispatchAsync(payload), this._cancelToken).ConfigureAwait(false);
				break;

			case GatewayOpCode.Heartbeat:
				await this.OnHeartbeatAsync((long)payload.Data).ConfigureAwait(false);
				break;

			case GatewayOpCode.Reconnect:
				await this.OnReconnectAsync().ConfigureAwait(false);
				break;

			case GatewayOpCode.InvalidSession:
				await this.OnInvalidateSessionAsync((bool)payload.Data).ConfigureAwait(false);
				break;

			case GatewayOpCode.Hello:
				await this.OnHelloAsync((payload.Data as JObject)!.ToObject<GatewayHello>()).ConfigureAwait(false);
				break;

			case GatewayOpCode.HeartbeatAck:
				await this.OnHeartbeatAckAsync().ConfigureAwait(false);
				break;
			case GatewayOpCode.Identify:
				break;
			case GatewayOpCode.StatusUpdate:
				break;
			case GatewayOpCode.VoiceStateUpdate:
				break;
			case GatewayOpCode.VoiceServerPing:
				break;
			case GatewayOpCode.Resume:
				break;
			case GatewayOpCode.RequestGuildMembers:
				break;
			case GatewayOpCode.GuildSync:
				break;
			default:
				this.Logger.LogWarning(LoggerEvents.WebSocketReceive,
					"Unknown Discord opcode: {op}\nPayload: {payload}", payload.OpCode,
					payload.Data);
				break;
		}
	}

	/// <summary>
	/// Handles the heartbeat.
	/// </summary>
	/// <param name="seq">The sequence.</param>
	internal Task OnHeartbeatAsync(long seq)
	{
		this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT (OP1)");
		return this.SendHeartbeatAsync(seq);
	}

	/// <summary>
	/// Handles the reconnect event.
	/// </summary>
	internal Task OnReconnectAsync()
	{
		this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received RECONNECT (OP7)");
		return this.InternalReconnectAsync(code: 4000, message: "OP7 acknowledged");
	}

	/// <summary>
	/// Handles the invalidate session event
	/// </summary>
	/// <param name="data">Unknown. Please fill documentation.</param>
	internal async Task OnInvalidateSessionAsync(bool data)
	{
		// begin a session if one is not open already
		if (this._sessionLock.Wait(0))
			this._sessionLock.Reset();

		// we are sending a fresh resume/identify, so lock the socket
		var socketLock = this.GetSocketLock();
		await socketLock.LockAsync().ConfigureAwait(false);
		socketLock.UnlockAfter(TimeSpan.FromSeconds(5));

		if (data)
		{
			this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, true)");
			await Task.Delay(6000, this._cancelToken).ConfigureAwait(false);
			await this.SendResumeAsync().ConfigureAwait(false);
		}
		else
		{
			this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received INVALID_SESSION (OP9, false)");
			this._sessionId = null;
			await this.SendIdentifyAsync(this._status).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Handles the hello event.
	/// </summary>
	/// <param name="hello">The gateway hello payload.</param>
	internal async Task OnHelloAsync(GatewayHello hello)
	{
		this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HELLO (OP10)");

		if (this._sessionLock.Wait(0))
		{
			this._sessionLock.Reset();
			this.GetSocketLock().UnlockAfter(TimeSpan.FromSeconds(5));
		}
		else
		{
			this.Logger.LogWarning(LoggerEvents.SessionUpdate,
				"Attempt to start a session while another session is active");
			return;
		}

		Interlocked.CompareExchange(ref this._skippedHeartbeats, 0, 0);
		this._heartbeatInterval = hello.HeartbeatInterval;
		this._heartbeatTask = Task.Run(this.HeartbeatLoopAsync, this._cancelToken);

		if (string.IsNullOrEmpty(this._sessionId))
			await this.SendIdentifyAsync(this._status).ConfigureAwait(false);
		else
			await this.SendResumeAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the heartbeat acknowledge event.
	/// </summary>
	internal Task OnHeartbeatAckAsync()
	{
		Interlocked.Decrement(ref this._skippedHeartbeats);

		var ping = (int)(DateTime.Now - this._lastHeartbeat).TotalMilliseconds;

		this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT_ACK (OP11, {ping}ms)", ping);

		Volatile.Write(ref this._ping, ping);

		var args = new HeartbeatEventArgs(this.ServiceProvider)
		{
			Ping = this.Ping,
			Timestamp = DateTimeOffset.Now
		};

		return this._heartbeated.InvokeAsync(this, args);
	}

	/// <summary>
	/// Handles the heartbeat loop.
	/// </summary>
	internal async Task HeartbeatLoopAsync()
	{
		this.Logger.LogDebug(LoggerEvents.Heartbeat, "Heartbeat task started");
		var token = this._cancelToken;
		try
		{
			while (true)
			{
				await this.SendHeartbeatAsync(this._lastSequence).ConfigureAwait(false);
				await Task.Delay(this._heartbeatInterval, token).ConfigureAwait(false);
				token.ThrowIfCancellationRequested();
			}
		}
		catch (OperationCanceledException)
		{ }
	}

#endregion

#region Internal Gateway Methods

	/// <summary>
	/// Updates the status.
	/// </summary>
	/// <param name="activity">The activity.</param>
	/// <param name="userStatus">The optional user status.</param>
	/// <param name="idleSince">Since when is the client performing the specified activity.</param>
	internal async Task InternalUpdateStatusAsync(
		DiscordActivity? activity, UserStatus? userStatus,
		DateTimeOffset? idleSince
	)
	{
		if (activity is { Name.Length: > 128 })
			throw new("Game name can't be longer than 128 characters!");

		var sinceUnix = idleSince != null ? (long?)Utilities.GetUnixTime(idleSince.Value) : null;
		var act = activity ?? new DiscordActivity();

		var status = new StatusUpdate
		{
			Activity = new(act),
			IdleSince = sinceUnix,
			IsAfk = idleSince != null,
			Status = userStatus ?? UserStatus.Online
		};

		// Solution to have status persist between sessions
		this._status = status;
		var statusUpdate = new GatewayPayload
		{
			OpCode = GatewayOpCode.StatusUpdate,
			Data = status
		};

		var statusString = JsonConvert.SerializeObject(statusUpdate);

		await this.WsSendAsync(statusString).ConfigureAwait(false);

		if (!this.PresencesInternal.ContainsKey(this.CurrentUser!.Id))
			this.PresencesInternal[this.CurrentUser.Id] = new()
			{
				Discord = this,
				Activity = act,
				Status = userStatus ?? UserStatus.Online,
				InternalUser = new()
				{
					Id = this.CurrentUser.Id
				}
			};
		else
		{
			var pr = this.PresencesInternal[this.CurrentUser.Id];
			pr.Activity = act;
			pr.Status = userStatus ?? pr.Status;
		}
	}

	/// <summary>
	/// Sends the heartbeat.
	/// </summary>
	/// <param name="seq">The sequence.</param>
	internal async Task SendHeartbeatAsync(long seq)
	{
		var moreThan5 = Volatile.Read(ref this._skippedHeartbeats) > 5;
		var guildsComp = Volatile.Read(ref this._guildDownloadCompleted);
		switch (guildsComp)
		{
			case true when moreThan5:
			{
				this.Logger.LogCritical(LoggerEvents.HeartbeatFailure,
					"Server failed to acknowledge more than 5 heartbeats - connection is zombie");

				var args = new ZombiedEventArgs(this.ServiceProvider)
				{
					Failures = Volatile.Read(ref this._skippedHeartbeats),
					GuildDownloadCompleted = true
				};
				await this._zombied.InvokeAsync(this, args).ConfigureAwait(false);

				await this.InternalReconnectAsync(code: 4001, message: "Too many heartbeats missed")
					.ConfigureAwait(false);
				return;
			}
			case false when moreThan5:
			{
				var args = new ZombiedEventArgs(this.ServiceProvider)
				{
					Failures = Volatile.Read(ref this._skippedHeartbeats),
					GuildDownloadCompleted = false
				};
				await this._zombied.InvokeAsync(this, args).ConfigureAwait(false);

				this.Logger.LogWarning(LoggerEvents.HeartbeatFailure,
					"Server failed to acknowledge more than 5 heartbeats, but the guild download is still running - check your connection speed");
				break;
			}
		}

		Volatile.Write(ref this._lastSequence, seq);
		this.Logger.LogTrace(LoggerEvents.Heartbeat, "Sending heartbeat");
		var heartbeat = new GatewayPayload
		{
			OpCode = GatewayOpCode.Heartbeat,
			Data = seq
		};
		var heartbeatStr = JsonConvert.SerializeObject(heartbeat);
		await this.WsSendAsync(heartbeatStr).ConfigureAwait(false);

		this._lastHeartbeat = DateTimeOffset.Now;

		Interlocked.Increment(ref this._skippedHeartbeats);
	}

	/// <summary>
	/// Sends the identify payload.
	/// </summary>
	/// <param name="status">The status update payload.</param>
	internal async Task SendIdentifyAsync(StatusUpdate? status)
	{
		var identify = new GatewayIdentify
		{
			Token = Utilities.GetFormattedToken(this),
			Compress = this.Configuration.GatewayCompressionLevel == GatewayCompressionLevel.Payload,
			LargeThreshold = this.Configuration.LargeThreshold,
			ShardInfo = new()
			{
				ShardId = this.Configuration.ShardId,
				ShardCount = this.Configuration.ShardCount
			},
			Presence = status,
			Intents = this.Configuration.Intents,
			Discord = this
		};
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Identify,
			Data = identify
		};
		var payloadstr = JsonConvert.SerializeObject(payload);
		await this.WsSendAsync(payloadstr).ConfigureAwait(false);

		this.Logger.LogDebug(LoggerEvents.Intents, "Registered gateway intents ({intents})",
			this.Configuration.Intents);
	}

	/// <summary>
	/// Sends the resume payload.
	/// </summary>
	internal Task SendResumeAsync()
	{
		if (this._sessionId is null)
			throw new NullReferenceException("Session ID was null, cannot resume!");
		if (this._resumeGatewayUrl is null)
			throw new NullReferenceException("Resume gateway url was null, cannot resume!");

		var resume = new GatewayResume
		{
			Token = Utilities.GetFormattedToken(this),
			SessionId = this._sessionId,
			SequenceNumber = Volatile.Read(ref this._lastSequence)
		};
		var resumePayload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Resume,
			Data = resume
		};
		var resumeString = JsonConvert.SerializeObject(resumePayload);
		this.GatewayUri = new(this._resumeGatewayUrl);
		this.Logger.LogDebug(LoggerEvents.ConnectionClose, "Request to resume via {gw}", this.GatewayUri.AbsoluteUri);
		return this.WsSendAsync(resumeString);
	}

	/// <summary>
	/// Internals the update gateway async.
	/// </summary>
	/// <returns>A Task.</returns>
	internal async Task InternalUpdateGatewayAsync()
	{
		var info = await this.GetGatewayInfoAsync().ConfigureAwait(false);
		this.GatewayInfo = info;
		this.GatewayUri = new(info.Url);
	}

	/// <summary>
	/// Sends a websocket message.
	/// </summary>
	/// <param name="payload">The payload to send.</param>
	internal Task WsSendAsync(string payload)
	{
		this.Logger.LogTrace(LoggerEvents.GatewayWsTx, "{pld}", payload);
		return this.WebSocketClient!.SendMessageAsync(payload);
	}

#endregion

#region Semaphore Methods

	/// <summary>
	/// Gets the socket lock.
	/// </summary>
	/// <returns>The added socket lock.</returns>
	private SocketLock GetSocketLock()
		=> s_socketLocks.GetOrAdd(this.CurrentApplication!.Id,
			appId => new(appId, this.GatewayInfo!.SessionBucket.MaxConcurrency));

#endregion
}
