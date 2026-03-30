using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.WebSocket;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp;

/// <summary>
///     Represents a discord websocket client.
/// </summary>
public sealed partial class DiscordClient
{
	#region Semaphore Methods

	/// <summary>
	///     Gets the socket lock.
	/// </summary>
	/// <returns>The added socket lock.</returns>
	private SocketLock GetSocketLock()
	{
		var appId = this.CurrentApplication?.Id ?? 0ul;
		var maxConcurrency = this.GatewayInfo?.SessionBucket.MaxConcurrency ?? 1;
		return s_socketLocks.GetOrAdd(appId, new SocketLock(appId, maxConcurrency));
	}

	#endregion

	#region Private Fields

	/// <summary>
	///     Gets the heartbeat interval.
	/// </summary>
	private int _heartbeatInterval;

	/// <summary>
	///     Gets when the last heartbeat was sent.
	/// </summary>
	private DateTimeOffset _lastHeartbeat;

	/// <summary>
	///     Gets whether we already identified
	/// </summary>
	private bool _identified = false;

	/// <summary>
	///     Gets the heartbeat task.
	/// </summary>
	private Task _heartbeatTask;

	/// <summary>
	///     Gets the default discord epoch.
	/// </summary>
	internal static DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

	/// <summary>
	///     Gets the count of skipped heartbeats.
	/// </summary>
	private int _skippedHeartbeats = 0;

	/// <summary>
	///     Gets the last sequence number.
	/// </summary>
	private long _lastSequence = 0;

	/// <summary>
	///     Gets the websocket client.
	/// </summary>
	internal IWebSocketClient WebSocketClient;

	/// <summary>
	///     Gets the payload decompressor.
	/// </summary>
	private PayloadDecompressor? _payloadDecompressor;

	/// <summary>
	///     Gets the cancel token source.
	/// </summary>
	private CancellationTokenSource _cancelTokenSource;

	/// <summary>
	///     Gets the cancel token.
	/// </summary>
	private CancellationToken _cancelToken;

	/// <summary>
	///     Gets the ordered dispatch queue for this shard/client.
	///     All dispatch payloads are written here and processed sequentially by <see cref="_dispatchConsumerTask" />.
	/// </summary>
	private Channel<GatewayPayload>? _dispatchQueue;

	/// <summary>
	///     Gets the background task that consumes payloads from <see cref="_dispatchQueue" />.
	/// </summary>
	private Task? _dispatchConsumerTask;

	#endregion

	#region Connection Semaphore

	/// <summary>
	///     Gets the socket locks.
	/// </summary>
	private static ConcurrentDictionary<ulong, SocketLock> s_socketLocks { get; } = [];

	/// <summary>
	///     Gets the session lock.
	/// </summary>
	private readonly ManualResetEventSlim _sessionLock = new(true);

	/// <summary>
	///     Guards concurrent reads and writes of <see cref="_sessionId" /> during the reconnect handshake.
	///     Specifically protects the TOCTOU window in <see cref="OnHelloAsync" /> (check + branch) against
	///     the clear performed by <see cref="InternalReconnectAsync" /> on a different thread.
	/// </summary>
	private readonly SemaphoreSlim _sessionStateLock = new(1, 1);

	#endregion

	#region Internal Connection Methods

	/// <summary>
	///     Reconnects the websocket client.
	/// </summary>
	/// <param name="startNewSession">Whether to start a new session.</param>
	/// <param name="code">The reconnect code.</param>
	/// <param name="message">The reconnect message.</param>
	private async Task InternalReconnectAsync(bool startNewSession = false, int code = 1000, string message = "")
	{
		if (startNewSession)
		{
			await this._sessionStateLock.WaitAsync().ConfigureAwait(false);
			try
			{
				this._sessionId = null;
			}
			finally
			{
				this._sessionStateLock.Release();
			}
		}

		_ = this.WebSocketClient.DisconnectAsync(code, message);
	}

