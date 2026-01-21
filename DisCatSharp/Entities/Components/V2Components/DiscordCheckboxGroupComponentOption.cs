using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an option within a checkbox group component.
/// </summary>
public sealed class DiscordCheckboxGroupComponentOption : ObservableApiObject
{
	/// <summary>
	///     Creates a new checkbox group option.
	/// </summary>
	/// <param name="label">The display label. Max 100 characters.</param>
	/// <param name="value">The option value. Max 100 characters.</param>
	/// <param name="description">An optional description. Max 100 characters.</param>
	/// <param name="isDefault">Whether this option should be selected by default.</param>
	public DiscordCheckboxGroupComponentOption(string label, string value, string? description = null, bool isDefault = false)
	{
		if (string.IsNullOrWhiteSpace(label))
			throw new ArgumentException("Label must be provided.", nameof(label));
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentException("Value must be provided.", nameof(value));
		if (label.Length > 100)
			throw new ArgumentException("Option label cannot exceed 100 characters.", nameof(label));
		if (value.Length > 100)
			throw new ArgumentException("Option value cannot exceed 100 characters.", nameof(value));
		if (description is { Length: > 100 })
			throw new ArgumentException("Option description cannot exceed 100 characters.", nameof(description));

		this.Label = label;
		this.Value = value;
		this.Description = description;
		this.Default = isDefault;
	}

	/// <summary>
	///     The display label.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	/// <summary>
	///     The underlying value returned on submit.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value { get; internal set; }

	/// <summary>
	///     Optional helper text.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	///     Whether this option is pre-selected.
	/// </summary>
	[JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
	public bool Default { get; internal set; }
}
