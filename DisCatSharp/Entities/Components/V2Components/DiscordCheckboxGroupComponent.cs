using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a checkbox group component. Modal-only.
/// </summary>
public sealed class DiscordCheckboxGroupComponent : DiscordComponent, ILabelComponent
{
	/// <summary>
	///     Creates a new empty checkbox group component.
	/// </summary>
	internal DiscordCheckboxGroupComponent()
	{
		this.Type = ComponentType.CheckboxGroup;
	}

	/// <summary>
	///     Creates a new checkbox group component with the provided options.
	/// </summary>
	/// <param name="options">The selectable options. Must contain between 1 and 10 entries.</param>
	/// <param name="customId">The custom id for this component.</param>
	/// <param name="minValues">The minimum number of selections. Defaults to 1.</param>
	/// <param name="maxValues">The maximum number of selections. Defaults to the number of options.</param>
	/// <param name="required">Whether a selection is required.</param>
	public DiscordCheckboxGroupComponent(IEnumerable<DiscordCheckboxGroupComponentOption> options, string? customId = null, int? minValues = null, int? maxValues = null, bool? required = null)
		: this()
	{
		ArgumentNullException.ThrowIfNull(options);
		var optionList = options.ToList();
		if (optionList.Count is < 1 or > 10)
			throw new ArgumentException("Checkbox groups must include between 1 and 10 options.");

		var minimum = minValues ?? 1;
		var maximum = maxValues ?? optionList.Count;

		if (minimum is < 0 or > 10)
			throw new ArgumentException("Minimum values must be between 0 and 10.", nameof(minValues));
		if (maximum is < 1 or > 10)
			throw new ArgumentException("Maximum values must be between 1 and 10.", nameof(maxValues));
		if (minimum > maximum)
			throw new ArgumentException("Minimum values cannot exceed maximum values.");
		if (maximum > optionList.Count)
			throw new ArgumentException("Maximum values cannot exceed the number of options.");

		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.Options = optionList;
		this.MinimumValues = minimum;
		this.MaximumValues = maximum;
		this.Required = required;
	}

	/// <summary>
	///     The custom id for this component.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public override string? CustomId { get; internal set; } = Guid.NewGuid().ToString();

	/// <summary>
	///     The available options.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordCheckboxGroupComponentOption> Options { get; internal set; } = Array.Empty<DiscordCheckboxGroupComponentOption>();

	/// <summary>
	///     The minimum number of selections.
	/// </summary>
	[JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinimumValues { get; internal set; } = 0;

	/// <summary>
	///     The maximum number of selections.
	/// </summary>
	[JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaximumValues { get; internal set; }

	/// <summary>
	///     Whether the component requires a selection.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Required { get; internal set; }

	/// <summary>
	///     The submitted values. Present on modal submit interactions.
	/// </summary>
	[JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
	public string[]? SelectedValues { get; internal set; }

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordCheckboxGroupComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