	/// <summary>
	///     Connects the websocket client.
	/// </summary>
	internal async Task InternalConnectAsync()
	{
		SocketLock? socketLock = null;
		try
		{
			if (this.GatewayInfo is null)
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

		if (!this.Presences.ContainsKey(this.CurrentUser.Id))
			this.CacheAggregatePresence(new()
			{
				Discord = this,
				RawActivity = new(),
				Activity = new(),
				Status = UserStatus.Online,
				InternalUser = new()
				{
					Id = this.CurrentUser.Id
				}
			});
		else
		{
			var pr = this.PresencesInternal[this.CurrentUser.Id];
			pr.RawActivity = new();
			pr.Activity = new();
			pr.Status = UserStatus.Online;
			this.CacheAggregatePresence(pr);
		}

		Volatile.Write(ref this._skippedHeartbeats, 0);

		this.WebSocketClient = this.Configuration.Gateway.WebSocketClientFactory(this.Configuration.Rest.Proxy, this.ServiceProvider);
		this.WebSocketClient.AddDefaultHeader(CommonHeaders.USER_AGENT, Utilities.GetUserAgent());
		this._payloadDecompressor = this.Configuration.Gateway.CompressionLevel is not GatewayCompressionLevel.None
			? new PayloadDecompressor(this.Configuration.Gateway.CompressionLevel)
			: null;

		this._cancelTokenSource = new();
		this._cancelToken = this._cancelTokenSource.Token;

		// Initialize the ordered dispatch queue.
		var capacity = this.Configuration.Gateway.Advanced.DispatchQueueCapacity;
		this._dispatchQueue = capacity > 0
			? Channel.CreateBounded<GatewayPayload>(new BoundedChannelOptions(capacity)
			{
				SingleReader = true,
				SingleWriter = true,
				FullMode = BoundedChannelFullMode.Wait
			})
			: Channel.CreateUnbounded<GatewayPayload>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = true
			});

		this._dispatchConsumerTask = Task.Run(() => this.DispatchConsumerLoopAsync(this._cancelToken), this._cancelToken);

		this.WebSocketClient.Connected += SocketOnConnect;
		this.WebSocketClient.Disconnected += SocketOnDisconnect;
		this.WebSocketClient.MessageReceived += SocketOnMessage;
		this.WebSocketClient.ExceptionThrown += SocketOnException;

		var gwuri = this.GatewayUri.AddParameter("v", this.Configuration.Api.Version).AddParameter("encoding", "json");

		if (this.Configuration.Gateway.CompressionLevel == GatewayCompressionLevel.Stream)
			gwuri = gwuri.AddParameter("compress", "zlib-stream");

		this.GatewayUri = gwuri;

		this.Logger.LogDebug(LoggerEvents.Startup, "Connecting to {gw}", this.GatewayUri.AbsoluteUri);

		this._identified = false;
		await this.WebSocketClient.ConnectAsync(this.GatewayUri).ConfigureAwait(false);
		return;

		Task SocketOnConnect(IWebSocketClient sender, SocketEventArgs e)
			=> this._socketOpened.InvokeAsync(this, e);

		async Task SocketOnMessage(IWebSocketClient sender, SocketMessageEventArgs e)
		{
			string? msg = null;
			switch (e)
			{
				case SocketTextMessageEventArgs etext:
					msg = etext.Message;
					break;
				case SocketBinaryMessageEventArgs ebin:
				{
					using var ms = new MemoryStream();
					if (this._payloadDecompressor?.TryDecompress(new(ebin.Message), ms) is false or null)
					{
						this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, "Payload decompression failed");
						return;
					}

					ms.Position = 0;
					using var sr = new StreamReader(ms, Utilities.UTF8);
					msg = await sr.ReadToEndAsync(this._cancelToken).ConfigureAwait(false);
					break;
				}
			}

