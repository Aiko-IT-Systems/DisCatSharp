namespace DisCatSharp.Voice.Enums.Dave;

/// <summary>
///     Describes the voice-gateway action required after preparing a DAVE transition.
/// </summary>
internal enum DaveTransitionAction
{
	/// <summary>
	///     No voice-gateway response is required.
	/// </summary>
	None,

	/// <summary>
	///     Send OP 23 for the transition carried by the result.
	/// </summary>
	Ready,

	/// <summary>
	///     Send OP 31 for the transition, reset MLS, and submit a replacement key package.
	/// </summary>
	RecoverInvalid
}
