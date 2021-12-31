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
using System.Linq;

namespace DisCatSharp.CommandsNext.Attributes
{
    /// <summary>
    /// Marks this method as a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Marks this method as a command, using the method's name as command name.
        /// </summary>
        public CommandAttribute()
        {
            this.Name = null;
        }

        /// <summary>
        /// Marks this method as a command with specified name.
        /// </summary>
        /// <param name="Name">Name of this command.</param>
        public CommandAttribute(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException(nameof(Name), "Command names cannot be null, empty, or all-whitespace.");

            if (Name.Any(Xc => char.IsWhiteSpace(Xc)))
                throw new ArgumentException("Command names cannot contain whitespace characters.", nameof(Name));

            this.Name = Name;
        }
    }

    /// <summary>
    /// Marks this method as a group command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class GroupCommandAttribute : Attribute
    {
        /// <summary>
        /// Marks this method as a group command.
        /// </summary>
        public GroupCommandAttribute()
        { }
    }
}
