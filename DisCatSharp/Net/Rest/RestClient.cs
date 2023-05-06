// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Exceptions;

using Microsoft.Extensions.Logging;

using Sentry;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a client used to make REST requests.
/// </summary>
internal sealed class RestClient : IDisposable
{
	/// <summary>
	/// Gets the route argument regex.
	/// </summary>
	private static Regex s_routeArgumentRegex { get; } = new(@":([a-z_]+)");

	/// <summary>
	/// Gets the http client.
	/// </summary>
	internal HttpClient HttpClient { get; }

	/// <summary>
	/// Gets the discord client.
	/// </summary>
	private readonly BaseDiscordClient _discord;

	/// <summary>
	/// Gets a value indicating whether debug is enabled.
	/// </summary>
	internal bool Debug { get; set; }

	/// <summary>
	/// Gets the logger.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	/// Gets the routes to hashes.
	/// </summary>
	private readonly ConcurrentDictionary<string, string> _routesToHashes;

	/// <summary>
	/// Gets the hashes to buckets.
	/// </summary>
	private readonly ConcurrentDictionary<string, RateLimitBucket> _hashesToBuckets;

	/// <summary>
	/// Gets the request queue.
	/// </summary>
	private readonly ConcurrentDictionary<string, int> _requestQueue;

	/// <summary>
	/// Gets the global rate limit event.
	/// </summary>
	private readonly AsyncManualResetEvent _globalRateLimitEvent;

	/// <summary>
	/// Gets a value indicating whether use reset after.
	/// </summary>
	private readonly bool _useResetAfter;

	private CancellationTokenSource _bucketCleanerTokenSource;
	private readonly TimeSpan _bucketCleanupDelay = TimeSpan.FromSeconds(60);
	private volatile bool _cleanerRunning;
	private Task _cleanerTask;
	private volatile bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="RestClient"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal RestClient(BaseDiscordClient client)
		: this(client.Configuration.Proxy, client.Configuration.HttpTimeout, client.Configuration.UseRelativeRatelimit, client.Logger)
	{
		this._discord = client;
		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(client));
		if (client.Configuration.Override != null)
		{
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-super-properties", client.Configuration.Override);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RestClient"/> class.
	/// This is for meta-clients, such as the webhook client.
	/// </summary>
	/// <param name="proxy">The proxy.</param>
	/// <param name="timeout">The timeout.</param>
	/// <param name="useRelativeRatelimit">Whether to use relative ratelimit.</param>
	/// <param name="logger">The logger.</param>
	internal RestClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRatelimit,
		ILogger logger)
	{
		this._logger = logger;

		var httphandler = new HttpClientHandler
		{
			UseCookies = false,
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
			UseProxy = proxy != null,
			Proxy = proxy
		};

		this.HttpClient = new HttpClient(httphandler)
		{
			BaseAddress = new Uri(Utilities.GetApiBaseUri(this._discord?.Configuration)),
			Timeout = timeout
		};

		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
		if (this._discord != null && this._discord.Configuration != null && this._discord.Configuration.Override != null)
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-super-properties", this._discord.Configuration.Override);

		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Discord-Locale", this._discord?.Configuration?.Locale ?? "en-US");

		this._routesToHashes = new ConcurrentDictionary<string, string>();
		this._hashesToBuckets = new ConcurrentDictionary<string, RateLimitBucket>();
		this._requestQueue = new ConcurrentDictionary<string, int>();

		this._globalRateLimitEvent = new AsyncManualResetEvent(true);
		this._useResetAfter = useRelativeRatelimit;
	}

