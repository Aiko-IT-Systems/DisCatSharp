using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents a channel create payload.
/// </summary>
internal sealed class RestChannelCreatePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	///     Gets or sets the type.
	/// </summary>
	[JsonProperty("type")]
	public ChannelType Type { get; set; }

	/// <summary>
	///     Gets or sets the parent.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Parent { get; set; }

	/// <summary>
	///     Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic")]
	public Optional<string> Topic { get; set; }

	/// <summary>
	///     Gets or sets the template.
	/// </summary>
	[JsonProperty("template")]
	public Optional<string> Template { get; set; }

	/// <summary>
	///     Gets or sets the bitrate.
	/// </summary>
	[JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
	public int? Bitrate { get; set; }

	/// <summary>
	///     Gets or sets the user limit.
	/// </summary>
	[JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
	public int? UserLimit { get; set; }

	/// <summary>
	///     Gets or sets the permission overwrites.
	/// </summary>
	[JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordRestOverwrite> PermissionOverwrites { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether nsfw.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Nsfw { get; set; }

	/// <summary>
	///     Gets or sets the per user rate limit.
	/// </summary>
	[JsonProperty("rate_limit_per_user")]
	public Optional<int?> PerUserRateLimit { get; set; }

	[JsonProperty("default_thread_rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int?> PostCreateUserRateLimit { get; internal set; }

	/// <summary>
	///     Gets or sets the quality mode.
	/// </summary>
	[JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
	public VideoQualityMode? QualityMode { get; set; }

	/// <summary>
	///     Gets or sets the default auto archive duration.
	/// </summary>
	[JsonProperty("default_auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ThreadAutoArchiveDuration?> DefaultAutoArchiveDuration { get; set; }

	/// <summary>
	///     Gets the default reaction emoji for forum posts.
	/// </summary>
	[JsonProperty("default_reaction_emoji", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ForumReactionEmoji> DefaultReactionEmoji { get; internal set; }

	/// <summary>
	///     Gets the default forum post sort order
	/// </summary>
	[JsonProperty("default_sort_order", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ForumPostSortOrder> DefaultSortOrder { get; internal set; }

	/// <summary>
	///     Gets the default forum layout for this channel
	/// </summary>
	[JsonProperty("default_forum_layout", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ForumLayout?> DefaultForumLayout { get; internal set; }

	/// <summary>
	///     Gets or sets the channel flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Include)]
	public Optional<ChannelFlags?> Flags { internal get; set; }
}

/// <summary>
///     Represents a channel modify payload.
/// </summary>
internal sealed class RestChannelModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// <summary>
	///     Gets or sets the type.
	/// </summary>
	[JsonProperty("type")]
	public Optional<ChannelType> Type { get; set; }

	/// <summary>
	///     Gets or sets the position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int? Position { get; set; }

	/// <summary>
	///     Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic")]
	public Optional<string> Topic { get; set; }

	/// <summary>
	///     Gets or sets the template.
	/// </summary>
	[JsonProperty("template")]
	public Optional<string> Template { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether nsfw.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Nsfw { get; set; }

	/// <summary>
	///     Gets or sets the parent.
	/// </summary>
	[JsonProperty("parent_id")]
	public Optional<ulong?> Parent { get; set; }

	/// <summary>
	///     Gets or sets the bitrate.
	/// </summary>
	[JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int?> Bitrate { get; set; }

	/// <summary>
	///     Gets or sets the user limit.
	/// </summary>
	[JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int?> UserLimit { get; set; }

	/// <summary>
	///     Gets or sets the per user rate limit.
	/// </summary>
	[JsonProperty("rate_limit_per_user")]
	public Optional<int?> PerUserRateLimit { get; set; }

	[JsonProperty("default_thread_rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int?> PostCreateUserRateLimit { get; internal set; }

	/// <summary>
	///     Gets or sets the rtc region.
	/// </summary>
	[JsonProperty("rtc_region")]
	public Optional<string> RtcRegion { get; set; }

	/// <summary>
	///     Gets or sets the quality mode.
	/// </summary>
	[JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
	public VideoQualityMode? QualityMode { get; set; }

	/// <summary>
	///     Gets or sets the default auto archive duration.
	/// </summary>
	[JsonProperty("default_auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ThreadAutoArchiveDuration?> DefaultAutoArchiveDuration { get; set; }

	/// <summary>
	///     Gets or sets the permission overwrites.
	/// </summary>
	[JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordRestOverwrite> PermissionOverwrites { get; set; }

	/// <summary>
	///     Gets the default reaction emoji for forum posts.
	/// </summary>
	[JsonProperty("default_reaction_emoji", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ForumReactionEmoji> DefaultReactionEmoji { get; internal set; }

	/// <summary>
	///     Gets the default forum post sort order
	/// </summary>
	[JsonProperty("default_sort_order", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ForumPostSortOrder?> DefaultSortOrder { get; internal set; }

	/// <summary>
	///     Gets the default tag matching setting for this forum channel.
	/// </summary>
	[JsonProperty("default_tag_setting", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<TagMatching?> DefaultTagSetting { get; internal set; }

	/// <summary>
	///     Gets the default forum layout for this channel
	/// </summary>
	[JsonProperty("default_forum_layout", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ForumLayout?> ForumLayout { get; internal set; }

	/// <summary>
	///     Gets or sets the channel flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Include)]
	public Optional<ChannelFlags?> Flags { internal get; set; }

	/// <summary>
	///     Gets or sets the available tags.
	/// </summary>
	[JsonProperty("available_tags", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<List<ForumPostTag>?> AvailableTags { internal get; set; }
}

/// <summary>
///     Represents a channel message edit payload.
/// </summary>
internal class RestChannelMessageEditPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Include)]
	public string Content { get; set; }

	/// <summary>
	///     Tracks if content was explicitly set (even to null).
	///     Used to control conditional serialization of the content field.
	/// </summary>
	[JsonIgnore]
	public bool HasContent { get; set; }

	/// <summary>
	///     Tracks if embeds were explicitly set (including empty).
	///     Used to control conditional serialization of the embeds field.
	/// </summary>
	[JsonIgnore]
	public bool HasEmbeds { get; set; }

	/// <summary>
	///     Tracks if components were explicitly set (including empty).
	///     Used to control conditional serialization of the components field.
	/// </summary>
	[JsonIgnore]
	public bool HasComponents { get; set; }

	/// <summary>
	///     Gets or sets the embeds.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Include)]
	public IEnumerable<DiscordEmbed>? Embeds { get; set; }

	/// <summary>
	///     Gets or sets the mentions.
	/// </summary>
	[JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMentions Mentions { get; set; }

	/// <summary>
	///     Gets or sets the attachments.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordAttachment>? Attachments { get; set; }

	/// <summary>
	///     Gets or sets the flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; set; }

	/// <summary>
	///     Gets or sets the components.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Include)]
	public IEnumerable<DiscordComponent>? Components { get; set; }

	/// <summary>
	///     Determines whether the content field should be serialized.
	/// </summary>
	public bool ShouldSerializeContent() => this.HasContent;

	/// <summary>
	///     Determines whether the embeds field should be serialized.
	/// </summary>
	public bool ShouldSerializeEmbeds() => this.HasEmbeds;

	/// <summary>
	///     Determines whether the components field should be serialized.
	/// </summary>
	public bool ShouldSerializeComponents() => this.HasComponents;
}

/// <summary>
///     Represents a channel message create payload.
/// </summary>
internal sealed class RestChannelMessageCreatePayload : RestChannelMessageEditPayload
{
	/// <summary>
	///     Gets or sets a value indicating whether t t is s.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsTts { get; set; }

	/// <summary>
	///     Gets or sets the stickers ids.
	/// </summary>
	[JsonProperty("sticker_ids", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<ulong>? StickersIds { get; set; }

	/// <summary>
	///     Gets or sets the message reference.
	/// </summary>
	[JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
	public InternalDiscordMessageReference? MessageReference { get; set; }

	/// <summary>
	///     Gets or sets the nonce sent with the message.
	/// </summary>
	[JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
	public string Nonce { get; internal set; }

	/// <summary>
	///     Gets or sets whether to enforce the <see cref="Nonce" /> to be validated.
	/// </summary>
	[JsonProperty("enforce_nonce", NullValueHandling = NullValueHandling.Ignore)]
	public bool EnforceNonce { get; internal set; }

	/// <summary>
	///     Gets or sets the poll request.
	/// </summary>
	[JsonProperty("poll", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordPollRequest? DiscordPollRequest { get; internal set; }
}

/// <summary>
///     Represents a channel message bulk delete payload.
/// </summary>
internal sealed class RestChannelMessageBulkDeletePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the messages.
	/// </summary>
	[JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<ulong> Messages { get; set; }
}

/// <summary>
///     Represents a channel invite create payload.
/// </summary>
internal sealed class RestChannelInviteCreatePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the max age.
	/// </summary>
	[JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxAge { get; set; }

	/// <summary>
	///     Gets or sets the max uses.
	/// </summary>
	[JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxUses { get; set; }

	/// <summary>
	///     Gets or sets the target type.
	/// </summary>
	[JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
	public TargetType? TargetType { get; set; }

	/// <summary>
	///     Gets or sets the target application.
	/// </summary>
	[JsonProperty("target_application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? TargetApplicationId { get; set; }

	/// <summary>
	///     Gets or sets the target user id.
	/// </summary>
	[JsonProperty("target_user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? TargetUserId { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether temporary.
	/// </summary>
	[JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
	public bool Temporary { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether unique.
	/// </summary>
	[JsonProperty("unique", NullValueHandling = NullValueHandling.Ignore)]
	public bool Unique { get; set; }
}

/// <summary>
///     Represents a channel permission edit payload.
/// </summary>
internal sealed class RestChannelPermissionEditPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the allow.
	/// </summary>
	[JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions Allow { get; set; }

	/// <summary>
	///     Gets or sets the deny.
	/// </summary>
	[JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions Deny { get; set; }

	/// <summary>
	///     Gets or sets the type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; set; }
}

/// <summary>
///     Represents a channel group dm recipient add payload.
/// </summary>
internal sealed class RestChannelGroupDmRecipientAddPayload : IOAuth2Payload
{
	/// <summary>
	///     Gets or sets the nickname.
	/// </summary>
	[JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
	public string Nickname { get; set; }

	/// <summary>
	///     Gets or sets the access token.
	/// </summary>
	[JsonProperty("access_token")]
	public string AccessToken { get; set; }
}

/// <summary>
///     The acknowledge payload.
/// </summary>
internal sealed class AcknowledgePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the token.
	/// </summary>
	[JsonProperty("token", NullValueHandling = NullValueHandling.Include)]
	public string Token { get; set; }
}

/// <summary>
///     Represents a thread channel create payload.
/// </summary>
internal sealed class RestThreadChannelCreatePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public RestChannelMessageCreatePayload Message { get; set; }

	/// <summary>
	///     Gets or sets the auto archive duration.
	/// </summary>
	[JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ThreadAutoArchiveDuration?> AutoArchiveDuration { get; set; }

	/// <summary>
	///     Gets or sets the rate limit per user.
	/// </summary>
	[JsonProperty("rate_limit_per_user")]
	public int? PerUserRateLimit { get; set; }

	/// <summary>
	///     Gets or sets the thread type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ChannelType? Type { get; set; }

	/// <summary>
	///     Gets or sets the applied tags.
	/// </summary>
	[JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<ulong>> AppliedTags { internal get; set; }
}

/// <summary>
///     Represents a thread channel modify payload.
/// </summary>
internal sealed class RestThreadChannelModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// <summary>
	///     Gets or sets the archived.
	/// </summary>
	[JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool?> Archived { get; set; }

	/// <summary>
	///     Gets or sets the auto archive duration.
	/// </summary>
	[JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ThreadAutoArchiveDuration?> AutoArchiveDuration { get; set; }

	/// <summary>
	///     Gets or sets the locked.
	/// </summary>
	[JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool?> Locked { get; set; }

	/// <summary>
	///     Gets or sets the per user rate limit.
	/// </summary>
	[JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int?> PerUserRateLimit { get; set; }

	/// <summary>
	///     Gets or sets the thread's invitable state.
	/// </summary>
	[JsonProperty("invitable", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool?> Invitable { internal get; set; }

	/// <summary>
	///     Gets or sets the applied tags.
	/// </summary>
	[JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<ulong>> AppliedTags { internal get; set; }

	/// <summary>
	///     Gets or sets the channel flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Include)]
	public Optional<ChannelFlags?> Flags { internal get; set; }
}

/// <summary>
///     Represents a voice channel status modify payload.
/// </summary>
internal sealed class RestVoiceChannelStatusModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the status.
	/// </summary>
	[JsonProperty("status", NullValueHandling = NullValueHandling.Include)]
	public string? Status { get; set; }
}
