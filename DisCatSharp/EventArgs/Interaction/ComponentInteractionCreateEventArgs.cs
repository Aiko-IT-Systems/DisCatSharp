using System;
using System.Diagnostics.CodeAnalysis;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.ComponentInteractionCreated" />.
/// </summary>
public class ComponentInteractionCreateEventArgs : InteractionCreateEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ComponentInteractionCreateEventArgs" /> class.
	/// </summary>
	internal ComponentInteractionCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     The Id of the component that was interacted with.
	/// </summary>
	public string Id => this.Interaction.Data.CustomId;

	/// <summary>
	///     The user that invoked this interaction.
	/// </summary>
	public DiscordUser User => this.Interaction.User;

	/// <summary>
	///     The guild this interaction was invoked on, if any.
	/// </summary>
	public DiscordGuild? Guild => this.Interaction.Guild;

	/// <summary>
	///     The member that invoked this interaction, if any.
	/// </summary>
	[NotNullIfNotNull(nameof(Guild))]
	public DiscordMember? Member => this.User is DiscordMember member ? member : null;

	/// <summary>
	///     The channel this interaction was invoked in.
	/// </summary>
	public DiscordChannel Channel => this.Interaction.Channel;

	/// <summary>
	///     The value(s) selected. Only applicable to SelectMenu components.
	/// </summary>
	public string[] Values => this.Interaction.Data.Values;

	/// <summary>
	///     The message this interaction is attached to.
	/// </summary>
	public DiscordMessage Message { get; internal set; }
}
