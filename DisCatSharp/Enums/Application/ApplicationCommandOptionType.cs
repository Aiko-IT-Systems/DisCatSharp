namespace DisCatSharp.Enums;

/// <summary>
/// Represents the type of parameter when invoking an interaction.
/// </summary>
public enum ApplicationCommandOptionType
{
	/// <summary>
	/// Whether this parameter is another subcommand.
	/// </summary>
	SubCommand = 1,

	/// <summary>
	/// Whether this parameter is apart of a subcommand group.
	/// </summary>
	SubCommandGroup = 2,

	/// <summary>
	/// Whether this parameter is a string.
	/// </summary>
	String = 3,

	/// <summary>
	/// Whether this parameter is an integer.
	/// </summary>
	Integer = 4,

	/// <summary>
	/// Whether this parameter is a boolean.
	/// </summary>
	Boolean = 5,

	/// <summary>
	/// Whether this parameter is a Discord user.
	/// </summary>
	User = 6,

	/// <summary>
	/// Whether this parameter is a Discord channel.
	/// </summary>
	Channel = 7,

	/// <summary>
	/// Whether this parameter is a Discord role.
	/// </summary>
	Role = 8,

	/// <summary>
	/// Whether this parameter is a mentionable.
	/// </summary>
	Mentionable = 9,

	/// <summary>
	/// Whether this parameter is a number.
	/// </summary>
	Number = 10,

	/// <summary>
	/// Whether this parameter is a attachment.
	/// </summary>
	Attachment = 11
}
