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
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that this application command is restricted to team members of the bot with admin role or higher.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireTeamAdminAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Defines that this application command is restricted to team members of the bot with admin role or higher.
	/// </summary>
	public ApplicationCommandRequireTeamAdminAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		var app = ctx.Client.CurrentApplication!;
		if (app.Team is null)
			return Task.FromResult(app.Owner.Id == ctx.User.Id);

		var teamMember = app.Team?.Members.FirstOrDefault(x => x.User.Id == ctx.User.Id);
		return teamMember == null ? Task.FromResult(false) : Task.FromResult(teamMember.Role is "admin" or "owner");
	}
}
