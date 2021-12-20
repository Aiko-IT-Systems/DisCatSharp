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
using System.Linq;
using System;

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

        internal IReadOnlyList<string> _validLocales = new List<string>() { "ru", "fi", "hr", "de", "hu", "sv-SE", "cs", "fr", "it", "en-GB", "pt-BR", "ja", "tr", "en-US", "es-ES", "uk", "hi", "th", "el", "no", "ro", "ko", "zh-TW", "vi", "zh-CN", "pl", "bg", "da", "nl", "lt" };

        /// <summary>
        /// Adds a localization.
        /// </summary>
        /// <param name="language">The language to add.</param>
        /// <param name="value">The translation to add.</param>
        public void AddLocalization(string language, string value)
        {
            if (this.Validate(language))
            {
                this.Localizations.Add(language, value);
            } else
            {
                throw new NotSupportedException($"The provided locale is not valid for Discord. Valid: {this._validLocales}");
            }
        }

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
            foreach(var locale in localizations.Keys)
            {
                if (!this.Validate(locale))
                    throw new NotSupportedException($"The provided locale is not valid for Discord. Valid: {this._validLocales}");
            }

            this.Localizations = localizations;
        }

        public Dictionary<string, string> GetKeyValuePairs()
                => this.Localizations;

        public bool Validate(string lang)
            => this._validLocales.Contains(lang);
    }
}
