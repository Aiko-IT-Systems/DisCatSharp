using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// The cooldown responder.
/// </summary>
public interface ICooldownResponder
{
	/// <summary>
	/// Responds to cooldown ratelimit hits with given response.
	/// </summary>
	/// <param name="context">The context.</param>
	Task Responder(BaseContext context);
}
