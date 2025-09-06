namespace DisCatSharp.Enums;

/// <summary>
///     Represents a type of component.
/// </summary>
public enum ComponentType
{
	/// <summary>
	///     A row of components.
	/// </summary>
	ActionRow = 1,

	/// <summary>
	///     A button.
	/// </summary>
	Button = 2,

	/// <summary>
	///     A select menu to select strings.
	/// </summary>
	StringSelect = 3,

	/// <summary>
	///     A text input.
	/// </summary>
	TextInput = 4,

	/// <summary>
	///     A select menu to select users.
	/// </summary>
	UserSelect = 5,

	/// <summary>
	///     A select menu to select roles.
	/// </summary>
	RoleSelect = 6,

	/// <summary>
	///     A select menu to select menu to select users and roles.
	/// </summary>
	MentionableSelect = 7,

	/// <summary>
	///     A select menu to select channels.
	/// </summary>
	ChannelSelect = 8,

	/// <summary>
	///     A section.
	/// </summary>
	Section = 9,

	/// <summary>
	///     A text display.
	/// </summary>
	TextDisplay = 10,

	/// <summary>
	///     A thumbnail.
	/// </summary>
	Thumbnail = 11,

	/// <summary>
	///     A media gallery.
	/// </summary>
	MediaGallery = 12,

	/// <summary>
	///     A file.
	/// </summary>
	File = 13,

	/// <summary>
	///     A separator.
	/// </summary>
	Separator = 14,

	/// <summary>
	///     Cannot be used by bots.
	/// </summary>
	ContentInventoryEntry = 16,

	/// <summary>
	///     A container.
	/// </summary>
	Container = 17,

	/// <summary>
	///     A label.
	/// </summary>
	Label = 18
}
