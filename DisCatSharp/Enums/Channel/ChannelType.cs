namespace DisCatSharp.Enums;

/// <summary>
/// Represents a channel's type.
/// </summary>
public enum ChannelType
{
	/// <summary>
	/// Indicates that this is a text channel.
	/// </summary>
	Text = 0,

	/// <summary>
	/// Indicates that this is a private channel.
	/// </summary>
	Private = 1,

	/// <summary>
	/// Indicates that this is a voice channel.
	/// </summary>
	Voice = 2,

	/// <summary>
	/// Indicates that this is a group direct message channel.
	/// </summary>
	Group = 3,

	/// <summary>
	/// Indicates that this is a channel category.
	/// </summary>
	Category = 4,

	/// <summary>
	/// Indicates that this is a news channel.
	/// </summary>
	News = 5,

	/// <summary>
	/// Indicates that this is a store channel.
	/// </summary>
	Store = 6,

	/// <summary>
	/// Indicates that this is a temporary sub-channel within a news channel.
	/// </summary>
	NewsThread = 10,

	/// <summary>
	/// Indicates that this is a temporary sub-channel within a text channel.
	/// </summary>
	PublicThread = 11,

	/// <summary>
	/// Indicates that this is a temporary sub-channel within a text channel that is only viewable
	/// by those invited and those with the MANAGE_THREADS permission.
	/// </summary>
	PrivateThread = 12,

	/// <summary>
	/// Indicates that this is a stage channel.
	/// </summary>
	Stage = 13,

	/// <summary>
	/// Indicates that this is a guild directory channel.
	/// This is used for hub guilds (feature for schools).
	/// </summary>
	GuildDirectory = 14,

	/// <summary>
	/// Indicates that this is a guild forum channel (Threads only channel).
	/// </summary>
	Forum = 15,

	/// <summary>
	/// Indicates that this is a media channel.
	/// </summary>
	GuildMedia = 16,

	/// <summary>
	/// Indicates unknown channel type.
	/// </summary>
	Unknown = int.MaxValue
}
