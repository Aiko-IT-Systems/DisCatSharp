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

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents options for <see cref="DiscordBaseSelectComponent"/>.
/// </summary>
public sealed class DiscordStringSelectComponentOption
{
	/// <summary>
	/// The label to add. This is required.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	/// <summary>
	/// The value of this option. Akin to the Custom Id of components.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value { get; internal set; }

	/// <summary>
	/// Whether this option is default. If true, this option will be pre-selected. Defaults to false.
	/// </summary>
	[JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
	public bool Default { get; internal set; } // false //

	/// <summary>
	/// The description of this option. This is optional.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// The emoji of this option. This is optional.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordComponentEmoji Emoji { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="DiscordStringSelectComponentOption"/>.
	/// </summary>
	/// <param name="label">The label of this option.</param>
	/// <param name="value">The value of this option.</param>
	/// <param name="description">Description of the option.</param>
	/// <param name="isDefault">Whether this option is default. If true, this option will be pre-selected.</param>
	/// <param name="emoji">The emoji to set with this option.</param>
	public DiscordStringSelectComponentOption(string label, string value, string description = null, bool isDefault = false, DiscordComponentEmoji emoji = null)
	{
		if (label.Length > 100)
			throw new NotSupportedException("Select label can't be longer then 100 chars.");
		if (value.Length > 100)
			throw new NotSupportedException("Select value can't be longer then 100 chars.");
		if (description != null && description.Length > 100)
			throw new NotSupportedException("Select description can't be longer then 100 chars.");

		this.Label = label;
		this.Value = value;
		this.Description = description;
		this.Default = isDefault;
		this.Emoji = emoji;
	}
}
