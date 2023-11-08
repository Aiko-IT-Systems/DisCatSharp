using System;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Attributes;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that usage of this application command is restricted to users with a specified entitlement.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false), RequiresFeature(Features.MonetizedApplication)]
public sealed class ApplicationCommandRequirePremiumAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Gets the entitlement id required by this attribute.
	/// </summary>
	public ulong? EntitlementId { get; } = null;

	/// <summary>
	/// Defines that usage of this command is restricted to users with a specified entitlement.
	/// </summary>
	/// <param name="entitlementId">Entitlement id required to execute this command.</param>
	[RequiresFeature(Features.MonetizedApplication)]
	public ApplicationCommandRequirePremiumAttribute(ulong entitlementId)
	{
		this.EntitlementId = entitlementId;
	}

	/// <summary>
	/// Defines that usage of this command is restricted to users with a specified entitlement.
	/// <para>Use this attribute if you set <see cref="DiscordConfiguration.SkuId"/> or <see cref="DiscordConfiguration.AutoFetchSkuIds"/> in your &lt;see cref="DiscordConfiguration"/&gt;.</para>
	/// </summary>
	[RequiresFeature(Features.MonetizedApplication)]
	public ApplicationCommandRequirePremiumAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		var targetSkuId = this.EntitlementId ?? ctx.Client.Configuration.SkuId ?? throw new("Missing SKU ID");

		if (ctx.Interaction.EntitlementSkuIds.Contains(targetSkuId))
			return await Task.FromResult(true).ConfigureAwait(false);

		await ctx.CreateResponseAsync(InteractionResponseType.PremiumRequired).ConfigureAwait(false);
		return await Task.FromResult(false).ConfigureAwait(false);
	}
}

/// <summary>
/// Defines that usage of this application command is restricted to users with a test entitlement.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false), RequiresFeature(Features.MonetizedApplication)]
public sealed class ApplicationCommandRequirePremiumTestAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Gets the entitlement id required by this attribute.
	/// </summary>
	public ulong? EntitlementId { get; } = null;

	/// <summary>
	/// Defines that usage of this command is restricted to users with a specified entitlement.
	/// </summary>
	/// <param name="entitlementId">Entitlement id required to execute this command.</param>
	[RequiresFeature(Features.MonetizedApplication)]
	public ApplicationCommandRequirePremiumTestAttribute(ulong entitlementId)
	{
		this.EntitlementId = entitlementId;
	}

	/// <summary>
	/// Defines that usage of this command is restricted to users with a specified entitlement.
	/// <para>Use this attribute if you set <see cref="DiscordConfiguration.TestSkuId"/> or <see cref="DiscordConfiguration.AutoFetchSkuIds"/> in your <see cref="DiscordConfiguration"/>.</para>
	/// </summary>
	[RequiresFeature(Features.MonetizedApplication)]
	public ApplicationCommandRequirePremiumTestAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		var targetSkuId = this.EntitlementId ?? ctx.Client.Configuration.TestSkuId ?? throw new("Missing SKU ID");

		if (ctx.Interaction.EntitlementSkuIds.Contains(targetSkuId))
			return await Task.FromResult(true).ConfigureAwait(false);

		await ctx.CreateResponseAsync(InteractionResponseType.PremiumRequired).ConfigureAwait(false);
		return await Task.FromResult(false).ConfigureAwait(false);
	}
}
