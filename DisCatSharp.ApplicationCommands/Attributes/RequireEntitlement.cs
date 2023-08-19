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
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that usage of this application command is restricted to users with a specified entitlement.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequirePremiumAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Gets the entitlement id required by this attribute.
	/// </summary>
	public ulong EntitlementId { get; }

	/// <summary>
	/// Defines that usage of this command is restricted to users with a specified entitlement.
	/// </summary>
	/// <param name="entitlementId">Entitlement id required to execute this command.</param>
	public ApplicationCommandRequirePremiumAttribute(ulong entitlementId)
	{
		this.EntitlementId = entitlementId;
	}

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		if (!ctx.Interaction.EntitlementSkuIds.Contains(this.EntitlementId))
		{
			await ctx.CreateResponseAsync(InteractionResponseType.PremiumRequired).ConfigureAwait(false);
			return await Task.FromResult(false).ConfigureAwait(false);
		}

		return await Task.FromResult(true).ConfigureAwait(false);
	}
}
