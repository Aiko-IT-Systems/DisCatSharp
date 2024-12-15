using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord text message.
/// </summary>
public class DiscordMessage : SnowflakeObject, IEquatable<DiscordMessage>
{
	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordAttachment>> _attachmentsLazy;

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordEmbed>> _embedsLazy;

	private readonly Lazy<Uri> _jumpLink;

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordChannel>> _mentionedChannelsLazy;

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordRole>> _mentionedRolesLazy;

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordUser>> _mentionedUsersLazy;

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordReaction>> _reactionsLazy;

	[JsonProperty("thread", NullValueHandling = NullValueHandling.Ignore)]
	private readonly DiscordThreadChannel _startedThread;

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordSticker>> _stickersLazy;

	private DiscordChannel _channel;

	private DiscordThreadChannel _thread;

	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordAttachment> AttachmentsInternal = [];

	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordEmbed> EmbedsInternal = [];

	[JsonIgnore]
	internal List<DiscordChannel> MentionedChannelsInternal = [];

	[JsonProperty("mention_roles")]
	public List<ulong> MentionedRoleIds = [];

	[JsonIgnore]
	internal List<DiscordRole> MentionedRolesInternal = [];

	[JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordUser> MentionedUsersInternal = [];

	[JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordReaction> ReactionsInternal = [];

	[JsonProperty("sticker_items", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordSticker> StickersInternal = [];

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordMessage" /> class.
	/// </summary>
	internal DiscordMessage()
	{
		this._attachmentsLazy = new(() => new ReadOnlyCollection<DiscordAttachment>(this.AttachmentsInternal));
		this._embedsLazy = new(() => new ReadOnlyCollection<DiscordEmbed>(this.EmbedsInternal));
		this._mentionedChannelsLazy = new(() => new ReadOnlyCollection<DiscordChannel>(this.MentionedChannelsInternal));
		this._mentionedRolesLazy = new(() => new ReadOnlyCollection<DiscordRole>(this.MentionedRolesInternal));
		this._mentionedUsersLazy = new(() => new ReadOnlyCollection<DiscordUser>(this.MentionedUsersInternal));
		this._reactionsLazy = new(() => new ReadOnlyCollection<DiscordReaction>(this.ReactionsInternal));
		this._stickersLazy = new(() => new ReadOnlyCollection<DiscordSticker>(this.StickersInternal));
		this._jumpLink = new(() =>
		{
			string? gid = null;
			if (this.Channel != null!)
				gid = this.Channel is DiscordDmChannel
					? "@me"
					: this.Channel is DiscordThreadChannel
						? this.INTERNAL_THREAD?.GuildId?.ToString(CultureInfo.InvariantCulture)
						: this.GuildId.HasValue
							? this.GuildId.Value.ToString(CultureInfo.InvariantCulture)
							: this.Channel.GuildId?.ToString(CultureInfo.InvariantCulture);

			var cid = this.ChannelId.ToString(CultureInfo.InvariantCulture);
			var mid = this.Id.ToString(CultureInfo.InvariantCulture);

			var baseUrl = this.Discord.Configuration.ApiChannel switch
			{
				ApiChannel.Stable => "discord.com",
				ApiChannel.Ptb => "ptb.discord.com",
				ApiChannel.Canary => "canary.discord.com",
				ApiChannel.Staging => "staging.discord.co",
				_ => throw new ArgumentException("Invalid api channel")
			};
			return new($"https://{baseUrl}/channels/{gid}/{cid}/{mid}");
		});
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordMessage" /> class.
	/// </summary>
	/// <param name="other">The other message.</param>
	internal DiscordMessage(DiscordMessage other)
		: this()
	{
		this.Discord = other.Discord;

		this.AttachmentsInternal = other.AttachmentsInternal; // the attachments cannot change, thus no need to copy and reallocate.
		this.EmbedsInternal = [..other.EmbedsInternal];

		this.MentionedChannelsInternal = [..other.MentionedChannelsInternal];
		this.MentionedRolesInternal = [..other.MentionedRolesInternal];
		this.MentionedRoleIds = [..other.MentionedRoleIds];
		this.MentionedUsersInternal = [..other.MentionedUsersInternal];
		this.ReactionsInternal = [..other.ReactionsInternal];
		this.StickersInternal = [..other.StickersInternal];

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
		this.GuildId = other.GuildId;
		this.Resolved = other.Resolved;
		this.Interaction = other.Interaction;
		this.InteractionMetadata = other.InteractionMetadata;
		if (other.InteractionMetadata is not null && this.InteractionMetadata is not null)
			this.InteractionMetadata.Discord = other.InteractionMetadata.Discord;
		this.Poll = other.Poll;
		if (this.Poll is null)
			return;

		this.Poll.ChannelId = this.ChannelId;
		this.Poll.MessageId = this.Id;
		this.ChannelType = other.ChannelType;
	}

	/// <summary>
	///     Gets the channel in which the message was sent.
	/// </summary>
	[JsonIgnore]
	public DiscordChannel Channel
	{
		get => (this.Discord as DiscordClient)?.InternalGetCachedChannel(this.ChannelId, this.GuildId) ?? this._channel;
		internal set => this._channel = value;
	}

	/// <summary>
	///     Gets the type of the <see cref="Channel" /> this message was send in.
	/// </summary>
	[JsonProperty("channel_type", NullValueHandling = NullValueHandling.Ignore)]
	public ChannelType ChannelType { get; internal set; }

	/// <summary>
	///     Gets the thread in which the message was sent.
	/// </summary>
	[JsonIgnore]
	private DiscordThreadChannel INTERNAL_THREAD
	{
		get => (this.Discord as DiscordClient)?.InternalGetCachedThread(this.ChannelId) ?? this._thread;
		set => this._thread = value;
	}

	/// <summary>
	///     Gets the ID of the channel in which the message was sent.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	///     Currently unknown.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int? Position { get; internal set; }

	/// <summary>
	///     Gets the components this message was sent with.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyCollection<DiscordComponent> Components { get; internal set; }

	/// <summary>
	///     Gets the user or member that sent the message.
	/// </summary>
	[JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser Author { get; internal set; }

	[JsonProperty("member", NullValueHandling = NullValueHandling.Ignore), SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	private TransportMember TRANSPORT_MEMBER { get; set; }

	/// <summary>
	///     Gets the message's content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; internal set; }

	/// <summary>
	///     Gets the message's creation timestamp.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Timestamp
		=> !string.IsNullOrWhiteSpace(this.TimestampRaw) && DateTimeOffset.TryParse(this.TimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : this.CreationTimestamp;

	/// <summary>
	///     Gets the message's creation timestamp as raw string.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string TimestampRaw { get; set; }

	/// <summary>
	///     Gets the message's edit timestamp. Will be null if the message was not edited.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EditedTimestamp
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw) && DateTimeOffset.TryParse(this.EditedTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets the message's edit timestamp as raw string. Will be null if the message was not edited.
	/// </summary>
	[JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string EditedTimestampRaw { get; set; }

	/// <summary>
	///     Gets whether this message was edited.
	/// </summary>
	[JsonIgnore]
	public bool IsEdited
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw);

	/// <summary>
	///     Gets whether the message is a text-to-speech message.
	/// </summary>
	[JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsTts { get; internal set; }

	/// <summary>
	///     Gets whether the message mentions everyone.
	/// </summary>
	[JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
	public bool MentionEveryone { get; internal set; }

	/// <summary>
	///     Gets users or members mentioned by this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordUser> MentionedUsers
		=> this._mentionedUsersLazy.Value;

	// TODO: this will probably throw an exception in DMs since it tries to wrap around a null List...
	// this is probably low priority but need to find out a clean way to solve it...
	/// <summary>
	///     Gets roles mentioned by this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordRole> MentionedRoles
		=> this._mentionedRolesLazy.Value;

	/// <summary>
	///     Gets channels mentioned by this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordChannel> MentionedChannels
		=> this._mentionedChannelsLazy.Value;

	/// <summary>
	///     Gets files attached to this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordAttachment> Attachments
		=> this._attachmentsLazy.Value;

	/// <summary>
	///     Gets embeds attached to this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordEmbed> Embeds
		=> this._embedsLazy.Value;

	/// <summary>
	///     Gets reactions used on this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordReaction> Reactions
		=> this._reactionsLazy.Value;

	/// <summary>
	///     Gets the nonce sent with the message, if the message was sent by the client.
	/// </summary>
	[JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
	public string Nonce { get; internal set; }

	/// <summary>
	///     Gets whether the <see cref="Nonce" /> is enforced to be validated.
	/// </summary>
	[JsonProperty("enforce_nonce", NullValueHandling = NullValueHandling.Ignore)]
	public bool EnforceNonce { get; internal set; }

	/// <summary>
	///     Gets whether the message is pinned.
	/// </summary>
	[JsonProperty("pinned", NullValueHandling = NullValueHandling.Ignore)]
	public bool Pinned { get; internal set; }

	/// <summary>
	///     Gets the id of the webhook that generated this message.
	/// </summary>
	[JsonProperty("webhook_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? WebhookId { get; internal set; }

	/// <summary>
	///     Gets the type of the message.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public MessageType? MessageType { get; internal set; }

	/// <summary>
	///     Gets the message activity in the Rich Presence embed.
	/// </summary>
	[JsonProperty("activity", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageActivity Activity { get; internal set; }

	/// <summary>
	///     Gets the message application in the Rich Presence embed.
	/// </summary>
	[JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageApplication Application { get; internal set; }

	/// <summary>
	///     Gets the message application id in the Rich Presence embed.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	///     Gets the internal reference.
	/// </summary>
	[JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
	internal InternalDiscordMessageReference? InternalReference { get; set; }

	/// <summary>
	///     Gets the message reference.
	/// </summary>
	[JsonIgnore]
	public DiscordMessageReference? Reference
		=> this.InternalReference.HasValue ? this?.InternalBuildMessageReference() : null;

	/// <summary>
	///     Gets the message snapshots.
	/// </summary>
	[JsonProperty("message_snapshots", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordMessageSnapshot>? MessageSnapshots { get; internal set; }

	/// <summary>
	///     Gets whether this message has a message reference (reply, announcement, etc.).
	/// </summary>
	[Attributes.Experimental("We provide that temporary, as we figure out things."), JsonIgnore]
	public bool HasMessageReference
		=> this.InternalReference is { Type: ReferenceType.Default };

	/// <summary>
	///     Gets whether this message has forwarded messages.
	/// </summary>
	[Attributes.Experimental("We provide that temporary, as we figure out things."), JsonIgnore]
	public bool HasMessageSnapshots
		=> this.InternalReference is { Type: ReferenceType.Forward };

	/// <summary>
	///     Gets the bitwise flags for this message.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	/// <summary>
	///     Gets whether the message originated from a webhook.
	/// </summary>
	[JsonIgnore]
	public bool WebhookMessage
		=> this.WebhookId != null;

	/// <summary>
	///     Gets the jump link to this message.
	/// </summary>
	[JsonIgnore]
	public Uri JumpLink => this._jumpLink.Value;

	/// <summary>
	///     Gets stickers for this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordSticker> Stickers
		=> this._stickersLazy.Value;

	/// <summary>
	///     Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	/// <summary>
	///     Gets the guild to which this channel belongs.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.GuildId.HasValue && this.Discord.Guilds.TryGetValue(this.GuildId.Value, out var guild) ? guild : null;

	/// <summary>
	///     Gets the message object for the referenced message
	/// </summary>
	[JsonProperty("referenced_message", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessage ReferencedMessage { get; internal set; }

	/// <summary>
	///     Gets whether the message is a response to an interaction.
	/// </summary>
	[JsonProperty("interaction", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessageInteraction Interaction { get; internal set; }

	/// <summary>
	///     <para>Gets the thread that was started from this message.</para>
	///     <para>
	///         <note type="warning">
	///             If you're looking to get the actual thread channel this message was send in, call
	///             <see cref="Channel" /> and convert it to a <see cref="DiscordThreadChannel" />.
	///         </note>
	///     </para>
	/// </summary>
	[JsonIgnore]
	public DiscordThreadChannel Thread
		=> this._startedThread != null!
			? this._startedThread!
			: this.GuildId.HasValue && this.Guild.ThreadsInternal.TryGetValue(this.Id, out var thread)
				? thread!
				: null!;

	/// <summary>
	///     Gets the Discord snowflake objects resolved from this message's auto-populated select menus.
	/// </summary>
	[JsonProperty("resolved", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionResolvedCollection Resolved { get; internal set; }

	/// <summary>
	///     Gets the interaction metadata if the message is a response to an interaction.
	/// </summary>
	[JsonProperty("interaction_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionMetadata? InteractionMetadata { get; internal set; }

	/// <summary>
	///     Gets the poll of the message if one was attached.
	/// </summary>
	[JsonProperty("poll", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordPoll? Poll { get; internal set; }

	/// <summary>
	///     Gets whether this message has a poll.
	/// </summary>
	[JsonIgnore]
	public bool HasPoll
		=> this.Poll is not null;

	/// <summary>
	///     Checks whether this <see cref="DiscordMessage" /> is equal to another <see cref="DiscordMessage" />.
	/// </summary>
	/// <param name="e"><see cref="DiscordMessage" /> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordMessage" /> is equal to this <see cref="DiscordMessage" />.</returns>
	public bool Equals(DiscordMessage e)
		=> e is not null && (ReferenceEquals(this, e) || (this.Id == e.Id && this.ChannelId == e.ChannelId));

	/// <summary>
	///     <para>Ends the poll on this message.</para>
	///     <para>Works only for own polls and if they are not expired yet. </para>
	/// </summary>
	/// <returns>The fresh discord message.</returns>
	/// <exception cref="InvalidOperationException">
	///     Thrown when the message has no poll, the author is not us, or the poll has
	///     been already ended.
	/// </exception>
	public async Task<DiscordMessage> EndPollAsync()
		=> this.Poll is null
			? throw new InvalidOperationException("This message has no poll.")
			: this.Author.Id != this.Discord.CurrentUser.Id
				? throw new InvalidOperationException("Can only end own polls.")
				: this.Poll.Results?.IsFinalized ?? false
					? throw new InvalidOperationException("The poll was already ended.")
					: await this.Discord.ApiClient.EndPollAsync(this.ChannelId, this.Id);

	/// <summary>
	///     Forwards this message to another channel.
	/// </summary>
	/// <param name="targetChannel">The channel to forward this message to.</param>
	/// <param name="content">Content is not available at the moment, but already added for the future.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the bot tries forwarding a message it doesn't has access to or the
	///     client tried to modify a message not sent by them.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the target channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> ForwardMessageAsync(DiscordChannel targetChannel, string? content = null)
		=> await this.Discord.ApiClient.ForwardMessageAsync(this, targetChannel.Id, content);

	/// <summary>
	///     Build the message reference.
	/// </summary>
	internal DiscordMessageReference InternalBuildMessageReference()
	{
		var client = this.Discord as DiscordClient;
		ArgumentNullException.ThrowIfNull(this.InternalReference);
		var guildId = this.InternalReference.Value.GuildId;
		var channelId = this.InternalReference.Value.ChannelId;
		var messageId = this.InternalReference.Value.MessageId;
		var type = this.InternalReference.Value.Type;

		var reference = new DiscordMessageReference
		{
			GuildId = guildId
		};

		if (guildId.HasValue)
			reference.Guild = client.GuildsInternal.TryGetValue(guildId.Value, out var g)
				? g
				: new()
				{
					Id = guildId.Value,
					Discord = client
				};

		var channel = client.InternalGetCachedChannel(channelId.Value);
		reference.Type = type;

		if (channel == null)
		{
			reference.Channel = new()
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
			reference.Message = new()
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
	///     Gets the mentions.
	/// </summary>
	/// <returns>An array of IMentions.</returns>
	private List<IMention> GetMentions()
	{
		var mentions = new List<IMention>();

		try
		{
			if (this.ReferencedMessage is not null && this.MentionedUsersInternal.Count is not 0 && this.MentionedUsersInternal.Contains(this.ReferencedMessage.Author))
				mentions.Add(new RepliedUserMention());

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
	///     Populates the mentions.
	/// </summary>
	internal void PopulateMentions()
	{
		var guild = this.Channel?.Guild;
		this.MentionedUsersInternal ??= [];
		this.MentionedRolesInternal ??= [];
		this.MentionedChannelsInternal ??= [];

		var mentionedUsers = new HashSet<DiscordUser>(new DiscordUserComparer());
		if (guild != null)
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
			if (guild != null)
			{
				this.MentionedRolesInternal = [.. this.MentionedRolesInternal.Union(this.MentionedRoleIds.Select(guild.GetRole))!];
				this.MentionedChannelsInternal = [.. this.MentionedChannelsInternal.Union(Utilities.GetChannelMentions(this.Content).Select(guild.GetChannel))!];
			}

		this.MentionedUsersInternal = [.. mentionedUsers];
	}

	/// <summary>
	///     Edits the message.
	/// </summary>
	/// <param name="content">New content.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<string> content)
		=> this.Flags?.HasMessageFlag(MessageFlags.UIKit) ?? false ? throw new InvalidOperationException("UI Kit messages can not have content.") : this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, default, this.GetMentions(), default, default, [], default);

	/// <summary>
	///     Edits the message.
	/// </summary>
	/// <param name="embed">New embed.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<DiscordEmbed> embed = default)
		=> this.Flags?.HasMessageFlag(MessageFlags.UIKit) ?? false ? throw new InvalidOperationException("UI Kit messages can not have embeds.") : this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, embed.Map(v => new[] { v }).ValueOr([]), this.GetMentions(), default, default, [], default);

	/// <summary>
	///     Edits the message.
	/// </summary>
	/// <param name="content">New content.</param>
	/// <param name="embed">New embed.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<DiscordEmbed> embed = default)
		=> this.Flags?.HasMessageFlag(MessageFlags.UIKit) ?? false ? throw new InvalidOperationException("UI Kit messages can not have content or embeds.") : this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, embed.Map(v => new[] { v }).ValueOr([]), this.GetMentions(), default, default, [], default);

	/// <summary>
	///     Edits the message.
	/// </summary>
	/// <param name="content">New content.</param>
	/// <param name="embeds">New embeds.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifyAsync(Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds = default)
		=> this.Flags?.HasMessageFlag(MessageFlags.UIKit) ?? false ? throw new InvalidOperationException("UI Kit messages can not have content or embeds.") : this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, embeds, this.GetMentions(), default, default, [], default);

	/// <summary>
	///     Edits the message.
	/// </summary>
	/// <param name="builder">The builder of the message to edit.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> ModifyAsync(DiscordMessageBuilder builder)
	{
		builder.Validate(true);
		return await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, builder.Content, Optional.Some(builder.Embeds.AsEnumerable()), builder.Mentions, builder.Components, builder.Suppressed, builder.Files, builder.Attachments.Count > 0
			? Optional.Some(builder.Attachments.AsEnumerable())
			: builder.KeepAttachmentsInternal.HasValue
				? builder.KeepAttachmentsInternal.Value && this.Attachments is not null ? Optional.Some(this.Attachments.AsEnumerable()) : Array.Empty<DiscordAttachment>()
				: Optional.None).ConfigureAwait(false);
	}

	/// <summary>
	///     Edits the message embed suppression.
	/// </summary>
	/// <param name="suppress">Suppress embeds.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ModifySuppressionAsync(bool suppress = false)
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, default, default, default, suppress, default, default);

	/// <summary>
	///     Clears all attachments from the message.
	/// </summary>
	/// <returns></returns>
	public Task<DiscordMessage> ClearAttachmentsAsync()
		=> this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, default, default, this.GetMentions(), default, default, default, Array.Empty<DiscordAttachment>());

	/// <summary>
	///     Edits the message.
	/// </summary>
	/// <param name="action">The builder of the message to edit.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client tried to modify a message not sent by them.</exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> ModifyAsync(Action<DiscordMessageBuilder> action)
	{
		var builder = new DiscordMessageBuilder();
		action(builder);
		builder.Validate(true);
		return await this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, builder.Content, Optional.Some(builder.Embeds.AsEnumerable()), builder.Mentions, builder.Components, builder.Suppressed, builder.Files, builder.Attachments.Count > 0
			? Optional.Some(builder.Attachments.AsEnumerable())
			: builder.KeepAttachmentsInternal.HasValue
				? builder.KeepAttachmentsInternal.Value && this.Attachments is not null ? Optional.Some(this.Attachments.AsEnumerable()) : Array.Empty<DiscordAttachment>()
				: Optional.None).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes the message.
	/// </summary>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteMessageAsync(this.ChannelId, this.Id, reason);

	/// <summary>
	///     Creates a thread.
	///     Depending on the <see cref="ChannelType" /> of the parent channel it's either a
	///     <see cref="ChannelType.PublicThread" /> or a <see cref="ChannelType.NewsThread" />.
	/// </summary>
	/// <param name="name">The name of the thread.</param>
	/// <param name="autoArchiveDuration">
	///     <see cref="ThreadAutoArchiveDuration" /> till it gets archived. Defaults to
	///     <see cref="ThreadAutoArchiveDuration.OneHour" />
	/// </param>
	/// <param name="rateLimitPerUser">The per user ratelimit, aka slowdown.</param>
	/// <param name="reason">The reason.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.CreatePrivateThreads" /> or <see cref="Permissions.SendMessagesInThreads" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordThreadChannel> CreateThreadAsync(string name, ThreadAutoArchiveDuration autoArchiveDuration = ThreadAutoArchiveDuration.OneHour, int? rateLimitPerUser = null, string? reason = null)
		=> await this.Discord.ApiClient.CreateThreadAsync(this.ChannelId, this.Id, name, autoArchiveDuration, this.Channel.Type == ChannelType.News ? ChannelType.NewsThread : ChannelType.PublicThread, rateLimitPerUser, isForum: false, reason: reason).ConfigureAwait(false);

	/// <summary>
	///     Pins the message in its channel.
	/// </summary>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task PinAsync()
		=> this.Discord.ApiClient.PinMessageAsync(this.ChannelId, this.Id);

	/// <summary>
	///     Unpins the message in its channel.
	/// </summary>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnpinAsync()
		=> this.Discord.ApiClient.UnpinMessageAsync(this.ChannelId, this.Id);

	/// <summary>
	///     Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="content">Message content to respond with.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(string content)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, content, null, null, this.Id, false, false);

	/// <summary>
	///     Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(DiscordEmbed embed)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, null, embed != null
			? new[] { embed }
			: null, null, this.Id, false, false);

	/// <summary>
	///     Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="content">Message content to respond with.</param>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public
		Task<DiscordMessage> RespondAsync(string content, DiscordEmbed embed)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, content, embed != null
			? new[] { embed }
			: null, null, this.Id, false, false);

	/// <summary>
	///     Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="builder">The Discord message builder.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public
		Task<DiscordMessage> RespondAsync(DiscordMessageBuilder builder)
		=> this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, builder.WithReply(this.Id));

	/// <summary>
	///     Responds to the message. This produces a reply.
	/// </summary>
	/// <param name="action">The Discord message builder.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> RespondAsync(Action<DiscordMessageBuilder> action)
	{
		var builder = new DiscordMessageBuilder();
		action(builder);
		return this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, builder.WithReply(this.Id));
	}

	/// <summary>
	///     Creates a reaction to this message.
	/// </summary>
	/// <param name="emoji">The emoji you want to react with, either an emoji or name:id</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.AddReactions" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task CreateReactionAsync(DiscordEmoji emoji)
		=> this.Discord.ApiClient.CreateReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

	/// <summary>
	///     Deletes your own reaction
	/// </summary>
	/// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteOwnReactionAsync(DiscordEmoji emoji)
		=> this.Discord.ApiClient.DeleteOwnReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

	/// <summary>
	///     Deletes another user's reaction.
	/// </summary>
	/// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id.</param>
	/// <param name="user">Member you want to remove the reaction for</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteReactionAsync(DiscordEmoji emoji, DiscordUser user, string reason = null)
		=> this.Discord.ApiClient.DeleteUserReactionAsync(this.ChannelId, this.Id, user.Id, emoji.ToReactionString(), reason);

	/// <summary>
	///     Gets users that reacted with this emoji.
	/// </summary>
	/// <param name="emoji">Emoji to react with.</param>
	/// <param name="limit">Limit of users to fetch.</param>
	/// <param name="after">Fetch users after this user's id.</param>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(DiscordEmoji emoji, int limit = 25, ulong? after = null)
		=> this.GetReactionsInternalAsync(emoji, limit, after);

	/// <summary>
	///     Deletes all reactions for this message.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAllReactionsAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteAllReactionsAsync(this.ChannelId, this.Id, reason);

	/// <summary>
	///     Deletes all reactions of a specific reaction for this message.
	/// </summary>
	/// <param name="emoji">The emoji to clear, either an emoji or name:id.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteReactionsEmojiAsync(DiscordEmoji emoji)
		=> this.Discord.ApiClient.DeleteReactionsEmojiAsync(this.ChannelId, this.Id, emoji.ToReactionString());

	/// <summary>
	///     Gets the reactions.
	/// </summary>
	/// <param name="emoji">The emoji to search for.</param>
	/// <param name="limit">The limit of results.</param>
	/// <param name="after">Get the reasctions after snowflake.</param>
	private async Task<IReadOnlyList<DiscordUser>> GetReactionsInternalAsync(DiscordEmoji emoji, int limit = 25, ulong? after = null)
	{
		if (limit < 0)
			throw new ArgumentException("Cannot get a negative number of reactions' users.");

		if (limit == 0)
			return [];

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
		}
		while (remaining > 0 && lastCount > 0);

		return new ReadOnlyCollection<DiscordUser>(users);
	}

	/// <summary>
	///     Returns a string representation of this message.
	/// </summary>
	/// <returns>String representation of this message.</returns>
	public override string ToString()
		=> $"Message {this.Id}; Attachment count: {this.AttachmentsInternal.Count}; Embed count: {this.EmbedsInternal.Count}; Contents: {this.Content}";

	/// <summary>
	///     Checks whether this <see cref="DiscordMessage" /> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordMessage" />.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordMessage);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordMessage" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordMessage" />.</returns>
	public override int GetHashCode()
	{
		var hash = 13;

		hash = (hash * 7) + this.Id.GetHashCode();
		hash = (hash * 7) + this.ChannelId.GetHashCode();

		return hash;
	}

	/// <summary>
	///     Gets whether the two <see cref="DiscordMessage" /> objects are equal.
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
	///     Gets whether the two <see cref="DiscordMessage" /> objects are not equal.
	/// </summary>
	/// <param name="e1">First message to compare.</param>
	/// <param name="e2">Second message to compare.</param>
	/// <returns>Whether the two messages are not equal.</returns>
	public static bool operator !=(DiscordMessage e1, DiscordMessage e2)
		=> !(e1 == e2);
}
