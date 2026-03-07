using System;
using System.Collections.Generic;
using System.Threading;

using DisCatSharp.Voice.Entities.Dave;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Voice.Dave;

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
///         snapshot in <see cref="InstallRatchets"/> and atomically publishes it; the audio
///         receive thread always reads a consistent, immutable snapshot via a single volatile read.
///         No lock is needed on the hot decrypt path.
///     </para>
///     <para>
///         <see cref="DaveSession"/> owns the state machine and dispatches gateway payloads
///         (OPs 11, 21–25, 27–31). It does not touch WebSocket, RTP, Sodium, or any networking
///         type — those responsibilities belong to <c>VoiceConnection</c>.
///     </para>
///     <para>Phase 4 uses <see cref="NullMlsProvider"/> so the session never reaches <see cref="DaveSessionState.Active"/>.</para>
///     <para>Phase 6 replaces the MLS provider with a libdave-backed implementation.</para>
/// </remarks>
internal sealed class DaveSession : IDisposable
{
	private readonly ulong _selfUserId;
	private readonly IMlsProvider _mlsProvider;
	private readonly Func<IDaveEncryptor> _encryptorFactory;
	private readonly Func<IDaveDecryptor> _decryptorFactory;
	private readonly ILogger _logger;
	private readonly IDaveEncryptor _encryptor;
	private volatile Dictionary<ulong, IDaveDecryptor> _decryptors = [];
	private readonly HashSet<ulong> _recognizedUserIds = [];
	private readonly DaveTransitionTracker _transitionTracker = new();
	private bool _disposed;

	/// <summary>Gets the current FSM state of this DAVE session.</summary>
	public DaveSessionState State { get; private set; } = DaveSessionState.Inactive;

	/// <summary>Gets whether this session is fully established and encrypting audio.</summary>
	public bool IsActive => this.State == DaveSessionState.Active;

	/// <summary>Gets the negotiated DAVE protocol version.</summary>
	public int ProtocolVersion { get; private set; }

	/// <summary>
	///     Initialises a new <see cref="DaveSession"/> for the specified voice channel and user.
	/// </summary>
	/// <param name="selfUserId">The Discord user ID of the local participant.</param>
	/// <param name="channelId">The voice channel ID (used for group identity).</param>
	/// <param name="protocolVersion">The initial DAVE protocol version; 0 means DAVE is disabled.</param>
	/// <param name="mlsProvider">The MLS provider to use for group operations.</param>
	/// <param name="encryptorFactory">Factory producing the outbound <see cref="IDaveEncryptor"/>.</param>
	/// <param name="decryptorFactory">Factory producing per-user inbound <see cref="IDaveDecryptor"/> instances.</param>
	/// <param name="logger">Logger for state transitions and diagnostics.</param>
	internal DaveSession(
		ulong selfUserId,
		ulong channelId,
		int protocolVersion,
		IMlsProvider mlsProvider,
		Func<IDaveEncryptor> encryptorFactory,
		Func<IDaveDecryptor> decryptorFactory,
		ILogger logger)
	{
		_ = channelId; // stored for future group-id use in Phase 6
		this._selfUserId = selfUserId;
		this._mlsProvider = mlsProvider ?? throw new ArgumentNullException(nameof(mlsProvider));
		this._encryptorFactory = encryptorFactory ?? throw new ArgumentNullException(nameof(encryptorFactory));
		this._decryptorFactory = decryptorFactory ?? throw new ArgumentNullException(nameof(decryptorFactory));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this._encryptor = encryptorFactory();
		this.ProtocolVersion = protocolVersion;

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

		foreach (var userId in this._recognizedUserIds)
		{
			if (!incoming.Contains(userId))
				this.DisposeDecryptor(userId);
		}

		this._recognizedUserIds.Clear();
		foreach (var id in incoming)
			this._recognizedUserIds.Add(id);

		this._logger.VoiceDebug("[DAVE] ClientsConnect: recognized {Count} user(s)", this._recognizedUserIds.Count);
	}

