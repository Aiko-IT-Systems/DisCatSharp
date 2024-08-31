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
///     Represents a forwarded <see cref="DiscordMessage" /> which contains only a subset of fields.
/// </summary>
[DiscordInExperiment, Experimental("This is subject to change at any time.")]
public sealed class DiscordForwardedMessage : ObservableApiObject
{
	/// <summary>
	///     Holds the list of <see cref="DiscordAttachment" />s.
	/// </summary>
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordAttachment>> _attachmentsLazy;

	/// <summary>
	///     Holds the list of <see cref="DiscordEmbed" />s.
	/// </summary>
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordEmbed>> _embedsLazy;

	/// <summary>
	///     Holds the list of <see cref="DiscordChannel" /> mentions.
	/// </summary>
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordChannel>> _mentionedChannelsLazy;

	/// <summary>
	///     Holds the list of <see cref="DiscordRole" /> mentions.
	/// </summary>
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordRole>> _mentionedRolesLazy;

	/// <summary>
	///     Holds the list of <see cref="DiscordRole" /> mentions.
	/// </summary>
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordUser>> _mentionedUsersLazy;

	/// <summary>
	///     Holds the list of <see cref="DiscordSticker"/>s.
	/// </summary>
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordSticker>> _stickersLazy;

	/// <summary>
	///     Holds the list of <see cref="DiscordComponent"/>s.
	/// </summary>
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordComponent>> _componentsLazy;

	/// <summary>
	///     Gets the <see cref="DiscordAttachment" />s.
	/// </summary>
	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordAttachment> AttachmentsInternal = [];

	/// <summary>
	///     Gets the attached <see cref="DiscordEmbed" />s.
	/// </summary>
	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordEmbed> EmbedsInternal = [];

	/// <summary>
	///     Gets the attached <see cref="DiscordComponent"/>s.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordComponent> ComponentsInternal = [];

	/// <summary>
	///     Constructs the list of <see cref="DiscordChannel" /> mentions.
	/// </summary>
	[JsonIgnore]
	internal List<DiscordChannel> MentionedChannelsInternal = [];

	/// <summary>
	///     Gets role ids mentioned by this forwarded message.
	///     If the bot is in the forwarded guild and has the guild on it's shard, you can use <see cref="MentionedRoles" /> to
	///     get the <see cref="DiscordRole" />'s directly.
	/// </summary>
	[JsonProperty("mention_roles")]
	public List<ulong> MentionedRoleIds = [];

	/// <summary>
	///     Gets components for this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordComponent> Components
		=> this._componentsLazy.Value;

	/// <summary>
	///     Gets stickers for this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordSticker> Stickers
		=> this._stickersLazy.Value;

	/// <summary>
	///     Constructs the list of <see cref="DiscordRole" /> mentions.
	/// </summary>
	[JsonIgnore]
	internal List<DiscordRole> MentionedRolesInternal = [];

