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

using Microsoft.Extensions.Configuration;

namespace DisCatSharp.Configuration.Models
{
    /// <summary>
    /// Represents an object in <see cref="IConfiguration"/>
    /// </summary>
    internal readonly struct ConfigSection
    {
        /// <summary>
        /// Key within <see cref="Config"/> which represents an object containing multiple values
        /// </summary>
        public string SectionName { get; }

        /// <summary>
        /// Optional used to indicate this section is nested within another
        /// </summary>
        public string? Root { get; }

        /// <summary>
        /// Reference to <see cref="IConfiguration"/> used within application
        /// </summary>
        public IConfiguration Config { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSection"/> class.
        /// </summary>
        /// <param name="Config">Reference to config</param>
        /// <param name="SectionName">Section of interest</param>
        /// <param name="RootName">(Optional) Indicates <paramref name="SectionName"/> is nested within this name. Default value is DisCatSharp</param>
        public ConfigSection(ref IConfiguration Config, string SectionName, string? RootName = "DisCatSharp")
        {
            this.Config = Config;
            this.SectionName = SectionName;
            this.Root = RootName;
        }

        /// <summary>
        /// Checks if key exists in <see cref="Config"/>
        /// </summary>
        /// <param name="Name">Property / Key to search for in section</param>
        /// <returns>True if key exists, otherwise false. Outputs path to config regardless</returns>
        public bool ContainsKey(string Name)
        {
            var path = string.IsNullOrEmpty(this.Root)
                ? this.Config.ConfigPath(this.SectionName, Name)
                : this.Config.ConfigPath(this.Root, this.SectionName, Name);

            return !string.IsNullOrEmpty(this.Config[path]);
        }

        /// <summary>
        /// Attempts to get value associated to the config path. <br/> Should be used in unison with <see cref="ContainsKey"/>
        /// </summary>
        /// <param name="PropName">Config path to value</param>
        /// <returns>Value found at <paramref name="PropName"/></returns>
        public string GetValue(string PropName)
            => this.Config[this.GetPath(PropName)];

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="Value">The value.</param>
        /// <returns>A string.</returns>
        public string GetPath(string Value)
        {
            return string.IsNullOrEmpty(this.Root)
                ? this.Config.ConfigPath(this.SectionName, Value)
                : this.Config.ConfigPath(this.Root, this.SectionName, Value);
        }
    }
}
