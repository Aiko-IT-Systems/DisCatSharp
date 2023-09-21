using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an scheduled event.
/// </summary>
public class DiscordScheduledEventEntityMetadata : ObservableApiObject
{
	/// <summary>
	/// External location if event type is <see cref="ScheduledEventEntityType.External"/>.
	/// </summary>
	[JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
	public string Location { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordScheduledEventEntityMetadata"/> class.
	/// </summary>
	internal DiscordScheduledEventEntityMetadata()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordScheduledEventEntityMetadata"/> class.
	/// </summary>
	/// <param name="location">The location.</param>
	public DiscordScheduledEventEntityMetadata(string location)
	{
		this.Location = location;
	}
}
