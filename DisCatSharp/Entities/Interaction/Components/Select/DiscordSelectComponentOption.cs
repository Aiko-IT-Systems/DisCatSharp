// This file is part of the DisCatSharp project, a fork of DSharpPlus.
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
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents options for <see cref="DiscordSelectComponent"/>.
    /// </summary>
    public sealed class DiscordSelectComponentOption
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
        /// Constructs a new <see cref="DiscordSelectComponentOption"/>.
        /// </summary>
        /// <param name="Label">The label of this option..</param>
        /// <param name="Value">The value of this option.</param>
        /// <param name="Description">Description of the option.</param>
        /// <param name="IsDefault">Whether this option is default. If true, this option will be pre-selected.</param>
        /// <param name="Emoji">The emoji to set with this option.</param>
        public DiscordSelectComponentOption(string Label, string Value, string Description = null, bool IsDefault = false, DiscordComponentEmoji Emoji = null)
        {
            if (Label.Length > 100)
                throw new NotSupportedException("Select label can't be longer then 100 chars.");
            if (Value.Length > 100)
                throw new NotSupportedException("Select label can't be longer then 100 chars.");

            this.Label = Label;
            this.Value = Value;
            this.Description = Description;
            this.Default = IsDefault;
            this.Emoji = Emoji;
        }
    }
}
