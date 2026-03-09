using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Payload for voice gateway OP 22 <c>dave_mls_execute_transition</c>.
///     Instructs the client to execute the previously prepared DAVE protocol version transition.
///     When <see cref="TransitionId"/> is 0, the client should skip sending OP 23 (ready_for_transition) acknowledgement.
/// </summary>
internal sealed class DaveExecuteTransitionPayload
{
	/// <summary>
	///     Gets or sets the transition identifier. A value of 0 means no OP 23 acknowledgement should be sent.
	/// </summary>
	[JsonProperty("transition_id")]
	public ushort TransitionId { get; set; }
}