	/// <summary>Handles OP 21 <c>dave_mls_prepare_transition</c>. Records the pending transition and enters <see cref="DaveSessionState.ReadyForTransition"/>.</summary>
	public void HandlePrepareTransition(DavePrepareTransitionPayload payload)
	{
		this._transitionTracker.Record(payload.TransitionId, payload.ProtocolVersion);
		this.TransitionTo(DaveSessionState.ReadyForTransition, nameof(HandlePrepareTransition), $"prepare transitionId={payload.TransitionId} version={payload.ProtocolVersion}");
		this._logger.VoiceDebug("[DAVE] PrepareTransition: id={TransitionId} version={Version}", payload.TransitionId, payload.ProtocolVersion);
	}

	/// <summary>
	///     Handles OP 22 <c>dave_mls_execute_transition</c>.
	/// </summary>
	/// <returns>
	///     A <see cref="DaveReadyForTransitionPayload"/> for the caller to send as OP 23, or
	///     <see langword="null"/> when no acknowledgement should be sent (transitionId 0 or transition unknown).
	/// </returns>
	public DaveReadyForTransitionPayload? HandleExecuteTransition(DaveExecuteTransitionPayload payload)
	{
		if (!this._transitionTracker.TryConsume(payload.TransitionId, out var targetVersion))
		{
			this._logger.LogWarning("[DAVE] ExecuteTransition: unknown transitionId={TransitionId}", payload.TransitionId);
			return null;
		}

		if (targetVersion == 0)
		{
			this.ResetMls();
			this.TransitionTo(DaveSessionState.Inactive, nameof(HandleExecuteTransition), "targetVersion=0");
			return null;
		}

		if (targetVersion < this.ProtocolVersion)
		{
			this.ResetMls();
			this.TransitionTo(DaveSessionState.Downgrading, nameof(HandleExecuteTransition), $"downgrade from {this.ProtocolVersion} to {targetVersion}");
			return null;
		}

		this.ProtocolVersion = targetVersion;
		this.ResetMls();
		this.TransitionTo(DaveSessionState.Pending, nameof(HandleExecuteTransition), $"transition to version {targetVersion}");

		return payload.TransitionId != 0
			? new DaveReadyForTransitionPayload { TransitionId = payload.TransitionId }
			: null;
	}

	/// <summary>Handles OP 24 <c>dave_mls_prepare_epoch</c>. Stores transition info for the upcoming epoch.</summary>
	public void HandlePrepareEpoch(DavePrepareEpochPayload payload)
	{
		this._transitionTracker.Record(payload.TransitionId, payload.ProtocolVersion);
		this._logger.VoiceDebug("[DAVE] PrepareEpoch: transitionId={TransitionId} epoch={Epoch} version={Version}",
			payload.TransitionId, payload.Epoch, payload.ProtocolVersion);
	}

