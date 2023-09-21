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
