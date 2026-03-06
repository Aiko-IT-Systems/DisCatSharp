namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Action the caller (<c>VoiceConnection</c>) must take after <see cref="DaveSession.HandleAnnounceCommit"/> returns.
/// </summary>
internal enum DaveAnnounceAction
{
	/// <summary>Nothing to send; state has been handled internally.</summary>
	None,

	/// <summary>
	///     Send OP 23 (<c>dave_protocol_ready_for_transition</c>) back to the server with the same
	///     <c>transitionId</c> that was passed to <see cref="DaveSession.HandleAnnounceCommit"/>.
	/// </summary>
	SendReadyForTransition,

	/// <summary>
	///     The commit was invalid. Send OP 31 (<c>mls_invalid_commit_welcome</c>), then call
	///     <see cref="DaveSession.PrepareKeyPackage"/> and send the returned bytes as OP 26.
	/// </summary>
	Restart,
}
