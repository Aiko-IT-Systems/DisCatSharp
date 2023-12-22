using System;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that a command is only usable within a community-enabled guild.
/// </summary>
///
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireCommunityAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Defines that this command is only usable within a community-enabled guild.
	/// </summary>
	public RequireCommunityAttribute()
	{ }

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild != null && ctx.Guild.IsCommunity);
}
