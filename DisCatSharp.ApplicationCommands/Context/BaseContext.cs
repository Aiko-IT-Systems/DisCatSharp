using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.Core;
using DisCatSharp.Enums;
using DisCatSharp.Enums.Core;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
///     Represents a base context for application command contexts.
/// </summary>
public class BaseContext : DisCatSharpCommandContext
{
	/// <summary>
	///     Initializes a new instance of the <see cref="BaseContext" /> class.
	/// </summary>
	/// <param name="type">The command type.</param>
	internal BaseContext(DisCatSharpCommandType type)
		: base(type)
	{ }

	/// <summary>
	///     Gets the interaction that was created.
	/// </summary>
	public DiscordInteraction Interaction { get; internal init; }

	/// <summary>
	///     Gets the guild this interaction was executed in.
	/// </summary>
	[NotNullIfNotNull(nameof(GuildId))]
	public DiscordGuild? Guild { get; internal init; }

	/// <summary>
	///     Gets the channel this interaction was executed in.
	/// </summary>
	public DiscordChannel Channel { get; internal init; }

	/// <summary>
	///     Gets the user which executed this interaction.
	/// </summary>
	public DiscordUser User { get; internal init; }

	/// <summary>
	///     Gets the member which executed this interaction, or null if the command is in a DM.
	/// </summary>
	[NotNullIfNotNull(nameof(Guild))]
	public DiscordMember? Member
		=> this.User is DiscordMember member ? member : null;

	/// <summary>
	///     Gets the application command module this interaction was created in.
	/// </summary>
	public ApplicationCommandsExtension ApplicationCommandsExtension { get; internal set; }

	/// <summary>
	///     Gets the token for this interaction.
	/// </summary>
	public string Token { get; internal set; }

	/// <summary>
	///     Gets the id for this interaction.
	/// </summary>
	public ulong InteractionId { get; internal set; }

	/// <summary>
	///     Gets the full command string, including the subcommand.
	/// </summary>
	public override string FullCommandName
		=> $"{this.CommandName}{(string.IsNullOrWhiteSpace(this.SubCommandName) ? "" : $" {this.SubCommandName}")}{(string.IsNullOrWhiteSpace(this.SubSubCommandName) ? "" : $" {this.SubSubCommandName}")}";

	/// <summary>
	///     Gets the invoking user locale.
	/// </summary>
	public string Locale { get; internal set; }

	/// <summary>
	///     Gets the guild locale if applicable.
	/// </summary>
	[NotNullIfNotNull(nameof(Guild))]
	public string? GuildLocale { get; internal set; }

	/// <summary>
	///     Gets the attachment size limit in bytes.
	/// </summary>
	public int AttachmentSizeLimit { get; internal set; }

	/// <summary>
	///     Gets the applications permissions.
	/// </summary>
	public Permissions AppPermissions { get; internal set; }

	/// <summary>
	///     <para>Gets the entitlements.</para>
	///     <para>This is related to premium subscriptions for bots.</para>
	///     <para>
	///         <note type="warning">Can only be used if you have an associated application subscription sku.</note>
	///     </para>
	/// </summary>
	public List<DiscordEntitlement> Entitlements { get; internal set; } = [];

	/// <summary>
	///     Gets the type of this interaction.
	/// </summary>
	public ApplicationCommandType Type { get; internal set; }

	/// <summary>
	///     <para>Gets the service provider.</para>
	///     <para>This allows passing data around without resorting to static members.</para>
	///     <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider Services { get; internal set; } = EmptyServiceProvider.Instance;

	/// <summary>
	///     Gets or sets the service scope for this command execution.
	///     Used internally for proper disposal of scoped services.
	/// </summary>
	internal IServiceScope? ServiceScope { get; set; }

