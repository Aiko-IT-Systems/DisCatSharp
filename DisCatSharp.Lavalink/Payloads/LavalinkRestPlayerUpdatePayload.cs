using DisCatSharp.Entities;
using DisCatSharp.Lavalink.Entities.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Payloads;

/// <summary>
///     The lavalink rest player update payload.
/// </summary>
internal sealed class LavalinkRestPlayerUpdatePayload
{
	/// <summary>
	///     Constructs a new <see cref="LavalinkRestPlayerUpdatePayload" />.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal LavalinkRestPlayerUpdatePayload(string guildId)
	{
		this.GuildId = guildId;
	}

	/// <summary>
	///     Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guildId", NullValueHandling = NullValueHandling.Ignore)]
	internal string GuildId { get; set; }

	/// <summary>
	///     Gets or sets the start or seek position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	internal Optional<int> Position { get; set; }

	/// <summary>
	///     Gets or sets the end time.
	/// </summary>
	[JsonProperty("endTime", NullValueHandling = NullValueHandling.Include)]
	internal Optional<int?> EndTime { get; set; }

	/// <summary>
	///     Gets or sets the volume.
	/// </summary>
	[JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
	internal Optional<int> Volume { get; set; }

	/// <summary>
	///     Gets or sets whether the player is paused.
	/// </summary>
	[JsonProperty("paused", NullValueHandling = NullValueHandling.Ignore)]
	internal Optional<bool> Paused { get; set; }

	/// <summary>
	///     Gets or sets the filters.
	/// </summary>
	[JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore)]
	internal Optional<LavalinkFilters> Filters { get; set; }

	/// <summary>
	///    Gets or sets the track.
	/// </summary>
	[JsonProperty("track", NullValueHandling = NullValueHandling.Ignore)]
	internal Optional<PlayerBase> Track { get; set; }
}

/// <summary>
///   The lavalink rest player update player track payload.
/// </summary>
internal class PlayerBase
{
	/// <summary>
	/// Gets or sets the user data.
	/// </summary>
	[JsonProperty("userData", NullValueHandling = NullValueHandling.Ignore)]
	public object? UserData { get; set; }
}

/// <summary>
///    The lavalink rest player update player track payload.
/// </summary>
internal sealed class PlayerWithIdentifier : PlayerBase
{
	/// <summary>
	///     Gets or sets the identifier.
	/// </summary>
	[JsonProperty("identifier", NullValueHandling = NullValueHandling.Ignore)]
	internal required string Identifier { get; set; }
}

/// <summary>
///    The lavalink rest player update player track payload.
/// </summary>
internal sealed class PlayerWithEncoded : PlayerBase
{
	/// <summary>
	///     Gets or sets the encoded track.
	/// </summary>
	[JsonProperty("encoded", NullValueHandling = NullValueHandling.Include)]
	internal string? Encoded { get; set; }
}
