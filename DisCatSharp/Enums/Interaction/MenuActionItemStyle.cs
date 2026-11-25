namespace DisCatSharp.Enums;

/// <summary>
///     Represents the action performed by an overflow menu item.
/// </summary>
public enum MenuActionItemStyle
{
	/// <summary>
	///     Sends a component interaction when selected.
	/// </summary>
	Default = 1,

	/// <summary>
	///     Sends a component interaction for a destructive action.
	/// </summary>
	Danger = 2,

	/// <summary>
	///     Opens the item's URL without sending a component interaction.
	/// </summary>
	Link = 3
}
