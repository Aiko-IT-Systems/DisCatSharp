using System;
using System.IO;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents information about a Discord webhook.
/// </summary>
public class DiscordWebhook : SnowflakeObject, IEquatable<DiscordWebhook>
{
	/// <summary>
	/// Gets the api client.
	/// </summary>
	internal DiscordApiClient ApiClient { get; set; }

	/// <summary>
	/// Gets the id of the guild this webhook belongs to.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the ID of the channel this webhook belongs to.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the ID of the application this webhook belongs to, if applicable.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	/// Gets the user this webhook was created by.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the default name of this webhook.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets hash of the default avatar for this webhook.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	internal string AvatarHash { get; set; }

	/// <summary>
	/// Gets the partial source guild for this webhook (For Channel Follower Webhooks).
	/// </summary>
	[JsonProperty("source_guild", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordGuild SourceGuild { get; set; }

	/// <summary>
	/// Gets the partial source channel for this webhook (For Channel Follower Webhooks).
	/// </summary>
	[JsonProperty("source_channel", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordChannel SourceChannel { get; set; }

	/// <summary>
	/// Gets the url used for executing the webhook.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string Url { get; set; }

	/// <summary>
	/// Gets the default avatar url for this webhook.
	/// </summary>
	public string AvatarUrl
		=> !string.IsNullOrWhiteSpace(this.AvatarHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Id}/{this.AvatarHash}.png?size=1024" : null;

	/// <summary>
	/// Gets the secure token of this webhook.
	/// </summary>
	[JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
	public string Token { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordWebhook"/> class.
	/// </summary>
	internal DiscordWebhook()
		: base(["type"])
	{ }

	/// <summary>
	/// Modifies this webhook.
	/// </summary>
	/// <param name="name">New default name for this webhook.</param>
	/// <param name="avatar">New avatar for this webhook.</param>
	/// <param name="channelId">The new channel id to move the webhook to.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The modified webhook.</returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordWebhook> ModifyAsync(string name = null, Optional<Stream> avatar = default, ulong? channelId = null, string reason = null)
	{
		var avatarb64 = ImageTool.Base64FromStream(avatar);

		var newChannelId = channelId ?? this.ChannelId;

		return this.Discord.ApiClient.ModifyWebhookAsync(this.Id, newChannelId, name, avatarb64, reason);
	}

	/// <summary>
	/// Gets a previously-sent webhook message.
	/// </summary>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> GetMessageAsync(ulong messageId)
		=> (this.Discord?.ApiClient ?? this.ApiClient).GetWebhookMessageAsync(this.Id, this.Token, messageId);

	/// <summary>
	/// Tries to get a previously-sent webhook message.
	/// </summary>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage?> TryGetMessageAsync(ulong messageId)
	{
		try
		{
			return await this.GetMessageAsync(messageId).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	/// Gets a previously-sent webhook message.
	/// </summary>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> GetMessageAsync(ulong messageId, ulong threadId)
		=> (this.Discord?.ApiClient ?? this.ApiClient).GetWebhookMessageAsync(this.Id, this.Token, messageId, threadId);

	/// <summary>
	/// Tries to get a previously-sent webhook message.
	/// </summary>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> TryGetMessageAsync(ulong messageId, ulong threadId)
	{
		try
		{
			return await this.GetMessageAsync(messageId, threadId).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	/// Permanently deletes this webhook.
	/// </summary>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync()
		=> (this.Discord?.ApiClient ?? this.ApiClient).DeleteWebhookAsync(this.Id, this.Token);

	/// <summary>
	/// Executes this webhook with the given <see cref="DiscordWebhookBuilder"/>.
	/// </summary>
	/// <param name="builder">Webhook builder filled with data to send.</param>
	/// <param name="threadId">Target thread id (Optional). Defaults to null.</param>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> ExecuteAsync(DiscordWebhookBuilder builder, string threadId = null)
		=> (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookAsync(this.Id, this.Token, builder, threadId);

	/// <summary>
	/// Executes this webhook in Slack compatibility mode.
	/// </summary>
	/// <param name="json">JSON containing Slack-compatible payload for this webhook.</param>
	/// <param name="threadId">Target thread id (Optional). Defaults to null.</param>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ExecuteSlackAsync(string json, string threadId = null)
		=> (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookSlackAsync(this.Id, this.Token, json, threadId);

	/// <summary>
	/// Executes this webhook in GitHub compatibility mode.
	/// </summary>
	/// <param name="json">JSON containing GitHub-compatible payload for this webhook.</param>
	/// <param name="threadId">Target thread id (Optional). Defaults to null.</param>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ExecuteGithubAsync(string json, string threadId = null)
		=> (this.Discord?.ApiClient ?? this.ApiClient).ExecuteWebhookGithubAsync(this.Id, this.Token, json, threadId);

	/// <summary>
	/// Edits a previously-sent webhook message.
	/// </summary>
	/// <param name="messageId">The id of the message to edit.</param>
	/// <param name="builder">The builder of the message to edit.</param>
	/// <param name="threadId">Target thread id (Optional). Defaults to null.</param>
	/// <returns>The modified <see cref="DiscordMessage"/></returns>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> EditMessageAsync(ulong messageId, DiscordWebhookBuilder builder, string threadId = null)
	{
		builder.Validate(true);
		if (builder.KeepAttachmentsInternal.HasValue && builder.KeepAttachmentsInternal.Value)
			builder.AttachmentsInternal.AddRange(this.ApiClient.GetWebhookMessageAsync(this.Id, this.Token, messageId.ToString(), threadId).Result.Attachments);
		else if (builder.KeepAttachmentsInternal.HasValue)
			builder.AttachmentsInternal.Clear();
		return await (this.Discord?.ApiClient ?? this.ApiClient).EditWebhookMessageAsync(this.Id, this.Token, messageId.ToString(), builder, threadId).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes a message that was created by the webhook.
	/// </summary>
	/// <param name="messageId">The id of the message to delete</param>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteMessageAsync(ulong messageId)
		=> (this.Discord?.ApiClient ?? this.ApiClient).DeleteWebhookMessageAsync(this.Id, this.Token, messageId);

	/// <summary>
	/// Deletes a message that was created by the webhook.
	/// </summary>
	/// <param name="messageId">The id of the message to delete</param>
	/// <param name="threadId">Target thread id (Optional). Defaults to null.</param>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteMessageAsync(ulong messageId, ulong threadId)
		=> (this.Discord?.ApiClient ?? this.ApiClient).DeleteWebhookMessageAsync(this.Id, this.Token, messageId, threadId);

	/// <summary>
	/// Checks whether this <see cref="DiscordWebhook"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordWebhook"/>.</returns>
	public override bool Equals(object obj) => this.Equals(obj as DiscordWebhook);

	/// <summary>
	/// Checks whether this <see cref="DiscordWebhook"/> is equal to another <see cref="DiscordWebhook"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordWebhook"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordWebhook"/> is equal to this <see cref="DiscordWebhook"/>.</returns>
	public bool Equals(DiscordWebhook e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordWebhook"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordWebhook"/>.</returns>
	public override int GetHashCode() => this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordWebhook"/> objects are equal.
	/// </summary>
	/// <param name="e1">First webhook to compare.</param>
	/// <param name="e2">Second webhook to compare.</param>
	/// <returns>Whether the two webhooks are equal.</returns>
	public static bool operator ==(DiscordWebhook e1, DiscordWebhook e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordWebhook"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First webhook to compare.</param>
	/// <param name="e2">Second webhook to compare.</param>
	/// <returns>Whether the two webhooks are not equal.</returns>
	public static bool operator !=(DiscordWebhook e1, DiscordWebhook e2)
		=> !(e1 == e2);
}
