using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.Interactions;

/// <summary>
///     Handles Discord interactions received over incoming HTTP.
/// </summary>
/// <remarks>
///     Discord expects the initial response to arrive within roughly three seconds. Return a deferred response when the
///     interaction requires more work, then complete follow-up work with the interaction token through the standard
///     outbound interaction webhook APIs within fifteen minutes.
/// </remarks>
public interface IDiscordInteractionIngressHandler
{
	/// <summary>
	///     Attempts to handle an incoming Discord interaction.
	/// </summary>
	/// <param name="context">The interaction ingress context.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>
	///     A transport-neutral interaction response when the interaction was handled, or <see langword="null" /> to allow
	///     other registered handlers to try.
	/// </returns>
	ValueTask<DiscordInteractionIngressResponse?> HandleAsync(DiscordInteractionIngressContext context, CancellationToken cancellationToken = default);
}
