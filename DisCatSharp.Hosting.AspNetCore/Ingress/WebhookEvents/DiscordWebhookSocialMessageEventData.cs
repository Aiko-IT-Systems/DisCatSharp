using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Represents common Social SDK webhook message fields shared by lobby and game direct message events.
/// </summary>
public abstract class DiscordWebhookSocialMessageEventData : ObservableApiObject
{
	/// <summary>
	///     Gets the message identifier.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; internal set; }

	/// <summary>
	///     Gets the Discord message type when present.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public MessageType? Type { get; internal set; }

	/// <summary>
	///     Gets the message content when present.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string? Content { get; internal set; }

	/// <summary>
	///     Gets the lobby identifier for lobby or linked-channel message contexts.
	/// </summary>
	[JsonProperty("lobby_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? LobbyId { get; internal set; }

	/// <summary>
	///     Gets the channel identifier when present.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	/// <summary>
	///     Gets the message author when present.
	/// </summary>
	[JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser? Author { get; internal set; }

	/// <summary>
	///     Gets the message timestamp as received from Discord.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public string? TimestampRaw { get; internal set; }

	/// <summary>
	///     Gets the parsed message timestamp when present.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? Timestamp
		=> !string.IsNullOrWhiteSpace(this.TimestampRaw) && DateTimeOffset.TryParse(this.TimestampRaw, out var timestamp) ? timestamp : null;

	/// <summary>
	///     Gets the message edit timestamp as received from Discord.
	/// </summary>
	[JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public string? EditedTimestampRaw { get; internal set; }

	/// <summary>
	///     Gets the parsed message edit timestamp when present.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EditedTimestamp
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw) && DateTimeOffset.TryParse(this.EditedTimestampRaw, out var timestamp) ? timestamp : null;

	/// <summary>
	///     Gets the message metadata flags when present.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	/// <summary>
	///     Gets the application identifier when present.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	///     Gets the Social SDK channel context when present.
	/// </summary>
	[JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordWebhookSocialChannel? Channel { get; internal set; }

	/// <summary>
	///     Gets the message activity payload when present.
	/// </summary>
	[JsonProperty("activity", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageActivity? Activity { get; internal set; }

	/// <summary>
	///     Gets the embedded message application when present.
	/// </summary>
	[JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageApplication? Application { get; internal set; }

	/// <summary>
	///     Gets the message attachments when present.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordAttachment> Attachments { get; internal set; } = [];

	/// <summary>
	///     Gets the message components when present.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordComponent> Components { get; internal set; } = [];
}
