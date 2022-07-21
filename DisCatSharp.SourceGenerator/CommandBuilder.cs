// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.SourceGenerator.Models;

namespace DisCatSharp.SourceGenerator;

public static class CommandBuilder
{
	public static string Build(CommandInfo command, ref HashSet<string> requiredNamespaces)
	{
		using var stringWriter = new StringWriter();
		using var writer = new IndentedTextWriter(stringWriter);
		writer.Indent = 1;

		/*
			Our primary endpoint interaction will get generated. This shall be the command that gets registered
			on discord.

			The method which handles the interaction will be virtual so the developer can focus on how to implement this interaction
			versus managing the attribute soup
		 */

		writer.WriteLine($@"[SlashCommand(""{command.Name}"",""{command.Description}"")]");

		if (command.RequiredPermissions.Any())
			writer.WriteLine($@"[ApplicationCommandRequirePermissions({string.Join(" | ", command.RequiredPermissions)})]");

		writer.Write($@"private async Task {command.SanitizedName}Async(InteractionContext context");

		List<string> plainParameters = new();
		foreach (var param in command.Parameters)
		{
			var parameterDefinition = string.Empty;

			List<string> attributes = new();

			if (!string.IsNullOrEmpty(param.AutoCompleteProviderType))
			{
				attributes.Add($@"Autocomplete(typeof({param.AutoCompleteProviderType}))");
				attributes.Add($@"Option(""{param.Name}"", ""{param.Description}"", true)");

				if (!requiredNamespaces.Contains("DisCatSharp.ApplicationCommands.Attributes"))
					requiredNamespaces.Add("DisCatSharp.ApplicationCommands.Attributes");
			}
			else
				attributes.Add($@"Option(""{param.Name}"", ""{param.Description}"")");

			if (param.Type.Equals("discorduser", StringComparison.OrdinalIgnoreCase))
				parameterDefinition += "DiscordUser";
			else if (param.Type.Equals("discordattachment", StringComparison.OrdinalIgnoreCase))
				parameterDefinition += "DiscordAttachment";
			else
				parameterDefinition += param.Type;

			parameterDefinition += " " + param.SanitizedName;
			plainParameters.Add(parameterDefinition);

			writer.Write($", [{string.Join(", ", attributes)}] {parameterDefinition}");
		}

		writer.WriteLine(")");

		var methodHandle = $"HandleSlashInteraction_{command.SanitizedName}";

		writer.WriteLine("{");
		writer.Indent++;
		writer.WriteLine($"await {methodHandle}(context, {string.Join(",", command.Parameters.Select(x=>x.SanitizedName))});");
		writer.Indent--;
		writer.WriteLine("}");

		writer.WriteLine("\n\n");

		writer.Write($"public abstract Task {methodHandle}(InteractionContext context");

		if (plainParameters.Any())
			writer.WriteLine($", {string.Join(", ", plainParameters)});");
		else
			writer.WriteLine(");");

		writer.Flush();
		return stringWriter.ToString();
	}
}
