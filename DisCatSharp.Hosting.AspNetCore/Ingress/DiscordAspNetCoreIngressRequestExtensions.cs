using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Adapts ASP.NET Core <see cref="HttpRequest" /> instances into transport-neutral ingress requests.
/// </summary>
/// <remarks>
///     The adapter rewinds buffered request bodies before reading them and restores the original stream position afterwards so other
///     ASP.NET Core components can still inspect the body later in the pipeline.
/// </remarks>
internal static class DiscordAspNetCoreIngressRequestExtensions
{
	/// <summary>
	///     Converts an ASP.NET Core request into a transport-neutral ingress request.
	/// </summary>
	/// <param name="request">The ASP.NET Core request to adapt.</param>
	/// <param name="bodyReader">The body reader responsible for enforcing ingress size limits.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The adapted ingress request.</returns>
	public static async ValueTask<DiscordIngressRequest> ToDiscordIngressRequestAsync(
		this HttpRequest request,
		IDiscordIngressBodyReader bodyReader,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(bodyReader);

		var originalPosition = request.Body.CanSeek ? request.Body.Position : 0;
		if (!request.Body.CanSeek)
			request.EnableBuffering();

		DiscordIngressPayload payload;
		try
		{
			if (request.Body.CanSeek)
				request.Body.Position = 0;

			payload = await bodyReader.ReadAsync(request.Body, cancellationToken);
		}
		finally
		{
			if (request.Body.CanSeek)
				request.Body.Position = originalPosition;
		}

		Dictionary<string, StringValues> headers = new(request.Headers.Count, StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in request.Headers)
			headers[key] = value;

		return new DiscordIngressRequest(
			request.Method,
			request.Host.HasValue ? new Uri(request.GetDisplayUrl()) : null,
			headers,
			payload);
	}
}
