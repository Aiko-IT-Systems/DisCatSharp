using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// A select menu with multiple options to choose from.
/// </summary>
public sealed class DiscordStringSelectComponent : DiscordBaseSelectComponent
{
	/// <summary>
	/// The options to pick from on this component.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordStringSelectComponentOption> Options { get; internal set; } = Array.Empty<DiscordStringSelectComponentOption>();

	/// <summary>
	/// Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordStringSelectComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	/// Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordStringSelectComponent Disable()
	{
		this.Disabled = true;
		return this;
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordStringSelectComponent"/>.
	/// </summary>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="options">Array of options</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	public DiscordStringSelectComponent(string placeholder, IEnumerable<DiscordStringSelectComponentOption> options, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false)
		: base(ComponentType.StringSelect, placeholder, customId, minOptions, maxOptions, disabled)
	{
		this.Options = options.ToArray();
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordStringSelectComponent"/> for modals.
	/// </summary>
	/// <param name="label">Maximum count of selectable options.</param>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="options">Array of options</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	public DiscordStringSelectComponent(string label, string placeholder, IEnumerable<DiscordStringSelectComponentOption> options, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false)
		: base(ComponentType.StringSelect, label, placeholder, customId, minOptions, maxOptions, disabled)
	{
		this.Options = options.ToArray();
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordStringSelectComponent"/>.
	/// </summary>
	public DiscordStringSelectComponent()
		: base()
	{
		this.Type = ComponentType.StringSelect;
	}
}
