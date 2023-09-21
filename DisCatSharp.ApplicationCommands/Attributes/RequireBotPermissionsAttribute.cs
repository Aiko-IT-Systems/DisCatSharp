using System;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that usage of this application command is only possible when the bot is granted a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireBotPermissionsAttribute : ApplicationCommandCheckBaseAttribute
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
	/// Defines that usage of this application command is only possible when the bot is granted a specific permission.
	/// </summary>
	/// <param name="permissions">Permissions required to execute this command.</param>
	/// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
	public ApplicationCommandRequireBotPermissionsAttribute(Permissions permissions, bool ignoreDms = true)
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

		var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
		if (bot == null)
			return false;

		if (bot.Id == ctx.Guild.OwnerId)
			return true;

		var pbot = ctx.Channel.PermissionsFor(bot);

		return (pbot & Permissions.Administrator) != 0 || (pbot & this.Permissions) == this.Permissions;
	}
}
