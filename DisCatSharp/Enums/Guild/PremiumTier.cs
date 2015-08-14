namespace DisCatSharp.Enums;

/// <summary>
/// Represents a server's premium tier.
/// </summary>
public enum PremiumTier
{
	/// <summary>
	/// Indicates that this server was not boosted.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates that this server was boosted two times.
	/// </summary>
	TierOne = 1,

	/// <summary>
	/// Indicates that this server was boosted seven times.
	/// </summary>
	TierTwo = 2,

	/// <summary>
	/// Indicates that this server was boosted fourteen times.
	/// </summary>
	TierThree = 3,

	/// <summary>
	/// Indicates an unknown premium tier.
	/// </summary>
	Unknown = int.MaxValue
}
