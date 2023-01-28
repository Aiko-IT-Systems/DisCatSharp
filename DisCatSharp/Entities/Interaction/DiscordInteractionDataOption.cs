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

using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents parameters for interaction commands.
/// </summary>
public sealed class DiscordInteractionDataOption
{
	/// <summary>
	/// Gets the name of this interaction parameter.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the type of this interaction parameter.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandOptionType Type { get; internal set; }

	/// <summary>
	/// Whether this option is currently focused by the user.
	/// Only applicable for autocomplete option choices.
	/// </summary>
	[JsonProperty("focused")]
	public bool Focused { get; internal set; }

	/// <summary>
	/// Gets the value of this interaction parameter.
	/// </summary>
	[JsonProperty("value")]
	internal string RawValue { get; set; }

	/// <summary>
	/// Gets the value of this interaction parameter.
	/// <para>This can be cast to a <see langword="long"/>, <see langword="bool"></see>, <see langword="string"></see>, <see langword="double"></see> or <see langword="ulong"/> depending on the <see cref="System.Type"/></para>
	/// </summary>
	[JsonIgnore]
	public object Value =>
		this.Type == ApplicationCommandOptionType.Integer && int.TryParse(this.RawValue, out var raw)
			? raw
			: this.Type == ApplicationCommandOptionType.Integer
				? long.Parse(this.RawValue)
				: this.Type switch
				{
					ApplicationCommandOptionType.Boolean => bool.Parse(this.RawValue),
					ApplicationCommandOptionType.String => this.RawValue,
					ApplicationCommandOptionType.Channel => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.User => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.Role => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.Mentionable => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.Number => double.Parse(this.RawValue),
					ApplicationCommandOptionType.Attachment => ulong.Parse(this.RawValue),
					_ => this.RawValue,
				};

	/// <summary>
	/// Gets the additional parameters if this parameter is a subcommand.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordInteractionDataOption> Options { get; internal set; }
}
