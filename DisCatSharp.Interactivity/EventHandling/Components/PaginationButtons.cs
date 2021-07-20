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

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Interactivity.EventHandling
{
    /// <summary>
    /// The pagination buttons.
    /// </summary>
    public class PaginationButtons
    {
        /// <summary>
        /// Gets or sets the skip left button.
        /// </summary>
        public DiscordButtonComponent SkipLeft { internal get; set; }

        /// <summary>
        /// Gets or sets the left button.
        /// </summary>
        public DiscordButtonComponent Left { internal get; set; }

        /// <summary>
        /// Gets or sets the stop button.
        /// </summary>
        public DiscordButtonComponent Stop { internal get; set; }

        /// <summary>
        /// Gets or sets the right button.
        /// </summary>
        public DiscordButtonComponent Right { internal get; set; }

        /// <summary>
        /// Gets or sets the skip right button.
        /// </summary>
        public DiscordButtonComponent SkipRight { internal get; set; }

        /// <summary>
        /// Gets the button array.
        /// </summary>
        internal DiscordButtonComponent[] ButtonArray => new[]
        {
            this.SkipLeft,
            this.Left,
            this.Stop,
            this.Right,
            this.SkipRight
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationButtons"/> class.
        /// </summary>
        public PaginationButtons()
        {
            this.SkipLeft = new(ButtonStyle.Secondary, "leftskip", null, false, new(DiscordEmoji.FromUnicode("⏮")));
            this.Left = new(ButtonStyle.Secondary, "left", null, false, new(DiscordEmoji.FromUnicode("◀")));
            this.Stop = new(ButtonStyle.Secondary, "stop", null, false, new(DiscordEmoji.FromUnicode("⏹")));
            this.Right = new(ButtonStyle.Secondary, "right", null, false, new(DiscordEmoji.FromUnicode("▶")));
            this.SkipRight = new(ButtonStyle.Secondary, "rightskip", null, false, new(DiscordEmoji.FromUnicode("⏭")));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationButtons"/> class.
        /// </summary>
        /// <param name="other">The other <see cref="PaginationButtons"/>.</param>
        public PaginationButtons(PaginationButtons other)
        {
            this.Stop = new(other.Stop);
            this.Left = new(other.Left);
            this.Right = new(other.Right);
            this.SkipLeft = new(other.SkipLeft);
            this.SkipRight = new(other.SkipRight);
        }
    }
}
