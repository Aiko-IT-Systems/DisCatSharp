using System;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     No-op <see cref="IAeadCipher"/> placeholder used when the cipher backend is not yet configured.
///     With <see cref="NullMlsProvider"/>, ratchets are never installed so this cipher is never invoked.
///     Replace with a libdave-backed cipher before enabling encryption.
/// </summary>
internal sealed class NullAeadCipher : IAeadCipher
{
	/// <inheritdoc/>
	public void Encrypt(
		ReadOnlySpan<byte> nonce,
		ReadOnlySpan<byte> plaintext,
		Span<byte> ciphertext,
		Span<byte> tag,
		ReadOnlySpan<byte> aad)
		=> throw new NotSupportedException("NullAeadCipher must not be used for actual encryption. Replace with a real cipher.");

	/// <inheritdoc/>
	public bool TryDecrypt(
		ReadOnlySpan<byte> nonce,
		ReadOnlySpan<byte> ciphertext,
		ReadOnlySpan<byte> tag,
		Span<byte> plaintext,
		ReadOnlySpan<byte> aad)
		=> throw new NotSupportedException("NullAeadCipher must not be used for actual decryption. Replace with a real cipher.");

	/// <inheritdoc/>
	public void Dispose() { }
}
