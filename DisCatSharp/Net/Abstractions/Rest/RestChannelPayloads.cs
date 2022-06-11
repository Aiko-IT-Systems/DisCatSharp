// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
/// Represents a channel create payload.
/// </summary>
internal sealed class RestChannelCreatePayload
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the type.
	/// </summary>
	[JsonProperty("type")]
	public ChannelType Type { get; set; }

	/// <summary>
	/// Gets or sets the parent.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Parent { get; set; }

	/// <summary>
	/// Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic")]
	public Optional<string> Topic { get; set; }

	/// <summary>
	/// Gets or sets the bitrate.
	/// </summary>
	[JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
	public int? Bitrate { get; set; }

	/// <summary>
	/// Gets or sets the user limit.
	/// </summary>
	[JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
	public int? UserLimit { get; set; }

	/// <summary>
	/// Gets or sets the permission overwrites.
	/// </summary>
	[JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordRestOverwrite> PermissionOverwrites { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether nsfw.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Nsfw { get; set; }

	/// <summary>
	/// Gets or sets the per user rate limit.
	/// </summary>
	[JsonProperty("rate_limit_per_user")]
	public Optional<int?> PerUserRateLimit { get; set; }

	/// <summary>
	/// Gets or sets the quality mode.
	/// </summary>
	[JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
	public VideoQualityMode? QualityMode { get; set; }

	/// <summary>
	/// Gets or sets the default auto archive duration.
	/// </summary>
	[JsonProperty("default_auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration { get; set; }
}

/// <summary>
/// Represents a channel modify payload.
/// </summary>
internal sealed class RestChannelModifyPayload
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the type.
	/// </summary>
	[JsonProperty("type")]
	public Optional<ChannelType> Type { get; set; }

	/// <summary>
	/// Gets or sets the position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int? Position { get; set; }

	/// <summary>
	/// Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic")]
	public Optional<string> Topic { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether nsfw.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Nsfw { get; set; }

	/// <summary>
	/// Gets or sets the parent.
	/// </summary>
	[JsonProperty("parent_id")]
	public Optional<ulong?> Parent { get; set; }

	/// <summary>
	/// Gets or sets the bitrate.
	/// </summary>
	[JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
	public int? Bitrate { get; set; }

	/// <summary>
	/// Gets or sets the user limit.
	/// </summary>
	[JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
	public int? UserLimit { get; set; }

	/// <summary>
	/// Gets or sets the per user rate limit.
	/// </summary>
	[JsonProperty("rate_limit_per_user")]
	public Optional<int?> PerUserRateLimit { get; set; }

	/// <summary>
	/// Gets or sets the rtc region.
	/// </summary>
	[JsonProperty("rtc_region")]
	public Optional<string> RtcRegion { get; set; }

	/// <summary>
	/// Gets or sets the quality mode.
	/// </summary>
	[JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
	public VideoQualityMode? QualityMode { get; set; }

	/// <summary>
	/// Gets or sets the default auto archive duration.
	/// </summary>
	[JsonProperty("default_auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public ThreadAutoArchiveDuration? DefaultAutoArchiveDuration { get; set; }

	/// <summary>
	/// Gets or sets the permission overwrites.
	/// </summary>
	[JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordRestOverwrite> PermissionOverwrites { get; set; }

	/// <summary>
	/// Gets or sets the banner base64.
	/// </summary>
	[JsonProperty("banner")]
	public Optional<string> BannerBase64 { get; set; }
}

/// <summary>
/// Represents a channel message edit payload.
/// </summary>
internal class RestChannelMessageEditPayload
{
	/// <summary>
	/// Gets or sets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Include)]
	public string Content { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether has content.
	/// </summary>
	[JsonIgnore]
	public bool HasContent { get; set; }

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
	/// Gets or sets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordAttachment> Attachments { get; set; }

	/// <summary>
	/// Gets or sets the flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; set; }

	/// <summary>
	/// Gets or sets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyCollection<DiscordActionRowComponent> Components { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether has embed.
	/// </summary>
	[JsonIgnore]
	public bool HasEmbed { get; set; }

	/// <summary>
	/// Should serialize the content.
	/// </summary>
	public bool ShouldSerializeContent()
		=> this.HasContent;

	/// <summary>
	/// Should serialize the embed.
	/// </summary>
	public bool ShouldSerializeEmbed()
		=> this.HasEmbed;
}

/// <summary>
/// Represents a channel message create payload.
/// </summary>
internal sealed class RestChannelMessageCreatePayload : RestChannelMessageEditPayload
{
	/// <summary>
	/// Gets or sets a value indicating whether t t is s.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsTts { get; set; }

	/// <summary>
	/// Gets or sets the stickers ids.
	/// </summary>
	[JsonProperty("sticker_ids", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<ulong> StickersIds { get; set; }

	/// <summary>
	/// Gets or sets the message reference.
	/// </summary>
	[JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
	public InternalDiscordMessageReference? MessageReference { get; set; }

}

/// <summary>
/// Represents a channel message create multipart payload.
/// </summary>
internal sealed class RestChannelMessageCreateMultipartPayload
{
	/// <summary>
	/// Gets or sets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether t t is s.
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
	/// Gets or sets the message reference.
	/// </summary>
	[JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
	public InternalDiscordMessageReference? MessageReference { get; set; }
}

/// <summary>
/// Represents a channel message bulk delete payload.
/// </summary>
internal sealed class RestChannelMessageBulkDeletePayload
{
	/// <summary>
	/// Gets or sets the messages.
	/// </summary>
	[JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<ulong> Messages { get; set; }
}

/// <summary>
/// Represents a channel invite create payload.
/// </summary>
internal sealed class RestChannelInviteCreatePayload
{
	/// <summary>
	/// Gets or sets the max age.
	/// </summary>
	[JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxAge { get; set; }

	/// <summary>
	/// Gets or sets the max uses.
	/// </summary>
	[JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxUses { get; set; }

	/// <summary>
	/// Gets or sets the target type.
	/// </summary>
	[JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
	public TargetType? TargetType { get; set; }

	/// <summary>
	/// Gets or sets the target application.
	/// </summary>
	[JsonProperty("target_application_id", NullValueHandling = NullValueHandling.Ignore)]
	public TargetActivity? TargetApplication { get; set; }

	/// <summary>
	/// Gets or sets the target user id.
	/// </summary>
	[JsonProperty("target_user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? TargetUserId { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether temporary.
	/// </summary>
	[JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
	public bool Temporary { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether unique.
	/// </summary>
	[JsonProperty("unique", NullValueHandling = NullValueHandling.Ignore)]
	public bool Unique { get; set; }
}

/// <summary>
/// Represents a channel permission edit payload.
/// </summary>
internal sealed class RestChannelPermissionEditPayload
{
	/// <summary>
	/// Gets or sets the allow.
	/// </summary>
	[JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions Allow { get; set; }

	/// <summary>
	/// Gets or sets the deny.
	/// </summary>
	[JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions Deny { get; set; }

	/// <summary>
	/// Gets or sets the type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; set; }
}

/// <summary>
/// Represents a channel group dm recipient add payload.
/// </summary>
internal sealed class RestChannelGroupDmRecipientAddPayload : IOAuth2Payload
{
	/// <summary>
	/// Gets or sets the access token.
	/// </summary>
	[JsonProperty("access_token")]
	public string AccessToken { get; set; }

	/// <summary>
	/// Gets or sets the nickname.
	/// </summary>
	[JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
	public string Nickname { get; set; }
}

/// <summary>
/// The acknowledge payload.
/// </summary>
internal sealed class AcknowledgePayload
{
	/// <summary>
	/// Gets or sets the token.
	/// </summary>
	[JsonProperty("token", NullValueHandling = NullValueHandling.Include)]
	public string Token { get; set; }
}

/// <summary>
/// Represents a thread channel create payload.
/// </summary>
internal sealed class RestThreadChannelCreatePayload
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the auto archive duration.
	/// </summary>
	[JsonProperty("auto_archive_duration")]
	public ThreadAutoArchiveDuration AutoArchiveDuration { get; set; }

	/// <summary>
	/// Gets or sets the rate limit per user.
	/// </summary>
	[JsonProperty("rate_limit_per_user")]
	public int? PerUserRateLimit { get; set; }

	/// <summary>
	/// Gets or sets the thread type.
	/// </summary>
	[JsonProperty("type")]
	public ChannelType Type { get; set; }
}

/// <summary>
/// Represents a thread channel modify payload.
/// </summary>
internal sealed class RestThreadChannelModifyPayload
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the archived.
	/// </summary>
	[JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool?> Archived { get; set; }

	/// <summary>
	/// Gets or sets the auto archive duration.
	/// </summary>
	[JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ThreadAutoArchiveDuration?> AutoArchiveDuration { get; set; }

	/// <summary>
	/// Gets or sets the locked.
	/// </summary>
	[JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool?> Locked { get; set; }

	/// <summary>
	/// Gets or sets the per user rate limit.
	/// </summary>
	[JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int?> PerUserRateLimit { get; set; }

	/// <summary>
	/// Gets or sets the thread's invitable state.
	/// </summary>
	[JsonProperty("invitable", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool?> Invitable { internal get; set; }
}
