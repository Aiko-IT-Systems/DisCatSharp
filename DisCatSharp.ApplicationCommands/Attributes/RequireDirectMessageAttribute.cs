using System;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that this application command is only usable within a direct message channel.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireDirectMessageAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Defines that this command is only usable within a direct message channel.
	/// </summary>
	public ApplicationCommandRequireDirectMessageAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
		=> Task.FromResult(ctx.Channel is DiscordDmChannel);
}
