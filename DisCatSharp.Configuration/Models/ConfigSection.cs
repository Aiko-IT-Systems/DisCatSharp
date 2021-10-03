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

namespace DisCatSharp.Common.Configuration.Models
{
    /// <summary>
    /// Represents an object in <see cref="IConfiguration"/>
    /// </summary>
    internal readonly struct ConfigSection
    {
        /// <summary>
        /// Key within <see cref="Config"/> which represents an object containing multiple values
        /// </summary>
        public string SectionName { get;}

        /// <summary>
        /// Reference to <see cref="IConfiguration"/> used within application
        /// </summary>
        public IConfiguration Config { get; }

        public ConfigSection(ref IConfiguration config, string sectionName)
        {
            this.Config = config;
            this.SectionName = sectionName;
        }

        /// <summary>
        /// Checks if key exists in <see cref="Config"/>
        /// </summary>
        /// <param name="name">Property / Key to search for in section</param>
        /// <param name="path">Config path to key</param>
        /// <param name="root">(Optional) Root key to use. Default is DisCatSharp because configs in library should be consolidated</param>
        /// <returns>True if key exists, otherwise false. Outputs path to config regardless</returns>
        public bool ContainsKey(string name, out string path, string root = "DisCatSharp")
        {
            path = string.IsNullOrEmpty(root)
                ? this.Config.ConfigPath(this.SectionName, name)
                : this.Config.ConfigPath(root, this.SectionName, name);

            return !string.IsNullOrEmpty(this.Config[path]);
        }

        /// <summary>
        /// Attempts to get value associated to the config path.
        /// </summary>
        /// <param name="path">Config path to value</param>
        /// <returns>Value found at <paramref name="path"/></returns>
        public string GetValue(string path)
            => this.Config[path];

    }
}
