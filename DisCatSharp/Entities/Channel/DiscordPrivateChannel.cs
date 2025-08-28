using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a base class for all private (non-guild) channels.
/// </summary>
public abstract class DiscordPrivateChannel : BaseDiscordChannel
{
	/// <summary>
	/// Gets the channel name (DM: recipient username, Group DM: group name).
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the icon hash (Group DM only).
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string Icon { get; internal set; }

	/// <summary>
	/// Gets the list of recipients (users in the DM or group DM).
	/// </summary>
	[JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
	public ulong[] Recipients { get; internal set; }

	/// <summary>
	/// Gets the last message id in the channel.
	/// </summary>
	[JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? LastMessageId { get; internal set; }

	/// <summary>
	/// Gets the owner id (Group DM only).
	/// </summary>
	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OwnerId { get; internal set; }

	/// <summary>
	/// Gets the application id of the group DM creator (Group DM only).
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	/// Gets the managed status (Group DM only).
	/// </summary>
	[JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Managed { get; internal set; }
}
