using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
///     Represents a <see cref="DisCatSharp.Entities.DiscordApplicationCommand" />.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SlashCommandAttribute : Attribute
{
	/// <summary>
	///     Marks this method as a slash command.
	/// </summary>
	/// <param name="name">The name of this slash command.</param>
	/// <param name="description">The description of this slash command.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of this slash command.</param>
	/// <param name="integrationTypes">The allowed integration types.</param>
	public SlashCommandAttribute(string name, string description, bool isNsfw = false, InteractionContextType[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = null;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}

	/// <summary>
	///     Marks this method as a slash command.
	/// </summary>
	/// <param name="name">The name of this slash command.</param>
	/// <param name="description">The description of this slash command.</param>
	/// <param name="defaultMemberPermissions">The default member permissions.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts of this slash command.</param>
	/// <param name="integrationTypes">The allowed integration types.</param>
	public SlashCommandAttribute(string name, string description, long defaultMemberPermissions, bool isNsfw = false, InteractionContextType[]? allowedContexts = null, ApplicationCommandIntegrationTypes[]? integrationTypes = null)
	{
		this.Name = name.ToLower();
		this.Description = description;
		this.DefaultMemberPermissions = (Permissions)defaultMemberPermissions;
		this.IsNsfw = isNsfw;
		this.AllowedContexts = allowedContexts?.ToList();
		this.IntegrationTypes = integrationTypes?.ToList();
	}

	/// <summary>
	///     Gets the name of this command.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///     Gets the description of this command.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///     Gets the needed permission of this command.
	/// </summary>
	public Permissions? DefaultMemberPermissions { get; set; }

	/// <summary>
	///     Gets the allowed contexts of this command.
	/// </summary>
	public List<InteractionContextType>? AllowedContexts { get; set; }

	/// <summary>
	///     Gets the allowed integration types of this command.
	/// </summary>
	public List<ApplicationCommandIntegrationTypes>? IntegrationTypes { get; set; }

	/// <summary>
	///     Gets whether this command is marked as NSFW.
	/// </summary>
	public bool IsNsfw { get; set; }
}
