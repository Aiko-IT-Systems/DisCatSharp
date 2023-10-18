using System;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that this application command is restricted to team members of the bot with owner role.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireTeamOwnerAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Defines that this application command is restricted to team members of the bot with owner role.
	/// </summary>
	public ApplicationCommandRequireTeamOwnerAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
	{
		var app = ctx.Client.CurrentApplication!;
		if (app.Team is null)
			return Task.FromResult(app.Owner.Id == ctx.User.Id);

		var teamMember = app.Team.Members.FirstOrDefault(x => x.User.Id == ctx.User.Id);
		return teamMember == null ? Task.FromResult(false) : Task.FromResult(teamMember.Role is "owner");
	}
}
