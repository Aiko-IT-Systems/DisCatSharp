using System;
using System.Collections.Generic;
using System.Threading;

using DisCatSharp.Voice.Enums.Dave;
using DisCatSharp.Voice.Interfaces.Dave;
using DisCatSharp.Voice.Payloads;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Manages the DAVE E2EE voice session for a single voice channel connection.
/// </summary>
/// <remarks>
///     <para>
///         <b>Thread model:</b> gateway-thread methods (<see cref="HandleClientsConnect"/>,
///         <see cref="HandleAnnounceCommit"/>, <see cref="HandleWelcome"/>, and all other
///         <c>Handle*</c> methods) must only be called from the voice gateway WebSocket thread.
///         Audio-thread methods (<see cref="TryEncrypt"/>, <see cref="TryDecrypt"/>) are called
///         from the media send/receive threads.
///     </para>
///     <para>
///         <b>Thread safety of <c>_decryptors</c>:</b> the field is declared <see langword="volatile"/>
///         and updated exclusively via <see cref="System.Threading.Interlocked.Exchange{T}"/>.
///         The gateway thread builds a new <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/>
///         snapshot when users are added or removed and atomically publishes it; the audio
///         receive thread always reads a consistent, immutable snapshot via a single volatile read.
///         No lock is needed on the hot decrypt path.
///     </para>
///     <para>
///         <see cref="DaveSession"/> owns the state machine and dispatches gateway payloads
///         (OPs 11, 21–25, 27–31). It does not touch WebSocket, RTP, Sodium, or any networking
///         type — those responsibilities belong to <c>VoiceConnection</c>.
///     </para>
///     <para>
///         Session activation depends on <see cref="IMlsProvider"/> producing valid commit/welcome
///         outcomes and ratchet installers.
///     </para>
/// </remarks>
internal sealed class DaveSession : IDisposable
{
	private readonly ulong _selfUserId;
	private readonly IMlsProvider _mlsProvider;
	private readonly Func<IDaveDecryptor> _decryptorFactory;
	private readonly Action<int, bool, DaveSessionState, DaveSessionState, string, string>? _stateChanged;
	private readonly ILogger _logger;
	private readonly IDaveEncryptor _encryptor;
	private volatile Dictionary<ulong, IDaveDecryptor> _decryptors = [];
	private readonly HashSet<ulong> _recognizedUserIds = [];
	private readonly DaveTransitionTracker _transitionTracker = new();
	private ushort _latestPreparedProtocolVersion;
	private bool _disposed;

	/// <summary>
	///     Gets the current FSM state of this DAVE session.
	/// </summary>
	public DaveSessionState State { get; private set; } = DaveSessionState.Inactive;

	/// <summary>
	///     Gets whether the currently executing sender transform is encrypting audio.
	/// </summary>
	/// <remarks>
	///     Control-plane state and media activity are independent during a transition. An established
	///     member remains active while the next epoch is <see cref="DaveSessionState.ReadyForTransition"/>.
	/// </remarks>
	public bool IsActive => this.ProtocolVersion > 0 && this._encryptor.IsActive;

	/// <summary>
	///     Gets whether media may flow using either the executing DAVE sender ratchet or protocol-0 passthrough.
	/// </summary>
	public bool IsMediaReady => this.ProtocolVersion == 0 || this._encryptor.IsActive;

	/// <summary>
	///     Gets the DAVE protocol version used by the currently executing sender transform.
	/// </summary>
	public int ProtocolVersion { get; private set; }

