// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a application command edit model.
/// </summary>
public class ApplicationCommandEditModel
{
	/// <summary>
	/// Sets the command's new name.
	/// </summary>
	public Optional<string> Name
	{
		internal get => this._name;
		set
		{
			if (value.Value.Length > 32)
				throw new ArgumentException("Application command name cannot exceed 32 characters.", nameof(value));
			this._name = value;
		}
	}
	private Optional<string> _name;

	/// <summary>
	/// Sets the command's new description
	/// </summary>
	public Optional<string> Description
	{
		internal get => this._description;
		set
		{
			if (value.Value.Length > 100)
				throw new ArgumentException("Application command description cannot exceed 100 characters.", nameof(value));
			this._description = value;
		}
	}
	private Optional<string> _description;

	/// <summary>
	/// Sets the command's name localizations.
	/// </summary>
	public Optional<DiscordApplicationCommandLocalization> NameLocalizations { internal get; set; }

	/// <summary>
	/// Sets the command's description localizations.
	/// </summary>
	public Optional<DiscordApplicationCommandLocalization> DescriptionLocalizations { internal get; set; }

	/// <summary>
	/// Sets the command's new options.
	/// </summary>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
	public Optional<List<DiscordApplicationCommandOption>?> Options { internal get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

	/// <summary>
	/// Sets the command's needed permissions.
	/// </summary>
	public Optional<Permissions?> DefaultMemberPermissions { internal get; set; }

	/// <summary>
	/// Sets whether the command can be used in direct messages.
	/// </summary>
	public Optional<bool> DmPermission { internal get; set; }

	/// <summary>
	/// Sets whether the command is marked as NSFW.
	/// </summary>
	public Optional<bool> IsNsfw { internal get; set; }
}
