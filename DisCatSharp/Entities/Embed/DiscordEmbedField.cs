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
/// Represents a field inside a discord embed.
/// </summary>
public sealed class DiscordEmbedField
{
	private string _name;

	/// <summary>
	/// The name of the field.
	/// Must be non-null, non-empty and &lt;= 256 characters.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name
	{
		get => this._name;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				throw new ArgumentException("Name cannot be empty or whitespace.", nameof(value));
			}

			if (value.Length > 256)
				throw new ArgumentException("Embed field name length cannot exceed 256 characters.");

			this._name = value;
		}
	}

	private string _value;

	/// <summary>
	/// The value of the field.
	/// Must be non-null, non-empty and &lt;= 1024 characters.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value
	{
		get => this._value;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				throw new ArgumentException("Value cannot be empty or whitespace.", nameof(value));
			}

			if (value.Length > 1024)
				throw new ArgumentException("Embed field value length cannot exceed 1024 characters.");

			this._value = value;
		}
	}

	/// <summary>
	/// Whether or not this field should display inline.
	/// </summary>
	[JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)]
	public bool Inline { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordEmbedField"/> class.
	/// </summary>
	/// <param name="name"><see cref="Name"/></param>
	/// <param name="value"><see cref="Value"/></param>
	/// <param name="inline"><see cref="Inline"/></param>
	public DiscordEmbedField(string name, string value, bool inline = false)
	{
		this.Name = name;
		this.Value = value;
		this.Inline = inline;
	}
}
