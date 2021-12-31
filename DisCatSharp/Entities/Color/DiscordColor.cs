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
using System.Globalization;
using System.Linq;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a color used in Discord API.
    /// </summary>
    public partial struct DiscordColor
    {
        private static char[] _hexAlphabet = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// Gets the integer representation of this color.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Gets the red component of this color as an 8-bit integer.
        /// </summary>
        public byte R
            => (byte)((this.Value >> 16) & 0xFF);

        /// <summary>
        /// Gets the green component of this color as an 8-bit integer.
        /// </summary>
        public byte G
            => (byte)((this.Value >> 8) & 0xFF);

        /// <summary>
        /// Gets the blue component of this color as an 8-bit integer.
        /// </summary>
        public byte B
            => (byte)(this.Value & 0xFF);

        /// <summary>
        /// Creates a new color with specified value.
        /// </summary>
        /// <param name="Color">Value of the color.</param>
        public DiscordColor(int Color)
        {
            this.Value = Color;
        }

        /// <summary>
        /// Creates a new color with specified values for red, green, and blue components.
        /// </summary>
        /// <param name="R">Value of the red component.</param>
        /// <param name="G">Value of the green component.</param>
        /// <param name="B">Value of the blue component.</param>
        public DiscordColor(byte R, byte G, byte B)
        {
            this.Value = (R << 16) | (G << 8) | B;
        }

        /// <summary>
        /// Creates a new color with specified values for red, green, and blue components.
        /// </summary>
        /// <param name="R">Value of the red component.</param>
        /// <param name="G">Value of the green component.</param>
        /// <param name="B">Value of the blue component.</param>
        public DiscordColor(float R, float G, float B)
        {
            if (R < 0 || R > 1 || G < 0 || G > 1 || B < 0 || B > 1)
                throw new ArgumentOutOfRangeException("Each component must be between 0.0 and 1.0 inclusive.");

            var rb = (byte)(R * 255);
            var gb = (byte)(G * 255);
            var bb = (byte)(B * 255);

            this.Value = (rb << 16) | (gb << 8) | bb;
        }

        /// <summary>
        /// Creates a new color from specified string representation.
        /// </summary>
        /// <param name="Color">String representation of the color. Must be 6 hexadecimal characters, optionally with # prefix.</param>
        public DiscordColor(string Color)
        {
            if (string.IsNullOrWhiteSpace(Color))
                throw new ArgumentNullException(nameof(Color), "Null or empty values are not allowed!");

            if (Color.Length != 6 && Color.Length != 7)
                throw new ArgumentException(nameof(Color), "Color must be 6 or 7 characters in length.");

            Color = Color.ToUpper();
            if (Color.Length == 7 && Color[0] != '#')
                throw new ArgumentException(nameof(Color), "7-character colors must begin with #.");
            else if (Color.Length == 7)
                Color = Color[1..];

            if (Color.Any(Xc => !_hexAlphabet.Contains(Xc)))
                throw new ArgumentException(nameof(Color), "Colors must consist of hexadecimal characters only.");

            this.Value = int.Parse(Color, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets a string representation of this color.
        /// </summary>
        /// <returns>String representation of this color.</returns>
        public override string ToString() => $"#{this.Value:X6}";

        public static implicit operator DiscordColor(int Value)
            => new(Value);
    }
}
