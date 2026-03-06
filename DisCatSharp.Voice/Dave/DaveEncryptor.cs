using System;
using System.Buffers;
using System.Security.Cryptography;
using System.Threading;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Outbound per-connection DAVE frame encryptor.
///     One instance per <c>VoiceConnection</c> (outbound direction only).
/// </summary>
/// <remarks>
///     In passthrough mode (before DAVE is active, or after <see cref="SetPassthrough"/> is called with
///     <see langword="true"/>), <see cref="TryEncrypt"/> returns <see langword="false"/> and the caller
///     should transmit the original frame unchanged.
///     Once a key ratchet is installed via <see cref="SetKeyRatchet"/>, frames are encrypted using
///     the configured <see cref="IAeadCipher"/> via <see cref="CryptorManager"/> and
///     <see cref="TryEncrypt"/> returns <see langword="true"/> with an allocated output buffer.
/// </remarks>
internal sealed class DaveEncryptor : IDaveEncryptor
{
	/// <summary>
	///     Upper bound on the serialised footer size (tag + nonce ULEB128 + ranges ULEB128 + size byte + 2-byte magic).
	/// </summary>
	private const int MaxFooterSize = 50;

	private readonly Func<byte[], IAeadCipher> _cipherFactory;

	// --- Synchronisation design note ---
	// _cryptorManager and _truncatedNonce are tightly coupled: the nonce upper-byte encodes
	// the ratchet generation, so replacing the CryptorManager must be atomic with respect to
	// nonce reads/writes.  A simple Interlocked.Exchange on _cryptorManager alone would leave
	// _truncatedNonce inconsistent with the new generation.  Therefore lock(_sync) is used for
	// all reads and writes of both fields, rather than an atomic-swap approach.
	// (Native implementations use lock for the same reason — see LibDaveEncryptor.)
	private readonly Lock _sync = new();
	private bool _passthrough = true;
	private CryptorManager? _cryptorManager;

	/// <summary>
	///     Monotonically increasing 32-bit truncated nonce.
	///     The upper 8 bits encode the ratchet generation (<c>_truncatedNonce >> 24</c>).
	///     Always read and written under <see cref="_sync"/>.
	/// </summary>
	private uint _truncatedNonce;

	/// <summary>
	///     Initialises a new <see cref="DaveEncryptor"/> with the provided AEAD cipher factory.
	///     The encryptor starts in passthrough mode; call <see cref="SetKeyRatchet"/> to activate encryption.
	/// </summary>
	/// <param name="cipherFactory">
	///     Factory that creates an <see cref="IAeadCipher"/> from a 16-byte key.
	///     Tests may supply a <c>TestAeadCipher</c> factory; production uses a libdave-backed factory.
	/// </param>
	public DaveEncryptor(Func<byte[], IAeadCipher> cipherFactory)
	{
		this._cipherFactory = cipherFactory ?? throw new ArgumentNullException(nameof(cipherFactory));
	}

	/// <summary>
	///     Replaces the current key ratchet and immediately begins encrypting with the new material.
	///     Passthrough mode is disabled automatically.
	/// </summary>
	public void SetKeyRatchet(HashRatchet ratchet)
	{
		lock (this._sync)
		{
			this._cryptorManager?.Dispose();
			this._cryptorManager = new CryptorManager(ratchet, this._cipherFactory);
			this._passthrough = false;
		}
	}

	/// <summary>
	///     Controls passthrough mode. When <see langword="true"/>, frames are forwarded without encryption.
	/// </summary>
	public void SetPassthrough(bool passthrough)
	{
		lock (this._sync)
			this._passthrough = passthrough;
	}

	/// <summary>
	///     Gets whether a ratchet has been installed and encryption is active.
	/// </summary>
	/// <remarks>
	///     This property reads <c>_passthrough</c> and <c>_cryptorManager</c> without acquiring
	///     <see cref="_sync"/>.  Both fields are individually atomic (bool and object-reference reads
	///     are pointer-sized or smaller, so no torn reads are possible), making this a safe
	///     best-effort observable snapshot.  Correctness for the encrypt path is independently
	///     enforced by the lock inside <see cref="TryEncrypt"/>.
	/// </remarks>
	public bool IsActive => !this._passthrough && this._cryptorManager is not null;

