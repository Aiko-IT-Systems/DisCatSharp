using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Processes Discord OAuth authorization-code callbacks.
/// </summary>
public interface IDiscordOAuthCallbackHandler
{
	/// <summary>
	///     Processes the specified callback request.
	/// </summary>
	/// <param name="request">The callback request to process.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The callback processing result.</returns>
	Task<DiscordOAuthCallbackResult> HandleAsync(DiscordOAuthCallbackRequest request, CancellationToken cancellationToken = default);
}
