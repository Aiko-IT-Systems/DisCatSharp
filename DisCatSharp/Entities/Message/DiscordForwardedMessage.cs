using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a forwarded <see cref="DiscordMessage"/> which contains only a subset of fields.
/// </summary>
[DiscordInExperiment, Experimental("This is subject to change at any time.")]
public sealed class DiscordForwardedMessage : ObservableApiObject
{
	/// <summary>
	/// Constructs a new <see cref="DiscordForwardedMessage"/>.
	/// </summary>
	internal DiscordForwardedMessage()
	{
		this._attachmentsLazy = new(() => new ReadOnlyCollection<DiscordAttachment>(this.AttachmentsInternal));
		this._embedsLazy = new(() => new ReadOnlyCollection<DiscordEmbed>(this.EmbedsInternal));
		this._mentionedChannelsLazy = new(() => new ReadOnlyCollection<DiscordChannel>(this.MentionedChannelsInternal));
		this._mentionedRolesLazy = new(() => new ReadOnlyCollection<DiscordRole>(this.MentionedRolesInternal));
		this._mentionedUsersLazy = new(() => new ReadOnlyCollection<DiscordUser>(this.MentionedUsersInternal));
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordForwardedMessage"/>.
	/// </summary>
	/// <param name="guildId">Optional guild id.</param>
	internal DiscordForwardedMessage(ulong guildId)
	{
		this._attachmentsLazy = new(() => new ReadOnlyCollection<DiscordAttachment>(this.AttachmentsInternal));
		this._embedsLazy = new(() => new ReadOnlyCollection<DiscordEmbed>(this.EmbedsInternal));
		this._mentionedChannelsLazy = new(() => new ReadOnlyCollection<DiscordChannel>(this.MentionedChannelsInternal));
		this._mentionedRolesLazy = new(() => new ReadOnlyCollection<DiscordRole>(this.MentionedRolesInternal));
		this._mentionedUsersLazy = new(() => new ReadOnlyCollection<DiscordUser>(this.MentionedUsersInternal));
		this.GuildId = guildId;
	}

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	internal ulong? GuildId { get; set; } = null;

	/// <summary>
	/// Gets the type of the forwarded message.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public MessageType? MessageType { get; internal set; }

	/// <summary>
	/// Gets the forwarded message's content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string? Content { get; internal set; }

	/// <summary>
	/// Gets embeds attached to this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordEmbed> Embeds
		=> this._embedsLazy.Value;

	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordEmbed> EmbedsInternal = [];

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordEmbed>> _embedsLazy;

	/// <summary>
	/// Gets files attached to this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordAttachment> Attachments
		=> this._attachmentsLazy.Value;

	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordAttachment> AttachmentsInternal = [];

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordAttachment>> _attachmentsLazy;

	/// <summary>
	/// Gets the forwarded message's creation timestamp.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Timestamp
		=> DateTimeOffset.TryParse(this.TimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : throw new ArithmeticException("Could not convert timestamp to DateTimeOffset");

	/// <summary>
	/// Gets the forwarded message's creation timestamp as raw string.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string TimestampRaw { get; set; }

	/// <summary>
	/// Gets the forwarded message's edit timestamp. Will be null if the forwarded message was not edited.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EditedTimestamp
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw) && DateTimeOffset.TryParse(this.EditedTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets the forwarded message's edit timestamp as raw string. Will be null if the forwarded message was not edited.
	/// </summary>
	[JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string? EditedTimestampRaw { get; set; }

	/// <summary>
	/// Gets whether this forwarded message was edited.
	/// </summary>
	[JsonIgnore]
	public bool IsEdited
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw);

	/// <summary>
	/// Gets the bitwise flags for this forwarded message.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	/// <summary>
	/// Gets whether the forwarded message mentions everyone.
	/// </summary>
	[JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
	public bool MentionEveryone { get; internal set; }

	/// <summary>
	/// Gets users or members mentioned by this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordUser> MentionedUsers
		=> this._mentionedUsersLazy.Value;

	[JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordUser> MentionedUsersInternal { get; set; } = [];

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordUser>> _mentionedUsersLazy;

	/// <summary>
	/// Gets roles mentioned by this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordRole> MentionedRoles
		=> this._mentionedRolesLazy.Value;

	[JsonIgnore]
	internal List<DiscordRole> MentionedRolesInternal
		=> this.GuildId.HasValue && this.Discord.Guilds.ContainsKey(this.GuildId.Value)
			? this.MentionedRoleIds.Select(x => this.Discord.Guilds[this.GuildId.Value].GetRole(x)).ToList()
			: [];

	/// <summary>
	/// Gets role ids mentioned by this forwarded message.
	/// If the bot is in the forwarded guild and has the guild on it's shard, you can use <see cref="MentionedRoles"/> to get the <see cref="DiscordRole"/>'s directly.
	/// </summary>
	[JsonProperty("mention_roles")]
	public List<ulong> MentionedRoleIds = [];

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordRole>> _mentionedRolesLazy;

	/// <summary>
	/// Gets channels mentioned by this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordChannel> MentionedChannels
		=> this._mentionedChannelsLazy.Value;

	[JsonIgnore]
	internal List<DiscordChannel> MentionedChannelsInternal = [];

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordChannel>> _mentionedChannelsLazy;

	/// <summary>
	/// Gets all mentions from this forwarded message.
	/// </summary>
	internal void GetMentions()
	{ }
}
