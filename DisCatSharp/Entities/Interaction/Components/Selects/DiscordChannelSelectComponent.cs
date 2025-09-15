using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     A select menu with multiple options to choose from.
/// </summary>
public sealed class DiscordChannelSelectComponent : DiscordBaseSelectComponent
{
	// TODO: Can we set required

	/// <summary>
	///     Constructs a new <see cref="DiscordChannelSelectComponent" />.
	/// </summary>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="channelTypes">The channel types to filter by.</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">
	///     Whether this select component should be initialized as being disabled. User sees a greyed out
	///     select component that cannot be interacted with.
	/// </param>
	/// <param name="defaultValues">The default values of this select menu.</param>
	/// <param name="required">Whether this select component is required. Applicable for Modals.</param>
	public DiscordChannelSelectComponent(string placeholder, IEnumerable<ChannelType> channelTypes = null, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false, IEnumerable<DiscordSelectDefaultValue>? defaultValues = null, bool? required = null)
		: base(ComponentType.ChannelSelect, placeholder, customId, minOptions, maxOptions, disabled, defaultValues, required)
	{
		this.ChannelTypes = channelTypes?.ToArray() ?? [];
	}

	/// <summary>
	///     Constructs a new <see cref="DiscordChannelSelectComponent" />.
	/// </summary>
	public DiscordChannelSelectComponent()
	{
		this.Type = ComponentType.ChannelSelect;
	}

	/// <summary>
	///     The channel types to filter by.
	/// </summary>
	[JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ChannelType>? ChannelTypes { get; internal set; } = null;

	/// <summary>
	///     Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordChannelSelectComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	///     Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordChannelSelectComponent Disable()
	{
		this.Disabled = true;
		return this;
	}

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordChannelSelectComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