	/// <inheritdoc/>
	public void InstallRatchet(DaveRatchetInstaller installer)
	{
		if (installer.ManagedSecret is null)
			throw new ArgumentException("Managed path requires ManagedSecret.", nameof(installer));
		// SetKeyRatchet internally uses _sync lock, so no additional lock needed here
		this.SetKeyRatchet(new HashRatchet(installer.ManagedSecret));
	}

	/// <summary>
	///     Encrypts an Opus frame for the DAVE protocol.
	/// </summary>
	/// <param name="frame">The raw Opus frame to encrypt.</param>
	/// <param name="ssrc">
	///     The RTP SSRC of the local sender.  Not used by the managed cipher path (which does not
	///     embed SFrame-style sender metadata), but required for interface uniformity with the
	///     native <c>LibDaveEncryptor</c>.
	/// </param>
	/// <param name="result">
	///     On success, a buffer rented from <see cref="ArrayPool{T}.Shared"/> containing the encrypted frame.
	///     Valid bytes span <c>result[0..resultLength]</c>.
	///     The caller MUST return it via <c>ArrayPool&lt;byte&gt;.Shared.Return(result)</c> after consuming <c>result[0..resultLength]</c>.
	///     Set to <see langword="null"/> when the method returns <see langword="false"/>.
	/// </param>
	/// <param name="resultLength">The number of valid bytes in <paramref name="result"/>.</param>
	/// <returns>
	///     <see langword="true"/> when the frame was encrypted and <paramref name="result"/> is populated.
	///     <see langword="false"/> when the caller should transmit the original <paramref name="frame"/> unchanged
	///     (passthrough mode, signal frame, or no ratchet installed).
	/// </returns>
	public bool TryEncrypt(ReadOnlySpan<byte> frame, uint ssrc, out byte[] result, out int resultLength)
	{
		_ = ssrc; // not used by the managed cipher path; present for IDaveEncryptor interface uniformity
		result = null!;
		resultLength = 0;

		lock (this._sync)
		{
			if (frame.IsEmpty || DaveFrameProcessor.IsSignalFrame(frame) || this._passthrough || this._cryptorManager is null)
				return false;

			var unencryptedRanges = DaveFrameProcessor.GetUnencryptedRanges(DaveCodec.Opus, frame);

			var totalUnencrypted = 0;
			foreach (var (_, length) in unencryptedRanges)
				totalUnencrypted += length;

			var aad = frame[..totalUnencrypted];
			var encryptedSlice = frame[totalUnencrypted..];

			// Output layout: [unencrypted prefix][ciphertext][footer]
			var rented = ArrayPool<byte>.Shared.Rent(frame.Length + MaxFooterSize);
			var success = false;
			try
			{
				aad.CopyTo(rented);

				var ciphertextDest = rented.AsSpan(totalUnencrypted, encryptedSlice.Length);

				Span<byte> tag = stackalloc byte[DaveConstants.TagSize];
				var generation = this._truncatedNonce >> 24;
				this._cryptorManager.Encrypt(generation, encryptedSlice, ciphertextDest, tag, this._truncatedNonce, aad);

				var footerWritten = DaveFrameProcessor.WriteFooter(
					rented.AsSpan(totalUnencrypted + encryptedSlice.Length),
					tag,
					this._truncatedNonce,
					unencryptedRanges);

				if (this._truncatedNonce == uint.MaxValue)
					throw new CryptographicException("[DAVE] Nonce counter exhausted. Ratchet rotation required before further encryption.");
				this._truncatedNonce++;

				result = rented;   // transfer ownership; caller returns to pool
				resultLength = totalUnencrypted + encryptedSlice.Length + footerWritten;
				success = true;
				return true;
			}
			finally
			{
				if (!success)
					ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		this._cryptorManager?.Dispose();
		this._cryptorManager = null;
	}
}
