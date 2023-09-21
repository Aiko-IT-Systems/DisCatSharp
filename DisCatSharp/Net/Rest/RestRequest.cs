using System;
using System.Collections.Generic;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a non-multipart HTTP request.
/// </summary>
internal sealed class RestRequest : BaseRestRequest
{
	/// <summary>
	/// Gets the payload sent with this request.
	/// </summary>
	public string? Payload { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RestRequest"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="payload">The payload.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	internal RestRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string>? headers = null, string? payload = null, double? ratelimitWaitOverride = null)
		: base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
	{
		this.Payload = payload;
	}
}
