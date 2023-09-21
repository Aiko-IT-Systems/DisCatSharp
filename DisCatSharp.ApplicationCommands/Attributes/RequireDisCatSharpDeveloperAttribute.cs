using System;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines that this application command is restricted to DisCatSharp Developers.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationCommandRequireDisCatSharpDeveloperAttribute : ApplicationCommandCheckBaseAttribute
{
	/// <summary>
	/// Defines that this application command is restricted to DisCatSharp Developers.
	/// </summary>
	public ApplicationCommandRequireDisCatSharpDeveloperAttribute()
	{ }

	/// <summary>
	/// Runs checks.
	/// </summary>
	public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
		=> Task.FromResult(false);
}
