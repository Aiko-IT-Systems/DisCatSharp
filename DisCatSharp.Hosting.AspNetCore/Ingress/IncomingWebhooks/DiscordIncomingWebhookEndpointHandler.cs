using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks;

internal sealed class DiscordIncomingWebhookEndpointHandler(
	IDiscordIngressBodyReader bodyReader,
	DiscordIncomingWebhookIngressService service
	)
{
	private readonly IDiscordIngressBodyReader _bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
	private readonly DiscordIncomingWebhookIngressService _service = service ?? throw new ArgumentNullException(nameof(service));

	public async ValueTask<IResult> HandleAsync(HttpRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		try
		{
			var ingressRequest = await request.ToDiscordIngressRequestAsync(this._bodyReader, cancellationToken).ConfigureAwait(false);
			var response = await this._service.HandleAsync(ingressRequest, cancellationToken).ConfigureAwait(false);
			return response.ToAspNetCoreResult();
		}
		catch (DiscordIngressBodyTooLargeException)
		{
			return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
		}
	}
}
