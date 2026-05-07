using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks;

/// <summary>
///     Runs the transport-neutral incoming webhook ingress pipeline.
/// </summary>
/// <remarks>
///     Registered handlers are invoked in registration order until one returns a response. When no handler accepts the request, the
///     pipeline returns <c>501 Not Implemented</c>.
/// </remarks>
internal sealed class DiscordIncomingWebhookIngressService
{
	private readonly IReadOnlyCollection<IDiscordIncomingWebhookHandler> _handlers;

	public DiscordIncomingWebhookIngressService(IEnumerable<IDiscordIncomingWebhookHandler> handlers)
	{
		ArgumentNullException.ThrowIfNull(handlers);

		this._handlers = [.. handlers];
	}

	/// <summary>
	///     Processes an incoming webhook request.
	/// </summary>
	/// <param name="request">The ingress request to process.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The response chosen by the webhook pipeline.</returns>
	public async ValueTask<DiscordIngressResponse> HandleAsync(DiscordIngressRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var context = new DiscordIncomingWebhookContext(request);
		foreach (var handler in this._handlers)
		{
			var response = await handler.HandleAsync(context, cancellationToken).ConfigureAwait(false);
			if (response is not null)
				return response;
		}

		return DiscordIngressResponse.Empty(StatusCodes.Status501NotImplemented);
	}
}
