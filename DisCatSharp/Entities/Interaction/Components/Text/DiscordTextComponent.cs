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

using DisCatSharp.Enums;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{

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
        /// The text to apply to the text component.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; internal set; }

        /// <summary>
        /// Whether this text component can be used.
        /// </summary>
        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Disabled { get; internal set; }

        /// <summary>
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
        }

        /// <summary>
        /// Constructs a new <see cref="DiscordTextComponent"/>.
        /// </summary>
        internal DiscordTextComponent()
        {
            this.Type = ComponentType.InputText;
        }

        /// <summary>
        /// Constucts a new text component based on another text component.
        /// </summary>
        /// <param name="other">The button to copy.</param>
        public DiscordTextComponent(DiscordTextComponent other) : this()
        {
            this.CustomId = other.CustomId;
            this.Style = other.Style;
            this.Label = other.Label;
            this.Disabled = other.Disabled;
        }

        /// <summary>
        /// Constructs a new text component field with the specified options.
        /// </summary>
        /// <param name="style">The style of the text component.</param>
        /// <param name="customId">The Id to assign to the text component. This is sent back when a user presses it.</param>
        /// <param name="label">The text to display before the text component, up to 80 characters.</param>
        /// <param name="disabled">Whether this text component should be initialized as being disabled.</param>
        public DiscordTextComponent(TextComponentStyle style, string customId, string label, bool disabled = false)
        {
            this.Style = style;
            this.Label = label;
            this.CustomId = customId;
            this.Disabled = disabled;
            this.Type = ComponentType.InputText;
        }
    }
}
