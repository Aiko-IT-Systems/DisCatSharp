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

using System.Collections.Generic;
using System.Globalization;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a application command localization.
    /// </summary>
    public sealed class DiscordApplicationCommandLocalization
    {
        /// <summary>
        /// Gets the localization dict.
        /// </summary>
        public Dictionary<string, string> Localizations { get; internal set; }

        /// <summary>
        /// Adds a localization.
        /// </summary>
        /// <param name="language">The <see cref="System.Globalization.CultureInfo"/> to add.</param>
        /// <param name="value">The translation to add.</param>
        public void AddLocalization(CultureInfo language, string value)
            => this.Localizations.Add(language.Name, value);

        /// <summary>
        /// Adds a localization.
        /// </summary>
        /// <param name="language">The language to add.</param>
        /// <param name="value">The translation to add.</param>
        public void AddLocalization(string language, string value)
            => this.Localizations.Add(language, value);

        /// <summary>
        /// Removes a localization.
        /// </summary>
        /// <param name="language">The <see cref="System.Globalization.CultureInfo"/> to remove.</param>
        public void RemoveLocalization(CultureInfo language)
            => this.Localizations.Remove(language.Name);

        /// <summary>
        /// Removes a localization.
        /// </summary>
        /// <param name="language">The language to remove.</param>
        public void RemoveLocalization(string language)
            => this.Localizations.Remove(language);

        /// <summary>
        /// Initializes a new instance of <see cref="DiscordApplicationCommandLocalization"/>.
        /// </summary>
        public DiscordApplicationCommandLocalization() { }

        /// <summary>
        /// Initializes a new instance of <see cref="DiscordApplicationCommandLocalization"/>.
        /// </summary>
        /// <param name="localizations">Localizations.</param>
        public DiscordApplicationCommandLocalization(Dictionary<string, string> localizations)
        {
            this.Localizations = localizations;
        }

        public Dictionary<string, string> GetKeyValuePairs()
                => this.Localizations;
    }
}
