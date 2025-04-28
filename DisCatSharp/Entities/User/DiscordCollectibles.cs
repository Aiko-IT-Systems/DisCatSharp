namespace DisCatSharp.Entities;

/// <summary>
///     Represents a user's collectibles.
/// </summary>
public sealed class DiscordCollectibles
{
	/// <summary>
	///     Gets the user's nameplate, if applicable.
	/// </summary>
	public DiscordNameplate? Nameplate { get; internal set; }
}
