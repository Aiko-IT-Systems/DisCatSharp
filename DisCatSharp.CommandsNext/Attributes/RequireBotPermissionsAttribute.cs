using System;
using System.Threading.Tasks;

using DisCatSharp.Enums;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is only possible when the bot is granted a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireBotPermissionsAttribute : CheckBaseAttribute
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
	/// Defines that usage of this command is only possible when the bot is granted a specific permission.
	/// </summary>
	/// <param name="permissions">Permissions required to execute this command.</param>
	/// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
	public RequireBotPermissionsAttribute(Permissions permissions, bool ignoreDms = true)
	{
		this.Permissions = permissions;
		this.IgnoreDms = ignoreDms;
	}

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		if (ctx.Guild == null)
			return this.IgnoreDms;

		var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
		if (bot == null)
			return false;

		if (bot.Id == ctx.Guild.OwnerId)
			return true;

		var channel = ctx.Channel;
		if (ctx.Channel.GuildId == null)
			channel = await ctx.Client.GetChannelAsync(ctx.Channel.Id, true).ConfigureAwait(false);
		var pbot = channel.PermissionsFor(bot);

		return (pbot & Permissions.Administrator) != 0 || (pbot & this.Permissions) == this.Permissions;
	}
}
