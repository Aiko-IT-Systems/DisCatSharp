namespace DisCatSharp.Voice.Enums;

/// <summary>
///     Represents the public DAVE state for a <see cref="VoiceConnection"/>.
/// </summary>
public enum DaveConnectionState
{
	/// <summary>
	///     DAVE is not negotiated for the current voice connection.
	/// </summary>
	NotNegotiated = 0,

	/// <summary>
	///     DAVE is inactive.
	/// </summary>
	Inactive = 1,

	/// <summary>
	///     DAVE is negotiated and waiting for MLS group establishment.
	/// </summary>
	Pending = 2,

	/// <summary>
	///     DAVE is waiting for server response after sending key material.
	/// </summary>
	AwaitingResponse = 3,

	/// <summary>
	///     DAVE receiver transforms are ready for transition execution. Existing encrypted media may
	///     remain active until the voice gateway sends OP 22.
	/// </summary>
	ReadyForTransition = 4,

	/// <summary>
	///     DAVE is active and end-to-end ratchets are installed.
	/// </summary>
	Active = 5,

	/// <summary>
	///     DAVE receivers are prepared for a lower protocol version while the existing sender remains active.
	/// </summary>
	Downgrading = 6
}
