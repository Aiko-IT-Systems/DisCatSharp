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
