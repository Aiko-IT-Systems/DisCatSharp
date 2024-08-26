namespace DisCatSharp.Experimental.Enums;

/// <summary>
///     Represents how a user joined the guild.
/// </summary>
public enum JoinSourceType
{
	/// <summary>
	///     The user joined the guild through an unknown source.
	/// </summary>
	Unspecified = 0,

	/// <summary>
	///     The user was added to the guild by a bot using the guilds.join OAuth2 scope.
	/// </summary>
	Bot = 1,

	/// <summary>
	///     The user was added to the guild by an integration.
	/// </summary>
	Integration = 2,

	/// <summary>
	///     The user joined the guild through guild discovery.
	/// </summary>
	Discovery = 3,

	/// <summary>
	///     The user joined the guild through a student hub.
	/// </summary>
	Hub = 4,

	/// <summary>
	///     The user joined the guild through an invite.
	/// </summary>
	Invite = 5,

	/// <summary>
	///     The user joined the guild through a vanity URL.
	/// </summary>
	VanityUrl = 6,

	/// <summary>
	///     The user was accepted into the guild after applying for membership.
	/// </summary>
	ManualMemberVerification = 7
}
