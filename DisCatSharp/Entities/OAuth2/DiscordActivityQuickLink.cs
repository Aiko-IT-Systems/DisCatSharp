using Newtonsoft.Json;

namespace DisCatSharp.Entities.OAuth2;

/// <summary>
///     Represents a generated activity quick link.
/// </summary>
public sealed class DiscordActivityQuickLink : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordActivityQuickLink" /> class.
	/// </summary>
	internal DiscordActivityQuickLink()
	{ }

	/// <summary>
	///     Gets the generated quick link id.
	/// </summary>
	[JsonProperty("link_id", NullValueHandling = NullValueHandling.Ignore)]
	public string? LinkId { get; internal set; }
}
