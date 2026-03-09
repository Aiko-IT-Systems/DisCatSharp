namespace DisCatSharp.Voice.Enums.Interop;

/// <summary>
///     Specifies an encryption mode to use with Sodium.
/// </summary>
public enum SodiumEncryptionMode
{
	/// <summary>
	///     AES-256-GCM AEAD mode with RTP-size framing. The 4-byte LE nonce counter followed by the 16-byte auth tag are appended to the payload.
	///     This is the preferred mode when offered by Discord.
	/// </summary>
	AeadAes256GcmRtpSize,

	/// <summary>
	///     XChaCha20-Poly1305 AEAD mode with RTP-size framing. The 4-byte LE nonce counter followed by the 16-byte auth tag are appended to the payload.
	///     Required fallback when AES-256-GCM is not offered.
	/// </summary>
	AeadXChaCha20Poly1305RtpSize,

	/// <summary>
	///     The nonce is an incrementing uint32 value. It is encoded as big endian value at the beginning of the nonce buffer.
	///     The 4 bytes are also appended at the end of the packet.
	/// </summary>
	XSalsa20Poly1305Lite,

	/// <summary>
	///     The nonce consists of random bytes. It is appended at the end of a packet.
	/// </summary>
	XSalsa20Poly1305Suffix,

	/// <summary>
	///     The nonce consists of the RTP header. Nothing is appended to the packet.
	/// </summary>
	XSalsa20Poly1305
}
