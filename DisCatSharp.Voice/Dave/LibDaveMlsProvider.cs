using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using DisCatSharp.Voice.Dave.Interop;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     MLS provider backed by the native libdave library.
/// </summary>
/// <remarks>
///     <para>
///         <b>Thread model:</b> all methods in this class are called exclusively on the voice gateway
///         WebSocket thread. <c>_knownUserIds</c> is a plain <see cref="System.Collections.Generic.List{T}"/>
///         and is not thread-safe by design — concurrent access from any other thread is not permitted.
///     </para>
///     <para>
///         <b><c>_session</c> lifecycle:</b> the <see cref="DaveSessionSafeHandle"/> is created
///         lazily on the <em>first</em> call to <see cref="InitGroup"/> and is reused for all
///         subsequent <see cref="InitGroup"/> calls (which only invoke <c>daveSessionInit</c> again
///         on the existing handle, mirroring the canonical TS and godave implementations).
///         The handle is destroyed in <see cref="Dispose"/>. All other methods call
///         <see cref="EnsureSession"/> to guard against use before initialization or after disposal.
///     </para>
/// </remarks>
internal sealed class LibDaveMlsProvider : IMlsProvider, IDisposable
{
	private readonly string _authSessionId;
	private readonly ulong _channelId;
	private readonly int _protocolVersion;
	private readonly ILogger _logger;
	private DaveSessionSafeHandle? _session;
	private bool _disposed;
	private bool _groupReady;
	private bool _sessionInitialized;
	private ulong _selfUserId;
	// Accessed only on the voice gateway thread. Not thread-safe by design.
	private readonly List<ulong> _knownUserIds = [];

