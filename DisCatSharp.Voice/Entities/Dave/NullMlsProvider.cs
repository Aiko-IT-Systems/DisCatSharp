using System;
using System.Collections.Generic;

using DisCatSharp.Voice.Interfaces.Dave;

using DisCatSharp.Voice.Enums.Dave;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     No-op <see cref="IMlsProvider"/> implementation used when native MLS support is unavailable.
///     MLS operations are not performed; the DAVE session does not transition to <see cref="DaveSessionState.Active"/>.
/// </summary>
internal sealed class NullMlsProvider : IMlsProvider
{
	/// <inheritdoc/>
	public bool IsSessionInitialized => true;

	/// <inheritdoc/>
	public bool IsGroupReady => false;

	/// <inheritdoc/>
	public void InitGroup(ulong selfUserId, int protocolVersion, byte[] groupId) { }

	/// <inheritdoc/>
	public void SetExternalSender(byte[] externalSenderBytes) { }

	/// <inheritdoc/>
	public byte[] GetKeyPackage() => [];

	/// <inheritdoc/>
	public MlsCommitResult ProcessProposals(byte[] proposalsBytes, IReadOnlySet<ulong> recognizedUserIds)
		=> new() { CommitBytes = [], WelcomeBytes = null };

	/// <inheritdoc/>
	public MlsCommitOutcome ProcessCommit(byte[] commitBytes) => default;

	/// <inheritdoc/>
	public void ProcessWelcome(byte[] welcomeBytes, byte[] ratchetKey, IReadOnlySet<ulong> recognizedUserIds) { }

	/// <inheritdoc/>
	public IReadOnlyList<(ulong UserId, DaveRatchetInstaller Installer)> GetUpdatedRatchets()
		=> [];

	/// <inheritdoc/>
	public DaveRatchetInstaller GetOwnRatchetInstaller()
		=> DaveRatchetInstaller.FromManaged(Array.Empty<byte>());

	/// <inheritdoc/>
	public void Reset() { }
}
