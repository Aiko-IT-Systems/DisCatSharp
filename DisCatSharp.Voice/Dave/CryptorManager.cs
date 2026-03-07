using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Manages per-generation AEAD cipher instances for a single DAVE sender,
///     built on top of <see cref="HashRatchet"/> for key derivation.
/// </summary>
/// <remarks>
///     Caches <see cref="IAeadCipher"/> instances keyed by generation to avoid repeated key
///     derivation. Enforces a maximum generation lookahead of <see cref="KMaxGenerationGap"/>
///     to prevent runaway key pre-computation. Supports an optional expiry for old-epoch
///     managers that must remain alive during DAVE epoch transitions.
/// </remarks>
internal sealed class CryptorManager : IDisposable
{
	/// <summary>
	///     Maximum number of generations a new generation may be ahead of the last seen one.
	/// </summary>
	internal const int KMaxGenerationGap = 250;

	/// <summary>
	///     How long an old-epoch manager remains valid after being superseded.
	/// </summary>
	internal static readonly TimeSpan KCryptorExpiry = TimeSpan.FromSeconds(10);

	private readonly HashRatchet _ratchet;
	private readonly Func<byte[], IAeadCipher> _cipherFactory;

	// Stores the cipher AND the nonceBase together so HashRatchet.Get() is called at most once per generation.
	private readonly Dictionary<uint, (IAeadCipher Cipher, byte[] NonceBase)> _cryptors = [];
	private uint _highestSeenGeneration;
	private bool _hasSeenAnyGeneration;
	private bool _disposed;

	/// <summary>
	///     When set, the UTC time at which this manager should be considered expired.
	///     Used by old-epoch managers during DAVE transitions.
	/// </summary>
	public DateTime? ExpiresAt { get; set; }

	/// <summary>
	///     Returns <c>true</c> if this manager has an expiry set and that time has passed.
	/// </summary>
	public bool IsExpired
		=> this.ExpiresAt.HasValue && DateTime.UtcNow >= this.ExpiresAt.Value;

	/// <summary>
	///     Initialises a new <see cref="CryptorManager"/> backed by the given ratchet and cipher factory.
	/// </summary>
	/// <param name="ratchet">The key ratchet that derives per-generation keys and nonce bases.</param>
	/// <param name="cipherFactory">
	///     Factory delegate that creates an <see cref="IAeadCipher"/> from a 16-byte key.
	///     Tests may pass a <c>TestAeadCipher</c> factory; production uses a libdave-backed factory.
	/// </param>
	public CryptorManager(HashRatchet ratchet, Func<byte[], IAeadCipher> cipherFactory)
	{
		this._ratchet = ratchet ?? throw new ArgumentNullException(nameof(ratchet));
		this._cipherFactory = cipherFactory ?? throw new ArgumentNullException(nameof(cipherFactory));
	}

	/// <summary>
	///     Encrypts <paramref name="plaintext"/> into <paramref name="ciphertext"/> and writes
	///     the 8-byte authentication tag to <paramref name="tag"/>.
	/// </summary>
	/// <param name="generation">The ratchet generation whose key is used.</param>
	/// <param name="plaintext">Data to encrypt.</param>
	/// <param name="ciphertext">Output buffer; must be the same length as <paramref name="plaintext"/>.</param>
	/// <param name="tag">Output buffer for the 8-byte auth tag.</param>
	/// <param name="truncatedNonce">32-bit per-frame counter XOR'd into the AES-GCM nonce.</param>
	/// <param name="aad">Additional authenticated data (not encrypted, but authenticated).</param>
	/// <exception cref="ArgumentOutOfRangeException">
	///     Thrown when <paramref name="generation"/> exceeds the highest seen generation by more
	///     than <see cref="KMaxGenerationGap"/>.
	/// </exception>
	public void Encrypt(
		uint generation,
		ReadOnlySpan<byte> plaintext,
		Span<byte> ciphertext,
		Span<byte> tag,
		uint truncatedNonce,
		ReadOnlySpan<byte> aad)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		var (cipher, nonceBase) = this.GetOrCreateCryptor(generation);
		Span<byte> nonce = stackalloc byte[DaveConstants.NonceSize];
		BuildNonce(nonceBase, truncatedNonce, nonce);
		cipher.Encrypt(nonce, plaintext, ciphertext, tag[..DaveConstants.TagSize], aad);

		this.UpdateHighestSeen(generation);
	}

	/// <summary>
	///     Attempts to decrypt <paramref name="ciphertext"/> into <paramref name="plaintext"/>,
	///     verifying the 8-byte <paramref name="tag"/>.
	/// </summary>
	/// <returns><c>true</c> on success; <c>false</c> if authentication fails.</returns>
	public bool TryDecrypt(
		uint generation,
		ReadOnlySpan<byte> ciphertext,
		ReadOnlySpan<byte> tag,
		Span<byte> plaintext,
		uint truncatedNonce,
		ReadOnlySpan<byte> aad)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		var (cipher, nonceBase) = this.GetOrCreateCryptor(generation);
		Span<byte> nonce = stackalloc byte[DaveConstants.NonceSize];
		BuildNonce(nonceBase, truncatedNonce, nonce);

		this.UpdateHighestSeen(generation);
		return cipher.TryDecrypt(nonce, ciphertext, tag[..DaveConstants.TagSize], plaintext, aad);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;
		foreach (var (cipher, _) in this._cryptors.Values)
			cipher.Dispose();

		this._cryptors.Clear();
	}

	// -------------------------------------------------------------------------
	// Private helpers
	// -------------------------------------------------------------------------

	/// <summary>
	///     Builds the 12-byte AES-GCM nonce: <c>nonceBase XOR (0x00…00 ‖ BE(truncatedNonce))</c>.
	/// </summary>
	private static void BuildNonce(byte[] nonceBase, uint truncatedNonce, Span<byte> nonce)
	{
		nonceBase.AsSpan().CopyTo(nonce);

		Span<byte> nonceSuffix = stackalloc byte[4];
		BinaryPrimitives.WriteUInt32BigEndian(nonceSuffix, truncatedNonce);

		for (var i = 0; i < 4; i++)
			nonce[8 + i] ^= nonceSuffix[i];
	}

	/// <summary>
	///     Returns (or creates and caches) the cipher and nonce base for the given generation.
	///     Calls <see cref="HashRatchet.Get"/> at most once per generation.
	/// </summary>
	private (IAeadCipher Cipher, byte[] NonceBase) GetOrCreateCryptor(uint generation)
	{
		if (this._hasSeenAnyGeneration && generation > this._highestSeenGeneration + KMaxGenerationGap)
			throw new ArgumentOutOfRangeException(nameof(generation),
				$"Generation {generation} is more than {KMaxGenerationGap} ahead of the last seen generation {this._highestSeenGeneration}.");

		if (this._cryptors.TryGetValue(generation, out var cached))
			return cached;

		var (key, nonceBase) = this._ratchet.Get(generation);
		var cipher = this._cipherFactory(key);
		var entry = (cipher, nonceBase);
		this._cryptors[generation] = entry;
		return entry;
	}

	/// <summary>
	///		Updates the highest seen generation if <paramref name="generation"/> is greater than the current highest.
	/// </summary>
	/// <param name="generation">The generation to compare against the current highest.</param>
	private void UpdateHighestSeen(uint generation)
	{
		if (!this._hasSeenAnyGeneration || generation > this._highestSeenGeneration)
		{
			this._highestSeenGeneration = generation;
			this._hasSeenAnyGeneration = true;
		}
	}
}
