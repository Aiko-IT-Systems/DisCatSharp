using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordWebhookEventEndpointHandler
{
	private readonly IDiscordIngressBodyReader _bodyReader;
	private readonly DiscordWebhookEventIngressService _service;

	public DiscordWebhookEventEndpointHandler(
		IDiscordIngressBodyReader bodyReader,
		DiscordWebhookEventIngressService service
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
			var ingressRequest = await request.ToDiscordIngressRequestAsync(this._bodyReader, cancellationToken);
			var result = await this._service.HandleAsync(ingressRequest, cancellationToken);
			return result.Response.ToAspNetCoreResult();
		}
		catch (DiscordIngressBodyTooLargeException)
		{
			return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
		}
	}
}
