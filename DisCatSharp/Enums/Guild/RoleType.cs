using System.ComponentModel;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents the classification of a Discord role.
/// </summary>
public enum RoleType
{
	/// <summary>
	///     The role type is unknown.
	/// </summary>
	[Description("Normal role type.")]
	Normal,

	/// <summary>
	///     A bot or application role.
	/// </summary>
	[Description("Bot or application role.")]
	Bot,

	/// <summary>
	///     A booster role.
	/// </summary>
	[Description("Server booster role.")]
	Booster,

	/// <summary>
	///     A server product role.
	/// </summary>
	[Description("Server product role.")]
	ServerProduct,

	/// <summary>
	///     A premium subscriber role.
	/// </summary>
	[Description("Premium subscriber role.")]
	PremiumSubscriber,

	/// <summary>
	///     A premium subscriber tier role.
	/// </summary>
	[Description("Premium subscriber tier role.")]
	PremiumSubscriberTier,

	/// <summary>
	///     A role linked to an external platform (e.g., Twitch or YouTube).
	/// </summary>
	[Description("External platform role. For example Twitch or YouTube.")]
	ExternalPlatform,

	/// <summary>
	///     A role connection role.
	/// </summary>
	[Description("Role connection role.")]
	RoleConnection
}
