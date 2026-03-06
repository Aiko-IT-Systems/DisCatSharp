using System;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Abstraction over an AEAD (Authenticated Encryption with Associated Data) cipher
///     used by the DAVE frame encryption pipeline.
/// </summary>
/// <remarks>
///     <para>
///         All DAVE frames use an 8-byte authentication tag on the wire (<see cref="DaveConstants.TagSize"/>).
///         Implementors must produce and verify exactly 8-byte tags.
///     </para>
///     <para>
///         Tests may supply <c>TestAeadCipher</c> (test-only, identity ciphertext + HMAC-SHA256 tag).
///         Production builds use an AES-128-GCM implementation via libdave.
///     </para>
/// </remarks>
internal interface IAeadCipher : IDisposable
{
	/// <summary>
	///     Encrypts <paramref name="plaintext"/> into <paramref name="ciphertext"/> and writes the
	///     8-byte authentication tag into <paramref name="tag"/>.
	/// </summary>
	/// <param name="nonce">12-byte AES-GCM nonce derived from the ratchet and frame counter.</param>
	/// <param name="plaintext">Data to encrypt.</param>
	/// <param name="ciphertext">Output buffer; must be the same length as <paramref name="plaintext"/>.</param>
	/// <param name="tag">Output buffer; must be exactly <see cref="DaveConstants.TagSize"/> (8) bytes.</param>
	/// <param name="aad">Additional authenticated data (unencrypted but covered by the tag).</param>
	void Encrypt(
		ReadOnlySpan<byte> nonce,
		ReadOnlySpan<byte> plaintext,
		Span<byte> ciphertext,
		Span<byte> tag,
		ReadOnlySpan<byte> aad);

	/// <summary>
	///     Decrypts <paramref name="ciphertext"/> into <paramref name="plaintext"/>, verifying the
	///     8-byte <paramref name="tag"/>.
	/// </summary>
	/// <returns><see langword="true"/> on success; <see langword="false"/> if authentication fails.</returns>
	bool TryDecrypt(
		ReadOnlySpan<byte> nonce,
		ReadOnlySpan<byte> ciphertext,
		ReadOnlySpan<byte> tag,
		Span<byte> plaintext,
		ReadOnlySpan<byte> aad);
}