	/// <summary>
	///     Creates a response to this interaction.
	///     <para>
	///         You must create a response within 3 seconds of this interaction being executed; if the command has the
	///         potential to take more than 3 seconds, create a
	///         <see cref="InteractionResponseType.DeferredChannelMessageWithSource" /> at the start, and edit the response
	///         later.
	///     </para>
	/// </summary>
	/// <param name="type">The type of the response.</param>
	/// <param name="builder">The data to be sent, if any.</param>
	/// <param name="modifyMode">The modify mode. Only useful for <see cref="InteractionResponseType.UpdateMessage"/>.</param>
	/// <returns>
	///     The created <see cref="DiscordMessage" />, or <see langword="null" /> if <paramref name="type" /> creates no
	///     content.
	/// </returns>
	public async Task<DiscordInteractionCallbackResponse> CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder? builder = null, ModifyMode modifyMode = ModifyMode.Update, CancellationToken cancellationToken = default)
		=> await this.Interaction.CreateResponseAsync(type, builder, modifyMode, cancellationToken);

	/// <summary>
	///     Creates a modal response to this interaction.
	/// </summary>
	/// <param name="builder">The data to send.</param>
	public Task CreateModalResponseAsync(DiscordInteractionModalBuilder builder, CancellationToken cancellationToken = default)
		=> this.Interaction.Type is not InteractionType.Ping && this.Interaction.Type is not InteractionType.ModalSubmit ? this.Interaction.CreateInteractionModalResponseAsync(builder, cancellationToken) : throw new NotSupportedException("You can't respond to a PING with a modal.");

	/// <summary>
	///     Creates an iframe response to this interaction.
	/// </summary>
	/// <param name="customId">The custom id of the iframe.</param>
	/// <param name="title">The title of the iframe.</param>
	/// <param name="modalSize">The size of the iframe.</param>
	/// <param name="iFramePath">The path of the iframe.</param>
	public Task CreateInteractionIframeResponseAsync(string customId, string title, IframeModalSize modalSize = IframeModalSize.Normal, string? iFramePath = null, CancellationToken cancellationToken = default)
		=> this.Interaction.Type is not InteractionType.Ping ? this.Interaction.CreateInteractionIframeResponseAsync(customId, title, modalSize, iFramePath, cancellationToken) : throw new NotSupportedException("You can't respond to a PING with an iframe.");

	/// <summary>
	///     Edits the interaction response.
	/// </summary>
	/// <param name="builder">The data to edit the response with.</param>
	/// <param name="modifyMode">The modify mode.</param>
	/// <returns></returns>
	public Task<DiscordMessage> EditResponseAsync(DiscordWebhookBuilder builder, ModifyMode modifyMode = ModifyMode.Update, CancellationToken cancellationToken = default)
		=> this.Interaction.EditOriginalResponseAsync(builder, modifyMode, cancellationToken);

	/// <summary>
	///     Edits the interaction response.
	/// </summary>
	/// <param name="content">The content to edit the response with.</param>
	/// <returns></returns>
	public Task<DiscordMessage> EditResponseAsync(string content, CancellationToken cancellationToken = default)
		=> this.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(content), cancellationToken: cancellationToken);

	/// <summary>
	///     Deletes the interaction response.
	/// </summary>
	/// <returns></returns>
	public Task DeleteResponseAsync(CancellationToken cancellationToken = default)
		=> this.Interaction.DeleteOriginalResponseAsync(cancellationToken);

	/// <summary>
	///     Creates a follow up message to the interaction.
	/// </summary>
	/// <param name="builder">The message to be sent, in the form of a webhook.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> FollowUpAsync(DiscordFollowupMessageBuilder builder, CancellationToken cancellationToken = default)
		=> this.Interaction.CreateFollowupMessageAsync(builder, cancellationToken);

	/// <summary>
	///     Creates a follow up message to the interaction.
	/// </summary>
	/// <param name="content">The content of the message to be sent.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> FollowUpAsync(string content, CancellationToken cancellationToken = default)
		=> this.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(content), cancellationToken);

	/// <summary>
	///     Edits a followup message.
	/// </summary>
	/// <param name="followupMessageId">The id of the followup message to edit.</param>
	/// <param name="builder">The webhook builder.</param>
	/// <param name="modifyMode">The modify mode.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> EditFollowupAsync(ulong followupMessageId, DiscordWebhookBuilder builder, ModifyMode modifyMode = ModifyMode.Update, CancellationToken cancellationToken = default)
		=> this.Interaction.EditFollowupMessageAsync(followupMessageId, builder, modifyMode, cancellationToken);

	/// <summary>
	///     Edits a followup message.
	/// </summary>
	/// <param name="followupMessageId">The id of the followup message to edit.</param>
	/// <param name="content">The content of the webhook.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> EditFollowupAsync(ulong followupMessageId, string content, CancellationToken cancellationToken = default)
		=> this.EditFollowupAsync(followupMessageId, new DiscordWebhookBuilder().WithContent(content), cancellationToken: cancellationToken);

	/// <summary>
	///     Deletes a followup message.
	/// </summary>
	/// <param name="followupMessageId">The id of the followup message to delete.</param>
	/// <returns></returns>
	public Task DeleteFollowupAsync(ulong followupMessageId, CancellationToken cancellationToken = default)
		=> this.Interaction.DeleteFollowupMessageAsync(followupMessageId, cancellationToken);

	/// <summary>
	///     Gets the followup message.
	/// </summary>
	/// <param name="followupMessageId">The followup message id.</param>
	public Task<DiscordMessage> GetFollowupMessageAsync(ulong followupMessageId, CancellationToken cancellationToken = default)
		=> this.Interaction.GetFollowupMessageAsync(followupMessageId, cancellationToken);

	/// <summary>
	///     Gets the original interaction response.
	/// </summary>
	/// <returns>The original interaction response.</returns>
	public Task<DiscordMessage> GetOriginalResponseAsync(CancellationToken cancellationToken = default)
		=> this.Interaction.GetOriginalResponseAsync(cancellationToken);
}
