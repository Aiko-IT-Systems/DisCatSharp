using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// The type of Nitro subscription on a user's account.
/// </summary>
public enum PremiumType
{
	/// <summary>
	/// User does not have any perks.
	/// </summary>
	None = 0,

	/// <summary>
	/// Includes basic app perks like animated emojis and avatars.
	/// </summary>
	[DiscordDeprecated("Nitro Classic got replaced by Nitro Basic")]
	NitroClassic = 1,

	/// <summary>
	/// Includes all app perks.
	/// </summary>
	Nitro = 2,

	/// <summary>
	/// Includes basic app perks.
	/// </summary>
	NitroBasic = 3
}
