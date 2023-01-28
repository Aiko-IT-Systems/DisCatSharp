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
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an option for a user to select for auto-completion.
/// </summary>
public sealed class DiscordApplicationCommandAutocompleteChoice
{
	/// <summary>
	/// Gets the name of this option which will be presented to the user.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Sets the name localizations.
	/// </summary>
	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<string, string> RawNameLocalizations { get; set; }

	/// <summary>
	/// Gets the name localizations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization NameLocalizations
		=> new(this.RawNameLocalizations);

	/// <summary>
	/// Gets the value of this option.
	/// </summary>
	[JsonProperty("value")]
	public object Value { get; internal set; }

	/// <summary>
	/// Creates a new instance of <see cref="DiscordApplicationCommandAutocompleteChoice"/>.
	/// </summary>
	/// <param name="name">The name of this option, which will be presented to the user.</param>
	/// <param name="nameLocalizations">The localizations of the option name.</param>
	/// <param name="value">The value of this option.</param>
	public DiscordApplicationCommandAutocompleteChoice(string name, object value, DiscordApplicationCommandLocalization nameLocalizations = null)
	{
		if (name.Length > 100)
			throw new ArgumentException("Application command choice name cannot exceed 100 characters.", nameof(name));
		if (value is string val && val.Length > 100)
			throw new ArgumentException("Application command choice value cannot exceed 100 characters.", nameof(value));
		if (!(value is string || value is long || value is int || value is double))
			throw new InvalidOperationException($"Only {typeof(string)}, {typeof(long)}, {typeof(double)} or {typeof(int)} types may be passed to a autocomplete choice.");

		this.Name = name;
		this.RawNameLocalizations = nameLocalizations?.GetKeyValuePairs();
		this.Value = value;
	}
}
