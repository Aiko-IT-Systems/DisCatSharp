using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that a command is only usable within a guild.
/// </summary>
public sealed class RequireGuildAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Defines that this command is only usable within a guild.
	/// </summary>
	public RequireGuildAttribute()
	{ }

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		=> Task.FromResult(ctx.Guild != null);
}