			try
			{
				if (msg is not null)
				{
					this.Logger.LogTrace(LoggerEvents.GatewayWsRx, "{Message}", msg);
					await this.HandleSocketMessageAsync(msg).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, ex, "Socket handler suppressed an exception");
				if (this.DiagnosticsSink.IsEnabled)
					this.DiagnosticsSink.CaptureException("DisCatSharp", ex);
			}
		}

		Task SocketOnException(IWebSocketClient sender, SocketErrorEventArgs e)
			=> this._socketErrored.InvokeAsync(this, e);

		async Task SocketOnDisconnect(IWebSocketClient sender, SocketCloseEventArgs e)
		{
			var shouldReconnect = e.CloseCode is 4000 or 4001 or 4002 or 4003 or 4005 or 4007 or 4008 or 4009 or >= 5000;
			var shouldResume = e.CloseCode is 4000 or 4002 or 4008 or -1;
			var fatalError = e.CloseCode is 4004 or 4010 or 4011 or 4012 or 4013 or 4014;

			if (this.DiagnosticsSink.IsEnabled)
				this.DiagnosticsSink.AddBreadcrumb("DisCatSharp", "gateway", $"WebSocket disconnected (code: {e.CloseCode})", fatalError ? Telemetry.DiagnosticSeverity.Error : Telemetry.DiagnosticSeverity.Warning, new Dictionary<string, string>
				{
					["close_code"] = e.CloseCode.ToString(),
					["fatal"] = fatalError.ToString().ToLowerInvariant(),
					["reconnect"] = shouldReconnect.ToString().ToLowerInvariant()
				});

			this._connectionLock.Set();
			this._sessionLock.Set();

			if (!this._disposed)
				await this._cancelTokenSource.CancelAsync();

			this.Logger.LogDebug(LoggerEvents.ConnectionClose, "Connection closed ({CloseCode}, '{Reason}')", e.CloseCode, e.CloseMessage ?? "No reason given");

			if (this.DiagnosticsSink.IsEnabled)
			{
				this.DiagnosticsSink.EndSession();
				this.DiagnosticsSink.CaptureReport(new()
				{
					Source = "DisCatSharp",
					Severity = fatalError
						? Telemetry.DiagnosticSeverity.Fatal
						: Telemetry.DiagnosticSeverity.Warning,
					Logger = "DiscordClient.WebSocket",
					Message = $"Gateway disconnected (code: {e.CloseCode})",
					Tags = new Dictionary<string, string>
					{
						["dcs.gateway_close_code"] = e.CloseCode.ToString(),
						["dcs.gateway_fatal"] = fatalError.ToString().ToLowerInvariant(),
						["dcs.gateway_reconnect"] = shouldReconnect.ToString().ToLowerInvariant()
					}
				});
			}

			await this._socketClosed.InvokeAsync(this, e).ConfigureAwait(false);

			if (fatalError)
			{
				this.Logger.LogCritical(LoggerEvents.ConnectionClose, "Fatal close code received ({CloseCode}, '{Reason}'). No reconnection attempt.", e.CloseCode, e.CloseMessage ?? "No reason given");
				return;
			}

			if (shouldReconnect && !shouldResume)
			{
				this.Logger.LogWarning(LoggerEvents.ConnectionClose, "Session is invalid. Clearing session ID before reconnecting.");
				this._sessionId = null;
			}

			if (this.Configuration.Gateway.AutoReconnect && shouldReconnect)
			{
				this.Logger.LogCritical(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{Reason}'), reconnecting", e.CloseCode, e.CloseMessage ?? "No reason given");
				this._identified = false;

				if (shouldResume && this._sessionId is not null)
					this.Logger.LogInformation(LoggerEvents.ConnectionClose, "Attempting to resume session with ID {SessionId}.", this._sessionId);
				else
					this.Logger.LogWarning(LoggerEvents.ConnectionClose, "No valid session to resume, starting a new connection.");

				await this.ConnectAsync(this._status?.ActivitiesInternal?.FirstOrDefault(), this._status?.Status, this._status?.IdleSince is not null ? Utilities.GetDateTimeOffsetFromMilliseconds(this._status.IdleSince.Value) : null).ConfigureAwait(false);
			}
			else if (this.Configuration.Gateway.AutoReconnect)
			{
				this._sessionId = null;
				this._identified = false;
				this.Logger.LogCritical(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{Reason}')", e.CloseCode, e.CloseMessage ?? "No reason given");
				await this.ConnectAsync(this._status?.ActivitiesInternal?.FirstOrDefault(), this._status?.Status, this._status?.IdleSince is not null ? Utilities.GetDateTimeOffsetFromMilliseconds(this._status.IdleSince.Value) : null).ConfigureAwait(false);
			}
			else
				this.Logger.LogCritical(LoggerEvents.ConnectionClose, "Connection terminated ({CloseCode}, '{Reason}')", e.CloseCode, e.CloseMessage ?? "No reason given");
		}
	}

	#endregion

	#region WebSocket (Events)

	/// <summary>
	///     Handles the socket message.
	/// </summary>
	/// <param name="data">The data.</param>
	internal async Task HandleSocketMessageAsync(string data)
	{
		var payload = JsonConvert.DeserializeObject<GatewayPayload>(data)!;
		this._lastSequence = payload.Sequence ?? this._lastSequence;

		if (this.DiagnosticsSink.IsEnabled)
			this.DiagnosticsSink.AddBreadcrumb("DisCatSharp", "gateway", $"Received opcode {payload.OpCode}", Telemetry.DiagnosticSeverity.Debug, new Dictionary<string, string>
			{
				["opcode"] = payload.OpCode.ToString(),
				["event_name"] = payload.EventName ?? "none",
				["sequence"] = (payload.Sequence ?? -1).ToString()
			});

		switch (payload.OpCode)
		{
			case GatewayOpCode.Dispatch:
				await this._dispatchQueue!.Writer.WriteAsync(payload, this._cancelToken).ConfigureAwait(false);
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
				await this.OnHelloAsync((payload.Data as JObject).ToObject<GatewayHello>()).ConfigureAwait(false);
				break;

			case GatewayOpCode.HeartbeatAck:
				await this.OnHeartbeatAckAsync().ConfigureAwait(false);
				break;

			case GatewayOpCode.Identify:
			case GatewayOpCode.StatusUpdate:
			case GatewayOpCode.VoiceStateUpdate:
			case GatewayOpCode.VoiceServerPing:
			case GatewayOpCode.Resume:
			case GatewayOpCode.RequestGuildMembers:
			case GatewayOpCode.GuildSync:
				this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received op code for non-bot event");
				break;
			default:
				this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown Discord opcode: {OpCode}\nPayload: {Payload}", payload.OpCode, payload.Data);

				if (this.DiagnosticsSink.IsEnabled)
				{
					var scrubbedPayload = Utilities.StripTokensAndOptIdsInJson(data, this.Configuration.Telemetry.EnableDiscordIdScrubber);
					byte[]? filePayload = null;
					string? filePayloadName = null;
					if (scrubbedPayload is not null && scrubbedPayload.Length > 8192)
					{
						filePayload = System.Text.Encoding.UTF8.GetBytes(scrubbedPayload);
						filePayloadName = $"unknown-opcode-{(int)payload.OpCode}.json";
						scrubbedPayload = scrubbedPayload[..8192] + "... (truncated, full payload in file)";
					}

					this.DiagnosticsSink.CaptureReport(new()
					{
						Source = "DisCatSharp",
						Severity = Telemetry.DiagnosticSeverity.Warning,
						Logger = "DiscordClient.WebSocket",
						Message = $"Unknown gateway opcode {(int)payload.OpCode}",
						DeduplicateByFingerprint = true,
						Tags = new Dictionary<string, string>
						{
							["dcs.gateway_opcode"] = ((int)payload.OpCode).ToString(),
							["dcs.gateway_event"] = payload.EventName ?? "none"
						},
						Extra = new Dictionary<string, object>
						{
							["opcode_name"] = payload.OpCode.ToString(),
							["sequence"] = payload.Sequence ?? -1,
							["Scrubbed Payload"] = scrubbedPayload ?? "null"
						},
						FilePayload = filePayload,
						FilePayloadName = filePayloadName
					});
				}

				break;
		}
	}

	/// <summary>
	///     Handles the heartbeat.
	/// </summary>
	/// <param name="seq">The sequence.</param>
	internal async Task OnHeartbeatAsync(long seq)
	{
		this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT (OP1)");
		await this.SendHeartbeatAsync(seq).ConfigureAwait(false);
	}

	/// <summary>
	///     Handles the reconnect event.
	/// </summary>
	internal async Task OnReconnectAsync()
	{
		this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received RECONNECT (OP7)");
		await this.InternalReconnectAsync(code: 4000, message: "OP7 acknowledged").ConfigureAwait(false);
	}

	/// <summary>
	///     Handles the invalidate session event
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
	///     Handles the hello event.
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
			this.Logger.LogWarning(LoggerEvents.SessionUpdate, "Attempt to start a session while another session is active");
			return;
		}

		Interlocked.Exchange(ref this._skippedHeartbeats, 0);
		this._heartbeatInterval = hello.HeartbeatInterval;
		if (this._heartbeatTask is { IsCompleted: false })
		{
			try { await this._heartbeatTask.ConfigureAwait(false); } catch { /* already cancelled */ }
		}

		this._heartbeatTask = Task.Run(this.HeartbeatLoopAsync, this._cancelToken);

		await this._sessionStateLock.WaitAsync().ConfigureAwait(false);
		string? capturedSessionId;
		try
		{
			capturedSessionId = this._sessionId;
		}
		finally
		{
			this._sessionStateLock.Release();
		}

		if (string.IsNullOrEmpty(capturedSessionId))
			await this.SendIdentifyAsync(this._status).ConfigureAwait(false);
		else
			await this.SendResumeAsync(capturedSessionId).ConfigureAwait(false);
	}

	/// <summary>
	///     Handles the heartbeat acknowledge event.
	/// </summary>
	internal async Task OnHeartbeatAckAsync()
	{
		Interlocked.Decrement(ref this._skippedHeartbeats);

		var ping = (int)(DateTimeOffset.UtcNow - this._lastHeartbeat).TotalMilliseconds;

		this.Logger.LogTrace(LoggerEvents.WebSocketReceive, "Received HEARTBEAT_ACK (OP11, {0}ms)", ping);

		Volatile.Write(ref this._ping, ping);

		var args = new HeartbeatEventArgs(this.ServiceProvider)
		{
			Ping = this.Ping,
			Timestamp = DateTimeOffset.Now
		};

		await this._heartbeated.InvokeAsync(this, args).ConfigureAwait(false);
	}

	/// <summary>
	///     Handles a gateway dispatch payload and reports suppressed failures through the diagnostics sink.
	/// </summary>
	/// <param name="payload">The dispatch payload.</param>
	internal async Task HandleDispatchSafelyAsync(GatewayPayload payload)
	{
		try
		{
			await this.HandleDispatchAsync(payload).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			this.Logger.LogError(LoggerEvents.WebSocketReceiveFailure, ex, "Dispatch handler suppressed an exception for event {EventName}", payload.EventName);

			if (this.DiagnosticsSink.IsEnabled)
			{
				Dictionary<string, object> context = new()
				{
					["event_name"] = payload.EventName ?? "unknown",
					["opcode"] = (int)payload.OpCode,
					["sequence"] = payload.Sequence ?? -1
				};
				Dictionary<string, string> tags = new()
				{
					[Telemetry.DiagnosticTags.ErrorOrigin] = Telemetry.DiagnosticTags.OriginLibrary,
					["dcs.gateway_event"] = payload.EventName ?? "unknown",
					["dcs.gateway_opcode"] = ((int)payload.OpCode).ToString()
				};

				this.DiagnosticsSink.CaptureException("DisCatSharp", ex, context, tags);
			}
		}
	}

	/// <summary>
	///     Sequentially consumes dispatch payloads from <see cref="_dispatchQueue" />.
	/// </summary>
	/// <remarks>
	///     <para>
	///         In <see cref="GatewayDispatchMode.ConcurrentHandlers" /> mode (default), each payload is dequeued in FIFO
	///         order but processing is fire-and-forget, allowing multiple events to be handled concurrently. This matches
	///         the throughput characteristics of the previous <c>Task.Run</c> dispatch, while adding bounded back-pressure
	///         and clean shutdown semantics.
	///     </para>
	///     <para>
	///         In <see cref="GatewayDispatchMode.SequentialHandlers" /> mode, each payload is fully awaited before the
	///         next one is dequeued — providing total serialization of internal cache mutations and user handler execution
	///         at the cost of throughput.
	///     </para>
	/// </remarks>
	/// <param name="cancellationToken">Token that signals shutdown.</param>
	private async Task DispatchConsumerLoopAsync(CancellationToken cancellationToken)
	{
		var mode = this.Configuration.Gateway.Advanced.DispatchMode;
		this.Logger.LogDebug(LoggerEvents.Startup, "Dispatch consumer loop started (mode: {DispatchMode})", mode);

		try
		{
			await foreach (var payload in this._dispatchQueue!.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
			{
				if (mode is GatewayDispatchMode.SequentialHandlers)
					await this.HandleDispatchSafelyAsync(payload).ConfigureAwait(false);
				else
					_ = Task.Run(async () => await this.HandleDispatchSafelyAsync(payload).ConfigureAwait(false), cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			// Expected during shutdown — the cancellation token was triggered.
		}
		catch (ChannelClosedException)
		{
			// Expected if the channel writer is completed (rare but safe).
		}
		catch (Exception ex)
		{
			this.Logger.LogCritical(LoggerEvents.WebSocketReceiveFailure, ex, "Dispatch consumer loop terminated unexpectedly");

			if (this.DiagnosticsSink.IsEnabled)
				this.DiagnosticsSink.CaptureException("DisCatSharp", ex, new Dictionary<string, object>
				{
					["component"] = "DispatchConsumerLoop"
				}, new Dictionary<string, string>
				{
					[Telemetry.DiagnosticTags.ErrorOrigin] = Telemetry.DiagnosticTags.OriginLibrary
				});
		}

		this.Logger.LogDebug(LoggerEvents.Misc, "Dispatch consumer loop stopped");
	}

	/// <summary>
	///     Handles the heartbeat loop.
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
	///     Updates the status.
	/// </summary>
	/// <param name="activities">The activity.</param>
	/// <param name="userStatus">The optional user status.</param>
	/// <param name="idleSince">Since when is the client performing the specified activity.</param>
	internal async Task InternalUpdateStatusAsync(List<DiscordActivity>? activities, UserStatus? userStatus, DateTimeOffset? idleSince)
	{
		if (activities?.Any(a => a.Name?.Length > 128) ?? false)
			throw new("Game name can't be longer than 128 characters!");

		var sinceUnix = idleSince != null ? (long?)Utilities.GetUnixTime(idleSince.Value) : null;
		var acts = activities ?? [new()];
		var status = new StatusUpdate
		{
			Activities = [.. acts.Select(a => new TransportActivity(a))],
			IdleSince = sinceUnix,
			IsAfk = idleSince != null,
			Status = userStatus ?? UserStatus.Online,
			ActivitiesInternal = acts
		};

		// Solution to have status persist between sessions
		this._status = status;
		var statusUpdate = new GatewayPayload
		{
			OpCode = GatewayOpCode.StatusUpdate,
			Data = status
		};

		var statusstr = JsonConvert.SerializeObject(statusUpdate);

		await this.WsSendAsync(statusstr).ConfigureAwait(false);

		if (!this.PresencesInternal.TryGetValue(this.CurrentUser.Id, out var value))
			this.CacheAggregatePresence(new()
			{
				Discord = this,
				Activity = acts.First(),
				InternalActivities = acts,
				Status = userStatus ?? UserStatus.Online,
				InternalUser = new()
				{
					Id = this.CurrentUser.Id
				}
			});
		else
		{
			value.Activity = acts.First();
			value.InternalActivities = acts;
			value.Status = userStatus ?? value.Status;
			this.CacheAggregatePresence(value);
		}
	}

	/// <summary>
	///     Sends the heartbeat.
	/// </summary>
	/// <param name="seq">The sequenze.</param>
	internal async Task SendHeartbeatAsync(long seq)
	{
		var moreThan5 = Volatile.Read(ref this._skippedHeartbeats) > 5;
		var guildsComp = Volatile.Read(ref this._guildDownloadCompleted);

		switch (guildsComp)
		{
			case true when moreThan5:
			{
				this.Logger.LogCritical(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats - connection is zombie");

				var args = new ZombiedEventArgs(this.ServiceProvider)
				{
					Failures = Volatile.Read(ref this._skippedHeartbeats),
					GuildDownloadCompleted = true
				};
				await this._zombied.InvokeAsync(this, args).ConfigureAwait(false);

				await this.InternalReconnectAsync(code: 4001, message: "Too many heartbeats missed").ConfigureAwait(false);
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

				this.Logger.LogWarning(LoggerEvents.HeartbeatFailure, "Server failed to acknowledge more than 5 heartbeats, but the guild download is still running - check your connection speed");
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

		this._lastHeartbeat = DateTimeOffset.UtcNow;

		Interlocked.Increment(ref this._skippedHeartbeats);
	}

	/// <summary>
	///     Sends the identify payload.
	/// </summary>
	/// <param name="status">The status update payload.</param>
	internal async Task SendIdentifyAsync(StatusUpdate? status)
	{
		if (this._identified)
			return;
		var identify = new GatewayIdentify
		{
			Token = Utilities.GetFormattedToken(this),
			Compress = this.Configuration.Gateway.CompressionLevel == GatewayCompressionLevel.Payload,
			LargeThreshold = this.Configuration.Gateway.LargeThreshold,
			ShardInfo = new()
			{
				ShardId = this.Configuration.Gateway.ShardId,
				ShardCount = this.Configuration.Gateway.ShardCount
			},
			Presence = status,
			Intents = this.Configuration.Intents,
			Discord = this,
			Capabilities = this.Configuration.Gateway.Capabilities
		};
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Identify,
			Data = identify
		};
		var payloadstr = JsonConvert.SerializeObject(payload);
		await this.WsSendAsync(payloadstr).ConfigureAwait(false);

		this.Logger.LogDebug(LoggerEvents.Intents, "Registered gateway intents ({Intents})", this.Configuration.Intents);
		this._identified = true;
	}

	/// <summary>
	///     Sends the resume payload.
	/// </summary>
	/// <param name="capturedSessionId">
	///     The session ID captured under <see cref="_sessionStateLock" /> by the caller.
	///     When <see langword="null" /> the method falls back to reading <c>_sessionId</c> directly
	///     (used by <see cref="OnInvalidateSessionAsync" /> which holds its own serialization guarantee
	///     via the socket-lock, so the TOCTOU window does not exist in that path).
	/// </param>
	internal async Task SendResumeAsync(string? capturedSessionId = null)
	{
		var sessionId = capturedSessionId ?? this._sessionId;
		ArgumentNullException.ThrowIfNull(sessionId);
		ArgumentNullException.ThrowIfNull(this._resumeGatewayUrl);

		var resume = new GatewayResume
		{
			Token = Utilities.GetFormattedToken(this),
			SessionId = sessionId,
			SequenceNumber = Volatile.Read(ref this._lastSequence)
		};
		var resumePayload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Resume,
			Data = resume
		};
		var resumestr = JsonConvert.SerializeObject(resumePayload);
		this.GatewayUri = new(this._resumeGatewayUrl);
		this.Logger.LogDebug(LoggerEvents.ConnectionClose, "Request to resume via {gw}", this.GatewayUri.AbsoluteUri);
		await this.WsSendAsync(resumestr).ConfigureAwait(false);
	}

	/// <summary>
	///     Internals the update gateway async.
	/// </summary>
	/// <returns>A Task.</returns>
	internal async Task InternalUpdateGatewayAsync()
	{
		var info = await this.GetGatewayInfoAsync().ConfigureAwait(false);
		this.GatewayInfo = info;
		this.GatewayUri = new(info.Url);
	}

	/// <summary>
	///     Sends a websocket message.
	/// </summary>
	/// <param name="payload">The payload to send.</param>
	internal async Task WsSendAsync(string payload)
	{
		this.Logger.LogTrace(LoggerEvents.GatewayWsTx, "{Payload}", payload);
		await this.WebSocketClient.SendMessageAsync(payload).ConfigureAwait(false);
	}

	#endregion
}
