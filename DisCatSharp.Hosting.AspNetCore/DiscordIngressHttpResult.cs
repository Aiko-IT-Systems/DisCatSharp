using System;
using System.Threading.Tasks;

using DisCatSharp.Hosting.AspNetCore.Ingress;

using Microsoft.AspNetCore.Http;

namespace DisCatSharp.Hosting.AspNetCore;

internal sealed class DiscordIngressHttpResult : IResult
{
	private readonly DiscordIngressResponse _response;

	public DiscordIngressHttpResult(DiscordIngressResponse response)
		=> this._response = response ?? throw new ArgumentNullException(nameof(response));

	public async Task ExecuteAsync(HttpContext httpContext)
	{
		ArgumentNullException.ThrowIfNull(httpContext);

		httpContext.Response.StatusCode = this._response.StatusCode;
		if (!string.IsNullOrWhiteSpace(this._response.ContentType))
			httpContext.Response.ContentType = this._response.ContentType;

		foreach (var (key, value) in this._response.Headers)
			httpContext.Response.Headers[key] = value;

		if (!this._response.Body.IsEmpty)
			await httpContext.Response.Body.WriteAsync(this._response.Body.Bytes, httpContext.RequestAborted).ConfigureAwait(false);
	}
}
