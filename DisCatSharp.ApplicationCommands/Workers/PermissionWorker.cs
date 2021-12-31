// This file is part of the DisCatSharp project, a fork of DSharpPlus.
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
        internal static async Task UpdateCommandPermissionAsync(IEnumerable<ApplicationCommandsModuleConfiguration> Types, ulong? Guildid, ulong CommandId, string CommandName, Type CommandDeclaringType, Type CommandRootType)
        {
            if (!Guildid.HasValue)
            {
                ApplicationCommandsExtension._client.Logger.LogTrace("You can't set global permissions till yet. See https://discord.com/developers/docs/interactions/application-commands#permissions");
                return;
            }

            var ctx = new ApplicationCommandsPermissionContext(CommandDeclaringType, CommandName);
            var conf = Types.First(T => T.Type == CommandRootType);
            conf.Setup?.Invoke(ctx);

            if (ctx.Permissions.Count == 0)
                return;

            await ApplicationCommandsExtension._client.OverwriteGuildApplicationCommandPermissions(Guildid.Value, CommandId, ctx.Permissions);
        }

        internal static async Task UpdateCommandPermissionGroupAsync(IEnumerable<ApplicationCommandsModuleConfiguration> Types, ulong? Guildid, List<KeyValuePair<Type, Type>> CommandTypeSources, GroupCommand GroupCommand)
        {
            foreach (var com in GroupCommand.Methods)
            {
                var source = CommandTypeSources.FirstOrDefault(F => F.Key == com.Value.DeclaringType);

                await UpdateCommandPermissionAsync(Types, Guildid, GroupCommand.CommandId, com.Key, source.Key, source.Value);
            }
        }
    }
}
