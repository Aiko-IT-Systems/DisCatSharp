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

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// The rest guild scheduled event create payload.
/// </summary>
internal class RestGuildScheduledEventCreatePayload
{
	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the entity metadata.
	/// </summary>
	[JsonProperty("entity_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordScheduledEventEntityMetadata EntityMetadata { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the privacy level of the scheduled event.
	/// </summary>
	[JsonProperty("privacy_level")]
	public ScheduledEventPrivacyLevel PrivacyLevel { get; set; }

	/// <summary>
	/// Gets or sets the time to schedule the scheduled event.
	/// </summary>
	[JsonProperty("scheduled_start_time")]
	public DateTimeOffset ScheduledStartTime { get; internal set; }

	/// <summary>
	/// Gets or sets the time when the scheduled event is scheduled to end.
	/// </summary>
	[JsonProperty("scheduled_end_time", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? ScheduledEndTime { get; internal set; }

	/// <summary>
	/// Gets or sets the entity type of the scheduled event.
	/// </summary>
	[JsonProperty("entity_type")]
	public ScheduledEventEntityType EntityType { get; set; }

	/// <summary>
	/// Gets or sets the image as base64.
	/// </summary>
	[JsonProperty("image", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> CoverBase64 { get; set; }
}

/// <summary>
/// The rest guild scheduled event modify payload.
/// </summary>
internal class RestGuildScheduledEventModifyPayload
{
	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public Optional<ulong?> ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the entity metadata.
	/// </summary>
	[JsonProperty("entity_metadata")]
	public Optional<DiscordScheduledEventEntityMetadata> EntityMetadata { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public Optional<string> Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[JsonProperty("description")]
	public Optional<string> Description { get; set; }

	/// <summary>
	/// Gets or sets the time to schedule the scheduled event.
	/// </summary>
	[JsonProperty("scheduled_start_time")]
	public Optional<DateTimeOffset> ScheduledStartTime { get; internal set; }

	/// <summary>
	/// Gets or sets the time when the scheduled event is scheduled to end.
	/// </summary>
	[JsonProperty("scheduled_end_time")]
	public Optional<DateTimeOffset> ScheduledEndTime { get; internal set; }

	/// <summary>
	/// Gets or sets the entity type of the scheduled event.
	/// </summary>
	[JsonProperty("entity_type")]
	public Optional<ScheduledEventEntityType> EntityType { get; set; }

	/// <summary>
	/// Gets or sets the cover image as base64.
	/// </summary>
	[JsonProperty("image")]
	public Optional<string> CoverBase64 { get; set; }

	/// <summary>
	/// Gets or sets the status of the scheduled event.
	/// </summary>
	[JsonProperty("status")]
	public Optional<ScheduledEventStatus> Status { get; set; }
}
