using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks;

/// <summary>
///     Handles generic incoming webhooks received over the DisCatSharp ASP.NET Core ingress surface.
/// </summary>
public interface IDiscordIncomingWebhookHandler
{
	/// <summary>
	///     Attempts to handle an incoming webhook request.
	/// </summary>
	/// <param name="context">The incoming webhook context.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>
	///     A transport-neutral ingress response when the request was handled, or <see langword="null" /> to allow other
	///     registered handlers to try.
	/// </returns>
	ValueTask<DiscordIngressResponse?> HandleAsync(DiscordIncomingWebhookContext context, CancellationToken cancellationToken = default);
}
