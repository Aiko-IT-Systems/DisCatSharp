using System.Collections.Generic;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     A select menu with multiple options to choose from.
/// </summary>
public sealed class DiscordMentionableSelectComponent : DiscordBaseSelectComponent
{
	// TODO: Can we set required

	/// <summary>
	///     Constructs a new <see cref="DiscordMentionableSelectComponent" />.
	/// </summary>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">
	///     Whether this select component should be initialized as being disabled. User sees a greyed out
	///     select component that cannot be interacted with.
	/// </param>
	/// <param name="defaultValues">The default values of this select menu.</param>
	/// <param name="required">Whether this select component is required. Applicable for Modals.</param>
	public DiscordMentionableSelectComponent(string placeholder, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false, IEnumerable<DiscordSelectDefaultValue>? defaultValues = null, bool? required = null)
		: base(ComponentType.MentionableSelect, placeholder, customId, minOptions, maxOptions, disabled, defaultValues, required)
	{ }

	/// <summary>
	///     Constructs a new <see cref="DiscordMentionableSelectComponent" />.
	/// </summary>
	public DiscordMentionableSelectComponent()
	{
		this.Type = ComponentType.MentionableSelect;
	}

	/// <summary>
	///     Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordMentionableSelectComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	///     Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordMentionableSelectComponent Disable()
	{
		this.Disabled = true;
		return this;
	}

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordMentionableSelectComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
