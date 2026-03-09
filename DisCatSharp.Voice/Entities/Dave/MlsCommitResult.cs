using DisCatSharp.Voice.Interfaces.Dave;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Result returned by <see cref="IMlsProvider.ProcessProposals"/> after generating an MLS commit.
/// </summary>
internal readonly struct MlsCommitResult
{
	/// <summary>
	///     The serialised MLS commit message to send to the server (OP 28).
	/// </summary>
	public byte[] CommitBytes { get; init; }

	/// <summary>
	///     The serialised MLS welcome message to send to newly-added members, or <see langword="null"/>
	///     if no new members were added in this commit.
	/// </summary>
	public byte[]? WelcomeBytes { get; init; }
}
