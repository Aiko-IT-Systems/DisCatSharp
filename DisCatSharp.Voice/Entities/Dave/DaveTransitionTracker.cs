using System.Collections.Generic;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Tracks pending DAVE protocol version transitions.
/// </summary>
/// <remarks>
///     <para>
///         When the server sends OP 21 (<c>dave_mls_prepare_transition</c>), the client records
///         <c>(transitionId, targetVersion)</c>. When OP 22 (<c>dave_mls_execute_transition</c>)
///         arrives with a matching <c>transitionId</c>, <see cref="TryConsume"/> retrieves and
///         removes the entry so it can be acted on exactly once.
///     </para>
///     <para>
///         A <c>transitionId</c> of <c>0</c> means no OP 23 acknowledgement should be sent.
///     </para>
/// </remarks>
internal sealed class DaveTransitionTracker
{
	private readonly Dictionary<ushort, ushort> _pending = [];

	/// <summary>
	///     Records a pending transition.
	/// </summary>
	/// <param name="transitionId">The transition identifier from OP 21.</param>
	/// <param name="targetVersion">The target DAVE protocol version from OP 21.</param>
	public void Record(ushort transitionId, ushort targetVersion)
		=> this._pending[transitionId] = targetVersion;

	/// <summary>
	///     Attempts to consume a previously recorded transition.
	/// </summary>
	/// <param name="transitionId">The transition identifier from OP 22.</param>
	/// <param name="targetVersion">
	///     On success, the target protocol version recorded in the corresponding OP 21.
	/// </param>
	/// <returns>
	///     <see langword="true"/> if a matching transition was found and removed;
	///     <see langword="false"/> if no matching transition was recorded.
	/// </returns>
	public bool TryConsume(ushort transitionId, out ushort targetVersion)
	{
		if (!this._pending.TryGetValue(transitionId, out targetVersion))
			return false;

		this._pending.Remove(transitionId);
		return true;
	}

	/// <summary>
	///     Removes all pending transitions. Called on reset or reconnect.
	/// </summary>
	public void Clear()
		=> this._pending.Clear();

	/// <summary>
	///     Gets the number of pending transitions currently tracked.
	/// </summary>
	public int Count
		=> this._pending.Count;
}
