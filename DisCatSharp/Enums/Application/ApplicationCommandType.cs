namespace DisCatSharp.Enums;

/// <summary>
/// Represents the type of an <see cref="DisCatSharp.Entities.DiscordApplicationCommand"/>.
/// </summary>
public enum ApplicationCommandType
{
	/// <summary>
	/// This command is registered as a slash-command, aka "Chat Input".
	/// </summary>
	ChatInput = 1,

	/// <summary>
	/// This command is registered as a user context menu, and is applicable when interacting a user.
	/// </summary>
	User = 2,

	/// <summary>
	/// This command is registered as a message context menu, and is applicable when interacting with a message.
	/// </summary>
	Message = 3,

	/// <summary>
	/// Inbound only: An auto-complete option is being interacted with.
	/// </summary>
	AutoCompleteRequest = 4,

	/// <summary>
	/// Inbound only: A modal was submitted.
	/// </summary>
	ModalSubmit = 5
}
