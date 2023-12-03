using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a application command edit model.
/// </summary>
public class ApplicationCommandEditModel : ObservableApiObject
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
	public Optional<List<DiscordApplicationCommandOption>?> Options { internal get; set; }

	/// <summary>
	/// Sets the command's needed permissions.
	/// </summary>
	public Optional<Permissions?> DefaultMemberPermissions { internal get; set; }

	/// <summary>
	/// Sets the command's allowed contexts.
	/// </summary>
	public Optional<List<ApplicationCommandContexts>?> AllowedContexts { internal get; set; }

	/// <summary>
	/// Sets the command's allowed integration types.
	/// </summary>
	public Optional<List<ApplicationCommandIntegrationTypes>?> IntegrationTypes { internal get; set; }

	/// <summary>
	/// Sets whether the command can be used in direct messages.
	/// </summary>
	public Optional<bool> DmPermission { internal get; set; }

	/// <summary>
	/// Sets whether the command is marked as NSFW.
	/// </summary>
	public Optional<bool> IsNsfw { internal get; set; }
}
