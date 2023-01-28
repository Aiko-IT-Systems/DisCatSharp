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

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a stage instance create payload.
/// </summary>
internal sealed class RestStageInstanceCreatePayload
{
	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string Topic { get; set; }

	/// <summary>
	/// Gets or sets the associated scheduled event id.
	/// </summary>
	[JsonProperty("guild_scheduled_event_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ScheduledEventId { get; set; }

	/// <summary>
	/// Whether everyone should be notified about the start.
	/// </summary>
	[JsonProperty("send_start_notification", NullValueHandling = NullValueHandling.Ignore)]
	public bool SendStartNotification { get; set; }
}

/// <summary>
/// Represents a stage instance modify payload.
/// </summary>
internal sealed class RestStageInstanceModifyPayload
{
	/// <summary>
	/// Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Topic { get; set; }
}
