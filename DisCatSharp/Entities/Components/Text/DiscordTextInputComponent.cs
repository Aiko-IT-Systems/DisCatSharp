using System;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a text input component that can be submitted. Fires
///     <see cref="DisCatSharp.DiscordClient.ComponentInteractionCreated" /> event when submitted.
/// </summary>
public sealed class DiscordTextInputComponent : DiscordComponent, ILabelComponent
{
	/*
	/// <summary>
	/// Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordTextInputComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	/// Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordTextInputComponent Disable()
	{
		this.Disabled = true;
		return this;
	}
	*/

	/// <summary>
	///     Constructs a new <see cref="DiscordTextInputComponent" />.
	/// </summary>
	internal DiscordTextInputComponent()
	{
		this.Type = ComponentType.TextInput;
	}

	/// <summary>
	///     Constructs a new text component based on another text component.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordTextInputComponent(DiscordTextInputComponent other)
		: this()
	{
		this.CustomId = other.CustomId;
		this.Style = other.Style;
		this.Label = other.Label;
		//this.Disabled = other.Disabled;
		this.MinLength = other.MinLength;
		this.MaxLength = other.MaxLength;
		this.Placeholder = other.Placeholder;
		this.Required = other.Required;
		this.Value = other.Value;
		this.Id = other.Id;
	}

	/// <summary>
	///     Constructs a new text input component with the specified options.
	/// </summary>
	/// <param name="style">The style of the text input component.</param>
	/// <param name="label">The text to display before the text input component, up to 80 characters.</param>
	/// <param name="customId">The Id to assign to the text input component. Max <c>100</c> characters.</param>
	/// <param name="placeholder">The placeholder for the text input component. Max <c>100</c> characters.</param>
	/// <param name="minLength">The minimal length of text input.</param>
	/// <param name="maxLength">The maximal length of text input.</param>
	/// <param name="required">Whether this text input component should be required.</param>
	/// <param name="defaultValue">Pre-filled value for text input component. Max <c>4000</c> characters.</param>
	/// <exception cref="ArgumentException">Is thrown when no label is set.</exception>
	[DiscordDeprecated("Deprecated in favor of DiscordLabelComponent. Use overload without label in the new component.")]
	public DiscordTextInputComponent(TextComponentStyle style, string label, string? customId = null, string? placeholder = null, int? minLength = null, int? maxLength = null, bool required = true, string? defaultValue = null)
		: this()
	{
		this.Style = style;
		this.Label = label;
		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.MinLength = minLength;
		this.MaxLength = maxLength ??= style is TextComponentStyle.Paragraph ? 4000 : 256;
		this.Placeholder = placeholder;
		//this.Disabled = disabled;
		this.Required = required;
		this.Value = defaultValue;
	}

	/// <summary>
	///     Constructs a new text input component with the specified options.
	/// </summary>
	/// <param name="style">The style of the text input component.</param>
	/// <param name="customId">The Id to assign to the text input component. Max <c>100</c> characters.</param>
	/// <param name="placeholder">The placeholder for the text input component. Max <c>100</c> characters.</param>
	/// <param name="minLength">The minimal length of text input.</param>
	/// <param name="maxLength">The maximal length of text input.</param>
	/// <param name="required">Whether this text input component should be required.</param>
	/// <param name="defaultValue">Pre-filled value for text input component. Max <c>4000</c> characters.</param>
	public DiscordTextInputComponent(TextComponentStyle style, string? customId = null, string? placeholder = null, int? minLength = null, int? maxLength = null, bool required = true, string? defaultValue = null)
		: this()
	{
		this.Style = style;
		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.MinLength = minLength;
		this.MaxLength = maxLength ??= style is TextComponentStyle.Paragraph ? 4000 : 256;
		this.Placeholder = placeholder;
		//this.Disabled = disabled;
		this.Required = required;
		this.Value = defaultValue;
	}

	/// <summary>
	///     The custom Id of this text input component.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public override string? CustomId { get; internal set; } = Guid.NewGuid().ToString();

	/// <summary>
	///     The style of the text input component.
	/// </summary>
	[JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
	public TextComponentStyle Style { get; internal set; }

	/// <summary>
	///     The text description to apply to the text input component.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore), DiscordDeprecated("Deprecated in favor of DiscordLabelComponent.")]
	public string? Label { get; internal set; }

	/// <summary>
	///     The placeholder for the text input component.
	/// </summary>
	[JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
	public string? Placeholder { get; internal set; }

	/// <summary>
	///     The pre-filled value for the text input component.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string? Value { get; internal set; }

	/// <summary>
	///     The minimal length of text input component.
	///     Defaults to 0.
	/// </summary>
	[JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinLength { get; internal set; } = 0;

	/// <summary>
	///     The maximal length of text input component.
	/// </summary>
	[JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxLength { get; internal set; }

	// NOTE: Probably will be introduced in future
	/*
	/// <summary>
	/// Whether this text input component can be used.
	/// </summary>
	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; internal set; }
	*/

	/// <summary>
	///     Whether this text input component is required.
	///     Defaults to true.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool Required { get; internal set; }

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordTextInputComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
