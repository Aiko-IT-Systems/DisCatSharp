using System;
using System.Globalization;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord thread metadata object.
/// </summary>
public class DiscordThreadChannelMetadata : ObservableApiObject
{
	/// <summary>
	/// Gets whether the thread is archived or not.
	/// </summary>
	[JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
	public bool Archived { get; internal set; }

	/// <summary>
	/// Gets ID of the archiver.
	/// </summary>
	[JsonProperty("archiver_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Archiver { get; internal set; }

	/// <summary>
	/// Gets the time when it will be archived, while there is no action inside the thread (In minutes).
	/// </summary>
	[JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public ThreadAutoArchiveDuration AutoArchiveDuration { get; internal set; }

	/// <summary>
	/// Gets the timestamp when it was archived.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? ArchiveTimestamp
		=> !string.IsNullOrWhiteSpace(this.ArchiveTimestampRaw) && DateTimeOffset.TryParse(this.ArchiveTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets the timestamp when it was archived as raw string.
	/// </summary>
	[JsonProperty("archive_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string ArchiveTimestampRaw { get; set; }

	/// <summary>
	/// Gets whether the thread is locked.
	/// </summary>
	[JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Locked { get; internal set; }

	/// <summary>
	/// Gets whether non-moderators can add other non-moderators to a thread; only available on private threads.
	/// </summary>
	[JsonProperty("invitable", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Invitable { get; internal set; }

	/// <summary>
	/// Gets the timestamp when the thread was created.
	/// Only populated for threads created after 2022-01-09.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? CreateTimestamp
		=> !string.IsNullOrWhiteSpace(this.CreateTimestampRaw) && DateTimeOffset.TryParse(this.CreateTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets the timestamp when the thread was created as raw string.
	/// Only populated for threads created after 2022-01-09.
	/// </summary>
	[JsonProperty("create_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string CreateTimestampRaw { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordThreadChannelMetadata"/> class.
	/// </summary>
	internal DiscordThreadChannelMetadata()
	{ }
}
