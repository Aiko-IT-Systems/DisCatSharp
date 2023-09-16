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
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that this application command is only usable within a guild by its owner.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireGuildOwnerAttributeAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Defines that this command is only usable within a guild by its owner.
	/// </summary>
	public ApplicationCommandRequireGuildOwnerAttributeAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
		=> Task.FromResult(ctx.Guild is not null && ctx.Guild.OwnerId == ctx.User.Id);
}
