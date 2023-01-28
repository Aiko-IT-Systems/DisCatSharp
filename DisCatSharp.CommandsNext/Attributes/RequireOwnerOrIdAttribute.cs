// This file is part of the DisCatSharp project, based off DSharpPlus.
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Requires ownership of the bot or a whitelisted id to execute this command.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireOwnerOrIdAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Allowed user ids
	/// </summary>
	public IReadOnlyList<ulong> UserIds { get; }

	/// <summary>
	/// Defines that usage of this command is restricted to the owner or whitelisted ids of the bot.
	/// </summary>
	/// <param name="userIds">List of allowed user ids</param>
	public RequireOwnerOrIdAttribute(params ulong[] userIds)
	{
		this.UserIds = new ReadOnlyCollection<ulong>(userIds);
	}

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		var app = ctx.Client.CurrentApplication;
		var me = ctx.Client.CurrentUser;

		var owner = app != null ? await Task.FromResult(app.Owners.Any(x => x.Id == ctx.User.Id)) : await Task.FromResult(ctx.User.Id == me.Id);

		var allowed = this.UserIds.Contains(ctx.User.Id);

		return owner || allowed;

	}
}