	/// <summary>
	/// Gets a ratelimit bucket.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="routeParams">The route parameters.</param>
	/// <param name="url">The url.</param>
	/// <returns>A ratelimit bucket.</returns>
	public RateLimitBucket GetBucket(RestRequestMethod method, string route, object routeParams, out string url)
	{
		var rparamsProps = routeParams.GetType()
			.GetTypeInfo()
			.DeclaredProperties;
		var rparams = new Dictionary<string, string>();
		foreach (var xp in rparamsProps)
		{
			var val = xp.GetValue(routeParams);
			rparams[xp.Name] = val is string xs
				? xs
				: val is DateTime dt
				? dt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)
				: val is DateTimeOffset dto
				? dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)
				: val is IFormattable xf ? xf.ToString(null, CultureInfo.InvariantCulture) : val.ToString();
		}

		var guildId = rparams.ContainsKey("guild_id") ? rparams["guild_id"] : "";
		var channelId = rparams.ContainsKey("channel_id") ? rparams["channel_id"] : "";
		var webhookId = rparams.ContainsKey("webhook_id") ? rparams["webhook_id"] : "";

		// Create a generic route (minus major params) key
		// ex: POST:/channels/channel_id/messages
		var hashKey = RateLimitBucket.GenerateHashKey(method, route);

		// We check if the hash is present, using our generic route (without major params)
		// ex: in POST:/channels/channel_id/messages, out 80c17d2f203122d936070c88c8d10f33
		// If it doesn't exist, we create an unlimited hash as our initial key in the form of the hash key + the unlimited constant
		// and assign this to the route to hash cache
		// ex: this.RoutesToHashes[POST:/channels/channel_id/messages] = POST:/channels/channel_id/messages:unlimited
		var hash = this._routesToHashes.GetOrAdd(hashKey, RateLimitBucket.GenerateUnlimitedHash(method, route));

		// Next we use the hash to generate the key to obtain the bucket.
		// ex: 80c17d2f203122d936070c88c8d10f33:guild_id:506128773926879242:webhook_id
		// or if unlimited: POST:/channels/channel_id/messages:unlimited:guild_id:506128773926879242:webhook_id
		var bucketId = RateLimitBucket.GenerateBucketId(hash, guildId, channelId, webhookId);

		// If it's not in cache, create a new bucket and index it by its bucket id.
		var bucket = this._hashesToBuckets.GetOrAdd(bucketId, new RateLimitBucket(hash, guildId, channelId, webhookId));

		bucket.LastAttemptAt = DateTimeOffset.UtcNow;

		// Cache the routes for each bucket so it can be used for GC later.
		if (!bucket.RouteHashes.Contains(bucketId))
			bucket.RouteHashes.Add(bucketId);

		// Add the current route to the request queue, which indexes the amount
		// of requests occurring to the bucket id.
		_ = this._requestQueue.TryGetValue(bucketId, out var count);

		// Increment by one atomically due to concurrency
		this._requestQueue[bucketId] = Interlocked.Increment(ref count);

		// Start bucket cleaner if not already running.
		if (!this._cleanerRunning)
		{
			this._cleanerRunning = true;
			this._bucketCleanerTokenSource = new CancellationTokenSource();
			this._cleanerTask = Task.Run(this.CleanupBucketsAsync, this._bucketCleanerTokenSource.Token);
			this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task started.");
		}

		url = s_routeArgumentRegex.Replace(route, xm => rparams[xm.Groups[1].Value]);
		return bucket;
	}

	/// <summary>
	/// Executes the request.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	public Task ExecuteRequestAsync(BaseRestRequest request)
		=> request == null ? throw new ArgumentNullException(nameof(request)) : this.ExecuteRequestAsync(request, null, null);

	/// <summary>
	/// Executes the request.
	/// This is to allow proper rescheduling of the first request from a bucket.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="ratelimitTcs">The ratelimit task completion source.</param>
	private async Task ExecuteRequestAsync(BaseRestRequest request, RateLimitBucket bucket, TaskCompletionSource<bool> ratelimitTcs)
	{
		if (this._disposed)
			return;

		HttpResponseMessage res = default;

		try
		{
			await this._globalRateLimitEvent.WaitAsync().ConfigureAwait(false);

			bucket ??= request.RateLimitBucket;

			ratelimitTcs ??= await this.WaitForInitialRateLimit(bucket).ConfigureAwait(false);

			if (ratelimitTcs == null) // check rate limit only if we are not the probe request
			{
				var now = DateTimeOffset.UtcNow;

				await bucket.TryResetLimitAsync(now).ConfigureAwait(false);

				// Decrement the remaining number of requests as there can be other concurrent requests before this one finishes and has a chance to update the bucket
				if (Interlocked.Decrement(ref bucket.RemainingInternal) < 0)
				{
					this._logger.LogWarning(LoggerEvents.RatelimitDiag, "Request for {bucket} is blocked. Url: {url}", bucket.ToString(), request.Url.AbsoluteUri);
					var delay = bucket.Reset - now;
					var resetDate = bucket.Reset;

					if (this._useResetAfter)
					{
						delay = bucket.ResetAfter.Value;
						resetDate = bucket.ResetAfterOffset;
					}

					if (delay < new TimeSpan(-TimeSpan.TicksPerMinute))
					{
						this._logger.LogError(LoggerEvents.RatelimitDiag, "Failed to retrieve ratelimits - giving up and allowing next request for bucket");
						bucket.RemainingInternal = 1;
					}

					if (delay < TimeSpan.Zero)
						delay = TimeSpan.FromMilliseconds(100);

					this._logger.LogWarning(LoggerEvents.RatelimitPreemptive, "Preemptive ratelimit triggered - waiting until {0:yyyy-MM-dd HH:mm:ss zzz} ({1:c}).", resetDate, delay);
					Task.Delay(delay)
						.ContinueWith(_ => this.ExecuteRequestAsync(request, null, null))
						.LogTaskFault(this._logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");

					return;
				}
				this._logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {bucket} is allowed. Url: {url}", bucket.ToString(), request.Url.AbsoluteUri);
			}
			else
				this._logger.LogDebug(LoggerEvents.RatelimitDiag, "Initial request for {bucket} is allowed. Url: {url}", bucket.ToString(), request.Url.AbsoluteUri);

			var req = this.BuildRequest(request);

			if (this.Debug)
				this._logger.LogTrace(LoggerEvents.Misc, await req.Content.ReadAsStringAsync());

			var response = new RestResponse();
			try
			{
				if (this._disposed)
					return;

				res = await this.HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

				var bts = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				var txt = Utilities.UTF8.GetString(bts, 0, bts.Length);

				this._logger.LogTrace(LoggerEvents.RestRx, txt);

				response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
				response.Response = txt;
				response.ResponseCode = (int)res.StatusCode;
			}
			catch (HttpRequestException httpex)
			{
				this._logger.LogError(LoggerEvents.RestError, httpex, "Request to {0} triggered an HttpException", request.Url.AbsoluteUri);
				request.SetFaulted(httpex);
				this.FailInitialRateLimitTest(request, ratelimitTcs);
				return;
			}

			this.UpdateBucket(request, response, ratelimitTcs);

			Exception ex = null;
			Exception senex = null;
			switch (response.ResponseCode)
			{
				case 400:
				case 405:
					ex = new BadRequestException(request, response);
					senex = new Exception(ex.Message + "\nJson Response: " + (ex as BadRequestException).JsonMessage ?? "null", ex);
					break;

				case 401:
				case 403:
					ex = new UnauthorizedException(request, response);
					break;

				case 404:
					ex = new NotFoundException(request, response);
					break;

				case 413:
					ex = new RequestSizeException(request, response);
					break;

				case 429:
					ex = new RateLimitException(request, response);

					// check the limit info and requeue
					this.Handle429(response, out var wait, out var global);
					if (wait != null)
					{
						if (global)
						{
							bucket.IsGlobal = true;
							this._logger.LogError(LoggerEvents.RatelimitHit, "Global ratelimit hit, cooling down for {uri}", request.Url.AbsoluteUri);
							try
							{
								this._globalRateLimitEvent.Reset();
								await wait.ConfigureAwait(false);
							}
							finally
							{
								// we don't want to wait here until all the blocked requests have been run, additionally Set can never throw an exception that could be suppressed here
								_ = this._globalRateLimitEvent.SetAsync();
							}
							this.ExecuteRequestAsync(request, bucket, ratelimitTcs)
								.LogTaskFault(this._logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
						}
						else
						{
							if (this._discord is DiscordClient)
							{
								await (this._discord as DiscordClient)._rateLimitHit.InvokeAsync(this._discord as DiscordClient, new EventArgs.RateLimitExceptionEventArgs(this._discord.ServiceProvider)
								{
									Exception = ex as RateLimitException,
									ApiEndpoint = request.Url.AbsoluteUri
								});
							}
							this._logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, requeuing request to {url}", request.Url.AbsoluteUri);
							await wait.ConfigureAwait(false);
							this.ExecuteRequestAsync(request, bucket, ratelimitTcs)
								.LogTaskFault(this._logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
						}

						return;
					}
					break;

				case 500:
				case 502:
				case 503:
				case 504:
					ex = new ServerErrorException(request, response);
					senex = new Exception(ex.Message + "\nJson Response: " + (ex as ServerErrorException).JsonMessage ?? "null", ex);
					break;
			}

			if (ex != null)
			{
				if (senex != null)
				{
					Dictionary<string, object> debugInfo = new()
					{
						{ "route", request.Route },
						{ "time", DateTimeOffset.UtcNow }
					};
					senex.AddSentryContext("Request", debugInfo);
					SentrySdk.CaptureException(senex);
				}
				request.SetFaulted(ex);
			}
			else
				request.SetCompleted(response);
		}
		catch (Exception ex)
		{
			this._logger.LogError(LoggerEvents.RestError, ex, "Request to {0} triggered an exception", request.Url.AbsoluteUri);

			// if something went wrong and we couldn't get rate limits for the first request here, allow the next request to run
			if (bucket != null && ratelimitTcs != null && bucket.LimitTesting != 0)
				this.FailInitialRateLimitTest(request, ratelimitTcs);

			if (!request.TrySetFaulted(ex))
				throw;
		}
		finally
		{
			res?.Dispose();

			// Get and decrement active requests in this bucket by 1.
			_ = this._requestQueue.TryGetValue(bucket.BucketId, out var count);
			this._requestQueue[bucket.BucketId] = Interlocked.Decrement(ref count);

			// If it's 0 or less, we can remove the bucket from the active request queue,
			// along with any of its past routes.
			if (count <= 0)
			{
				foreach (var r in bucket.RouteHashes)
				{
					if (this._requestQueue.ContainsKey(r))
					{
						_ = this._requestQueue.TryRemove(r, out _);
					}
				}
			}
		}
	}

	/// <summary>
	/// Fails the initial rate limit test.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="ratelimitTcs">The ratelimit task completion source.</param>
	/// <param name="resetToInitial">Whether to reset to initial values.</param>
	private void FailInitialRateLimitTest(BaseRestRequest request, TaskCompletionSource<bool> ratelimitTcs, bool resetToInitial = false)
	{
		if (ratelimitTcs == null && !resetToInitial)
			return;

		var bucket = request.RateLimitBucket;

		bucket.LimitValid = false;
		bucket.LimitTestFinished = null;
		bucket.LimitTesting = 0;

		//Reset to initial values.
		if (resetToInitial)
		{
			this.UpdateHashCaches(request, bucket);
			bucket.Maximum = 0;
			bucket.RemainingInternal = 0;
			return;
		}

		// no need to wait on all the potentially waiting tasks
		_ = Task.Run(() => ratelimitTcs.TrySetResult(false));
	}

	/// <summary>
	/// Waits for the initial rate limit.
	/// </summary>
	/// <param name="bucket">The bucket.</param>
	private async Task<TaskCompletionSource<bool>> WaitForInitialRateLimit(RateLimitBucket bucket)
	{
		while (!bucket.LimitValid)
		{
			if (bucket.LimitTesting == 0)
			{
				if (Interlocked.CompareExchange(ref bucket.LimitTesting, 1, 0) == 0)
				{
					// if we got here when the first request was just finishing, we must not create the waiter task as it would signal ExecuteRequestAsync to bypass rate limiting
					if (bucket.LimitValid)
						return null;

					// allow exactly one request to go through without having rate limits available
					var ratelimitsTcs = new TaskCompletionSource<bool>();
					bucket.LimitTestFinished = ratelimitsTcs.Task;
					return ratelimitsTcs;
				}
			}
			// it can take a couple of cycles for the task to be allocated, so wait until it happens or we are no longer probing for the limits
			Task waitTask = null;
			while (bucket.LimitTesting != 0 && (waitTask = bucket.LimitTestFinished) == null)
				await Task.Yield();
			if (waitTask != null)
				await waitTask.ConfigureAwait(false);

			// if the request failed and the response did not have rate limit headers we have allow the next request and wait again, thus this is a loop here
		}
		return null;
	}

	/// <summary>
	/// Builds the request.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <returns>A http request message.</returns>
	private HttpRequestMessage BuildRequest(BaseRestRequest request)
	{
		var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.Url);
		if (request.Headers != null && request.Headers.Any())
			foreach (var kvp in request.Headers)
				req.Headers.Add(kvp.Key, kvp.Value);

		if (request is RestRequest nmprequest && !string.IsNullOrWhiteSpace(nmprequest.Payload))
		{
			this._logger.LogTrace(LoggerEvents.RestTx, nmprequest.Payload);

			req.Content = new StringContent(nmprequest.Payload);
			req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		}

		if (request is MultipartWebRequest mprequest)
		{
			this._logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

			var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

			req.Headers.Add("Connection", "keep-alive");
			req.Headers.Add("Keep-Alive", "600");

			var content = new MultipartFormDataContent(boundary);
			if (mprequest.Values != null && mprequest.Values.Any())
				foreach (var kvp in mprequest.Values)
					content.Add(new StringContent(kvp.Value), kvp.Key);

			var fileId = mprequest.OverwriteFileIdStart ?? 0;

			if (mprequest.Files != null && mprequest.Files.Any())
			{
				foreach (var f in mprequest.Files)
				{
					var name = $"files[{fileId.ToString(CultureInfo.InvariantCulture)}]";
					content.Add(new StreamContent(f.Value), name, f.Key);
					fileId++;
				}
			}

			req.Content = content;
		}

		if (request is MultipartStickerWebRequest mpsrequest)
		{
			this._logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

			var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

			req.Headers.Add("Connection", "keep-alive");
			req.Headers.Add("Keep-Alive", "600");

			var sc = new StreamContent(mpsrequest.File.Stream);

			if (mpsrequest.File.ContentType != null)
				sc.Headers.ContentType = new MediaTypeHeaderValue(mpsrequest.File.ContentType);

			var fileName = mpsrequest.File.FileName;

			if (mpsrequest.File.FileType != null)
				fileName += '.' + mpsrequest.File.FileType;

			var content = new MultipartFormDataContent(boundary)
			{
				{ new StringContent(mpsrequest.Name), "name" },
				{ new StringContent(mpsrequest.Tags), "tags" },
				{ new StringContent(mpsrequest.Description), "description" },
				{ sc, "file", fileName }
			};

			req.Content = content;
		}
		return req;
	}

	/// <summary>
	/// Handles the HTTP 429 status.
	/// </summary>
	/// <param name="response">The response.</param>
	/// <param name="waitTask">The wait task.</param>
	/// <param name="global">If true, global.</param>
	private void Handle429(RestResponse response, out Task waitTask, out bool global)
	{
		waitTask = null;
		global = false;

		if (response.Headers == null)
			return;
		var hs = response.Headers;

		// handle the wait
		if (hs.TryGetValue("Retry-After", out var retryAfterRaw))
		{
			var retryAfter = TimeSpan.FromSeconds(int.Parse(retryAfterRaw, CultureInfo.InvariantCulture));
			waitTask = Task.Delay(retryAfter);
		}

		// check if global b1nzy
		if (hs.TryGetValue("X-RateLimit-Global", out var isGlobal) && isGlobal.ToLowerInvariant() == "true")
		{
			// global
			global = true;
		}
	}

	/// <summary>
	/// Updates the bucket.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="response">The response.</param>
	/// <param name="ratelimitTcs">The ratelimit task completion source.</param>
	private void UpdateBucket(BaseRestRequest request, RestResponse response, TaskCompletionSource<bool> ratelimitTcs)
	{
		var bucket = request.RateLimitBucket;

		if (response.Headers == null)
		{
			if (response.ResponseCode != 429) // do not fail when ratelimit was or the next request will be scheduled hitting the rate limit again
				this.FailInitialRateLimitTest(request, ratelimitTcs);
			return;
		}

		var hs = response.Headers;

		if (hs.TryGetValue("X-RateLimit-Scope", out var scope))
		{
			bucket.Scope = scope;
		}


		if (hs.TryGetValue("X-RateLimit-Global", out var isGlobal) && isGlobal.ToLowerInvariant() == "true")
		{
			if (response.ResponseCode != 429)
			{
				bucket.IsGlobal = true;
				this.FailInitialRateLimitTest(request, ratelimitTcs);
			}

			return;
		}

		var r1 = hs.TryGetValue("X-RateLimit-Limit", out var usesMax);
		var r2 = hs.TryGetValue("X-RateLimit-Remaining", out var usesLeft);
		var r3 = hs.TryGetValue("X-RateLimit-Reset", out var reset);
		var r4 = hs.TryGetValue("X-Ratelimit-Reset-After", out var resetAfter);
		var r5 = hs.TryGetValue("X-Ratelimit-Bucket", out var hash);

		if (!r1 || !r2 || !r3 || !r4)
		{
			//If the limits were determined before this request, make the bucket initial again.
			if (response.ResponseCode != 429)
				this.FailInitialRateLimitTest(request, ratelimitTcs, ratelimitTcs == null);

			return;
		}

		var clientTime = DateTimeOffset.UtcNow;
		var resetTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(double.Parse(reset, CultureInfo.InvariantCulture));
		var serverTime = clientTime;
		if (hs.TryGetValue("Date", out var rawDate))
			serverTime = DateTimeOffset.Parse(rawDate, CultureInfo.InvariantCulture).ToUniversalTime();

		var resetDelta = resetTime - serverTime;
		//var difference = clientTime - serverTime;
		//if (Math.Abs(difference.TotalSeconds) >= 1)
		////    this.Logger.LogMessage(LogLevel.DebugBaseDiscordClient.RestEventId,  $"Difference between machine and server time: {difference.TotalMilliseconds.ToString("#,##0.00", CultureInfo.InvariantCulture)}ms", DateTime.Now);
		//else
		//    difference = TimeSpan.Zero;

		if (request.RateLimitWaitOverride.HasValue)
			resetDelta = TimeSpan.FromSeconds(request.RateLimitWaitOverride.Value);
		var newReset = clientTime + resetDelta;

		if (this._useResetAfter)
		{
			bucket.ResetAfter = TimeSpan.FromSeconds(double.Parse(resetAfter, CultureInfo.InvariantCulture));
			newReset = clientTime + bucket.ResetAfter.Value + (request.RateLimitWaitOverride.HasValue
				? resetDelta
				: TimeSpan.Zero);
			bucket.ResetAfterOffset = newReset;
		}
		else
			bucket.Reset = newReset;

		var maximum = int.Parse(usesMax, CultureInfo.InvariantCulture);
		var remaining = int.Parse(usesLeft, CultureInfo.InvariantCulture);

		if (ratelimitTcs != null)
		{
			// initial population of the ratelimit data
			bucket.SetInitialValues(maximum, remaining, newReset);

			_ = Task.Run(() => ratelimitTcs.TrySetResult(true));
		}
		else
		{
			// only update the bucket values if this request was for a newer interval than the one
			// currently in the bucket, to avoid issues with concurrent requests in one bucket
			// remaining is reset by TryResetLimit and not the response, just allow that to happen when it is time
			if (bucket.NextReset == 0)
				bucket.NextReset = newReset.UtcTicks;
		}

		this.UpdateHashCaches(request, bucket, hash);
	}

	/// <summary>
	/// Updates the hash caches.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="newHash">The new hash.</param>
	private void UpdateHashCaches(BaseRestRequest request, RateLimitBucket bucket, string newHash = null)
	{
		var hashKey = RateLimitBucket.GenerateHashKey(request.Method, request.Route);

		if (!this._routesToHashes.TryGetValue(hashKey, out var oldHash))
			return;

		// This is an unlimited bucket, which we don't need to keep track of.
		if (newHash == null)
		{
			_ = this._routesToHashes.TryRemove(hashKey, out _);
			_ = this._hashesToBuckets.TryRemove(bucket.BucketId, out _);
			return;
		}

		// Only update the hash once, due to a bug on Discord's end.
		// This will cause issues if the bucket hashes are dynamically changed from the API while running,
		// in which case, Dispose will need to be called to clear the caches.
		if (bucket.IsUnlimited && newHash != oldHash)
		{
			this._logger.LogDebug(LoggerEvents.RestHashMover, "Updating hash in {0}: \"{1}\" -> \"{2}\"", hashKey, oldHash, newHash);
			var bucketId = RateLimitBucket.GenerateBucketId(newHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

			_ = this._routesToHashes.AddOrUpdate(hashKey, newHash, (key, oldHash) =>
			{
				bucket.Hash = newHash;

				var oldBucketId = RateLimitBucket.GenerateBucketId(oldHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

				// Remove the old unlimited bucket.
				_ = this._hashesToBuckets.TryRemove(oldBucketId, out _);
				_ = this._hashesToBuckets.AddOrUpdate(bucketId, bucket, (key, oldBucket) => bucket);

				return newHash;
			});
		}

		return;
	}

	/// <summary>
	/// Cleans the buckets.
	/// </summary>
	private async Task CleanupBucketsAsync()
	{
		while (!this._bucketCleanerTokenSource.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(this._bucketCleanupDelay, this._bucketCleanerTokenSource.Token).ConfigureAwait(false);
			}
			catch { }

			if (this._disposed)
				return;

			//Check and clean request queue first in case it wasn't removed properly during requests.
			foreach (var key in this._requestQueue.Keys)
			{
				var bucket = this._hashesToBuckets.Values.FirstOrDefault(x => x.RouteHashes.Contains(key));

				if (bucket == null || (bucket != null && bucket.LastAttemptAt.AddSeconds(5) < DateTimeOffset.UtcNow))
					_ = this._requestQueue.TryRemove(key, out _);
			}

			var removedBuckets = 0;
			StringBuilder bucketIdStrBuilder = default;

			foreach (var kvp in this._hashesToBuckets)
			{
				bucketIdStrBuilder ??= new StringBuilder();

				var key = kvp.Key;
				var value = kvp.Value;

				// Don't remove the bucket if it's currently being handled by the rest client, unless it's an unlimited bucket.
				if (this._requestQueue.ContainsKey(value.BucketId) && !value.IsUnlimited)
					continue;

				var resetOffset = this._useResetAfter ? value.ResetAfterOffset : value.Reset;

				// Don't remove the bucket if it's reset date is less than now + the additional wait time, unless it's an unlimited bucket.
				if (!value.IsUnlimited && (resetOffset > DateTimeOffset.UtcNow || DateTimeOffset.UtcNow - resetOffset < this._bucketCleanupDelay))
					continue;

				_ = this._hashesToBuckets.TryRemove(key, out _);
				removedBuckets++;
				bucketIdStrBuilder.Append(value.BucketId + ", ");
			}

			if (removedBuckets > 0)
				this._logger.LogDebug(LoggerEvents.RestCleaner, "Removed {0} unused bucket{1}: [{2}]", removedBuckets, removedBuckets > 1 ? "s" : string.Empty, bucketIdStrBuilder.ToString().TrimEnd(',', ' '));

			if (this._hashesToBuckets.IsEmpty)
				break;
		}

		if (!this._bucketCleanerTokenSource.IsCancellationRequested)
			this._bucketCleanerTokenSource.Cancel();

		this._cleanerRunning = false;
		this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
	}

	~RestClient()
	{
		this.Dispose();
	}

	/// <summary>
	/// Disposes the rest client.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;

		this._globalRateLimitEvent.Reset();

		if (this._bucketCleanerTokenSource?.IsCancellationRequested == false)
		{
			this._bucketCleanerTokenSource?.Cancel();
			this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
		}

		try
		{
			this._cleanerTask?.Dispose();
			this._bucketCleanerTokenSource?.Dispose();
			this.HttpClient?.Dispose();
		}
		catch { }

		this._routesToHashes.Clear();
		this._hashesToBuckets.Clear();
		this._requestQueue.Clear();
		GC.SuppressFinalize(this);
	}
}
