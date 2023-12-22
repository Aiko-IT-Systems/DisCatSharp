using System;
using System.Collections.Generic;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
/// Represents a context for an autocomplete interaction.
/// </summary>
public sealed class AutocompleteContext
{
	/// <summary>
	/// The interaction created.
	/// </summary>
	public DiscordInteraction Interaction { get; internal set; }

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
	public DiscordChannel Channel { get; internal set; }

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
	/// Gets the slash command module this interaction was created in.
	/// </summary>
	public ApplicationCommandsExtension ApplicationCommandsExtension { get; internal set; }

	/// <summary>
	/// <para>Gets the service provider.</para>
	/// <para>This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider Services { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	/// The options already provided.
	/// </summary>
	public IReadOnlyList<DiscordInteractionDataOption> Options { get; internal init; }

	/// <summary>
	/// The option to autocomplete.
	/// </summary>
	public DiscordInteractionDataOption FocusedOption { get; internal set; }
}
