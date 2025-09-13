namespace DisCatSharp.Enums;

/// <summary>
///     Represents a high-level Discord media type category for attachments.
/// </summary>
public enum DiscordMediaType
{
	/// <summary>
	///     Uncategorized or unknown type.
	/// </summary>
	Other,

	/// <summary>
	///     File types such as text and model files.
	/// </summary>
	File,

	/// <summary>
	///     Media types such as video and image files.
	/// </summary>
	Media,

	/// <summary>
	///     Audio types such as audio and voice messages.
	/// </summary>
	Audio,

	/// <summary>
	///     Executable or application files.
	/// </summary>
	Executable
}
