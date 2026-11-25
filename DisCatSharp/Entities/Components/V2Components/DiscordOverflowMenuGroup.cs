using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a labeled group of actions displayed in an overflow menu.
/// </summary>
public sealed class DiscordOverflowMenuGroup
{
	/// <summary>
	///     Constructs a group containing the supplied menu items.
	/// </summary>
	/// <param name="components">The one to eight menu action items in the group.</param>
	/// <param name="label">The optional group heading, up to 80 characters.</param>
	public DiscordOverflowMenuGroup(IEnumerable<DiscordMenuActionItemComponent> components, string? label = null)
	{
		ArgumentNullException.ThrowIfNull(components);
		var items = components.ToList();
		if (items.Count is < 1 or > 8)
			throw new ArgumentException("An overflow menu group must contain between 1 and 8 items.", nameof(components));
		if (items.Any(x => x is null))
			throw new ArgumentException("Overflow menu groups cannot contain null items.", nameof(components));
		if (label?.Length > 80)
			throw new ArgumentException("An overflow menu group label cannot exceed 80 characters.", nameof(label));

		this.Components = items.AsReadOnly();
		this.Label = label;
	}

	/// <summary>
	///     Gets the optional group heading.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string? Label { get; }

	/// <summary>
	///     Gets the actions in the group.
	/// </summary>
	[JsonProperty("components")]
	public IReadOnlyList<DiscordMenuActionItemComponent> Components { get; }
}