	/// <summary>
	///     Initialises a new <see cref="DaveSession"/> for the specified user and protocol context.
	/// </summary>
	/// <param name="selfUserId">The Discord user ID of the local participant.</param>
	/// <param name="protocolVersion">The initial DAVE protocol version; 0 means DAVE is disabled.</param>
	/// <param name="mlsProvider">The MLS provider to use for group operations.</param>
	/// <param name="encryptorFactory">Factory producing the outbound <see cref="IDaveEncryptor"/>.</param>
	/// <param name="decryptorFactory">Factory producing per-user inbound <see cref="IDaveDecryptor"/> instances.</param>
	/// <param name="logger">Logger for state transitions and diagnostics.</param>
	/// <param name="stateChanged">Optional callback invoked for every state transition.</param>
	internal DaveSession(
		ulong selfUserId,
		int protocolVersion,
		IMlsProvider mlsProvider,
		Func<IDaveEncryptor> encryptorFactory,
		Func<IDaveDecryptor> decryptorFactory,
		ILogger logger,
		Action<int, bool, DaveSessionState, DaveSessionState, string, string>? stateChanged = null)
	{
		this._selfUserId = selfUserId;
		this._mlsProvider = mlsProvider ?? throw new ArgumentNullException(nameof(mlsProvider));
		ArgumentNullException.ThrowIfNull(encryptorFactory);
		this._decryptorFactory = decryptorFactory ?? throw new ArgumentNullException(nameof(decryptorFactory));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this._stateChanged = stateChanged;
		this._encryptor = encryptorFactory();
		this.ProtocolVersion = protocolVersion;
		this._latestPreparedProtocolVersion = checked((ushort)protocolVersion);

		if (protocolVersion > 0)
			this.TransitionTo(DaveSessionState.Pending, nameof(DaveSession), "constructed with protocolVersion > 0");
	}

	// -------------------------------------------------------------------------
	// Gateway payload handlers
	// -------------------------------------------------------------------------

	/// <summary>
	///     Handles OP 11 <c>clients_connect</c>.
	///     Updates the recognised user set and disposes decryptors for departed users.
	/// </summary>
	public void HandleClientsConnect(VoiceClientsConnectPayload payload)
	{
		var incoming = new HashSet<ulong>(payload.UserIds);
		var added = new List<ulong>();

		foreach (var userId in this._recognizedUserIds)
		{
			if (!incoming.Contains(userId))
				this.DisposeDecryptor(userId);
		}

		foreach (var userId in incoming)
		{
			if (!this._recognizedUserIds.Contains(userId))
				added.Add(userId);
		}

		this._recognizedUserIds.Clear();
		foreach (var id in incoming)
			this._recognizedUserIds.Add(id);

		foreach (var userId in added)
			this.PrepareReceiver(userId, this._latestPreparedProtocolVersion);

		this._logger.VoiceDebug("[DAVE] ClientsConnect: recognized {Count} user(s)", this._recognizedUserIds.Count);
	}

	/// <summary>
	///     Handles OP 21 <c>dave_mls_prepare_transition</c> by preparing receiver transforms before
	///     reporting readiness to the voice gateway.
	/// </summary>
	/// <returns>The action and authoritative transition ID for the caller.</returns>
	public DaveTransitionResult HandlePrepareTransition(DavePrepareTransitionPayload payload)
	{
		this._logger.VoiceDebug("[DAVE] PrepareTransition: id={TransitionId} version={Version}", payload.TransitionId, payload.ProtocolVersion);
		return this.PrepareTransition(payload.TransitionId, payload.ProtocolVersion, nameof(HandlePrepareTransition));
	}

	/// <summary>
	///     Handles OP 22 <c>dave_mls_execute_transition</c> by switching only the local sender transform.
	/// </summary>
	/// <returns><see langword="true"/> when a staged transition was consumed and executed.</returns>
	public bool HandleExecuteTransition(DaveExecuteTransitionPayload payload)
	{
		if (!this._transitionTracker.TryConsume(payload.TransitionId, out var targetVersion))
		{
			this._logger.LogWarning("[DAVE] ExecuteTransition: unknown transitionId={TransitionId}", payload.TransitionId);
			return false;
		}

		return this.ExecuteSenderTransition(payload.TransitionId, targetVersion, nameof(HandleExecuteTransition));
	}

	/// <summary>
	///     Handles OP 24 <c>dave_mls_prepare_epoch</c> and prepares a replacement key package for epoch 1.
	/// </summary>
	/// <returns>The OP 26 key-package payload for epoch 1, or an empty array for other epochs.</returns>
	public byte[] HandlePrepareEpoch(DavePrepareEpochPayload payload)
	{
		this._latestPreparedProtocolVersion = payload.ProtocolVersion;
		this._logger.VoiceDebug("[DAVE] PrepareEpoch: epoch={Epoch} version={Version}", payload.Epoch, payload.ProtocolVersion);

		return payload.Epoch == 1
			? this.PrepareKeyPackage(payload.ProtocolVersion)
			: [];
	}

