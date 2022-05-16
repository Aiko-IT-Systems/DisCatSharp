// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

namespace DisCatSharp.ApplicationCommands;

/// <summary>
/// Marks this parameter as an option for a slash command
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class OptionAttribute : Attribute
{
	/// <summary>
	/// Gets the name of this option.
	/// </summary>
	public string Name;

	/// <summary>
	/// Gets the description of this option.
	/// </summary>
	public string Description;

	/// <summary>
	/// Whether to autocomplete this option.
	/// </summary>
	public bool Autocomplete;

	/// <summary>
	/// Initializes a new instance of the <see cref="OptionAttribute"/> class.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="autocomplete">If true, autocomplete.</param>
	public OptionAttribute(string name, string description, bool autocomplete = false)
	{
		if (name.Length > 32)
			throw new ArgumentException("Slash command option names cannot go over 32 characters.");
		else if (description.Length > 100)
			throw new ArgumentException("Slash command option descriptions cannot go over 100 characters.");

		this.Name = name.ToLower();
		this.Description = description;
		this.Autocomplete = autocomplete;
	}
}
