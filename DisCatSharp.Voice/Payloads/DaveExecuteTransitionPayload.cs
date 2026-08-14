using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Payload for voice gateway OP 22 <c>dave_mls_execute_transition</c>.
///     Instructs the client to execute the previously prepared DAVE protocol version transition.
///     OP 23 readiness is sent during preparation, before this opcode arrives; OP 22 never receives an acknowledgement.
/// </summary>
internal sealed class DaveExecuteTransitionPayload
{
	/// <summary>
	///     Gets or sets the transition identifier. A value of 0 means no OP 23 acknowledgement should be sent.
	/// </summary>
	[JsonProperty("transition_id")]
	public ushort TransitionId { get; set; }
}
