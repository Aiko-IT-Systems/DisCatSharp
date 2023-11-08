using System;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines that usage of this command is restricted to boosters.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class RequireBoostingAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Gets the required boost time.
	/// </summary>
	public int Since { get; }

	/// <summary>
	/// Gets the required guild.
	/// </summary>
	public ulong GuildId { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RequireBoostingAttribute"/> class.
	/// </summary>
	/// <param name="days">Boosting since days.</param>
	public RequireBoostingAttribute(int days = 0)
	{
		this.GuildId = 0;
		this.Since = days;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RequireBoostingAttribute"/> class.
	/// </summary>
	/// <param name="guildId">Target guild id.</param>
	/// <param name="days">Boosting since days.</param>
	public RequireBoostingAttribute(ulong guildId, int days = 0)
	{
		this.GuildId = guildId;
		this.Since = days;
	}

	/// <summary>
	/// Executes the a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		if (this.GuildId != 0)
		{
			var guild = await ctx.Client.GetGuildAsync(this.GuildId).ConfigureAwait(false);
			var member = await guild.GetMemberAsync(ctx.User.Id).ConfigureAwait(false);
			return member != null && member.PremiumSince.HasValue ? await Task.FromResult(member.PremiumSince.Value.UtcDateTime.Date < DateTime.UtcNow.Date.AddDays(-this.Since)).ConfigureAwait(false) : await Task.FromResult(false).ConfigureAwait(false);
		}
		else
			return ctx.Member != null && ctx.Member.PremiumSince.HasValue ? await Task.FromResult(ctx.Member.PremiumSince.Value.UtcDateTime.Date < DateTime.UtcNow.Date.AddDays(-this.Since)).ConfigureAwait(false) : await Task.FromResult(false).ConfigureAwait(false);
	}
}
