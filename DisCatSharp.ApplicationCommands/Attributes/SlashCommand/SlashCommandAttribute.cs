// This file is part of the DisCatSharp project.
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Marks this method as a slash command
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SlashCommandAttribute : Attribute
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
	/// Gets the allowed contexts of this command
	/// </summary>
	public ApplicationCommandContexts? AllowedContexts { get; set; }

	/// <summary>
	/// Gets the dm permission of this command
	/// </summary>
	public bool? DmPermission { get; set; }

	/// <summary>
	/// Gets whether this command is marked as NSFW
	/// </summary>
	public bool IsNsfw { get; set; }

	/// <summary>
	/// Marks this method as a slash command
	/// </summary>
	/// <param name="name">The name of this slash command.</param>
	/// <param name="description">The description of this slash command.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of this slash command.</param>
	public SlashCommandAttribute(string name, string description, bool isNsfw = false, int allowedContexts = 0)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = null;
		this.DmPermission = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts != 0 ? (ApplicationCommandContexts)allowedContexts : null;
	}

	/// <summary>
	/// Marks this method as a slash command
	/// </summary>
	/// <param name="name">The name of this slash command.</param>
	/// <param name="description">The description of this slash command.</param>
	/// <param name="defaultMemberPermissions">The default member permissions.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of this slash command.</param>
	public SlashCommandAttribute(string name, string description, long defaultMemberPermissions, bool isNsfw = false, int allowedContexts = 0)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.DmPermission = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts != 0 ? (ApplicationCommandContexts)allowedContexts : null;
	}

	/// <summary>
	/// Marks this method as a slash command
	/// </summary>
	/// <param name="name">The name of this slash command.</param>
	/// <param name="description">The description of this slash command.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of this slash command.</param>
	public SlashCommandAttribute(string name, string description, bool dmPermission, bool isNsfw = false, int allowedContexts = 0)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = null;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts != 0 ? (ApplicationCommandContexts)allowedContexts : null;
	}

	/// <summary>
	/// Marks this method as a slash command
	/// </summary>
	/// <param name="name">The name of this slash command.</param>
	/// <param name="description">The description of this slash command.</param>
	/// <param name="defaultMemberPermissions">The default member permissions.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of this slash command.</param>
	public SlashCommandAttribute(string name, string description, long defaultMemberPermissions, bool dmPermission, bool isNsfw = false, int allowedContexts = 0)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts != 0 ? (ApplicationCommandContexts)allowedContexts : null;
	}
}
