using System;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

/// <summary>
///     Processes Discord OAuth authorization-code callbacks.
/// </summary>
/// <remarks>
///     The default implementation consumes the pending state entry, validates the callback against the original authorization request,
///     and then delegates the code exchange to <see cref="IDiscordOAuthTokenExchangeService" />.
/// </remarks>
public interface IDiscordOAuthCallbackHandler
{
	/// <summary>
	///     Processes the specified callback request.
	/// </summary>
	/// <param name="request">The callback request to process.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The callback processing result.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="request" /> is <see langword="null" />.</exception>
	Task<DiscordOAuthCallbackResult> HandleAsync(DiscordOAuthCallbackRequest request, CancellationToken cancellationToken = default);
}
