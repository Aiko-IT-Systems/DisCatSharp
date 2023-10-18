using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the channel to which an invite is linked.
/// </summary>
public class DiscordInviteChannel : SnowflakeObject
{
	/// <summary>
	/// Gets the name of the channel.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the type of the channel.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ChannelType Type { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordInviteChannel"/> class.
	/// </summary>
	internal DiscordInviteChannel()
	{ }
}
