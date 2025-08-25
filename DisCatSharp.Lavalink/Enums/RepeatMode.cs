namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Represents the repeat mode for the Lavalink player.
/// </summary>
public enum RepeatMode
{
	/// <summary>
	/// No repeat mode. The queue operates normally and does not repeat.
	/// </summary>
	None,

	/// <summary>
	/// Repeat all mode. The entire queue repeats once it ends.
	/// </summary>
	All,

	/// <summary>
	/// Repeat current mode. The current track is played over and over again.
	/// </summary>
	Current
}
