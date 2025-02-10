using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a command that is registered to an application.
/// </summary>
public class DiscordApplicationCommand : SnowflakeObject, IEquatable<DiscordApplicationCommand>
{
	/// <summary>
	///     Creates a new instance of a <see cref="DiscordApplicationCommand" />.
	/// </summary>
	/// <param name="name">The name of the command.</param>
	/// <param name="description">The description of the command.</param>
	/// <param name="options">Optional parameters for this command.</param>
	/// <param name="type">The type of the command. Defaults to ChatInput.</param>
	/// <param name="nameLocalizations">The localizations of the command name.</param>
	/// <param name="descriptionLocalizations">The localizations of the command description.</param>
	/// <param name="defaultMemberPermissions">The default member permissions.</param>
	/// <param name="isNsfw">Whether this command is NSFW.</param>
	/// <param name="allowedContexts">Where the command can be used.</param>
	/// <param name="integrationTypes">The allowed integration types.</param>
	/// <param name="handlerType">The handler type.</param>
	public DiscordApplicationCommand(
		string name,
		string? description,
		IEnumerable<DiscordApplicationCommandOption>? options = null,
		ApplicationCommandType type = ApplicationCommandType.ChatInput,
		DiscordApplicationCommandLocalization? nameLocalizations = null,
		DiscordApplicationCommandLocalization? descriptionLocalizations = null,
		Permissions? defaultMemberPermissions = null,
		bool isNsfw = false,
		List<InteractionContextType>? allowedContexts = null,
		List<ApplicationCommandIntegrationTypes>? integrationTypes = null,
		ApplicationCommandHandlerType? handlerType = null
	)
		: base(["guild_id", "name_localizations", "description_localizations"])
	{
		if (type is ApplicationCommandType.ChatInput)
		{
			if (!Utilities.IsValidSlashCommandName(name))
				throw new ArgumentException($"Invalid slash command name specified. It must be below 32 characters and not contain any whitespace. Error for command {name}.", nameof(name));
			if (name.Any(char.IsUpper))
				throw new ArgumentException($"Slash command name cannot have any upper case characters. Error for command {name}.", nameof(name));
			if (description?.Length > 100)
				throw new ArgumentException($"Slash command description cannot exceed 100 characters. Error for command {name}.", nameof(description));
			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentException($"Slash commands need a description. Error for command {name}.", nameof(description));

			this.RawNameLocalizations = nameLocalizations?.GetKeyValuePairs();
			this.RawDescriptionLocalizations = descriptionLocalizations?.GetKeyValuePairs();
			this.Type = type;
			this.Name = name;
			this.Description = description;
			this.Options = options != null && options.Any() ? options.ToList() : null;
			this.DefaultMemberPermissions = defaultMemberPermissions;
			this.IsNsfw = isNsfw;
			this.AllowedContexts = allowedContexts;
			this.IntegrationTypes = integrationTypes;
		}
		else if (type is ApplicationCommandType.PrimaryEntryPoint)
		{
			if (!Utilities.IsValidSlashCommandName(name))
				throw new ArgumentException($"Invalid slash command name specified. It must be below 32 characters and not contain any whitespace. Error for command {name}.", nameof(name));
			if (name.Any(char.IsUpper))
				throw new ArgumentException($"Slash command name cannot have any upper case characters. Error for command {name}.", nameof(name));
			if (description?.Length > 100)
				throw new ArgumentException($"Slash command description cannot exceed 100 characters. Error for command {name}.", nameof(description));
			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentException($"Slash commands need a description. Error for command {name}.", nameof(description));
			if (options?.Any() ?? false)
				throw new ArgumentException($"Primary entrypoints do not support options. Error for command {name}.");

			this.RawNameLocalizations = nameLocalizations?.GetKeyValuePairs();
			this.RawDescriptionLocalizations = descriptionLocalizations?.GetKeyValuePairs();
			this.Type = type;
			this.Name = name;
			this.Description = description;
			this.Options = null;
			this.DefaultMemberPermissions = defaultMemberPermissions;
			this.IsNsfw = isNsfw;
			this.AllowedContexts = allowedContexts;
			this.IntegrationTypes = integrationTypes;
			this.HandlerType = handlerType;
		}
		else
		{
			if (!string.IsNullOrWhiteSpace(description))
				throw new ArgumentException($"Context menus do not support descriptions. Error for command {name}.");
			if (options?.Any() ?? false)
				throw new ArgumentException($"Context menus do not support options. Error for command {name}.");

			description = string.Empty;

			this.RawNameLocalizations = nameLocalizations?.GetKeyValuePairs();
			this.Type = type;
			this.Name = name;
			this.Description = description;
			this.Options = null;
			this.DefaultMemberPermissions = defaultMemberPermissions;
			this.IsNsfw = isNsfw;
			this.AllowedContexts = allowedContexts;
			this.IntegrationTypes = integrationTypes;
		}
	}

