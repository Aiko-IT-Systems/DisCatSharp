using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal static class DiscordAspNetCoreIngressResponseExtensions
{
	public static IResult ToAspNetCoreResult(this DiscordIngressResponse response)
	{
		ArgumentNullException.ThrowIfNull(response);

		return new DiscordAspNetCoreIngressResult(response);
	}

	private sealed class DiscordAspNetCoreIngressResult(DiscordIngressResponse response) : IResult
	{
		public async Task ExecuteAsync(HttpContext httpContext)
		{
			ArgumentNullException.ThrowIfNull(httpContext);

			httpContext.Response.StatusCode = response.StatusCode;

			foreach (var (key, value) in response.Headers)
				httpContext.Response.Headers[key] = value;

			if (response.Body.IsEmpty)
				return;

			if (!string.IsNullOrWhiteSpace(response.ContentType))
				httpContext.Response.ContentType = response.ContentType;

			httpContext.Response.ContentLength = response.Body.Length;
			await httpContext.Response.Body.WriteAsync(response.Body.Bytes);
		}
	}
}
