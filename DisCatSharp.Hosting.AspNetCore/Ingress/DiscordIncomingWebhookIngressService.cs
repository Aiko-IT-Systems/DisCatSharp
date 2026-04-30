using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordIncomingWebhookIngressService
{
	private readonly IReadOnlyCollection<IDiscordIncomingWebhookHandler> _handlers;

	public DiscordIncomingWebhookIngressService(IEnumerable<IDiscordIncomingWebhookHandler> handlers)
	{
		ArgumentNullException.ThrowIfNull(handlers);

		this._handlers = handlers.ToArray();
	}

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
