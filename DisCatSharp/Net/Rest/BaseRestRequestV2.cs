// BaseRestRequestV2.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisCatSharp.Net.V2;

public abstract class BaseRestRequestV2
{
	protected internal BaseRestRequestV2(
		BaseDiscordClient client,
		RateLimitBucketV2 bucket,
		Uri url,
		RestRequestMethod method,
		string route,
		IReadOnlyDictionary<string, string>? headers = null,
		double? ratelimitWaitOverride = null
	)
	{
		this.Discord = client;
		this.RateLimitBucketV2 = bucket;
		this.RequestTaskSource = new();
		this.Url = url;
		this.Method = method;
		this.Route = route;
		this.RateLimitWaitOverride = ratelimitWaitOverride;
		this.Headers = ProcessHeaders(headers);
	}

	protected internal BaseRestRequestV2(
		DiscordOAuth2Client client,
		RateLimitBucketV2 bucket,
		Uri url,
		RestRequestMethod method,
		string route,
		IReadOnlyDictionary<string, string>? headers = null,
		double? ratelimitWaitOverride = null
	)
	{
		this.OAuth2Client = client;
		this.RateLimitBucketV2 = bucket;
		this.RequestTaskSource = new();
		this.Url = url;
		this.Method = method;
		this.Route = route;
		this.RateLimitWaitOverride = ratelimitWaitOverride;
		this.Headers = ProcessHeaders(headers);
	}

	public BaseDiscordClient? Discord { get; }
	public DiscordOAuth2Client? OAuth2Client { get; }
	public TaskCompletionSource<RestResponseV2> RequestTaskSource { get; }
	public Uri Url { get; }
	public RestRequestMethod Method { get; }
	public string Route { get; }
	public IReadOnlyDictionary<string, string>? Headers { get; }
	public double? RateLimitWaitOverride { get; }
	internal RateLimitBucketV2 RateLimitBucketV2 { get; }

	private static IReadOnlyDictionary<string, string>? ProcessHeaders(IReadOnlyDictionary<string, string>? headers)
		=> headers?.ToDictionary(
			x => x.Key,
			x => Uri.EscapeDataString(x.Value));

	public Task<RestResponseV2> WaitForCompletionAsync()
		=> this.RequestTaskSource.Task;

	protected internal void SetCompleted(RestResponseV2 response)
		=> this.RequestTaskSource.SetResult(response);

	protected internal void SetFaulted(Exception ex)
		=> this.RequestTaskSource.SetException(ex);

	protected internal bool TrySetFaulted(Exception ex)
		=> this.RequestTaskSource.TrySetException(ex);
}