	/// <summary>
	///     Initialises the MLS group and returns the serialised key package to send to the server as binary OP 26.
	/// </summary>
	/// <remarks>
	///     Must be called in response to OP 4 (<c>session_description</c>) when <c>dave_protocol_version &gt; 0</c>,
	///     and again in response to OP 24 (<c>dave_mls_prepare_epoch</c>) when <c>epoch == 1</c>
	///     (i.e. <c>MLS_NEW_GROUP_EXPECTED_EPOCH</c>).
	///     This mirrors the canonical flow from the official libdave TypeScript sample and godave:
	///     <c>onSelectProtocolAck → Init + GetMarshalledKeyPackage → send OP26</c>
	///     and
	///     <c>onDavePrepareEpoch(epoch=1) → Init + GetMarshalledKeyPackage → send OP26</c>.
	/// </remarks>
	/// <returns>
	///     The serialised MLS key package bytes to transmit as OP 26, or an empty array if the provider
	///     produced nothing (e.g. <see cref="NullMlsProvider"/> in non-DAVE builds).
	/// </returns>
	public byte[] PrepareKeyPackage(int? protocolVersion = null)
	{
		var targetProtocolVersion = checked((ushort)(protocolVersion ?? this._latestPreparedProtocolVersion));
		this._latestPreparedProtocolVersion = targetProtocolVersion;
		this._mlsProvider.InitGroup(this._selfUserId, targetProtocolVersion, []);
		var keyPackage = this._mlsProvider.GetKeyPackage();
		this._logger.VoiceDebug("[DAVE] PrepareKeyPackage: {Len} bytes, protocolVersion={Version}", keyPackage.Length, targetProtocolVersion);
		this.TransitionTo(DaveSessionState.AwaitingResponse, nameof(PrepareKeyPackage), "key package prepared");
		return keyPackage;
	}

	/// <summary>
	///     Resets only MLS preparation state after an invalid OP 29 or OP 30 and creates a replacement key package.
	/// </summary>
	/// <remarks>
	///     Currently executing media ratchets are retained so audio can continue until the voice gateway
	///     removes and re-adds this member with a later valid transition.
	/// </remarks>
	/// <param name="transitionId">The invalid transition ID reported to the voice gateway through OP 31.</param>
	/// <returns>The replacement OP 26 key-package payload.</returns>
	public byte[] RecoverFromInvalidTransition(ushort transitionId)
	{
		this._logger.LogWarning("[DAVE] Recovering from invalid commit or Welcome (transitionId={TransitionId})", transitionId);
		this._mlsProvider.Reset();
		this._transitionTracker.Clear();
		this.TransitionTo(DaveSessionState.Pending, nameof(RecoverFromInvalidTransition), $"invalid transitionId={transitionId}");
		return this.PrepareKeyPackage(this._latestPreparedProtocolVersion);
	}

	/// <summary>
	///     Handles binary OP 25 (external sender credential).
	///     Stores the external sender key so that <c>CreatePendingGroup</c> inside libdave can embed
	///     it in the group extensions.  Must be called <em>after</em> <see cref="PrepareKeyPackage"/>
	///     (i.e. after OP 4 or OP 24 have already triggered <see cref="IMlsProvider.InitGroup"/>).
	/// </summary>
	/// <remarks>
	///     Per the canonical libdave TypeScript sample and godave, OP 25 calls
	///     <c>SetExternalSender</c> only — no re-Init, no new key package, no OP 26.
	///     The OP 26 key package is always sent from OP 4 or OP 24.
	///
	///     Some gateway orderings can deliver OP 25 before any InitGroup-triggering path.
	///     In that case we lazily initialize once here so SetExternalSender can be applied.
	///     We only emit a key package from this handler for that lazy-init path.
	/// </remarks>
	public byte[] HandleExternalSender(byte[] externalSenderBytes)
	{
		var lazyInitialized = false;
		if (!this._mlsProvider.IsSessionInitialized)
		{
			this._mlsProvider.InitGroup(this._selfUserId, this._latestPreparedProtocolVersion, []);
			lazyInitialized = true;
			this._logger.VoiceDebug("[DAVE] HandleExternalSender: lazily initialized MLS session for OP25");
		}

		this._mlsProvider.SetExternalSender(externalSenderBytes);
		this._logger.VoiceDebug("[DAVE] HandleExternalSender: external sender stored ({ESLen} bytes)", externalSenderBytes.Length);

		if (!lazyInitialized)
			return [];

		var keyPackage = this._mlsProvider.GetKeyPackage();
		this._logger.VoiceDebug("[DAVE] HandleExternalSender: prepared key package {Len} bytes after lazy init", keyPackage.Length);
		if (keyPackage.Length > 0)
			this.TransitionTo(DaveSessionState.AwaitingResponse, nameof(HandleExternalSender), "key package prepared after lazy init");

		return keyPackage;
	}

