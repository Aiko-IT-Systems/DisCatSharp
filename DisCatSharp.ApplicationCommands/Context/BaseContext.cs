using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
/// Represents a base context for application command contexts.
/// </summary>
public class BaseContext
{
	/// <summary>
	/// Gets the interaction that was created.
	/// </summary>
	public DiscordInteraction Interaction { get; internal init; }

	/// <summary>
	/// Gets the client for this interaction.
	/// </summary>
	public DiscordClient Client { get; internal init; }

	/// <summary>
	/// Gets the guild this interaction was executed in.
	/// </summary>
	public DiscordGuild Guild { get; internal init; }

	/// <summary>
	/// Gets the channel this interaction was executed in.
	/// </summary>
	public DiscordChannel Channel { get; internal init; }

	/// <summary>
	/// Gets the user which executed this interaction.
	/// </summary>
	public DiscordUser User { get; internal init; }

	/// <summary>
	/// Gets the member which executed this interaction, or null if the command is in a DM.
	/// </summary>
	public DiscordMember Member
		=> this.User is DiscordMember member ? member : null;

	/// <summary>
	/// Gets the application command module this interaction was created in.
	/// </summary>
	public ApplicationCommandsExtension ApplicationCommandsExtension { get; internal set; }

	/// <summary>
	/// Gets the token for this interaction.
	/// </summary>
	public string Token { get; internal set; }

	/// <summary>
	/// Gets the id for this interaction.
	/// </summary>
	public ulong InteractionId { get; internal set; }

	/// <summary>
	/// Gets the name of the command.
	/// </summary>
	public string CommandName { get; internal init; }

	/// <summary>
	/// Gets the name of the sub command.
	/// </summary>
	public string? SubCommandName { get; internal set; }

	/// <summary>
	/// Gets the name of the sub command.
	/// </summary>
	public string? SubSubCommandName { get; internal set; }

	/// <summary>
	/// Gets the full command string, including the subcommand.
	/// </summary>
	public string FullCommandName
		=> $"{this.CommandName}{(string.IsNullOrWhiteSpace(this.SubCommandName) ? "" : $" {this.SubCommandName}")}{(string.IsNullOrWhiteSpace(this.SubSubCommandName) ? "" : $" {this.SubSubCommandName}")}";

	/// <summary>
	/// Gets the invoking user locale.
	/// </summary>
	public string Locale { get; internal set; }

	/// <summary>
	/// Gets the guild locale if applicable.
	/// </summary>
	public string GuildLocale { get; internal set; }

	/// <summary>
	/// Gets the applications permissions.
	/// </summary>
	public Permissions AppPermissions { get; internal set; }

	/// <summary>
	/// <para>Gets the entitlements.</para>
	/// <para>This is related to premium subscriptions for bots.</para>
	/// <para><note type="warning">Can only be used if you have an associated application subscription sku.</note></para>
	/// </summary>
	[DiscordInExperiment("Currently in closed beta."), Experimental("We provide this type but can't provide support.")]
	public List<DiscordEntitlement> Entitlements { get; internal set; } = [];

	/// <summary>
	/// <para>Gets the entitlement sku ids.</para>
	/// <para>This is related to premium subscriptions for bots.</para>
	/// <para><note type="warning">Can only be used if you have an associated application subscription sku.</note></para>
	/// </summary>
	[DiscordInExperiment("Currently in closed beta."), Experimental("We provide this type but can't provide support.")]
	public List<ulong> EntitlementSkuIds { get; internal set; } = [];

	/// <summary>
	/// Gets the type of this interaction.
	/// </summary>
	public ApplicationCommandType Type { get; internal set; }

