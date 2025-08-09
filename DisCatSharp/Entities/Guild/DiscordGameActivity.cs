using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///    Represents a Discord game activity.
/// </summary>
public sealed class DiscordGameActivity
{
	/// <summary>
	///    Gets the activity level.
	/// </summary>
	[JsonProperty("activity_level", NullValueHandling = NullValueHandling.Ignore)]
	public int ActivityLevel { get; set; }

	/// <summary>
	///    Gets the activity score.
	/// </summary>
	[JsonProperty("activity_score", NullValueHandling = NullValueHandling.Ignore)]
	public int ActivityScore { get; set; }
}
