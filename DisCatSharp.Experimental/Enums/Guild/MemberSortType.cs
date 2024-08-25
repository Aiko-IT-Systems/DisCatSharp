namespace DisCatSharp.Experimental.Enums;

/// <summary>
/// Represents the sorting algorithm used when searching for guild members.
/// </summary>
public enum MemberSortType
{
	/// <summary>
	/// Sort by when the user joined the guild descending (default).
	/// </summary>
	JoinedAtDesc = 1,

	/// <summary>
	/// Sort by when the user joined the guild ascending.
	/// </summary>
	JoinedAtAsc = 2,

	/// <summary>
	/// Sort by when the user joined Discord descending.
	/// </summary>
	UserIdDesc = 3,

	/// <summary>
	/// Sort by when the user joined Discord ascending.
	/// </summary>
	UserIdAsc = 4
}
