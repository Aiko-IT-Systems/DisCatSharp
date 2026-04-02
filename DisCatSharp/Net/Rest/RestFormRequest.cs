using System;
using System.Collections.Generic;
using System.Threading;

namespace DisCatSharp.Net;

/// <summary>
///     Represents a form data HTTP request.
/// </summary>
internal sealed class RestFormRequest : BaseRestRequest
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RestRequest" /> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="formData">The form data.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	/// <param name="cancellationToken">Optional cancellation token for caller-initiated cancellation.</param>
	internal RestFormRequest(DiscordOAuth2Client client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, Dictionary<string, string> formData, IReadOnlyDictionary<string, string>? headers = null, double? ratelimitWaitOverride = null, CancellationToken cancellationToken = default)
		: base(client, bucket, url, method, route, headers, ratelimitWaitOverride, cancellationToken)
	{
		this.FormData = formData;
	}

	/// <summary>
	///     Gets the form data sent with this request.
	/// </summary>
	public Dictionary<string, string> FormData { get; }
}
