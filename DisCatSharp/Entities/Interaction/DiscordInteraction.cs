using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Enums.Core;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an interaction that was invoked.
/// </summary>
public sealed class DiscordInteraction : SnowflakeObject
{
	internal DiscordInteraction()
		: base(["member", "guild_id", "channel_id", "channel", "guild", "user", "entitlement_sku_ids"])
	{ }

	/// <summary>
	///     Gets the type of interaction invoked.
	/// </summary>
	[JsonProperty("type")]
	public InteractionType Type { get; internal set; }

	/// <summary>
	///     Gets the command data for this interaction.
	/// </summary>
	[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionData Data { get; internal set; }

	/// <summary>
	///     Gets the ID of the guild that invoked this interaction, if any.
	/// </summary>
	[JsonIgnore]
	public ulong? GuildId { get; internal set; }

	/// <summary>
	///     Gets the guild that invoked this interaction.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild? Guild
		=> (this.Discord as DiscordClient)?.InternalGetCachedGuild(this.GuildId) ?? this.PartialGuild;

	/// <summary>
	///     Gets the partial guild from the interaction.
	/// </summary>
	[JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
	internal DiscordGuild? PartialGuild { get; set; }

	/// <summary>
	///     Gets the ID of the channel that invoked this interaction.
	/// </summary>
	[JsonIgnore]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	///     Gets the channel that invoked this interaction.
	/// </summary>
	[JsonIgnore] // TODO: Is now also partial "channel"
	public DiscordChannel Channel
	{
		get
		{
			return this.Discord is DiscordClient dc
				? dc.InternalGetCachedChannel(this.ChannelId) ?? (DiscordChannel)dc.InternalGetCachedThread(this.ChannelId) ?? (this.Guild is null
					? new DiscordDmChannel
					{
						Id = this.ChannelId,
						Type = ChannelType.Private,
						Discord = this.Discord
					}
					: new DiscordChannel
					{
						Id = this.ChannelId,
						Discord = this.Discord
					})
				: this.Guild is null
				? new DiscordDmChannel
				{
					Id = this.ChannelId,
					Type = ChannelType.Private,
					Discord = this.Discord
				}
				: new DiscordChannel
				{
					Id = this.ChannelId,
					Discord = this.Discord
				};
		}
	}

	/// <summary>
	///     Gets the user that invoked this interaction.
	///     <para>This can be cast to a <see cref="DisCatSharp.Entities.DiscordMember" /> if created in a guild.</para>
	/// </summary>
	[JsonIgnore]
	public DiscordUser User { get; internal set; }

	/// <summary>
	///     Gets the continuation token for responding to this interaction.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; internal set; }

	/// <summary>
	///     Gets the version number for this interaction type.
	/// </summary>
	[JsonProperty("version")]
	public int Version { get; internal set; }

	/// <summary>
	///     Gets the ID of the application that created this interaction.
	/// </summary>
	[JsonProperty("application_id")]
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	///     The message this interaction was created with, if any.
	/// </summary>
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	internal DiscordMessage? Message { get; set; }

	/// <summary>
	///     Gets the invoking user locale.
	/// </summary>
	[JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
	public string Locale { get; internal set; }

	/// <summary>
	///     Gets the guild locale if applicable.
	/// </summary>
	[JsonProperty("guild_locale", NullValueHandling = NullValueHandling.Ignore)]
	public string GuildLocale { get; internal set; }

	/// <summary>
	///     Gets the attachment size limit in bytes.
	/// </summary>
	[JsonProperty("attachment_size_limit", NullValueHandling = NullValueHandling.Ignore)]
	public int AttachmentSizeLimit { get; internal set; }

	/// <summary>
	///     Gets the applications permissions.
	/// </summary>
	[JsonProperty("app_permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions AppPermissions { get; internal set; }

	/// <summary>
	///     <para>Gets the entitlements.</para>
	/// </summary>
	[JsonProperty("entitlements", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordEntitlement> Entitlements { get; internal set; } = [];

	/// <summary>
	///     Gets which integrations authorized the interaction.
	/// </summary>
	[JsonProperty("authorizing_integration_owners", NullValueHandling = NullValueHandling.Ignore)]
	public AuthorizingIntegrationOwners? AuthorizingIntegrationOwners { get; internal set; }

	/// <summary>
	///     Gets the interaction's calling context.
	/// </summary>
	[JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionContextType Context { get; internal set; }

	/// <summary>
	///     Creates a response to this interaction.
	/// </summary>
	/// <param name="type">The type of the response.</param>
	/// <param name="builder">The data, if any, to send.</param>
	/// <param name="modifyMode">The modify mode. Only useful for <see cref="InteractionResponseType.UpdateMessage"/>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>
	///     The created <see cref="DiscordMessage" />, or <see langword="null" /> if <paramref name="type" /> creates no
	///     content.
	/// </returns>
	public async Task<DiscordInteractionCallbackResponse> CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder? builder = null, ModifyMode modifyMode = ModifyMode.Update, CancellationToken cancellationToken = default)
	{
		if (modifyMode is ModifyMode.Replace)
			builder.DoConditionalReplace();
		return await this.Discord.ApiClient.CreateInteractionResponseAsync(this.Id, this.Token, type, builder, cancellationToken: cancellationToken);
	}

	/// <summary>
	///     Creates a modal response to this interaction.
	/// </summary>
	/// <param name="builder">The data to send.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	public Task CreateInteractionModalResponseAsync(DiscordInteractionModalBuilder builder, CancellationToken cancellationToken = default)
		=> this.Type is not InteractionType.Ping && this.Type is not InteractionType.ModalSubmit ? this.Discord.ApiClient.CreateInteractionModalResponseAsync(this.Id, this.Token, InteractionResponseType.Modal, builder, cancellationToken: cancellationToken) : throw new NotSupportedException("You can't respond to a PING with a modal.");

	/// <summary>
	///     Creates an iframe response to this interaction.
	/// </summary>
	/// <param name="customId">The custom id of the iframe.</param>
	/// <param name="title">The title of the iframe.</param>
	/// <param name="modalSize">The size of the iframe.</param>
	/// <param name="iFramePath">The path of the iframe. Uses %application_id%.discordsays.com/<c>:iframe_path</c>.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	public Task CreateInteractionIframeResponseAsync(string customId, string title, IframeModalSize modalSize = IframeModalSize.Normal, string? iFramePath = null, CancellationToken cancellationToken = default)
		=> this.Type is not InteractionType.Ping ? this.Discord.ApiClient.CreateInteractionIframeResponseAsync(this.Id, this.Token, InteractionResponseType.Iframe, customId, title, modalSize, iFramePath, cancellationToken: cancellationToken) : throw new NotSupportedException("You can't respond to a PING with an iframe.");

	/// <summary>
	///     Gets the original interaction response.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The original message that was sent. This <b>does not work on ephemeral messages.</b></returns>
	public Task<DiscordMessage> GetOriginalResponseAsync(CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.GetOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token, cancellationToken: cancellationToken);

	/// <summary>
	///     Edits the original interaction response.
	/// </summary>
	/// <param name="builder">The webhook builder.</param>
	/// <param name="modifyMode">The modify mode.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The edited <see cref="DiscordMessage" />.</returns>
	public async Task<DiscordMessage> EditOriginalResponseAsync(DiscordWebhookBuilder builder, ModifyMode modifyMode = ModifyMode.Update, CancellationToken cancellationToken = default)
	{
		if (modifyMode is ModifyMode.Replace)
			builder.DoConditionalReplace();
		// TODO: This might not work if i.e. the original response was deferred and edited.
		builder.MentionsInternal ??= this.Message?.GetMentions();
		builder.Validate(isInteractionResponse: true);
		if (builder.KeepAttachmentsInternal.HasValue && builder.KeepAttachmentsInternal.Value)
		{
			var originalResponse = await this.Discord.ApiClient.GetOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token, cancellationToken: cancellationToken).ConfigureAwait(false);
			var attachments = originalResponse.Attachments;
			if (attachments?.Count > 0)
			{
				builder.AttachmentsInternal ??= [];
				builder.AttachmentsInternal.AddRange(attachments);
			}
		}
		else if (builder.KeepAttachmentsInternal.HasValue)
			builder.AttachmentsInternal?.Clear();

		return await this.Discord.ApiClient.EditOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token, builder, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes the original interaction response.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// >
	public Task DeleteOriginalResponseAsync(CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.DeleteOriginalInteractionResponseAsync(this.Discord.CurrentApplication.Id, this.Token, cancellationToken: cancellationToken);

	/// <summary>
	///     Creates a follow-up message to this interaction.
	/// </summary>
	/// <param name="builder">The webhook builder.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The created <see cref="DiscordMessage" />.</returns>
	public async Task<DiscordMessage> CreateFollowupMessageAsync(DiscordFollowupMessageBuilder builder, CancellationToken cancellationToken = default)
	{
		builder.Validate();

		return await this.Discord.ApiClient.CreateFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, builder, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets a follow-up message.
	/// </summary>
	/// <param name="messageId">The id of the follow-up message.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	public Task<DiscordMessage> GetFollowupMessageAsync(ulong messageId, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.GetFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId, cancellationToken: cancellationToken);

	/// <summary>
	///     Edits a follow-up message.
	/// </summary>
	/// <param name="messageId">The id of the follow-up message.</param>
	/// <param name="builder">The webhook builder.</param>
	/// <param name="modifyMode">The modify mode.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The edited <see cref="DiscordMessage" />.</returns>
	public async Task<DiscordMessage> EditFollowupMessageAsync(ulong messageId, DiscordWebhookBuilder builder, ModifyMode modifyMode = ModifyMode.Update, CancellationToken cancellationToken = default)
	{
		if (modifyMode is ModifyMode.Replace)
			builder.DoConditionalReplace();
		builder.MentionsInternal ??= this.GetFollowupMessageAsync(messageId, cancellationToken).Result.GetMentions();
		builder.Validate(isFollowup: true);
		if (builder.KeepAttachmentsInternal.HasValue && builder.KeepAttachmentsInternal.Value)
		{
			var attachments = this.Discord.ApiClient.GetFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId, cancellationToken: cancellationToken).Result.Attachments;
			if (attachments?.Count > 0)
			{
				builder.AttachmentsInternal ??= [];
				builder.AttachmentsInternal.AddRange(attachments);
			}
		}
		else if (builder.KeepAttachmentsInternal.HasValue)
			builder.AttachmentsInternal?.Clear();

		return await this.Discord.ApiClient.EditFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId, builder, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes a follow-up message.
	/// </summary>
	/// <param name="messageId">The id of the follow-up message.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	public Task DeleteFollowupMessageAsync(ulong messageId, CancellationToken cancellationToken = default)
		=> this.Discord.ApiClient.DeleteFollowupMessageAsync(this.Discord.CurrentApplication.Id, this.Token, messageId, cancellationToken: cancellationToken);
}
