
namespace DisCatSharp.Enums;

/// <summary>
/// Represents where application commands can be used.
/// </summary>
public enum ApplicationCommandContexts : int
{
	/// <summary>
	/// Command can be used in guilds.
	/// </summary>
	Guilds = 0,

	/// <summary>
	/// Command can be used in direct messages.
	/// </summary>
	DirectMessages = 1,

	/// <summary>
	/// Command can be used in group direct messages and direct messages.
	/// </summary>
	PrivateChannels = 2
}
