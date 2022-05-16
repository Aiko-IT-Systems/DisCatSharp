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

namespace DisCatSharp.CommandsNext.Exceptions;

/// <summary>
/// Thrown when the command service fails to find a command.
/// </summary>
public sealed class CommandNotFoundException : Exception
{
	/// <summary>
	/// Gets the name of the command that was not found.
	/// </summary>
	public string CommandName { get; set; }

	/// <summary>
	/// Creates a new <see cref="CommandNotFoundException"/>.
	/// </summary>
	/// <param name="command">Name of the command that was not found.</param>
	public CommandNotFoundException(string command)
		: base("Specified command was not found.")
	{
		this.CommandName = command;
	}

	/// <summary>
	/// Returns a string representation of this <see cref="CommandNotFoundException"/>.
	/// </summary>
	/// <returns>A string representation.</returns>
	public override string ToString() => $"{this.GetType()}: {this.Message}\nCommand name: {this.CommandName}"; // much like System.ArgumentNullException works
}
