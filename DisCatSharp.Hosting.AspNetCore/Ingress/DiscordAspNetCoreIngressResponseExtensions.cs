using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Adapts transport-neutral ingress responses back into ASP.NET Core results.
/// </summary>
/// <remarks>
///     This adapter is the final step for the endpoint handlers after the transport-neutral ingress services finish processing a
///     request.
/// </remarks>
internal static class DiscordAspNetCoreIngressResponseExtensions
{
	/// <summary>
	///     Converts the supplied ingress response into an ASP.NET Core result.
	/// </summary>
	/// <param name="response">The transport-neutral response to adapt.</param>
	/// <returns>The ASP.NET Core result.</returns>
	public static IResult ToAspNetCoreResult(this DiscordIngressResponse response)
	{
		ArgumentNullException.ThrowIfNull(response);

		return new DiscordAspNetCoreIngressResult(response);
	}

	private sealed class DiscordAspNetCoreIngressResult(DiscordIngressResponse response) : IResult
	{
		/// <summary>
		///     Writes the adapted response into the current ASP.NET Core HTTP response.
		/// </summary>
		/// <param name="httpContext">The current HTTP context.</param>
		/// <returns>A task that completes when the response has been written.</returns>
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
