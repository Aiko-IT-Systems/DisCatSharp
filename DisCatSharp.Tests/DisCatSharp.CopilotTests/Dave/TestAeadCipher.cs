using System;
using System.Security.Cryptography;

using DisCatSharp.Voice.Dave;

namespace DisCatSharp.Copilot.Tests.Dave;

/// <summary>
///     Test-only AEAD cipher for Phase 3 pipeline validation.
///     Uses an identity ciphertext transform and an HMAC-SHA256 tag so that the full
///     frame format, ratchet integration, and encrypt/decrypt path can be exercised
///     without requiring native AES-GCM.
/// </summary>
/// <remarks>
///     <b>Not for production use.</b>
///     <list type="bullet">
///         <item>ciphertext = plaintext (identity — no confidentiality)</item>
///         <item>tag = HMAC-SHA256(key, nonce ‖ plaintext)[0..8]</item>
///     </list>
///     Phase 6 will replace this with a real AES-128-GCM implementation via libdave.
/// </remarks>
internal sealed class TestAeadCipher : IAeadCipher
{
	private readonly byte[] _key;

	/// <param name="key">16-byte AES-128 key derived from the ratchet.</param>
	public TestAeadCipher(byte[] key)
	{
		ArgumentNullException.ThrowIfNull(key);
		this._key = key;
	}

	/// <inheritdoc/>
	public void Encrypt(
		ReadOnlySpan<byte> nonce,
		ReadOnlySpan<byte> plaintext,
		Span<byte> ciphertext,
		Span<byte> tag,
		ReadOnlySpan<byte> aad)
	{
		// ciphertext = plaintext (identity transform — no real encryption in tests)
		plaintext.CopyTo(ciphertext);
		this.ComputeTag(nonce, aad, plaintext, tag);
	}

	/// <inheritdoc/>
	public bool TryDecrypt(
		ReadOnlySpan<byte> nonce,
		ReadOnlySpan<byte> ciphertext,
		ReadOnlySpan<byte> tag,
		Span<byte> plaintext,
		ReadOnlySpan<byte> aad)
	{
		// Verify tag against the (identity) ciphertext
		Span<byte> expected = stackalloc byte[DaveConstants.TagSize];
		this.ComputeTag(nonce, aad, ciphertext, expected);

		if (!expected.SequenceEqual(tag))
			return false;

		// ciphertext == plaintext for the identity cipher
		ciphertext.CopyTo(plaintext);
		return true;
	}

	/// <inheritdoc/>
	public void Dispose() { }

	// -------------------------------------------------------------------------

	/// <summary>tag = HMAC-SHA256(_key, nonce ‖ aad ‖ data)[0..8]</summary>
	private void ComputeTag(ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> aad, ReadOnlySpan<byte> data, Span<byte> tag)
	{
		var input = new byte[nonce.Length + aad.Length + data.Length];
		nonce.CopyTo(input);
		aad.CopyTo(input.AsSpan(nonce.Length));
		data.CopyTo(input.AsSpan(nonce.Length + aad.Length));

		var hmac = HMACSHA256.HashData(this._key, input);
		hmac.AsSpan(0, DaveConstants.TagSize).CopyTo(tag);
	}
}
