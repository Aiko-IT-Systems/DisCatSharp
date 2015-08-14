namespace DisCatSharp.Enums;

/// <summary>
/// Represents the default sort order for posts in a forum channel.
/// </summary>
public enum ForumPostSortOrder
{
	/// <summary>
	/// Sort forum posts by activity.
	/// </summary>
	LatestActivity = 0,

	/// <summary>
	/// Sort forum posts by creation time (from most recent to oldest).
	/// </summary>
	CreationDate = 1
}
