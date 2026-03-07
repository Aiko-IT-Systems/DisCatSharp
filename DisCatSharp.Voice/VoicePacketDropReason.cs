namespace DisCatSharp.Voice;

/// <summary>
///     Classifies why an inbound voice packet was dropped.
/// </summary>
public enum VoicePacketDropReason
{
	/// <summary>
	///     Packet was not a valid RTP packet.
	/// </summary>
	MalformedRtp = 0,

	/// <summary>
	///     Packet was older than the last accepted sequence and was discarded.
	/// </summary>
	OutOfOrder = 1,

	/// <summary>
	///     Packet RTP extension section was malformed.
	/// </summary>
	MalformedExtension = 2,

	/// <summary>
	///     DAVE is negotiated but not active yet.
	/// </summary>
	DavePending = 3,

	/// <summary>
	///     SSRC-to-user mapping required for DAVE decryption was not available.
	/// </summary>
	MissingSenderMapping = 4,

	/// <summary>
	///     No DAVE decryptor ratchet was available for the sender.
	/// </summary>
	MissingRatchet = 5,

	/// <summary>
	///     Opus decode failed.
	/// </summary>
	DecodeFailure = 6,

	/// <summary>
	///     Producer attempted to enqueue more packets than the configured queue policy allowed.
	/// </summary>
	QueueOverflow = 7
}
