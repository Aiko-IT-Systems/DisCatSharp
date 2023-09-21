using DisCatSharp.Entities;
using DisCatSharp.Lavalink.Entities.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Payloads;

/// <summary>
/// The lavalink rest player update payload.
/// </summary>
internal sealed class LavalinkRestPlayerUpdatePayload
{
	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guildId")]
	internal string GuildId { get; set; }

	/// <summary>
	/// Gets or sets the encoded track.
	/// </summary>
	[JsonProperty("encodedTrack")]
	internal Optional<string?> EncodedTrack { get; set; }

	/// <summary>
	/// Gets or sets the identifier.
	/// </summary>
	[JsonProperty("identifier")]
	internal Optional<string> Identifier { get; set; }

	/// <summary>
	/// Gets or sets the start or seek position.
	/// </summary>
	[JsonProperty("position")]
	internal Optional<int> Position { get; set; }

	/// <summary>
	/// Gets or sets the end time.
	/// </summary>
	[JsonProperty("endTime")]
	internal Optional<int?> EndTime { get; set; }

	/// <summary>
	/// Gets or sets the volume.
	/// </summary>
	[JsonProperty("volume")]
	internal Optional<int> Volume { get; set; }

	/// <summary>
	/// Gets or sets whether the player is paused.
	/// </summary>
	[JsonProperty("paused")]
	internal Optional<bool> Paused { get; set; }

	/// <summary>
	/// Gets or sets the filters.
	/// </summary>
	[JsonProperty("filters")]
	internal Optional<LavalinkFilters> Filters { get; set; }

	/// <summary>
	/// Constructs a new <see cref="LavalinkRestPlayerUpdatePayload"/>.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal LavalinkRestPlayerUpdatePayload(string guildId)
	{
		this.GuildId = guildId;
	}
}
