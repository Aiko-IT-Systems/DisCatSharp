using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents a payload for requesting soundboard sounds for specific guilds.
/// </summary>
internal sealed class RequestSoundboardSoundsPayload
{
	/// <summary>
	///     Gets or sets the list of guild IDs for which to request soundboard sounds.
	/// </summary>
	[JsonProperty("guild_ids")]
	internal IEnumerable<ulong> GuildIds { get; set; }
}

/// <summary>
///     Represents the payload for the soundboard sounds event response.
/// </summary>
internal sealed class SoundboardSoundsEventPayload
{
	/// <summary>
	///     Gets or sets the guild ID associated with the soundboard sounds.
	/// </summary>
	[JsonProperty("guild_id")]
	internal ulong GuildId { get; set; }

	/// <summary>
	///     Gets or sets the list of soundboard sounds in the guild.
	/// </summary>
	[JsonProperty("soundboard_sounds")]
	internal List<DiscordSoundboardSound> SoundboardSounds { get; set; }
}
