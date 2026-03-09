using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     The voice heartbeat payload for voice gateway v8.
///     Since v8, heartbeats must include a nonce (<c>t</c>) and
///     the sequence number of the last numbered message received (<c>seq_ack</c>).
/// </summary>
internal sealed class VoiceHeartbeatPayload
{
	/// <summary>
	///     Gets or sets the heartbeat nonce (Unix timestamp in seconds).
	/// </summary>
	[JsonProperty("t")]
	public uint Nonce { get; set; }

	/// <summary>
	///     Gets or sets the sequence number of the last numbered message received from the gateway.
	///     Must be -1 (or omitted) if no sequence-numbered messages have been received yet.
	/// </summary>
	[JsonProperty("seq_ack")]
	public int SequenceAck { get; set; }
}
