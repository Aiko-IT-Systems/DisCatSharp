using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     ASP.NET Core endpoint adapter for signed webhook events.
/// </summary>
/// <remarks>
///     This bridge applies the shared request body reader and converts transport-neutral webhook results back into ASP.NET Core
///     responses.
/// </remarks>
internal sealed class DiscordWebhookEventEndpointHandler(
	IDiscordIngressBodyReader bodyReader,
	DiscordWebhookEventIngressService service
	)
{
	private readonly IDiscordIngressBodyReader _bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
	private readonly DiscordWebhookEventIngressService _service = service ?? throw new ArgumentNullException(nameof(service));

	/// <summary>
	///     Handles an ASP.NET Core signed webhook request.
	/// </summary>
	/// <param name="request">The incoming ASP.NET Core request.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The adapted ASP.NET Core result.</returns>
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
