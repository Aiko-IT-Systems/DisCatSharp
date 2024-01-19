using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities.Core;

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
	/// <param name="cooldownBucket">Gets the current cooldown bucket.</param>
	Task Responder(BaseContext context, CooldownBucket cooldownBucket);
}
