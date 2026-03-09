namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Protocol-level constants for the DAVE (Discord Audio/Video Encryption) frame format.
///     These values are fixed by the DAVE wire format and must not vary by cipher implementation.
/// </summary>
internal static class DaveConstants
{
	/// <summary>
	///     Authentication tag size in bytes on the DAVE wire format.
	///     DAVE always uses 8-byte truncated AES-128-GCM tags, regardless of the underlying crypto backend.
	/// </summary>
	internal const int TagSize = 8;

	/// <summary>
	///     AES-GCM nonce length in bytes (96-bit nonce, fixed by AES-GCM spec).
	/// </summary>
	internal const int NonceSize = 12;

	/// <summary>
	///     AES-128-GCM key length in bytes.
	/// </summary>
	internal const int KeySize = 16;
}
