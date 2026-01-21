using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a single checkbox component. Modal-only.
/// </summary>
public sealed class DiscordCheckboxComponent : DiscordComponent, ILabelComponent
{
	/// <summary>
	///     Creates a new empty checkbox component.
	/// </summary>
	internal DiscordCheckboxComponent()
	{
		this.Type = ComponentType.Checkbox;
	}

	/// <summary>
	///     Creates a new checkbox component with the provided options.
	/// </summary>
	/// <param name="customId">The custom id for this component.</param>
	/// <param name="isDefault">Whether the checkbox is checked by default.</param>
	public DiscordCheckboxComponent(string? customId = null, bool? isDefault = null)
		: this()
	{
		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.Default = isDefault;
	}

	/// <summary>
	///     The custom id for this component.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public override string? CustomId { get; internal set; } = Guid.NewGuid().ToString();

	/// <summary>
	///     Whether the checkbox is checked by default.
	/// </summary>
	[JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Default { get; internal set; }

	/// <summary>
	///     The submitted value. Present on modal submit interactions.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Value { get; internal set; }

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordCheckboxComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
