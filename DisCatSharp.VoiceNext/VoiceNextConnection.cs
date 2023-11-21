using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Net;
using DisCatSharp.Net.Udp;
using DisCatSharp.Net.WebSocket;
using DisCatSharp.VoiceNext.Codec;
using DisCatSharp.VoiceNext.Entities;
using DisCatSharp.VoiceNext.EventArgs;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.VoiceNext;

internal delegate Task VoiceDisconnectedEventHandler(DiscordGuild guild);

/// <summary>
/// VoiceNext connection to a voice channel.
/// </summary>
public sealed class VoiceNextConnection : IDisposable
{
	/// <summary>
	/// Triggered whenever a user speaks in the connected voice channel.
	/// </summary>
	public event AsyncEventHandler<VoiceNextConnection, UserSpeakingEventArgs> UserSpeaking
	{
		add => this._userSpeaking.Register(value);
		remove => this._userSpeaking.Unregister(value);
	}
	private readonly AsyncEvent<VoiceNextConnection, UserSpeakingEventArgs> _userSpeaking;

	/// <summary>
	/// Triggered whenever a user joins voice in the connected guild.
	/// </summary>
	public event AsyncEventHandler<VoiceNextConnection, VoiceUserJoinEventArgs> UserJoined
	{
		add => this._userJoined.Register(value);
		remove => this._userJoined.Unregister(value);
	}
	private readonly AsyncEvent<VoiceNextConnection, VoiceUserJoinEventArgs> _userJoined;

	/// <summary>
	/// Triggered whenever a user leaves voice in the connected guild.
	/// </summary>
	public event AsyncEventHandler<VoiceNextConnection, VoiceUserLeaveEventArgs> UserLeft
	{
		add => this._userLeft.Register(value);
		remove => this._userLeft.Unregister(value);
	}
	private readonly AsyncEvent<VoiceNextConnection, VoiceUserLeaveEventArgs> _userLeft;

	/// <summary>
	/// Triggered whenever voice data is received from the connected voice channel.
	/// </summary>
	public event AsyncEventHandler<VoiceNextConnection, VoiceReceiveEventArgs> VoiceReceived
	{
		add => this._voiceReceived.Register(value);
		remove => this._voiceReceived.Unregister(value);
	}
	private readonly AsyncEvent<VoiceNextConnection, VoiceReceiveEventArgs> _voiceReceived;

	/// <summary>
	/// Triggered whenever voice WebSocket throws an exception.
	/// </summary>
	public event AsyncEventHandler<VoiceNextConnection, SocketErrorEventArgs> VoiceSocketErrored
	{
		add => this._voiceSocketError.Register(value);
		remove => this._voiceSocketError.Unregister(value);
	}
	private readonly AsyncEvent<VoiceNextConnection, SocketErrorEventArgs> _voiceSocketError;

	internal event VoiceDisconnectedEventHandler VoiceDisconnected;

	/// <summary>
	/// Gets the unix epoch.
	/// </summary>
	private static DateTimeOffset s_unixEpoch { get; } = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

	/// <summary>
	/// Gets the discord.
	/// </summary>
	private readonly DiscordClient _discord;

	/// <summary>
	/// Gets the guild.
	/// </summary>
	private readonly DiscordGuild _guild;

	/// <summary>
	/// Gets the transmitting s s r cs.
	/// </summary>
	private readonly ConcurrentDictionary<uint, AudioSender> _transmittingSsrCs;

	/// <summary>
	/// Gets the udp client.
	/// </summary>
	private readonly BaseUdpClient _udpClient;

	/// <summary>
	/// Gets or sets the voice ws.
	/// </summary>
	private IWebSocketClient _voiceWs;

	/// <summary>
	/// Gets or sets the heartbeat task.
	/// </summary>
	private Task _heartbeatTask;

	/// <summary>
	/// Gets or sets the heartbeat interval.
	/// </summary>
	private int _heartbeatInterval;

	/// <summary>
	/// Gets or sets the last heartbeat.
	/// </summary>
	private DateTimeOffset _lastHeartbeat;

	/// <summary>
	/// Gets or sets the token source.
	/// </summary>
	private CancellationTokenSource _tokenSource;
	/// <summary>
	/// Gets the token.
	/// </summary>
	private CancellationToken TOKEN
		=> this._tokenSource.Token;

	/// <summary>
	/// Saves the last speaking flag
	/// </summary>
	private SpeakingFlags _speakingFlags;

