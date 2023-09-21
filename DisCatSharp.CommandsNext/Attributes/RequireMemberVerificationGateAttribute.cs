using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that a command is only usable within a guild which has enabled the member verification gate.
/// </summary>
public sealed class RequireMemberVerificationGateAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Defines that this command is only usable within guild which has enabled the member verification gate.
	/// </summary>
	public RequireMemberVerificationGateAttribute()
	{ }

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild != null && ctx.Guild.HasMemberVerificationGate);
}
