using System;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that this application command is only usable within a guild.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireNsfwAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Defines that this command is only usable within a guild.
	/// </summary>
	public ApplicationCommandRequireNsfwAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
		=> Task.FromResult(ctx.Guild == null || ctx.Channel.IsNsfw);
}
