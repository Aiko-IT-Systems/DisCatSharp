namespace DisCatSharp.Enums;

/// <summary>
/// The metadata visibility type of user account connections.
/// </summary>
public enum ConnectionMetadataVisibilityType
{
	/// <summary>
	/// This connections metadata is only visible to the owning user.
	/// </summary>
	None = 0,

	/// <summary>
	/// This connections metadata is visible to everyone.
	/// </summary>
	Everyone = 1
}
