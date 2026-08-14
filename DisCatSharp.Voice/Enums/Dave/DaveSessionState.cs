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
	///     Receiver transforms and ratchets are prepared and OP 23 may be sent. The existing sender
	///     transform remains unchanged until OP 22 executes the transition.
	/// </summary>
	ReadyForTransition,

	/// <summary>
	///     MLS group is established and audio is end-to-end encrypted.
	/// </summary>
	Active,

	/// <summary>
	///     Receiver transforms are prepared for a lower or no-op protocol version while the existing
	///     sender continues using its executing version until OP 22.
	/// </summary>
	Downgrading,
}
