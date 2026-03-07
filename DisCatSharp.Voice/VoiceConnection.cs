using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
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
using DisCatSharp.Voice.Codec;
using DisCatSharp.Voice.Dave;
using DisCatSharp.Voice.Entities;
using DisCatSharp.Voice.Entities.Dave;
using DisCatSharp.Voice.EventArgs;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Voice;

internal delegate Task VoiceDisconnectedEventHandler(DiscordGuild guild);

/// <summary>
///     Voice connection to a voice channel.
/// </summary>
public sealed class VoiceConnection : IDisposable
{
	/// <summary>
	///     Gets the configuration.
	/// </summary>
	private readonly VoiceConfiguration _configuration;

	/// <summary>
	///     Gets the discord.
	/// </summary>
	private readonly DiscordClient _discord;

	/// <summary>
	///     Gets the guild.
	/// </summary>
	private readonly DiscordGuild _guild;

	/// <summary>
	///     Gets the keepalive timestamps.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, long> _keepaliveTimestamps;

	/// <summary>
	///     Gets the pause event.
	/// </summary>
	private readonly AsyncManualResetEvent _pauseEvent;

	/// <summary>
	///     Gets or sets the ready wait.
	/// </summary>
	private readonly TaskCompletionSource<bool> _readyWait;

	/// <summary>
	///     Gets the transmit channel.
	/// </summary>
	private readonly Channel<RawVoicePacket> _transmitChannel;

	/// <summary>
	///     Serializes reconnect attempts.
	/// </summary>
	private readonly SemaphoreSlim _reconnectSemaphore = new(1, 1);

	/// <summary>
	///     Gets the transmitting s s r cs.
	/// </summary>
	private readonly ConcurrentDictionary<uint, AudioSender> _transmittingSsrCs;

	/// <summary>
	///     Gets the udp client.
	/// </summary>
	private readonly BaseUdpClient _udpClient;

	private readonly AsyncEvent<VoiceConnection, VoiceUserJoinEventArgs> _userJoined;

	private readonly AsyncEvent<VoiceConnection, VoiceUserLeaveEventArgs> _userLeft;

	private readonly AsyncEvent<VoiceConnection, UserSpeakingEventArgs> _userSpeaking;

	private readonly AsyncEvent<VoiceConnection, VoiceReceiveEventArgs> _voiceReceived;

	private readonly AsyncEvent<VoiceConnection, SocketErrorEventArgs> _voiceSocketError;

	/// <summary>
	///     Gets or sets the discovered endpoint.
	/// </summary>
	private IpEndpoint _discoveredEndpoint;

	/// <summary>
	///     Gets or sets the heartbeat interval.
	/// </summary>
	private int _heartbeatInterval;

	/// <summary>
	///     Gets or sets the heartbeat task.
	/// </summary>
	private Task _heartbeatTask;

	/// <summary>
	///     Gets or sets a value indicating whether is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	///     Gets or sets a value indicating whether is initialized.
	/// </summary>
	private bool _isInitialized;

	/// <summary>
	///     Gets or sets the keepalive task.
	/// </summary>
	private Task _keepaliveTask;

	/// <summary>
	///     Gets or sets the keepalive token source.
	/// </summary>
	private CancellationTokenSource _keepaliveTokenSource;

	/// <summary>
	///     Gets or sets the key.
	/// </summary>
	private byte[] _key;

	/// <summary>
	///     Gets or sets the last heartbeat.
	/// </summary>
	private DateTimeOffset _lastHeartbeat;

	/// <summary>
	///     Gets the last keepalive.
	/// </summary>
	private ulong _lastKeepalive;

	/// <summary>
	///     Sequence number of the last numbered message received from the voice gateway.
	///     Used for v8 seq_ack in heartbeats and resume payloads. -1 means no numbered message received yet.
	/// </summary>
	private volatile int _lastSeq = -1;

	/// <summary>
	///     Gets or sets the nonce.
	/// </summary>
	private uint _nonce;

	/// <summary>
	///     One-shot diagnostic flag: log AEAD receive parameters for the first incoming packet to aid debugging.
	/// </summary>
	private volatile int _aeadDiagLogged;

	/// <summary>
	///     One-shot diagnostic flag: log send pipeline parameters for the first outgoing packet to aid debugging.
	/// </summary>
	private volatile int _sendDiagLogged;

	/// <summary>
	///     One-shot diagnostic flag: log AEAD send parameters for the first outgoing AEAD packet to aid debugging.
	/// </summary>
	private volatile int _sendAeadDiagLogged;

	/// <summary>
	///     One-shot diagnostic flag: log DAVE encryption status on the first PreparePacket call when the DAVE session is non-null.
	/// </summary>
	private volatile int _davePrepDiagLogged;

	/// <summary>
	///     One-shot flag: log when outbound audio is dropped because DAVE is pending and
	///     <see cref="VoiceConfiguration.DavePendingAudioBehavior"/> is <see cref="DavePendingAudioBehavior.Drop"/>.
	/// </summary>
	private volatile int _daveInactiveDropDiagLogged;

	/// <summary>
	///     One-shot flag: log when inbound packets are dropped while DAVE is pending.
	/// </summary>
	private volatile int _daveRecvPendingDropDiagLogged;

	/// <summary>
	///     One-shot flag: log when inbound DAVE packets are dropped because SSRC→user mapping is not ready.
	/// </summary>
	private volatile int _daveRecvMissingSenderDropDiagLogged;

	/// <summary>
	///     One-shot flag: log when inbound DAVE packets are dropped because no ratchet exists for the sender.
	/// </summary>
	private volatile int _daveRecvMissingRatchetDropDiagLogged;

	/// <summary>
	///     One-shot guard: when OP 27 proposals produce no commit (ProcessProposals returns 0 bytes),
	///     the session is reset and a fresh OP 26 is sent exactly once per session-init cycle.
	///     Prevents stale <c>stateWithProposals_</c> corruption in libdave from cascading across
	///     multiple OP 27 arrivals.  Cleared when a new <see cref="DaveSession"/> is created or when
	///     proposals succeed.
	/// </summary>
	private bool _daveProposalRestartSent;


	/// <summary>
	///     Gets or sets the opus.
	/// </summary>
	private Opus _opus;

	/// <summary>
	///     Gets or sets the playing wait.
	/// </summary>
	private TaskCompletionSource<bool>? _playingWait;

	/// <summary>
	///     Gets the queue count.
	/// </summary>
	private int _queueCount;

	/// <summary>
	///     Gets or sets the receiver task.
	/// </summary>
	private Task _receiverTask;

	/// <summary>
	///     Gets or sets the receiver token source.
	/// </summary>
	private CancellationTokenSource _receiverTokenSource;

	/// <summary>
	///     Gets or sets the rtp.
	/// </summary>
	private Rtp _rtp;

	/// <summary>
	///     Gets or sets the selected encryption mode.
	/// </summary>
	private EncryptionMode _selectedEncryptionMode;

	/// <summary>
	///     Gets or sets the sender task.
	/// </summary>
	private Task _senderTask;

	/// <summary>
	///     Gets or sets the sender token source.
	/// </summary>
	private CancellationTokenSource _senderTokenSource;

	/// <summary>
	///     Gets or sets the sequence.
	/// </summary>
	private ushort _sequence;

	/// <summary>
	///     Gets or sets the sodium.
	/// </summary>
	private Sodium _sodium;

	/// <summary>The active DAVE session, or <c>null</c> when DAVE is not negotiated.</summary>
	private DaveSession? _daveSession;

	/// <summary>
	///     Saves the last speaking flag
	/// </summary>
	private SpeakingFlags _speakingFlags;

	/// <summary>
	///     Gets or sets the s s r c.
	/// </summary>
	private uint _ssrc;

	/// <summary>
	///     Gets or sets the timestamp.
	/// </summary>
	private uint _timestamp;

	/// <summary>
	///     Gets or sets the token source.
	/// </summary>
	private CancellationTokenSource _tokenSource;

	/// <summary>
	///     Gets or sets the transmit stream.
	/// </summary>
	private VoiceTransmitSink? _transmitStream;

	/// <summary>
	///     Gets the ping.
	/// </summary>
	private int _udpPing;

	/// <summary>
	///     Gets or sets the voice ws.
	/// </summary>
	private IWebSocketClient _voiceWs;

	private int _wsPing;

