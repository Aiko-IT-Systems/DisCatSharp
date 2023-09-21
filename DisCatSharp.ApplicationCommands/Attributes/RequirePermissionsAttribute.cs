using System;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that usage of this application command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequirePermissionsAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Gets the permissions required by this attribute.
	/// </summary>
	public Permissions Permissions { get; }

	/// <summary>
	/// Gets or sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.
	/// </summary>
	public bool IgnoreDms { get; } = true;

	/// <summary>
	/// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
	/// </summary>
	/// <param name="permissions">Permissions required to execute this command.</param>
	/// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
	public ApplicationCommandRequirePermissionsAttribute(Permissions permissions, bool ignoreDms = true)
	{
		this.Permissions = permissions;
		this.IgnoreDms = ignoreDms;
	}

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		if (ctx.Guild == null)
			return this.IgnoreDms;

		var usr = ctx.Member;
		if (usr == null)
			return false;

		var pusr = ctx.Channel.PermissionsFor(usr);

		var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
		if (bot == null)
			return false;

		var pbot = ctx.Channel.PermissionsFor(bot);

		var usrok = ctx.Guild.OwnerId == usr.Id;
		var botok = ctx.Guild.OwnerId == bot.Id;

		if (!usrok)
			usrok = (pusr & Permissions.Administrator) != 0 || (pusr & this.Permissions) == this.Permissions;

		if (!botok)
			botok = (pbot & Permissions.Administrator) != 0 || (pbot & this.Permissions) == this.Permissions;

		return usrok && botok;
	}
}