	/// <summary>
	///     Initialises a new <see cref="LibDaveMlsProvider"/> for the specified session and channel.
	/// </summary>
	/// <param name="authSessionId">The main-gateway session ID from the Discord READY event.</param>
	/// <param name="channelId">The voice channel / guild ID used as the MLS group ID.</param>
	/// <param name="protocolVersion">The negotiated DAVE protocol version.</param>
	/// <param name="logger">Logger for diagnostics.</param>
	public LibDaveMlsProvider(string authSessionId, ulong channelId, int protocolVersion, ILogger logger)
	{
		this._authSessionId = authSessionId ?? string.Empty;
		this._channelId = channelId;
		this._protocolVersion = protocolVersion;
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc/>
	public bool IsSessionInitialized => this._sessionInitialized;

	/// <inheritdoc/>
	public bool IsGroupReady => this._groupReady;

	/// <inheritdoc/>
	public void InitGroup(ulong selfUserId, int protocolVersion, byte[] groupId)
	{
		// Discord uses the voice channel ID as the MLS group ID.
		// The groupId parameter from IMlsProvider is ignored intentionally.
		this._selfUserId = selfUserId;
		this._groupReady = false;

		// Create the native session ONCE per provider lifetime.
		//
		// InitGroup() is called twice per DAVE handshake (once from OP4 and once from OP24 with epoch=1).
		// The canonical TS and godave implementations call Init() on the SAME session handle each time.
		// libdave's Init() internally calls Reset(), which clears pendingGroupState_ but does NOT clear
		// externalSender_. This means a prior SetExternalSender() call (from OP25) survives the second
		// InitGroup() call, and CreatePendingGroup() is invoked automatically inside Init() when
		// externalSender_ is already set. Destroying and recreating the handle loses externalSender_,
		// causing ProcessProposals() to fail when OP25 arrives before OP24's second InitGroup().
		if (this._session == null || this._session.IsInvalid)
		{
			this._session?.Dispose();
			this._session = null;
			this._session = DaveNative.SessionCreate(
				IntPtr.Zero,
				string.Empty,   // empty → libdave generates a transient signing key; passing the real session ID
								// fails when the binary uses the null persisted-key store (no GetPersistedKeyPair impl)
				IntPtr.Zero,
				IntPtr.Zero);

			if (this._session.IsInvalid)
				throw new InvalidOperationException("[DAVE] daveSessionCreate returned an invalid handle.");
		}

		try
		{
			// Use the negotiated protocol version captured during construction
			// rather than the InitGroup parameter (consistent with the OP4→OP24 version negotiation).
			DaveNative.SessionInit(
				this._session,
				(ushort)this._protocolVersion,
				this._channelId,
				selfUserId.ToString());
			this._sessionInitialized = true;
		}
		catch
		{
			this._sessionInitialized = false;
			this._session.Dispose();
			this._session = null;
			throw;
		}

		this._logger.VoiceDebug("[DAVE] MLS session initialized (channelId={ChannelId}, version={Version})",
			this._channelId, this._protocolVersion);
	}

	/// <inheritdoc/>
	public unsafe void SetExternalSender(byte[] externalSenderBytes)
	{
		this.EnsureSession();
		fixed (byte* p = externalSenderBytes)
			DaveNative.SessionSetExternalSender(this._session!, p, (nuint)externalSenderBytes.Length);
	}

	/// <inheritdoc/>
	public unsafe byte[] GetKeyPackage()
	{
		this.EnsureSession();
		DaveNative.SessionGetMarshalledKeyPackage(this._session!, out var ptr, out var length);
		// libdave allocated this buffer; free after copying to managed memory.
		try
		{
			if (ptr == null || length == 0)
				return [];
			var result = new byte[(int)length];
			Marshal.Copy((IntPtr)ptr, result, 0, (int)length);
			return result;
		}
		finally
		{
			if (ptr != null)
				DaveNative.Free((IntPtr)ptr);
		}
	}

	/// <inheritdoc/>
	public unsafe MlsCommitResult ProcessProposals(byte[] proposalsBytes, IReadOnlySet<ulong> recognizedUserIds)
	{
		this.EnsureSession();

		this._knownUserIds.Clear();
		foreach (var id in recognizedUserIds)
			this._knownUserIds.Add(id);

		var userIdStrings = new byte[this._knownUserIds.Count][];
		for (var i = 0; i < this._knownUserIds.Count; i++)
			userIdStrings[i] = Encoding.UTF8.GetBytes(this._knownUserIds[i].ToString() + '\0');

		var userCount = (nuint)this._knownUserIds.Count;
		byte* commitWelcomePtr = null;
		nuint commitWelcomeLength = 0;

		if (userCount == 0)
		{
			fixed (byte* pProposals = proposalsBytes)
			{
				DaveNative.SessionProcessProposals(
					this._session!,
					pProposals, (nuint)proposalsBytes.Length,
					null, 0,
					out commitWelcomePtr, out commitWelcomeLength);
			}
		}
		else
		{
			var handles = new GCHandle[userIdStrings.Length];
			var userIdPtrs = new byte*[userIdStrings.Length];
			try
			{
				for (var i = 0; i < userIdStrings.Length; i++)
				{
					handles[i] = GCHandle.Alloc(userIdStrings[i], GCHandleType.Pinned);
					userIdPtrs[i] = (byte*)handles[i].AddrOfPinnedObject();
				}

				fixed (byte* pProposals = proposalsBytes)
				fixed (byte** pUserIds = userIdPtrs)
				{
					DaveNative.SessionProcessProposals(
						this._session!,
						pProposals, (nuint)proposalsBytes.Length,
						pUserIds, userCount,
						out commitWelcomePtr, out commitWelcomeLength);
				}
			}
			finally
			{
				foreach (var h in handles)
					if (h.IsAllocated) h.Free();
			}
		}

		// Treat native output as commit payload; welcome processing is handled by OP30/ProcessWelcome.
		// libdave allocated this buffer; free after copying to managed memory.
		try
		{
			if (commitWelcomePtr == null || commitWelcomeLength == 0)
				return new MlsCommitResult { CommitBytes = [], WelcomeBytes = null };

			var bytes = new byte[(int)commitWelcomeLength];
			Marshal.Copy((IntPtr)commitWelcomePtr, bytes, 0, (int)commitWelcomeLength);
			return new MlsCommitResult { CommitBytes = bytes, WelcomeBytes = null };
		}
		finally
		{
			if (commitWelcomePtr != null)
				DaveNative.Free((IntPtr)commitWelcomePtr);
		}
	}

	/// <inheritdoc/>
	public unsafe MlsCommitOutcome ProcessCommit(byte[] commitBytes)
	{
		this.EnsureSession();
		IntPtr resultHandle;
		fixed (byte* p = commitBytes)
			resultHandle = DaveNative.SessionProcessCommit(this._session!, p, (nuint)commitBytes.Length);

		if (resultHandle == IntPtr.Zero)
		{
			this._logger.LogWarning("[DAVE] ProcessCommit: result handle is null");
			return new MlsCommitOutcome { IsFailed = true };
		}

		var failed = false;
		var ignored = false;
		try
		{
			failed = DaveNative.CommitResultIsFailed(resultHandle);
			ignored = DaveNative.CommitResultIsIgnored(resultHandle);
		}
		finally
		{
			// libdave allocated this result handle; always destroy it.
			DaveNative.CommitResultDestroy(resultHandle);
		}

		if (!failed && !ignored)
		{
			this._groupReady = true;
			this._logger.VoiceDebug("[DAVE] MLS commit processed — group ready");
		}
		else
		{
			this._logger.LogWarning("[DAVE] ProcessCommit: failed={Failed} ignored={Ignored}", failed, ignored);
		}

		return new MlsCommitOutcome { IsFailed = failed, IsIgnored = ignored };
	}

	/// <inheritdoc/>
	public unsafe void ProcessWelcome(byte[] welcomeBytes, byte[] ratchetKey, IReadOnlySet<ulong> recognizedUserIds)
	{
		this.EnsureSession();

		this._knownUserIds.Clear();
		foreach (var id in recognizedUserIds)
			this._knownUserIds.Add(id);

		var userIdStrings = new byte[this._knownUserIds.Count][];
		for (var i = 0; i < this._knownUserIds.Count; i++)
			userIdStrings[i] = Encoding.UTF8.GetBytes(this._knownUserIds[i].ToString() + '\0');

		var userCount = (nuint)this._knownUserIds.Count;
		var resultHandle = IntPtr.Zero;

		if (userCount == 0)
		{
			fixed (byte* pWelcome = welcomeBytes)
			{
				resultHandle = DaveNative.SessionProcessWelcome(
					this._session!,
					pWelcome, (nuint)welcomeBytes.Length,
					null, 0);
			}
		}
		else
		{
			var handles = new GCHandle[userIdStrings.Length];
			var userIdPtrs = new byte*[userIdStrings.Length];
			try
			{
				for (var i = 0; i < userIdStrings.Length; i++)
				{
					handles[i] = GCHandle.Alloc(userIdStrings[i], GCHandleType.Pinned);
					userIdPtrs[i] = (byte*)handles[i].AddrOfPinnedObject();
				}

				fixed (byte* pWelcome = welcomeBytes)
				fixed (byte** pUserIds = userIdPtrs)
				{
					resultHandle = DaveNative.SessionProcessWelcome(
						this._session!,
						pWelcome, (nuint)welcomeBytes.Length,
						pUserIds, userCount);
				}
			}
			finally
			{
				foreach (var h in handles)
					if (h.IsAllocated) h.Free();
			}
		}

		// libdave allocated this result handle; always destroy it.
		try
		{
			// Nothing to read from the welcome result — existence confirms success.
		}
		finally
		{
			if (resultHandle != IntPtr.Zero)
				DaveNative.WelcomeResultDestroy(resultHandle);
		}

		// Only mark the group ready when libdave returned a valid result handle.
		// A null handle indicates the welcome was rejected or failed internally.
		if (resultHandle == IntPtr.Zero)
		{
			this._logger.LogWarning("[DAVE] ProcessWelcome: libdave returned a null result handle; group NOT marked ready");
			return;
		}

		this._groupReady = true;
		this._logger.VoiceDebug("[DAVE] MLS welcome processed — group ready");
	}

	/// <inheritdoc/>
	public IReadOnlyList<(ulong UserId, DaveRatchetInstaller Installer)> GetUpdatedRatchets()
	{
		if (this._session is null || this._session.IsInvalid)
			return [];

		var result = new List<(ulong, DaveRatchetInstaller)>(this._knownUserIds.Count);
		foreach (var userId in this._knownUserIds)
		{
			// Skip self — the own ratchet is installed via GetOwnRatchetInstaller() into the encryptor,
			// not into a decryptor.  Mirrors libdave-jvm's setupKeyRatchetForUser() which routes self
			// to setSelfKeyRatchet() rather than to a Decryptor.
			if (userId == this._selfUserId)
				continue;

			var ratchetHandle = DaveNative.SessionGetKeyRatchet(this._session, userId.ToString());
			if (ratchetHandle.IsInvalid)
			{
				ratchetHandle.Dispose();
				continue;
			}

			// Ownership passes to the encryptor/decryptor via InstallRatchet().
			// The caller disposes the handle after passing it to the native library.
			result.Add((userId, DaveRatchetInstaller.FromNative(ratchetHandle)));
		}

		return result;
	}

	/// <inheritdoc/>
	public DaveRatchetInstaller GetOwnRatchetInstaller()
	{
		if (this._session is null || this._session.IsInvalid || this._selfUserId == 0)
			return DaveRatchetInstaller.FromManaged(Array.Empty<byte>());

		var ratchetHandle = DaveNative.SessionGetKeyRatchet(this._session, this._selfUserId.ToString());
		if (ratchetHandle.IsInvalid)
		{
			ratchetHandle.Dispose();
			this._logger.LogWarning("[DAVE] Own ratchet unavailable; falling back to managed empty installer.");
			return DaveRatchetInstaller.FromManaged(Array.Empty<byte>());
		}

		return DaveRatchetInstaller.FromNative(ratchetHandle);
	}

	/// <inheritdoc/>
	public void Reset()
	{
		if (this._disposed)
			return;
		if (this._session is not null && !this._session.IsInvalid)
			DaveNative.SessionReset(this._session);
		this._sessionInitialized = false;
		this._groupReady = false;
		this._knownUserIds.Clear();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (this._disposed)
			return;
		this._disposed = true;
		this._sessionInitialized = false;
		this._session?.Dispose();
		this._session = null;
	}

	/// <summary>Throws <see cref="ObjectDisposedException"/> if this instance has been disposed, or <see cref="InvalidOperationException"/> if the session has not been initialized.</summary>
	private void EnsureSession()
	{
		if (this._disposed)
			throw new ObjectDisposedException(nameof(LibDaveMlsProvider));
		if (this._session is null || this._session.IsInvalid)
			throw new InvalidOperationException("[DAVE] MLS session not initialized. Call InitGroup first.");
	}
}

