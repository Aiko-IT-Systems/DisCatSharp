namespace DisCatSharp.Voice.Enums.Dave;

/// <summary>
///     Represents the state of a DAVE session FSM.
/// </summary>
internal enum DaveSessionState
{
	/// <summary>
	///     DAVE is not active. All audio passes through unencrypted.
	/// </summary>
	Inactive,

	/// <summary>
	///     DAVE has been activated but the MLS group has not yet been established.
	/// </summary>
	Pending,

	/// <summary>
	///     MLS group establishment is in progress (proposals sent, awaiting commit).
	/// </summary>
	AwaitingResponse,

	/// <summary>
	///     OP 21 (<c>dave_mls_prepare_transition</c>) has been received and recorded.
	///     The session is waiting for OP 22 (<c>dave_mls_execute_transition</c>) before resetting MLS state.
	/// </summary>
	ReadyForTransition,

	/// <summary>
	///     MLS group is established and audio is end-to-end encrypted.
	/// </summary>
	Active,

	/// <summary>
	///     DAVE is being downgraded; transitioning back to a lower or no-op protocol version.
	/// </summary>
	Downgrading,
}
