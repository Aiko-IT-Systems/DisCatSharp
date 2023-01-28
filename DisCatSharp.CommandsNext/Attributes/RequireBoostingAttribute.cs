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
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is restricted to boosters.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireBoostingAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Gets the required boost time.
	/// </summary>
	public int Since { get; }

	/// <summary>
	/// Gets the required guild.
	/// </summary>
	public ulong GuildId { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RequireBoostingAttribute"/> class.
	/// </summary>
	/// <param name="days">Boosting since days.</param>
	public RequireBoostingAttribute(int days = 0)
	{
		this.GuildId = 0;
		this.Since = days;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RequireBoostingAttribute"/> class.
	/// </summary>
	/// <param name="guildId">Target guild id.</param>
	/// <param name="days">Boosting since days.</param>
	public RequireBoostingAttribute(ulong guildId, int days = 0)
	{
		this.GuildId = guildId;
		this.Since = days;
	}

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		if (this.GuildId != 0)
		{
			var guild = await ctx.Client.GetGuildAsync(this.GuildId);
			var member = await guild.GetMemberAsync(ctx.User.Id);
			return member != null && member.PremiumSince.HasValue ? await Task.FromResult(member.PremiumSince.Value.UtcDateTime.Date < DateTime.UtcNow.Date.AddDays(-this.Since)) : await Task.FromResult(false);
		}
		else
		{
			return ctx.Member != null && ctx.Member.PremiumSince.HasValue ? await Task.FromResult(ctx.Member.PremiumSince.Value.UtcDateTime.Date < DateTime.UtcNow.Date.AddDays(-this.Since)) : await Task.FromResult(false);
		}
	}
}
