using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the activity instance.
/// </summary>
public sealed class DiscordActivityInstance : ObservableApiObject
{
	/// <summary>
	///     Constructs a new <see cref="DiscordActivityInstance" /> object.
	/// </summary>
	internal DiscordActivityInstance()
	{ }

	/// <summary>
	///     The instance ID of the Activity if one was launched or joined.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; internal set; }
}
