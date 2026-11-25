using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an overflow menu that groups actions in a message.
/// </summary>
public sealed class DiscordOverflowMenuComponent : DiscordSectionAccessory
{
	/// <summary>
	///     Initializes a new instance for payload deserialization.
	/// </summary>
	internal DiscordOverflowMenuComponent()
	{
		this.Type = ComponentType.OverflowMenu;
	}

	/// <summary>
	///     Constructs a menu containing up to eight groups.
	/// </summary>
	/// <param name="groups">The one to eight groups shown by this menu.</param>
	public DiscordOverflowMenuComponent(IEnumerable<DiscordOverflowMenuGroup> groups)
		: this()
	{
		ArgumentNullException.ThrowIfNull(groups);
		var groupList = groups.ToList();
		if (groupList.Count is < 1 or > 8)
			throw new ArgumentException("An overflow menu must contain between 1 and 8 groups.", nameof(groups));
		if (groupList.Any(x => x is null))
			throw new ArgumentException("Overflow menus cannot contain null groups.", nameof(groups));

		this.Groups = groupList.AsReadOnly();
	}

	/// <summary>
	///     Gets the groups of actions displayed by this menu.
	/// </summary>
	[JsonProperty("groups")]
	public IReadOnlyList<DiscordOverflowMenuGroup> Groups { get; internal set; } = [];

	/// <inheritdoc/>
	public override IEnumerable<DiscordComponent> GetChildren()
	{
		foreach (var group in this.Groups)
			foreach (var item in group.Components)
				yield return item;
	}

	/// <summary>
	///     Assigns a component identifier.
	/// </summary>
	/// <param name="id">The identifier assigned by the application.</param>
	/// <returns>This menu.</returns>
	public DiscordOverflowMenuComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
