using System.Collections.Generic;
using DisCatSharp.Voice.Entities.Dave;

namespace DisCatSharp.Voice.Interfaces.Dave;

/// <summary>
///     Abstraction over MLS group operations for the DAVE protocol.
/// </summary>
/// <remarks>
///     Production builds typically provide a native-backed implementation.
///     Non-native builds may use <see cref="NullMlsProvider"/> as a no-op fallback.
/// </remarks>
internal interface IMlsProvider
{
	/// <summary>
	///     Gets whether the MLS session has already been initialized via <see cref="InitGroup"/>.
	/// </summary>
	bool IsSessionInitialized { get; }

	/// <summary>
	///     Gets whether the MLS group has been successfully established and is ready for use.
	/// </summary>
	bool IsGroupReady { get; }

	/// <summary>
	///     Initialises a new MLS group for this session.
	/// </summary>
	/// <param name="selfUserId">The Discord user ID of the local participant.</param>
	/// <param name="protocolVersion">The negotiated DAVE protocol version.</param>
	/// <param name="groupId">The opaque group identifier supplied by the server.</param>
	void InitGroup(ulong selfUserId, int protocolVersion, byte[] groupId);

	/// <summary>
	///     Stores the external sender credential received from the server (binary OP 25).
	/// </summary>
	void SetExternalSender(byte[] externalSenderBytes);

	/// <summary>
	///     Returns the serialised MLS key package to send to the server during group join.
	/// </summary>
	byte[] GetKeyPackage();

	/// <summary>
	///     Processes a set of MLS proposals received from the server (binary OP 27) and generates a commit.
	/// </summary>
	/// <param name="proposalsBytes">The raw proposals payload.</param>
	/// <param name="recognizedUserIds">The set of user IDs currently recognised by the server (from OP 11).</param>
	/// <returns>A <see cref="MlsCommitResult"/> containing the commit and optional welcome bytes.</returns>
	MlsCommitResult ProcessProposals(byte[] proposalsBytes, IReadOnlySet<ulong> recognizedUserIds);

	/// <summary>
	///     Applies an MLS commit received from the server (binary OP 29).
	/// </summary>
	/// <returns>A <see cref="MlsCommitOutcome"/> indicating whether the commit succeeded, was ignored, or failed.</returns>
	MlsCommitOutcome ProcessCommit(byte[] commitBytes);

	/// <summary>
	///     Processes an MLS welcome message, joining the group as a new member (binary OP 30).
	/// </summary>
	/// <param name="welcomeBytes">The serialised MLS welcome message.</param>
	/// <param name="ratchetKey">The ratchet key to bootstrap media encryption after joining.</param>
	/// <param name="recognizedUserIds">The set of user IDs currently recognised by the server (from OP 11).</param>
	void ProcessWelcome(byte[] welcomeBytes, byte[] ratchetKey, IReadOnlySet<ulong> recognizedUserIds);

	/// <summary>
	///     Returns the updated per-user ratchet secrets after a commit has been applied.
	///     Each tuple contains the Discord user ID and its new 32-byte ratchet secret.
	/// </summary>
	IReadOnlyList<(ulong UserId, DaveRatchetInstaller Installer)> GetUpdatedRatchets();

	/// <summary>
	///     Returns a <see cref="DaveRatchetInstaller"/> for the local participant's own outbound ratchet for this epoch.
	/// </summary>
	DaveRatchetInstaller GetOwnRatchetInstaller();

	/// <summary>
	///     Resets all MLS state. Called on reconnect, session downgrade, or invalid commit.
	/// </summary>
	void Reset();
}
