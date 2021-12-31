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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes
{
    /// <summary>
    /// Requires ownership of the bot or a whitelisted id to execute this command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequireOwnerOrIdAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Allowed user ids
        /// </summary>
        public IReadOnlyList<ulong> UserIds { get; }

        /// <summary>
        /// Defines that usage of this command is restricted to the owner or whitelisted ids of the bot.
        /// </summary>
        /// <param name="user_ids">List of allowed user ids</param>
        public RequireOwnerOrIdAttribute(params ulong[] UserIds)
        {
            this.UserIds = new ReadOnlyCollection<ulong>(UserIds);
        }

        /// <summary>
        /// Executes the a check.
        /// </summary>
        /// <param name="Ctx">The command context.</param>
        /// <param name="Help">If true, help - returns true.</param>
        public override async Task<bool> ExecuteCheck(CommandContext Ctx, bool Help)
        {
            var app = Ctx.Client.CurrentApplication;
            var me = Ctx.Client.CurrentUser;

            var owner = app != null ? await Task.FromResult(app.Owners.Any(X => X.Id == Ctx.User.Id)) : await Task.FromResult(Ctx.User.Id == me.Id);

            var allowed = this.UserIds.Contains(Ctx.User.Id);

            return owner || allowed;

        }
    }
}
