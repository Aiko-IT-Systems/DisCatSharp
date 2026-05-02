using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordInteractionEndpointHandler(IDiscordIngressBodyReader bodyReader, DiscordInteractionIngressService service)
{
	private readonly IDiscordIngressBodyReader _bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
	private readonly DiscordInteractionIngressService _service = service ?? throw new ArgumentNullException(nameof(service));

	public async ValueTask<IResult> HandleAsync(HttpRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		try
		{
			var ingressRequest = await request.ToDiscordIngressRequestAsync(this._bodyReader, cancellationToken).ConfigureAwait(false);
			var result = await this._service.HandleAsync(ingressRequest, cancellationToken).ConfigureAwait(false);
			return result.Response.ToAspNetCoreResult();
		}
		catch (DiscordIngressBodyTooLargeException)
		{
			return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
		}
	}
}
