using System;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is restricted to the guild owner.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireGuildOwnerAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		var guild = await Task.FromResult(ctx.Guild != null).ConfigureAwait(false);
		if (guild)
		{
			var owner = await Task.FromResult(ctx.Member == ctx.Guild.Owner).ConfigureAwait(false);

			return owner;
		}
		else
			return false;
	}
}
