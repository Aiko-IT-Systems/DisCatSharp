
namespace DisCatSharp.Enums;

/// <summary>
/// Represents where application commands can be used.
/// </summary>
public enum ApplicationCommandContexts : int
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
