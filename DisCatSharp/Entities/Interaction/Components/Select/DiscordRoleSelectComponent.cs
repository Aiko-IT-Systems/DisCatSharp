using System.Collections.Generic;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// A select menu with multiple options to choose from.
/// </summary>
public sealed class DiscordRoleSelectComponent : DiscordBaseSelectComponent
{
	/// <summary>
	/// Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordRoleSelectComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	/// Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordRoleSelectComponent Disable()
	{
		this.Disabled = true;
		return this;
	}

	// TODO: Can we set required

	/// <summary>
	/// Constructs a new <see cref="DiscordRoleSelectComponent"/>.
	/// </summary>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	/// <param name="defaultValues">The default values of this select menu.</param>
	public DiscordRoleSelectComponent(string placeholder, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false, IEnumerable<DiscordSelectDefaultValue>? defaultValues = null)
		: base(ComponentType.RoleSelect, placeholder, customId, minOptions, maxOptions, disabled, defaultValues)
	{ }

	/// <summary>
	/// Constructs a new <see cref="DiscordRoleSelectComponent"/> for modals.
	/// </summary>
	/// <param name="label">Maximum count of selectable options.</param>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	/// <param name="defaultValues">The default values of this select menu.</param>
	public DiscordRoleSelectComponent(string label, string placeholder, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false, IEnumerable<DiscordSelectDefaultValue>? defaultValues = null)
		: base(ComponentType.RoleSelect, label, placeholder, customId, minOptions, maxOptions, disabled, defaultValues)
	{ }

	/// <summary>
	/// Constructs a new <see cref="DiscordRoleSelectComponent"/>.
	/// </summary>
	public DiscordRoleSelectComponent()
		: base()
	{
		this.Type = ComponentType.RoleSelect;
	}
}
