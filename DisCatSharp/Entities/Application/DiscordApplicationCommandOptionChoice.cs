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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a command parameter choice for a <see cref="DiscordApplicationCommandOption"/>.
    /// </summary>
    public sealed class DiscordApplicationCommandOptionChoice
    {
        /// <summary>
        /// Gets the name of this choice parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

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
        /// Gets the value of this choice parameter. This will either be a type of <see cref="int"/>, <see cref="long"/>, <see cref="double"/> or <see cref="string"/>.
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordApplicationCommandOptionChoice"/>.
        /// </summary>
        /// <param name="Name">The name of the parameter choice.</param>
        /// <param name="Value">The value of the parameter choice.</param>
        /// <param name="NameLocalizations">The localizations of the parameter choice name.</param>
        public DiscordApplicationCommandOptionChoice(string Name, object Value, DiscordApplicationCommandLocalization NameLocalizations = null)
        {
            if (!(Value is string || Value is long || Value is int || Value is double))
                throw new InvalidOperationException($"Only {typeof(string)}, {typeof(long)}, {typeof(double)} or {typeof(int)} types may be passed to a command option choice.");

            if (Name.Length > 100)
                throw new ArgumentException("Application command choice name cannot exceed 100 characters.", nameof(Name));
            if (Value is string val && val.Length > 100)
                throw new ArgumentException("Application command choice value cannot exceed 100 characters.", nameof(Value));

            this.Name = Name;
            this.RawNameLocalizations = NameLocalizations?.GetKeyValuePairs();
            this.Value = Value;
        }
    }
}
