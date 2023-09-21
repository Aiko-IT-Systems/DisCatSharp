using System;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that a command is only usable within a guild which has enabled the welcome screen.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireWelcomeScreenAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Defines that this command is only usable within a guild which has enabled the welcome screen.
	/// </summary>
	public RequireWelcomeScreenAttribute()
	{ }

	/// <summary>
	/// Executes a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild != null && ctx.Guild.HasWelcomeScreen);
}
