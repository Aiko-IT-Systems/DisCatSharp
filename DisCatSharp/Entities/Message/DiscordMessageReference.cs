using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents data from the original message.
/// </summary>
public class DiscordMessageReference
{
	/// <summary>
	/// Gets the original message.
	/// </summary>
	public DiscordMessage Message { get; internal set; }

	/// <summary>
	/// Gets the channel of the original message.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the guild of the original message.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets a readable message reference string.
	/// </summary>
	public override string ToString()
		=> $"Guild: {this.Guild.Id}, Channel: {this.Channel.Id}, Message: {this.Message.Id}";

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMessageReference"/> class.
	/// </summary>
	internal DiscordMessageReference()
	{ }
}

internal struct InternalDiscordMessageReference
{
	/// <summary>
	/// Gets the message id.
	/// </summary>
	[JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? MessageId { get; set; }

	/// <summary>
	/// Gets the channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? ChannelId { get; set; }

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? GuildId { get; set; }

	/// <summary>
	/// Whether it should fail if it does not exists.
	/// </summary>
	[JsonProperty("fail_if_not_exists", NullValueHandling = NullValueHandling.Ignore)]
	public bool FailIfNotExists { get; set; }
}
