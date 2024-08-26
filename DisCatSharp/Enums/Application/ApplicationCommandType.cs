namespace DisCatSharp.Enums;

/// <summary>
///     Represents the type of an <see cref="DisCatSharp.Entities.DiscordApplicationCommand" />.
/// </summary>
public enum ApplicationCommandType
{
	/// <summary>
	///     Slash commands; a text-based command that shows up when a user types "/".
	/// </summary>
	ChatInput = 1,

	/// <summary>
	///     A UI-based command that shows up when you right click or tap on a user.
	/// </summary>
	User = 2,

	/// <summary>
	///     A UI-based command that shows up when you right click or tap on a message.
	/// </summary>
	Message = 3,

	/// <summary>
	///     A UI-based command that represents the primary way to invoke an app's Activit.
	/// </summary>
	PrimaryEntryPoint = 4
}
