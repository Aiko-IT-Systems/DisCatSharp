using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Payloads;

/// <summary>
/// The lavalink rest player create payload.
/// </summary>
internal sealed class LavalinkRestPlayerCreatePayload
{
	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guildId")]
	internal string GuildId { get; set; }

	/// <summary>
	/// Gets or sets the volume.
	/// </summary>
	[JsonProperty("volume")]
	internal int Volume { get; set; }

	/// <summary>
	/// Constructs a new <see cref="LavalinkRestPlayerCreatePayload"/>.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="volume">The volume.</param>
	internal LavalinkRestPlayerCreatePayload(string guildId, int volume)
	{
		this.GuildId = guildId;
		this.Volume = volume;
	}
}
