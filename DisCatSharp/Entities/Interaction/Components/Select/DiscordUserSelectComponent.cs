using System.Collections.Generic;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// A select menu with multiple options to choose from.
/// </summary>
public sealed class DiscordUserSelectComponent : DiscordBaseSelectComponent
{
	/// <summary>
	/// Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordUserSelectComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	/// Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordUserSelectComponent Disable()
	{
		this.Disabled = true;
		return this;
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordUserSelectComponent"/>.
	/// </summary>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	/// <param name="defaultValues">The default values of this select menu.</param>
	public DiscordUserSelectComponent(string placeholder, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false, IEnumerable<DiscordSelectDefaultValue>? defaultValues = null)
		: base(ComponentType.UserSelect, placeholder, customId, minOptions, maxOptions, disabled, defaultValues)
	{ }

	/// <summary>
	/// Constructs a new <see cref="DiscordUserSelectComponent"/> for modals.
	/// </summary>
	/// <param name="label">Maximum count of selectable options.</param>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	/// <param name="defaultValues">The default values of this select menu.</param>
	public DiscordUserSelectComponent(string label, string placeholder, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false, IEnumerable<DiscordSelectDefaultValue>? defaultValues = null)
		: base(ComponentType.UserSelect, label, placeholder, customId, minOptions, maxOptions, disabled, defaultValues)
	{ }

	/// <summary>
	/// Constructs a new <see cref="DiscordUserSelectComponent"/>.
	/// </summary>
	public DiscordUserSelectComponent()
		: base()
	{
		this.Type = ComponentType.UserSelect;
	}
}
