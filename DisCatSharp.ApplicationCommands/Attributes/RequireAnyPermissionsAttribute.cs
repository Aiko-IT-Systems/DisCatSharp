using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that usage of this application command is restricted to members with any of the specified permissions. This check also verifies that the bot has the same permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireAnyPermissionsAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Gets the permissions required by this attribute.
	/// </summary>
	public List<Permissions> AnyPermissions { get; }

	/// <summary>
	/// Defines that usage of this command is restricted to members with any of the specified permissions. This check also verifies that the bot has the same permissions.
	/// </summary>
	/// <param name="permissions">Permissions required to execute this command.</param>
	public RequireAnyPermissionsAttribute(params Permissions[] permissions)
	{
		this.AnyPermissions = [.. permissions];
	}

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		if (ctx.Guild == null)
			throw new InvalidOperationException("This check cannot be used in a DM.");

		var usr = ctx.Member;
		if (usr == null)
			return false;

		var pusr = ctx.Channel.PermissionsFor(usr);

		var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
		if (bot == null)
			return false;

		var pbot = ctx.Channel.PermissionsFor(bot);

		var usrok = ctx.Guild.OwnerId == usr.Id || pusr.HasPermission(Permissions.Administrator);
		var botok = ctx.Guild.OwnerId == bot.Id || pusr.HasPermission(Permissions.Administrator);

		if (!usrok)
			usrok = this.AnyPermissions.Any(x => (pusr & x) != 0);
		if (!botok)
			botok = this.AnyPermissions.Any(x => (pbot & x) != 0);

		return usrok && botok;
	}
}
