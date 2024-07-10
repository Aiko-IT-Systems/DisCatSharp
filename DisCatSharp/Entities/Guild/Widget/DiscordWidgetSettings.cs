using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord guild's widget settings.
/// </summary>
public class DiscordWidgetSettings : ObservableApiObject
{
	/// <summary>
	/// Gets the guild.
	/// </summary>
	[JsonIgnore]
	internal DiscordGuild Guild { get; set; }

	/// <summary>
	/// Gets the guild's widget channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the guild's widget channel.
	/// </summary>
	[JsonIgnore]
	public DiscordChannel Channel
		=> this.Guild?.GetChannel(this.ChannelId);

	/// <summary>
	/// Whether if the guild's widget is enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsEnabled { get; internal set; }
}