	/// <summary>
	///     Handles binary OP 27 (MLS proposals). Processes proposals and returns a commit for the caller to send as OP 28.
	/// </summary>
	/// <returns>
	///     A <see cref="MlsCommitResult"/> with non-empty <see cref="MlsCommitResult.CommitBytes"/> that the caller
	///     should transmit as OP 28, or <see langword="null"/> if the provider produced no commit.
	/// </returns>
	public MlsCommitResult? HandleProposals(byte[] proposalsBytes)
	{
		// Always include self so libdave accepts the bot's own add-proposal.
		// Mirrors libdave-jvm's recognizedUserIdArray() which unconditionally appends selfUserIdString.
		var allRecognized = new HashSet<ulong>(this._recognizedUserIds) { this._selfUserId };
		var result = this._mlsProvider.ProcessProposals(proposalsBytes, allRecognized);
		this._logger.VoiceDebug("[DAVE] HandleProposals: commitBytes={CommitLen} welcomeBytes={WelcomeLen}",
			result.CommitBytes?.Length ?? 0, result.WelcomeBytes?.Length ?? 0);

		if (result.CommitBytes is not { Length: > 0 })
			return null;

		this.TransitionTo(DaveSessionState.AwaitingResponse, nameof(HandleProposals), "commit prepared from proposals");
		return result;
	}

	/// <summary>
	///     Handles binary OP 29 (announce commit).
	///     Applies the commit and returns the action the caller must take.
	/// </summary>
	/// <param name="commitBytes">The raw MLS commit payload (transitionId already stripped by caller).</param>
	/// <param name="transitionId">
	///     The transition ID prefix extracted by the caller from the raw OP 29 frame.
	///     When <c>0</c> (initial epoch), ratchets are installed immediately and the session transitions
	///     to <see cref="DaveSessionState.Active"/> without requiring an OP 23 acknowledgement.
	///     When non-zero, the session remains pre-active until OP 22 executes the transition,
	///     and the caller must send OP 23 to the server.
	/// </param>
	/// <returns>
	///     A <see cref="DaveTransitionResult"/> whose action the caller must perform:
	///     <list type="bullet">
	///       <item><description><see cref="DaveTransitionAction.None"/> — nothing to send.</description></item>
	///       <item><description><see cref="DaveTransitionAction.Ready"/> — send OP 23 for the result's transition ID.</description></item>
	///       <item><description><see cref="DaveTransitionAction.RecoverInvalid"/> — send OP 31, recover MLS, and send OP 26.</description></item>
	///     </list>
	/// </returns>
	public DaveTransitionResult HandleAnnounceCommit(byte[] commitBytes, ushort transitionId)
	{
		var outcome = this._mlsProvider.ProcessCommit(commitBytes);

		if (outcome.IsIgnored)
		{
			this._logger.VoiceDebug("[DAVE] AnnounceCommit: commit ignored (transitionId={TransId})", transitionId);
			return DaveTransitionResult.None(transitionId);
		}

		if (outcome.IsFailed)
		{
			this._logger.LogWarning("[DAVE] AnnounceCommit: commit FAILED (transitionId={TransId}), requesting restart", transitionId);
			return DaveTransitionResult.RecoverInvalid(transitionId);
		}

		this._logger.VoiceDebug("[DAVE] AnnounceCommit: commit applied (transitionId={TransId}), groupReady={Ready}", transitionId, this._mlsProvider.IsGroupReady);

		if (!this._mlsProvider.IsGroupReady)
		{
			this._logger.VoiceDebug("[DAVE] AnnounceCommit: group not ready after commit");
			return DaveTransitionResult.None(transitionId);
		}

		return this.PrepareTransition(transitionId, this._mlsProvider.ProtocolVersion, nameof(HandleAnnounceCommit));
	}

