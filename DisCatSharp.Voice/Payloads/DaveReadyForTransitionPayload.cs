using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Payload for voice gateway OP 23 <c>dave_mls_ready_for_transition</c> (client-to-server).
///     Acknowledges receipt of OP 22 (<c>dave_mls_execute_transition</c>).
///     Must NOT be sent when the received <c>transition_id</c> was 0.
/// </summary>
internal sealed class DaveReadyForTransitionPayload
{
	/// <summary>
	///     Gets or sets the transition identifier from the corresponding OP 22 payload.
	/// </summary>
	[JsonProperty("transition_id")]
	public ushort TransitionId { get; set; }
}
