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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

internal class CommandGroupWithSubGroups : BaseCommand
{
	[JsonProperty("groups")]
	internal List<CommandGroup> SubGroups { get; set; }

	internal CommandGroupWithSubGroups(string name, string description, List<CommandGroup> commands, ApplicationCommandType type)
		: base(name, description, type)
	{
		this.SubGroups = commands;
	}
}

internal class CommandGroup : BaseCommand
{
	[JsonProperty("commands")]
	internal List<Command> Commands { get; set; }

	internal CommandGroup(string name, string description, List<Command> commands, ApplicationCommandType? type = null)
		: base(name, description, type)
	{
		this.Commands = commands;
	}
}

internal class Command : BaseCommand
{
	[JsonProperty("options")]
	internal List<DiscordApplicationCommandOption> Options { get; set; }

	internal Command(string name, string? description = null, List<DiscordApplicationCommandOption> options = null, ApplicationCommandType? type = null)
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
