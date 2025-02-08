using System;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

// TODO: Add method to respond with button
/// <summary>
///     Defines that usage of this application command is restricted to users with a specified entitlement.
/// </summary>
/// <remarks>
///     Defines that usage of this command is restricted to users with a specified entitlement.
/// </remarks>
/// <param name="skuId">Sku id for which an entitlement is required to execute this command.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false), RequiresFeature(Features.MonetizedApplication)]
[method: RequiresFeature(Features.MonetizedApplication)]
public sealed class ApplicationCommandRequireSkuEntitlementAttribute(ulong skuId) : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	///     Gets the sku id requiring an entitlement.
	/// </summary>
	public ulong SkuId { get; } = skuId;

	/// <summary>
	///     Runs checks.
	/// </summary>
	public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		if (ctx.Interaction.Entitlements.Any(x => x.SkuId == this.SkuId))
			return await Task.FromResult(true).ConfigureAwait(false);

		await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddComponents(new DiscordPremiumButtonComponent(this.SkuId))).ConfigureAwait(false);
		return await Task.FromResult(false).ConfigureAwait(false);
	}
}