	/// <summary>
	///     Handles binary OP 30 (MLS welcome).
	///     Joins the group and prepares receiver ratchets for the Welcome's transition ID.
	/// </summary>
	/// <param name="welcomeBytes">The MLS Welcome bytes after the transition-ID prefix.</param>
	/// <param name="transitionId">The authoritative transition ID prefixed to OP 30.</param>
	/// <returns>The action and transition ID the caller must use for OP 23 or OP 31.</returns>
	public DaveTransitionResult HandleWelcome(byte[] welcomeBytes, ushort transitionId)
	{
		// Include self so libdave can match our leaf node when processing the welcome.
		var allRecognized = new HashSet<ulong>(this._recognizedUserIds) { this._selfUserId };
		if (!this._mlsProvider.ProcessWelcome(welcomeBytes, [], allRecognized))
		{
			this._logger.LogWarning("[DAVE] Welcome: processing failed (transitionId={TransitionId})", transitionId);
			return DaveTransitionResult.RecoverInvalid(transitionId);
		}

		this._logger.VoiceDebug("[DAVE] Welcome: group ready, preparing transitionId={TransitionId}", transitionId);
		return this.PrepareTransition(transitionId, this._mlsProvider.ProtocolVersion, nameof(HandleWelcome));
	}

	// -------------------------------------------------------------------------
	// Encrypt / Decrypt
	// -------------------------------------------------------------------------

	/// <summary>
	///     Encrypts an outbound Opus frame. Returns <see langword="false"/> (passthrough) when not
	///     <see cref="IsActive"/>.
	/// </summary>
	/// <param name="frame">The raw Opus frame to encrypt.</param>
	/// <param name="ssrc">
	///     The RTP SSRC of the local sender (from the voice READY payload).
	///     Threaded through to <see cref="IDaveEncryptor.TryEncrypt"/> so the native encryptor
	///     can embed the correct sender SSRC in the SFrame header.
	/// </param>
	/// <param name="result">On success, the encrypted frame bytes.</param>
	/// <param name="resultLength">Number of valid bytes in <paramref name="result"/>.</param>
	public bool TryEncrypt(ReadOnlySpan<byte> frame, uint ssrc, out byte[] result, out int resultLength)
	{
		if (!this.IsActive)
		{
			result = null!;
			resultLength = 0;
			return false;
		}

		return this._encryptor.TryEncrypt(frame, ssrc, out result, out resultLength);
	}

	/// <summary>
	///     Decrypts an inbound frame for the specified user.
	///     Returns <see langword="false"/> if no decryptor exists for <paramref name="userId"/>.
	/// </summary>
	/// <remarks>
	///     Threading: takes a single volatile snapshot of <c>_decryptors</c> at method entry so the
	///     audio-receive thread always operates on a consistent, immutable map.  Individual
	///     <see cref="IDaveDecryptor"/> instances are themselves thread-safe via their internal
	///     <c>lock(_sync)</c>.
	/// </remarks>
	public bool TryDecrypt(ulong userId, ReadOnlySpan<byte> frame, out byte[] result, out int resultLength)
	{
		var snapshot = this._decryptors; // single volatile read — safe on audio thread
		if (!snapshot.TryGetValue(userId, out var dec))
		{
			result = null!;
			resultLength = 0;
			return false;
		}

		return dec.TryDecrypt(frame, out result, out resultLength);
	}

