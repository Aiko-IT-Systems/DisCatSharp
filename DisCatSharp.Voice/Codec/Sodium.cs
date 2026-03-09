using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;

using Microsoft.Extensions.Logging;

using DisCatSharp.Voice.Interop;
using DisCatSharp.Voice.Enums.Interop;

namespace DisCatSharp.Voice.Codec;

/// <summary>
///     The sodium.
/// </summary>
// ReSharper disable once ClassCanBeSealed.Global - This class can be used by other projects.
internal class Sodium : IDisposable
{
	/// <summary>
	/// 	The size of the AEAD authentication tag in bytes.
	/// </summary>
	public const int AES_GCM_TAG_SIZE = 16;

	/// <summary>
	/// 	The size of the nonce counter suffix appended to AEAD packets in bytes.
	/// </summary>
	public const int AEAD_NONCE_SUFFIX_SIZE = 4;

	/// <summary>
	///     Gets the buffer.
	/// </summary>
	private readonly byte[] _buffer;

	/// <summary>
	///     Gets the random number generator.
	/// </summary>
	private readonly RandomNumberGenerator _csprng;

	/// <summary>
	///     Gets the key.
	/// </summary>
	private readonly ReadOnlyMemory<byte> _key;

	/// <summary>
	/// 	AES-256-GCM instance for AEAD encryption/decryption.
	/// </summary>
	private readonly AesGcm? _aesGcm;

	/// <summary>
	/// 	Optional logger for one-shot diagnostics.
	/// </summary>
	private readonly ILogger? _logger;

	/// <summary>
	/// 	One-shot guard for DecryptAead diagnostics (0 = not yet logged, 1 = logged).
	/// </summary>
	private static volatile int _decryptDiagLogged;

