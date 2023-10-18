using System;
using System.Threading.Tasks;

using DisCatSharp.Enums;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is restricted to discord employees.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireStaffAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => ctx.User.Flags.HasValue ? Task.FromResult(ctx.User.Flags.Value.HasFlag(UserFlags.Staff)) : Task.FromResult(false);
}
