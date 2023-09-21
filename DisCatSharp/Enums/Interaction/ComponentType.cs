namespace DisCatSharp.Enums;

/// <summary>
/// Represents a type of component.
/// </summary>
public enum ComponentType
{
	/// <summary>
	/// A row of components.
	/// </summary>
	ActionRow = 1,

	/// <summary>
	/// A button.
	/// </summary>
	Button = 2,

	/// <summary>
	/// A select menu to select strings.
	/// </summary>
	StringSelect = 3,

	/// <summary>
	/// A input text.
	/// </summary>
	InputText = 4,

	/// <summary>
	/// A select menu to select users.
	/// </summary>
	UserSelect = 5,

	/// <summary>
	/// A select menu to select roles.
	/// </summary>
	RoleSelect = 6,

	/// <summary>
	/// A select menu to select menu to select users and roles.
	/// </summary>
	MentionableSelect = 7,

	/// <summary>
	/// A select menu to select channels.
	/// </summary>
	ChannelSelect = 8
}
