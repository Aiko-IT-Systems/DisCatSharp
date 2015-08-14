namespace DisCatSharp.Enums;

/// <summary>
/// Represents the interaction context type.
/// </summary>
public enum InteractionContextType
{
	/// <summary>
	/// Command can be used in guilds.
	/// </summary>
	Guild = 0,

	/// <summary>
	/// Command can be used in direct messages with the bot.
	/// </summary>
	BotDm = 1,

	/// <summary>
	/// Command can be used in group direct messages and direct messages.
	/// </summary>
	PrivateChannel = 2
}
