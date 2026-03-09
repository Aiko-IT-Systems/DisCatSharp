using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     The voice identify payload.
/// </summary>
internal sealed class VoiceIdentifyPayload
{
	/// <summary>
	///     Gets or sets the server id.
	/// </summary>
	[JsonProperty("server_id")]
	public ulong ServerId { get; set; }

	/// <summary>
	///     Gets or sets the user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; set; }

	/// <summary>
	///     Gets or sets the session id.
	/// </summary>
	[JsonProperty("session_id")]
	public string SessionId { get; set; }

	/// <summary>
	///     Gets or sets the token.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; set; }

	/// <summary>
	///     Gets or sets the maximum DAVE protocol version this client supports.
	///     A value of 0 means DAVE is not supported. Defaults to 0.
	/// </summary>
	[JsonProperty("max_dave_protocol_version")]
	public int MaxDaveProtocolVersion { get; set; }

	/// <summary>
	///     Gets or sets the sequence number of the last numbered message received from the gateway.
	///     Required in Opcode 7 Resume payloads for voice gateway v8 buffered resume.
	///     Omitted from Opcode 0 Identify payloads.
	/// </summary>
	[JsonProperty("seq_ack", NullValueHandling = NullValueHandling.Ignore)]
	public int? SequenceAck { get; set; }
}