	/// <summary>
	/// <para>Gets the service provider.</para>
	/// <para>This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider Services { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	/// Creates a response to this interaction.
	/// <para>You must create a response within 3 seconds of this interaction being executed; if the command has the potential to take more than 3 seconds, create a <see cref="InteractionResponseType.DeferredChannelMessageWithSource"/> at the start, and edit the response later.</para>
	/// </summary>
	/// <param name="type">The type of the response.</param>
	/// <param name="builder">The data to be sent, if any.</param>
	/// <returns></returns>
	public Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder builder = null)
		=> this.Interaction.CreateResponseAsync(type, builder);

	/// <summary>
	/// Creates a modal response to this interaction.
	/// </summary>
	/// <param name="builder">The data to send.</param>
	public Task CreateModalResponseAsync(DiscordInteractionModalBuilder builder)
		=> this.Interaction.Type != InteractionType.Ping && this.Interaction.Type != InteractionType.ModalSubmit ? this.Interaction.CreateInteractionModalResponseAsync(builder) : throw new NotSupportedException("You can't respond to a PING with a modal.");

	/// <summary>
	/// Creates an iframe response to this interaction.
	/// </summary>
	/// <param name="customId">The custom id of the iframe.</param>
	/// <param name="title">The title of the iframe.</param>
	/// <param name="modalSize">The size of the iframe.</param>
	/// <param name="iFramePath">The path of the iframe.</param>
	public Task CreateInteractionIframeResponseAsync(string customId, string title, IframeModalSize modalSize = IframeModalSize.Normal, string? iFramePath = null)
		=> this.Interaction.Type != InteractionType.Ping ? this.Interaction.CreateInteractionIframeResponseAsync(customId, title, modalSize, iFramePath) : throw new NotSupportedException("You can't respond to a PING with an iframe.");

	/// <summary>
	/// Edits the interaction response.
	/// </summary>
	/// <param name="builder">The data to edit the response with.</param>
	/// <returns></returns>
	public Task<DiscordMessage> EditResponseAsync(DiscordWebhookBuilder builder)
		=> this.Interaction.EditOriginalResponseAsync(builder);

	/// <summary>
	/// Deletes the interaction response.
	/// </summary>
	/// <returns></returns>
	public Task DeleteResponseAsync()
		=> this.Interaction.DeleteOriginalResponseAsync();

	/// <summary>
	/// Creates a follow up message to the interaction.
	/// </summary>
	/// <param name="builder">The message to be sent, in the form of a webhook.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> FollowUpAsync(DiscordFollowupMessageBuilder builder)
		=> this.Interaction.CreateFollowupMessageAsync(builder);

	/// <summary>
	/// Creates a follow up message to the interaction.
	/// </summary>
	/// <param name="content">The content of the message to be sent.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> FollowUpAsync(string content)
		=> this.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(content));

	/// <summary>
	/// Edits a followup message.
	/// </summary>
	/// <param name="followupMessageId">The id of the followup message to edit.</param>
	/// <param name="builder">The webhook builder.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> EditFollowupAsync(ulong followupMessageId, DiscordWebhookBuilder builder)
		=> this.Interaction.EditFollowupMessageAsync(followupMessageId, builder);

	/// <summary>
	/// Edits a followup message.
	/// </summary>
	/// <param name="followupMessageId">The id of the followup message to edit.</param>
	/// <param name="content">The content of the webhook.</param>
	/// <returns>The created message.</returns>
	public Task<DiscordMessage> EditFollowupAsync(ulong followupMessageId, string content)
		=> this.EditFollowupAsync(followupMessageId, new DiscordWebhookBuilder().WithContent(content));

	/// <summary>
	/// Deletes a followup message.
	/// </summary>
	/// <param name="followupMessageId">The id of the followup message to delete.</param>
	/// <returns></returns>
	public Task DeleteFollowupAsync(ulong followupMessageId)
		=> this.Interaction.DeleteFollowupMessageAsync(followupMessageId);

	/// <summary>
	/// Gets the followup message.
	/// </summary>
	/// <param name="followupMessageId">The followup message id.</param>
	public Task<DiscordMessage> GetFollowupMessageAsync(ulong followupMessageId)
		=> this.Interaction.GetFollowupMessageAsync(followupMessageId);

	/// <summary>
	/// Gets the original interaction response.
	/// </summary>
	/// <returns>The original interaction response.</returns>
	public Task<DiscordMessage> GetOriginalResponseAsync()
		=> this.Interaction.GetOriginalResponseAsync();
}
