using System;
using System.Collections.Generic;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     No-op <see cref="IMlsProvider"/> stub used in Phase 4 and Phase 5.
///     MLS operations are not performed; the DAVE session never reaches <see cref="DaveSessionState.Active"/>.
///     Phase 6 replaces this with a libdave-backed implementation.
/// </summary>
internal sealed class NullMlsProvider : IMlsProvider
{
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
