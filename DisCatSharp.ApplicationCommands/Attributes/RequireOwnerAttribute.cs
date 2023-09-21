using System;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Attributes;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that this application command is restricted to the owner of the bot.
/// </summary>
[Deprecated("This is deprecated and will be remove in future in favor of RequireTeamXY"),
 AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireOwnerAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Defines that this application command is restricted to the owner of the bot.
	/// </summary>
	[Deprecated("This is deprecated and will be remove in future in favor of RequireTeamXY")]
	public ApplicationCommandRequireOwnerAttribute()
	{
	}

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		var app = ctx.Client.CurrentApplication!;
		var me = ctx.Client.CurrentUser!;

		return app != null
			       ? Task.FromResult(app.Members.Any(x => x.Id == ctx.User.Id))
			       : Task.FromResult(ctx.User.Id == me.Id);
	}
}
