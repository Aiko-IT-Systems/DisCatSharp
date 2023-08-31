// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
public class AutocompleteContext
{
	/// <summary>
	/// The interaction created.
	/// </summary>
	public DiscordInteraction Interaction { get; internal set; }

	/// <summary>
	/// Gets the client for this interaction.
	/// </summary>
	public DiscordClient Client { get; internal set; }

	/// <summary>
	/// Gets the guild this interaction was executed in.
	/// </summary>
	public DiscordGuild? Guild { get; internal set; }

	/// <summary>
	/// Gets the channel this interaction was executed in.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the user which executed this interaction.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the member which executed this interaction, or null if the command is in a DM.
	/// </summary>
	public DiscordMember? Member
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
	public List<DiscordEntitlement> Entitlements { get; internal set; } = new();

	/// <summary>
	/// <para>Gets the entitlement sku ids.</para>
	/// <para>This is related to premium subscriptions for bots.</para>
	/// <para><note type="warning">Can only be used if you have an associated application subscription sku.</note></para>
	/// </summary>
	[DiscordInExperiment("Currently in closed beta."), Experimental("We provide this type but can't provide support.")]
	public List<ulong> EntitlementSkuIds { get; internal set; } = new();

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
	public IReadOnlyList<DiscordInteractionDataOption>? Options { get; internal set; }

	/// <summary>
	/// The option to autocomplete.
	/// </summary>
	public DiscordInteractionDataOption FocusedOption { get; internal set; }
}
