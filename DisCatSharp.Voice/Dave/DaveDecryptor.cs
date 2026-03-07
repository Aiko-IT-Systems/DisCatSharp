using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Inbound per-user DAVE frame decryptor.
///     One instance must be maintained per remote user in the voice channel.
/// </summary>
/// <remarks>
///     <para>
///         Holds the current-epoch <see cref="CryptorManager"/> plus a small retention list of managers
///         from previous epochs.  Old managers are kept alive for <see cref="s_cryptorExpiry"/> (10 s) so
///         that in-flight frames encrypted under the previous epoch can still be authenticated.
///     </para>
///     <para>
///         Passthrough mode is entered via <see cref="TransitionToPassthrough"/>.  While active,
///         frames that carry no DAVE footer are forwarded without modification.
///     </para>
/// </remarks>
internal sealed class DaveDecryptor : IDaveDecryptor
{
	/// <summary>
	///     How long a superseded <see cref="CryptorManager"/> is retained to service in-flight frames.
	/// </summary>
	private static readonly TimeSpan s_cryptorExpiry = TimeSpan.FromSeconds(10);

	private readonly Func<byte[], IAeadCipher> _cipherFactory;

	// --- Synchronisation design note ---
	// TransitionToKeyRatchet is a multi-step operation: it appends _currentManager to
	// _retainedManagers (a mutable List<>) before assigning a new one.  Because two separate
	// fields change together, and because List<T> is not thread-safe, an atomic-swap strategy
	// (e.g., Interlocked.Exchange on _currentManager alone) is not feasible.
	// lock(_sync) serialises all reads and writes to _currentManager, _retainedManagers,
	// _passthroughUntil, and _disposed across the encrypt/decrypt and ratchet-install paths.
	// IMPORTANT: Dispose() also acquires _sync so that a concurrent TryDecrypt() on the audio
	// thread cannot use _currentManager after it has been freed.
	private readonly Lock _sync = new();
	private CryptorManager? _currentManager;
	private readonly List<CryptorManager> _retainedManagers = [];
	private DateTime? _passthroughUntil;
	private bool _disposed;

	/// <summary>
	///     Initialises a new <see cref="DaveDecryptor"/> with the provided AEAD cipher factory.
	///     The decryptor starts with no active ratchet; call <see cref="TransitionToKeyRatchet"/> to activate decryption.
	/// </summary>
	/// <param name="cipherFactory">
	///     Factory that creates an <see cref="IAeadCipher"/> from a 16-byte key.
	///     Tests may supply a <c>TestAeadCipher</c> factory; production uses a libdave-backed factory.
	/// </param>
	public DaveDecryptor(Func<byte[], IAeadCipher> cipherFactory)
	{
		this._cipherFactory = cipherFactory ?? throw new ArgumentNullException(nameof(cipherFactory));
	}

	/// <summary>
	///     Installs a new key ratchet for the current epoch.
	///     The previous <see cref="CryptorManager"/> (if any) is moved to the retention list
	///     and will be expired after <see cref="s_cryptorExpiry"/>.
	/// </summary>
	public void TransitionToKeyRatchet(HashRatchet ratchet)
	{
		lock (this._sync)
		{
			if (this._currentManager is not null)
			{
				this._currentManager.ExpiresAt = DateTime.UtcNow + s_cryptorExpiry;
				this._retainedManagers.Add(this._currentManager);
			}

			this._currentManager = new CryptorManager(ratchet, this._cipherFactory);
		}
	}

	/// <summary>
	///     Enters passthrough mode for <paramref name="window"/>.
	///     Non-DAVE frames received within this window are forwarded as-is.
	/// </summary>
	public void TransitionToPassthrough(TimeSpan window)
	{
		lock (this._sync)
			this._passthroughUntil = DateTime.UtcNow + window;
	}

	/// <inheritdoc/>
	public void InstallRatchet(DaveRatchetInstaller installer)
	{
		if (installer.ManagedSecret is null)
			throw new ArgumentException("Managed path requires ManagedSecret.", nameof(installer));
		// TransitionToKeyRatchet internally uses _sync lock, so no additional lock needed here
		this.TransitionToKeyRatchet(new HashRatchet(installer.ManagedSecret));
	}

