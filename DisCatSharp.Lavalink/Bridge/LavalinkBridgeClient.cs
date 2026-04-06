using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Interfaces;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Lavalink.Bridge;

/// <summary>
///     WebSocket client for the Lavalink transport bridge.
///     Connects to a patched Lavalink server's bridge endpoint and demultiplexes
///     Opus frames per guild.
/// </summary>
internal sealed class LavalinkBridgeClient : IAsyncDisposable
{
	private const int BINARY_HEADER_SIZE = 20;
	private const byte PROTOCOL_VERSION = 1;
	private const byte PACKET_TYPE_OPUS = 0;

	private readonly LavalinkBridgeConfiguration _config;
	private readonly ILogger _logger;
	private readonly ConcurrentDictionary<ulong, Channel<ExternalOpusFrame>> _guildChannels = new();
	private readonly CancellationTokenSource _cts = new();

	private ClientWebSocket? _ws;
	private Task? _receiveTask;

	/// <summary>
	///     Fired when a control message is received from the bridge.
	/// </summary>
	internal event Func<string, ulong, JObject, Task>? ControlMessageReceived;

	/// <summary>
	///     Creates a new instance of <see cref="LavalinkBridgeClient" />.
	/// </summary>
	/// <param name="config">The bridge configuration.</param>
	/// <param name="logger">The logger instance.</param>
	public LavalinkBridgeClient(LavalinkBridgeConfiguration config, ILogger logger)
	{
		this._config = config;
		this._logger = logger;
	}

	/// <summary>
	///     Connects to the bridge WebSocket endpoint and starts receiving frames.
	/// </summary>
	/// <param name="cancellationToken">Token to cancel the connection attempt.</param>
	public async Task ConnectAsync(CancellationToken cancellationToken = default)
	{
		this._config.Validate();

		this._ws = new ClientWebSocket();
		this._ws.Options.SetRequestHeader("Authorization", $"Bearer {this._config.BridgeAuthToken}");

		this._logger.LogInformation("[Lavalink Bridge] Connecting to {Endpoint}", this._config.BridgeEndpoint);
		await this._ws.ConnectAsync(this._config.BridgeEndpoint!, cancellationToken).ConfigureAwait(false);
		this._logger.LogInformation("[Lavalink Bridge] Connected successfully");

		this._receiveTask = Task.Run(() => this.ReceiveLoopAsync(this._cts.Token), this._cts.Token);
	}

	/// <summary>
	///     Gets or creates an <see cref="IExternalOpusSource" /> for the specified guild.
	/// </summary>
	/// <param name="guildId">The guild snowflake ID.</param>
	/// <returns>An <see cref="IExternalOpusSource" /> that yields Opus frames for the guild.</returns>
	public IExternalOpusSource GetGuildOpusSource(ulong guildId)
	{
		var channel = this._guildChannels.GetOrAdd(guildId, _ =>
			Channel.CreateBounded<ExternalOpusFrame>(new BoundedChannelOptions(50)
			{
				FullMode = BoundedChannelFullMode.DropOldest,
				SingleReader = true,
				SingleWriter = true
			}));

		return new GuildOpusSource(channel.Reader);
	}

	/// <summary>
	///     Removes the guild channel, stopping frame delivery for that guild.
	/// </summary>
	/// <param name="guildId">The guild snowflake ID.</param>
	public void DetachGuild(ulong guildId)
	{
		if (this._guildChannels.TryRemove(guildId, out var channel))
			channel.Writer.TryComplete();
	}

	/// <summary>
	///     Main receive loop that reads frames from the bridge WebSocket.
	/// </summary>
	private async Task ReceiveLoopAsync(CancellationToken ct)
	{
		var buffer = new byte[4096];
		var segment = new ArraySegment<byte>(buffer);

		try
		{
			while (!ct.IsCancellationRequested && this._ws?.State == WebSocketState.Open)
			{
				var result = await this._ws.ReceiveAsync(segment, ct).ConfigureAwait(false);

				if (result.MessageType == WebSocketMessageType.Close)
				{
					this._logger.LogWarning("[Lavalink Bridge] Server closed connection: {Status} {Description}",
						result.CloseStatus, result.CloseStatusDescription);
					break;
				}

				if (result.MessageType == WebSocketMessageType.Binary)
					this.ProcessBinaryFrame(buffer.AsSpan(0, result.Count));
				else if (result.MessageType == WebSocketMessageType.Text)
					this.ProcessTextFrame(buffer.AsSpan(0, result.Count));
			}
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			// Normal shutdown
		}
		catch (WebSocketException ex)
		{
			this._logger.LogError(ex, "[Lavalink Bridge] WebSocket error, attempting reconnect");
			_ = Task.Run(() => this.ReconnectAsync(ct), ct);
		}
	}

