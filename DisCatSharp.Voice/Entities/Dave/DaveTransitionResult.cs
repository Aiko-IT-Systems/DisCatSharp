using DisCatSharp.Voice.Enums.Dave;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Carries the action and authoritative transition ID produced by a DAVE preparation handler.
/// </summary>
/// <param name="Action">The voice-gateway action the caller must perform.</param>
/// <param name="TransitionId">The transition ID received with the message that produced the result.</param>
internal readonly record struct DaveTransitionResult(DaveTransitionAction Action, ushort TransitionId)
{
	/// <summary>
	///     Creates a result that requires no voice-gateway response.
	/// </summary>
	/// <param name="transitionId">The authoritative transition ID.</param>
	/// <returns>A result whose <see cref="Action"/> is <see cref="DaveTransitionAction.None"/>.</returns>
	public static DaveTransitionResult None(ushort transitionId)
		=> new(DaveTransitionAction.None, transitionId);

	/// <summary>
	///     Creates a result that requires an OP 23 ready notification.
	/// </summary>
	/// <param name="transitionId">The authoritative transition ID.</param>
	/// <returns>A result whose <see cref="Action"/> is <see cref="DaveTransitionAction.Ready"/>.</returns>
	public static DaveTransitionResult Ready(ushort transitionId)
		=> new(DaveTransitionAction.Ready, transitionId);

	/// <summary>
	///     Creates a result that requires invalid commit or Welcome recovery.
	/// </summary>
	/// <param name="transitionId">The authoritative transition ID.</param>
	/// <returns>A result whose <see cref="Action"/> is <see cref="DaveTransitionAction.RecoverInvalid"/>.</returns>
	public static DaveTransitionResult RecoverInvalid(ushort transitionId)
		=> new(DaveTransitionAction.RecoverInvalid, transitionId);
}
