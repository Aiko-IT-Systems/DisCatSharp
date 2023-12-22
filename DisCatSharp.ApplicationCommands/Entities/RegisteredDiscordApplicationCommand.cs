using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DisCatSharp.Entities;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a discord application command registered by the ApplicationCommands extensions.
/// </summary>
public sealed class RegisteredDiscordApplicationCommand : DiscordApplicationCommand
{
	/// <summary>
	/// Creates a new empty registered discord application command.
	/// </summary>
	internal RegisteredDiscordApplicationCommand()
	{ }

	/// <summary>
	/// Creates a new registered discord application command to control a dildo. (Lala told me to leave it)
	/// </summary>
	/// <param name="parent"></param>
	internal RegisteredDiscordApplicationCommand(DiscordApplicationCommand parent)
	{
		this.AdditionalProperties = parent.AdditionalProperties;
		this.ApplicationId = parent.ApplicationId;
		this.DefaultMemberPermissions = parent.DefaultMemberPermissions;
		this.Description = parent.Description;
		this.Discord = parent.Discord;
		this.DmPermission = parent.DmPermission;
		this.Id = parent.Id;
		this.IgnoredJsonKeys = parent.IgnoredJsonKeys;
		this.IsNsfw = parent.IsNsfw;
		this.Name = parent.Name;
		this.Options = parent.Options;
		this.RawDescriptionLocalizations = parent.RawDescriptionLocalizations;
		this.RawNameLocalizations = parent.RawNameLocalizations;
		this.Type = parent.Type;
		this.UnknownProperties = parent.UnknownProperties;
		this.Version = parent.Version;

		try
		{
			if (ApplicationCommandsExtension.CommandMethods.Any(x => x.CommandId == this.Id))
			{
				this.CommandMethod = ApplicationCommandsExtension.CommandMethods.First(x => x.CommandId == this.Id).Method;
				this.ContainingType = this.CommandMethod.DeclaringType;
				this.CustomAttributes = this.CommandMethod.GetCustomAttributes().Where(x => !x.GetType().Namespace.StartsWith("DisCatSharp", StringComparison.Ordinal)).ToList();
			}
			else if (ApplicationCommandsExtension.ContextMenuCommands.Any(x => x.CommandId == this.Id))
			{
				this.CommandMethod = ApplicationCommandsExtension.ContextMenuCommands.First(x => x.CommandId == this.Id).Method;
				this.ContainingType = this.CommandMethod.DeclaringType;
				this.CustomAttributes = this.CommandMethod.GetCustomAttributes().Where(x => !x.GetType().Namespace.StartsWith("DisCatSharp", StringComparison.Ordinal)).ToList();
			}
			else if (ApplicationCommandsExtension.GroupCommands.Any(x => x.CommandId == this.Id))
			{
				this.CommandType = ApplicationCommandsExtension.GroupCommands.First(x => x.CommandId == this.Id).Methods.First().Value.DeclaringType;
				this.ContainingType = this.CommandType.DeclaringType;
				this.CustomAttributes = this.CommandType.GetCustomAttributes().Where(x => !x.GetType().Namespace.StartsWith("DisCatSharp", StringComparison.Ordinal)).ToList();
			}
		}
		catch (Exception)
		{
			ApplicationCommandsExtension.Logger.LogError("Failed to generate reflection properties for '{cmd}'", parent.Name);
		}
	}

	/// <summary>
	/// The method that will be executed when somebody runs this command.
	/// <see langword="null"/> if command is a group command or reflection failed.
	/// </summary>
	public MethodInfo? CommandMethod { get; internal set; }

	/// <summary>
	/// The type that contains the sub commands of this command.
	/// <see langword="null"/> if command is not a group command or reflection failed.
	/// </summary>
	public Type? CommandType { get; internal set; }

	/// <summary>
	/// The type this command is contained in.
	/// <see langword="null"/> if reflection failed.
	/// </summary>
	public Type? ContainingType { get; internal set; }

	/// <summary>
	/// Gets all Non-DisCatSharp attributes this command has.
	/// <see langword="null"/> if reflection failed.
	/// </summary>
	public IReadOnlyList<Attribute>? CustomAttributes { get; internal set; }
}