	/// <summary>
	///     Creates a new empty Discord Application Command.
	/// </summary>
	internal DiscordApplicationCommand()
		: base(["name_localizations", "description_localizations", "guild_id"]) // Why tf is that so inconsistent?!
	{ }

	/// <summary>
	///     Gets the type of this application command.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandType Type { get; internal set; }

	/// <summary>
	///     Gets the unique ID of this command's application.
	/// </summary>
	[JsonProperty("application_id")]
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	///     Gets the name of this command.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	///     Sets the name localizations.
	/// </summary>
	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<string, string>? RawNameLocalizations { get; set; }

	/// <summary>
	///     Gets the name localizations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? NameLocalizations
		=> this.RawNameLocalizations != null ? new(this.RawNameLocalizations) : null;

	/// <summary>
	///     Gets the description of this command.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	///     Sets the description localizations.
	/// </summary>
	[JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<string, string>? RawDescriptionLocalizations { get; set; }

	/// <summary>
	///     Gets the description localizations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? DescriptionLocalizations
		=> this.RawDescriptionLocalizations != null ? new(this.RawDescriptionLocalizations) : null;

	/// <summary>
	///     Gets the potential parameters for this command.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordApplicationCommandOption>? Options { get; internal set; } = null;

	/// <summary>
	///     Gets the commands needed permissions.
	/// </summary>
	[JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? DefaultMemberPermissions { get; internal set; } = null;

	/// <summary>
	///     Gets whether the command can be used in direct messages.
	/// </summary>
	[JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Ignore), DiscordDeprecated("Replaced by AllowedContexts"), Obsolete("Replaced by AllowedContexts", true)]
	public bool? DmPermission { get; set; }

	/// <summary>
	///     Gets where the application command can be used.
	/// </summary>
	[JsonProperty("contexts", NullValueHandling = NullValueHandling.Ignore), DiscordUnreleased]
	public List<InteractionContextType>? AllowedContexts { get; internal set; }

	/// <summary>
	///     Gets the application command allowed integration types.
	/// </summary>
	[JsonProperty("integration_types", NullValueHandling = NullValueHandling.Ignore), DiscordUnreleased]
	public List<ApplicationCommandIntegrationTypes>? IntegrationTypes { get; internal set; }

	/// <summary>
	///     Gets whether the command is marked as NSFW.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsNsfw { get; internal set; } = false;

	/// <summary>
	///     Gets the version number for this command.
	/// </summary>
	[JsonProperty("version")]
	public ulong Version { get; internal set; }

	[JsonProperty("handler", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandHandlerType? HandlerType { get; internal set; }

	/// <summary>
	///     Gets the mention for this command.
	/// </summary>
	[JsonIgnore]
	public string Mention
		=> this.Type == ApplicationCommandType.ChatInput ? $"</{this.Name}:{this.Id}>" : this.Name;

	/// <summary>
	///     Checks whether this <see cref="DiscordApplicationCommand" /> object is equal to another object.
	/// </summary>
	/// <param name="other">The command to compare to.</param>
	/// <returns>Whether the command is equal to this <see cref="DiscordApplicationCommand" />.</returns>
	public bool Equals(DiscordApplicationCommand other)
		=> this.Id == other.Id;

	/// <summary>
	///     Determines if two <see cref="DiscordApplicationCommand" /> objects are equal.
	/// </summary>
	/// <param name="e1">The first command object.</param>
	/// <param name="e2">The second command object.</param>
	/// <returns>Whether the two <see cref="DiscordApplicationCommand" /> objects are equal.</returns>
	public static bool operator ==(DiscordApplicationCommand e1, DiscordApplicationCommand e2)
		=> e1.Equals(e2);

	/// <summary>
	///     Determines if two <see cref="DiscordApplicationCommand" /> objects are not equal.
	/// </summary>
	/// <param name="e1">The first command object.</param>
	/// <param name="e2">The second command object.</param>
	/// <returns>Whether the two <see cref="DiscordApplicationCommand" /> objects are not equal.</returns>
	public static bool operator !=(DiscordApplicationCommand e1, DiscordApplicationCommand e2)
		=> !(e1 == e2);

	/// <summary>
	///     Determines if a <see cref="object" /> is equal to the current <see cref="DiscordApplicationCommand" />.
	/// </summary>
	/// <param name="other">The object to compare to.</param>
	/// <returns>Whether the two <see cref="DiscordApplicationCommand" /> objects are not equal.</returns>
	public override bool Equals(object other)
		=> other is DiscordApplicationCommand dac && this.Equals(dac);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordApplicationCommand" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordApplicationCommand" />.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();
}
