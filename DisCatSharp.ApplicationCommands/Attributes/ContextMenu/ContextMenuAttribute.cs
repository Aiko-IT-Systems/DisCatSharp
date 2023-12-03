using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Represents a <see cref="DisCatSharp.Entities.DiscordApplicationCommand"/> with the type of <see cref="ApplicationCommandType.User"/> or <see cref="ApplicationCommandType.Message"/>.
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
	/// Gets this context menu's needed permissions.
	/// </summary>
	public Permissions? DefaultMemberPermissions { get; internal set; }

	/// <summary>
	/// Gets the allowed contexts of this context menu.
	/// </summary>
	public List<ApplicationCommandContexts>? AllowedContexts { get; set; }

	/// <summary>
	/// Gets the allowed integration types of this context menu.
	/// </summary>
	public List<ApplicationCommandIntegrationTypes>? IntegrationTypes { get; set; }

	/// <summary>
	/// Gets whether this context menu can be used in direct messages.
	/// </summary>
	public bool? DmPermission { get; set; }

	/// <summary>
	/// Gets whether this context menu is marked as NSFW.
	/// </summary>
	public bool IsNsfw { get; set; }

	/// <summary>
	/// Marks this method as a context menu.
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="isNsfw">Whether the context menu is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	/// <param name="integrationTypes">The allowed integration types of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		if (type == ApplicationCommandType.ChatInput)
			throw new ArgumentException("Context menus cannot be of type ChatInput (Slash).");

		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = null;
		this.DmPermission = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}

	/// <summary>
	/// Marks this method as a context menu.
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="defaultMemberPermissions">The default member permissions of the context menu.</param>
	/// <param name="isNsfw">Whether the context menu is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	/// <param name="integrationTypes">The allowed integration types of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, long defaultMemberPermissions, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		if (type == ApplicationCommandType.ChatInput)
			throw new ArgumentException("Context menus cannot be of type ChatInput (Slash).");

		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.DmPermission = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}

	/// <summary>
	/// Marks this method as context menu.
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="dmPermission">The dm permission of the context menu.</param>
	/// <param name="isNsfw">Whether the context menu is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	/// <param name="integrationTypes">The allowed integration types of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, bool dmPermission, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		if (type == ApplicationCommandType.ChatInput)
			throw new ArgumentException("Context menus cannot be of type ChatInput (Slash).");

		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = null;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}

	/// <summary>
	/// Marks this method as a context menu.
	/// </summary>
	/// <param name="type">The type of the context menu.</param>
	/// <param name="name">The name of the context menu.</param>
	/// <param name="defaultMemberPermissions">The default member permissions of the context menu.</param>
	/// <param name="dmPermission">The dm permission of the context menu.</param>
	/// <param name="isNsfw">Whether the context menu is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the context menu.</param>
	/// <param name="integrationTypes">The allowed integration types of the context menu.</param>
	public ContextMenuAttribute(ApplicationCommandType type, string name, long defaultMemberPermissions, bool dmPermission, bool isNsfw = false, ApplicationCommandContexts[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		if (type == ApplicationCommandType.ChatInput)
			throw new ArgumentException("Context menus cannot be of type ChatInput (Slash).");

		this.Type = type;
		this.Name = name;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.DmPermission = dmPermission;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}
}
