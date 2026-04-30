using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Extensions.Primitives;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a transport-neutral ingress response for future endpoint handlers.
/// </summary>
public sealed class DiscordIngressResponse
{
	private static readonly IReadOnlyDictionary<string, StringValues> EmptyHeaders =
		new ReadOnlyDictionary<string, StringValues>(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressResponse" /> class.
	/// </summary>
	/// <param name="statusCode">The HTTP status code to return.</param>
	/// <param name="body">The response body.</param>
	/// <param name="contentType">The response content type.</param>
	/// <param name="headers">Additional response headers.</param>
	public DiscordIngressResponse(
		int statusCode,
		DiscordIngressPayload? body = null,
		string? contentType = null,
		IReadOnlyDictionary<string, StringValues>? headers = null
	)
	{
		if (statusCode is < 100 or > 599)
			throw new ArgumentOutOfRangeException(nameof(statusCode), "The ingress response status code must be a valid HTTP status code.");

		this.StatusCode = statusCode;
		this.Body = body ?? DiscordIngressPayload.Empty;
		this.ContentType = contentType;
		this.Headers = CreateHeaders(headers);
	}

	/// <summary>
	///     Gets the HTTP status code to return.
	/// </summary>
	public int StatusCode { get; }

	/// <summary>
	///     Gets the response body.
	/// </summary>
	public DiscordIngressPayload Body { get; }

	/// <summary>
	///     Gets the response content type.
	/// </summary>
	public string? ContentType { get; }

	/// <summary>
	///     Gets any additional response headers.
	/// </summary>
	public IReadOnlyDictionary<string, StringValues> Headers { get; }

	/// <summary>
	///     Creates a response without a body.
	/// </summary>
	/// <param name="statusCode">The HTTP status code to return.</param>
	/// <returns>A response without a body.</returns>
	public static DiscordIngressResponse Empty(int statusCode = 204) => new(statusCode);

	/// <summary>
	///     Creates a text response.
	/// </summary>
	/// <param name="statusCode">The HTTP status code to return.</param>
	/// <param name="content">The response text.</param>
	/// <param name="contentType">The content type to emit.</param>
	/// <returns>A text response.</returns>
	public static DiscordIngressResponse Text(int statusCode, string content, string contentType = "text/plain; charset=utf-8")
		=> new(statusCode, DiscordIngressPayload.FromString(content), contentType);

	/// <summary>
	///     Creates a JSON response.
	/// </summary>
	/// <param name="statusCode">The HTTP status code to return.</param>
	/// <param name="json">The JSON payload.</param>
	/// <returns>A JSON response.</returns>
	public static DiscordIngressResponse Json(int statusCode, string json)
		=> new(statusCode, DiscordIngressPayload.FromString(json), "application/json; charset=utf-8");

	private static IReadOnlyDictionary<string, StringValues> CreateHeaders(IReadOnlyDictionary<string, StringValues>? headers)
	{
		if (headers is null || headers.Count == 0)
			return EmptyHeaders;

		Dictionary<string, StringValues> normalizedHeaders = new(StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in headers)
			normalizedHeaders[key] = value;

		return new ReadOnlyDictionary<string, StringValues>(normalizedHeaders);
	}
}
