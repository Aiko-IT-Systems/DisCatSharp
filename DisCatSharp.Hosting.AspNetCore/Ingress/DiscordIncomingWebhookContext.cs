using System;
using System.Collections.Generic;

using Microsoft.Extensions.Primitives;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents an incoming webhook request received through the ASP.NET Core ingress surface.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DiscordIncomingWebhookContext" /> class.
/// </remarks>
/// <param name="request">The transport-neutral ingress request.</param>
public sealed class DiscordIncomingWebhookContext(DiscordIngressRequest request)
{

	/// <summary>
	///     Gets the transport-neutral ingress request.
	/// </summary>
	public DiscordIngressRequest Request { get; } = request ?? throw new ArgumentNullException(nameof(request));

	/// <summary>
	///     Gets the incoming HTTP method.
	/// </summary>
	public string Method => this.Request.Method;

	/// <summary>
	///     Gets the request URI when available.
	/// </summary>
	public Uri? RequestUri => this.Request.RequestUri;

	/// <summary>
	///     Gets the incoming request headers.
	/// </summary>
	public IReadOnlyDictionary<string, StringValues> Headers => this.Request.Headers;

	/// <summary>
	///     Gets the raw request body.
	/// </summary>
	public DiscordIngressPayload Body => this.Request.Body;

	/// <summary>
	///     Attempts to resolve a request header by name.
	/// </summary>
	/// <param name="name">The header name to resolve.</param>
	/// <param name="value">The resolved header value.</param>
	/// <returns><see langword="true" /> when the header exists.</returns>
	public bool TryGetHeader(string name, out StringValues value) => this.Request.TryGetHeader(name, out value);

	/// <summary>
	///     Gets the first value for a request header when present.
	/// </summary>
	/// <param name="name">The header name to resolve.</param>
	/// <returns>The first header value, or <see langword="null" /> when the header does not exist.</returns>
	public string? GetHeaderValue(string name) => this.Request.GetHeaderValue(name);

	/// <summary>
	///     Attempts to resolve a single header value by name.
	/// </summary>
	/// <param name="name">The header name to resolve.</param>
	/// <param name="value">The single header value when one is present.</param>
	/// <returns><see langword="true" /> when exactly one header value exists.</returns>
	public bool TryGetSingleHeaderValue(string name, out string? value) => this.Request.TryGetSingleHeaderValue(name, out value);
}
