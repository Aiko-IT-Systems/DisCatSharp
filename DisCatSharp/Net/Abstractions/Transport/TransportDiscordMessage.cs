using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordMessage : SnowflakeObject
{
	[JsonProperty("channel_id")]
	public ulong ChannelId { get; internal set; }

	[JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser Author { get; internal set; }

	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string? Content { get; internal set; }

	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset Timestamp { get; internal set; }

	[JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? EditedTimestamp { get; internal set; }

	[JsonProperty("tts")]
	public bool IsTts { get; internal set; }

	[JsonProperty("mention_everyone")]
	public bool MentionEveryone { get; internal set; }

	[JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordUser> MentionedUsers { get; internal set; } = [];

	[JsonProperty("mention_roles", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> MentionedRoleIds { get; internal set; } = [];

	[JsonProperty("mention_channels", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordChannel> MentionedChannels { get; internal set; } = [];

	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordAttachment> Attachments { get; internal set; } = [];

	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordEmbed> Embeds { get; internal set; } = [];

	[JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordReaction> Reactions { get; internal set; } = [];

	[JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
	public string? Nonce { get; internal set; }

	[JsonProperty("pinned")]
	public bool Pinned { get; internal set; }

	[JsonProperty("webhook_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? WebhookId { get; internal set; }

	[JsonProperty("type")]
	public MessageType MessageType { get; internal set; }

	[JsonProperty("activity", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordMessageActivity? Activity { get; internal set; }

	[JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordMessageApplication? Application { get; internal set; }

	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	[JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordMessageReference? MessageReference { get; internal set; }

	[JsonProperty("referenced_message", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordMessage? ReferencedMessage { get; internal set; }

	[JsonProperty("interaction_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordInteractionMetadata? InteractionMetadata { get; internal set; }

	[JsonProperty("interaction", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordMessageInteraction? Interaction { get; internal set; }

	[JsonProperty("thread", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordThreadChannel? Thread { get; internal set; }

	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordActionRowComponent>? Components { get; internal set; }

	[JsonProperty("sticker_items", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordSticker>? Stickers { get; internal set; }

	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int? Position { get; internal set; }

	[JsonProperty("role_subscription_data", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordRoleSubscriptionData? RoleSubscriptionData { get; internal set; }

	[JsonProperty("resolved", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordInteractionResolvedCollection? Resolved { get; internal set; }

	[JsonProperty("poll", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordPoll? Poll { get; internal set; }

	[JsonProperty("call", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordMessageCall? Call { get; internal set; }
}
