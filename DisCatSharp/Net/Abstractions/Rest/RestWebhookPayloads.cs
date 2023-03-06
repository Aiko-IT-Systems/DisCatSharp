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

using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a webhook payload.
/// </summary>
internal sealed class RestWebhookPayload
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the avatar base64.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
	public string AvatarBase64 { get; set; }

	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public ulong ChannelId { get; set; }

	/// <summary>
	/// Gets whether an avatar is set.
	/// </summary>
	[JsonProperty]
	public bool AvatarSet { get; set; }

	/// <summary>
	/// Gets whether the avatar should be serialized.
	/// </summary>
	public bool ShouldSerializeAvatarBase64()
		=> this.AvatarSet;
}

/// <summary>
/// Represents a webhook execute payload.
/// </summary>
internal sealed class RestWebhookExecutePayload
{
	/// <summary>
	/// Gets or sets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; set; }

	/// <summary>
	/// Gets or sets the username.
	/// </summary>
	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public string Username { get; set; }

	/// <summary>
	/// Gets or sets the avatar url.
	/// </summary>
	[JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
	public string AvatarUrl { get; set; }

	/// <summary>
	/// Whether this message is tts.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsTts { get; set; }

	/// <summary>
	/// Gets or sets the embeds.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordEmbed> Embeds { get; set; }

	/// <summary>
	/// Gets or sets the mentions.
	/// </summary>
	[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMentions Mentions { get; set; }

	/// <summary>
	/// Gets or sets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordActionRowComponent> Components { get; set; }

	/// <summary>
	/// Gets or sets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAttachment> Attachments { get; set; }

	///<summary>
	/// Gets or sets the thread name.
	/// </summary>
	[JsonProperty("thread_name", NullValueHandling = NullValueHandling.Ignore)]
	public string ThreadName { get; set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags Flags { get; set; }
}

/// <summary>
/// Represents a webhook message edit payload.
/// </summary>
internal sealed class RestWebhookMessageEditPayload
{
	/// <summary>
	/// Gets or sets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Content { get; set; }

	/// <summary>
	/// Gets or sets the embeds.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordEmbed> Embeds { get; set; }

	/// <summary>
	/// Gets or sets the mentions.
	/// </summary>
	[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<IMention> Mentions { get; set; }

	/// <summary>
	/// Gets or sets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordAttachment> Attachments { get; set; }

	/// <summary>
	/// Gets or sets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordActionRowComponent> Components { get; set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; set; }
}