	/// <summary>
	///     Attempts to decrypt a DAVE-encrypted frame.
	/// </summary>
	/// <param name="frame">The raw received frame (may or may not carry a DAVE footer).</param>
	/// <param name="result">
	///     On success, a buffer rented from <see cref="System.Buffers.ArrayPool{T}.Shared"/> whose first <paramref name="resultLength"/>
	///     bytes contain the decrypted (or passthrough) payload.
	///     The caller MUST return it via <c>ArrayPool&lt;byte&gt;.Shared.Return(result)</c> after consuming <c>result[0..resultLength]</c>.
	///     Set to <see langword="null"/> when the method returns <see langword="false"/>.
	/// </param>
	/// <param name="resultLength">Number of valid bytes in <paramref name="result"/>.</param>
	/// <returns>
	///     <see langword="true"/> when the output is ready in <paramref name="result"/>.
	///     <see langword="false"/> when the frame is rejected (no DAVE footer outside the passthrough
	///     window, or decryption failed with all available managers).
	/// </returns>
	public bool TryDecrypt(ReadOnlySpan<byte> frame, out byte[] result, out int resultLength)
	{
		result = null!;
		resultLength = 0;

		lock (this._sync)
		{
			// Guard against use after Dispose() — Dispose() acquires the same lock so this check
			// is always consistent with the disposed state.
			if (this._disposed)
				return false;

			this.PurgeExpiredManagers();

			if (!DaveFrameProcessor.TryReadFooter(frame, out var footer))
			{
				if (this._passthroughUntil.HasValue && DateTime.UtcNow < this._passthroughUntil.Value)
				{
					var passRented = ArrayPool<byte>.Shared.Rent(frame.Length);
					frame.CopyTo(passRented);
					result = passRented;   // transfer ownership; caller returns to pool
					resultLength = frame.Length;
					return true;
				}

				return false;
			}

			var totalUnencrypted = 0;
			foreach (var (_, length) in footer.UnencryptedRanges)
				totalUnencrypted += length;

			var ciphertextStart = totalUnencrypted;
			var ciphertextEnd = frame.Length - footer.FooterSize;

			if (ciphertextEnd <= ciphertextStart)
				return false;

			var aad = frame[..totalUnencrypted];
			var ciphertext = frame[ciphertextStart..ciphertextEnd];
			var tag = footer.Tag.Span;
			var truncatedNonce = footer.TruncatedNonce;
			var generation = truncatedNonce >> 24;

			var rented = ArrayPool<byte>.Shared.Rent(totalUnencrypted + ciphertext.Length);
			aad.CopyTo(rented);
			var plaintextDest = rented.AsSpan(totalUnencrypted);
			var decryptSuccess = false;
			try
			{
				if (this._currentManager is not null
					&& this._currentManager.TryDecrypt(generation, ciphertext, tag, plaintextDest, truncatedNonce, aad))
				{
					result = rented;   // transfer ownership; caller returns to pool
					resultLength = totalUnencrypted + ciphertext.Length;
					decryptSuccess = true;
					return true;
				}

				foreach (var manager in this._retainedManagers)
				{
					if (manager.TryDecrypt(generation, ciphertext, tag, plaintextDest, truncatedNonce, aad))
					{
						result = rented;   // transfer ownership; caller returns to pool
						resultLength = totalUnencrypted + ciphertext.Length;
						decryptSuccess = true;
						return true;
					}
				}

				// All managers failed — frame rejected.
				result = null!;
				return false;
			}
			finally
			{
				// Return the rented buffer to the pool on every failure path (including exceptions).
				// On success, ownership has been transferred to the caller via `result`.
				if (!decryptSuccess)
					ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Acquire _sync so that any in-flight TryDecrypt() on the audio thread completes
		// before we free _currentManager and the retained managers.  Without this lock,
		// the gateway thread disposing a replaced decryptor could race with the audio thread
		// still holding a snapshot reference and executing TryDecrypt() under lock(_sync).
		lock (this._sync)
		{
			if (this._disposed)
				return;
			this._disposed = true;

			this._currentManager?.Dispose();
			this._currentManager = null;

			foreach (var manager in this._retainedManagers)
				manager.Dispose();

			this._retainedManagers.Clear();
		}
	}

	/// <summary>
	/// Purges expired managers from the retention list.
	/// </summary>
	private void PurgeExpiredManagers()
	{
		for (var i = this._retainedManagers.Count - 1; i >= 0; i--)
		{
			if (!this._retainedManagers[i].IsExpired)
				continue;

			this._retainedManagers[i].Dispose();
			this._retainedManagers.RemoveAt(i);
		}
	}
}
