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

using System.IO;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a role edit model.
/// </summary>
public class RoleEditModel : BaseEditModel
{
	/// <summary>
	/// New role name
	/// </summary>
	public string Name { internal get; set; }

	/// <summary>
	/// New role permissions
	/// </summary>
	public Permissions? Permissions { internal get; set; }

	/// <summary>
	/// New role color
	/// </summary>
	public DiscordColor? Color { internal get; set; }

	/// <summary>
	/// Whether new role should be hoisted (Shown in the sidebar)
	/// </summary>
	public bool? Hoist { internal get; set; }

	/// <summary>
	/// Whether new role should be mentionable
	/// </summary>
	public bool? Mentionable { internal get; set; }

	/// <summary>
	/// The new role icon.
	/// </summary>
	public Optional<Stream> Icon { internal get; set; }

	/// <summary>
	/// The new role icon from unicode emoji.
	/// </summary>
	public Optional<DiscordEmoji> UnicodeEmoji { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RoleEditModel"/> class.
	/// </summary>
	internal RoleEditModel()
	{
		this.Name = null;
		this.Permissions = null;
		this.Color = null;
		this.Hoist = null;
		this.Mentionable = null;
		this.Icon = null;
		this.UnicodeEmoji = null;
	}
}