	/// <summary>
	///     Pre-seeds the recognised user set from guild voice states collected before the first OP 11 arrives.
	///     Unlike <see cref="HandleClientsConnect"/>, this method <em>adds</em> to the existing set without
	///     clearing it, so it is safe to call before any OP 11 has been processed.
	/// </summary>
	/// <remarks>
	///     Called from the OP 4 handler to eliminate the race where OP 27 arrives before OP 11,
	///     causing ADD proposals for already-present users to be rejected as unrecognised.
	///     OP 11 (when it arrives) replaces the set authoritatively via <see cref="HandleClientsConnect"/>.
	/// </remarks>
	/// <param name="userIds">User IDs of channel members known from guild voice-state cache.</param>
	public void PreSeedRecognizedUsers(IEnumerable<ulong> userIds)
	{
		var addedUserIds = new List<ulong>();
		foreach (var id in userIds)
		{
			if (this._recognizedUserIds.Add(id))
				addedUserIds.Add(id);
		}

		foreach (var userId in addedUserIds)
			this.PrepareReceiver(userId, this._latestPreparedProtocolVersion);

		if (addedUserIds.Count > 0)
			this._logger.VoiceDebug("[DAVE] PreSeedRecognizedUsers: added {Count} user(s) from guild voice states", addedUserIds.Count);
	}

	/// <summary>
	///     Removes a user's decryptor and recognised-user entry.
	/// </summary>
	public void RemoveUser(ulong userId)
	{
		this.DisposeDecryptor(userId);
		this._recognizedUserIds.Remove(userId);
	}

	/// <summary>
	///     Resets all MLS and ratchet state.
	///     Returns to <see cref="DaveSessionState.Pending"/> if <see cref="ProtocolVersion"/> is &gt; 0,
	///     otherwise to <see cref="DaveSessionState.Inactive"/>.
	/// </summary>
	public void Reset()
	{
		this.ResetAllState();

		if (this.ProtocolVersion > 0)
			this.TransitionTo(DaveSessionState.Pending, nameof(Reset), "reset");
		else
			this.TransitionTo(DaveSessionState.Inactive, nameof(Reset), "reset with version 0");
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (this._disposed)
			return;
		this._disposed = true;
		this.ResetAllState();
		this._encryptor.Dispose();
		this.TransitionTo(DaveSessionState.Inactive, nameof(Dispose), "disposed");
	}

	// -------------------------------------------------------------------------
	// Private helpers
	// -------------------------------------------------------------------------

	/// <summary>
	///     Transitions the session FSM and emits a standard state-transition log entry.
	/// </summary>
	private void TransitionTo(DaveSessionState newState, string handler, string reason)
	{
		var oldState = this.State;
		if (oldState == newState)
			return;

		this.State = newState;
		this._logger.VoiceDebug("[DAVE FSM] {OldState} -> {NewState} via {Handler} ({Reason})", oldState, newState, handler, reason);
		this._stateChanged?.Invoke(this.ProtocolVersion, this.IsActive, oldState, newState, handler, reason);
	}

	/// <summary>
	///     Prepares all receiver transforms and either stages or immediately executes a transition.
	/// </summary>
	/// <param name="transitionId">The authoritative transition ID.</param>
	/// <param name="targetVersion">The protocol version receivers must be prepared to accept.</param>
	/// <param name="handler">The gateway handler that initiated preparation.</param>
	/// <returns>The voice-gateway action produced by preparation.</returns>
	private DaveTransitionResult PrepareTransition(ushort transitionId, ushort targetVersion, string handler)
	{
		this._latestPreparedProtocolVersion = targetVersion;
		foreach (var userId in this._recognizedUserIds)
			this.PrepareReceiver(userId, targetVersion);

		if (transitionId == 0)
		{
			this.ExecuteSenderTransition(transitionId, targetVersion, handler);
			return DaveTransitionResult.None(transitionId);
		}

		this._transitionTracker.Record(transitionId, targetVersion);
		var preparedState = targetVersion < this.ProtocolVersion
			? DaveSessionState.Downgrading
			: DaveSessionState.ReadyForTransition;
		this.TransitionTo(preparedState, handler, $"prepared transitionId={transitionId} version={targetVersion}");
		return DaveTransitionResult.Ready(transitionId);
	}

