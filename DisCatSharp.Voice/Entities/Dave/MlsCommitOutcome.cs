using DisCatSharp.Voice.Interfaces.Dave;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Outcome returned by <see cref="IMlsProvider.ProcessCommit"/> after applying an MLS commit.
/// </summary>
internal readonly struct MlsCommitOutcome
{
	/// <summary>
	///     The commit was rejected (invalid signature, wrong epoch, etc.).
	/// </summary>
	public bool IsFailed { get; init; }

	/// <summary>
	///     The commit was intentionally ignored (e.g., the session already applied this epoch).
	/// </summary>
	public bool IsIgnored { get; init; }
}
