using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Exceptions;
using DisCatSharp.Telemetry;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Net;

/// <summary>
///     Represents a client used to make REST requests.
///     Requests are processed through per-bucket <see cref="BucketWorker" /> instances that enforce
///     FIFO ordering and independent rate-limit management per bucket.
/// </summary>
internal sealed class RestClient : IDisposable, IRestDiagnostics
{
	/// <summary>
	///     Gets the discord client.
	/// </summary>
	private readonly BaseDiscordClient? _discord;

	/// <summary>
	///     Gets the global rate limit event.
	/// </summary>
	private readonly AsyncManualResetEvent _globalRateLimitEvent;

	/// <summary>
	///     Gets the logger.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	///     Gets a value indicating whether use reset after.
	/// </summary>
	private readonly bool _useResetAfter;

	/// <summary>
	///     Gets the advanced REST configuration.
	/// </summary>
	private readonly RestAdvancedConfiguration _advancedConfig;

	/// <summary>
	///     Gets whether the client is disposed.
	/// </summary>
	private volatile bool _disposed;

	/// <summary>
	///     Initializes a new instance of the <see cref="RestClient" /> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal RestClient(BaseDiscordClient client)
		: this(client.Configuration, client.Logger)
	{
		this._discord = client;

		this.Debug = this._discord.Configuration.Logging.MinimumLogLevel is LogLevel.Trace;

		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.AUTHORIZATION, Utilities.GetFormattedToken(client));
		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_LOCALE, client.Configuration.Api.Locale);
		if (!string.IsNullOrWhiteSpace(client.Configuration.Api.Timezone))
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_TIMEZONE, client.Configuration.Api.Timezone);
		if (!string.IsNullOrWhiteSpace(client.Configuration.Api.Override))
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.SUPER_PROPERTIES, client.Configuration.Api.Override);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="RestClient" /> class.
	///     This is for meta-clients, such as the <see cref="DiscordWebhookClient" /> and <see cref="DiscordOAuth2Client" />.
	/// </summary>
	/// <param name="configuration">The configuration.</param>
	/// <param name="logger">The logger.</param>
	internal RestClient(
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

		this.HttpClient = new(httphandler)
		{
			BaseAddress = new(Utilities.GetApiBaseUri(configuration)),
			Timeout = configuration.Rest.RequestTimeout
		};

		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.USER_AGENT, Utilities.GetUserAgent());
		if (this._discord is { Configuration: not null })
		{
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_LOCALE, configuration.Api.Locale);
			if (!string.IsNullOrWhiteSpace(configuration.Api.Timezone))
				this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.DISCORD_TIMEZONE, configuration.Api.Timezone);
			if (!string.IsNullOrWhiteSpace(configuration.Api.Override))
				this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.SUPER_PROPERTIES, configuration.Api.Override);
		}

		this._globalRateLimitEvent = new(true);
		this._useResetAfter = configuration.Rest.UseRelativeRatelimit;
		this._advancedConfig = new(configuration.Rest.Advanced);

		this.Registry = new(this, this._advancedConfig, this._logger, this._useResetAfter, TimeSpan.FromSeconds(60));
	}

	/// <summary>
	///     Gets the http client.
	/// </summary>
	internal HttpClient HttpClient { get; }

	/// <summary>
	///     Gets the bucket registry that manages route→hash→bucket→worker mappings.
	/// </summary>
	internal BucketRegistry Registry { get; }

	/// <summary>
	///     Gets a value indicating whether debug is enabled.
	/// </summary>
	internal bool Debug { get; set; } = false;

	/// <summary>
	///     Disposes the rest client.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		lock (this)
		{
			if (this._disposed)
				return;

			this._disposed = true;
		}

		// Dispose the registry — disposes all workers, stops the cleaner, clears caches
		this.Registry.Dispose();

		// Now reset the global gate — no workers are waiting on it anymore
		this._globalRateLimitEvent.Reset();

		try
		{
			this.HttpClient?.Dispose();
		}
		catch
		{ }

		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Gets a ratelimit bucket.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="routeParams">The route parameters.</param>
	/// <param name="url">The url.</param>
	/// <returns>A ratelimit bucket.</returns>
	public RateLimitBucket GetBucket(RestRequestMethod method, string route, object routeParams, out string url)
		=> this.Registry.GetBucket(method, route, routeParams, out url);

	/// <summary>
	///     Executes the request by enqueuing it into the appropriate bucket worker.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	public async Task ExecuteRequestAsync(BaseRestRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		if (this._disposed)
		{
			request.SetFaulted(new ObjectDisposedException(nameof(RestClient), "Cannot execute request on a disposed RestClient."));
			return;
		}

		var worker = this.Registry.GetOrCreateWorker(request.RateLimitBucket);
		worker.Enqueue(request);
		await request.WaitForCompletionAsync().ConfigureAwait(false);
	}

	/// <summary>
	///     Executes the form data request by enqueuing it into the appropriate bucket worker.
	///     Form and regular requests share the same queue/worker infrastructure.
	/// </summary>
	/// <param name="request">The request to be executed.</param>
	public Task ExecuteFormRequestAsync(BaseRestRequest request)
		=> this.ExecuteRequestAsync(request);

	/// <summary>
	///     Cancels all pending requests across all bucket workers by draining their queues
	///     and faulting each request with <see cref="OperationCanceledException" />.
	/// </summary>
	/// <param name="reason">Optional reason message for the cancellation.</param>
	public void CancelAllPendingRequests(string? reason = null)
	{
		var cancelled = this.Registry.CancelAllPendingRequests(reason ?? "All pending REST requests were cancelled by the application.");
		this._logger.LogInformation(LoggerEvents.RestCleaner, "Cancelled {Count} pending REST requests", cancelled);
	}

	// ── Internal methods called by BucketWorker ──────────────────────────────

	/// <summary>
	///     Waits until the global rate limit gate is open.
	/// </summary>
	/// <param name="ct">Optional cancellation token to abort the wait.</param>
	internal Task WaitForGlobalGateAsync(CancellationToken ct = default)
		=> this._globalRateLimitEvent.WaitAsync(ct);

	/// <summary>
	///     Gets whether the global rate limit gate is currently blocking requests.
	/// </summary>
	internal bool IsGlobalGateBlocked => !this._globalRateLimitEvent.IsSet;

	/// <summary>
	///     Computes the delay for a preemptive bucket rate limit.
	/// </summary>
	internal TimeSpan ComputeBucketDelay(RateLimitBucket bucket)
	{
		if (this._useResetAfter)
			return bucket.ResetAfter ?? TimeSpan.Zero;

		return bucket.Reset - DateTimeOffset.UtcNow;
	}

	/// <summary>
	///     Blocks the global gate, waits the specified delay, then reopens it.
	/// </summary>
	/// <param name="delay">The delay to enforce.</param>
	/// <param name="ct">Cancellation token to abort the delay (e.g., on dispose).</param>
	internal async Task EnforceGlobalRateLimitAsync(TimeSpan delay, CancellationToken ct = default)
	{
		try
		{
			this._globalRateLimitEvent.Reset();

			if (delay > TimeSpan.Zero)
				await Task.Delay(delay, ct).ConfigureAwait(false);
		}
		finally
		{
			_ = this._globalRateLimitEvent.SetAsync();
		}
	}

	/// <summary>
	///     Fires the internal RateLimitHit event, if the client supports it.
	/// </summary>
	internal async Task RaiseRateLimitHitAsync(BaseRestRequest request, RateLimitException? exception)
	{
		if (exception is null)
			return;

		if (this._discord is DiscordClient client)
			await client.RateLimitHitInternal.InvokeAsync(client, new(client.ServiceProvider)
			{
				Exception = exception,
				ApiEndpoint = request.Url.AbsoluteUri
			});
	}

	/// <summary>
	///     Builds an HTTP request, sends it, parses the response, and updates the bucket from headers.
	///     Returns a <see cref="SendResult" /> indicating success, retry, or error.
	/// </summary>
	internal async Task<SendResult> SendAndParseAsync(BaseRestRequest request, RateLimitBucket bucket, bool isProbe)
	{
		HttpResponseMessage? httpRes = null;

		try
		{
			var httpReq = request is RestFormRequest
				? this.BuildFormRequest(request)
				: this.BuildRequest(request);

			if (this._discord?.DiagnosticsSink.IsEnabled ?? false)
			{
				var isForm = request is RestFormRequest;
				this._discord.DiagnosticsSink.AddBreadcrumb("DisCatSharp", "rest",
					isForm ? $"{httpReq.Method} {request.Route} (form)" : $"{httpReq.Method} {request.Route}",
					DiagnosticSeverity.Debug, new Dictionary<string, string>
					{
						["method"] = httpReq.Method.Method,
						["route"] = request.Route,
						["type"] = isForm ? "form" : "json"
					});
			}

			if (this.Debug && httpReq.Content is not null)
				this._logger.Log(LogLevel.Trace, LoggerEvents.RestTx, "Rest Request Content:\n{Content}", await httpReq.Content.ReadAsStringAsync());

			ObjectDisposedException.ThrowIf(this._disposed, this);

			httpRes = await this.HttpClient.SendAsync(httpReq, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

			var bts = await httpRes.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			var txt = Utilities.UTF8.GetString(bts, 0, bts.Length);

			if (this.Debug)
				this._logger.Log(LogLevel.Trace, LoggerEvents.RestRx, "Rest Response Content: {Content}", txt);

			var response = new RestResponse
			{
				Headers = httpRes.Headers?.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase),
				Response = txt,
				ResponseCode = httpRes.StatusCode
			};

			if (this._discord?.DiagnosticsSink.IsEnabled ?? false)
				this._discord.DiagnosticsSink.AddBreadcrumb("DisCatSharp", "rest",
					$"Response {(int)httpRes.StatusCode} {request.Route}",
					DiagnosticSeverity.Debug, new Dictionary<string, string>
					{
						["status_code"] = ((int)httpRes.StatusCode).ToString(),
						["route"] = request.Route
					});

			// Update bucket state from response headers
			this.UpdateBucket(request, response, isProbe);

			// Evaluate the response and determine next action
			return this.EvaluateResponse(request, response);
		}
		catch (HttpRequestException httpEx)
		{
			this._logger.LogError(LoggerEvents.RestError, httpEx, "Request to {Url} triggered an HttpException", request.Url.AbsoluteUri);

			if (isProbe)
				this.ResetProbeState(request.RateLimitBucket);

			// Classify transient vs permanent network errors
			var isTransient = IsTransientHttpError(httpEx);

			return new()
			{
				Error = httpEx,
				ShouldRetry = isTransient,
				IsTransientNetworkError = isTransient
			};
		}
		finally
		{
			httpRes?.Dispose();
		}
	}

	/// <summary>
	///     Maps an HTTP response status to a <see cref="SendResult" />.
	/// </summary>
	private SendResult EvaluateResponse(BaseRestRequest request, RestResponse response)
	{
		switch (response.ResponseCode)
		{
			case HttpStatusCode.TooManyRequests:
			{
				var retryDelay = TimeSpan.Zero;
				var isGlobal = false;

				if (response.Headers is not null)
				{
					if (response.Headers.TryGetValue(CommonHeaders.RETRY_AFTER, out var retryAfterRaw))
					{
						var retryAfterSeconds = Math.Min(int.Parse(retryAfterRaw, CultureInfo.InvariantCulture), 3600);
						retryDelay = TimeSpan.FromSeconds(retryAfterSeconds);
					}

					if (response.Headers.TryGetValue(CommonHeaders.RATELIMIT_GLOBAL, out var isGlobalRaw)
						&& isGlobalRaw.Equals("true", StringComparison.OrdinalIgnoreCase))
					{
						isGlobal = true;
						request.RateLimitBucket.IsGlobal = true;
					}
				}

				return new()
				{
					Response = response,
					ShouldRetry = true,
					RetryDelay = retryDelay,
					IsGlobalRateLimit = isGlobal,
					Error = new RateLimitException(request, response)
				};
			}

			case HttpStatusCode.BadRequest:
			case HttpStatusCode.MethodNotAllowed:
				return new() { Response = response, Error = new BadRequestException(request, response) };

			case HttpStatusCode.Unauthorized:
			case HttpStatusCode.Forbidden:
				return new() { Response = response, Error = new UnauthorizedException(request, response) };

			case HttpStatusCode.NotFound:
				return new() { Response = response, Error = new NotFoundException(request, response) };

			case HttpStatusCode.RequestEntityTooLarge:
				return new() { Response = response, Error = new RequestSizeException(request, response) };

			case HttpStatusCode.InternalServerError:
			case HttpStatusCode.BadGateway:
			case HttpStatusCode.ServiceUnavailable:
			case HttpStatusCode.GatewayTimeout:
				return new() { Response = response, ShouldRetry = true, IsServerError = true, RetryDelay = TimeSpan.FromSeconds(1), Error = new ServerErrorException(request, response) };

			default:
				return new() { Response = response };
		}
	}

	/// <summary>
	///     Reports diagnostics/Sentry for error responses.
	/// </summary>
	internal void ReportDiagnostics(BaseRestRequest request, RestResponse response, Exception error)
	{
		if (!(this._discord?.DiagnosticsSink.IsEnabled ?? false))
			return;

		SentryCapturableException? senex = error switch
		{
			BadRequestException brex => new(error.Message + "\nJson Response: " + (brex.JsonMessage ?? "null"), error),
			ServerErrorException seex => new(error.Message + "\nJson Response: " + (seex.JsonMessage ?? "null"), error),
			_ => null
		};

		if (senex is null)
			return;

		Dictionary<string, object> debugInfo = new()
		{
			{ "route", request.Route },
			{ "time", DateTimeOffset.UtcNow }
		};

		Dictionary<string, string> tags = new()
		{
			["dcs.rest_route"] = request.Route,
			["dcs.rest_status"] = ((int)response.ResponseCode).ToString()
		};

		this._discord.DiagnosticsSink.CaptureException("DisCatSharp", senex, debugInfo, tags);
	}

	/// <summary>
	///     Resets a bucket's probe state so the next request becomes the new probe.
	/// </summary>
	private void ResetProbeState(RateLimitBucket bucket)
	{
		bucket.LimitValid = false;
		bucket.Maximum = 0;
		bucket.RemainingInternal = 0;
	}

	/// <summary>
	///     Classifies whether an <see cref="HttpRequestException" /> is transient (worth retrying)
	///     or permanent (should fail immediately).
	/// </summary>
	/// <param name="ex">The HTTP exception to classify.</param>
	/// <returns><c>true</c> if the error is transient (DNS, socket, timeout); <c>false</c> otherwise.</returns>
	private static bool IsTransientHttpError(HttpRequestException ex)
		=> ex.InnerException is System.Net.Sockets.SocketException
			or System.IO.IOException
			or TimeoutException
			or OperationCanceledException;

	/// <summary>
	///     Builds the form data request.
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
	///     Builds the request.
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

				var boundary = "---------------------------" + Guid.NewGuid().ToString("N");

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
						var name = mprequest.FileFieldNameFactory?.Invoke(fileId) ?? $"files[{fileId.ToString(CultureInfo.InvariantCulture)}]";
						var streamContent = new StreamContent(f.Stream);
						if (f.ContentType is not null)
							streamContent.Headers.ContentType = new(f.ContentType);

						content.Add(streamContent, name, f.Filename);
						fileId++;
					}

				req.Content = content;
				break;
			}
			case MultipartStickerWebRequest mpsrequest:
			{
				this._logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

				var boundary = "---------------------------" + Guid.NewGuid().ToString("N");

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
	///     Updates the bucket.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="response">The response.</param>
	/// <param name="isProbe">Whether this was a probe request (first request for unprobed bucket).</param>
	private void UpdateBucket(BaseRestRequest request, RestResponse response, bool isProbe)
	{
		var bucket = request.RateLimitBucket;

		if (response.Headers is null)
		{
			if (response.ResponseCode is not HttpStatusCode.TooManyRequests)
			{
				if (isProbe)
					this.ResetProbeState(bucket);
				else if (bucket.LimitValid)
				{
					// Previously valid bucket lost its headers — reset to probe state
					this.Registry.UpdateHashCaches(request, bucket);
					this.ResetProbeState(bucket);
				}
			}

			return;
		}

		var hs = response.Headers;

		if (hs.TryGetValue(CommonHeaders.RATELIMIT_SCOPE, out var scope))
			bucket.Scope = scope;

		if (hs.TryGetValue(CommonHeaders.RATELIMIT_GLOBAL, out var isGlobal) && isGlobal.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			if (response.ResponseCode is HttpStatusCode.TooManyRequests)
				return;

			bucket.IsGlobal = true;

			if (isProbe)
				this.ResetProbeState(bucket);

			return;
		}

		var r1 = hs.TryGetValue(CommonHeaders.RATELIMIT_LIMIT, out var usesMax);
		var r2 = hs.TryGetValue(CommonHeaders.RATELIMIT_REMAINING, out var usesLeft);
		var r3 = hs.TryGetValue(CommonHeaders.RATELIMIT_RESET, out var reset);
		var r4 = hs.TryGetValue(CommonHeaders.RATELIMIT_RESET_AFTER, out var resetAfter);
		var r5 = hs.TryGetValue(CommonHeaders.RATELIMIT_BUCKET, out var hash);

		if (!r1 || !r2 || !r3 || !r4)
		{
			if (response.ResponseCode is not HttpStatusCode.TooManyRequests)
			{
				if (isProbe)
					this.ResetProbeState(bucket);
				else if (bucket.LimitValid)
				{
					this.Registry.UpdateHashCaches(request, bucket);
					this.ResetProbeState(bucket);
				}
			}

			return;
		}

		var clientTime = DateTimeOffset.UtcNow;
		var resetTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(double.Parse(reset, CultureInfo.InvariantCulture));
		var serverTime = clientTime;
		if (hs.TryGetValue("Date", out var rawDate))
			serverTime = DateTimeOffset.Parse(rawDate, CultureInfo.InvariantCulture).ToUniversalTime();

		var resetDelta = resetTime - serverTime;

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

		if (isProbe)
		{
			// Initial population of the ratelimit data
			bucket.SetInitialValues(maximum, remaining, newReset);
		}
		else
		{
			// Only update the bucket values if this request was for a newer interval than the one
			// currently in the bucket, to avoid issues with concurrent requests in one bucket.
			// Remaining is reset by TryResetLimit and not the response.
			if (bucket.NextReset == 0)
				bucket.NextReset = newReset.UtcTicks;
		}

		this.Registry.UpdateHashCaches(request, bucket, hash);
	}

	/// <summary>
	///     Disposes the rest client.
	/// </summary>
	~RestClient()
	{
		this.Dispose();
	}

	// ── IRestDiagnostics ────────────────────────────────────────────────────

	/// <inheritdoc />
	public int ActiveWorkerCount
		=> this.Registry.GetActiveWorkerCount();

	/// <inheritdoc />
	public int TotalQueuedRequests
		=> this.Registry.GetTotalQueuedRequests();

	/// <inheritdoc />
	public IReadOnlyList<BucketDiagnostics> GetBucketSnapshots()
		=> this.Registry.GetBucketSnapshots();
}
