using DisCatSharp.Lavalink.Entities.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink player.
/// </summary>
public sealed class LavalinkPlayer
{
	/// <summary>
	/// Gets the guild id this player belongs to.
	/// </summary>
	[JsonProperty("guildId")]
	public string GuildId { get; internal set; }

	/// <summary>
	/// Gets the currently loaded track.
	/// </summary>
	[JsonProperty("track", NullValueHandling = NullValueHandling.Include)]
	public LavalinkTrack? Track { get; internal set; }

	/// <summary>
	/// Gets the volume of this player.
	/// </summary>
	[JsonProperty("volume")]
	public int Volume { get; internal set; }

	/// <summary>
	/// Gets whether this player is paused.
	/// </summary>
	[JsonProperty("paused")]
	public bool Paused { get; internal set; }

	/// <summary>
	/// Gets the player state.
	/// </summary>
	[JsonProperty("state")]
	public LavalinkPlayerState PlayerState { get; internal set; }

	/// <summary>
	/// Gets the player voice state.
	/// </summary>
	[JsonProperty("voice")]
	public LavalinkVoiceState VoiceState { get; set; }

	/// <summary>
	/// Gets the player filters.
	/// </summary>
	[JsonProperty("filters")]
	public LavalinkFilters Filters { get; internal set; }
}
