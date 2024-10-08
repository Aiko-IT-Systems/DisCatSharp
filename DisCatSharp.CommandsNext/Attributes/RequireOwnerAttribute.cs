using System;
using System.Linq;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

// TODO: Like in applciation commands, allow the different team roles to be chosen
/// <summary>
///     Defines that usage of this command is restricted to the owner(s) of the bot.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireOwnerAttribute : CheckBaseAttribute
{
	/// <summary>
	///     Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		var app = ctx.Client.CurrentApplication;
		var me = ctx.Client.CurrentUser;

		return app != null ? Task.FromResult(app.Members.Any(x => x.Id == ctx.User.Id)) : Task.FromResult(ctx.User.Id == me.Id);
	}
}
