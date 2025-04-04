using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Net.V2;

internal sealed class RestClientV2 : IDisposable
{
	private readonly ConcurrentDictionary<string, RateLimitBucketV2> _buckets = new();
	private readonly Timer _cleanupTimer;

	private readonly BaseDiscordClient? _discord;
	private readonly HttpClient _httpClient;
	private readonly ILogger _logger;
	private readonly ConcurrentDictionary<string, int> _requestQueue = new();
	private readonly ConcurrentDictionary<string, string> _routeHashes = new();
	private bool _cleanerRunning;
	private long _disposedState;

	internal RestClientV2(BaseDiscordClient client)
		: this(client.Configuration, client.Logger)
	{
		this._discord = client;
		this._cleanupTimer = new(_ => this.CleanupBuckets(), null,
			TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
		this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.AUTHORIZATION, Utilities.GetFormattedToken(client));
		this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_LOCALE, client.Configuration.Locale);
		if (!string.IsNullOrWhiteSpace(client.Configuration.Timezone))
			this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_TIMEZONE, client.Configuration.Timezone);
		if (!string.IsNullOrWhiteSpace(client.Configuration.Override))
			this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.SUPER_PROPERTIES, client.Configuration.Override);
	}

	internal RestClientV2(
		DiscordConfiguration configuration,
		ILogger logger
	)
	{
		this._logger = logger;

		var httphandler = new HttpClientHandler
		{
			UseCookies = false,
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
			UseProxy = configuration.Proxy != null,
			Proxy = configuration.Proxy
		};

		this._httpClient = new(httphandler)
		{
			BaseAddress = new(Utilities.GetApiBaseUri(configuration)),
			Timeout = configuration.HttpTimeout
		};

		this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.USER_AGENT, Utilities.GetUserAgent());
		if (this._discord is { Configuration: not null })
		{
			this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_LOCALE, configuration.Locale);
			if (!string.IsNullOrWhiteSpace(configuration.Timezone))
				this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_TIMEZONE, configuration.Timezone);
			if (!string.IsNullOrWhiteSpace(configuration.Override))
				this._httpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.SUPER_PROPERTIES, configuration.Override);
		}
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref this._disposedState, 1) == 1)
			return;

		this._cleanupTimer.Dispose();
		foreach (var bucket in this._buckets.Values)
			bucket.Dispose();

		this._buckets.Clear();
		this._requestQueue.Clear();
		this._logger.LogInformation("[V2] Rest client disposed");
	}

	public RateLimitBucketV2 GetBucketV2(RestRequestMethod method, string route, object routeParams, out string url)
	{
		this.ThrowIfDisposed();
		var rparamsProps = routeParams.GetType()
			.GetTypeInfo()
			.DeclaredProperties;
		var rparams = new Dictionary<string, string>();
		foreach (var xp in rparamsProps)
		{
			var val = xp.GetValue(routeParams);
			rparams[xp.Name] = val switch
			{
				string xs => xs,
				DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
				DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
				IFormattable xf => xf.ToString(null, CultureInfo.InvariantCulture),
				_ => val.ToString()
			};
		}

		var parameters = string.Join(":",
			rparams.GetValueOrDefault("guild_id"),
			rparams.GetValueOrDefault("channel_id"),
			rparams.GetValueOrDefault("webhook_id"));

		var hashKey = $"{method}:{route}";

		var bucketHash = this._routeHashes.GetOrAdd(hashKey, _ => this.GenerateInitialHashV2(method, route)
		);

		var bucketId = $"{bucketHash}:{parameters}";
		var bucket = this._buckets.GetOrAdd(bucketId, id =>
			new(id, this._logger)
		);

		bucket.LastAttempt = DateTimeOffset.UtcNow;

		this._requestQueue.AddOrUpdate(bucketId, 1, (_, count) => count + 1);
		this.StartCleanupIfNeeded();

		this._logger.LogDebug(LoggerEvents.RatelimitDiag, "[V2] Acquired bucket: {Bucket}", bucketId);
		url = CommonRegEx.HttpRouteRegex().Replace(route, xm => rparams[xm.Groups[1].Value]);
		return bucket;
	}

	public async Task<RestResponseV2> ExecuteRequestV2Async(BaseRestRequestV2 request)
	{
		this.ThrowIfDisposed();

		try
		{
			await request.RateLimitBucketV2.WaitAsync().ConfigureAwait(false);

			var message = this.BuildRequestMessageV2(request);
			var response = await this._httpClient.SendAsync(message).ConfigureAwait(false);

			this.UpdateBucketFromResponseV2(request.RateLimitBucketV2, response, request);
			var res = await this.ProcessResponseV2(response);

			if (res.ResponseCode is HttpStatusCode.TooManyRequests)
			{
				this._logger.LogWarning(LoggerEvents.RatelimitHit, "[V2] Rate limit hit on {Bucket}. Requeue.. Retrying at: {Retry}", request.RateLimitBucketV2.BucketId, request.RateLimitBucketV2.Reset.DateTime);
				await request.RateLimitBucketV2.WaitAsync();
				return await this.ExecuteRequestV2Async(request);
			}
			else
				return res;
		}
		catch (Exception ex)
		{
			this._logger.LogError(ex, "[V2] Request failed to {Url}", request.Url);
			request.RateLimitBucketV2.Release();
			return new()
			{
				IsFaulted = true
			};
		}
		finally
		{
			this._requestQueue.AddOrUpdate(
				request.RateLimitBucketV2.BucketId,
				0,
				(_, count) => Math.Max(0, count - 1)
			);
		}
	}

	private HttpRequestMessage BuildRequestMessageV2(BaseRestRequestV2 request)
	{
		var message = new HttpRequestMessage(
			new(request.Method.ToString()),
			request.Url
		);

		var req = new HttpRequestMessage(new(request.Method.ToString()), request.Url);
		if (request.Headers is not null && request.Headers.Any())
			foreach (var kvp in request.Headers)
				switch (kvp.Key)
				{
					case "Bearer":
						message.Headers.Authorization = new(CommonHeaders.AUTHORIZATION_BEARER, kvp.Value);
						break;
					case "Basic":
						message.Headers.Authorization = new(CommonHeaders.AUTHORIZATION_BASIC, kvp.Value);
						break;
					default:
						message.Headers.Add(kvp.Key, kvp.Value);
						break;
				}

		switch (request)
		{
			case RestRequestV2 nmprequest when !string.IsNullOrWhiteSpace(nmprequest.Payload):
				this._logger.LogTrace(LoggerEvents.RestTx, nmprequest.Payload);

				message.Content = new StringContent(nmprequest.Payload);
				message.Content.Headers.ContentType = new("application/json");
				break;
			case RestFormRequestV2 formRequest:
				message.Content = new FormUrlEncodedContent(formRequest.FormData);
				message.Content.Headers.ContentType = new("application/x-www-form-urlencoded");
				break;
			case MultipartWebRequestV2 mprequest:
			{
				this._logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

				var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

				message.Headers.Add(CommonHeaders.CONNECTION, CommonHeaders.CONNECTION_KEEP_ALIVE);
				message.Headers.Add(CommonHeaders.KEEP_ALIVE, "600");

				var content = new MultipartFormDataContent(boundary);
				if (mprequest.Values is not null && mprequest.Values.Any())
					foreach (var kvp in mprequest.Values)
						content.Add(new StringContent(kvp.Value), kvp.Key);

				var fileId = mprequest.OverwriteFileIdStart ?? 0;

				if (mprequest.Files is not null && mprequest.Files.Any())
					foreach (var f in mprequest.Files)
					{
						var name = $"files[{fileId.ToString(CultureInfo.InvariantCulture)}]";
						content.Add(new StreamContent(f.Value), name, f.Key);
						fileId++;
					}

				message.Content = content;
				break;
			}
			case MultipartStickerWebRequestV2 mpsrequest:
			{
				this._logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

				var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

				message.Headers.Add(CommonHeaders.CONNECTION, CommonHeaders.CONNECTION_KEEP_ALIVE);
				message.Headers.Add(CommonHeaders.KEEP_ALIVE, "600");

				var sc = new StreamContent(mpsrequest.File.Stream);

				if (mpsrequest.File.ContentType is not null)
					sc.Headers.ContentType = new(mpsrequest.File.ContentType);

				var fileName = mpsrequest.File.Filename;

				if (mpsrequest.File.FileType is not null)
					fileName += '.' + mpsrequest.File.FileType;

				var content = new MultipartFormDataContent(boundary)
				{
					{ new StringContent(mpsrequest.Name), "name" },
					{ new StringContent(mpsrequest.Tags), "tags" }
				};

				if (!string.IsNullOrEmpty(mpsrequest.Description))
					content.Add(new StringContent(mpsrequest.Description), "description");

				content.Add(sc, "file", fileName);

				message.Content = content;
				break;
			}
		}

		return message;
	}

	private void UpdateBucketFromResponseV2(RateLimitBucketV2 bucket, HttpResponseMessage response, BaseRestRequestV2 request)
	{
		try
		{
			if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limits) &&
			    response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining) &&
			    response.Headers.TryGetValues("X-RateLimit-Reset", out var resets))
			{
				var resetUnix = double.Parse(resets.First(), CultureInfo.InvariantCulture);
				bucket.UpdateLimits(
					int.Parse(remaining.First()),
					int.Parse(limits.First()),
					DateTimeOffset.FromUnixTimeSeconds((long)resetUnix)
				);
			}

			if (response.Headers.TryGetValues("X-RateLimit-Bucket", out var newHashes))
			{
				var newHash = newHashes.First();
				var routeKey = $"{request.Method}:{request.Route}";
				if (this._routeHashes.TryGetValue(routeKey, out var currentHash) && currentHash != newHash)
				{
					this._routeHashes.AddOrUpdate(routeKey, newHash, (_, _) => newHash);
					this._logger.LogDebug("[V2] Updated route hash to {Hash}", newHash);
				}
			}

			if (response.StatusCode == HttpStatusCode.TooManyRequests)
			{
				var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
				bucket.SetReset(DateTimeOffset.UtcNow + retryAfter);
				this._logger.LogWarning(LoggerEvents.RatelimitHit, "[V2] Rate limit hit on {Bucket}. Retry after: {Retry}", bucket.BucketId, retryAfter);
			}
		}
		catch (Exception ex)
		{
			this._logger.LogWarning(ex, "[V2] Failed to update bucket from headers");
		}
	}

	private void CleanupBuckets()
	{
		var cutoff = DateTimeOffset.UtcNow.AddMinutes(-30);
		foreach (var (id, bucket) in this._buckets.ToArray())
			if (bucket.LastAttempt < cutoff && !this.IsBucketActiveV2(bucket))
			{
				this._buckets.TryRemove(id, out _);
				bucket.Dispose();
				this._logger.LogDebug("[V2] Cleaned up bucket: {Bucket}", id);
			}
	}

	private bool IsBucketActiveV2(RateLimitBucketV2 bucket)
		=> this._requestQueue.TryGetValue(bucket.BucketId, out var count) && count > 0;

	private void StartCleanupIfNeeded()
	{
		if (this._cleanerRunning) return;

		lock (this._cleanupTimer)
		{
			if (!this._cleanerRunning)
			{
				this._cleanerRunning = true;
				this._cleanupTimer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(5));
				this._logger.LogDebug("[V2] Bucket cleanup started");
			}
		}
	}

	private void ThrowIfDisposed()
	{
		if (Interlocked.Read(ref this._disposedState) == 1)
			throw new ObjectDisposedException("RestClientV2");
	}

	private string GenerateInitialHashV2(RestRequestMethod method, string route)
	{
		var input = $"{method}:{route}";
		var hash = input.Aggregate(0, (h, c) => h = (h * 31) + c);
		return $"v2_{Math.Abs(hash):x8}";
	}

	private async Task<RestResponseV2> ProcessResponseV2(HttpResponseMessage response)
	{
		var restResponse = new RestResponseV2
		{
			ResponseCode = response.StatusCode,
			Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
		};

		try
		{
			restResponse.Response = await response.Content.ReadAsStringAsync();
			return restResponse;
		}
		catch
		{
			restResponse.IsFaulted = true;
			return restResponse;
		}
	}
}
