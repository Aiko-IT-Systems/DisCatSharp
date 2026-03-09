using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     The voice dispatch payload.
/// </summary>
internal sealed class VoiceDispatchPayload
{
	/// <summary>
	///     Gets or sets the op code.
	/// </summary>
	[JsonProperty("op")]
	public int OpCode { get; set; }

	/// <summary>
	///     Gets or sets the payload.
	/// </summary>
	[JsonProperty("d")]
	public object Payload { get; set; }

	/// <summary>
	///     Gets or sets the sequence.
	/// </summary>
	[JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
	public int? Sequence { get; set; }

	/// <summary>
	///     Gets or sets the event name.
	/// </summary>
	[JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
	public string EventName { get; set; }
}
