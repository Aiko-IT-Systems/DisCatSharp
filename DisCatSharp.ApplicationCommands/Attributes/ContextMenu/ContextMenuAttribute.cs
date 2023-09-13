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
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Marks this method as a context menu.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ContextMenuAttribute : Attribute
{
	/// <summary>
	/// Gets the name of this context menu.
	/// </summary>
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the type of this context menu.
	/// </summary>
	public ApplicationCommandType Type { get; internal set; }

	/// <summary>
	/// Gets the commands needed permissions.
	/// </summary>
	public Permissions? DefaultMemberPermissions { get; internal set; }

	/// <summary>
	/// Gets the allowed contexts of this command
	/// </summary>
	public List<ApplicationCommandContexts>? AllowedContexts { get; set; }


	/// <summary>
	/// Gets whether the command can be used in direct messages.
	/// </summary>
	public bool? DmPermission { get; set; }

	/// <summary>
	/// Gets whether this command is marked as NSFW
	/// </summary>
	public bool IsNsfw { get; set; }

	/// <summary>
	/// Marks this method as a context menu.
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="isNsfw">Whether this context menu command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null)
	{
		if (type == ApplicationCommandType.ChatInput)
			throw new ArgumentException("Context menus cannot be of type ChatInput (Slash).");

		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = null;
		this.DmPermission = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
	}


	/// <summary>
	/// Marks this method as a context menu.
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="defaultMemberPermissions">The default member permissions.</param>
	/// <param name="isNsfw">Whether this context menu command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, long defaultMemberPermissions, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null)
	{
		if (type == ApplicationCommandType.ChatInput)
			throw new ArgumentException("Context menus cannot be of type ChatInput (Slash).");

		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.DmPermission = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
	}


	/// <summary>
	/// Marks this method as a slash command
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, bool dmPermission, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null)
	{
		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = null;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
	}

	/// <summary>
	/// Marks this method as a slash command
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="defaultMemberPermissions">The default member permissions.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, long defaultMemberPermissions, bool dmPermission, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null)
	{
		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
	}
}