	/// <summary>
	///     Initializes a new instance of the <see cref="Sodium" /> class.
	/// </summary>
	static Sodium()
	{
		SupportedModes = new ReadOnlyDictionary<string, SodiumEncryptionMode>(new Dictionary<string, SodiumEncryptionMode>
		{
			// AEAD modes listed first — Discord exclusively offers these as of November 2024.
			// Explicit insertion order ensures AES-256-GCM is preferred over XChaCha20 when both are offered.
			// Do not reorder: SelectMode iterates this dict and returns the first match.
			["aead_aes256_gcm_rtpsize"] = SodiumEncryptionMode.AeadAes256GcmRtpSize,
			["aead_xchacha20_poly1305_rtpsize"] = SodiumEncryptionMode.AeadXChaCha20Poly1305RtpSize,
			["xsalsa20_poly1305_lite"] = SodiumEncryptionMode.XSalsa20Poly1305Lite,
			["xsalsa20_poly1305_suffix"] = SodiumEncryptionMode.XSalsa20Poly1305Suffix,
			["xsalsa20_poly1305"] = SodiumEncryptionMode.XSalsa20Poly1305
		});
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="Sodium" /> class.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <param name="logger">Optional logger for one-shot AEAD diagnostics.</param>
	public Sodium(ReadOnlyMemory<byte> key, ILogger? logger = null)
	{
		if (key.Length != SodiumNative.SodiumKeySize)
			throw new ArgumentException($"Invalid Sodium key size. Key needs to have a length of {SodiumNative.SodiumKeySize} bytes.", nameof(key));

		this._key = key;
		this._logger = logger;

		this._csprng = RandomNumberGenerator.Create();
		this._buffer = new byte[SodiumNative.SodiumNonceSize];
		this._aesGcm = new AesGcm(key.Span, AES_GCM_TAG_SIZE);
	}

	/// <summary>
	///     Gets the supported modes.
	/// </summary>
	public static IReadOnlyDictionary<string, SodiumEncryptionMode> SupportedModes { get; }

	/// <summary>
	///     Gets the nonce size.
	/// </summary>
	public static int NonceSize => SodiumNative.SodiumNonceSize;

	/// <summary>Gets the length of the secret key held by this instance.</summary>
	public int KeyLength => this._key.Length;

	/// <summary>
	///     Disposes the Sodium.
	/// </summary>
	public void Dispose()
	{
		this._csprng.Dispose();
		this._aesGcm?.Dispose();
	}

	/// <summary>
	///     Generates the nonce.
	/// </summary>
	/// <param name="rtpHeader">The rtp header.</param>
	/// <param name="target">The target.</param>
	public void GenerateNonce(ReadOnlySpan<byte> rtpHeader, Span<byte> target)
	{
		if (rtpHeader.Length is not Rtp.HEADER_SIZE)
			throw new ArgumentException($"RTP header needs to have a length of exactly {Rtp.HEADER_SIZE} bytes.", nameof(rtpHeader));

		if (target.Length != SodiumNative.SodiumNonceSize)
			throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {SodiumNative.SodiumNonceSize} bytes.", nameof(target));

		// Write the header to the beginning of the span.
		rtpHeader.CopyTo(target);

		// Zero rest of the span.
		Helpers.ZeroFill(target[rtpHeader.Length..]);
	}

	/// <summary>
	///     Generates the nonce.
	/// </summary>
	/// <param name="target">The target.</param>
	public void GenerateNonce(Span<byte> target)
	{
		if (target.Length != SodiumNative.SodiumNonceSize)
			throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {SodiumNative.SodiumNonceSize} bytes.", nameof(target));

		this._csprng.GetBytes(this._buffer);
		this._buffer.AsSpan().CopyTo(target);
	}

	/// <summary>
	///     Generates the nonce.
	/// </summary>
	/// <param name="nonce">The nonce.</param>
	/// <param name="target">The target.</param>
	public void GenerateNonce(uint nonce, Span<byte> target)
	{
		if (target.Length != SodiumNative.SodiumNonceSize)
			throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {SodiumNative.SodiumNonceSize} bytes.", nameof(target));

		// Write the uint to memory
		BinaryPrimitives.WriteUInt32BigEndian(target, nonce);

		// Zero rest of the buffer.
		Helpers.ZeroFill(target[4..]);
	}

	/// <summary>
	///     Appends the nonce.
	/// </summary>
	/// <param name="nonce">The nonce.</param>
	/// <param name="target">The target.</param>
	/// <param name="encryptionMode">The encryption mode.</param>
	public void AppendNonce(ReadOnlySpan<byte> nonce, Span<byte> target, SodiumEncryptionMode encryptionMode)
	{
		switch (encryptionMode)
		{
			case SodiumEncryptionMode.XSalsa20Poly1305:
				return;

			case SodiumEncryptionMode.XSalsa20Poly1305Suffix:
				nonce.CopyTo(target[^12..]);
				return;

			case SodiumEncryptionMode.XSalsa20Poly1305Lite:
				nonce[..4].CopyTo(target[^4..]);
				return;

			default:
				throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
		}
	}

	/// <summary>
	///     Gets the nonce.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="target">The target.</param>
	/// <param name="encryptionMode">The encryption mode.</param>
	public void GetNonce(ReadOnlySpan<byte> source, Span<byte> target, SodiumEncryptionMode encryptionMode)
	{
		if (target.Length != SodiumNative.SodiumNonceSize)
			throw new ArgumentException($"Invalid nonce buffer size. Target buffer for the nonce needs to have a capacity of {SodiumNative.SodiumNonceSize} bytes.", nameof(target));

		switch (encryptionMode)
		{
			case SodiumEncryptionMode.XSalsa20Poly1305:
				source[..12].CopyTo(target);
				return;

			case SodiumEncryptionMode.XSalsa20Poly1305Suffix:
				source[^SodiumNative.SodiumNonceSize..].CopyTo(target);
				return;

			case SodiumEncryptionMode.XSalsa20Poly1305Lite:
				source[^4..].CopyTo(target);
				return;

			default:
				throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
		}
	}

	/// <summary>
	///     Encrypts the Sodium.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="target">The target.</param>
	/// <param name="nonce">The nonce.</param>
	public void Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
	{
		if (nonce.Length != SodiumNative.SodiumNonceSize)
			throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {SodiumNative.SodiumNonceSize} bytes.", nameof(nonce));

		if (target.Length != SodiumNative.SodiumMacSize + source.Length)
			throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is a sum of input buffer length and Sodium MAC size ({SodiumNative.SodiumMacSize} bytes).", nameof(target));

		int result;
		if ((result = SodiumNative.Encrypt(source, target, this._key.Span, nonce)) is not 0)
			throw new CryptographicException($"Could not encrypt the buffer. Sodium returned code {result}.");
	}

	/// <summary>
	///     Decrypts the Sodium.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="target">The target.</param>
	/// <param name="nonce">The nonce.</param>
	public void Decrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> nonce)
	{
		if (nonce.Length != SodiumNative.SodiumNonceSize)
			throw new ArgumentException($"Invalid nonce size. Nonce needs to have a length of {SodiumNative.SodiumNonceSize} bytes.", nameof(nonce));

		if (target.Length != source.Length - SodiumNative.SodiumMacSize)
			throw new ArgumentException($"Invalid target buffer size. Target buffer needs to have a length that is input buffer decreased by Sodium MAC size ({SodiumNative.SodiumMacSize} bytes).", nameof(target));

		int result;
		if ((result = SodiumNative.Decrypt(source, target, this._key.Span, nonce)) is not 0)
			throw new CryptographicException($"Could not decrypt the buffer. Sodium returned code {result}.");
	}

