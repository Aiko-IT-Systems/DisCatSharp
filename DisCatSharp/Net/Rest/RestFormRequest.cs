using System;
using System.Collections.Generic;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a form data HTTP request.
/// </summary>
internal sealed class RestFormRequest : BaseRestRequest
{
	/// <summary>
	/// Gets the form data sent with this request.
	/// </summary>
	public Dictionary<string, string> FormData { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RestRequest"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="formData">The form data.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	internal RestFormRequest(DiscordOAuth2Client client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, Dictionary<string, string> formData, IReadOnlyDictionary<string, string>? headers = null, double? ratelimitWaitOverride = null)
		: base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
	{
		this.FormData = formData;
	}
}
