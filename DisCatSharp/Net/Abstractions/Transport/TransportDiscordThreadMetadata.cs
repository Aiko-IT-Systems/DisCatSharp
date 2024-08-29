using System;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordThreadMetadata
{
	[JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
	public bool Archived { get; internal set; }

	[JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public int AutoArchiveDuration { get; internal set; }

	[JsonProperty("archive_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ArchiveTimestamp { get; internal set; }

	[JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
	public bool Locked { get; internal set; }

	[JsonProperty("invitable", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Invitable { get; internal set; }

	[JsonProperty("create_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? CreateTimestamp { get; internal set; }
}
