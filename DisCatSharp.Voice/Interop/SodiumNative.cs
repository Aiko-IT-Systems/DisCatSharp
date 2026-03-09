using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.Voice.Interop;

/// <summary>
///     Native libsodium bindings and helpers used by the voice encryption pipeline.
/// </summary>
internal static class SodiumNative
{
	/// <summary>
	///     The sodium library name.
	/// </summary>
	private const string SODIUM_LIBRARY_NAME = "libsodium";

	/// <summary>
	///     Gets the Sodium key size for xsalsa20_poly1305 algorithm.
	/// </summary>
	public static int SodiumKeySize { get; } = (int)_SodiumSecretBoxKeySize();

	/// <summary>
	///     Gets the Sodium nonce size for xsalsa20_poly1305 algorithm.
	/// </summary>
	public static int SodiumNonceSize { get; } = (int)_SodiumSecretBoxNonceSize();

	/// <summary>
	///     Gets the Sodium MAC size for xsalsa20_poly1305 algorithm.
	/// </summary>
	public static int SodiumMacSize { get; } = (int)_SodiumSecretBoxMacSize();

	/// <summary>
	///     Gets the libsodium secretbox key size.
	/// </summary>
	/// <returns>An UIntPtr.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_keybytes")]
	[return: MarshalAs(UnmanagedType.SysUInt)]
	private static extern UIntPtr _SodiumSecretBoxKeySize();

	/// <summary>
	///     Gets the libsodium secretbox nonce size.
	/// </summary>
	/// <returns>An UIntPtr.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_noncebytes")]
	[return: MarshalAs(UnmanagedType.SysUInt)]
	private static extern UIntPtr _SodiumSecretBoxNonceSize();

	/// <summary>
	///     Gets the libsodium secretbox MAC size.
	/// </summary>
	/// <returns>An UIntPtr.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_xsalsa20poly1305_macbytes")]
	[return: MarshalAs(UnmanagedType.SysUInt)]
	private static extern UIntPtr _SodiumSecretBoxMacSize();

	/// <summary>
	///     Native binding for <c>crypto_secretbox_easy</c>.
	/// </summary>
	/// <param name="buffer">The buffer.</param>
	/// <param name="message">The message.</param>
	/// <param name="messageLength">The message length.</param>
	/// <param name="nonce">The nonce.</param>
	/// <param name="key">The key.</param>
	/// <returns>An int.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
	private static extern unsafe int _SodiumSecretBoxCreate(byte* buffer, byte* message, ulong messageLength, byte* nonce, byte* key);

	/// <summary>
	///     Native binding for <c>crypto_secretbox_open_easy</c>.
	/// </summary>
	/// <param name="buffer">The buffer.</param>
	/// <param name="encryptedMessage">The encrypted message.</param>
	/// <param name="encryptedLength">The encrypted length.</param>
	/// <param name="nonce">The nonce.</param>
	/// <param name="key">The key.</param>
	/// <returns>An int.</returns>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_open_easy")]
	private static extern unsafe int _SodiumSecretBoxOpen(byte* buffer, byte* encryptedMessage, ulong encryptedLength, byte* nonce, byte* key);

	/// <summary>
	///     Encrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform encryption.
	/// </summary>
	/// <param name="source">Contents to encrypt.</param>
	/// <param name="target">Buffer to encrypt to.</param>
	/// <param name="key">Key to use for encryption.</param>
	/// <param name="nonce">Nonce to use for encryption.</param>
	/// <returns>Encryption status.</returns>
	public static unsafe int Encrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
	{
		var status = 0;
		fixed (byte* sourcePtr = &source.GetPinnableReference())
		fixed (byte* targetPtr = &target.GetPinnableReference())
		fixed (byte* keyPtr = &key.GetPinnableReference())
		fixed (byte* noncePtr = &nonce.GetPinnableReference())
		{
			status = _SodiumSecretBoxCreate(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);
		}

		return status;
	}

