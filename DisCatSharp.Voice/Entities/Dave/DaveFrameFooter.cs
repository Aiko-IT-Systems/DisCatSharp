using System;
using System.Collections.Generic;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Holds the parsed fields from the DAVE frame footer appended to each encrypted media packet.
///     The footer contains the authentication tag, the truncated per-frame nonce, any codec-specific
///     unencrypted header ranges, and the two-byte magic marker that identifies the footer.
/// </summary>
internal readonly struct DaveFrameFooter
{
	/// <summary>
	///     Gets the bottom 32 bits of the per-frame counter used as the nonce suffix.
	/// </summary>
	public uint TruncatedNonce { get; init; }

	/// <summary>
	///     Gets the 8-byte AES-GCM authentication tag.
	/// </summary>
	public ReadOnlyMemory<byte> Tag { get; init; }

	/// <summary>
	///     Gets the codec-specific byte ranges within the packet that are left unencrypted (e.g. RTP header extensions).
	/// </summary>
	public IReadOnlyList<(int Offset, int Length)> UnencryptedRanges { get; init; }

	/// <summary>
	///     Gets the total byte size of the footer: tag (8) + nonce (4) + encoded ranges + supplemental-size byte + 2-byte magic marker.
	/// </summary>
	public int FooterSize { get; init; }
}
