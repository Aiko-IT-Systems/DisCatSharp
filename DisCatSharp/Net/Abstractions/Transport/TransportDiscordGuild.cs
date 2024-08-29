using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordGuild : SnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string? Icon { get; internal set; }

	[JsonProperty("icon_hash", NullValueHandling = NullValueHandling.Ignore)]
	public string? IconHash { get; internal set; }

	[JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
	public string? Splash { get; internal set; }

	[JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
	public string? DiscoverySplash { get; internal set; }

	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong OwnerId { get; internal set; }

	[JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
	public string? Region { get; internal set; }

	[JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? AfkChannelId { get; internal set; }

	[JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
	public int AfkTimeout { get; internal set; }

	[JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
	public VerificationLevel VerificationLevel { get; internal set; }

	[JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
	public DefaultMessageNotifications DefaultMessageNotifications { get; internal set; }

	[JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
	public ExplicitContentFilter ExplicitContentFilter { get; internal set; }

	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordRole>? Roles { get; internal set; }

	[JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordEmoji>? Emojis { get; internal set; }

	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? Features { get; internal set; }

	[JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
	public MfaLevel MfaLevel { get; internal set; }

	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	[JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? SystemChannelId { get; internal set; }

	[JsonProperty("system_channel_flags", NullValueHandling = NullValueHandling.Ignore)]
	public SystemChannelFlags SystemChannelFlags { get; internal set; }

	[JsonProperty("rules_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? RulesChannelId { get; internal set; }

	[JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? JoinedAt { get; internal set; }

	[JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
	public bool Large { get; internal set; }

	[JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
	public bool Unavailable { get; internal set; }

	[JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int MemberCount { get; internal set; }

	[JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordVoiceState>? VoiceStates { get; internal set; }

	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordGuildMember>? Members { get; internal set; }

	[JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordChannel>? Channels { get; internal set; }

	[JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordThreadChannel>? Threads { get; internal set; }

	[JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordPresenceUpdate>? Presences { get; internal set; }

	[JsonProperty("max_presences", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxPresences { get; internal set; }

	[JsonProperty("max_members", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxMembers { get; internal set; }

	[JsonProperty("vanity_url_code", NullValueHandling = NullValueHandling.Ignore)]
	public string? VanityUrlCode { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public string? Banner { get; internal set; }

	[JsonProperty("premium_tier", NullValueHandling = NullValueHandling.Ignore)]
	public PremiumTier PremiumTier { get; internal set; }

	[JsonProperty("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? PremiumSubscriptionCount { get; internal set; }

	[JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
	public string? PreferredLocale { get; internal set; }

	[JsonProperty("public_updates_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? PublicUpdatesChannelId { get; internal set; }

	[JsonProperty("max_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxVideoChannelUsers { get; internal set; }

	[JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximateMemberCount { get; internal set; }

	[JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximatePresenceCount { get; internal set; }

	[JsonProperty("welcome_screen", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordWelcomeScreen? WelcomeScreen { get; internal set; }

	[JsonProperty("nsfw_level", NullValueHandling = NullValueHandling.Ignore)]
	public NsfwLevel NsfwLevel { get; internal set; }

	[JsonProperty("stage_instances", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordStageInstance>? StageInstances { get; internal set; }

	[JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordSticker>? Stickers { get; internal set; }

	[JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordGuildScheduledEvent>? GuildScheduledEvents { get; internal set; }
}
