using Microsoft.Extensions.Logging;

namespace DisCatSharp.Voice;

/// <summary>
///     Contains well-defined event IDs used by the Voice extension.
/// </summary>
public static class VoiceEvents
{
	/// <summary>
	///     Miscellaneous events, that do not fit in any other category.
	/// </summary>
	public static EventId Misc { get; } = new(300, "Voice");

	/// <summary>
	///     Events pertaining to Voice Gateway connection lifespan, specifically, heartbeats.
	/// </summary>
	public static EventId VoiceHeartbeat { get; } = new(301, nameof(VoiceHeartbeat));

	/// <summary>
	///     Events pertaining to Voice Gateway connection early lifespan, specifically, the establishing thereof as well as
	///     negotiating various modes.
	/// </summary>
	public static EventId VoiceHandshake { get; } = new(302, nameof(VoiceHandshake));

	/// <summary>
	///     Events emitted when incoming voice data is corrupted, or packets are being dropped.
	/// </summary>
	public static EventId VoiceReceiveFailure { get; } = new(303, nameof(VoiceReceiveFailure));

	/// <summary>
	///     Events emitted when sending a voice packet fails (encryption error, UDP send error, etc.).
	/// </summary>
	public static EventId VoiceSendFailure { get; } = new(315, nameof(VoiceSendFailure));

	/// <summary>
	///     Events pertaining to UDP connection lifespan, specifically the keepalive (or heartbeats).
	/// </summary>
	public static EventId VoiceKeepalive { get; } = new(304, nameof(VoiceKeepalive));

	/// <summary>
	///     Events emitted for high-level dispatch receive events.
	/// </summary>
	public static EventId VoiceDispatch { get; } = new(305, nameof(VoiceDispatch));

	/// <summary>
	///     Events emitted for Voice Gateway connection closes, clean or otherwise.
	/// </summary>
	public static EventId VoiceConnectionClose { get; } = new(306, nameof(VoiceConnectionClose));

	/// <summary>
	///     Events emitted when decoding data received via Voice Gateway fails for any reason.
	/// </summary>
	public static EventId VoiceGatewayError { get; } = new(307, nameof(VoiceGatewayError));

	/// <summary>
	///     Events containing raw (but decompressed) payloads, received from Discord Voice Gateway.
	/// </summary>
	public static EventId VoiceWsRx { get; } = new(308, "Voice ↓");

	/// <summary>
	///     Events containing raw payloads, as they're being sent to Discord Voice Gateway.
	/// </summary>
	public static EventId VoiceWsTx { get; } = new(309, "Voice ↑");

	/// <summary>
	///     Events pertaining to DAVE (Discord Audio/Video Encryption) handshake and session negotiation.
	/// </summary>
	public static EventId DaveHandshake { get; } = new(310, nameof(DaveHandshake));

	/// <summary>
	///     Events pertaining to DAVE key rotation and epoch transitions.
	/// </summary>
	public static EventId DaveKeyRotation { get; } = new(311, nameof(DaveKeyRotation));

	/// <summary>
	///     Events emitted when DAVE frame decryption fails.
	/// </summary>
	public static EventId DaveDecryptFailure { get; } = new(312, nameof(DaveDecryptFailure));

	/// <summary>
	///     Events emitted when the DAVE session is reset (e.g., after reconnect).
	/// </summary>
	public static EventId DaveReset { get; } = new(313, nameof(DaveReset));

	/// <summary>
	///     Events emitted when audio passes through unencrypted (DAVE inactive or DAVE disabled).
	/// </summary>
	public static EventId DavePassthrough { get; } = new(314, nameof(DavePassthrough));
}
