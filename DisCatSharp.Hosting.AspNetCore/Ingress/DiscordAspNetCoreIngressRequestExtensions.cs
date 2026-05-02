using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal static class DiscordAspNetCoreIngressRequestExtensions
{
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
