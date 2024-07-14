using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents data from the original message.
/// </summary>
public class DiscordMessageReference
{
	/// <summary>
	/// Gets the type of the reference.
	/// </summary>
	public ReferenceType Type { get; internal set; }

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
	public DiscordGuild? Guild { get; internal set; }

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	public ulong? GuildId { get; internal set; }

	/// <summary>
	/// Gets a readable message reference string.
	/// </summary>
	public override string ToString()
		=> $"Type: {this.Type}, Guild: {(this.GuildId.HasValue ? this.GuildId.Value : "Not from guild")}, Channel: {this.Channel.Id}, Message: {this.Message.Id}";

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMessageReference"/> class.
	/// </summary>
	internal DiscordMessageReference()
	{ }
}

/// <summary>
/// Represents raw data from the original message.
/// </summary>
internal struct InternalDiscordMessageReference
{
	public InternalDiscordMessageReference()
	{
		this.Type = ReferenceType.Default;
		this.MessageId = null;
		this.ChannelId = null;
		this.GuildId = null;
		this.FailIfNotExists = false;
	}

	/// <summary>
	/// Gets the type of the reference.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	internal ReferenceType Type { get; set; }

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
