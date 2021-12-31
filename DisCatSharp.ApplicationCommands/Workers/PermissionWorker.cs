// This file is part of the DisCatSharp project, based of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.ApplicationCommands
{
	/// <summary>
	/// Represents a <see cref="PermissionWorker"/>.
	/// </summary>
	internal class PermissionWorker
	{
		internal static async Task UpdateCommandPermissionAsync(IEnumerable<ApplicationCommandsModuleConfiguration> types, ulong? guildid, ulong commandId, string commandName, Type commandDeclaringType, Type commandRootType)
		{
			if (!guildid.HasValue)
			{
				ApplicationCommandsExtension._client.Logger.LogTrace("You can't set global permissions till yet. See https://discord.com/developers/docs/interactions/application-commands#permissions");
				return;
			}

			var ctx = new ApplicationCommandsPermissionContext(commandDeclaringType, commandName);
			var conf = types.First(t => t.Type == commandRootType);
			conf.Setup?.Invoke(ctx);

			if (ctx.Permissions.Count == 0)
				return;

			await ApplicationCommandsExtension._client.OverwriteGuildApplicationCommandPermissionsAsync(guildid.Value, commandId, ctx.Permissions);
		}

		internal static async Task UpdateCommandPermissionGroupAsync(IEnumerable<ApplicationCommandsModuleConfiguration> types, ulong? guildid, List<KeyValuePair<Type, Type>> commandTypeSources, GroupCommand groupCommand)
		{
			foreach (var com in groupCommand.Methods)
			{
				var source = commandTypeSources.FirstOrDefault(f => f.Key == com.Value.DeclaringType);

				await UpdateCommandPermissionAsync(types, guildid, groupCommand.CommandId, com.Key, source.Key, source.Value);
			}
		}
	}
}