	/// <summary>
	///     Encrypts <paramref name="source" /> using the specified AEAD mode.
	/// </summary>
	/// <param name="source">Opus plaintext payload to encrypt.</param>
	/// <param name="ciphertextDest">Destination span for ciphertext (must equal source.Length).</param>
	/// <param name="tagDest">Destination span for the 16-byte authentication tag.</param>
	/// <param name="nonceCounter4">4-byte little-endian nonce counter to embed in the nonce and append to the packet.</param>
	/// <param name="aad">Additional authenticated data — must be the RTP header bytes.</param>
	/// <param name="mode">The AEAD encryption mode.</param>
	/// <exception cref="CryptographicException">Thrown when encryption fails.</exception>
	public void EncryptAead(ReadOnlySpan<byte> source, Span<byte> ciphertextDest, Span<byte> tagDest, ReadOnlySpan<byte> nonceCounter4, ReadOnlySpan<byte> aad, SodiumEncryptionMode mode)
	{
		if (tagDest.Length != AES_GCM_TAG_SIZE)
			throw new ArgumentException($"Tag buffer must be exactly {AES_GCM_TAG_SIZE} bytes.", nameof(tagDest));
		if (nonceCounter4.Length != AEAD_NONCE_SUFFIX_SIZE)
			throw new ArgumentException($"Nonce counter must be exactly {AEAD_NONCE_SUFFIX_SIZE} bytes.", nameof(nonceCounter4));

		switch (mode)
		{
			case SodiumEncryptionMode.AeadAes256GcmRtpSize:
			{
				// 12-byte AES-GCM nonce: [4-byte LE counter][8 zero bytes]
				// Read as LE uint32 then write back LE so the intent is explicit.
				var counterValue = BinaryPrimitives.ReadUInt32LittleEndian(nonceCounter4);
				Span<byte> nonce = stackalloc byte[12]; // zero-initialized
				BinaryPrimitives.WriteUInt32LittleEndian(nonce, counterValue);
				this._aesGcm!.Encrypt(nonce, source, ciphertextDest, tagDest, aad);
				break;
			}
			case SodiumEncryptionMode.AeadXChaCha20Poly1305RtpSize:
			{
				// 24-byte XChaCha20 nonce: [4-byte LE counter][20 zero bytes]
				var counterValue = BinaryPrimitives.ReadUInt32LittleEndian(nonceCounter4);
				Span<byte> nonce = stackalloc byte[24]; // zero-initialized
				BinaryPrimitives.WriteUInt32LittleEndian(nonce, counterValue);
				int result;
				if ((result = SodiumNative.EncryptXChaCha20(source, ciphertextDest, tagDest, nonce, aad, this._key.Span)) != 0)
					throw new CryptographicException($"XChaCha20 encryption failed with code {result}.");
				break;
			}
			default:
				throw new ArgumentException("EncryptAead called with a non-AEAD mode.", nameof(mode));
		}
	}

