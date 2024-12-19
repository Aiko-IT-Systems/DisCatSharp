using System.Collections.Generic;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a callback hint builder for an interaction.
/// </summary>
internal sealed class DiscordCallbackHintBuilder
{
	/// <summary>
	///     Constructs a new <see cref="DiscordCallbackHintBuilder" />.
	/// </summary>
	internal DiscordCallbackHintBuilder()
	{
		this.Clear();
	}

	/// <summary>
	///     Constructs a new <see cref="DiscordCallbackHintBuilder" /> from an existing one.
	/// </summary>
	/// <param name="other">The existing <see cref="DiscordCallbackHintBuilder" />.</param>
	internal DiscordCallbackHintBuilder(DiscordCallbackHintBuilder other)
	{
		this.Clear();
		this.CallbackHints.AddRange(other.CallbackHints);
	}

	/// <summary>
	///     Gets the callback hints.
	/// </summary>
	internal List<DiscordInteractionCallbackHint> CallbackHints { get; set; } = [];

	/// <summary>
	///     Adds a callback hint to the builder.
	/// </summary>
	/// <param name="intendedCallbackType">The intended respond type.</param>
	/// <param name="intendedCallbackEphemerality">The intended use of ephemeral. Required if it's only ephemeral.</param>
	/// <param name="intendedRequiredPermissions">The intended required permissions.</param>
	/// <returns>The updated <see cref="DiscordCallbackHintBuilder" />.</returns>
	internal DiscordCallbackHintBuilder AddCallbackHint(InteractionResponseType intendedCallbackType, InteractionCallbackEphemerality intendedCallbackEphemerality = InteractionCallbackEphemerality.Optional, Permissions? intendedRequiredPermissions = null)
	{
		this.CallbackHints.Add(new()
		{
			AllowedCallbackType = intendedCallbackType,
			Ephemerality = intendedCallbackEphemerality,
			RequiredPermissions = intendedRequiredPermissions
		});
		return this;
	}

	/// <summary>
	///     Clears the callback hints.
	/// </summary>
	internal void Clear()
		=> this.CallbackHints.Clear();
}