	/// <summary>
	///     Handles OP 31 <c>dave_mls_invalid_commit_welcome</c>.
	///     Resets MLS state and returns to <see cref="DaveSessionState.Pending"/>.
	/// </summary>
	public void HandleInvalidCommit(DaveMlsInvalidCommitWelcomePayload? payload)
	{
		this._logger.LogWarning("[DAVE] InvalidCommit: {Description}", payload?.Description ?? "(no description)");
		this._mlsProvider.Reset();

		if (this.ProtocolVersion > 0)
			this.TransitionTo(DaveSessionState.Pending, nameof(HandleInvalidCommit), "invalid commit");
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
	public byte[] PrepareKeyPackage()
	{
		this._mlsProvider.InitGroup(this._selfUserId, this.ProtocolVersion, []);
		var keyPackage = this._mlsProvider.GetKeyPackage();
		this._logger.VoiceDebug("[DAVE] PrepareKeyPackage: {Len} bytes, protocolVersion={Version}", keyPackage.Length, this.ProtocolVersion);
		this.TransitionTo(DaveSessionState.AwaitingResponse, nameof(PrepareKeyPackage), "key package prepared");
		return keyPackage;
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
	/// </remarks>
	public byte[] HandleExternalSender(byte[] externalSenderBytes)
	{
		this._mlsProvider.InitGroup(this._selfUserId, this.ProtocolVersion, []);
		this._mlsProvider.SetExternalSender(externalSenderBytes);
		this._logger.VoiceDebug("[DAVE] HandleExternalSender: {ESLen} bytes", externalSenderBytes.Length);
		var keyPackage = this._mlsProvider.GetKeyPackage();
		this._logger.VoiceDebug("[DAVE] HandleExternalSender: prepared key package {Len} bytes, protocolVersion={Version}", keyPackage.Length, this.ProtocolVersion);
		this.TransitionTo(DaveSessionState.AwaitingResponse, nameof(HandleExternalSender), "external sender processed and key package prepared");
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
	///     When non-zero, only decryptors are installed; the encryptor is activated on OP 22,
	///     and the caller must send OP 23 to the server.
	/// </param>
	/// <returns>
	///     A <see cref="DaveAnnounceAction"/> the caller must act on:
	///     <list type="bullet">
	///       <item><description><see cref="DaveAnnounceAction.None"/> — nothing to send.</description></item>
	///       <item><description><see cref="DaveAnnounceAction.SendReadyForTransition"/> — send OP 23 with the same <paramref name="transitionId"/>.</description></item>
	///       <item><description><see cref="DaveAnnounceAction.Restart"/> — commit failed; send OP 31, then call <see cref="PrepareKeyPackage"/> and send OP 26.</description></item>
	///     </list>
	/// </returns>
	public DaveAnnounceAction HandleAnnounceCommit(byte[] commitBytes, ushort transitionId)
	{
		var outcome = this._mlsProvider.ProcessCommit(commitBytes);

		if (outcome.IsIgnored)
		{
			this._logger.VoiceDebug("[DAVE] AnnounceCommit: commit ignored (transitionId={TransId})", transitionId);
			return DaveAnnounceAction.None;
		}

		if (outcome.IsFailed)
		{
			this._logger.LogWarning("[DAVE] AnnounceCommit: commit FAILED (transitionId={TransId}), requesting restart", transitionId);
			this.TransitionTo(DaveSessionState.Pending, nameof(HandleAnnounceCommit), "commit failed");
			return DaveAnnounceAction.Restart;
		}

		this._logger.VoiceDebug("[DAVE] AnnounceCommit: commit applied (transitionId={TransId}), groupReady={Ready}", transitionId, this._mlsProvider.IsGroupReady);

		if (this._mlsProvider.IsGroupReady)
		{
			this.InstallRatchets();
			this.TransitionTo(DaveSessionState.Active, nameof(HandleAnnounceCommit), transitionId == 0
				? "initial commit applied"
				: $"commit applied (transitionId={transitionId})");
			return transitionId != 0
				? DaveAnnounceAction.SendReadyForTransition
				: DaveAnnounceAction.None;
		}
		else
		{
			this._logger.VoiceDebug("[DAVE] AnnounceCommit: group not ready after commit");
			return DaveAnnounceAction.None;
		}
	}

	/// <summary>
	///     Handles binary OP 30 (MLS welcome).
	///     Joins the group; if the group becomes ready, installs ratchets and transitions to
	///     <see cref="DaveSessionState.Active"/>.
	/// </summary>
	public void HandleWelcome(byte[] welcomeBytes)
	{
		// Include self so libdave can match our leaf node when processing the welcome.
		var allRecognized = new HashSet<ulong>(this._recognizedUserIds) { this._selfUserId };
		this._mlsProvider.ProcessWelcome(welcomeBytes, [], allRecognized);

		if (this._mlsProvider.IsGroupReady)
		{
			this._logger.VoiceDebug("[DAVE] Welcome: group ready, installing ratchets");
			this.InstallRatchets();
			this.TransitionTo(DaveSessionState.Active, nameof(HandleWelcome), "welcome applied and group ready");
		}
		else
		{
			this._logger.VoiceDebug("[DAVE] Welcome: group not ready after welcome");
		}
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
		var added = 0;
		foreach (var id in userIds)
		{
			if (this._recognizedUserIds.Add(id))
				added++;
		}

		if (added > 0)
			this._logger.VoiceDebug("[DAVE] PreSeedRecognizedUsers: added {Count} user(s) from guild voice states", added);
	}

	/// <summary>Removes a user's decryptor and recognised-user entry.</summary>
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
		this.ResetMls();

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
		this.ResetMls();
		this._encryptor.Dispose();
		this.TransitionTo(DaveSessionState.Inactive, nameof(Dispose), "disposed");
	}

	// -------------------------------------------------------------------------
	// Private helpers
	// -------------------------------------------------------------------------

	/// <summary>Transitions the session FSM and emits a standard state-transition log entry.</summary>
	private void TransitionTo(DaveSessionState newState, string handler, string reason)
	{
		var oldState = this.State;
		if (oldState == newState)
			return;

		this.State = newState;
		this._logger.VoiceDebug("[DAVE FSM] {OldState} -> {NewState} via {Handler} ({Reason})", oldState, newState, handler, reason);
	}

	/// <summary>Resets the MLS provider, transition tracker, decryptors, and encryptor passthrough.</summary>
	private void ResetMls()
	{
		this._mlsProvider.Reset();
		this._transitionTracker.Clear();
		this.ClearDecryptors();
		this._encryptor.SetPassthrough(true);
	}

	/// <summary>
	///     Installs per-user and own ratchet secrets returned by the MLS provider after a commit or welcome.
	///     Only called when <see cref="IMlsProvider.IsGroupReady"/> is <see langword="true"/>.
	/// </summary>
	/// <remarks>
	///     Threading: runs on the gateway WebSocket thread.  Builds a new dictionary snapshot,
	///     atomically publishes it via <see cref="Interlocked.Exchange{T}"/>, then disposes any
	///     replaced decryptor instances.  The audio-receive thread always reads a consistent snapshot
	///     via the volatile field, so no lock is needed on the hot decrypt path.
	/// </remarks>
	private void InstallRatchets()
	{
		var current = this._decryptors;
		var updated = new Dictionary<ulong, IDaveDecryptor>(current);
		var toDispose = new List<IDaveDecryptor>();

		foreach (var (userId, installer) in this._mlsProvider.GetUpdatedRatchets())
		{
			if (!installer.IsNative && (installer.ManagedSecret?.Length ?? 0) < 32)
			{
				this._logger.LogWarning("[DAVE] InstallRatchets: ratchet for user {UserId} is too short, skipping", userId);
				installer.NativeHandle?.Dispose();
				continue;
			}

			// Always create a fresh decryptor for each ratchet rotation so the old instance can
			// be cleanly disposed after the atomic swap — no audio thread can reach it afterward.
			var dec = this._decryptorFactory();
			dec.InstallRatchet(installer);

			if (updated.TryGetValue(userId, out var old))
				toDispose.Add(old);

			updated[userId] = dec;
			this._logger.VoiceDebug("[DAVE] Installed ratchet for user {UserId}", userId);
		}

		// Atomically publish the new map.  After this point the audio thread will only see
		// decryptors from `updated`; any instance collected in toDispose is no longer reachable
		// through _decryptors.  Each decryptor's internal lock(_sync) serialises any in-flight
		// TryDecrypt call, so disposal is safe immediately after the exchange.
#pragma warning disable CS0420 // Interlocked already provides full barriers — volatile is intentional
		Interlocked.Exchange(ref this._decryptors, updated);
#pragma warning restore CS0420

		foreach (var dec in toDispose)
			dec.Dispose();

		var ownInstaller = this._mlsProvider.GetOwnRatchetInstaller();
		if (ownInstaller.IsNative || (ownInstaller.ManagedSecret?.Length ?? 0) >= 32)
			this._encryptor.InstallRatchet(ownInstaller);
		else
			ownInstaller.NativeHandle?.Dispose();
	}

	/// <summary>Atomically removes and disposes the decryptor for the specified user.</summary>
	private void DisposeDecryptor(ulong userId)
	{
		var current = this._decryptors;
		if (!current.ContainsKey(userId))
			return;

		var updated = new Dictionary<ulong, IDaveDecryptor>(current);
		if (!updated.Remove(userId, out var dec))
			return;

#pragma warning disable CS0420
		Interlocked.Exchange(ref this._decryptors, updated);
#pragma warning restore CS0420
		dec.Dispose();
	}

	/// <summary>Atomically replaces <c>_decryptors</c> with an empty map and disposes all previous decryptor instances.</summary>
	private void ClearDecryptors()
	{
#pragma warning disable CS0420
		var old = Interlocked.Exchange(ref this._decryptors, []);
#pragma warning restore CS0420
		foreach (var dec in old.Values)
			dec.Dispose();
	}
}

