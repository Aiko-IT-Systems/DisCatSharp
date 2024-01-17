using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Entities;

/// <summary>
/// The cooldown responder.
/// </summary>
public interface ICooldownResponder
{
	/// <summary>
	/// <para>Responds to cooldown ratelimit hits with given actions.</para>
	/// <para>For example you could respond with a reaction or send a dm.</para>
	/// </summary>
	/// <param name="context">The context.</param>
	Task Responder(CommandContext context);
}
