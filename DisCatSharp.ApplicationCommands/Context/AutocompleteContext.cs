using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using DisCatSharp.Entities;
using DisCatSharp.Entities.Core;
using DisCatSharp.Enums;
using DisCatSharp.Enums.Core;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
///     Represents a context for an autocomplete interaction.
/// </summary>
public sealed class AutocompleteContext : DisCatSharpCommandContext
{
	/// <summary>
	///     Initializes a new instance of the <see cref="AutocompleteContext" /> class.
	/// </summary>
	internal AutocompleteContext()
		: base(DisCatSharpCommandType.Autocomplete)
	{ }

	/// <summary>
	///     The interaction created.
	/// </summary>
	public DiscordInteraction Interaction { get; internal set; }

	/// <summary>
	///     Gets the client for this interaction.
	/// </summary>
	public DiscordClient Client { get; internal init; }

	/// <summary>
	///     Gets the guild this interaction was executed in.
	/// </summary>
	[NotNullIfNotNull(nameof(GuildId))]
	public DiscordGuild? Guild { get; internal init; }

	/// <summary>
	///     Gets the channel this interaction was executed in.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

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
	///     Gets the invoking user locale.
	/// </summary>
	public string Locale { get; internal set; }

	/// <summary>
	///     Gets the guild locale if applicable.
	/// </summary>
	[NotNullIfNotNull(nameof(Guild))]
	public string? GuildLocale { get; internal set; }

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
	///     Gets the slash command module this interaction was created in.
	/// </summary>
	public ApplicationCommandsExtension ApplicationCommandsExtension { get; internal set; }

	/// <summary>
	///     <para>Gets the service provider.</para>
	///     <para>This allows passing data around without resorting to static members.</para>
	///     <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider Services { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	///     The options already provided.
	/// </summary>
	public IReadOnlyList<DiscordInteractionDataOption> Options { get; internal init; }

	/// <summary>
	///     The option to autocomplete.
	/// </summary>
	public DiscordInteractionDataOption FocusedOption { get; internal set; }
}
