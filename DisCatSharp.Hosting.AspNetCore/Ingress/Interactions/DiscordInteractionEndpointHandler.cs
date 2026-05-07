using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.Interactions;

/// <summary>
///     ASP.NET Core endpoint adapter for the interaction ingress pipeline.
/// </summary>
/// <remarks>
///     This bridge converts the raw <see cref="HttpRequest" /> into <see cref="DiscordIngressRequest" />, executes the transport-neutral
///     pipeline, and converts the result back into an ASP.NET Core response.
/// </remarks>
internal sealed class DiscordInteractionEndpointHandler(IDiscordIngressBodyReader bodyReader, DiscordInteractionIngressService service)
{
	private readonly IDiscordIngressBodyReader _bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
	private readonly DiscordInteractionIngressService _service = service ?? throw new ArgumentNullException(nameof(service));

	/// <summary>
	///     Handles an ASP.NET Core interaction request.
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
			var result = await this._service.HandleAsync(ingressRequest, cancellationToken).ConfigureAwait(false);
			return result.Response.ToAspNetCoreResult();
		}
		catch (DiscordIngressBodyTooLargeException)
		{
			return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
		}
	}
}