	/// <summary>
	///     Parses a binary frame containing an Opus audio packet.
	/// </summary>
	private void ProcessBinaryFrame(ReadOnlySpan<byte> data)
	{
		if (data.Length < BINARY_HEADER_SIZE)
		{
			this._logger.LogWarning("[Lavalink Bridge] Binary frame too short: {Length} bytes", data.Length);
			return;
		}

		var version = data[0];
		if (version != PROTOCOL_VERSION)
		{
			this._logger.LogWarning("[Lavalink Bridge] Unknown protocol version: {Version}", version);
			return;
		}

		var packetType = data[1];
		if (packetType != PACKET_TYPE_OPUS)
		{
			this._logger.LogDebug("[Lavalink Bridge] Unknown packet type: {Type}", packetType);
			return;
		}

		var guildId = BinaryPrimitives.ReadUInt64BigEndian(data[2..]);
		var sequence = BinaryPrimitives.ReadUInt32BigEndian(data[10..]);
		var timestamp = BinaryPrimitives.ReadUInt32BigEndian(data[14..]);
		var durationMs = BinaryPrimitives.ReadUInt16BigEndian(data[18..]);
		var opusPayload = data[BINARY_HEADER_SIZE..];

		if (opusPayload.IsEmpty)
			return;

		var frame = new ExternalOpusFrame(
			opusPayload.ToArray(),
			durationMs,
			sequence,
			timestamp);

		if (this._guildChannels.TryGetValue(guildId, out var channel))
			channel.Writer.TryWrite(frame);
	}

	/// <summary>
	///     Parses a JSON text frame containing a control message.
	/// </summary>
	private void ProcessTextFrame(ReadOnlySpan<byte> data)
	{
		try
		{
			var json = Encoding.UTF8.GetString(data);
			var obj = JObject.Parse(json);
			var op = obj.Value<string>("op");
			var guildIdStr = obj.Value<string>("guildId");

			if (op is null || guildIdStr is null || !ulong.TryParse(guildIdStr, out var guildId))
			{
				this._logger.LogWarning("[Lavalink Bridge] Malformed control message: {Json}", json);
				return;
			}

			this._logger.LogDebug("[Lavalink Bridge] Control message: op={Op} guild={Guild}", op, guildId);
			this.ControlMessageReceived?.Invoke(op, guildId, obj);
		}
		catch (Exception ex)
		{
			this._logger.LogWarning(ex, "[Lavalink Bridge] Failed to parse text frame");
		}
	}

	/// <summary>
	///     Attempts to reconnect to the bridge WebSocket with exponential backoff.
	/// </summary>
	private async Task ReconnectAsync(CancellationToken ct)
	{
		var attempts = 0;
		var maxAttempts = this._config.MaxReconnectAttempts;

		while (!ct.IsCancellationRequested && (maxAttempts < 0 || attempts < maxAttempts))
		{
			attempts++;
			this._logger.LogInformation("[Lavalink Bridge] Reconnect attempt {Attempt}/{Max}",
				attempts, maxAttempts < 0 ? "∞" : maxAttempts.ToString());

			try
			{
				this._ws?.Dispose();
				this._ws = new ClientWebSocket();
				this._ws.Options.SetRequestHeader("Authorization", $"Bearer {this._config.BridgeAuthToken}");

				await this._ws.ConnectAsync(this._config.BridgeEndpoint!, ct).ConfigureAwait(false);
				this._logger.LogInformation("[Lavalink Bridge] Reconnected successfully");

				this._receiveTask = Task.Run(() => this.ReceiveLoopAsync(ct), ct);
				return;
			}
			catch (Exception ex)
			{
				this._logger.LogWarning(ex, "[Lavalink Bridge] Reconnect attempt {Attempt} failed", attempts);
				await Task.Delay(this._config.ReconnectDelay, ct).ConfigureAwait(false);
			}
		}

		this._logger.LogError("[Lavalink Bridge] Max reconnect attempts reached, giving up");
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await this._cts.CancelAsync().ConfigureAwait(false);

		foreach (var kvp in this._guildChannels)
			kvp.Value.Writer.TryComplete();

		this._guildChannels.Clear();

		if (this._ws is { State: WebSocketState.Open })
		{
			try
			{
				await this._ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None).ConfigureAwait(false);
			}
			catch
			{
				// Best-effort close
			}
		}

		this._ws?.Dispose();
		this._cts.Dispose();
	}

	/// <summary>
	///     Per-guild <see cref="IExternalOpusSource" /> implementation that reads from a channel.
	/// </summary>
	private sealed class GuildOpusSource(ChannelReader<ExternalOpusFrame> reader) : IExternalOpusSource
	{
		/// <inheritdoc />
		public async IAsyncEnumerable<ExternalOpusFrame> ReadFramesAsync(
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var frame in reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
				yield return frame;
		}
	}
}
