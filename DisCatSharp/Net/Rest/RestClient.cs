using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Exceptions;

using Microsoft.Extensions.Logging;

using Sentry;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a client used to make REST requests.
/// </summary>
[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
internal sealed class RestClient : IDisposable
{
	/// <summary>
	/// Gets the http client.
	/// </summary>
	internal HttpClient HttpClient { get; }

	/// <summary>
	/// Gets the discord client.
	/// </summary>
	private readonly BaseDiscordClient? _discord;

	/// <summary>
	/// Gets a value indicating whether debug is enabled.
	/// </summary>
	internal bool Debug { get; set; } = false;

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

	/// <summary>
	/// Gets the bucket cleaner token source.
	/// </summary>
	private CancellationTokenSource? _bucketCleanerTokenSource;

	/// <summary>
	/// Gets the bucket cleanup delay.
	/// </summary>
	private readonly TimeSpan _bucketCleanupDelay = TimeSpan.FromSeconds(60);

	/// <summary>
	/// Gets whether the bucket cleaner is running.
	/// </summary>
	private volatile bool _cleanerRunning;

	/// <summary>
	/// Gets the cleaner task.
	/// </summary>
	private Task? _cleanerTask;

	/// <summary>
	/// Gets whether the client is disposed.
	/// </summary>
	private volatile bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="RestClient"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal RestClient(BaseDiscordClient client)
		: this(client.Configuration.Proxy, client.Configuration.HttpTimeout, client.Configuration.UseRelativeRatelimit, client.Logger)
	{
		this._discord = client;

		this.Debug = this._discord.Configuration.MinimumLogLevel is LogLevel.Trace;

		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.AUTHORIZATION, Utilities.GetFormattedToken(client));
		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_LOCALE, client.Configuration.Locale);
		if (!string.IsNullOrWhiteSpace(client.Configuration.Timezone))
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_TIMEZONE, client.Configuration.Timezone);
		if (!string.IsNullOrWhiteSpace(client.Configuration.Override))
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.SUPER_PROPERTIES, client.Configuration.Override);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RestClient"/> class.
	/// This is for meta-clients, such as the <see cref="DiscordWebhookClient"/> and <see cref="DiscordOAuth2Client"/>.
	/// </summary>
	/// <param name="proxy">The proxy.</param>
	/// <param name="timeout">The timeout.</param>
	/// <param name="useRelativeRatelimit">Whether to use relative ratelimit.</param>
	/// <param name="logger">The logger.</param>
	internal RestClient(
		IWebProxy? proxy,
		TimeSpan timeout,
		bool useRelativeRatelimit,
		ILogger logger
	)
	{
		this._logger = logger;

		var httphandler = new HttpClientHandler
		{
			UseCookies = false,
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
			UseProxy = proxy != null,
			Proxy = proxy
		};

		this.HttpClient = new(httphandler)
		{
			BaseAddress = new(Utilities.GetApiBaseUri(this._discord?.Configuration)),
			Timeout = timeout
		};

		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.USER_AGENT, Utilities.GetUserAgent());
		if (this._discord is { Configuration: not null })
		{
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_LOCALE, this._discord.Configuration.Locale);
			if (!string.IsNullOrWhiteSpace(this._discord.Configuration.Timezone))
				this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_TIMEZONE, this._discord.Configuration.Timezone);
			if (!string.IsNullOrWhiteSpace(this._discord.Configuration.Override))
				this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.SUPER_PROPERTIES, this._discord.Configuration.Override);
		}

		this._routesToHashes = new();
		this._hashesToBuckets = new();
		this._requestQueue = new();

		this._globalRateLimitEvent = new(true);
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
			rparams[xp.Name] = val switch
			{
				string xs => xs,
				DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
				DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
				IFormattable xf => xf.ToString(null, CultureInfo.InvariantCulture),
				_ => val.ToString()
			};
		}

		var guildId = rparams.GetValueOrDefault("guild_id", "");
		var channelId = rparams.GetValueOrDefault("channel_id", "");
		var webhookId = rparams.GetValueOrDefault("webhook_id", "");

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
			this._bucketCleanerTokenSource = new();
			this._cleanerTask = Task.Run(this.CleanupBucketsAsync, this._bucketCleanerTokenSource.Token);
			this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task started.");
		}

		url = CommonRegEx.HttpRouteRegex().Replace(route, xm => rparams[xm.Groups[1].Value]);
		return bucket;
	}

	/// <summary>
	/// Executes the request.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	public Task ExecuteRequestAsync(BaseRestRequest request, bool targetDebug = false)
		=> request is null ? throw new ArgumentNullException(nameof(request)) : this.ExecuteRequestAsync(request, null, null, targetDebug);

	/// <summary>
	/// Executes the form data request.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	public Task ExecuteFormRequestAsync(BaseRestRequest request, bool targetDebug = false)
		=> request is null ? throw new ArgumentNullException(nameof(request)) : this.ExecuteFormRequestAsync(request, null, null, targetDebug);

	/// <summary>
	/// Executes the form data request.
	/// This is to allow proper rescheduling of the first request from a bucket.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="ratelimitTcs">The ratelimit task completion source.</param>
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	private async Task ExecuteFormRequestAsync(BaseRestRequest request, RateLimitBucket? bucket, TaskCompletionSource<bool>? ratelimitTcs, bool targetDebug = false)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		if (targetDebug)
			Console.WriteLine("Meow");

		HttpResponseMessage? res = default;

		try
		{
			await this._globalRateLimitEvent.WaitAsync().ConfigureAwait(false);

			bucket ??= request.RateLimitBucket;

			ratelimitTcs ??= await this.WaitForInitialRateLimit(bucket).ConfigureAwait(false);

			if (ratelimitTcs is null) // check rate limit only if we are not the probe request
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

					this._logger.LogWarning(LoggerEvents.RatelimitPreemptive, "Preemptive ratelimit triggered - waiting until {ResetDate:yyyy-MM-dd HH:mm:ss zzz} ({Delay:c})", resetDate, delay);
					Task.Delay(delay)
						.ContinueWith(_ => this.ExecuteFormRequestAsync(request, null, null))
						.LogTaskFault(this._logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");

					return;
				}

				this._logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {bucket} is allowed. Url: {url}", bucket.ToString(), request.Url.AbsoluteUri);
			}
			else
				this._logger.LogDebug(LoggerEvents.RatelimitDiag, "Initial request for {bucket} is allowed. Url: {url}", bucket.ToString(), request.Url.AbsoluteUri);

			var req = this.BuildFormRequest(request);

			if (this.Debug && req.Content is not null)
				this._logger.Log(LogLevel.Trace, LoggerEvents.RestTx, "Rest Form Request Content:\n{Content}", await req.Content.ReadAsStringAsync()!);

			var response = new RestResponse();
			try
			{
				ObjectDisposedException.ThrowIf(this._disposed, this);

				res = await this.HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

				var bts = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				var txt = Utilities.UTF8.GetString(bts, 0, bts.Length);

				if (this.Debug)
					this._logger.Log(LogLevel.Trace, LoggerEvents.RestRx, "Rest Form Response Content: {Content}", txt);

				response.Headers = res.Headers?.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
				response.Response = txt;
				response.ResponseCode = res.StatusCode;
			}
			catch (HttpRequestException httpex)
			{
				this._logger.LogError(LoggerEvents.RestError, httpex, "Request to {Url} triggered an HttpException", request.Url.AbsoluteUri);
				request.SetFaulted(httpex);
				this.FailInitialRateLimitTest(request, ratelimitTcs);
				return;
			}

			this.UpdateBucket(request, response, ratelimitTcs);

			Exception? ex = null;

			switch (response.ResponseCode)
			{
				case HttpStatusCode.BadRequest:
				case HttpStatusCode.MethodNotAllowed:
					ex = new BadRequestException(request, response);
					break;

				case HttpStatusCode.Unauthorized:
				case HttpStatusCode.Forbidden:
					ex = new UnauthorizedException(request, response);
					break;

				case HttpStatusCode.NotFound:
					ex = new NotFoundException(request, response);
					break;

				case HttpStatusCode.RequestEntityTooLarge:
					ex = new RequestSizeException(request, response);
					break;

				case HttpStatusCode.TooManyRequests:
					ex = new RateLimitException(request, response);

					// check the limit info and requeue
					this.Handle429(response, out var wait, out var global);
					if (wait is not null)
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
							this._logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, requeuing request to {url}", request.Url.AbsoluteUri);
							await wait.ConfigureAwait(false);
							this.ExecuteRequestAsync(request, bucket, ratelimitTcs)
								.LogTaskFault(this._logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
						}

						return;
					}

					break;

				case HttpStatusCode.InternalServerError:
				case HttpStatusCode.BadGateway:
				case HttpStatusCode.ServiceUnavailable:
				case HttpStatusCode.GatewayTimeout:
					ex = new ServerErrorException(request, response);
					break;
			}

			if (ex is not null)
				request.SetFaulted(ex);
			else
				request.SetCompleted(response);
		}
		catch (Exception ex)
		{
			this._logger.LogError(LoggerEvents.RestError, ex, "Request to {Url} triggered an exception", request.Url.AbsoluteUri);

			// if something went wrong and we couldn't get rate limits for the first request here, allow the next request to run
			if (bucket is not null && ratelimitTcs is not null && bucket.LimitTesting is not 0)
				this.FailInitialRateLimitTest(request, ratelimitTcs);

			if (!request.TrySetFaulted(ex))
				throw;
		}
		finally
		{
			res?.Dispose();

			// Get and decrement active requests in this bucket by 1.
			if (bucket?.BucketId is not null)
			{
				_ = this._requestQueue.TryGetValue(bucket.BucketId, out var count);
				this._requestQueue[bucket.BucketId] = Interlocked.Decrement(ref count);

				// If it's 0 or less, we can remove the bucket from the active request queue,
				// along with any of its past routes.
				if (count <= 0)
					foreach (var r in bucket.RouteHashes)
						if (this._requestQueue.ContainsKey(r))
							_ = this._requestQueue.TryRemove(r, out _);
			}
		}
	}

	/// <summary>
	/// Executes the request.
	/// This is to allow proper rescheduling of the first request from a bucket.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="ratelimitTcs">The ratelimit task completion source.</param>
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	private async Task ExecuteRequestAsync(BaseRestRequest request, RateLimitBucket? bucket, TaskCompletionSource<bool>? ratelimitTcs, bool targetDebug = false)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		if (targetDebug)
			Console.WriteLine("Meow");

		HttpResponseMessage? res = default;

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

			if (this.Debug && req.Content is not null)
				this._logger.Log(LogLevel.Trace, LoggerEvents.RestTx, "Rest Request Content:\n{Content}", await req.Content.ReadAsStringAsync());

			var response = new RestResponse();
			try
			{
				ObjectDisposedException.ThrowIf(this._disposed, this);

				res = await this.HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

				var bts = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				var txt = Utilities.UTF8.GetString(bts, 0, bts.Length);

				if (this.Debug)
					this._logger.Log(LogLevel.Trace, LoggerEvents.RestRx, "Rest Response Content: {Content}", txt);

				response.Headers = res.Headers?.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
				response.Response = txt;
				response.ResponseCode = res.StatusCode;
			}
			catch (HttpRequestException httpex)
			{
				this._logger.LogError(LoggerEvents.RestError, httpex, "Request to {0} triggered an HttpException", request.Url.AbsoluteUri);
				request.SetFaulted(httpex);
				this.FailInitialRateLimitTest(request, ratelimitTcs);
				return;
			}

			this.UpdateBucket(request, response, ratelimitTcs);

			Exception? ex = null;
			Exception? senex = null;
			switch (response.ResponseCode)
			{
				case HttpStatusCode.BadRequest:
				case HttpStatusCode.MethodNotAllowed:
					ex = new BadRequestException(request, response);
					senex = new(ex.Message + "\nJson Response: " + ((ex as BadRequestException)?.JsonMessage ?? "null"), ex);
					break;

				case HttpStatusCode.Unauthorized:
				case HttpStatusCode.Forbidden:
					ex = new UnauthorizedException(request, response);
					break;

				case HttpStatusCode.NotFound:
					ex = new NotFoundException(request, response);
					break;

				case HttpStatusCode.RequestEntityTooLarge:
					ex = new RequestSizeException(request, response);
					break;

				case HttpStatusCode.TooManyRequests:
					ex = new RateLimitException(request, response);

					// check the limit info and requeue
					this.Handle429(response, out var wait, out var global);
					if (wait is not null)
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
							if (this._discord is DiscordClient client)
								await client.RateLimitHitInternal.InvokeAsync(client, new(client.ServiceProvider)
								{
									Exception = (ex as RateLimitException)!,
									ApiEndpoint = request.Url.AbsoluteUri
								});
							this._logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, requeuing request to {url}", request.Url.AbsoluteUri);
							await wait.ConfigureAwait(false);
							this.ExecuteRequestAsync(request, bucket, ratelimitTcs)
								.LogTaskFault(this._logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
						}

						return;
					}

					break;

				case HttpStatusCode.InternalServerError:
				case HttpStatusCode.BadGateway:
				case HttpStatusCode.ServiceUnavailable:
				case HttpStatusCode.GatewayTimeout:
					ex = new ServerErrorException(request, response);
					senex = new(ex.Message + "\nJson Response: " + ((ex as ServerErrorException)!.JsonMessage ?? "null"), ex);
					break;
			}

			if (ex is not null)
			{
				if (this._discord?.Configuration?.EnableSentry ?? false)
					if (senex is not null)
					{
						Dictionary<string, object> debugInfo = new()
						{
							{ "route", request.Route },
							{ "time", DateTimeOffset.UtcNow }
						};
						senex.AddSentryContext("Request", debugInfo);
						this._discord.Sentry.CaptureException(senex);
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
			if (bucket is not null && ratelimitTcs is not null && bucket.LimitTesting is not 0)
				this.FailInitialRateLimitTest(request, ratelimitTcs);

			if (!request.TrySetFaulted(ex))
				throw;
		}
		finally
		{
			res?.Dispose();

			if (bucket?.BucketId is not null)
			{
				// Get and decrement active requests in this bucket by 1.
				_ = this._requestQueue.TryGetValue(bucket.BucketId, out var count);
				this._requestQueue[bucket.BucketId] = Interlocked.Decrement(ref count);

				// If it's 0 or less, we can remove the bucket from the active request queue,
				// along with any of its past routes.
				if (count <= 0)
					foreach (var r in bucket.RouteHashes)
						if (this._requestQueue.ContainsKey(r))
							_ = this._requestQueue.TryRemove(r, out _);
			}
		}
	}

	/// <summary>
	/// Fails the initial rate limit test.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="ratelimitTcs">The ratelimit task completion source.</param>
	/// <param name="resetToInitial">Whether to reset to initial values.</param>
	private void FailInitialRateLimitTest(BaseRestRequest request, TaskCompletionSource<bool>? ratelimitTcs, bool resetToInitial = false)
	{
		if (ratelimitTcs is null && !resetToInitial)
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
		_ = Task.Run(() => ratelimitTcs?.TrySetResult(false));
	}

	/// <summary>
	/// Waits for the initial rate limit.
	/// </summary>
	/// <param name="bucket">The bucket.</param>
	private async Task<TaskCompletionSource<bool>?> WaitForInitialRateLimit(RateLimitBucket bucket)
	{
		while (!bucket.LimitValid)
		{
			if (bucket.LimitTesting is 0)
				if (Interlocked.CompareExchange(ref bucket.LimitTesting, 1, 0) is 0)
				{
					// if we got here when the first request was just finishing, we must not create the waiter task as it would signal ExecuteRequestAsync to bypass rate limiting
					if (bucket.LimitValid)
						return null;

					// allow exactly one request to go through without having rate limits available
					var ratelimitsTcs = new TaskCompletionSource<bool>();
					bucket.LimitTestFinished = ratelimitsTcs.Task;
					return ratelimitsTcs;
				}

			// it can take a couple of cycles for the task to be allocated, so wait until it happens or we are no longer probing for the limits
			Task? waitTask = null;
			while (bucket.LimitTesting is not 0 && (waitTask = bucket.LimitTestFinished) is null)
				await Task.Yield();
			if (waitTask is not null)
				await waitTask.ConfigureAwait(false);

			// if the request failed and the response did not have rate limit headers we have allow the next request and wait again, thus this is a loop here
		}

		return null;
	}

	/// <summary>
	/// Builds the form data request.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <returns>A http request message.</returns>
	private HttpRequestMessage BuildFormRequest(BaseRestRequest request)
	{
		var req = new HttpRequestMessage(new(request.Method.ToString()), request.Url);
		if (request.Headers is not null && request.Headers.Any())
			foreach (var kvp in request.Headers)
				switch (kvp.Key)
				{
					case "Bearer":
						req.Headers.Authorization = new(CommonHeaders.AUTHORIZATION_BEARER, kvp.Value);
						break;
					case "Basic":
						req.Headers.Authorization = new(CommonHeaders.AUTHORIZATION_BASIC, kvp.Value);
						break;
					default:
						req.Headers.Add(kvp.Key, kvp.Value);
						break;
				}

		if (request is not RestFormRequest formRequest)
			throw new InvalidOperationException();

		req.Content = new FormUrlEncodedContent(formRequest.FormData);
		req.Content.Headers.ContentType = new("application/x-www-form-urlencoded");

		return req;
	}

	/// <summary>
	/// Builds the request.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <returns>A http request message.</returns>
	private HttpRequestMessage BuildRequest(BaseRestRequest request)
	{
		var req = new HttpRequestMessage(new(request.Method.ToString()), request.Url);
		if (request.Headers is not null && request.Headers.Any())
			foreach (var kvp in request.Headers)
				switch (kvp.Key)
				{
					case "Bearer":
						req.Headers.Authorization = new(CommonHeaders.AUTHORIZATION_BEARER, kvp.Value);
						break;
					case "Basic":
						req.Headers.Authorization = new(CommonHeaders.AUTHORIZATION_BASIC, kvp.Value);
						break;
					default:
						req.Headers.Add(kvp.Key, kvp.Value);
						break;
				}

		switch (request)
		{
			case RestRequest nmprequest when !string.IsNullOrWhiteSpace(nmprequest.Payload):
				this._logger.LogTrace(LoggerEvents.RestTx, nmprequest.Payload);

				req.Content = new StringContent(nmprequest.Payload);
				req.Content.Headers.ContentType = new("application/json");
				break;
			case MultipartWebRequest mprequest:
			{
				this._logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

				var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

				req.Headers.Add(CommonHeaders.CONNECTION, CommonHeaders.CONNECTION_KEEP_ALIVE);
				req.Headers.Add(CommonHeaders.KEEP_ALIVE, "600");

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

				req.Content = content;
				break;
			}
			case MultipartStickerWebRequest mpsrequest:
			{
				this._logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

				var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

				req.Headers.Add(CommonHeaders.CONNECTION, CommonHeaders.CONNECTION_KEEP_ALIVE);
				req.Headers.Add(CommonHeaders.KEEP_ALIVE, "600");

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

				req.Content = content;
				break;
			}
		}

		return req;
	}

	/// <summary>
	/// Handles the HTTP 429 status.
	/// </summary>
	/// <param name="response">The response.</param>
	/// <param name="waitTask">The wait task.</param>
	/// <param name="global">If true, global.</param>
	private void Handle429(RestResponse response, out Task? waitTask, out bool global)
	{
		waitTask = null;
		global = false;

		if (response.Headers is null)
			return;

		var hs = response.Headers;

		// handle the wait
		if (hs.TryGetValue(CommonHeaders.RETRY_AFTER, out var retryAfterRaw))
		{
			var retryAfter = TimeSpan.FromSeconds(int.Parse(retryAfterRaw, CultureInfo.InvariantCulture));
			waitTask = Task.Delay(retryAfter);
		}

		// check if global b1nzy
		if (hs.TryGetValue(CommonHeaders.RATELIMIT_GLOBAL, out var isGlobal) && isGlobal.ToLowerInvariant() is "true")
			// global
			global = true;
	}

	/// <summary>
	/// Updates the bucket.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="response">The response.</param>
	/// <param name="ratelimitTcs">The ratelimit task completion source.</param>
	private void UpdateBucket(BaseRestRequest request, RestResponse response, TaskCompletionSource<bool>? ratelimitTcs)
	{
		var bucket = request.RateLimitBucket;

		if (response.Headers is null)
		{
			if (response.ResponseCode is not HttpStatusCode.TooManyRequests) // do not fail when ratelimit was or the next request will be scheduled hitting the rate limit again
				this.FailInitialRateLimitTest(request, ratelimitTcs);
			return;
		}

		var hs = response.Headers;

		if (hs.TryGetValue(CommonHeaders.RATELIMIT_SCOPE, out var scope))
			bucket.Scope = scope;

		if (hs.TryGetValue(CommonHeaders.RATELIMIT_GLOBAL, out var isGlobal) && isGlobal.ToLowerInvariant() is "true")
		{
			if (response.ResponseCode is HttpStatusCode.TooManyRequests)
				return;

			bucket.IsGlobal = true;
			this.FailInitialRateLimitTest(request, ratelimitTcs);

			return;
		}

		var r1 = hs.TryGetValue(CommonHeaders.RATELIMIT_LIMIT, out var usesMax);
		var r2 = hs.TryGetValue(CommonHeaders.RATELIMIT_REMAINING, out var usesLeft);
		var r3 = hs.TryGetValue(CommonHeaders.RATELIMIT_RESET, out var reset);
		var r4 = hs.TryGetValue(CommonHeaders.RATELIMIT_RESET_AFTER, out var resetAfter);
		var r5 = hs.TryGetValue(CommonHeaders.RATELIMIT_BUCKET, out var hash);

		if (!r1 || !r2 || !r3 || !r4)
		{
			//If the limits were determined before this request, make the bucket initial again.
			if (response.ResponseCode is not HttpStatusCode.TooManyRequests)
				this.FailInitialRateLimitTest(request, ratelimitTcs, ratelimitTcs is null);

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
			bucket.ResetAfter = TimeSpan.FromSeconds(double.Parse(resetAfter!, CultureInfo.InvariantCulture));
			newReset = clientTime + bucket.ResetAfter.Value + (request.RateLimitWaitOverride.HasValue
				? resetDelta
				: TimeSpan.Zero);
			bucket.ResetAfterOffset = newReset;
		}
		else
			bucket.Reset = newReset;

		var maximum = int.Parse(usesMax!, CultureInfo.InvariantCulture);
		var remaining = int.Parse(usesLeft!, CultureInfo.InvariantCulture);

		if (ratelimitTcs is not null)
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
	private void UpdateHashCaches(BaseRestRequest request, RateLimitBucket bucket, string? newHash = null)
	{
		var hashKey = RateLimitBucket.GenerateHashKey(request.Method, request.Route);

		if (!this._routesToHashes.TryGetValue(hashKey, out var oldHash))
			return;

		// This is an unlimited bucket, which we don't need to keep track of.
		if (newHash is null)
		{
			_ = this._routesToHashes.TryRemove(hashKey, out _);
			if (bucket.BucketId is not null)
				_ = this._hashesToBuckets.TryRemove(bucket.BucketId, out _);
			return;
		}

		// Only update the hash once, due to a bug on Discord's end.
		// This will cause issues if the bucket hashes are dynamically changed from the API while running,
		// in which case, Dispose will need to be called to clear the caches.
		if (!bucket.IsUnlimited || newHash == oldHash)
			return;

		this._logger.LogDebug(LoggerEvents.RestHashMover, "Updating hash in {0}: \"{1}\" -> \"{2}\"", hashKey, oldHash, newHash);
		var bucketId = RateLimitBucket.GenerateBucketId(newHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

		_ = this._routesToHashes.AddOrUpdate(hashKey, newHash, (key, previousHash) =>
		{
			bucket.Hash = newHash;

			var oldBucketId = RateLimitBucket.GenerateBucketId(oldHash!, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

			// Remove the old unlimited bucket.
			_ = this._hashesToBuckets.TryRemove(oldBucketId, out _);
			_ = this._hashesToBuckets.AddOrUpdate(bucketId, bucket, (_, _) => bucket);

			return newHash;
		});
	}

	/// <summary>
	/// Cleans the buckets.
	/// </summary>
	private async Task CleanupBucketsAsync()
	{
		while (!this._bucketCleanerTokenSource?.IsCancellationRequested ?? false)
		{
			try
			{
				await Task.Delay(this._bucketCleanupDelay, this._bucketCleanerTokenSource.Token).ConfigureAwait(false);
			}
			catch
			{ }

			ObjectDisposedException.ThrowIf(this._disposed, this);

			//Check and clean request queue first in case it wasn't removed properly during requests.
			foreach (var key in this._requestQueue.Keys)
			{
				var bucket = this._hashesToBuckets.Values.FirstOrDefault(x => x.RouteHashes.Contains(key));

				if (bucket is null || bucket.LastAttemptAt.AddSeconds(5) < DateTimeOffset.UtcNow)
					_ = this._requestQueue.TryRemove(key, out _);
			}

			var removedBuckets = 0;
			StringBuilder? bucketIdStrBuilder = default;

			foreach (var (key, value) in this._hashesToBuckets)
			{
				bucketIdStrBuilder ??= new();

				// Don't remove the bucket if it's currently being handled by the rest client, unless it's an unlimited bucket.
				if (string.IsNullOrEmpty(value.BucketId) || (this._requestQueue.ContainsKey(value.BucketId) && !value.IsUnlimited))
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

		if (!this._bucketCleanerTokenSource?.IsCancellationRequested ?? true)
			this._bucketCleanerTokenSource?.Cancel();

		this._cleanerRunning = false;
		this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
	}

	/// <summary>
	/// Disposes the rest client.
	/// </summary>
	~RestClient()
	{
		this.Dispose();
	}

	/// <summary>
	/// Disposes the rest client.
	/// </summary>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		this._disposed = true;

		this._globalRateLimitEvent.Reset();

		if (this._bucketCleanerTokenSource?.IsCancellationRequested is false)
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
		catch
		{ }

		this._routesToHashes.Clear();
		this._hashesToBuckets.Clear();
		this._requestQueue.Clear();
		GC.SuppressFinalize(this);
	}
}
