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
    /// Adds a choice for this slash command option
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ChoiceAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the choice
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the choice
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Adds a choice to the slash command option
        /// </summary>
        /// <param name="name">The name of the choice</param>
        /// <param name="value">The value of the choice</param>
        public ChoiceAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Adds a choice to the slash command option
        /// </summary>
        /// <param name="name">The name of the choice</param>
        /// <param name="value">The value of the choice</param>
        public ChoiceAttribute(string name, long value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
