// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp.Entities
{
    /// <summary>
    /// A select menu with multiple options to choose from.
    /// </summary>
    public sealed class DiscordSelectComponent : DiscordComponent
    {
        /// <summary>
        /// The options to pick from on this component.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSelectComponentOption> Options { get; internal set; } = Array.Empty<DiscordSelectComponentOption>();

        /// <summary>
        /// The text to show when no option is selected.
        /// </summary>
        [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
        public string Placeholder { get; internal set; }

        /// <summary>
        /// The minimum amount of options that can be selected. Must be less than or equal to <see cref="MaximumSelectedValues"/>. Defaults to one.
        /// </summary>
        [JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinimumSelectedValues { get; internal set; }

        /// <summary>
        /// The maximum amount of options that can be selected. Must be greater than or equal to zero or <see cref="MinimumSelectedValues"/>. Defaults to one.
        /// </summary>
        [JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaximumSelectedValues { get; internal set; }

        /// <summary>
        /// Whether this select can be used.
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Disabled { get; internal set; }

        /// <summary>
        /// Constructs a new <see cref="DiscordSelectComponent"/>.
        /// </summary>
        /// <param name="customId">The Id to assign to the button. This is sent back when a user presses it.</param>
        /// <param name="options">Array of options</param>
        /// <param name="placeholder">Text to show if no option is slected.</param>
        /// <param name="minOptions">Minmum count of selectable options.</param>
        /// <param name="maxOptions">Maximum count of selectable options.</param>
        /// <param name="disabled">Whether this button should be initialized as being disabled. User sees a greyed out button that cannot be interacted with.</param>
        public DiscordSelectComponent(string customId, string placeholder, IEnumerable<DiscordSelectComponentOption> options, int minOptions = 1, int maxOptions = 1, bool disabled = false) : this()
        {
            this.CustomId = customId;
            this.Disabled = disabled;
            this.Options = options.ToArray();
            this.Placeholder = placeholder;
            this.MinimumSelectedValues = minOptions;
            this.MaximumSelectedValues = maxOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordSelectComponent"/> class.
        /// </summary>
        internal DiscordSelectComponent()
        {
            this.Type = ComponentType.Select;
        }
    }
}
