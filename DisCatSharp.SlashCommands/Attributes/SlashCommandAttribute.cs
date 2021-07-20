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

namespace DisCatSharp.SlashCommands
{
    /// <summary>
    /// Marks this method as a slash command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SlashCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this command
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this command
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the default permission of this command
        /// </summary>
        public bool DefaultPermission { get; set; }

        /// <summary>
        /// Marks this method as a slash command
        /// </summary>
        /// <param name="name">The name of this slash command</param>
        /// <param name="description">The description of this slash command</param>
        /// <param name="default_permission">Whether everyone can execute this command.</param>
        public SlashCommandAttribute(string name, string description, bool default_permission = true)
        {
            this.Name = name.ToLower();
            this.Description = description;
            this.DefaultPermission = default_permission;
        }
    }
}
