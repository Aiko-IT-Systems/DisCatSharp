// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a text component that can be submitted. Fires <see cref="DisCatSharp.DiscordClient.ComponentInteractionCreated"/> event when submitted.
/// </summary>
public sealed class DiscordTextComponent : DiscordComponent
{
	/// <summary>
	/// The style of the text component.
	/// </summary>
	[JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
	public TextComponentStyle Style { get; internal set; }

	/// <summary>
	/// The text description to apply to the text component.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	/// <summary>
	/// The placeholder for the text component.
	/// </summary>
	[JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
	public string Placeholder { get; internal set; }

	/// <summary>
	/// The pre-filled value for the text component.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value { get; internal set; }

	/// <summary>
	/// The minimal length of text input.
	/// Defaults to 0.
	/// </summary>
	[JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinLength { get; internal set; } = 0;

	/// <summary>
	/// The maximal length of text input.
	/// </summary>
	[JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxLength { get; internal set; }

	// NOTE: Probably will be introduced in future
	/*/// <summary>
        /// Whether this text component can be used.
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Disabled { get; internal set; }*/

	/// <summary>
	/// Whether this text component is required.
	/// Defaults to true.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool Required { get; internal set; }

	/*/// <summary>
        /// Enables this component if it was disabled before.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordTextComponent Enable()
        {
            this.Disabled = false;
            return this;
        }

        /// <summary>
        /// Disables this component.
        /// </summary>
        /// <returns>The current component.</returns>
        public DiscordTextComponent Disable()
        {
            this.Disabled = true;
            return this;
        }*/

	/// <summary>
	/// Constructs a new <see cref="DiscordTextComponent"/>.
	/// </summary>
	internal DiscordTextComponent()
	{
		this.Type = ComponentType.InputText;
	}

	/// <summary>
	/// Constructs a new text component based on another text component.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordTextComponent(DiscordTextComponent other) : this()
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
	}

	/// <summary>
	/// Constructs a new text component field with the specified options.
	/// </summary>
	/// <param name="style">The style of the text component.</param>
	/// <param name="customId">The Id to assign to the text component. This is sent back when a user presses it.</param>
	/// <param name="label">The text to display before the text component, up to 80 characters. Required, but set to null to avoid breaking change.</param>
	/// <param name="placeholder">The placeholder for the text input.</param>
	/// <param name="minLength">The minimal length of text input.</param>
	/// <param name="maxLength">The maximal length of text input.</param>
	/// <param name="required">Whether this text component should be required.</param>
	/// <param name="defaultValue">Pre-filled value for text field.</param>
	/// <exception cref="ArgumentException">Is thrown when no label is set.</exception>
	public DiscordTextComponent(TextComponentStyle style, string customId = null, string label = null, string placeholder = null, int? minLength = null, int? maxLength = null, bool required = true, string defaultValue = null)
	{
		this.Style = style;
		this.Label = label ?? throw new ArgumentException("A label is required.");
		this.CustomId = customId ?? Guid.NewGuid().ToString();
		this.MinLength = minLength;
		this.MaxLength = style == TextComponentStyle.Small ? 256 : maxLength;
		this.Placeholder = placeholder;
		//this.Disabled = disabled;
		this.Required = required;
		this.Value = defaultValue;
		this.Type = ComponentType.InputText;
	}
}
