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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord text message.
/// </summary>
public class DiscordMessage : SnowflakeObject, IEquatable<DiscordMessage>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMessage"/> class.
	/// </summary>
	internal DiscordMessage()
	{
		this._attachmentsLazy = new Lazy<IReadOnlyList<DiscordAttachment>>(() => new ReadOnlyCollection<DiscordAttachment>(this.AttachmentsInternal));
		this._embedsLazy = new Lazy<IReadOnlyList<DiscordEmbed>>(() => new ReadOnlyCollection<DiscordEmbed>(this.EmbedsInternal));
		this._mentionedChannelsLazy = new Lazy<IReadOnlyList<DiscordChannel>>(() => this.MentionedChannelsInternal != null
				? new ReadOnlyCollection<DiscordChannel>(this.MentionedChannelsInternal)
				: Array.Empty<DiscordChannel>());
		this._mentionedRolesLazy = new Lazy<IReadOnlyList<DiscordRole>>(() => this.MentionedRolesInternal != null ? new ReadOnlyCollection<DiscordRole>(this.MentionedRolesInternal) : Array.Empty<DiscordRole>());
		this.MentionedUsersLazy = new Lazy<IReadOnlyList<DiscordUser>>(() => new ReadOnlyCollection<DiscordUser>(this.MentionedUsersInternal));
		this._reactionsLazy = new Lazy<IReadOnlyList<DiscordReaction>>(() => new ReadOnlyCollection<DiscordReaction>(this.ReactionsInternal));
		this._stickersLazy = new Lazy<IReadOnlyList<DiscordSticker>>(() => new ReadOnlyCollection<DiscordSticker>(this.StickersInternal));
		this._jumpLink = new Lazy<Uri>(() =>
		{
			string gid = null;
			if (this.Channel != null)
				gid = this.Channel is DiscordDmChannel
					? "@me"
					: this.Channel is DiscordThreadChannel
					? this.INTERNAL_THREAD.GuildId.Value.ToString(CultureInfo.InvariantCulture)
					: this.Channel.GuildId.Value.ToString(CultureInfo.InvariantCulture);

			var cid = this.ChannelId.ToString(CultureInfo.InvariantCulture);
			var mid = this.Id.ToString(CultureInfo.InvariantCulture);

			return new Uri($"https://{(this.Discord.Configuration.UseCanary ? "canary.discord.com" : this.Discord.Configuration.UsePtb ? "ptb.discord.com" : "discord.com")}/channels/{gid}/{cid}/{mid}");
		});
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMessage"/> class.
	/// </summary>
	/// <param name="other">The other message.</param>
	internal DiscordMessage(DiscordMessage other)
		: this()
	{
		this.Discord = other.Discord;

		this.AttachmentsInternal = other.AttachmentsInternal; // the attachments cannot change, thus no need to copy and reallocate.
		this.EmbedsInternal = new List<DiscordEmbed>(other.EmbedsInternal);

		if (other.MentionedChannelsInternal != null)
			this.MentionedChannelsInternal = new List<DiscordChannel>(other.MentionedChannelsInternal);
		if (other.MentionedRolesInternal != null)
			this.MentionedRolesInternal = new List<DiscordRole>(other.MentionedRolesInternal);
		if (other.MentionedRoleIds != null)
			this.MentionedRoleIds = new List<ulong>(other.MentionedRoleIds);
		this.MentionedUsersInternal = new List<DiscordUser>(other.MentionedUsersInternal);
		this.ReactionsInternal = new List<DiscordReaction>(other.ReactionsInternal);
		this.StickersInternal = new List<DiscordSticker>(other.StickersInternal);

		this.Author = other.Author;
		this.ChannelId = other.ChannelId;
		this.Content = other.Content;
		this.EditedTimestampRaw = other.EditedTimestampRaw;
		this.Id = other.Id;
		this.IsTts = other.IsTts;
		this.MessageType = other.MessageType;
		this.Pinned = other.Pinned;
		this.TimestampRaw = other.TimestampRaw;
		this.WebhookId = other.WebhookId;
	}

	/// <summary>
	/// Gets the channel in which the message was sent.
	/// </summary>
	[JsonIgnore]
	public DiscordChannel Channel
	{
		get => (this.Discord as DiscordClient)?.InternalGetCachedChannel(this.ChannelId) ?? this._channel;
		internal set => this._channel = value;
	}

	private DiscordChannel _channel;

	/// <summary>
	/// Gets the thread in which the message was sent.
	/// </summary>
	[JsonIgnore]
	private DiscordThreadChannel INTERNAL_THREAD
	{
		get => (this.Discord as DiscordClient)?.InternalGetCachedThread(this.ChannelId) ?? this._thread;
		set => this._thread = value;
	}

	private DiscordThreadChannel _thread;

	/// <summary>
	/// Gets the ID of the channel in which the message was sent.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the components this message was sent with.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyCollection<DiscordActionRowComponent> Components { get; internal set; }

	/// <summary>
	/// Gets the user or member that sent the message.
	/// </summary>
	[JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser Author { get; internal set; }

	/// <summary>
	/// Gets the message's content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; internal set; }

	/// <summary>
	/// Gets the message's creation timestamp.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Timestamp
		=> !string.IsNullOrWhiteSpace(this.TimestampRaw) && DateTimeOffset.TryParse(this.TimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
			dto : this.CreationTimestamp;

	/// <summary>
	/// Gets the message's creation timestamp as raw string.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string TimestampRaw { get; set; }

	/// <summary>
	/// Gets the message's edit timestamp. Will be null if the message was not edited.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EditedTimestamp
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw) && DateTimeOffset.TryParse(this.EditedTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
			(DateTimeOffset?)dto : null;

	/// <summary>
	/// Gets the message's edit timestamp as raw string. Will be null if the message was not edited.
	/// </summary>
	[JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string EditedTimestampRaw { get; set; }

	/// <summary>
	/// Gets whether this message was edited.
	/// </summary>
	[JsonIgnore]
	public bool IsEdited
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw);

	/// <summary>
	/// Gets whether the message is a text-to-speech message.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsTts { get; internal set; }

	/// <summary>
	/// Gets whether the message mentions everyone.
	/// </summary>
	[JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
	public bool MentionEveryone { get; internal set; }

	/// <summary>
	/// Gets users or members mentioned by this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordUser> MentionedUsers
		=> this.MentionedUsersLazy.Value;

	[JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordUser> MentionedUsersInternal;
	[JsonIgnore]
	internal readonly Lazy<IReadOnlyList<DiscordUser>> MentionedUsersLazy;

	// TODO: this will probably throw an exception in DMs since it tries to wrap around a null List...
	// this is probably low priority but need to find out a clean way to solve it...
	/// <summary>
	/// Gets roles mentioned by this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordRole> MentionedRoles
		=> this._mentionedRolesLazy.Value;

	[JsonIgnore]
	internal List<DiscordRole> MentionedRolesInternal;

	[JsonProperty("mention_roles")]
	internal List<ulong> MentionedRoleIds;

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordRole>> _mentionedRolesLazy;

	/// <summary>
	/// Gets channels mentioned by this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordChannel> MentionedChannels
		=> this._mentionedChannelsLazy.Value;

	[JsonIgnore]
	internal List<DiscordChannel> MentionedChannelsInternal;
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordChannel>> _mentionedChannelsLazy;

	/// <summary>
	/// Gets files attached to this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordAttachment> Attachments
		=> this._attachmentsLazy.Value;

	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordAttachment> AttachmentsInternal = new();
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordAttachment>> _attachmentsLazy;

	/// <summary>
	/// Gets embeds attached to this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordEmbed> Embeds
		=> this._embedsLazy.Value;

	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordEmbed> EmbedsInternal = new();
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordEmbed>> _embedsLazy;

	/// <summary>
	/// Gets reactions used on this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordReaction> Reactions
		=> this._reactionsLazy.Value;

	[JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordReaction> ReactionsInternal = new();
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordReaction>> _reactionsLazy;

	/// <summary>
	/// Gets the nonce sent with the message, if the message was sent by the client.
	/// </summary>
	[JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
	public string Nonce { get; internal set; }


	/// <summary>
	/// Gets whether the message is pinned.
	/// </summary>
	[JsonProperty("pinned", NullValueHandling = NullValueHandling.Ignore)]
	public bool Pinned { get; internal set; }

	/// <summary>
	/// Gets the id of the webhook that generated this message.
	/// </summary>
	[JsonProperty("webhook_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? WebhookId { get; internal set; }

	/// <summary>
	/// Gets the type of the message.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public MessageType? MessageType { get; internal set; }

	/// <summary>
	/// Gets the message activity in the Rich Presence embed.
	/// </summary>
	[JsonProperty("activity", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageActivity Activity { get; internal set; }

	/// <summary>
	/// Gets the message application in the Rich Presence embed.
	/// </summary>
	[JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageApplication Application { get; internal set; }

	/// <summary>
	/// Gets the message application id in the Rich Presence embed.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	/// Gets the internal reference.
	/// </summary>
	[JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
	internal InternalDiscordMessageReference? InternalReference { get; set; }

	/// <summary>
	/// Gets the original message reference from the crossposted message.
	/// </summary>
	[JsonIgnore]
	public DiscordMessageReference Reference
		=> this.InternalReference.HasValue ? this?.InternalBuildMessageReference() : null;

	/// <summary>
	/// Gets the bitwise flags for this message.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	/// <summary>
	/// Gets whether the message originated from a webhook.
	/// </summary>
	[JsonIgnore]
	public bool WebhookMessage
		=> this.WebhookId != null;

	/// <summary>
	/// Gets the jump link to this message.
	/// </summary>
	[JsonIgnore]
	public Uri JumpLink => this._jumpLink.Value;
	private readonly Lazy<Uri> _jumpLink;

	/// <summary>
	/// Gets stickers for this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordSticker> Stickers
		=> this._stickersLazy.Value;

	[JsonProperty("sticker_items", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordSticker> StickersInternal = new();
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordSticker>> _stickersLazy;

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? GuildId { get; set; }

	/// <summary>
	/// Gets the message object for the referenced message
	/// </summary>
	[JsonProperty("referenced_message", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessage ReferencedMessage { get; internal set; }

	/// <summary>
	/// Gets whether the message is a response to an interaction.
	/// </summary>
	[JsonProperty("interaction", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageInteraction Interaction { get; internal set; }

	/// <summary>
	/// Gets the thread that was started from this message.
	/// </summary>
	[JsonProperty("thread", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordThreadChannel Thread { get; internal set; }

	/// <summary>
	/// Build the message reference.
	/// </summary>
	internal DiscordMessageReference InternalBuildMessageReference()
	{
		var client = this.Discord as DiscordClient;
		var guildId = this.InternalReference.Value.GuildId;
		var channelId = this.InternalReference.Value.ChannelId;
		var messageId = this.InternalReference.Value.MessageId;

		var reference = new DiscordMessageReference();

		if (guildId.HasValue)
			reference.Guild = client.GuildsInternal.TryGetValue(guildId.Value, out var g)
				? g
				: new DiscordGuild
				{
					Id = guildId.Value,
					Discord = client
				};

		var channel = client.InternalGetCachedChannel(channelId.Value);

		if (channel == null)
		{
			reference.Channel = new DiscordChannel
			{
				Id = channelId.Value,
				Discord = client
			};

			if (guildId.HasValue)
				reference.Channel.GuildId = guildId.Value;
		}

		else reference.Channel = channel;

		if (client.MessageCache != null && client.MessageCache.TryGet(m => m.Id == messageId.Value && m.ChannelId == channelId, out var msg))
			reference.Message = msg;

		else
		{
			reference.Message = new DiscordMessage
			{
				ChannelId = this.ChannelId,
				Discord = client
			};

			if (messageId.HasValue)
				reference.Message.Id = messageId.Value;
		}

		return reference;
	}


	/// <summary>
	/// Gets the mentions.
	/// </summary>
	/// <returns>An array of IMentions.</returns>
	private List<IMention> GetMentions()
	{
		var mentions = new List<IMention>();

		if (this.ReferencedMessage != null && this.MentionedUsersInternal.Contains(this.ReferencedMessage.Author))
			mentions.Add(new RepliedUserMention());

		if (this.MentionedUsersInternal.Any())
			mentions.AddRange(this.MentionedUsersInternal.Select(m => (IMention)new UserMention(m)));

		if (this.MentionedRoleIds.Any())
			mentions.AddRange(this.MentionedRoleIds.Select(r => (IMention)new RoleMention(r)));

		return mentions;
	}

	/// <summary>
	/// Populates the mentions.
	/// </summary>
	internal void PopulateMentions()
	{
		var guild = this.Channel?.Guild;
		this.MentionedUsersInternal ??= new List<DiscordUser>();
		this.MentionedRolesInternal ??= new List<DiscordRole>();
		this.MentionedChannelsInternal ??= new List<DiscordChannel>();

		var mentionedUsers = new HashSet<DiscordUser>(new DiscordUserComparer());
		if (guild != null)
		{
			foreach (var usr in this.MentionedUsersInternal)
			{
				usr.Discord = this.Discord;
				this.Discord.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					return old;
				});

				mentionedUsers.Add(guild.MembersInternal.TryGetValue(usr.Id, out var member) ? member : usr);
			}
		}
		if (!string.IsNullOrWhiteSpace(this.Content))
		{
			//mentionedUsers.UnionWith(Utilities.GetUserMentions(this).Select(this.Discord.GetCachedOrEmptyUserInternal));
			if (guild != null)
			{
				//this._mentionedRoles = this._mentionedRoles.Union(Utilities.GetRoleMentions(this).Select(xid => guild.GetRole(xid))).ToList();
				this.MentionedRolesInternal = this.MentionedRolesInternal.Union(this.MentionedRoleIds.Select(xid => guild.GetRole(xid))).ToList();
				this.MentionedChannelsInternal = this.MentionedChannelsInternal.Union(Utilities.GetChannelMentions(this).Select(xid => guild.GetChannel(xid))).ToList();
			}
		}

		this.MentionedUsersInternal = mentionedUsers.ToList();
	}

	/// <summary>
	/// Edits the message.
	/// </summary>
	/// <param name="content">New content.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<string> content)
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, default, this.GetMentions(), default, default, Array.Empty<DiscordMessageFile>(), default);

	/// <summary>
	/// Edits the message.
	/// </summary>
	/// <param name="embed">New embed.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<DiscordEmbed> embed = default)
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, embed.Map(v => new[] { v }).ValueOr(Array.Empty<DiscordEmbed>()), this.GetMentions(), default, default, Array.Empty<DiscordMessageFile>(), default);

	/// <summary>
	/// Edits the message.
	/// </summary>
	/// <param name="content">New content.</param>
	/// <param name="embed">New embed.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<DiscordEmbed> embed = default)
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, embed.Map(v => new[] { v }).ValueOr(Array.Empty<DiscordEmbed>()), this.GetMentions(), default, default, Array.Empty<DiscordMessageFile>(), default);

	/// <summary>
	/// Edits the message.
	/// </summary>
	/// <param name="content">New content.</param>
	/// <param name="embeds">New embeds.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds = default)
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, embeds, this.GetMentions(), default, default, Array.Empty<DiscordMessageFile>(), default);

	/// <summary>
	/// Edits the message.
	/// </summary>
	/// <param name="builder">The builder of the message to edit.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> ModifyAsync(DiscordMessageBuilder builder)
	{
		builder.Validate(true);
		return await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, builder.Content, Optional.Some(builder.Embeds.AsEnumerable()), builder.Mentions, builder.Components, builder.Suppressed, builder.Files, builder.Attachments.Count > 0 ? Optional.Some(builder.Attachments.AsEnumerable()) : builder.KeepAttachmentsInternal.HasValue ? builder.KeepAttachmentsInternal.Value ? Optional.Some(this.Attachments.AsEnumerable()) : Array.Empty<DiscordAttachment>() : null);
	}

	/// <summary>
	/// Edits the message embed suppression.
	/// </summary>
	/// <param name="suppress">Suppress embeds.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifySuppressionAsync(bool suppress = false)
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, default, default, default, suppress, default, default);

	/// <summary>
	/// Clears all attachments from the message.
	/// </summary>
	/// <returns></returns>
	public Task<DiscordMessage> ClearAttachmentsAsync()
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, default, this.GetMentions(), default, default, default, Array.Empty<DiscordAttachment>());

	/// <summary>
	/// Edits the message.
	/// </summary>
	/// <param name="action">The builder of the message to edit.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> ModifyAsync(Action<DiscordMessageBuilder> action)
	{
		var builder = new DiscordMessageBuilder();
		action(builder);
		builder.Validate(true);
		return await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, builder.Content, Optional.Some(builder.Embeds.AsEnumerable()), builder.Mentions, builder.Components, builder.Suppressed, builder.Files, builder.Attachments.Count > 0 ? Optional.Some(builder.Attachments.AsEnumerable()) : builder.KeepAttachmentsInternal.HasValue ? builder.KeepAttachmentsInternal.Value ? Optional.Some(this.Attachments.AsEnumerable()) : Array.Empty<DiscordAttachment>() : null);
	}

	/// <summary>
	/// Deletes the message.
	/// </summary>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteMessageAsync(this.ChannelId, this.Id, reason);

	/// <summary>
	/// Creates a thread.
	/// Depending on the <see cref="ChannelType"/> of the parent channel it's either a <see cref="ChannelType.PublicThread"/> or a <see cref="ChannelType.NewsThread"/>.
	/// </summary>
	/// <param name="name">The name of the thread.</param>
	/// <param name="autoArchiveDuration"><see cref="ThreadAutoArchiveDuration"/> till it gets archived. Defaults to <see cref="ThreadAutoArchiveDuration.OneHour"/></param>
	/// <param name="rateLimitPerUser">The per user ratelimit, aka slowdown.</param>
	/// <param name="reason">The reason.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.CreatePrivateThreads"/> or <see cref="Permissions.SendMessagesInThreads"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="System.NotSupportedException">Thrown when the <see cref="ThreadAutoArchiveDuration"/> cannot be modified.</exception>
	public async Task<DiscordThreadChannel> CreateThreadAsync(string name, ThreadAutoArchiveDuration autoArchiveDuration = ThreadAutoArchiveDuration.OneHour, int? rateLimitPerUser = null, string reason = null) =>
		Utilities.CheckThreadAutoArchiveDurationFeature(this.Channel.Guild, autoArchiveDuration)
			? await this.Discord.ApiClient.CreateThreadAsync(this.ChannelId, this.Id, name, autoArchiveDuration, this.Channel.Type == ChannelType.News ? ChannelType.NewsThread : ChannelType.PublicThread, rateLimitPerUser, reason)
			: throw new NotSupportedException($"Cannot modify ThreadAutoArchiveDuration. Guild needs boost tier {(autoArchiveDuration == ThreadAutoArchiveDuration.ThreeDays ? "one" : "two")}.");

	/// <summary>
	/// Pins the message in its channel.
	/// </summary>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task PinAsync()
		=> this.Discord.ApiClient.PinMessageAsync(this.ChannelId, this.Id);

	/// <summary>
	/// Unpins the message in its channel.
	/// </summary>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnpinAsync()
		=> this.Discord.ApiClient.UnpinMessageAsync(this.ChannelId, this.Id);

	/// <summary>
	/// Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="content">Message content to respond with.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(string content)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, content, null, sticker: null, replyMessageId: this.Id, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(DiscordEmbed embed)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, null, embed != null ? new[] { embed } : null, sticker: null, replyMessageId: this.Id, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="content">Message content to respond with.</param>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(string content, DiscordEmbed embed)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, content, embed != null ? new[] { embed } : null, sticker: null, replyMessageId: this.Id, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="builder">The Discord message builder.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(DiscordMessageBuilder builder)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, builder.WithReply(this.Id, mention: false, failOnInvalidReply: false));

	/// <summary>
	/// Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="action">The Discord message builder.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(Action<DiscordMessageBuilder> action)
	{
		var builder = new DiscordMessageBuilder();
		action(builder);
		return this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, builder.WithReply(this.Id, mention: false, failOnInvalidReply: false));
	}

	/// <summary>
	/// Creates a reaction to this message.
	/// </summary>
	/// <param name="emoji">The emoji you want to react with, either an emoji or name:id</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.AddReactions"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task CreateReactionAsync(DiscordEmoji emoji)
		=> this.Discord.ApiClient.CreateReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

	/// <summary>
	/// Deletes your own reaction
	/// </summary>
	/// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteOwnReactionAsync(DiscordEmoji emoji)
		=> this.Discord.ApiClient.DeleteOwnReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

	/// <summary>
	/// Deletes another user's reaction.
	/// </summary>
	/// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id.</param>
	/// <param name="user">Member you want to remove the reaction for</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteReactionAsync(DiscordEmoji emoji, DiscordUser user, string reason = null)
		=> this.Discord.ApiClient.DeleteUserReactionAsync(this.ChannelId, this.Id, user.Id, emoji.ToReactionString(), reason);

	/// <summary>
	/// Gets users that reacted with this emoji.
	/// </summary>
	/// <param name="emoji">Emoji to react with.</param>
	/// <param name="limit">Limit of users to fetch.</param>
	/// <param name="after">Fetch users after this user's id.</param>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(DiscordEmoji emoji, int limit = 25, ulong? after = null)
		=> this.GetReactionsInternalAsync(emoji, limit, after);

	/// <summary>
	/// Deletes all reactions for this message.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAllReactionsAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteAllReactionsAsync(this.ChannelId, this.Id, reason);

	/// <summary>
	/// Deletes all reactions of a specific reaction for this message.
	/// </summary>
	/// <param name="emoji">The emoji to clear, either an emoji or name:id.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageMessages"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteReactionsEmojiAsync(DiscordEmoji emoji)
		=> this.Discord.ApiClient.DeleteReactionsEmojiAsync(this.ChannelId, this.Id, emoji.ToReactionString());

	/// <summary>
	/// Gets the reactions.
	/// </summary>
	/// <param name="emoji">The emoji to search for.</param>
	/// <param name="limit">The limit of results.</param>
	/// <param name="after">Get the reasctions after snowflake.</param>
	private async Task<IReadOnlyList<DiscordUser>> GetReactionsInternalAsync(DiscordEmoji emoji, int limit = 25, ulong? after = null)
	{
		if (limit < 0)
			throw new ArgumentException("Cannot get a negative number of reactions' users.");

		if (limit == 0)
			return Array.Empty<DiscordUser>();

		var users = new List<DiscordUser>(limit);
		var remaining = limit;
		var last = after;

		int lastCount;
		do
		{
			var fetchSize = remaining > 100 ? 100 : remaining;
			var fetch = await this.Discord.ApiClient.GetReactionsAsync(this.Channel.Id, this.Id, emoji.ToReactionString(), last, fetchSize).ConfigureAwait(false);

			lastCount = fetch.Count;
			remaining -= lastCount;

			users.AddRange(fetch);
			last = fetch.LastOrDefault()?.Id;
		} while (remaining > 0 && lastCount > 0);

		return new ReadOnlyCollection<DiscordUser>(users);
	}

	/// <summary>
	/// Returns a string representation of this message.
	/// </summary>
	/// <returns>String representation of this message.</returns>
	public override string ToString()
		=> $"Message {this.Id}; Attachment count: {this.AttachmentsInternal.Count}; Embed count: {this.EmbedsInternal.Count}; Contents: {this.Content}";

	/// <summary>
	/// Checks whether this <see cref="DiscordMessage"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordMessage"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordMessage);

	/// <summary>
	/// Checks whether this <see cref="DiscordMessage"/> is equal to another <see cref="DiscordMessage"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordMessage"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordMessage"/> is equal to this <see cref="DiscordMessage"/>.</returns>
	public bool Equals(DiscordMessage e)
		=> e is not null && (ReferenceEquals(this, e) || (this.Id == e.Id && this.ChannelId == e.ChannelId));

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordMessage"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordMessage"/>.</returns>
	public override int GetHashCode()
	{
		var hash = 13;

		hash = (hash * 7) + this.Id.GetHashCode();
		hash = (hash * 7) + this.ChannelId.GetHashCode();

		return hash;
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordMessage"/> objects are equal.
	/// </summary>
	/// <param name="e1">First message to compare.</param>
	/// <param name="e2">Second message to compare.</param>
	/// <returns>Whether the two messages are equal.</returns>
	public static bool operator ==(DiscordMessage e1, DiscordMessage e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null)
			&& (o1 == null || o2 != null)
			&& ((o1 == null && o2 == null) || (e1.Id == e2.Id && e1.ChannelId == e2.ChannelId));
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordMessage"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First message to compare.</param>
	/// <param name="e2">Second message to compare.</param>
	/// <returns>Whether the two messages are not equal.</returns>
	public static bool operator !=(DiscordMessage e1, DiscordMessage e2)
		=> !(e1 == e2);
}
