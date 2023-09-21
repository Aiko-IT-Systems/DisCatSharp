using System;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// The base class for a pre-execution check for a application command.
/// </summary>
public abstract class ApplicationCommandCheckBaseAttribute : Attribute
{
	/// <summary>
	/// Checks whether this command can be executed within the current context.
	/// </summary>
	/// <param name="ctx">The context.</param>
	/// <returns>Whether the checks passed.</returns>
	public abstract Task<bool> ExecuteChecksAsync(BaseContext ctx);
}