	/// <summary>
	///     Switches the local sender transform for one prepared transition.
	/// </summary>
	/// <param name="transitionId">The transition ID being executed.</param>
	/// <param name="targetVersion">The protocol version to begin sending.</param>
	/// <param name="handler">The handler responsible for execution.</param>
	/// <returns><see langword="true"/> when the sender transform was switched.</returns>
	private bool ExecuteSenderTransition(ushort transitionId, ushort targetVersion, string handler)
	{
		DaveRatchetInstaller? installer = null;
		if (targetVersion > 0)
		{
			installer = this._mlsProvider.GetRatchetInstaller(this._selfUserId);
			if (!IsUsableInstaller(installer))
			{
				installer?.NativeHandle?.Dispose();
				this._logger.LogError("[DAVE] ExecuteTransition: own ratchet unavailable for transitionId={TransitionId} version={Version}", transitionId, targetVersion);
				this.TransitionTo(DaveSessionState.Pending, handler, $"own ratchet unavailable for transitionId={transitionId}");
				return false;
			}
		}

		this._encryptor.TransitionTo(installer, targetVersion == 0);
		this.ProtocolVersion = targetVersion;
		this._latestPreparedProtocolVersion = targetVersion;

		if (targetVersion == 0)
		{
			this._mlsProvider.Reset();
			this._transitionTracker.Clear();
			this.TransitionTo(DaveSessionState.Inactive, handler, $"executed transitionId={transitionId} to protocol 0");
		}
		else
		{
			this.TransitionTo(DaveSessionState.Active, handler, $"executed transitionId={transitionId} version={targetVersion}");
		}

		return true;
	}

	/// <summary>
	///     Prepares one remote user's existing or newly created receiver transform.
	/// </summary>
	/// <param name="userId">The remote media sender.</param>
	/// <param name="targetVersion">The protocol version the receiver must accept.</param>
	private void PrepareReceiver(ulong userId, ushort targetVersion)
	{
		if (userId == this._selfUserId)
			return;

		DaveRatchetInstaller? installer = null;
		if (targetVersion > 0)
		{
			installer = this._mlsProvider.GetRatchetInstaller(userId);
			if (!IsUsableInstaller(installer))
			{
				installer?.NativeHandle?.Dispose();
				this._logger.VoiceDebug("[DAVE] Receiver ratchet unavailable for user {UserId} at protocol version {Version}", userId, targetVersion);
				return;
			}
		}

		var current = this._decryptors;
		if (current.TryGetValue(userId, out var decryptor))
		{
			decryptor.TransitionTo(installer, targetVersion == 0);
		}
		else
		{
			decryptor = this._decryptorFactory();
			decryptor.TransitionTo(installer, targetVersion == 0);
			var updated = new Dictionary<ulong, IDaveDecryptor>(current)
			{
				[userId] = decryptor
			};
			Interlocked.Exchange(ref this._decryptors, updated);
		}

		this._logger.VoiceDebug("[DAVE] Prepared receiver for user {UserId} at protocol version {Version}", userId, targetVersion);
	}

	/// <summary>
	///     Determines whether a provider-produced ratchet installer can be passed to a transform.
	/// </summary>
	/// <param name="installer">The optional ratchet installer.</param>
	/// <returns><see langword="true"/> for a native handle or a managed secret of at least 32 bytes.</returns>
	private static bool IsUsableInstaller(DaveRatchetInstaller? installer)
		=> installer is { } value
			&& (value.IsNative || (value.ManagedSecret?.Length ?? 0) >= 32);

	/// <summary>
	///     Resets the MLS provider, transition tracker, receiver transforms, and sender transform.
	/// </summary>
	private void ResetAllState()
	{
		this._mlsProvider.Reset();
		this._transitionTracker.Clear();
		this.ClearDecryptors();
		this._encryptor.TransitionTo(null, passthrough: true);
	}

	/// <summary>
	///     Atomically removes and disposes the decryptor for the specified user.
	/// </summary>
	private void DisposeDecryptor(ulong userId)
	{
		var current = this._decryptors;
		if (!current.ContainsKey(userId))
			return;

		var updated = new Dictionary<ulong, IDaveDecryptor>(current);
		if (!updated.Remove(userId, out var dec))
			return;

		Interlocked.Exchange(ref this._decryptors, updated);
		dec.Dispose();
	}

	/// <summary>
	///     Atomically replaces <c>_decryptors</c> with an empty map and disposes all previous decryptor instances.
	/// </summary>
	private void ClearDecryptors()
	{
		var old = Interlocked.Exchange(ref this._decryptors, []);
		foreach (var dec in old.Values)
			dec.Dispose();
	}
}
