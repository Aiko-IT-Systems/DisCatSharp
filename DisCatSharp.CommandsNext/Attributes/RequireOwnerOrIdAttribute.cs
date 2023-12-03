using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Requires ownership of the bot or a whitelisted id to execute this command.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireOwnerOrIdAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Allowed user ids
	/// </summary>
	public IReadOnlyList<ulong> UserIds { get; }

	/// <summary>
	/// Defines that usage of this command is restricted to the owner or whitelisted ids of the bot.
	/// </summary>
	/// <param name="userIds">List of allowed user ids</param>
	public RequireOwnerOrIdAttribute(params ulong[] userIds)
	{
		this.UserIds = new ReadOnlyCollection<ulong>(userIds);
	}

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		var app = ctx.Client.CurrentApplication;
		var me = ctx.Client.CurrentUser;

		var owner = app != null ? await Task.FromResult(app.Owners.Any(x => x.Id == ctx.User.Id)).ConfigureAwait(false) : await Task.FromResult(ctx.User.Id == me.Id).ConfigureAwait(false);

		var allowed = this.UserIds.Contains(ctx.User.Id);

		return owner || allowed;
	}
}