	[JsonProperty("sticker_items", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordSticker> StickersInternal = [];

	/// <summary>
	///     Constructs a new <see cref="DiscordForwardedMessage" />.
	/// </summary>
	internal DiscordForwardedMessage()
	{
		this._attachmentsLazy = new(() => new ReadOnlyCollection<DiscordAttachment>(this.AttachmentsInternal));
		this._embedsLazy = new(() => new ReadOnlyCollection<DiscordEmbed>(this.EmbedsInternal));
		this._mentionedChannelsLazy = new(() => new ReadOnlyCollection<DiscordChannel>(this.MentionedChannelsInternal));
		this._mentionedRolesLazy = new(() => new ReadOnlyCollection<DiscordRole>(this.MentionedRolesInternal));
		this._mentionedUsersLazy = new(() => new ReadOnlyCollection<DiscordUser>(this.MentionedUsersInternal));
		this._stickersLazy = new(() => new ReadOnlyCollection<DiscordSticker>(this.StickersInternal));
		this._componentsLazy = new(() => new ReadOnlyCollection<DiscordComponent>(this.ComponentsInternal));
	}

	/// <summary>
	///     Constructs a new <see cref="DiscordForwardedMessage" />.
	/// </summary>
	/// <param name="guildId">Optional guild id.</param>
	internal DiscordForwardedMessage(ulong guildId)
	{
		this._attachmentsLazy = new(() => new ReadOnlyCollection<DiscordAttachment>(this.AttachmentsInternal));
		this._embedsLazy = new(() => new ReadOnlyCollection<DiscordEmbed>(this.EmbedsInternal));
		this._mentionedChannelsLazy = new(() => new ReadOnlyCollection<DiscordChannel>(this.MentionedChannelsInternal));
		this._mentionedRolesLazy = new(() => new ReadOnlyCollection<DiscordRole>(this.MentionedRolesInternal));
		this._mentionedUsersLazy = new(() => new ReadOnlyCollection<DiscordUser>(this.MentionedUsersInternal));
		this._stickersLazy = new(() => new ReadOnlyCollection<DiscordSticker>(this.StickersInternal));
		this.GuildId = guildId;
	}

	/// <summary>
	///     Gets the guild id.
	/// </summary>
	internal ulong? GuildId { get; set; } = null;

	/// <summary>
	///     Gets the type of the forwarded message.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public MessageType? MessageType { get; internal set; }

	/// <summary>
	///     Gets the forwarded message's content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string? Content { get; internal set; }

	/// <summary>
	///     Gets embeds attached to this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordEmbed> Embeds
		=> this._embedsLazy.Value;

	/// <summary>
	///     Gets the attached <see cref="DiscordAttachment" />s.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordAttachment> Attachments
		=> this._attachmentsLazy.Value;

	/// <summary>
	///     Gets the forwarded message's creation timestamp.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Timestamp
		=> DateTimeOffset.TryParse(this.TimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : throw new ArithmeticException("Could not convert timestamp to DateTimeOffset");

	/// <summary>
	///     Gets the forwarded message's creation timestamp as raw string.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string TimestampRaw { get; set; }

	/// <summary>
	///     Gets the forwarded message's edit timestamp. Will be null if the forwarded message was not edited.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EditedTimestamp
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw) && DateTimeOffset.TryParse(this.EditedTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets the forwarded message's edit timestamp as raw string. Will be null if the forwarded message was not edited.
	/// </summary>
	[JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string? EditedTimestampRaw { get; set; }

	/// <summary>
	///     Gets whether this forwarded message was edited.
	/// </summary>
	[JsonIgnore]
	public bool IsEdited
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw);

	/// <summary>
	///     Gets the bitwise flags for this forwarded message.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	/// <summary>
	///     Gets whether the forwarded message mentions everyone.
	/// </summary>
	[JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
	public bool MentionEveryone { get; internal set; }

	/// <summary>
	///     Gets users or members mentioned by this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordUser> MentionedUsers
		=> this._mentionedUsersLazy.Value;

	/// <summary>
	///     Gets the mentioned <see cref="DiscordUser" />s.
	/// </summary>
	[JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordUser> MentionedUsersInternal { get; set; } = [];

	/// <summary>
	///     Gets roles mentioned by this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordRole> MentionedRoles
		=> this._mentionedRolesLazy.Value;

	/// <summary>
	///     Gets channels mentioned by this forwarded message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordChannel> MentionedChannels
		=> this._mentionedChannelsLazy.Value;

	/// <summary>
	///     Gets the mentions of this forwarded message.
	/// </summary>
	/// <returns>An array of IMentions.</returns>
	private List<IMention> GetMentions()
	{
		var mentions = new List<IMention>();

		try
		{
			if (this.MentionedUsersInternal.Count is not 0)
				mentions.AddRange(this.MentionedUsersInternal.Select(m => (IMention)new UserMention(m)));

			if (this.MentionedRoleIds.Count is not 0)
				mentions.AddRange(this.MentionedRoleIds.Select(r => (IMention)new RoleMention(r)));
		}
		catch
		{ }

		return mentions;
	}

	/// <summary>
	///     Populates the mentions of this forwarded message.
	/// </summary>
	internal void PopulateMentions()
	{
		var guild = this.GuildId.HasValue && this.Discord.Guilds.TryGetValue(this.GuildId.Value, out var gld) ? gld : null;
		this.MentionedUsersInternal ??= [];
		this.MentionedRolesInternal ??= [];
		this.MentionedChannelsInternal ??= [];

		var mentionedUsers = new HashSet<DiscordUser>(new DiscordUserComparer());
		if (guild is not null)
			foreach (var usr in this.MentionedUsersInternal)
			{
				usr.Discord = this.Discord;
				this.Discord.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					old.GlobalName = usr.GlobalName;
					return old;
				});

				mentionedUsers.Add(guild.MembersInternal.TryGetValue(usr.Id, out var member) ? member : usr);
			}

		if (!string.IsNullOrWhiteSpace(this.Content))
		{
			if (guild is not null)
				this.MentionedRolesInternal = this.MentionedRolesInternal.Union(this.MentionedRoleIds.Select(xid => guild.GetRole(xid))).ToList();

			this.MentionedChannelsInternal = this.MentionedChannelsInternal.Union(Utilities.GetChannelMentions(this.Content).Select(xid => guild.GetChannel(xid))).ToList();
		}

		this.MentionedUsersInternal = [.. mentionedUsers];
	}
}
