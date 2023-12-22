using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

internal sealed class CommandGroupWithSubGroups : BaseCommand
{
	[JsonProperty("groups")]
	internal List<CommandGroup> SubGroups { get; set; }

	internal CommandGroupWithSubGroups(string name, string description, List<CommandGroup> commands, ApplicationCommandType type)
		: base(name, description, type)
	{
		this.SubGroups = commands;
	}
}

internal sealed class CommandGroup : BaseCommand
{
	[JsonProperty("commands")]
	internal List<Command> Commands { get; set; }

	internal CommandGroup(string name, string description, List<Command> commands, ApplicationCommandType? type = null)
		: base(name, description, type)
	{
		this.Commands = commands;
	}
}

internal sealed class Command : BaseCommand
{
	[JsonProperty("options")]
	internal List<DiscordApplicationCommandOption>? Options { get; set; }

	internal Command(string name, string? description = null, List<DiscordApplicationCommandOption>? options = null, ApplicationCommandType? type = null)
		: base(name, description, type)
	{
		this.Options = options;
	}
}

internal class BaseCommand
{
	[JsonProperty("name")]
	internal string Name { get; set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	internal string? Description { get; set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	internal ApplicationCommandType? Type { get; set; }

	internal BaseCommand(string name, string? description = null, ApplicationCommandType? type = null)
	{
		this.Name = name;
		this.Type = type;
		this.Description = description;
	}
}
