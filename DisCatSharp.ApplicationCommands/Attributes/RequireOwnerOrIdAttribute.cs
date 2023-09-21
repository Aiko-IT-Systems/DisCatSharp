using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Attributes;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Requires ownership of the bot or a whitelisted id to execute this command.
/// </summary>
[Deprecated("This is deprecated and will be remove in future in favor of RequireTeamXY"),
 AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireOwnerOrIdAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Allowed user ids
	/// </summary>
	public IReadOnlyList<ulong> UserIds { get; }

	/// <summary>
	/// Defines that usage of this command is restricted to the owner or whitelisted ids of the bot.
	/// </summary>
	/// <param name="userIds">List of allowed user ids</param>
	[Deprecated("This is deprecated and will be remove in future in favor of RequireTeamXY")]
	public ApplicationCommandRequireOwnerOrIdAttribute(params ulong[] userIds)
	{
		this.UserIds = new ReadOnlyCollection<ulong>(userIds);
	}

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>s
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		var app = ctx.Client.CurrentApplication!;
		var me = ctx.Client.CurrentUser!;

		var owner = app != null
			            ? Task.FromResult(app.Members.Any(x => x.Id == ctx.User.Id))
			            : Task.FromResult(ctx.User.Id == me.Id);

		var allowed = this.UserIds.Contains(ctx.User.Id);

		return allowed ? Task.FromResult(true) : owner;
	}
}
