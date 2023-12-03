using DisCatSharp.Attributes;
using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a clyde settings object in guilds.
/// </summary>
[DiscordDeprecated]
public sealed class ClydeSettings : ObservableApiObject
{
	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the personality.
	/// </summary>
	[JsonProperty("personality", NullValueHandling = NullValueHandling.Ignore)]
	public string Personality { get; internal set; }

	/// <summary>
	/// Gets the id of the last user who edited the settings, if applicable.
	/// </summary>
	[JsonProperty("last_edited_by", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? LastEditedById { get; internal set; } = null;

	/// <summary>
	/// Gets the clyde profile id.
	/// Use <see cref="DisCatSharpExtensions.GetClydeProfileAsync"/> to fetch details about this profile.
	/// </summary>
	[JsonProperty("clyde_profile_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ClydeProfileId { get; internal set; }
}