	/// <summary>
	///     Initializes a new instance of the <see cref="VoiceConnection" /> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="guild">The guild.</param>
	/// <param name="channel">The channel.</param>
	/// <param name="config">The config.</param>
	/// <param name="server">The server.</param>
	/// <param name="state">The state.</param>
	internal VoiceConnection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, VoiceConfiguration config, VoiceServerUpdatePayload server, VoiceStateUpdatePayload state)
	{
		this._discord = client;
		this._guild = guild;
		this.TargetChannel = channel;
		this._transmittingSsrCs = new();

		this._userSpeaking = new("VOICE_USER_SPEAKING", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._userJoined = new("VOICE_USER_JOINED", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._userLeft = new("VOICE_USER_LEFT", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._voiceReceived = new("VOICE_VOICE_RECEIVED", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._voiceSocketError = new("VOICE_WS_ERROR", TimeSpan.Zero, this._discord.EventErrorHandler);
		this._tokenSource = new();

		this._configuration = config;
		this._isInitialized = false;
		this._isDisposed = false;
		this._opus = new(this.AudioFormat);
		this._rtp = new();

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
			eph = eps;

		this.WebSocketEndpoint = new()
		{
			Hostname = eph,
			Port = epp
		};

		this._readyWait = new();

		this._playingWait = null;
		this._transmitChannel = Channel.CreateBounded<RawVoicePacket>(new BoundedChannelOptions(this._configuration.PacketQueueSize));
		this._keepaliveTimestamps = new();
		this._pauseEvent = new(true);

		this._udpClient = this._discord.Configuration.UdpClientFactory();
		this._voiceWs = this._discord.Configuration.WebSocketClientFactory(this._discord.Configuration.Proxy, this._discord.ServiceProvider);
		this._voiceWs.Disconnected += this.VoiceWS_SocketClosed;
		this._voiceWs.MessageReceived += this.VoiceWS_SocketMessage;
		this._voiceWs.Connected += this.VoiceWS_SocketOpened;
		this._voiceWs.ExceptionThrown += this.VoiceWs_SocketException;
	}

	/// <summary>
	///     Gets the unix epoch.
	/// </summary>
	private static DateTimeOffset s_unixEpoch { get; } = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

	/// <summary>
	///     Gets the token.
	/// </summary>
	private CancellationToken TOKEN
		=> this._tokenSource.Token;

	/// <summary>
	///     Gets or sets the server data.
	/// </summary>
	internal VoiceServerUpdatePayload ServerData { get; set; }

	/// <summary>
	///     Gets or sets the state data.
	/// </summary>
	internal VoiceStateUpdatePayload StateData { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether resume.
	/// </summary>
	internal bool Resume { get; set; }

	/// <summary>
	///     Gets or sets the web socket endpoint.
	/// </summary>
	internal ConnectionEndpoint WebSocketEndpoint { get; set; }

	/// <summary>
	///     Gets or sets the udp endpoint.
	/// </summary>
	internal ConnectionEndpoint UdpEndpoint { get; set; }

	/// <summary>
	///     Gets the sender token.
	/// </summary>
	private CancellationToken SENDER_TOKEN
		=> this._senderTokenSource.Token;

	/// <summary>
	///     Gets the receiver token.
	/// </summary>
	private CancellationToken RECEIVER_TOKEN
		=> this._receiverTokenSource.Token;

	/// <summary>
	///     Gets the keepalive token.
	/// </summary>
	private CancellationToken KEEPALIVE_TOKEN
		=> this._keepaliveTokenSource.Token;

	/// <summary>
	///     Gets the audio format used by the Opus encoder.
	/// </summary>
	public AudioFormat AudioFormat => this._configuration.AudioFormat;

	/// <summary>
	///     Gets whether this connection is still playing audio.
	/// </summary>
	public bool IsPlaying
		=> this._playingWait is { Task.IsCompleted: false };

	/// <summary>
	///     Gets the websocket round-trip time in ms.
	/// </summary>
	public int WebSocketPing
		=> Volatile.Read(ref this._wsPing);

	/// <summary>
	///     Gets the UDP round-trip time in ms.
	/// </summary>
	public int UdpPing
		=> Volatile.Read(ref this._udpPing);

	/// <summary>
	///     Gets the channel this voice client is connected to.
	/// </summary>
	public DiscordChannel TargetChannel { get; internal set; }

	/// <summary>
	///     Disconnects and disposes this voice connection.
	/// </summary>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

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
			this._discord.Logger.LogError(ex, "{Message}", ex.Message);
		}

		try
		{
			this._voiceWs.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			this._udpClient.Close();
		}
		catch
		{ }

		try
		{
			this._keepaliveTokenSource?.Cancel();
			this.ClearAudioSenders();
			this._tokenSource?.Dispose();
			this._senderTokenSource?.Dispose();
			this._receiverTokenSource?.Dispose();
			this._keepaliveTokenSource?.Dispose();
			this._opus?.Dispose();
			this._opus = null;
			this._sodium?.Dispose();
			this._sodium = null;
			this._daveSession?.Dispose();
			this._daveSession = null;
			this._rtp?.Dispose();
			this._rtp = null;
			this._reconnectSemaphore.Dispose();
		}
		catch (Exception ex)
		{
			this._discord.Logger.LogError(ex, "{Message}", ex.Message);
		}

		this.VoiceDisconnected?.Invoke(this._guild);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Triggered whenever a user speaks in the connected voice channel.
	/// </summary>
	public event AsyncEventHandler<VoiceConnection, UserSpeakingEventArgs> UserSpeaking
	{
		add => this._userSpeaking.Register(value);
		remove => this._userSpeaking.Unregister(value);
	}

	/// <summary>
	///     Triggered whenever a user joins voice in the connected guild.
	/// </summary>
	public event AsyncEventHandler<VoiceConnection, VoiceUserJoinEventArgs> UserJoined
	{
		add => this._userJoined.Register(value);
		remove => this._userJoined.Unregister(value);
	}

	/// <summary>
	///     Triggered whenever a user leaves voice in the connected guild.
	/// </summary>
	public event AsyncEventHandler<VoiceConnection, VoiceUserLeaveEventArgs> UserLeft
	{
		add => this._userLeft.Register(value);
		remove => this._userLeft.Unregister(value);
	}

	/// <summary>
	///     Triggered whenever voice data is received from the connected voice channel.
	/// </summary>
	public event AsyncEventHandler<VoiceConnection, VoiceReceiveEventArgs> VoiceReceived
	{
		add => this._voiceReceived.Register(value);
		remove => this._voiceReceived.Unregister(value);
	}

	/// <summary>
	///     Triggered whenever voice WebSocket throws an exception.
	/// </summary>
	public event AsyncEventHandler<VoiceConnection, SocketErrorEventArgs> VoiceSocketErrored
	{
		add => this._voiceSocketError.Register(value);
		remove => this._voiceSocketError.Unregister(value);
	}

	internal event VoiceDisconnectedEventHandler VoiceDisconnected;

	~VoiceConnection()
	{
		this.Dispose();
	}

	/// <summary>
	///     Connects to the specified voice channel.
	/// </summary>
	/// <returns>A task representing the connection operation.</returns>
	internal Task ConnectAsync()
	{
		// Voice gateway URL format: wss://{host}:{port}?v=8
		// The voice gateway does NOT support encoding=json (unlike the main gateway).
		// discord.net and koe both omit it; including it causes 4006 rejections.
		var gwuri = new UriBuilder
		{
			Scheme = "wss",
			Host = this.WebSocketEndpoint.Hostname,
			Port = this.WebSocketEndpoint.Port,
			Query = "v=8"
		};

		this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake, "[Voice] Connecting to voice WS: {Uri}", gwuri.Uri);
		return this._voiceWs.ConnectAsync(gwuri.Uri);
	}

	/// <summary>
	///     Reconnects .
	/// </summary>
	/// <returns>A Task.</returns>
	internal async Task ReconnectAsync()
	{
		if (this._isDisposed)
			return;

		try
		{
			await this._reconnectSemaphore.WaitAsync().ConfigureAwait(false);
		}
		catch (ObjectDisposedException)
		{
			return;
		}

		try
		{
			if (this._isDisposed)
				return;

			// Fresh session (e.g. channel/server move) should connect immediately.
			// Resume=true path intentionally closes first so SocketClosed can rebuild/reconnect.
			if (this.Resume)
				await this._voiceWs.DisconnectAsync().ConfigureAwait(false);
			else
				await this.ConnectAsync().ConfigureAwait(false);
		}
		finally
		{
			this._reconnectSemaphore.Release();
		}
	}

	/// <summary>
	///     Starts .
	/// </summary>
	/// <returns>A Task.</returns>
	internal async Task StartAsync()
	{
		// Let's announce our intentions to the server
		var vdp = new VoiceDispatch();

		// Capture whether this is a fresh session BEFORE mutating this.Resume,
		// so the log message below is correct.
		var isNewSession = !this.Resume;

		if (isNewSession)
		{
			vdp.OpCode = 0;
			this._lastSeq = -1;
			vdp.Payload = new VoiceIdentifyPayload
			{
				ServerId = this.ServerData.GuildId,
				UserId = this.StateData.UserId.Value,
				SessionId = this.StateData.SessionId,
				Token = this.ServerData.Token,
				MaxDaveProtocolVersion = this._configuration.MaxDaveProtocolVersion
				// seq_ack is NOT included in Identify (OP0)
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
				Token = this.ServerData.Token,
				MaxDaveProtocolVersion = this._configuration.MaxDaveProtocolVersion,
				// Voice gateway v8: Resume must include seq_ack for buffered-resume support.
				SequenceAck = this._lastSeq
			};
		}

		var vdj = JsonConvert.SerializeObject(vdp, Formatting.None);
		this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake, "[Voice] Sending {Op}: {Payload}", isNewSession ? "IDENTIFY (OP0)" : "RESUME (OP7)", vdj);
		await this.WsSendAsync(vdj).ConfigureAwait(false);
	}

	/// <summary>
	///     Waits the for ready async.
	/// </summary>
	/// <returns>A Task.</returns>
	internal Task WaitForReadyAsync()
		=> this._readyWait.Task;

	/// <summary>
	///     Enqueues the packet async.
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
	///     Prepares the packet.
	/// </summary>
	/// <param name="pcm">The pcm.</param>
	/// <param name="target">The target.</param>
	/// <param name="length">The length.</param>
	/// <returns>A bool.</returns>
	internal bool PreparePacket(ReadOnlySpan<byte> pcm, [NotNullWhen(true)] out byte[]? target, out int length)
	{
		target = null;
		length = 0;

		if (this._isDisposed)
		{
			this._discord.Logger.VoiceDebug(VoiceEvents.VoiceSendFailure, "[VoiceSend] PreparePacket early exit: {Reason}", "connection disposed");
			return false;
		}

		var audioFormat = this.AudioFormat;

		var packetArray = ArrayPool<byte>.Shared.Rent(this._rtp.CalculatePacketSize(audioFormat.SampleCountToSampleSize(audioFormat.CalculateMaximumFrameSize()), this._selectedEncryptionMode));
		var packet = packetArray.AsSpan();

		this._rtp.EncodeHeader(this._sequence, this._timestamp, this._ssrc, packet);
		var opus = packet.Slice(Rtp.HEADER_SIZE, pcm.Length);
		this._opus.Encode(pcm, ref opus);

		// DAVE E2EE: encrypt the Opus frame when the session is active.
		// With NullMlsProvider (Phase 5), IsActive is always false — this is a no-op.
		// Phase 6 will activate this path with a real MLS backend.
		// Pass this._ssrc so libdave embeds the correct sender SSRC in the SFrame header.

		// One-shot DAVE encrypt diagnostic: logs session state before the first TryEncrypt attempt.
		if (this._daveSession is not null && Interlocked.CompareExchange(ref this._davePrepDiagLogged, 1, 0) == 0)
		{
			this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake,
				"[DAVE] PreparePacket DAVE state: sessionNull={Null} state={State} isActive={Active}",
				false, this._daveSession.State, this._daveSession.IsActive);
		}

		if (this._daveSession is { IsActive: false } && this._configuration.DavePendingAudioBehavior == DavePendingAudioBehavior.Drop)
		{
			if (Interlocked.CompareExchange(ref this._daveInactiveDropDiagLogged, 1, 0) == 0)
				this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake,
					"[DAVE] Dropping outbound frame while session is pending (DavePendingAudioBehavior=Drop)");

			ArrayPool<byte>.Shared.Return(packetArray);
			return false;
		}

		byte[]? daveEncrypted = null;
		var daveEncryptSucceeded = false;
		var daveEncryptedLen = 0;
		try
		{
			if (this._daveSession is { IsActive: true }
				&& this._daveSession.TryEncrypt(opus, this._ssrc, out daveEncrypted, out var daveLen)
				&& daveLen <= packetArray.Length - Rtp.HEADER_SIZE)
			{
				daveEncryptSucceeded = true;
				daveEncryptedLen = daveLen;
				opus = packet.Slice(Rtp.HEADER_SIZE, daveLen);
				daveEncrypted.AsSpan(0, daveLen).CopyTo(opus);
			}
		}
		finally
		{
			if (daveEncrypted != null && daveEncrypted.Length > 0)
				ArrayPool<byte>.Shared.Return(daveEncrypted);
		}

		if (this._davePrepDiagLogged == 1)
		{
			this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake,
				"[DAVE] PreparePacket encrypt result: encryptCalled={Called} encryptedLen={Len}",
				daveEncryptSucceeded, daveEncryptedLen);
			this._davePrepDiagLogged = 2; // suppress further logs from this second one-shot
		}

		this._sequence++;
		this._timestamp += (uint)audioFormat.CalculateFrameSize(audioFormat.CalculateSampleDuration(pcm.Length));

		// AEAD transport encryption (aead_aes256_gcm_rtpsize / aead_xchacha20_poly1305_rtpsize)
		// Packet layout: [RTP header (AAD)] [ciphertext] [16-byte AEAD tag] [4-byte LE nonce counter]
		// Discord *_rtpsize AAD = exactly the fixed 12-byte RTP header. No CSRC entries.
		// Extension bytes are NOT in AAD — they are inside ciphertext.
		if (Sodium.IsAeadMode(this._selectedEncryptionMode))
		{
			var sendHeaderLength = Rtp.HEADER_SIZE; // Discord *_rtpsize: AAD is always the fixed 12-byte RTP header

			// Write the little-endian nonce counter into a 4-byte span and append it to the packet.
			// Discord appends the counter in little-endian byte order (confirmed via live packet capture:
			// counter value 2 arrives as [02 00 00 00], not [00 00 00 02]).
			Span<byte> nonceCounter4 = stackalloc byte[Sodium.AEAD_NONCE_SUFFIX_SIZE];
			BinaryPrimitives.WriteUInt32LittleEndian(nonceCounter4, this._nonce++);

			// Ciphertext overwrites the opus region in-place (same length as plaintext).
			var ciphertextDest = packet.Slice(sendHeaderLength, opus.Length);

			// Tag and counter suffix sit immediately after ciphertext.
			Span<byte> tag = stackalloc byte[Sodium.AES_GCM_TAG_SIZE];

			// Fixed 12-byte RTP header is the AAD — authenticated but not encrypted.
			this._sodium.EncryptAead(opus, ciphertextDest, tag, nonceCounter4, packet[..sendHeaderLength], this._selectedEncryptionMode);

			// Write [16-byte AEAD tag][4-byte LE nonce counter] after the ciphertext.
			var afterCipher = packet[(sendHeaderLength + opus.Length)..];
			tag.CopyTo(afterCipher);                                              // tag first (16 bytes)
			nonceCounter4.CopyTo(afterCipher[Sodium.AES_GCM_TAG_SIZE..]);        // counter after tag (4 bytes)

			// One-shot send-path diagnostic: log AEAD layout for the first outgoing AEAD packet.
			if (Interlocked.CompareExchange(ref this._sendAeadDiagLogged, 1, 0) == 0)
			{
				var counterVal = BinaryPrimitives.ReadUInt32LittleEndian(nonceCounter4);
				Span<byte> diagNonce = stackalloc byte[12];
				BinaryPrimitives.WriteUInt32LittleEndian(diagNonce, counterVal);

				// First 8 bytes of ciphertext (starts at sendHeaderLength in packet)
				var diagCipher = packet.Slice(sendHeaderLength, Math.Min(8, opus.Length));
				// Last 20 bytes of assembled packet: [16-byte tag][4-byte counter]
				var totalLen = sendHeaderLength + opus.Length + Sodium.AES_GCM_TAG_SIZE + Sodium.AEAD_NONCE_SUFFIX_SIZE;
				var diagLast20 = packet.Slice(totalLen - 20, 20);

				this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake,
					"[AEAD send diag] mode={Mode} packetLen={PktLen} aeadHdrLen={HdrLen} ciphertextLen={CLen} tagLen=16 counterValue={CVal} counter=[{CBytes}] nonce=[{NBytes}]",
					this._selectedEncryptionMode, totalLen, sendHeaderLength, opus.Length,
					counterVal,
					BitConverter.ToString(nonceCounter4.ToArray()).Replace("-", ""),
					BitConverter.ToString(diagNonce.ToArray()).Replace("-", ""));
				this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake,
					"[AEAD send diag] ciphertext first 8 bytes: {Bytes}",
					BitConverter.ToString(diagCipher.ToArray()));
				this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake,
					"[AEAD send diag] packet last 20 bytes (tag+counter): {Bytes}",
					BitConverter.ToString(diagLast20.ToArray()));
			}

			target = packetArray;
			length = sendHeaderLength + opus.Length + Sodium.AES_GCM_TAG_SIZE + Sodium.AEAD_NONCE_SUFFIX_SIZE;
			return true;
		}

		// Legacy XSalsa20 transport encryption
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
				throw new("Unsupported encryption mode.");
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
	///     Voices the sender task.
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task VoiceSenderTask()
	{
		var token = this.SENDER_TOKEN;
		var reader = this._transmitChannel.Reader;

		byte[]? data = null;
		var length = 0;

		var synchronizerTicks = (double)Stopwatch.GetTimestamp();
		var synchronizerResolution = Stopwatch.Frequency * 0.005;
		var tickResolution = 10_000_000.0 / Stopwatch.Frequency;
		this._discord.Logger.VoiceDebug(VoiceEvents.Misc, "Timer accuracy: {Frequency}/{SynchronizerResolution} (high resolution? {IsHighResolution})", Stopwatch.Frequency, synchronizerResolution, Stopwatch.IsHighResolution);

		while (!token.IsCancellationRequested)
		{
			try
			{
				await this._pauseEvent.WaitAsync().ConfigureAwait(false);
				var hasPacket = reader.TryRead(out var rawPacket);
				if (hasPacket)
				{
					this._queueCount--;

					if (this._playingWait == null || this._playingWait.Task.IsCompleted)
						this._playingWait = new();

					if (Interlocked.CompareExchange(ref this._sendDiagLogged, 1, 0) == 0)
						this._discord.Logger.VoiceDebug(VoiceEvents.Misc, "[VoiceSend] Dequeued PCM frame pcmLen={PcmLen}", rawPacket.Bytes.Length);
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
					var pcmLenBeforePrepare = rawPacket.Bytes.Length;
					hasPacket = this.PreparePacket(rawPacket.Bytes.Span, out data, out length);
					if (rawPacket.RentedBuffer is not null)
						ArrayPool<byte>.Shared.Return(rawPacket.RentedBuffer);

					if (this._sendDiagLogged == 1)
					{
						if (hasPacket)
							this._discord.Logger.VoiceDebug(VoiceEvents.Misc, "[VoiceSend] PreparePacket result={Result} packetLen={PacketLen}", hasPacket, length);
						else
							this._discord.Logger.VoiceDebug(VoiceEvents.VoiceSendFailure, "[VoiceSend] PreparePacket returned false — mode={Mode} pcmLen={PcmLen}", this._selectedEncryptionMode, pcmLenBeforePrepare);
					}
				}

				var durationModifier = hasPacket ? rawPacket.Duration / 5 : 4;
				var cts = Math.Max(Stopwatch.GetTimestamp() - synchronizerTicks, 0);
				if (cts < synchronizerResolution * durationModifier)
					await Task.Delay(TimeSpan.FromTicks((long)(((synchronizerResolution * durationModifier) - cts) * tickResolution)), CancellationToken.None).ConfigureAwait(false);

				synchronizerTicks += synchronizerResolution * durationModifier;

				if (!hasPacket)
					continue;

				await this.SendSpeakingAsync(this._speakingFlags is not SpeakingFlags.NotSpeaking ? this._speakingFlags : SpeakingFlags.Microphone).ConfigureAwait(false);
				ArgumentNullException.ThrowIfNull(data);
				if (this._sendDiagLogged == 1)
					this._discord.Logger.VoiceDebug(VoiceEvents.Misc, "[VoiceSend] UDP SendAsync packetLen={PacketLen} ssrc={Ssrc}", length, this._ssrc);
				await this._udpClient.SendAsync(data, length).ConfigureAwait(false);
				ArrayPool<byte>.Shared.Return(data);
				data = null;

				if (!rawPacket.Silence && this._queueCount is 0)
				{
					var nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
					for (var i = 0; i < 3; i++)
					{
						var nullpacket = new byte[nullpcm.Length];
						var nullpacketmem = nullpacket.AsMemory();
						await this.EnqueuePacketAsync(new(nullpacketmem, 20, true), CancellationToken.None).ConfigureAwait(false);
					}
				}
				else if (this._queueCount is 0)
				{
					this._speakingFlags = SpeakingFlags.NotSpeaking;
					await this.SendSpeakingAsync(this._speakingFlags).ConfigureAwait(false);
					this._playingWait?.SetResult(true);
				}
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				// Return any rented packet buffer before logging to avoid a pool leak.
				if (data != null)
				{
					ArrayPool<byte>.Shared.Return(data);
					data = null;
				}

				this._discord.Logger.LogError(VoiceEvents.VoiceSendFailure, ex, "Exception in voice sender task");
			}
		}
	}

	/// <summary>
	///     Processes the packet.
	/// </summary>
	/// <param name="data">The data.</param>
	/// <param name="opus">The opus.</param>
	/// <param name="pcm">The pcm.</param>
	/// <param name="pcmPackets">The pcm packets.</param>
	/// <param name="voiceSender">The voice sender.</param>
	/// <param name="outputFormat">The output format.</param>
	/// <returns>A bool.</returns>
	private bool ProcessPacket(ReadOnlySpan<byte> data, ref Memory<byte> opus, ref Memory<byte> pcm, List<ReadOnlyMemory<byte>> pcmPackets, [NotNullWhen(true)] out AudioSender? voiceSender, out AudioFormat outputFormat)
	{
		voiceSender = null;
		outputFormat = default;

		if (!this._rtp.IsRtpHeader(data))
			return false;

		this._rtp.DecodeHeader(data, out var shortSequence, out _, out var ssrc, out var hasExtension);

		// Discord *_rtpsize AAD = exactly the fixed 12-byte RTP header. No CSRC entries.
		// Extension bytes are NOT part of AAD — they are inside the ciphertext.
		var headerLength = Rtp.HEADER_SIZE; // Discord *_rtpsize: AAD is always the fixed 12-byte RTP header

		if (headerLength >= data.Length)
		{
			this._discord.Logger.LogWarning(VoiceEvents.VoiceReceiveFailure,
				"Dropping RTP packet: computed header length ({HeaderLength} bytes) >= packet length ({PacketLength} bytes)",
				headerLength, data.Length);
			return false;
		}

		if (!this._transmittingSsrCs.TryGetValue(ssrc, out var vtx))
		{
			var decoder = this._opus.CreateDecoder();

			vtx = new(ssrc, decoder)
			{
				// user isn't present as we haven't received a speaking event yet.
				User = null
			};

			this._transmittingSsrCs.TryAdd(ssrc, vtx);
		}

		voiceSender = vtx;
		var sequence = vtx.GetTrueSequenceAfterWrapping(shortSequence);
		ushort gap = 0;
		if (vtx.LastTrueSequence is { } lastTrueSequence)
		{
			if (sequence <= lastTrueSequence) // out-of-order packet; discard
				return false;

			gap = (ushort)(sequence - 1 - lastTrueSequence);
			if (gap >= 5)
				this._discord.Logger.LogWarning(VoiceEvents.VoiceReceiveFailure, "5 or more voice packets were dropped when receiving");
		}

		var opusSpan = Span<byte>.Empty;
		byte[]? daveDecrypted = null;
		try
		{
			if (Sodium.IsAeadMode(this._selectedEncryptionMode))
			{
				// AEAD receive path — rtpsize variants follow the SRTP §3.1 rule:
				//   Unencrypted (AAD) = 12-byte fixed RTP header
				//                     + 4-byte extension preamble (profile+length) IF X=1
				//   Ciphertext        = extension body (if X=1) + Opus payload
				//   Suffix            = [tag 16 bytes][counter 4 bytes]
				//
				// After decryption the extension body sits at the front of the plaintext and
				// must be stripped before passing the result to the Opus decoder.
				var aeadHeaderLen = Rtp.HEADER_SIZE; // 12 bytes for non-extended packets
				var extBodyLen = 0;

				if (hasExtension && data.Length > Rtp.HEADER_SIZE + 4)
				{
					// Extension preamble (profile 2 bytes + length-words 2 bytes) is authenticated
					// but NOT encrypted — include it in the AAD.
					// Extension body (length-words × 4 bytes) is encrypted as part of the ciphertext.
					aeadHeaderLen = Rtp.HEADER_SIZE + 4; // 16 bytes
					extBodyLen = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(14, 2)) * 4;
				}

				var minLen = aeadHeaderLen + Sodium.AES_GCM_TAG_SIZE + Sodium.AEAD_NONCE_SUFFIX_SIZE;
				if (data.Length <= minLen)
					return false;

				var ciphertextLen = data.Length - aeadHeaderLen - Sodium.AES_GCM_TAG_SIZE - Sodium.AEAD_NONCE_SUFFIX_SIZE;
				var ciphertext = data.Slice(aeadHeaderLen, ciphertextLen);
				var tag = data.Slice(data.Length - Sodium.AES_GCM_TAG_SIZE - Sodium.AEAD_NONCE_SUFFIX_SIZE, Sodium.AES_GCM_TAG_SIZE);
				var nonceCounter4 = data[^Sodium.AEAD_NONCE_SUFFIX_SIZE..];
				var aad = data[..aeadHeaderLen];

				// One-shot diagnostic: log AEAD receive layout for the first packet.
				if (Interlocked.CompareExchange(ref this._aeadDiagLogged, 1, 0) == 0)
				{
					var counterValue = BinaryPrimitives.ReadUInt32LittleEndian(nonceCounter4);
					Span<byte> diagNonce = stackalloc byte[12];
					BinaryPrimitives.WriteUInt32LittleEndian(diagNonce, counterValue);
					this._discord.Logger.VoiceDebug(VoiceEvents.VoiceDispatch,
						"[AEAD recv diag] mode={Mode} pktLen={PktLen} aeadHdrLen={HdrLen} hasExt={HasExt} extBodyLen={ExtBodyLen} " +
						"ciphertextLen={CiphertextLen} tagLen={TagLen} counterValue={CounterValue} " +
						"counter=[{C0:X2}{C1:X2}{C2:X2}{C3:X2}] " +
						"nonce=[{N0:X2}{N1:X2}{N2:X2}{N3:X2}{N4:X2}{N5:X2}{N6:X2}{N7:X2}{N8:X2}{N9:X2}{N10:X2}{N11:X2}] " +
						"aadLen={AadLen} keyLen={KeyLen}",
						this._selectedEncryptionMode, data.Length, aeadHeaderLen, hasExtension, extBodyLen,
						ciphertextLen, tag.Length, counterValue,
						nonceCounter4[0], nonceCounter4[1], nonceCounter4[2], nonceCounter4[3],
						diagNonce[0], diagNonce[1], diagNonce[2], diagNonce[3],
						diagNonce[4], diagNonce[5], diagNonce[6], diagNonce[7],
						diagNonce[8], diagNonce[9], diagNonce[10], diagNonce[11],
						aad.Length, this._sodium.KeyLength);

					var cipherStart = data[aeadHeaderLen..];
					var first8 = cipherStart.Length >= 8 ? cipherStart[..8].ToArray() : cipherStart.ToArray();
					this._discord.Logger.VoiceDebug(VoiceEvents.VoiceDispatch, "[AEAD recv diag] ciphertext first 8 bytes: {Bytes}", BitConverter.ToString(first8));

					var last20 = data[^Math.Min(20, data.Length)..].ToArray();
					this._discord.Logger.VoiceDebug(VoiceEvents.VoiceDispatch, "[AEAD recv diag] packet last 20 bytes (tag+counter): {Bytes}", BitConverter.ToString(last20));
				}

				opus = opus[..ciphertextLen];
				opusSpan = opus.Span;
				this._sodium.DecryptAead(ciphertext, opusSpan, tag, nonceCounter4, aad, this._selectedEncryptionMode);

				// Strip the decrypted extension body from the front of the plaintext.
				// When aeadHeaderLen=16 the preamble is in the AAD; the body is the first
				// extBodyLen bytes of the decrypted output and must be removed before Opus decode.
				if (extBodyLen > 0 && opusSpan.Length > extBodyLen)
					opusSpan = opusSpan[extBodyLen..];
			}
			else
			{
				// Legacy XSalsa20 receive path
				Span<byte> nonce = stackalloc byte[Sodium.NonceSize];
				this._sodium.GetNonce(data, nonce, this._selectedEncryptionMode);
				this._rtp.GetDataFromPacket(data, out var encryptedOpus, this._selectedEncryptionMode);

				var opusSize = Sodium.CalculateSourceSize(encryptedOpus);
				opus = opus[..opusSize];
				opusSpan = opus.Span;
				this._sodium.Decrypt(encryptedOpus, opusSpan, nonce);
			}

			// Receive pipeline ordering:
			// 1. RTP header parsed + CSRC-aware offset computed
			// 2. DAVE decrypt (operates on payload starting after RTP header; DAVE preserves the unencrypted extension bytes)
			// 3. RFC 5285 RTP extension stripped from decrypted payload
			// 4. Opus decode
			// This ordering must not be changed: DAVE encrypts from the payload start,
			// and extension data lives in the unencrypted portion that DAVE passes through unchanged.

			// DAVE E2EE: once DAVE is negotiated, we should only attempt Opus decode on successfully
			// decrypted SFrame payloads. Passing encrypted bytes to Opus causes invalid packets and
			// can destabilize receive.
			if (this._daveSession is not null)
			{
				if (!this._daveSession.IsActive)
				{
					if (Interlocked.CompareExchange(ref this._daveRecvPendingDropDiagLogged, 1, 0) == 0)
						this._discord.Logger.VoiceDebug(VoiceEvents.VoiceReceiveFailure,
							"[DAVE] Dropping inbound packet while session is pending");

					return false;
				}

				if (vtx.Id == 0)
				{
					if (Interlocked.CompareExchange(ref this._daveRecvMissingSenderDropDiagLogged, 1, 0) == 0)
						this._discord.Logger.VoiceDebug(VoiceEvents.VoiceReceiveFailure,
							"[DAVE] Dropping inbound packet for SSRC {Ssrc}: sender mapping not ready", ssrc);

					return false;
				}

				if (this._daveSession.TryDecrypt(vtx.Id, opusSpan, out daveDecrypted, out var daveLen))
					opusSpan = daveDecrypted.AsSpan(0, daveLen);
				else
				{
					if (Interlocked.CompareExchange(ref this._daveRecvMissingRatchetDropDiagLogged, 1, 0) == 0)
						this._discord.Logger.VoiceDebug(VoiceEvents.VoiceReceiveFailure,
							"[DAVE] Dropping inbound packet for SSRC {Ssrc}: no decryptor mapping for user {UserId}", ssrc, vtx.Id);

					return false;
				}
			}

			// Strip extensions, if any
			if (hasExtension)
				// RFC 5285, 4.2 One-Byte header
				// http://www.rfcreader.com/#rfc5285_line186
				if (opusSpan.Length >= 4 && opusSpan[0] is 0xBE && opusSpan[1] is 0xDE)
				{
					var extWords = (opusSpan[2] << 8) | opusSpan[3];
					var extBytes = extWords * 4;
					var extEnd = 4 + extBytes;
					if (extEnd > opusSpan.Length)
					{
						this._discord.Logger.VoiceDebug(VoiceEvents.VoiceReceiveFailure,
							"[VoiceRecv] Dropping packet with malformed RTP extension length: ssrc={Ssrc} seq={Seq} extEnd={ExtEnd} payloadLen={PayloadLen}",
							ssrc, sequence, extEnd, opusSpan.Length);
						return false;
					}

					var i = 4;
					while (i < extEnd)
					{
						var extByte = opusSpan[i];

						// 0 means padding between elements.
						if (extByte == 0)
						{
							i++;
							continue;
						}

						var id = (byte)(extByte >> 4);
						var len = (extByte & 0x0F) + 1;

						// 0xF is reserved by RFC 5285; skip defensively.
						if (id == 0x0F)
						{
							i++;
							continue;
						}

						i++;
						if (i + len > extEnd)
						{
							this._discord.Logger.VoiceDebug(VoiceEvents.VoiceReceiveFailure,
								"[VoiceRecv] Dropping packet with malformed RTP extension element: ssrc={Ssrc} seq={Seq} extIndex={Index} extEnd={ExtEnd}",
								ssrc, sequence, i, extEnd);
							return false;
						}

						i += len;
					}

					// Strip extension padding too
					while (i < opusSpan.Length && opusSpan[i] is 0)
						i++;

					if (i >= opusSpan.Length)
					{
						this._discord.Logger.VoiceDebug(VoiceEvents.VoiceReceiveFailure,
							"[VoiceRecv] Dropping packet with no Opus payload after RTP extension strip: ssrc={Ssrc} seq={Seq}",
							ssrc, sequence);
						return false;
					}

					opusSpan = opusSpan[i..];
				}

			// TODO: consider implementing RFC 5285, 4.3. Two-Byte Header
			if (opusSpan.Length >= 2 && opusSpan[0] is 0x90)
				// I'm not 100% sure what this header is/does, however removing the data causes no
				// real issues, and has the added benefit of removing a lot of noise.
				opusSpan = opusSpan[2..];

			switch (gap)
			{
				case 1:
				{
					var lastSampleCount = this._opus.GetLastPacketSampleCount(vtx.Decoder);
					var fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
					var fecpcmMem = fecpcm.AsSpan();
					this._opus.Decode(vtx.Decoder, opusSpan, ref fecpcmMem, true, out _);
					pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
					break;
				}
				case > 1:
				{
					var lastSampleCount = this._opus.GetLastPacketSampleCount(vtx.Decoder);
					for (var i = 0; i < gap; i++)
					{
						var fecpcm = new byte[this.AudioFormat.SampleCountToSampleSize(lastSampleCount)];
						var fecpcmMem = fecpcm.AsSpan();
						this._opus.ProcessPacketLoss(vtx.Decoder, lastSampleCount, ref fecpcmMem);
						pcmPackets.Add(fecpcm.AsMemory(0, fecpcmMem.Length));
					}

					break;
				}
			}

			var pcmSpan = pcm.Span;
			try
			{
				this._opus.Decode(vtx.Decoder, opusSpan, ref pcmSpan, false, out outputFormat);
			}
			catch (Exception ex)
			{
				this._discord.Logger.VoiceDebug(VoiceEvents.VoiceReceiveFailure, ex,
					"[VoiceRecv] Dropping undecodable Opus packet: ssrc={Ssrc} seq={Seq} len={Len}",
					ssrc, sequence, opusSpan.Length);
				return false;
			}

			pcm = pcm[..pcmSpan.Length];
		}
		finally
		{
			vtx.LastTrueSequence = sequence;
			if (daveDecrypted != null && daveDecrypted.Length > 0)
				ArrayPool<byte>.Shared.Return(daveDecrypted);
		}

		return true;
	}

	/// <summary>
	///     Processes the voice packet.
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
				await this._voiceReceived.InvokeAsync(this, new(this._discord.ServiceProvider)
				{
					Ssrc = vtx.Ssrc,
					User = vtx.User,
					PcmData = pcmFiller,
					OpusData = Array.Empty<byte>().AsMemory(),
					AudioFormat = audioFormat,
					AudioDuration = audioFormat.CalculateSampleDuration(pcmFiller.Length)
				}).ConfigureAwait(false);

			await this._voiceReceived.InvokeAsync(this, new(this._discord.ServiceProvider)
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
			this._discord.Logger.LogError(VoiceEvents.VoiceReceiveFailure, ex, "Exception occurred when decoding incoming audio data");
		}
	}

	/// <summary>
	///     Processes the keepalive.
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
			this._discord.Logger.VoiceDebug(VoiceEvents.VoiceKeepalive, "Received UDP keepalive {KeepAlive} (ping {TimeDelta}ms)", keepalive, tdelta);
			Volatile.Write(ref this._udpPing, tdelta);
		}
		catch (Exception ex)
		{
			this._discord.Logger.LogError(VoiceEvents.VoiceKeepalive, ex, "Exception occurred when handling keepalive");
		}
	}

	/// <summary>
	///     Udps the receiver task.
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task UdpReceiverTask()
	{
		var token = this.RECEIVER_TOKEN;

		while (!token.IsCancellationRequested)
		{
			try
			{
				var data = await this._udpClient.ReceiveAsync(token).ConfigureAwait(false);
				if (data.Length is 8)
					this.ProcessKeepalive(data);
				else if (this._configuration.EnableIncoming)
					await this.ProcessVoicePacket(data).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				this._discord.Logger.LogError(VoiceEvents.VoiceReceiveFailure, ex, "Exception in UDP receiver task");
			}
		}
	}

	/// <summary>
	///     Sends a speaking status to the connected voice channel.
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
					Delay = 0,
					Ssrc = this._ssrc
				}
			};

			var plj = JsonConvert.SerializeObject(pld, Formatting.None);
			await this.WsSendAsync(plj).ConfigureAwait(false);
			this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake, "[Voice] Sent Speaking payload: speaking={Flags} ssrc={Ssrc}", flags, this._ssrc);
		}
	}

	/// <summary>
	///     Gets a transmit stream for this connection, optionally specifying a packet size to use with the stream. If a stream
	///     is already configured, it will return the existing one.
	/// </summary>
	/// <param name="sampleDuration">Duration, in ms, to use for audio packets.</param>
	/// <returns>Transmit stream.</returns>
	public VoiceTransmitSink GetTransmitSink(int sampleDuration = 20)
	{
		if (!AudioFormat.AllowedSampleDurations.Contains(sampleDuration))
			throw new ArgumentOutOfRangeException(nameof(sampleDuration), "Invalid PCM sample duration specified.");

		this._transmitStream ??= new(this, sampleDuration);

		return this._transmitStream;
	}

	/// <summary>
	///     Asynchronously waits for playback to be finished. Playback is finished when speaking = false is signaled.
	/// </summary>
	/// <returns>A task representing the waiting operation.</returns>
	public async Task WaitForPlaybackFinishAsync()
	{
		if (this._playingWait is not null)
			await this._playingWait.Task.ConfigureAwait(false);
	}

	/// <summary>
	///     Pauses playback.
	/// </summary>
	public void Pause()
		=> this._pauseEvent.Reset();

	/// <summary>
	///     Asynchronously resumes playback.
	/// </summary>
	/// <returns></returns>
	public async Task ResumeAsync()
		=> await this._pauseEvent.SetAsync().ConfigureAwait(false);

	/// <summary>
	///     Disconnects and disposes this voice connection.
	/// </summary>
	public void Disconnect()
		=> this.Dispose();

	/// <summary>
	///     Heartbeats .
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task HeartbeatAsync()
	{
		await Task.Yield();

		var token = this.TOKEN;
		while (true)
			try
			{
				token.ThrowIfCancellationRequested();

				var dt = DateTime.Now;
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceHeartbeat, "Sent heartbeat");

				var hbd = new VoiceDispatch
				{
					OpCode = 3,
					// Voice gateway v8: heartbeat must be {"t": nonce, "seq_ack": lastSeq}
					// rather than a bare integer. seq_ack is the last sequence-numbered
					// message received from the server; -1 if none received yet.
					Payload = new VoiceHeartbeatPayload
					{
						Nonce = UnixTimestamp(dt),
						SequenceAck = this._lastSeq
					}
				};
				var hbj = JsonConvert.SerializeObject(hbd);
				await this.WsSendAsync(hbj).ConfigureAwait(false);

				this._lastHeartbeat = dt;
				await Task.Delay(this._heartbeatInterval, CancellationToken.None).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				return;
			}
	}

	/// <summary>
	///     Keepalives .
	/// </summary>
	/// <returns>A Task.</returns>
	private async Task KeepaliveAsync()
	{
		await Task.Yield();

		var token = this.KEEPALIVE_TOKEN;

		while (!token.IsCancellationRequested)
		{
			var timestamp = Stopwatch.GetTimestamp();
			var keepalive = Volatile.Read(ref this._lastKeepalive);
			Volatile.Write(ref this._lastKeepalive, keepalive + 1);
			this._keepaliveTimestamps.TryAdd(keepalive, timestamp);

			var packet = new byte[8];
			BinaryPrimitives.WriteUInt64LittleEndian(packet, keepalive);

			await this._udpClient.SendAsync(packet, packet.Length).ConfigureAwait(false);

			await Task.Delay(5000, token).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Stage1S .
	/// </summary>
	/// <param name="voiceReady">The voice ready.</param>
	/// <returns>A Task.</returns>
	private async Task Stage1(VoiceReadyPayload voiceReady)
	{
		// IP Discovery
		this._udpClient.Setup(this.UdpEndpoint);
		this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake,
			"[Voice] Stage1 UDP discovery started: endpoint={Host}:{Port}",
			this.UdpEndpoint.Hostname, this.UdpEndpoint.Port);

		var pck = new byte[74];
		byte[]? ipd = null;
		for (var attempt = 1; attempt <= 3; attempt++)
		{
			PreparePacketStage1(pck);
			await this._udpClient.SendAsync(pck, pck.Length).ConfigureAwait(false);
			this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake,
				"[Voice] Stage1 UDP discovery probe sent (attempt {Attempt}/3)", attempt);

			try
			{
				using var discoveryTimeout = CancellationTokenSource.CreateLinkedTokenSource(this.TOKEN);
				discoveryTimeout.CancelAfter(TimeSpan.FromSeconds(3));
				ipd = await this._udpClient.ReceiveAsync(discoveryTimeout.Token).ConfigureAwait(false);
				break;
			}
			catch (OperationCanceledException) when (!this.TOKEN.IsCancellationRequested)
			{
				this._discord.Logger.LogWarning(VoiceEvents.VoiceHandshake,
					"[Voice] Stage1 UDP discovery timed out (attempt {Attempt}/3)", attempt);
			}
		}

		if (ipd is null)
			throw new TimeoutException("Voice UDP endpoint discovery timed out after 3 attempts.");

		ReadPacket(ipd, out var ip, out var port);
		this._discoveredEndpoint = new()
		{
			Address = ip,
			Port = port
		};
		this._discord.Logger.VoiceTrace(VoiceEvents.VoiceHandshake, "Endpoint discovery finished - discovered endpoint is {IP}:{Port}", ip, port);

		// Select voice encryption mode
		var selectedEncryptionMode = Sodium.SelectMode(voiceReady.Modes);
		this._selectedEncryptionMode = selectedEncryptionMode.Value;

		// Ready
		this._discord.Logger.VoiceTrace(VoiceEvents.VoiceHandshake, "Selected encryption mode is {EncryptionMode}", selectedEncryptionMode.Key);
		var vsp = new VoiceDispatch
		{
			OpCode = 1,
			Payload = new VoiceSelectProtocolPayload
			{
				Protocol = "udp",
				Data = new()
				{
					Address = this._discoveredEndpoint.Address.ToString(),
					Port = (ushort)this._discoveredEndpoint.Port,
					Mode = selectedEncryptionMode.Key
				}
			}
		};
		var vsj = JsonConvert.SerializeObject(vsp, Formatting.None);
		await this.WsSendAsync(vsj).ConfigureAwait(false);

		this._senderTokenSource = new();
		this._senderTask = Task.Run(this.VoiceSenderTask, this.SENDER_TOKEN);

		this._receiverTokenSource = new();
		this._receiverTask = Task.Run(this.UdpReceiverTask, this.RECEIVER_TOKEN);
		return;

		void ReadPacket(byte[] packet, out IPAddress decodedIp, out ushort decodedPort)
		{
			var packetSpan = packet.AsSpan();

			var ipString = Utilities.UTF8.GetString(packet, 8, 64 /* 74 - 10 */).TrimEnd('\0');
			decodedIp = IPAddress.Parse(ipString);

			decodedPort = BinaryPrimitives.ReadUInt16BigEndian(packetSpan[72..] /* 74 - 2 */);
		}

		void PreparePacketStage1(byte[] packet)
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
	}

	/// <summary>
	///     Stage2S .
	/// </summary>
	/// <param name="voiceSessionDescription">The voice session description.</param>
	/// <returns>A Task.</returns>
	private async Task Stage2(VoiceSessionDescriptionPayload voiceSessionDescription)
	{
		this._selectedEncryptionMode = Sodium.SupportedModes[voiceSessionDescription.Mode.ToLowerInvariant()];
		this._discord.Logger.VoiceTrace(VoiceEvents.VoiceHandshake, "Discord updated encryption mode - new mode is {EncryptionMode}", this._selectedEncryptionMode);
		this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake, "[Voice] Selected encryption mode for send: {Mode}", this._selectedEncryptionMode);

		// start keepalive
		this._keepaliveTokenSource = new();
		this._keepaliveTask = this.KeepaliveAsync();

		// send 3 packets of silence to get things going
		var nullpcm = new byte[this.AudioFormat.CalculateSampleSize(20)];
		for (var i = 0; i < 3; i++)
		{
			var nullPcm = new byte[nullpcm.Length];
			var nullpacketmem = nullPcm.AsMemory();
			await this.EnqueuePacketAsync(new(nullpacketmem, 20, true), CancellationToken.None).ConfigureAwait(false);
		}

		this._isInitialized = true;
		this._readyWait.TrySetResult(true);
	}

	/// <summary>
	///     Handles the dispatch.
	/// </summary>
	/// <param name="jo">The jo.</param>
	/// <returns>A Task.</returns>
	private async Task HandleDispatch(JObject jo)
	{
		var opc = (int?)jo["op"];
		ArgumentNullException.ThrowIfNull(opc);
		var opp = jo["d"] as JObject;

		// Voice gateway v8: track the last sequence number for seq_ack heartbeats and buffered resume.
		var msgSeq = (int?)jo["seq"];
		if (msgSeq.HasValue)
			this._lastSeq = msgSeq.Value;

		switch (opc)
		{
			case 2: // READY
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received READY (OP2)");
				ArgumentNullException.ThrowIfNull(opp);
				var vrp = opp.ToObject<VoiceReadyPayload>()!;
				this._ssrc = vrp.Ssrc;
				this.UdpEndpoint = new(vrp.Address, vrp.Port);
				// this is not the valid interval
				// oh, discord
				//this.HeartbeatInterval = vrp.HeartbeatInterval;
				this._heartbeatTask = Task.Run(this.HeartbeatAsync, CancellationToken.None);
				await this.Stage1(vrp).ConfigureAwait(false);
				break;

			case 4: // SESSION_DESCRIPTION
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received SESSION_DESCRIPTION (OP4)");
				ArgumentNullException.ThrowIfNull(opp);
				var vsd = opp.ToObject<VoiceSessionDescriptionPayload>()!;
				this._key = vsd.SecretKey;
				this._sodium = new(this._key.AsMemory(), this._discord.Logger);
				// Create a DAVE session if the server has activated DAVE for this channel.
				if (vsd.DaveProtocolVersion > 0)
				{
					this._daveSession?.Dispose();
					this._daveSession = null;
					try
					{
						// MLS group ID must be the voice CHANNEL ID, not the guild ID.
						// Discord derives the MLS group_id from the channel ID; using the guild ID
						// causes "PublicMessage not for this group" when processing OP27 proposals.
						var voiceChannelId = this.TargetChannel.Id;
						this._daveSession = new DaveSession(
							selfUserId: this.StateData.UserId ?? 0UL,
							channelId: voiceChannelId,
							protocolVersion: vsd.DaveProtocolVersion,
							mlsProvider: new LibDaveMlsProvider(
								authSessionId: this._discord.SessionId ?? string.Empty,
								channelId: voiceChannelId,
								protocolVersion: vsd.DaveProtocolVersion,
								logger: this._discord.Logger),
							encryptorFactory: () => new LibDaveEncryptor(),
							decryptorFactory: () => new LibDaveDecryptor(),
							logger: this._discord.Logger);
						this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] Session created for channel {ChannelId}, protocol version {Version}", voiceChannelId, vsd.DaveProtocolVersion);
						// Pre-seed recognised users from the guild voice-state cache so ADD proposals for
						// already-present channel members are not rejected when OP 27 arrives before OP 11.
						var preSeedIds = this._guild.VoiceStates
							.Where(kv => kv.Value.ChannelId == voiceChannelId && kv.Value.UserId != (this.StateData.UserId ?? 0UL))
							.Select(kv => kv.Value.UserId);
						this._daveSession.PreSeedRecognizedUsers(preSeedIds);
						this._daveProposalRestartSent = false;
						this._daveInactiveDropDiagLogged = 0;
						this._daveRecvPendingDropDiagLogged = 0;
						this._daveRecvMissingSenderDropDiagLogged = 0;
						this._daveRecvMissingRatchetDropDiagLogged = 0;
					}
					catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
					{
						// libdave is not available on this platform/RID.  Disable DAVE gracefully
						// rather than crashing the voice connection.  Audio will continue unencrypted.
						this._daveSession = null;
						this._daveInactiveDropDiagLogged = 0;
						this._daveRecvPendingDropDiagLogged = 0;
						this._daveRecvMissingSenderDropDiagLogged = 0;
						this._daveRecvMissingRatchetDropDiagLogged = 0;
						this._discord.Logger.LogError(VoiceEvents.DaveHandshake,
							"[DAVE] Native libdave library not found or entry point missing — DAVE disabled for this connection. ({ExType}: {Message})",
							ex.GetType().Name, ex.Message);
					}
				}
				else
				{
					// Non-DAVE channel — dispose any stale session
					this._daveSession?.Dispose();
					this._daveSession = null;
					this._daveInactiveDropDiagLogged = 0;
					this._daveRecvPendingDropDiagLogged = 0;
					this._daveRecvMissingSenderDropDiagLogged = 0;
					this._daveRecvMissingRatchetDropDiagLogged = 0;
				}

				await this.Stage2(vsd).ConfigureAwait(false);
				break;

			case 5: // SPEAKING
					// Don't spam OP5
					// No longer spam, Discord supposedly doesn't send many of these
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received SPEAKING (OP5)");
				ArgumentNullException.ThrowIfNull(opp);
				var spd = opp.ToObject<VoiceSpeakingPayload>()!;
				ArgumentNullException.ThrowIfNull(spd.Ssrc);
				DiscordUser? resolvedUser = null;
				if (spd.UserId.HasValue && !this._discord.TryGetCachedUserInternal(spd.UserId.Value, out resolvedUser))
					resolvedUser = await this._discord.GetUserAsync(spd.UserId.Value, true).ConfigureAwait(false);

				var spk = new UserSpeakingEventArgs(this._discord.ServiceProvider)
				{
					Speaking = spd.Speaking,
					Ssrc = spd.Ssrc.Value,
					User = resolvedUser
				};

				if (this._transmittingSsrCs.TryGetValue(spk.Ssrc, out var existingSender))
				{
					// Sender can be created by inbound RTP before OP5 arrives. Bind user id now.
					if (existingSender.Id == 0 && spk.User is not null)
						existingSender.User = spk.User;
				}
				else
				{
					var opus = this._opus.CreateDecoder();
					var vtx = new AudioSender(spk.Ssrc, opus)
					{
						User = spk.User
					};

					if (!this._transmittingSsrCs.TryAdd(spk.Ssrc, vtx))
					{
						this._opus.DestroyDecoder(opus);
						if (spk.User is not null && this._transmittingSsrCs.TryGetValue(spk.Ssrc, out var racedSender) && racedSender.Id == 0)
							racedSender.User = spk.User;
					}
				}

				await this._userSpeaking.InvokeAsync(this, spk).ConfigureAwait(false);
				break;

			case 6: // HEARTBEAT ACK
				var dt = DateTime.Now;
				var ping = (int)(dt - this._lastHeartbeat).TotalMilliseconds;
				Volatile.Write(ref this._wsPing, ping);
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received HEARTBEAT_ACK (OP6, {Ping}ms)", ping);
				this._lastHeartbeat = dt;
				break;

			case 8: // HELLO
					// this sends a heartbeat interval that we need to use for
					// ArgumentNullException.ThrowIfNull(opp);
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received HELLO (OP8)");
				ArgumentNullException.ThrowIfNull(opp);
				this._heartbeatInterval = opp["heartbeat_interval"].ToObject<int>();
				break;

			case 9: // RESUMED
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received RESUMED (OP9)");
				this._heartbeatTask = Task.Run(this.HeartbeatAsync, CancellationToken.None);
				break;

			case 12: // CLIENT_CONNECTED
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received CLIENT_CONNECTED (OP12)");
				ArgumentNullException.ThrowIfNull(opp);
				var ujpd = opp.ToObject<VoiceUserJoinPayload>()!;
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

				await this._userJoined.InvokeAsync(this, new(this._discord.ServiceProvider)
				{
					User = usrj,
					Ssrc = ujpd.Ssrc
				}).ConfigureAwait(false);
				break;

			case 13: // CLIENT_DISCONNECTED
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received CLIENT_DISCONNECTED (OP13)");
				ArgumentNullException.ThrowIfNull(opp);
				var ulpd = opp.ToObject<VoiceUserLeavePayload>()!;
				var txssrc = this._transmittingSsrCs.FirstOrDefault(x => x.Value.Id == ulpd.UserId);
				if (this._transmittingSsrCs.ContainsKey(txssrc.Key))
				{
					this._transmittingSsrCs.TryRemove(txssrc.Key, out var txssrc13);
					this._opus.DestroyDecoder(txssrc13?.Decoder);
				}

				this._daveSession?.RemoveUser(ulpd.UserId);
				var usrl = await this._discord.GetUserAsync(ulpd.UserId, true).ConfigureAwait(false);
				await this._userLeft.InvokeAsync(this, new(this._discord.ServiceProvider)
				{
					User = usrl,
					Ssrc = txssrc.Key
				}).ConfigureAwait(false);
				break;

			case 11: // DAVE CLIENTS_CONNECT
				this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received DAVE CLIENTS_CONNECT (OP11)");
				this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP11 received");
				if (opp is not null && this._daveSession is not null)
				{
					var ccp = opp.ToObject<VoiceClientsConnectPayload>();
					if (ccp is not null)
						this._daveSession.HandleClientsConnect(ccp);
				}

				break;

			case 21: // DAVE MLS_PREPARE_TRANSITION
				this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received DAVE MLS_PREPARE_TRANSITION (OP21)");
				this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP21 received");
				if (opp is not null && this._daveSession is not null)
				{
					var ptp = opp.ToObject<DavePrepareTransitionPayload>();
					if (ptp is not null)
						this._daveSession.HandlePrepareTransition(ptp);
				}

				break;

			case 22: // DAVE MLS_EXECUTE_TRANSITION
				this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received DAVE MLS_EXECUTE_TRANSITION (OP22)");
				this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP22 received");
				if (opp is not null && this._daveSession is not null)
				{
					var etp = opp.ToObject<DaveExecuteTransitionPayload>();
					if (etp is not null)
					{
						var ack = this._daveSession.HandleExecuteTransition(etp);
						if (ack is not null)
						{
							this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP23 sent");
							await this.WsSendAsync(JsonConvert.SerializeObject(new VoiceDispatch { OpCode = 23, Payload = ack }, Formatting.None)).ConfigureAwait(false);
						}
					}
				}

				break;

			case 24: // DAVE MLS_PREPARE_EPOCH
				this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received DAVE MLS_PREPARE_EPOCH (OP24)");
				this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP24 received");
				if (opp is not null && this._daveSession is not null)
				{
					var pep = opp.ToObject<DavePrepareEpochPayload>();
					if (pep is not null)
					{
						this._daveSession.HandlePrepareEpoch(pep);
						// When epoch == 1 (MLS_NEW_GROUP_EXPECTED_EPOCH), the server is starting a fresh
						// group. We must re-init and re-send our key package (OP26), mirroring the
						// canonical onDaveProtocolPrepareEpoch(epoch=1) → Init + sendMLSKeyPackage flow.
						if (pep.Epoch == 1)
						{
							this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] OP24 epoch=1 (new group expected), re-initialising and re-sending OP26");
							var kp24 = this._daveSession.PrepareKeyPackage();
							if (kp24.Length > 0)
							{
								this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] Sending key package OP26 ({Len} bytes) from OP24 handler", kp24.Length);
								this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP26 sent");
								await this.WsSendBinaryAsync(BuildDaveBinaryMessage(26, kp24)).ConfigureAwait(false);
							}
						}
					}
				}

				break;

			case 31: // DAVE MLS_INVALID_COMMIT_WELCOME
				this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received DAVE MLS_INVALID_COMMIT_WELCOME (OP31)");
				this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP31 received");
				if (this._daveSession is not null)
				{
					var icp = opp?.ToObject<DaveMlsInvalidCommitWelcomePayload>();
					this._daveSession.HandleInvalidCommit(icp);
				}

				break;

			default:
				this._discord.Logger.VoiceTrace(VoiceEvents.VoiceDispatch, "Received unknown voice opcode (OP{OpCode})", opc);
				break;
		}
	}

	/// <summary>
	///     Voices the w s_ socket closed.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private async Task VoiceWS_SocketClosed(IWebSocketClient client, SocketCloseEventArgs e)
	{
		this._discord.Logger.VoiceDebug(VoiceEvents.VoiceConnectionClose, "Voice WebSocket closed ({Code}, '{Message}')", e.CloseCode, e.CloseMessage ?? "No reason given");

		// generally this should not be disposed on all disconnects, only on requested ones
		// or something
		// otherwise problems happen
		//this.Dispose();

		var shouldAutoReconnect = this.Resume;
		if (e.CloseCode is 4006 or 4009)
		{
			this.Resume = false;
			shouldAutoReconnect = true;
		}
		else if (e.CloseCode is 4014 or 4022)
		{
			// Voice server move/channel move/region migration. Wait for VOICE_SERVER_UPDATE
			// before reconnecting, otherwise we can reconnect to a stale endpoint and get stuck.
			this.Resume = false;
			shouldAutoReconnect = false;
		}

		if (!this._isDisposed)
		{
			this._tokenSource.Cancel();
			this._senderTokenSource?.Cancel();
			this._receiverTokenSource?.Cancel();
			this._keepaliveTokenSource?.Cancel();
			this._isInitialized = false;
			this.ClearAudioSenders();
			this._daveSession?.Reset();
			this._tokenSource = new();
			this._voiceWs = this._discord.Configuration.WebSocketClientFactory(this._discord.Configuration.Proxy, this._discord.ServiceProvider);
			this._voiceWs.Disconnected += this.VoiceWS_SocketClosed;
			this._voiceWs.MessageReceived += this.VoiceWS_SocketMessage;
			this._voiceWs.Connected += this.VoiceWS_SocketOpened;
			this._voiceWs.ExceptionThrown += this.VoiceWs_SocketException;

			if (shouldAutoReconnect)
				await this.ConnectAsync().ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Voices the w s_ socket message.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task VoiceWS_SocketMessage(IWebSocketClient client, SocketMessageEventArgs e)
	{
		if (e is SocketTextMessageEventArgs et)
		{
			this._discord.Logger.VoiceTrace(VoiceEvents.VoiceWsRx, "{Message}", et.Message);
			return this.HandleDispatch(JObject.Parse(et.Message));
		}
		else if (e is SocketBinaryMessageEventArgs eb)
		{
			this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received binary WebSocket message ({Length} bytes)", eb.Message.Length);
			return this.HandleBinaryDispatch(eb.Message);
		}

		return Task.CompletedTask;
	}

	/// <summary>
	///     Voices the w s_ socket opened.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task VoiceWS_SocketOpened(IWebSocketClient client, SocketEventArgs e)
	{
		this._discord.Logger.VoiceDebug(VoiceEvents.VoiceHandshake, "[Voice] Voice WS socket opened — sending IDENTIFY");
		return this.StartAsync();
	}

	/// <summary>
	///     Voices the ws_ socket exception.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task VoiceWs_SocketException(IWebSocketClient client, SocketErrorEventArgs e)
		=> this._voiceSocketError.InvokeAsync(this, new(this._discord.ServiceProvider)
		{
			Exception = e.Exception
		});

	/// <summary>
	///     Ws the send async.
	/// </summary>
	/// <param name="payload">The payload.</param>
	/// <returns>A Task.</returns>
	private async Task WsSendAsync(string payload)
	{
		this._discord.Logger.VoiceTrace(VoiceEvents.VoiceWsTx, payload);
		await this._voiceWs.SendMessageAsync(payload).ConfigureAwait(false);
	}

	/// <summary>
	///     Sends a binary payload to the Voice WebSocket server.
	/// </summary>
	/// <param name="data">The binary data to send.</param>
	/// <returns>A Task.</returns>
	private async Task WsSendBinaryAsync(byte[] data)
	{
		this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Sending binary WebSocket message ({Length} bytes)", data.Length);
		await this._voiceWs.SendMessageAsync(data).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets the unix timestamp.
	/// </summary>
	/// <param name="dt">The datetime.</param>
	private static uint UnixTimestamp(DateTime dt)
	{
		var ts = dt - s_unixEpoch;
		var sd = ts.TotalSeconds;
		var si = (uint)sd;
		return si;
	}

	/// <summary>
	///     Clears cached per-SSRC sender state after a reconnect/move so sequence tracking and
	///     decoders start fresh for the new voice server session.
	/// </summary>
	private void ClearAudioSenders()
	{
		foreach (var kv in this._transmittingSsrCs)
		{
			if (!this._transmittingSsrCs.TryRemove(kv.Key, out var sender))
				continue;

			try
			{
				this._opus?.DestroyDecoder(sender.Decoder);
			}
			catch
			{
				// best-effort cleanup
			}
		}
	}

	/// <summary>
	///     Handles a binary WebSocket message from the Voice Gateway.
	///     Server-sent binary messages use the framing: [seq: uint16 BE][opcode: uint8][payload...].
	/// </summary>
	/// <param name="data">The raw binary message data.</param>
	/// <returns>A Task.</returns>
	private async Task HandleBinaryDispatch(byte[] data)
	{
		// Minimum valid binary message: 2 bytes seq + 1 byte opcode = 3 bytes
		if (data.Length < 3)
		{
			this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received undersized binary WebSocket message ({Length} bytes), ignoring", data.Length);
			return;
		}

		var seq = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(0, 2));
		var opcode = data[2];
		var payload = data.AsMemory(3);

		// Voice gateway v8: track the last sequence number for seq_ack heartbeats and buffered resume.
		this._lastSeq = (int)seq;

		this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received binary DAVE opcode {Opcode} seq={Seq} payload={PayloadLength} bytes", opcode, seq, payload.Length);

		switch (opcode)
		{
			case 25: // MLS_EXTERNAL_SENDER_PACKAGE
				if (this._daveSession is not null)
				{
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP25 received");
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] OP25 external sender received, {Len} bytes", payload.Length);
					var kp25 = this._daveSession.HandleExternalSender(payload.ToArray());
					if (kp25.Length > 0)
					{
						this._daveProposalRestartSent = false;
						this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP26 sent");
						this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] Sending key package OP26 ({Len} bytes) from OP25 handler", kp25.Length);
						await this.WsSendBinaryAsync(BuildDaveBinaryMessage(26, kp25)).ConfigureAwait(false);
					}
				}

				break;
			case 27: // MLS_PROPOSALS
				if (this._daveSession is not null)
				{
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP27 received");
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] OP27 proposals received, {Len} bytes", payload.Length);
					var commitResult = this._daveSession.HandleProposals(payload.ToArray());
					if (commitResult is { CommitBytes.Length: > 0 })
					{
						this._daveProposalRestartSent = false; // reset guard on success
						var op28Payload = BuildOp28Payload(commitResult.Value);
						this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP28 sent");
						this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] OP28 commit sent, {Len} bytes", op28Payload.Length);
						await this.WsSendBinaryAsync(BuildDaveBinaryMessage(28, op28Payload)).ConfigureAwait(false);
					}
					else if (!this._daveProposalRestartSent)
					{
						// ProcessProposals returned 0 bytes — stateWithProposals_ inside libdave may be
						// corrupted by the failed round.  Reset and re-send OP 26 so the server delivers a
						// fresh OP 25 + OP 27 with clean libdave state.  Guard prevents infinite loops.
						this._daveProposalRestartSent = true;
						this._discord.Logger.LogWarning(VoiceEvents.DaveHandshake,
							"[DAVE] OP27 proposals produced no commit — resetting MLS state and re-sending OP26 (one-shot restart)");
						var kp27 = this._daveSession.PrepareKeyPackage();
						if (kp27.Length > 0)
						{
							this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP26 sent");
							this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] Re-sending key package OP26 ({Len} bytes) after OP27 failure", kp27.Length);
							await this.WsSendBinaryAsync(BuildDaveBinaryMessage(26, kp27)).ConfigureAwait(false);
						}
					}
					else
					{
						this._discord.Logger.LogWarning(VoiceEvents.DaveHandshake,
							"[DAVE] OP27 proposals failed again after one-shot restart — skipping further retries");
					}
				}

				break;
			case 29: // MLS_ANNOUNCE_COMMIT_TRANSITION  [transId: u16][commit bytes]
				if (payload.Length >= 2 && this._daveSession is not null)
				{
					var transId29 = BinaryPrimitives.ReadUInt16BigEndian(payload.Span[..2]);
					var commitPayload29 = payload.Slice(2).ToArray();
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP29 received");
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] OP29 announce_commit received, transId={TransId} {Len} bytes", transId29, commitPayload29.Length);
					var action29 = this._daveSession.HandleAnnounceCommit(commitPayload29, transId29);
					switch (action29)
					{
						case DaveAnnounceAction.SendReadyForTransition:
							this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] OP29 → sending OP23 ReadyForTransition (transId={TransId})", transId29);
							this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP23 sent");
							await this.WsSendAsync(JsonConvert.SerializeObject(new VoiceDispatch { OpCode = 23, Payload = new DaveReadyForTransitionPayload { TransitionId = transId29 } }, Formatting.None)).ConfigureAwait(false);
							break;
						case DaveAnnounceAction.Restart:
							this._discord.Logger.LogWarning(VoiceEvents.DaveHandshake, "[DAVE] OP29 commit failed — sending OP31 and re-initialising");
							this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP31 sent");
							await this.WsSendAsync(JsonConvert.SerializeObject(new VoiceDispatch { OpCode = 31, Payload = new DaveMlsInvalidCommitWelcomePayload() }, Formatting.None)).ConfigureAwait(false);
							this._daveProposalRestartSent = false; // allow proposal restart again after OP29-triggered reset
							var kp29 = this._daveSession.PrepareKeyPackage();
							if (kp29.Length > 0)
							{
								this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP26 sent");
								this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] Sending key package OP26 ({Len} bytes) after OP29 restart", kp29.Length);
								await this.WsSendBinaryAsync(BuildDaveBinaryMessage(26, kp29)).ConfigureAwait(false);
							}

							break;
					}
				}

				break;
			case 30: // MLS_WELCOME  [transId: u16][welcome bytes]
				if (payload.Length >= 2 && this._daveSession is not null)
				{
					var transId30 = BinaryPrimitives.ReadUInt16BigEndian(payload.Span[..2]);
					var welcomePayload = payload.Slice(2).ToArray();
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE FLOW] OP30 received");
					this._discord.Logger.VoiceDebug(VoiceEvents.DaveHandshake, "[DAVE] OP30 welcome received, transId={TransId} {Len} bytes", transId30, welcomePayload.Length);
					this._daveSession.HandleWelcome(welcomePayload);
				}

				break;
			default:
				this._discord.Logger.VoiceTrace(VoiceEvents.DaveHandshake, "Received unknown binary DAVE opcode {Opcode}", opcode);
				break;
		}
	}

	private static byte[] BuildOp28Payload(in MlsCommitResult commitResult)
	{
		if (commitResult.WelcomeBytes is not { Length: > 0 })
			return commitResult.CommitBytes;

		var payload = new byte[commitResult.CommitBytes.Length + commitResult.WelcomeBytes.Length];
		Buffer.BlockCopy(commitResult.CommitBytes, 0, payload, 0, commitResult.CommitBytes.Length);
		Buffer.BlockCopy(commitResult.WelcomeBytes, 0, payload, commitResult.CommitBytes.Length, commitResult.WelcomeBytes.Length);
		return payload;
	}

	/// <summary>Builds a client-to-server DAVE binary message: [opcode: u8][payload].</summary>
	private static byte[] BuildDaveBinaryMessage(byte opcode, byte[] payload)
	{
		var msg = new byte[1 + payload.Length];
		msg[0] = opcode;
		payload.CopyTo(msg, 1);
		return msg;
	}
}

