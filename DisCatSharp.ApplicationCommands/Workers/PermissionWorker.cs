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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.ApplicationCommands
{
	/// <summary>
	/// Represents a <see cref="PermissionWorker"/>.
	/// </summary>
	internal class PermissionWorker
	{
		/// <summary>
		/// Updates the application command permissions.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <param name="guildid">The optional guild id.</param>
		/// <param name="commandId">The command id.</param>
		/// <param name="commandName">The command name.</param>
		/// <param name="commandDeclaringType">The declaring command type.</param>
		/// <param name="commandRootType">The root command type.</param>
		internal static async Task UpdateCommandPermissionAsync(IEnumerable<ApplicationCommandsModuleConfiguration> types, ulong? guildid, ulong commandId, string commandName, Type commandDeclaringType, Type commandRootType)
		{
			if (!guildid.HasValue)
			{
				ApplicationCommandsExtension.ClientInternal.Logger.LogTrace("You can't set global permissions till yet. See https://discord.com/developers/docs/interactions/application-commands#permissions");
				return;
			}

			var ctx = new ApplicationCommandsPermissionContext(commandDeclaringType, commandName);
			var conf = types.First(t => t.Type == commandRootType);
			conf.Permissions?.Invoke(ctx);

			if (ctx.Permissions.Count == 0)
				return;

			ApplicationCommandsExtension.ClientInternal.Logger.Log(ApplicationCommandsExtension.ApplicationCommandsLogLevel, $"[AC Perms] Command {commandName} permission update on {guildid.Value}");

			await ApplicationCommandsExtension.ClientInternal.OverwriteGuildApplicationCommandPermissionsAsync(guildid.Value, commandId, ctx.Permissions);
		}

		/// <summary>
		/// Gets the permissions.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <param name="commandId">The command id.</param>
		/// <param name="commandName">The command name.</param>
		/// <param name="commandDeclaringType">The declaring command type.</param>
		/// <param name="commandRootType">The root command type.</param>
		/// <returns>Permissions on success.</returns>
		internal static (
			bool success,
			ulong? commandId,
			IReadOnlyList<DiscordApplicationCommandPermission> permissions
		) ResolvePermissions(IEnumerable<ApplicationCommandsModuleConfiguration> types, ulong commandId, string commandName, Type commandDeclaringType, Type commandRootType)
		{
			var ctx = new ApplicationCommandsPermissionContext(commandDeclaringType, commandName);
			var conf = types.First(t => t.Type == commandRootType);
			conf.Permissions?.Invoke(ctx);

			return ctx.Permissions.Count == 0 || commandId == 0
				? (false, null, null)
				: (true, commandId, ctx.Permissions.ToList());
		}

		/// <summary>
		/// Gets the global permissions.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <param name="commandId">The command id.</param>
		/// <param name="commandName">The command name.</param>
		/// <param name="commandDeclaringType">The declaring command type.</param>
		/// <param name="commandRootType">The root command type.</param>
		/// <returns>Permissions on success.</returns>
		internal static (
			bool success,
			ulong? commandId,
			IReadOnlyList<KeyValuePair<ulong, DiscordApplicationCommandPermission>> permissions
		) ResolveGlobalPermissions(IEnumerable<ApplicationCommandsModuleConfiguration> types, ulong commandId, string commandName, Type commandDeclaringType, Type commandRootType)
		{
			var ctx = new ApplicationCommandsGlobalPermissionContext(commandDeclaringType, commandName);
			var conf = types.First(t => t.Type == commandRootType);
			conf.GlobalGuildPermissions?.Invoke(ctx);

			return ctx.GuildPermissions.Count == 0 || commandId == 0
				? (false, null, null)
				: (true, commandId, ctx.GuildPermissions);
		}

		/// <summary>
		/// Updates the permissions.
		/// </summary>
		/// <param name="guildId">The guild id.</param>
		/// <param name="permissionOverwrites">The permission overwrites.</param>
		internal static async Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> BulkOverwriteCommandPermissionsAsync(ulong guildId, IEnumerable<DiscordGuildApplicationCommandPermission> permissionOverwrites)
			=> await ApplicationCommandsExtension.ClientInternal.BulkOverwriteGuildApplicationCommandsAsync(guildId, permissionOverwrites);
	}
}