	/// <summary>
	///     Decrypts <paramref name="ciphertext" /> using the specified AEAD mode.
	/// </summary>
	/// <param name="ciphertext">Ciphertext payload to decrypt.</param>
	/// <param name="plaintextDest">Destination span for decrypted plaintext (must equal ciphertext.Length).</param>
	/// <param name="tag">16-byte authentication tag.</param>
	/// <param name="nonceCounter4">4-byte little-endian nonce counter read from the end of the received packet.</param>
	/// <param name="aad">Additional authenticated data — must be the RTP header bytes (including any CSRC extension).</param>
	/// <param name="mode">The AEAD encryption mode.</param>
	/// <exception cref="CryptographicException">Thrown when decryption or authentication fails.</exception>
	public void DecryptAead(ReadOnlySpan<byte> ciphertext, Span<byte> plaintextDest, ReadOnlySpan<byte> tag, ReadOnlySpan<byte> nonceCounter4, ReadOnlySpan<byte> aad, SodiumEncryptionMode mode)
	{
		if (tag.Length != AES_GCM_TAG_SIZE)
			throw new ArgumentException($"Tag must be exactly {AES_GCM_TAG_SIZE} bytes.", nameof(tag));
		if (nonceCounter4.Length != AEAD_NONCE_SUFFIX_SIZE)
			throw new ArgumentException($"Nonce counter must be exactly {AEAD_NONCE_SUFFIX_SIZE} bytes.", nameof(nonceCounter4));

		if (this._logger != null && Interlocked.CompareExchange(ref _decryptDiagLogged, 1, 0) == 0)
		{
			var counterValue = BinaryPrimitives.ReadUInt32LittleEndian(nonceCounter4);

			// Build the full nonce so we can log the exact bytes passed to the cipher.
			var nonceLen = mode == SodiumEncryptionMode.AeadAes256GcmRtpSize ? 12 : 24;
			Span<byte> diagNonce = stackalloc byte[nonceLen]; // zero-initialized
			BinaryPrimitives.WriteUInt32LittleEndian(diagNonce, counterValue);

			this._logger.VoiceDebug(
				"[AEAD decrypt diag] mode={Mode} ciphertextLen={CLen} aadLen={AadLen} keyLen={KeyLen}",
				mode, ciphertext.Length, aad.Length, this._key.Length);
			this._logger.VoiceDebug(
				"[AEAD decrypt diag] counter bytes: {Bytes} = LE uint32 {Value}",
				BitConverter.ToString(nonceCounter4.ToArray()), counterValue);
			this._logger.VoiceDebug(
				"[AEAD decrypt diag] nonce bytes ({NonceLen}): {Nonce}",
				nonceLen, BitConverter.ToString(diagNonce.ToArray()));
		}

		switch (mode)
		{
			case SodiumEncryptionMode.AeadAes256GcmRtpSize:
			{
				// 12-byte AES-GCM nonce: [4-byte LE counter][8 zero bytes]
				// Read as LE uint32 then write back LE so the intent is explicit.
				var counterValue = BinaryPrimitives.ReadUInt32LittleEndian(nonceCounter4);
				Span<byte> nonce = stackalloc byte[12]; // zero-initialized
				BinaryPrimitives.WriteUInt32LittleEndian(nonce, counterValue);
				this._aesGcm!.Decrypt(nonce, ciphertext, tag, plaintextDest, aad);
				break;
			}
			case SodiumEncryptionMode.AeadXChaCha20Poly1305RtpSize:
			{
				// 24-byte XChaCha20 nonce: [4-byte LE counter][20 zero bytes]
				var counterValue = BinaryPrimitives.ReadUInt32LittleEndian(nonceCounter4);
				Span<byte> nonce = stackalloc byte[24]; // zero-initialized
				BinaryPrimitives.WriteUInt32LittleEndian(nonce, counterValue);
				int result;
				if ((result = SodiumNative.DecryptXChaCha20(ciphertext, plaintextDest, tag, nonce, aad, this._key.Span)) != 0)
					throw new CryptographicException($"XChaCha20 decryption/authentication failed with code {result}.");
				break;
			}
			default:
				throw new ArgumentException("DecryptAead called with a non-AEAD mode.", nameof(mode));
		}
	}

	/// <summary>
	///     Returns <see langword="true" /> when <paramref name="mode" /> is an AEAD mode
	///     (<see cref="SodiumEncryptionMode.AeadAes256GcmRtpSize" /> or <see cref="SodiumEncryptionMode.AeadXChaCha20Poly1305RtpSize" />).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAeadMode(SodiumEncryptionMode mode)
		=> mode is SodiumEncryptionMode.AeadAes256GcmRtpSize or SodiumEncryptionMode.AeadXChaCha20Poly1305RtpSize;

	/// <summary>
	///     Selects the mode.
	/// </summary>
	/// <param name="availableModes">The available modes.</param>
	/// <returns>A KeyValuePair.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static KeyValuePair<string, SodiumEncryptionMode> SelectMode(IEnumerable<string> availableModes)
	{
		foreach (var kvMode in SupportedModes)
			if (availableModes.Contains(kvMode.Key))
				return kvMode;

		throw new CryptographicException("Could not negotiate Sodium encryption modes, as none of the modes offered by Discord are supported. This is usually an indicator that something went very wrong.");
	}

	/// <summary>
	///     Calculates the target size.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <returns>An int.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateTargetSize(ReadOnlySpan<byte> source)
		=> source.Length + SodiumNative.SodiumMacSize;

	/// <summary>
	///     Calculates the source size.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <returns>An int.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CalculateSourceSize(ReadOnlySpan<byte> source)
		=> source.Length - SodiumNative.SodiumMacSize;
}