	/// <summary>
	/// Gets or sets the server data.
	/// </summary>
	internal VoiceServerUpdatePayload ServerData { get; set; }
	/// <summary>
	/// Gets or sets the state data.
	/// </summary>
	internal VoiceStateUpdatePayload StateData { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether resume.
	/// </summary>
	internal bool Resume { get; set; }

	/// <summary>
	/// Gets the configuration.
	/// </summary>
	private readonly VoiceNextConfiguration _configuration;

	/// <summary>
	/// Gets or sets the opus.
	/// </summary>
	private Opus _opus;

	/// <summary>
	/// Gets or sets the sodium.
	/// </summary>
	private Sodium _sodium;

	/// <summary>
	/// Gets or sets the rtp.
	/// </summary>
	private Rtp _rtp;

	/// <summary>
	/// Gets or sets the selected encryption mode.
	/// </summary>
	private EncryptionMode _selectedEncryptionMode;
	/// <summary>
	/// Gets or sets the nonce.
	/// </summary>
	private uint _nonce;

	/// <summary>
	/// Gets or sets the sequence.
	/// </summary>
	private ushort _sequence;

	/// <summary>
	/// Gets or sets the timestamp.
	/// </summary>
	private uint _timestamp;

	/// <summary>
	/// Gets or sets the s s r c.
	/// </summary>
	private uint _ssrc;

	/// <summary>
	/// Gets or sets the key.
	/// </summary>
	private byte[] _key;

	/// <summary>
	/// Gets or sets the discovered endpoint.
	/// </summary>
	private IpEndpoint _discoveredEndpoint;
	/// <summary>
	/// Gets or sets the web socket endpoint.
	/// </summary>
	internal ConnectionEndpoint WebSocketEndpoint { get; set; }
	/// <summary>
	/// Gets or sets the udp endpoint.
	/// </summary>
	internal ConnectionEndpoint UdpEndpoint { get; set; }

	/// <summary>
	/// Gets or sets the ready wait.
	/// </summary>
	private readonly TaskCompletionSource<bool> _readyWait;

	/// <summary>
	/// Gets or sets a value indicating whether is initialized.
	/// </summary>
	private bool _isInitialized;

	/// <summary>
	/// Gets or sets a value indicating whether is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	/// Gets or sets the playing wait.
	/// </summary>
	private TaskCompletionSource<bool> _playingWait;

	/// <summary>
	/// Gets the pause event.
	/// </summary>
	private readonly AsyncManualResetEvent _pauseEvent;

	/// <summary>
	/// Gets or sets the transmit stream.
	/// </summary>
	private VoiceTransmitSink _transmitStream;

	/// <summary>
	/// Gets the transmit channel.
	/// </summary>
	private readonly Channel<RawVoicePacket> _transmitChannel;

	/// <summary>
	/// Gets the keepalive timestamps.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, long> _keepaliveTimestamps;
	private ulong _lastKeepalive;

	/// <summary>
	/// Gets or sets the sender task.
	/// </summary>
	private Task _senderTask;

	/// <summary>
	/// Gets or sets the sender token source.
	/// </summary>
	private CancellationTokenSource _senderTokenSource;
	/// <summary>
	/// Gets the sender token.
	/// </summary>
	private CancellationToken SENDER_TOKEN
		=> this._senderTokenSource.Token;

	/// <summary>
	/// Gets or sets the receiver task.
	/// </summary>
	private Task _receiverTask;

	/// <summary>
	/// Gets or sets the receiver token source.
	/// </summary>
	private CancellationTokenSource _receiverTokenSource;
	/// <summary>
	/// Gets the receiver token.
	/// </summary>
	private CancellationToken RECEIVER_TOKEN
		=> this._receiverTokenSource.Token;

	/// <summary>
	/// Gets or sets the keepalive task.
	/// </summary>
	private Task _keepaliveTask;

	/// <summary>
	/// Gets or sets the keepalive token source.
	/// </summary>
	private CancellationTokenSource _keepaliveTokenSource;
	/// <summary>
	/// Gets the keepalive token.
	/// </summary>
	private CancellationToken KEEPALIVE_TOKEN
		=> this._keepaliveTokenSource.Token;

	/// <summary>
	/// Gets the audio format used by the Opus encoder.
	/// </summary>
	public AudioFormat AudioFormat => this._configuration.AudioFormat;

	/// <summary>
	/// Gets whether this connection is still playing audio.
	/// </summary>
	public bool IsPlaying
		=> this._playingWait != null && !this._playingWait.Task.IsCompleted;

	/// <summary>
	/// Gets the websocket round-trip time in ms.
	/// </summary>
	public int WebSocketPing
		=> Volatile.Read(ref this._wsPing);
	private int _wsPing;

	/// <summary>
	/// Gets the UDP round-trip time in ms.
	/// </summary>
	public int UdpPing
		=> Volatile.Read(ref this._udpPing);
	private int _udpPing;

	private int _queueCount;

	/// <summary>
	/// Gets the channel this voice client is connected to.
	/// </summary>
	public DiscordChannel TargetChannel { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="VoiceNextConnection"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="guild">The guild.</param>
	/// <param name="channel">The channel.</param>
	/// <param name="config">The config.</param>
	/// <param name="server">The server.</param>
	/// <param name="state">The state.</param>
	internal VoiceNextConnection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, VoiceNextConfiguration config, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
	{
		this._discord = client;
		this._guild = guild;
		this.TargetChannel = channel;
		this._transmittingSsrCs = new ConcurrentDictionary<uint, AudioSender>();

		this._userSpeaking = new AsyncEvent<VoiceNextConnection, UserSpeakingEventArgs>("VNEXT_USER_SPEAKING", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._userJoined = new AsyncEvent<VoiceNextConnection, VoiceUserJoinEventArgs>("VNEXT_USER_JOINED", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._userLeft = new AsyncEvent<VoiceNextConnection, VoiceUserLeaveEventArgs>("VNEXT_USER_LEFT", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._voiceReceived = new AsyncEvent<VoiceNextConnection, VoiceReceiveEventArgs>("VNEXT_VOICE_RECEIVED", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._voiceSocketError = new AsyncEvent<VoiceNextConnection, SocketErrorEventArgs>("VNEXT_WS_ERROR", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._tokenSource = new CancellationTokenSource();

		this._configuration = config;
		this._isInitialized = false;
		this._isDisposed = false;
		this._opus = new Opus(this.AudioFormat);
		//this.Sodium = new Sodium();
		this._rtp = new Rtp();

		this.ServerData = server;
		this.StateData = state;

		var eps = this.ServerData.Endpoint;
		var epi = eps.LastIndexOf(':');
		var eph = string.Empty;
		var epp = 443;
		if (epi != -1)
		{
			eph = eps[..epi];
			epp = int.Parse(eps[(epi + 1)..]);
		}
		else
		{
			eph = eps;
		}
		this.WebSocketEndpoint = new ConnectionEndpoint { Hostname = eph, Port = epp };

		this._readyWait = new TaskCompletionSource<bool>();

		this._playingWait = null;
		this._transmitChannel = Channel.CreateBounded<RawVoicePacket>(new BoundedChannelOptions(this._configuration.PacketQueueSize));
		this._keepaliveTimestamps = new ConcurrentDictionary<ulong, long>();
		this._pauseEvent = new AsyncManualResetEvent(true);

		this._udpClient = this._discord.Configuration.UdpClientFactory();
		this._voiceWs = this._discord.Configuration.WebSocketClientFactory(this._discord.Configuration.Proxy, this._discord.ServiceProvider);
		this._voiceWs.Disconnected += this.VoiceWS_SocketClosed;
		this._voiceWs.MessageReceived += this.VoiceWS_SocketMessage;
		this._voiceWs.Connected += this.VoiceWS_SocketOpened;
		this._voiceWs.ExceptionThrown += this.VoiceWs_SocketException;
	}

	~VoiceNextConnection()
	{
		this.Dispose();
	}

	/// <summary>
	/// Connects to the specified voice channel.
	/// </summary>
	/// <returns>A task representing the connection operation.</returns>
	internal Task ConnectAsync()
	{
		var gwuri = new UriBuilder
		{
			Scheme = "wss",
			Host = this.WebSocketEndpoint.Hostname,
			Query = "encoding=json&v=4"
		};

		return this._voiceWs.ConnectAsync(gwuri.Uri);
	}

	/// <summary>
	/// Reconnects .
	/// </summary>
	/// <returns>A Task.</returns>
	internal Task ReconnectAsync()
		=> this._voiceWs.DisconnectAsync();

	/// <summary>
	/// Starts .
	/// </summary>
	/// <returns>A Task.</returns>
	internal async Task StartAsync()
	{
		// Let's announce our intentions to the server
		var vdp = new VoiceDispatch();

		if (!this.Resume)
		{
			vdp.OpCode = 0;
			vdp.Payload = new VoiceIdentifyPayload
			{
				ServerId = this.ServerData.GuildId,
				UserId = this.StateData.UserId.Value,
				SessionId = this.StateData.SessionId,
				Token = this.ServerData.Token
			};
			this.Resume = true;
		}
		else
		{
			vdp.OpCode = 7;
			vdp.Payload = new VoiceIdentifyPayload
			{
				ServerId = this.ServerData.GuildId,
				SessionId = this.StateData.SessionId,
				Token = this.ServerData.Token
			};
		}
		var vdj = JsonConvert.SerializeObject(vdp, Formatting.None);
		await this.WsSendAsync(vdj).ConfigureAwait(false);
	}

	/// <summary>
	/// Waits the for ready async.
	/// </summary>
	/// <returns>A Task.</returns>
	internal Task WaitForReadyAsync()
		=> this._readyWait.Task;

	/// <summary>
	/// Enqueues the packet async.
	/// </summary>
	/// <param name="packet">The packet.</param>
	/// <param name="token">The token.</param>
	/// <returns>A Task.</returns>
	internal async Task EnqueuePacketAsync(RawVoicePacket packet, CancellationToken token = default)
	{
		await this._transmitChannel.Writer.WriteAsync(packet, token).ConfigureAwait(false);
		this._queueCount++;
	}

	/// <summary>
	/// Prepares the packet.
	/// </summary>
	/// <param name="pcm">The pcm.</param>
	/// <param name="target">The target.</param>
	/// <param name="length">The length.</param>
	/// <returns>A bool.</returns>
	internal bool PreparePacket(ReadOnlySpan<byte> pcm, out byte[] target, out int length)
	{
		target = null;
		length = 0;

		if (this._isDisposed)
			return false;

		var audioFormat = this.AudioFormat;

		var packetArray = ArrayPool<byte>.Shared.Rent(this._rtp.CalculatePacketSize(audioFormat.SampleCountToSampleSize(audioFormat.CalculateMaximumFrameSize()), this._selectedEncryptionMode));
		var packet = packetArray.AsSpan();

		this._rtp.EncodeHeader(this._sequence, this._timestamp, this._ssrc, packet);
		var opus = packet.Slice(Rtp.HEADER_SIZE, pcm.Length);
		this._opus.Encode(pcm, ref opus);

		this._sequence++;
		this._timestamp += (uint)audioFormat.CalculateFrameSize(audioFormat.CalculateSampleDuration(pcm.Length));

		Span<byte> nonce = stackalloc byte[Sodium.NonceSize];
		switch (this._selectedEncryptionMode)
		{
			case EncryptionMode.XSalsa20Poly1305:
				this._sodium.GenerateNonce(packet[..Rtp.HEADER_SIZE], nonce);
				break;

			case EncryptionMode.XSalsa20Poly1305Suffix:
				this._sodium.GenerateNonce(nonce);
				break;

			case EncryptionMode.XSalsa20Poly1305Lite:
				this._sodium.GenerateNonce(this._nonce++, nonce);
				break;

			default:
				ArrayPool<byte>.Shared.Return(packetArray);
				throw new Exception("Unsupported encryption mode.");
		}

		Span<byte> encrypted = stackalloc byte[Sodium.CalculateTargetSize(opus)];
		this._sodium.Encrypt(opus, encrypted, nonce);
		encrypted.CopyTo(packet[Rtp.HEADER_SIZE..]);
		packet = packet[..this._rtp.CalculatePacketSize(encrypted.Length, this._selectedEncryptionMode)];
		this._sodium.AppendNonce(nonce, packet, this._selectedEncryptionMode);

		target = packetArray;
		length = packet.Length;
		return true;
	}

	/// <summary>
	/// Voices the sender task.
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task VoiceSenderTask()
	{
		var token = this.SENDER_TOKEN;
		var client = this._udpClient;
		var reader = this._transmitChannel.Reader;

		byte[] data = null;
		var length = 0;

		var synchronizerTicks = (double)Stopwatch.GetTimestamp();
		var synchronizerResolution = Stopwatch.Frequency * 0.005;
		var tickResolution = 10_000_000.0 / Stopwatch.Frequency;
		this._discord.Logger.LogDebug(VoiceNextEvents.Misc, "Timer accuracy: {0}/{1} (high resolution? {2})", Stopwatch.Frequency, synchronizerResolution, Stopwatch.IsHighResolution);

		while (!token.IsCancellationRequested)
		{
			await this._pauseEvent.WaitAsync().ConfigureAwait(false);

			var hasPacket = reader.TryRead(out var rawPacket);
			if (hasPacket)
			{
				this._queueCount--;

				if (this._playingWait == null || this._playingWait.Task.IsCompleted)
					this._playingWait = new TaskCompletionSource<bool>();
			}

			// Provided by Laura#0090 (214796473689178133); this is Python, but adaptable:
			//
			// delay = max(0, self.delay + ((start_time + self.delay * loops) + - time.time()))
			//
			// self.delay
			//   sample size
			// start_time
			//   time since streaming started
			// loops
			//   number of samples sent
			// time.time()
			//   DateTime.Now

			if (hasPacket)
			{
				hasPacket = this.PreparePacket(rawPacket.Bytes.Span, out data, out length);
				if (rawPacket.RentedBuffer != null)
					ArrayPool<byte>.Shared.Return(rawPacket.RentedBuffer);
			}

			var durationModifier = hasPacket ? rawPacket.Duration / 5 : 4;
			var cts = Math.Max(Stopwatch.GetTimestamp() - synchronizerTicks, 0);
			if (cts < synchronizerResolution * durationModifier)
				await Task.Delay(TimeSpan.FromTicks((long)(((synchronizerResolution * durationModifier) - cts) * tickResolution))).ConfigureAwait(false);

			synchronizerTicks += synchronizerResolution * durationModifier;

			if (!hasPacket)
				continue;

			await this.SendSpeakingAsync(this._speakingFlags != SpeakingFlags.NotSpeaking ? this._speakingFlags : SpeakingFlags.Microphone).ConfigureAwait(false);
			await client.SendAsync(data, length).ConfigureAwait(false);
			ArrayPool<byte>.Shared.Return(data);

			if (!rawPacket.Silence && this._queueCount == 0)
			{
				var nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
				for (var i = 0; i < 3; i++)
				{
					var nullpacket = new byte[nullpcm.Length];
					var nullpacketmem = nullpacket.AsMemory();
					await this.EnqueuePacketAsync(new RawVoicePacket(nullpacketmem, 20, true)).ConfigureAwait(false);
				}
			}
			else if (this._queueCount == 0)
			{
				this._speakingFlags = SpeakingFlags.NotSpeaking;
				await this.SendSpeakingAsync(this._speakingFlags).ConfigureAwait(false);
				this._playingWait?.SetResult(true);
			}
		}
	}

	/// <summary>
	/// Processes the packet.
	/// </summary>
	/// <param name="data">The data.</param>
	/// <param name="opus">The opus.</param>
	/// <param name="pcm">The pcm.</param>
	/// <param name="pcmPackets">The pcm packets.</param>
	/// <param name="voiceSender">The voice sender.</param>
	/// <param name="outputFormat">The output format.</param>
	/// <returns>A bool.</returns>
	private bool ProcessPacket(ReadOnlySpan<byte> data, ref Memory<byte> opus, ref Memory<byte> pcm, IList<ReadOnlyMemory<byte>> pcmPackets, out AudioSender voiceSender, out AudioFormat outputFormat)
	{
		voiceSender = null;
		outputFormat = default;

		if (!this._rtp.IsRtpHeader(data))
			return false;

		this._rtp.DecodeHeader(data, out var shortSequence, out var timestamp, out var ssrc, out var hasExtension);

		if (!this._transmittingSsrCs.TryGetValue(ssrc, out var vtx))
		{
			var decoder = this._opus.CreateDecoder();

			vtx = new AudioSender(ssrc, decoder)
			{
				// user isn't present as we haven't received a speaking event yet.
				User = null
			};

			_transmittingSsrCs.TryAdd(ssrc, vtx);
		}

		voiceSender = vtx;
		var sequence = vtx.GetTrueSequenceAfterWrapping(shortSequence);
		ushort gap = 0;
		if (vtx.LastTrueSequence is ulong lastTrueSequence)
		{
			if (sequence <= lastTrueSequence) // out-of-order packet; discard
				return false;

			gap = (ushort)(sequence - 1 - lastTrueSequence);
			if (gap >= 5)
				this._discord.Logger.LogWarning(VoiceNextEvents.VoiceReceiveFailure, "5 or more voice packets were dropped when receiving");
		}

		Span<byte> nonce = stackalloc byte[Sodium.NonceSize];
		this._sodium.GetNonce(data, nonce, this._selectedEncryptionMode);
		this._rtp.GetDataFromPacket(data, out var encryptedOpus, this._selectedEncryptionMode);

		var opusSize = Sodium.CalculateSourceSize(encryptedOpus);
		opus = opus[..opusSize];
		var opusSpan = opus.Span;
		try
		{
			this._sodium.Decrypt(encryptedOpus, opusSpan, nonce);

			// Strip extensions, if any
			if (hasExtension)
			{
				// RFC 5285, 4.2 One-Byte header
				// http://www.rfcreader.com/#rfc5285_line186
				if (opusSpan[0] == 0xBE && opusSpan[1] == 0xDE)
				{
					var headerLen = (opusSpan[2] << 8) | opusSpan[3];
					var i = 4;
					for (; i < headerLen + 4; i++)
					{
						var @byte = opusSpan[i];

						// ID is currently unused since we skip it anyway
						//var id = (byte)(@byte >> 4);
						var length = (byte)(@byte & 0x0F) + 1;

						i += length;
					}

					// Strip extension padding too
					while (opusSpan[i] == 0)
						i++;

					opusSpan = opusSpan[i..];
				}

				// TODO: consider implementing RFC 5285, 4.3. Two-Byte Header
			}

			if (opusSpan[0] == 0x90)
			{
				// I'm not 100% sure what this header is/does, however removing the data causes no
				// real issues, and has the added benefit of removing a lot of noise.
				opusSpan = opusSpan[2..];
			}

			if (gap == 1)
			{
				var lastSampleCount = this._opus.GetLastPacketSampleCount(vtx.Decoder);
				var fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
				var fecpcmMem = fecpcm.AsSpan();
				this._opus.Decode(vtx.Decoder, opusSpan, ref fecpcmMem, true, out _);
				pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
			}
			else if (gap > 1)
			{
				var lastSampleCount = this._opus.GetLastPacketSampleCount(vtx.Decoder);
				for (var i = 0; i < gap; i++)
				{
					var fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
					var fecpcmMem = fecpcm.AsSpan();
					this._opus.ProcessPacketLoss(vtx.Decoder, lastSampleCount, ref fecpcmMem);
					pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
				}
			}

			var pcmSpan = pcm.Span;
			this._opus.Decode(vtx.Decoder, opusSpan, ref pcmSpan, false, out outputFormat);
			pcm = pcm[..pcmSpan.Length];
		}
		finally
		{
			vtx.LastTrueSequence = sequence;
		}

		return true;
	}

	/// <summary>
	/// Processes the voice packet.
	/// </summary>
	/// <param name="data">The data.</param>
	/// <returns>A Task.</returns>
	private async Task ProcessVoicePacket(byte[] data)
	{
		if (data.Length < 13) // minimum packet length
			return;

		try
		{
			var pcm = new byte[this.AudioFormat.CalculateMaximumFrameSize()];
			var pcmMem = pcm.AsMemory();
			var opus = new byte[pcm.Length];
			var opusMem = opus.AsMemory();
			var pcmFillers = new List<ReadOnlyMemory<byte>>();
			if (!this.ProcessPacket(data, ref opusMem, ref pcmMem, pcmFillers, out var vtx, out var audioFormat))
				return;

			foreach (var pcmFiller in pcmFillers)
				await this._voiceReceived.InvokeAsync(this, new VoiceReceiveEventArgs(this._discord.ServiceProvider)
				{
					Ssrc = vtx.Ssrc,
					User = vtx.User,
					PcmData = pcmFiller,
					OpusData = Array.Empty<byte>().AsMemory(),
					AudioFormat = audioFormat,
					AudioDuration = audioFormat.CalculateSampleDuration(pcmFiller.Length)
				}).ConfigureAwait(false);

			await this._voiceReceived.InvokeAsync(this, new VoiceReceiveEventArgs(this._discord.ServiceProvider)
			{
				Ssrc = vtx.Ssrc,
				User = vtx.User,
				PcmData = pcmMem,
				OpusData = opusMem,
				AudioFormat = audioFormat,
				AudioDuration = audioFormat.CalculateSampleDuration(pcmMem.Length)
			}).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			this._discord.Logger.LogError(VoiceNextEvents.VoiceReceiveFailure, ex, "Exception occurred when decoding incoming audio data");
		}
	}

	/// <summary>
	/// Processes the keepalive.
	/// </summary>
	/// <param name="data">The data.</param>
	private void ProcessKeepalive(byte[] data)
	{
		try
		{
			var keepalive = BinaryPrimitives.ReadUInt64LittleEndian(data);

			if (!this._keepaliveTimestamps.TryRemove(keepalive, out var timestamp))
				return;

			var tdelta = (int)((Stopwatch.GetTimestamp() - timestamp) / (double)Stopwatch.Frequency * 1000);
			this._discord.Logger.LogDebug(VoiceNextEvents.VoiceKeepalive, "Received UDP keepalive {0} (ping {1}ms)", keepalive, tdelta);
			Volatile.Write(ref this._udpPing, tdelta);
		}
		catch (Exception ex)
		{
			this._discord.Logger.LogError(VoiceNextEvents.VoiceKeepalive, ex, "Exception occurred when handling keepalive");
		}
	}

	/// <summary>
	/// Udps the receiver task.
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task UdpReceiverTask()
	{
		var token = this.RECEIVER_TOKEN;
		var client = this._udpClient;

		while (!token.IsCancellationRequested)
		{
			var data = await client.ReceiveAsync().ConfigureAwait(false);
			if (data.Length == 8)
				this.ProcessKeepalive(data);
			else if (this._configuration.EnableIncoming)
				await this.ProcessVoicePacket(data).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Sends a speaking status to the connected voice channel.
	/// </summary>
	/// <param name="flags">Set the speaking flags.</param>
	/// <returns>A task representing the sending operation.</returns>
	public async Task SendSpeakingAsync(SpeakingFlags flags = SpeakingFlags.Microphone)
	{
		if (!this._isInitialized)
			throw new InvalidOperationException("The connection is not initialized");

		if (this._speakingFlags != flags)
		{
			this._speakingFlags = flags;
			var pld = new VoiceDispatch
			{
				OpCode = 5,
				Payload = new VoiceSpeakingPayload
				{
					Speaking = flags,
					Delay = 0
				}
			};

			var plj = JsonConvert.SerializeObject(pld, Formatting.None);
			await this.WsSendAsync(plj).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Gets a transmit stream for this connection, optionally specifying a packet size to use with the stream. If a stream is already configured, it will return the existing one.
	/// </summary>
	/// <param name="sampleDuration">Duration, in ms, to use for audio packets.</param>
	/// <returns>Transmit stream.</returns>
	public VoiceTransmitSink GetTransmitSink(int sampleDuration = 20)
	{
		if (!AudioFormat.AllowedSampleDurations.Contains(sampleDuration))
			throw new ArgumentOutOfRangeException(nameof(sampleDuration), "Invalid PCM sample duration specified.");

		this._transmitStream ??= new VoiceTransmitSink(this, sampleDuration);

		return this._transmitStream;
	}

	/// <summary>
	/// Asynchronously waits for playback to be finished. Playback is finished when speaking = false is signaled.
	/// </summary>
	/// <returns>A task representing the waiting operation.</returns>
	public async Task WaitForPlaybackFinishAsync()
	{
		if (this._playingWait != null)
			await this._playingWait.Task.ConfigureAwait(false);
	}

	/// <summary>
	/// Pauses playback.
	/// </summary>
	public void Pause()
		=> this._pauseEvent.Reset();

	/// <summary>
	/// Asynchronously resumes playback.
	/// </summary>
	/// <returns></returns>
	public async Task ResumeAsync()
		=> await this._pauseEvent.SetAsync().ConfigureAwait(false);

	/// <summary>
	/// Disconnects and disposes this voice connection.
	/// </summary>
	public void Disconnect()
		=> this.Dispose();

	/// <summary>
	/// Disconnects and disposes this voice connection.
	/// </summary>
	public void Dispose()
	{
		if (this._isDisposed)
			return;

		try
		{
			this._isDisposed = true;
			this._isInitialized = false;
			this._tokenSource?.Cancel();
			this._senderTokenSource?.Cancel();
			this._receiverTokenSource?.Cancel();
		}
		catch (Exception ex)
		{
			this._discord.Logger.LogError(ex, ex.Message);
		}

		try
		{
			this._voiceWs.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			this._udpClient.Close();
		}
		catch { }

		try
		{
			this._keepaliveTokenSource?.Cancel();
			this._tokenSource?.Dispose();
			this._senderTokenSource?.Dispose();
			this._receiverTokenSource?.Dispose();
			this._keepaliveTokenSource?.Dispose();
			this._opus?.Dispose();
			this._opus = null;
			this._sodium?.Dispose();
			this._sodium = null;
			this._rtp?.Dispose();
			this._rtp = null;
		}
		catch (Exception ex)
		{
			this._discord.Logger.LogError(ex, ex.Message);
		}

		this.VoiceDisconnected?.Invoke(this._guild);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Heartbeats .
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task HeartbeatAsync()
	{
		await Task.Yield();

		var token = this.TOKEN;
		while (true)
		{
			try
			{
				token.ThrowIfCancellationRequested();

				var dt = DateTime.Now;
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceHeartbeat, "Sent heartbeat");

				var hbd = new VoiceDispatch
				{
					OpCode = 3,
					Payload = UnixTimestamp(dt)
				};
				var hbj = JsonConvert.SerializeObject(hbd);
				await this.WsSendAsync(hbj).ConfigureAwait(false);

				this._lastHeartbeat = dt;
				await Task.Delay(this._heartbeatInterval).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				return;
			}
		}
	}

	/// <summary>
	/// Keepalives .
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task KeepaliveAsync()
	{
		await Task.Yield();

		var token = this.KEEPALIVE_TOKEN;
		var client = this._udpClient;

		while (!token.IsCancellationRequested)
		{
			var timestamp = Stopwatch.GetTimestamp();
			var keepalive = Volatile.Read(ref this._lastKeepalive);
			Volatile.Write(ref this._lastKeepalive, keepalive + 1);
			this._keepaliveTimestamps.TryAdd(keepalive, timestamp);

			var packet = new byte[8];
			BinaryPrimitives.WriteUInt64LittleEndian(packet, keepalive);

			await client.SendAsync(packet, packet.Length).ConfigureAwait(false);

			await Task.Delay(5000, token).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Stage1S .
	/// </summary>
	/// <param name="voiceReady">The voice ready.</param>
	/// <returns>A Task.</returns>
	private async Task Stage1(VoiceReadyPayload voiceReady)
	{
		// IP Discovery
		this._udpClient.Setup(this.UdpEndpoint);

		var pck = new byte[74];
		PreparePacket(pck);
		await this._udpClient.SendAsync(pck, pck.Length).ConfigureAwait(false);

		var ipd = await this._udpClient.ReceiveAsync().ConfigureAwait(false);
		ReadPacket(ipd, out var ip, out var port);
		this._discoveredEndpoint = new IpEndpoint
		{
			Address = ip,
			Port = port
		};
		this._discord.Logger.LogTrace(VoiceNextEvents.VoiceHandshake, "Endpoint discovery finished - discovered endpoint is {0}:{1}", ip, port);

		void PreparePacket(byte[] packet)
		{
			var ssrc = this._ssrc;
			ushort type = 0x1;
			ushort length = 70;
			var packetSpan = packet.AsSpan();
			BinaryPrimitives.WriteUInt16BigEndian(packetSpan[..2], type);
			BinaryPrimitives.WriteUInt16BigEndian(packetSpan.Slice(2, 2), length);
			BinaryPrimitives.WriteUInt32BigEndian(packetSpan.Slice(4, 4), ssrc);
			packetSpan[8..].Clear();
		}

		void ReadPacket(byte[] packet, out System.Net.IPAddress decodedIp, out ushort decodedPort)
		{
			var packetSpan = packet.AsSpan();

			var ipString = Utilities.UTF8.GetString(packet, 8, 64 /* 74 - 10 */).TrimEnd('\0');
			decodedIp = System.Net.IPAddress.Parse(ipString);

			decodedPort = BinaryPrimitives.ReadUInt16BigEndian(packetSpan[72..] /* 74 - 2 */);
		}

		// Select voice encryption mode
		var selectedEncryptionMode = Sodium.SelectMode(voiceReady.Modes);
		this._selectedEncryptionMode = selectedEncryptionMode.Value;

		// Ready
		this._discord.Logger.LogTrace(VoiceNextEvents.VoiceHandshake, "Selected encryption mode is {0}", selectedEncryptionMode.Key);
		var vsp = new VoiceDispatch
		{
			OpCode = 1,
			Payload = new VoiceSelectProtocolPayload
			{
				Protocol = "udp",
				Data = new VoiceSelectProtocolPayloadData
				{
					Address = this._discoveredEndpoint.Address.ToString(),
					Port = (ushort)this._discoveredEndpoint.Port,
					Mode = selectedEncryptionMode.Key
				}
			}
		};
		var vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
		await this.WsSendAsync(vsj).ConfigureAwait(false);

		this._senderTokenSource = new CancellationTokenSource();
		this._senderTask = Task.Run(this.VoiceSenderTask, this.SENDER_TOKEN);

		this._receiverTokenSource = new CancellationTokenSource();
		this._receiverTask = Task.Run(this.UdpReceiverTask, this.RECEIVER_TOKEN);
	}

	/// <summary>
	/// Stage2S .
	/// </summary>
	/// <param name="voiceSessionDescription">The voice session description.</param>
	/// <returns>A Task.</returns>
	private async Task Stage2(VoiceSessionDescriptionPayload voiceSessionDescription)
	{
		this._selectedEncryptionMode = Sodium.SupportedModes[voiceSessionDescription.Mode.ToLowerInvariant()];
		this._discord.Logger.LogTrace(VoiceNextEvents.VoiceHandshake, "Discord updated encryption mode - new mode is {0}", this._selectedEncryptionMode);

		// start keepalive
		this._keepaliveTokenSource = new CancellationTokenSource();
		this._keepaliveTask = this.KeepaliveAsync();

		// send 3 packets of silence to get things going
		var nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
		for (var i = 0; i < 3; i++)
		{
			var nullPcm = new byte[nullpcm.Length];
			var nullpacketmem = nullPcm.AsMemory();
			await this.EnqueuePacketAsync(new RawVoicePacket(nullpacketmem, 20, true)).ConfigureAwait(false);
		}

		this._isInitialized = true;
		this._readyWait.SetResult(true);
	}

	/// <summary>
	/// Handles the dispatch.
	/// </summary>
	/// <param name="jo">The jo.</param>
	/// <returns>A Task.</returns>
	private async Task HandleDispatch(JObject jo)
	{
		var opc = (int)jo["op"];
		var opp = jo["d"] as JObject;

		switch (opc)
		{
			case 2: // READY
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received READY (OP2)");
				var vrp = opp.ToObject<VoiceReadyPayload>();
				this._ssrc = vrp.Ssrc;
				this.UdpEndpoint = new ConnectionEndpoint(vrp.Address, vrp.Port);
				// this is not the valid interval
				// oh, discord
				//this.HeartbeatInterval = vrp.HeartbeatInterval;
				this._heartbeatTask = Task.Run(this.HeartbeatAsync);
				await this.Stage1(vrp).ConfigureAwait(false);
				break;

			case 4: // SESSION_DESCRIPTION
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received SESSION_DESCRIPTION (OP4)");
				var vsd = opp.ToObject<VoiceSessionDescriptionPayload>();
				this._key = vsd.SecretKey;
				this._sodium = new Sodium(this._key.AsMemory());
				await this.Stage2(vsd).ConfigureAwait(false);
				break;

			case 5: // SPEAKING
					// Don't spam OP5
					// No longer spam, Discord supposedly doesn't send many of these
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received SPEAKING (OP5)");
				var spd = opp.ToObject<VoiceSpeakingPayload>();
				var foundUserInCache = this._discord.TryGetCachedUserInternal(spd.UserId.Value, out var resolvedUser);
				var spk = new UserSpeakingEventArgs(this._discord.ServiceProvider)
				{
					Speaking = spd.Speaking,
					Ssrc = spd.Ssrc.Value,
					User = resolvedUser,
				};

				if (foundUserInCache && this._transmittingSsrCs.TryGetValue(spk.Ssrc, out var txssrc5) && txssrc5.Id == 0)
				{
					txssrc5.User = spk.User;
				}
				else
				{
					var opus = this._opus.CreateDecoder();
					var vtx = new AudioSender(spk.Ssrc, opus)
					{
						User = await this._discord.GetUserAsync(spd.UserId.Value, true).ConfigureAwait(false)
					};

					if (!this._transmittingSsrCs.TryAdd(spk.Ssrc, vtx))
						this._opus.DestroyDecoder(opus);
				}

				await this._userSpeaking.InvokeAsync(this, spk).ConfigureAwait(false);
				break;

			case 6: // HEARTBEAT ACK
				var dt = DateTime.Now;
				var ping = (int)(dt - this._lastHeartbeat).TotalMilliseconds;
				Volatile.Write(ref this._wsPing, ping);
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received HEARTBEAT_ACK (OP6, {0}ms)", ping);
				this._lastHeartbeat = dt;
				break;

			case 8: // HELLO
					// this sends a heartbeat interval that we need to use for heartbeating
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received HELLO (OP8)");
				this._heartbeatInterval = opp["heartbeat_interval"].ToObject<int>();
				break;

			case 9: // RESUMED
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received RESUMED (OP9)");
				this._heartbeatTask = Task.Run(this.HeartbeatAsync);
				break;

			case 12: // CLIENT_CONNECTED
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received CLIENT_CONNECTED (OP12)");
				var ujpd = opp.ToObject<VoiceUserJoinPayload>();
				var usrj = await this._discord.GetUserAsync(ujpd.UserId, true).ConfigureAwait(false);
				{
					var opus = this._opus.CreateDecoder();
					var vtx = new AudioSender(ujpd.Ssrc, opus)
					{
						User = usrj
					};

					if (!this._transmittingSsrCs.TryAdd(vtx.Ssrc, vtx))
						this._opus.DestroyDecoder(opus);
				}

				await this._userJoined.InvokeAsync(this, new VoiceUserJoinEventArgs(this._discord.ServiceProvider) { User = usrj, Ssrc = ujpd.Ssrc }).ConfigureAwait(false);
				break;

			case 13: // CLIENT_DISCONNECTED
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received CLIENT_DISCONNECTED (OP13)");
				var ulpd = opp.ToObject<VoiceUserLeavePayload>();
				var txssrc = this._transmittingSsrCs.FirstOrDefault(x => x.Value.Id == ulpd.UserId);
				if (this._transmittingSsrCs.ContainsKey(txssrc.Key))
				{
					this._transmittingSsrCs.TryRemove(txssrc.Key, out var txssrc13);
					this._opus.DestroyDecoder(txssrc13.Decoder);
				}

				var usrl = await this._discord.GetUserAsync(ulpd.UserId, true).ConfigureAwait(false);
				await this._userLeft.InvokeAsync(this, new VoiceUserLeaveEventArgs(this._discord.ServiceProvider)
				{
					User = usrl,
					Ssrc = txssrc.Key
				}).ConfigureAwait(false);
				break;

			default:
				this._discord.Logger.LogTrace(VoiceNextEvents.VoiceDispatch, "Received unknown voice opcode (OP{0})", opc);
				break;
		}
	}

	/// <summary>
	/// Voices the w s_ socket closed.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private async Task VoiceWS_SocketClosed(IWebSocketClient client, SocketCloseEventArgs e)
	{
		this._discord.Logger.LogDebug(VoiceNextEvents.VoiceConnectionClose, "Voice WebSocket closed ({0}, '{1}')", e.CloseCode, e.CloseMessage);

		// generally this should not be disposed on all disconnects, only on requested ones
		// or something
		// otherwise problems happen
		//this.Dispose();

		if (e.CloseCode == 4006 || e.CloseCode == 4009)
			this.Resume = false;

		if (!this._isDisposed)
		{
			this._tokenSource.Cancel();
			this._tokenSource = new CancellationTokenSource();
			this._voiceWs = this._discord.Configuration.WebSocketClientFactory(this._discord.Configuration.Proxy, this._discord.ServiceProvider);
			this._voiceWs.Disconnected += this.VoiceWS_SocketClosed;
			this._voiceWs.MessageReceived += this.VoiceWS_SocketMessage;
			this._voiceWs.Connected += this.VoiceWS_SocketOpened;

			if (this.Resume) // emzi you dipshit
				await this.ConnectAsync().ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Voices the w s_ socket message.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task VoiceWS_SocketMessage(IWebSocketClient client, SocketMessageEventArgs e)
	{
		if (e is not SocketTextMessageEventArgs et)
		{
			this._discord.Logger.LogCritical(VoiceNextEvents.VoiceGatewayError, "Discord Voice Gateway sent binary data - unable to process");
			return Task.CompletedTask;
		}

		this._discord.Logger.LogTrace(VoiceNextEvents.VoiceWsRx, et.Message);
		return this.HandleDispatch(JObject.Parse(et.Message));
	}

	/// <summary>
	/// Voices the w s_ socket opened.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task VoiceWS_SocketOpened(IWebSocketClient client, SocketEventArgs e)
		=> this.StartAsync();

	/// <summary>
	/// Voices the ws_ socket exception.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task VoiceWs_SocketException(IWebSocketClient client, SocketErrorEventArgs e)
		=> this._voiceSocketError.InvokeAsync(this, new SocketErrorEventArgs(this._discord.ServiceProvider) { Exception = e.Exception });

	/// <summary>
	/// Ws the send async.
	/// </summary>
	/// <param name="payload">The payload.</param>
	/// <returns>A Task.</returns>
	private async Task WsSendAsync(string payload)
	{
		this._discord.Logger.LogTrace(VoiceNextEvents.VoiceWsTx, payload);
		await this._voiceWs.SendMessageAsync(payload).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets the unix timestamp.
	/// </summary>
	/// <param name="dt">The datetime.</param>
	private static uint UnixTimestamp(DateTime dt)
	{
		var ts = dt - s_unixEpoch;
		var sd = ts.TotalSeconds;
		var si = (uint)sd;
		return si;
	}
}
