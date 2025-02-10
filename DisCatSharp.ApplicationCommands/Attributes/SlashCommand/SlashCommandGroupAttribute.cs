using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
///     Represents a <see cref="DisCatSharp.Entities.DiscordApplicationCommand" /> group.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SlashCommandGroupAttribute : Attribute
{
	/// <summary>
	///     Marks this class as a slash command group.
	/// </summary>
	/// <param name="name">The name of the slash command group.</param>
	/// <param name="description">The description of the slash command group.</param>
	/// <param name="isNsfw">Whether the slash command group is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the slash command group.</param>
	/// <param name="integrationTypes">The allowed integration types of the slash command group.</param>
	public SlashCommandGroupAttribute(string name, string description, bool isNsfw = false, InteractionContextType[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}

	/// <summary>
	///     Marks this method as a slash command group.
	/// </summary>
	/// <param name="name">The name of the slash command group.</param>
	/// <param name="description">The description of the slash command group.</param>
	/// <param name="defaultMemberPermissions">The default member permissions of the slash command group.</param>
	/// <param name="isNsfw">Whether the slash command group is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of the slash command group.</param>
	/// <param name="integrationTypes">The allowed integration types of the slash command group.</param>
	public SlashCommandGroupAttribute(string name, string description, long defaultMemberPermissions, bool isNsfw = false, InteractionContextType[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}

	/// <summary>
	///     Gets the name of this slash command group.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///     Gets the description of this slash command group.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///     Gets the needed permission of this slash command group.
	/// </summary>
	public Permissions? DefaultMemberPermissions { get; set; }

	/// <summary>
	///     Gets the allowed contexts of this slash command group.
	/// </summary>
	public List<InteractionContextType>? AllowedContexts { get; set; }

	/// <summary>
	///     Gets the allowed integration types of this slash command group.
	/// </summary>
	public List<ApplicationCommandIntegrationTypes>? IntegrationTypes { get; set; }

	/// <summary>
	///     Gets whether this slash command group is marked as NSFW.
	/// </summary>
	public bool IsNsfw { get; set; }
}
