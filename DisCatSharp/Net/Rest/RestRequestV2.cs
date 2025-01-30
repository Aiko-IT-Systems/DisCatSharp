// RestRequestV2.cs

using System;
using System.Collections.Generic;

namespace DisCatSharp.Net.V2;

public sealed class RestRequestV2 : BaseRestRequestV2
{
	public RestRequestV2(
		BaseDiscordClient client,
		RateLimitBucketV2 bucket,
		Uri url,
		RestRequestMethod method,
		string route,
		IReadOnlyDictionary<string, string>? headers = null,
		string? payload = null,
		double? ratelimitWaitOverride = null
	)
		: base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
	{
		this.Payload = payload;
	}

	public string? Payload { get; }
}
