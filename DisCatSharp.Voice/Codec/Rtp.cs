using System;
using System.Buffers.Binary;

using DisCatSharp.Voice.Enums.Interop;
using DisCatSharp.Voice.Interop;

namespace DisCatSharp.Voice.Codec;

/// <summary>
///     The rtp.
/// </summary>
internal class Rtp : IDisposable
{
	/// <summary>
	///     The header size.
	/// </summary>
	public const int HEADER_SIZE = 12;

	/// <summary>
	///     The rtp no extension.
	/// </summary>
	private const byte RTP_NO_EXTENSION = 0x80;

	/// <summary>
	///     The rtp extension.
	/// </summary>
	private const byte RTP_EXTENSION = 0x90;

	/// <summary>
	///     The rtp version.
	/// </summary>
	private const byte RTP_VERSION = 0x78;

	/// <summary>
	///     Initializes a new instance of the <see cref="Rtp" /> class.
	/// </summary>
	public Rtp()
	{ }

	/// <summary>
	///     Disposes the Rtp.
	/// </summary>
	public void Dispose()
	{ }

	/// <summary>
	///     Encodes the header.
	/// </summary>
	/// <param name="sequence">The sequence.</param>
	/// <param name="timestamp">The timestamp.</param>
	/// <param name="ssrc">The ssrc.</param>
	/// <param name="target">The target.</param>
	public void EncodeHeader(ushort sequence, uint timestamp, uint ssrc, Span<byte> target)
	{
		if (target.Length < HEADER_SIZE)
			throw new ArgumentException("Header buffer is too short.", nameof(target));

		target[0] = RTP_NO_EXTENSION;
		target[1] = RTP_VERSION;

		// Write data big endian
		BinaryPrimitives.WriteUInt16BigEndian(target[2..], sequence); // header + magic
		BinaryPrimitives.WriteUInt32BigEndian(target[4..], timestamp); // header + magic + sizeof(sequence)
		BinaryPrimitives.WriteUInt32BigEndian(target[8..], ssrc); // header + magic + sizeof(sequence) + sizeof(timestamp)
	}

	/// <summary>
	///     Determines whether the buffer starts with a valid RTP header.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <returns><see langword="true"/> when a valid RTP header is present.</returns>
	public bool IsRtpHeader(ReadOnlySpan<byte> source)
		=> source.Length >= HEADER_SIZE && (source[0] is RTP_NO_EXTENSION || source[0] is RTP_EXTENSION) && source[1] is RTP_VERSION;

	/// <summary>
	///     Decodes the header.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="sequence">The sequence.</param>
	/// <param name="timestamp">The timestamp.</param>
	/// <param name="ssrc">The ssrc.</param>
	/// <param name="hasExtension">If true, has extension.</param>
	public void DecodeHeader(ReadOnlySpan<byte> source, out ushort sequence, out uint timestamp, out uint ssrc, out bool hasExtension)
	{
		if (source.Length < HEADER_SIZE)
			throw new ArgumentException("Header buffer is too short.", nameof(source));

		if ((source[0] is not RTP_NO_EXTENSION && source[0] is not RTP_EXTENSION) || source[1] is not RTP_VERSION)
			throw new ArgumentException("Invalid RTP header.", nameof(source));

		hasExtension = source[0] is RTP_EXTENSION;

		// Read data big endian
		sequence = BinaryPrimitives.ReadUInt16BigEndian(source[2..]);
		timestamp = BinaryPrimitives.ReadUInt32BigEndian(source[4..]);
		ssrc = BinaryPrimitives.ReadUInt32BigEndian(source[8..]);
	}

	/// <summary>
	///     Calculates the packet size.
	/// </summary>
	/// <param name="encryptedLength">The encrypted length.</param>
	/// <param name="encryptionMode">The encryption mode.</param>
	/// <returns>An int.</returns>
	public int CalculatePacketSize(int encryptedLength, SodiumEncryptionMode encryptionMode) =>
		encryptionMode switch
		{
			// AEAD: ciphertext (same length as plaintext) + 16-byte detached tag + 4-byte nonce counter suffix
			SodiumEncryptionMode.AeadAes256GcmRtpSize or SodiumEncryptionMode.AeadXChaCha20Poly1305RtpSize
				=> HEADER_SIZE + encryptedLength + Sodium.AES_GCM_TAG_SIZE + Sodium.AEAD_NONCE_SUFFIX_SIZE,
			SodiumEncryptionMode.XSalsa20Poly1305 => HEADER_SIZE + encryptedLength,
			SodiumEncryptionMode.XSalsa20Poly1305Suffix => HEADER_SIZE + encryptedLength + SodiumNative.SodiumNonceSize,
			SodiumEncryptionMode.XSalsa20Poly1305Lite => HEADER_SIZE + encryptedLength + 4,
			_ => throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode))
		};

	/// <summary>
	///     Gets the data from packet.
	/// </summary>
	/// <param name="packet">The packet.</param>
	/// <param name="data">The data.</param>
	/// <param name="encryptionMode">The encryption mode.</param>
	public void GetDataFromPacket(ReadOnlySpan<byte> packet, out ReadOnlySpan<byte> data, SodiumEncryptionMode encryptionMode)
	{
		switch (encryptionMode)
		{
			// AEAD: ciphertext = packet[HEADER_SIZE .. ^(TAG_SIZE + NONCE_SUFFIX)]
			// tag  = packet[^(TAG_SIZE + NONCE_SUFFIX) .. ^NONCE_SUFFIX]  (last 20..4 bytes)
			// counter = packet[^NONCE_SUFFIX ..]                           (last 4 bytes)
			case SodiumEncryptionMode.AeadAes256GcmRtpSize:
			case SodiumEncryptionMode.AeadXChaCha20Poly1305RtpSize:
				data = packet.Slice(HEADER_SIZE, packet.Length - HEADER_SIZE - Sodium.AES_GCM_TAG_SIZE - Sodium.AEAD_NONCE_SUFFIX_SIZE);
				return;

			case SodiumEncryptionMode.XSalsa20Poly1305:
				data = packet[HEADER_SIZE..];
				return;

			case SodiumEncryptionMode.XSalsa20Poly1305Suffix:
				data = packet.Slice(HEADER_SIZE, packet.Length - HEADER_SIZE - SodiumNative.SodiumNonceSize);
				return;

			case SodiumEncryptionMode.XSalsa20Poly1305Lite:
				data = packet.Slice(HEADER_SIZE, packet.Length - HEADER_SIZE - 4);
				return;

			default:
				throw new ArgumentException("Unsupported encryption mode.", nameof(encryptionMode));
		}
	}
}
