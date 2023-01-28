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
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// A select menu with multiple options to choose from.
/// </summary>
public sealed class DiscordStringSelectComponent : DiscordBaseSelectComponent
{
	/// <summary>
	/// The options to pick from on this component.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordStringSelectComponentOption> Options { get; internal set; } = Array.Empty<DiscordStringSelectComponentOption>();

	/// <summary>
	/// Enables this component if it was disabled before.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordStringSelectComponent Enable()
	{
		this.Disabled = false;
		return this;
	}

	/// <summary>
	/// Disables this component.
	/// </summary>
	/// <returns>The current component.</returns>
	public DiscordStringSelectComponent Disable()
	{
		this.Disabled = true;
		return this;
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordStringSelectComponent"/>.
	/// </summary>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="options">Array of options</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	public DiscordStringSelectComponent(string placeholder, IEnumerable<DiscordStringSelectComponentOption> options, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false)
		: base(ComponentType.StringSelect, placeholder, customId, minOptions, maxOptions, disabled)
	{
		this.Options = options.ToArray();
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordStringSelectComponent"/> for modals.
	/// </summary>
	/// <param name="label">Maximum count of selectable options.</param>
	/// <param name="placeholder">Text to show if no option is selected.</param>
	/// <param name="options">Array of options</param>
	/// <param name="customId">The Id to assign to the select component.</param>
	/// <param name="minOptions">Minimum count of selectable options.</param>
	/// <param name="maxOptions">Maximum count of selectable options.</param>
	/// <param name="disabled">Whether this select component should be initialized as being disabled. User sees a greyed out select component that cannot be interacted with.</param>
	public DiscordStringSelectComponent(string label, string placeholder, IEnumerable<DiscordStringSelectComponentOption> options, string customId = null, int minOptions = 1, int maxOptions = 1, bool disabled = false)
		: base(ComponentType.StringSelect, label, placeholder, customId, minOptions, maxOptions, disabled)
	{
		this.Options = options.ToArray();
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordStringSelectComponent"/>.
	/// </summary>
	public DiscordStringSelectComponent() : base()
	{
		this.Type = ComponentType.StringSelect;
	}
}
