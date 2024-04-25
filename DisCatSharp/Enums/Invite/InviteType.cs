using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the invite type .
/// </summary>
public enum InviteType
{
	/// <summary>
	/// Represents a guild invite.
	/// </summary>
	Guild = 0,

	/// <summary>
	/// Represents a group dm invite.
	/// </summary>
	GroupDm = 1,

	/// <summary>
	/// Represents a friend invite.
	/// </summary>
	Friend = 2,

	/// <inheritdoc cref="Friend"/>
	[Deprecated("We used the wrong name.")]
	User = Friend
}
