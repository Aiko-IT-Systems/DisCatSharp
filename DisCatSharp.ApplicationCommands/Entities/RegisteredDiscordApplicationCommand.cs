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
using System.Reflection;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a discord application command registered by the ApplicationCommands extensions.
/// </summary>
public sealed class RegisteredDiscordApplicationCommand : DiscordApplicationCommand
{
	/// <summary>
	/// Creates a new empty registered discord application command.
	/// </summary>
	internal RegisteredDiscordApplicationCommand() { }

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
	}

	/// <summary>
	/// The method that will be executed when somebody runs this command.
	/// </summary>
	public MethodInfo CommandMethod
	{
		get
		{
			var methodInfo = ApplicationCommandsExtension.s_commandMethods.FirstOrDefault(x => x.CommandId == this.Id, null)?.Method;
			methodInfo ??= ApplicationCommandsExtension.s_contextMenuCommands.FirstOrDefault(x => x.CommandId == this.Id, null)?.Method;
			methodInfo ??= ApplicationCommandsExtension.s_groupCommands.FirstOrDefault(x => x.CommandId == this.Id, null)?.Methods.First().Value;

			return methodInfo;
		}
	}


	/// <summary>
	/// The type this command is contained in.
	/// </summary>
	public Type ContainingType
		=> this.CommandMethod.DeclaringType;

	/// <summary>
	/// Gets all Non-DisCatSharp attributes this command has.
	/// </summary>
	public IReadOnlyList<Attribute> CustomAttributes
	{
		get
		{
			this._customAttributes ??= this.CommandMethod.GetCustomAttributes().Where(x => !x.GetType().Namespace.StartsWith("DisCatSharp")).ToList();
			return this._customAttributes.AsReadOnly();
		}
	}

	private List<Attribute> _customAttributes = null;
}
