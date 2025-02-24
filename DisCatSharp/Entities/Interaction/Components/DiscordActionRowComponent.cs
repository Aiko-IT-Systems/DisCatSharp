using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a row of components. Action rows can have up to five components.
/// </summary>
public sealed class DiscordActionRowComponent : DiscordComponent
{
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	private List<DiscordComponent> _components;

	/// <summary>
	///     Constructs a new <see cref="DiscordActionRowComponent" />.
	/// </summary>
	/// <param name="components">List of components</param>
	public DiscordActionRowComponent(IEnumerable<DiscordComponent> components)
		: this()
	{
		this.Components = components.ToList().AsReadOnly();
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordActionRowComponent" /> class.
	/// </summary>
	internal DiscordActionRowComponent()
	{
		this.Type = ComponentType.ActionRow;
	}

	/// <summary>
	///     The components contained within the action row.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyCollection<DiscordComponent> Components
	{
		get => this._components ?? [];
		set => this._components = [.. value];
	}

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordActionRowComponent WithId(uint id)
	{
		this.Id = id;
		return this;
	}
}
