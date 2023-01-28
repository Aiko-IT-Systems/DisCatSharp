// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Globalization;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord thread metadata object.
/// </summary>
public class DiscordThreadChannelMetadata
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
		=> !string.IsNullOrWhiteSpace(this.ArchiveTimestampRaw) && DateTimeOffset.TryParse(this.ArchiveTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
			dto : null;

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
		=> !string.IsNullOrWhiteSpace(this.CreateTimestampRaw) && DateTimeOffset.TryParse(this.CreateTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
			dto : null;

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
