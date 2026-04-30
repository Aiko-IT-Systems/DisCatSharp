using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordIncomingWebhookEndpointHandler
{
	private readonly IDiscordIngressBodyReader _bodyReader;
	private readonly DiscordIncomingWebhookIngressService _service;

	public DiscordIncomingWebhookEndpointHandler(
		IDiscordIngressBodyReader bodyReader,
		DiscordIncomingWebhookIngressService service
	)
	{
		this._bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
		this._service = service ?? throw new ArgumentNullException(nameof(service));
	}

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