	/// <summary>
	///     Decrypts supplied buffer using xsalsa20_poly1305 algorithm, using supplied key and nonce to perform decryption.
	/// </summary>
	/// <param name="source">Buffer to decrypt from.</param>
	/// <param name="target">Decrypted message buffer.</param>
	/// <param name="key">Key to use for decryption.</param>
	/// <param name="nonce">Nonce to use for decryption.</param>
	/// <returns>Decryption status.</returns>
	public static unsafe int Decrypt(ReadOnlySpan<byte> source, Span<byte> target, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
	{
		var status = 0;
		fixed (byte* sourcePtr = &source.GetPinnableReference())
		fixed (byte* targetPtr = &target.GetPinnableReference())
		fixed (byte* keyPtr = &key.GetPinnableReference())
		fixed (byte* noncePtr = &nonce.GetPinnableReference())
		{
			status = _SodiumSecretBoxOpen(targetPtr, sourcePtr, (ulong)source.Length, noncePtr, keyPtr);
		}

		return status;
	}

	/// <summary>
	///     Encrypts using XChaCha20-Poly1305-IETF (detached MAC).
	/// </summary>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl,
		EntryPoint = "crypto_aead_xchacha20poly1305_ietf_encrypt_detached")]
	private static extern unsafe int _XChaCha20EncryptDetached(
		byte* c, byte* mac, ulong* maclen_p,
		byte* m, ulong mlen,
		byte* ad, ulong adlen,
		byte* nsec, byte* npub, byte* k);

	/// <summary>
	///     Decrypts using XChaCha20-Poly1305-IETF (detached MAC).
	/// </summary>
	[DllImport(SODIUM_LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl,
		EntryPoint = "crypto_aead_xchacha20poly1305_ietf_decrypt_detached")]
	private static extern unsafe int _XChaCha20DecryptDetached(
		byte* m, byte* nsec,
		byte* c, ulong clen,
		byte* mac, byte* ad, ulong adlen,
		byte* npub, byte* k);

	/// <summary>
	///     Encrypts <paramref name="source" /> using XChaCha20-Poly1305-IETF with a detached MAC.
	/// </summary>
	/// <param name="source">Plaintext to encrypt.</param>
	/// <param name="ciphertext">Output buffer for ciphertext (same length as source).</param>
	/// <param name="tag">Output buffer for the 16-byte auth tag.</param>
	/// <param name="nonce">24-byte nonce.</param>
	/// <param name="aad">Additional authenticated data (RTP header).</param>
	/// <param name="key">32-byte encryption key.</param>
	/// <returns>0 on success, non-zero on failure.</returns>
	public static unsafe int EncryptXChaCha20(ReadOnlySpan<byte> source, Span<byte> ciphertext, Span<byte> tag, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> aad, ReadOnlySpan<byte> key)
	{
		if (nonce.Length != 24)
			throw new ArgumentException("XChaCha20 nonce must be exactly 24 bytes.", nameof(nonce));
		if (key.Length != 32)
			throw new ArgumentException("XChaCha20 key must be exactly 32 bytes.", nameof(key));
		if (tag.Length != 16)
			throw new ArgumentException("XChaCha20 tag buffer must be exactly 16 bytes.", nameof(tag));

		ulong maclen = 0;
		fixed (byte* srcPtr = &source.GetPinnableReference())
		fixed (byte* cPtr = &ciphertext.GetPinnableReference())
		fixed (byte* macPtr = &tag.GetPinnableReference())
		fixed (byte* noncePtr = &nonce.GetPinnableReference())
		fixed (byte* aadPtr = &aad.GetPinnableReference())
		fixed (byte* keyPtr = &key.GetPinnableReference())
		{
			return _XChaCha20EncryptDetached(cPtr, macPtr, &maclen, srcPtr, (ulong)source.Length, aadPtr, (ulong)aad.Length, null, noncePtr, keyPtr);
		}
	}

	/// <summary>
	///     Decrypts <paramref name="ciphertext" /> using XChaCha20-Poly1305-IETF with a detached MAC.
	/// </summary>
	/// <param name="ciphertext">Ciphertext to decrypt.</param>
	/// <param name="plaintext">Output buffer for plaintext (same length as ciphertext).</param>
	/// <param name="tag">16-byte auth tag.</param>
	/// <param name="nonce">24-byte nonce.</param>
	/// <param name="aad">Additional authenticated data (RTP header).</param>
	/// <param name="key">32-byte decryption key.</param>
	/// <returns>0 on success, non-zero on failure (authentication failed).</returns>
	public static unsafe int DecryptXChaCha20(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext, ReadOnlySpan<byte> tag, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> aad, ReadOnlySpan<byte> key)
	{
		if (nonce.Length != 24)
			throw new ArgumentException("XChaCha20 nonce must be exactly 24 bytes.", nameof(nonce));
		if (key.Length != 32)
			throw new ArgumentException("XChaCha20 key must be exactly 32 bytes.", nameof(key));
		if (tag.Length != 16)
			throw new ArgumentException("XChaCha20 tag must be exactly 16 bytes.", nameof(tag));

		fixed (byte* mPtr = &plaintext.GetPinnableReference())
		fixed (byte* cPtr = &ciphertext.GetPinnableReference())
		fixed (byte* macPtr = &tag.GetPinnableReference())
		fixed (byte* noncePtr = &nonce.GetPinnableReference())
		fixed (byte* aadPtr = &aad.GetPinnableReference())
		fixed (byte* keyPtr = &key.GetPinnableReference())
		{
			return _XChaCha20DecryptDetached(mPtr, null, cPtr, (ulong)ciphertext.Length, macPtr, aadPtr, (ulong)aad.Length, noncePtr, keyPtr);
		}
	}
}
