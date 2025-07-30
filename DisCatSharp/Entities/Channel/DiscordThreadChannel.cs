using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a base class for all thread channels (public, private, news threads).
/// </summary>
public abstract class DiscordThreadChannel : DiscordGuildTextChannel
{
	/// <summary>
	/// Gets the thread owner id.
	/// </summary>
	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OwnerId { get; internal set; }

	/// <summary>
	/// Gets the thread message count.
	/// </summary>
	[JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? MessageCount { get; internal set; }

	/// <summary>
	/// Gets the thread member count.
	/// </summary>
	[JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? MemberCount { get; internal set; }

	/// <summary>
	/// Gets the thread archive status.
	/// </summary>
	[JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Archived { get; internal set; }

	/// <summary>
	/// Gets the thread auto archive duration (in minutes).
	/// </summary>
	[JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public int? AutoArchiveDuration { get; internal set; }

	/// <summary>
	/// Gets the thread archive timestamp.
	/// </summary>
	[JsonProperty("archive_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public string ArchiveTimestamp { get; internal set; }

	/// <summary>
	/// Gets the thread locked status.
	/// </summary>
	[JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Locked { get; internal set; }

	/// <summary>
	/// Gets the thread invitable status (private threads only).
	/// </summary>
	[JsonProperty("invitable", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Invitable { get; internal set; }

	/// <summary>
	/// Gets the thread create timestamp.
	/// </summary>
	[JsonProperty("create_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public string CreateTimestamp { get; internal set; }
}
