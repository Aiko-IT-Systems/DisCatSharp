using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Payload for voice gateway OP 21 <c>dave_mls_prepare_transition</c>.
///     Signals an upcoming transition to a new DAVE protocol version.
/// </summary>
internal sealed class DavePrepareTransitionPayload
{
	/// <summary>
	///     Gets or sets the transition identifier used to correlate this prepare with the subsequent execute and ack.
	/// </summary>
	[JsonProperty("transition_id")]
	public ushort TransitionId { get; set; }

	/// <summary>
	///     Gets or sets the target DAVE protocol version for this transition.
	/// </summary>
	[JsonProperty("protocol_version")]
	public ushort ProtocolVersion { get; set; }
}
