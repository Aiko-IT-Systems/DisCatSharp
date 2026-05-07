using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks;

/// <summary>
///     ASP.NET Core endpoint adapter for generic incoming webhooks.
/// </summary>
/// <remarks>
///     This bridge applies the shared request body reader and response adapter around the transport-neutral incoming webhook pipeline.
/// </remarks>
internal sealed class DiscordIncomingWebhookEndpointHandler(
	IDiscordIngressBodyReader bodyReader,
	DiscordIncomingWebhookIngressService service
	)
{
	private readonly IDiscordIngressBodyReader _bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
	private readonly DiscordIncomingWebhookIngressService _service = service ?? throw new ArgumentNullException(nameof(service));

	/// <summary>
	///     Handles an ASP.NET Core incoming webhook request.
	/// </summary>
	/// <param name="request">The incoming ASP.NET Core request.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The adapted ASP.NET Core result.</returns>
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
