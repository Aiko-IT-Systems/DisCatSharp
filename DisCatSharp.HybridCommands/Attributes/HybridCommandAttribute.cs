// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using DisCatSharp.Enums;

namespace DisCatSharp.HybridCommands.Attributes;

/// <summary>
/// Marks this method as a hybrid command
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HybridCommandAttribute : Attribute
{
	/// <summary>
	/// Gets the name of this command
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets the description of this command
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Gets the needed permission of this command
	/// </summary>
	public Permissions? DefaultMemberPermissions { get; set; }

	/// <summary>
	/// Gets the dm permission of this command
	/// </summary>
	public bool? DmPermission { get; set; }

	/// <summary>
	/// Gets whether this command is marked as NSFW
	/// </summary>
	public bool IsNsfw { get; set; }

	/// <summary>
	/// Marks this method as hybrid command.
	/// </summary>
	/// <param name="name">The name of this hybrid command.</param>
	/// <param name="description">The description of this hybrid command.</param>
	/// <param name="defaultMemberPermissions">
	///		<para>The default permissions required for execution.</para>
	///		<para>This can be overriden by a guild and is not enforced via prefix commands.</para>
	///	</param>
	/// <param name="dmPermission">Whether this command should be available in direct messages.</param>
	/// <param name="isNsfw">
	///		<para>Whether this command should be marked as nsfw.</para>
	///		<para>Does not protect this command from being ran as prefix command from users under 18.</para>
	/// </param>
	public HybridCommandAttribute(string name, string description, Permissions? defaultMemberPermissions = null, bool dmPermission = true, bool isNsfw = false)
	{
		this.Name = name;
		this.Description = description;
		this.DefaultMemberPermissions = defaultMemberPermissions;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
	}

	/// <summary>
	/// Marks this method as hybrid command.
	/// </summary>
	/// <param name="name">The name of this hybrid command.</param>
	/// <param name="description">The description of this hybrid command.</param>
	/// <param name="dmPermission">Whether this command should be available in direct messages.</param>
	/// <param name="isNsfw">
	///		<para>Whether this command should be marked as nsfw.</para>
	///		<para>Does not protect this command from being ran as prefix command from users under 18.</para>
	/// </param>
	public HybridCommandAttribute(string name, string description, bool dmPermission, bool isNsfw = false)
	{
		this.Name = name;
		this.Description = description;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
	}
}
