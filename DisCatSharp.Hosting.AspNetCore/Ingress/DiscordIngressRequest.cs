using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Extensions.Primitives;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a transport-neutral ingress request that future handlers can operate on.
/// </summary>
public sealed class DiscordIngressRequest
{
	private static readonly IReadOnlyDictionary<string, StringValues> s_emptyHeaders =
		new ReadOnlyDictionary<string, StringValues>(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressRequest" /> class.
	/// </summary>
	/// <param name="method">The request method.</param>
	/// <param name="requestUri">The request URI when one is available.</param>
	/// <param name="headers">The request headers.</param>
	/// <param name="body">The raw request body.</param>
	public DiscordIngressRequest(
		string method,
		Uri? requestUri = null,
		IReadOnlyDictionary<string, StringValues>? headers = null,
		DiscordIngressPayload? body = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(method);

		this.Method = method;
		this.RequestUri = requestUri;
		this.Headers = CreateHeaders(headers);
		this.Body = body ?? DiscordIngressPayload.Empty;
	}

	/// <summary>
	///     Gets the request method.
	/// </summary>
	public string Method { get; }

	/// <summary>
	///     Gets the request URI when one is available.
	/// </summary>
	public Uri? RequestUri { get; }

	/// <summary>
	///     Gets the request headers.
	/// </summary>
	public IReadOnlyDictionary<string, StringValues> Headers { get; }

	/// <summary>
	///     Gets the raw request body.
	/// </summary>
	public DiscordIngressPayload Body { get; }

	/// <summary>
	///     Attempts to resolve a request header by name.
	/// </summary>
	/// <param name="name">The header name to resolve.</param>
	/// <param name="value">The resolved header value.</param>
	/// <returns><see langword="true" /> when the header exists.</returns>
	public bool TryGetHeader(string name, out StringValues value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return this.Headers.TryGetValue(name, out value);
	}

	/// <summary>
	///     Gets the first value for a request header when present.
	/// </summary>
	/// <param name="name">The header name to resolve.</param>
	/// <returns>The first header value, or <see langword="null" /> when the header does not exist.</returns>
	public string? GetHeaderValue(string name) => this.TryGetHeader(name, out var value) ? value.ToString() : null;

	/// <summary>
	///     Attempts to resolve a single header value by name.
	/// </summary>
	/// <param name="name">The header name to resolve.</param>
	/// <param name="value">The single header value when one is present.</param>
	/// <returns><see langword="true" /> when exactly one header value exists.</returns>
	public bool TryGetSingleHeaderValue(string name, out string? value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		value = null;
		if (!this.TryGetHeader(name, out var values) || values.Count != 1)
			return false;

		value = values[0];
		return value is not null;
	}

	private static IReadOnlyDictionary<string, StringValues> CreateHeaders(IReadOnlyDictionary<string, StringValues>? headers)
	{
		if (headers is null || headers.Count == 0)
			return s_emptyHeaders;

		Dictionary<string, StringValues> normalizedHeaders = new(StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in headers)
			normalizedHeaders[key] = value;

		return new ReadOnlyDictionary<string, StringValues>(normalizedHeaders);
	}
}
