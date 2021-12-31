// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp.Net
{
    /// <summary>
    /// Represents a client used to make REST requests.
    /// </summary>
    internal sealed class RestClient : IDisposable
    {
        /// <summary>
        /// Gets the route argument regex.
        /// </summary>
        private static Regex RouteArgumentRegex { get; } = new Regex(@":([a-z_]+)");

        /// <summary>
        /// Gets the http client.
        /// </summary>
        internal HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the discord client.
        /// </summary>
        private BaseDiscordClient Discord { get; }

        /// <summary>
        /// Gets a value indicating whether debug is enabled.
        /// </summary>
        internal bool Debug { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the routes to hashes.
        /// </summary>
        private ConcurrentDictionary<string, string> RoutesToHashes { get; }

        /// <summary>
        /// Gets the hashes to buckets.
        /// </summary>
        private ConcurrentDictionary<string, RateLimitBucket> HashesToBuckets { get; }

        /// <summary>
        /// Gets the request queue.
        /// </summary>
        private ConcurrentDictionary<string, int> RequestQueue { get; }

        /// <summary>
        /// Gets the global rate limit event.
        /// </summary>
        private AsyncManualResetEvent GlobalRateLimitEvent { get; }

        /// <summary>
        /// Gets a value indicating whether use reset after.
        /// </summary>
        private bool UseResetAfter { get; }

        private CancellationTokenSource _bucketCleanerTokenSource;
        private readonly TimeSpan _bucketCleanupDelay = TimeSpan.FromSeconds(60);
        private volatile bool _cleanerRunning;
        private Task _cleanerTask;
        private volatile bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// </summary>
        /// <param name="Client">The client.</param>
        internal RestClient(BaseDiscordClient Client)
            : this(Client.Configuration.Proxy, Client.Configuration.HttpTimeout, Client.Configuration.UseRelativeRatelimit, Client.Logger)
        {
            this.Discord = Client;
            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(Client));
            if (Client.Configuration.Override != null)
            {
                this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-super-properties", Client.Configuration.Override);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// </summary>
        /// <param name="Proxy">The proxy.</param>
        /// <param name="Timeout">The timeout.</param>
        /// <param name="UseRelativeRatelimit">If true, use relative ratelimit.</param>
        /// <param name="Logger">The logger.</param>
        internal RestClient(IWebProxy Proxy, TimeSpan Timeout, bool UseRelativeRatelimit,
            ILogger Logger) // This is for meta-clients, such as the webhook client
        {
            this.Logger = Logger;

            var httphandler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = Proxy != null,
                Proxy = Proxy
            };

            this.HttpClient = new HttpClient(httphandler)
            {
                BaseAddress = new Uri(Utilities.GetApiBaseUri(this.Discord?.Configuration)),
                Timeout = Timeout
            };

            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            if (this.Discord != null && this.Discord.Configuration != null && this.Discord.Configuration.Override != null)
            {
                this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-super-properties", this.Discord.Configuration.Override);
            }

            this.RoutesToHashes = new ConcurrentDictionary<string, string>();
            this.HashesToBuckets = new ConcurrentDictionary<string, RateLimitBucket>();
            this.RequestQueue = new ConcurrentDictionary<string, int>();

            this.GlobalRateLimitEvent = new AsyncManualResetEvent(true);
            this.UseResetAfter = UseRelativeRatelimit;
        }

        /// <summary>
        /// Gets a bucket.
        /// </summary>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="route_params">The route paramaters.</param>
        /// <param name="Url">The url.</param>
        /// <returns>A ratelimit bucket.</returns>
        public RateLimitBucket GetBucket(RestRequestMethod Method, string Route, object RouteParams, out string Url)
        {
            var rparamsProps = RouteParams.GetType()
                .GetTypeInfo()
                .DeclaredProperties;
            var rparams = new Dictionary<string, string>();
            foreach (var xp in rparamsProps)
            {
                var val = xp.GetValue(RouteParams);
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
            var hashKey = RateLimitBucket.GenerateHashKey(Method, Route);

            // We check if the hash is present, using our generic route (without major params)
            // ex: in POST:/channels/channel_id/messages, out 80c17d2f203122d936070c88c8d10f33
            // If it doesn't exist, we create an unlimited hash as our initial key in the form of the hash key + the unlimited constant
            // and assign this to the route to hash cache
            // ex: this.RoutesToHashes[POST:/channels/channel_id/messages] = POST:/channels/channel_id/messages:unlimited
            var hash = this.RoutesToHashes.GetOrAdd(hashKey, RateLimitBucket.GenerateUnlimitedHash(Method, Route));

            // Next we use the hash to generate the key to obtain the bucket.
            // ex: 80c17d2f203122d936070c88c8d10f33:guild_id:506128773926879242:webhook_id
            // or if unlimited: POST:/channels/channel_id/messages:unlimited:guild_id:506128773926879242:webhook_id
            var bucketId = RateLimitBucket.GenerateBucketId(hash, guildId, channelId, webhookId);

            // If it's not in cache, create a new bucket and index it by its bucket id.
            var bucket = this.HashesToBuckets.GetOrAdd(bucketId, new RateLimitBucket(hash, guildId, channelId, webhookId));

            bucket.LastAttemptAt = DateTimeOffset.UtcNow;

            // Cache the routes for each bucket so it can be used for GC later.
            if (!bucket.RouteHashes.Contains(bucketId))
                bucket.RouteHashes.Add(bucketId);

            // Add the current route to the request queue, which indexes the amount
            // of requests occurring to the bucket id.
            _ = this.RequestQueue.TryGetValue(bucketId, out var count);

            // Increment by one atomically due to concurrency
            this.RequestQueue[bucketId] = Interlocked.Increment(ref count);

            // Start bucket cleaner if not already running.
            if (!this._cleanerRunning)
            {
                this._cleanerRunning = true;
                this._bucketCleanerTokenSource = new CancellationTokenSource();
                this._cleanerTask = Task.Run(this.CleanupBuckets, this._bucketCleanerTokenSource.Token);
                this.Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task started.");
            }

            Url = RouteArgumentRegex.Replace(Route, Xm => rparams[Xm.Groups[1].Value]);
            return bucket;
        }

        /// <summary>
        /// Executes the request async.
        /// </summary>
        /// <param name="Request">The request to be executed.</param>
        public Task ExecuteRequest(BaseRestRequest Request)
            => Request == null ? throw new ArgumentNullException(nameof(Request)) : this.ExecuteRequest(Request, null, null);

        /// <summary>
        /// Executes the request async.
        /// This is to allow proper rescheduling of the first request from a bucket.
        /// </summary>
        /// <param name="Request">The request to be executed.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="RatelimitTcs">The ratelimit task completion source.</param>
        private async Task ExecuteRequest(BaseRestRequest Request, RateLimitBucket Bucket, TaskCompletionSource<bool> RatelimitTcs)
        {
            if (this._disposed)
                return;

            HttpResponseMessage res = default;

            try
            {
                await this.GlobalRateLimitEvent.Wait().ConfigureAwait(false);

                if (Bucket == null)
                    Bucket = Request.RateLimitBucket;

                if (RatelimitTcs == null)
                    RatelimitTcs = await this.WaitForInitialRateLimit(Bucket).ConfigureAwait(false);

                if (RatelimitTcs == null) // ckeck rate limit only if we are not the probe request
                {
                    var now = DateTimeOffset.UtcNow;

                    await Bucket.TryResetLimitAsync(now).ConfigureAwait(false);

                    // Decrement the remaining number of requests as there can be other concurrent requests before this one finishes and has a chance to update the bucket
                    if (Interlocked.Decrement(ref Bucket._remaining) < 0)
                    {
                        this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {0} is blocked", Bucket.ToString());
                        var delay = Bucket.Reset - now;
                        var resetDate = Bucket.Reset;

                        if (this.UseResetAfter)
                        {
                            delay = Bucket.ResetAfter.Value;
                            resetDate = Bucket.ResetAfterOffset;
                        }

                        if (delay < new TimeSpan(-TimeSpan.TicksPerMinute))
                        {
                            this.Logger.LogError(LoggerEvents.RatelimitDiag, "Failed to retrieve ratelimits - giving up and allowing next request for bucket");
                            Bucket._remaining = 1;
                        }

                        if (delay < TimeSpan.Zero)
                            delay = TimeSpan.FromMilliseconds(100);

                        this.Logger.LogWarning(LoggerEvents.RatelimitPreemptive, "Pre-emptive ratelimit triggered - waiting until {0:yyyy-MM-dd HH:mm:ss zzz} ({1:c}).", resetDate, delay);
                        Task.Delay(delay)
                            .ContinueWith(_ => this.ExecuteRequest(Request, null, null))
                            .LogTaskFault(this.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");

                        return;
                    }
                    this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {0} is allowed", Bucket.ToString());
                }
                else
                    this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "Initial request for {0} is allowed", Bucket.ToString());

                var req = this.BuildRequest(Request);

                if (this.Debug)
                    this.Logger.LogTrace(LoggerEvents.Misc, await req.Content.ReadAsStringAsync());

                var response = new RestResponse();
                try
                {
                    if (this._disposed)
                        return;

                    res = await this.HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

                    var bts = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    var txt = Utilities.Utf8.GetString(bts, 0, bts.Length);

                    this.Logger.LogTrace(LoggerEvents.RestRx, txt);

                    response.Headers = res.Headers.ToDictionary(Xh => Xh.Key, Xh => string.Join("\n", Xh.Value), StringComparer.OrdinalIgnoreCase);
                    response.Response = txt;
                    response.ResponseCode = (int)res.StatusCode;
                }
                catch (HttpRequestException httpex)
                {
                    this.Logger.LogError(LoggerEvents.RestError, httpex, "Request to {0} triggered an HttpException", Request.Url);
                    Request.SetFaulted(httpex);
                    this.FailInitialRateLimitTest(Request, RatelimitTcs);
                    return;
                }

                this.UpdateBucket(Request, response, RatelimitTcs);

                Exception ex = null;
                switch (response.ResponseCode)
                {
                    case 400:
                    case 405:
                        ex = new BadRequestException(Request, response);
                        break;

                    case 401:
                    case 403:
                        ex = new UnauthorizedException(Request, response);
                        break;

                    case 404:
                        ex = new NotFoundException(Request, response);
                        break;

                    case 413:
                        ex = new RequestSizeException(Request, response);
                        break;

                    case 429:
                        ex = new RateLimitException(Request, response);

                        // check the limit info and requeue
                        this.Handle429(response, out var wait, out var global);
                        if (wait != null)
                        {
                            if (global)
                            {
                                Bucket.IsGlobal = true;
                                this.Logger.LogError(LoggerEvents.RatelimitHit, "Global ratelimit hit, cooling down");
                                try
                                {
                                    this.GlobalRateLimitEvent.Reset();
                                    await wait.ConfigureAwait(false);
                                }
                                finally
                                {
                                    // we don't want to wait here until all the blocked requests have been run, additionally Set can never throw an exception that could be suppressed here
                                    _ = this.GlobalRateLimitEvent.Set();
                                }
                                this.ExecuteRequest(Request, Bucket, RatelimitTcs)
                                    .LogTaskFault(this.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
                            }
                            else
                            {
                                this.Logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, requeueing request to {0}", Request.Url);
                                await wait.ConfigureAwait(false);
                                this.ExecuteRequest(Request, Bucket, RatelimitTcs)
                                    .LogTaskFault(this.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
                            }

                            return;
                        }
                        break;

                    case 500:
                    case 502:
                    case 503:
                    case 504:
                        ex = new ServerErrorException(Request, response);
                        break;
                }

                if (ex != null)
                    Request.SetFaulted(ex);
                else
                    Request.SetCompleted(response);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(LoggerEvents.RestError, ex, "Request to {0} triggered an exception", Request.Url);

                // if something went wrong and we couldn't get rate limits for the first request here, allow the next request to run
                if (Bucket != null && RatelimitTcs != null && Bucket._limitTesting != 0)
                    this.FailInitialRateLimitTest(Request, RatelimitTcs);

                if (!Request.TrySetFaulted(ex))
                    throw;
            }
            finally
            {
                res?.Dispose();

                // Get and decrement active requests in this bucket by 1.
                _ = this.RequestQueue.TryGetValue(Bucket.BucketId, out var count);
                this.RequestQueue[Bucket.BucketId] = Interlocked.Decrement(ref count);

                // If it's 0 or less, we can remove the bucket from the active request queue,
                // along with any of its past routes.
                if (count <= 0)
                {
                    foreach (var r in Bucket.RouteHashes)
                    {
                        if (this.RequestQueue.ContainsKey(r))
                        {
                            _ = this.RequestQueue.TryRemove(r, out _);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fails the initial rate limit test.
        /// </summary>
        /// <param name="Request">The request.</param>
        /// <param name="RatelimitTcs">The ratelimit task completion source.</param>
        /// <param name="ResetToInitial">If true, reset to initial.</param>
        private void FailInitialRateLimitTest(BaseRestRequest Request, TaskCompletionSource<bool> RatelimitTcs, bool ResetToInitial = false)
        {
            if (RatelimitTcs == null && !ResetToInitial)
                return;

            var bucket = Request.RateLimitBucket;

            bucket._limitValid = false;
            bucket._limitTestFinished = null;
            bucket._limitTesting = 0;

            //Reset to initial values.
            if (ResetToInitial)
            {
                this.UpdateHashCaches(Request, bucket);
                bucket.Maximum = 0;
                bucket._remaining = 0;
                return;
            }

            // no need to wait on all the potentially waiting tasks
            _ = Task.Run(() => RatelimitTcs.TrySetResult(false));
        }

        /// <summary>
        /// Waits for the initial rate limit.
        /// </summary>
        /// <param name="Bucket">The bucket.</param>
        private async Task<TaskCompletionSource<bool>> WaitForInitialRateLimit(RateLimitBucket Bucket)
        {
            while (!Bucket._limitValid)
            {
                if (Bucket._limitTesting == 0)
                {
                    if (Interlocked.CompareExchange(ref Bucket._limitTesting, 1, 0) == 0)
                    {
                        // if we got here when the first request was just finishing, we must not create the waiter task as it would signel ExecureRequestAsync to bypass rate limiting
                        if (Bucket._limitValid)
                            return null;

                        // allow exactly one request to go through without having rate limits available
                        var ratelimitsTcs = new TaskCompletionSource<bool>();
                        Bucket._limitTestFinished = ratelimitsTcs.Task;
                        return ratelimitsTcs;
                    }
                }
                // it can take a couple of cycles for the task to be allocated, so wait until it happens or we are no longer probing for the limits
                Task waitTask = null;
                while (Bucket._limitTesting != 0 && (waitTask = Bucket._limitTestFinished) == null)
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
        /// <param name="Request">The request.</param>
        /// <returns>A http request message.</returns>
        private HttpRequestMessage BuildRequest(BaseRestRequest Request)
        {
            var req = new HttpRequestMessage(new HttpMethod(Request.Method.ToString()), Request.Url);
            if (Request.Headers != null && Request.Headers.Any())
                foreach (var kvp in Request.Headers)
                    req.Headers.Add(kvp.Key, kvp.Value);

            if (Request is RestRequest nmprequest && !string.IsNullOrWhiteSpace(nmprequest.Payload))
            {
                this.Logger.LogTrace(LoggerEvents.RestTx, nmprequest.Payload);

                req.Content = new StringContent(nmprequest.Payload);
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            if (Request is MultipartWebRequest mprequest)
            {
                this.Logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

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

            if (Request is MultipartStickerWebRequest mpsrequest)
            {
                this.Logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");

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
        /// Handles the http 429 status.
        /// </summary>
        /// <param name="Response">The response.</param>
        /// <param name="wait_task">The wait task.</param>
        /// <param name="Global">If true, global.</param>
        private void Handle429(RestResponse Response, out Task WaitTask, out bool Global)
        {
            WaitTask = null;
            Global = false;

            if (Response.Headers == null)
                return;
            var hs = Response.Headers;

            // handle the wait
            if (hs.TryGetValue("Retry-After", out var retryAfterRaw))
            {
                var retryAfter = TimeSpan.FromSeconds(int.Parse(retryAfterRaw, CultureInfo.InvariantCulture));
                WaitTask = Task.Delay(retryAfter);
            }

            // check if global b1nzy
            if (hs.TryGetValue("X-RateLimit-Global", out var isglobal) && isglobal.ToLowerInvariant() == "true")
            {
                // global
                Global = true;
            }
        }

        /// <summary>
        /// Updates the bucket.
        /// </summary>
        /// <param name="Request">The request.</param>
        /// <param name="Response">The response.</param>
        /// <param name="RatelimitTcs">The ratelimit task completion source.</param>
        private void UpdateBucket(BaseRestRequest Request, RestResponse Response, TaskCompletionSource<bool> RatelimitTcs)
        {
            var bucket = Request.RateLimitBucket;

            if (Response.Headers == null)
            {
                if (Response.ResponseCode != 429) // do not fail when ratelimit was or the next request will be scheduled hitting the rate limit again
                    this.FailInitialRateLimitTest(Request, RatelimitTcs);
                return;
            }

            var hs = Response.Headers;

            if (hs.TryGetValue("X-RateLimit-Scope", out var scope))
            {
                bucket.Scope = scope;
            }


            if (hs.TryGetValue("X-RateLimit-Global", out var isglobal) && isglobal.ToLowerInvariant() == "true")
            {
                if (Response.ResponseCode != 429)
                {
                    bucket.IsGlobal = true;
                    this.FailInitialRateLimitTest(Request, RatelimitTcs);
                }

                return;
            }

            var r1 = hs.TryGetValue("X-RateLimit-Limit", out var usesmax);
            var r2 = hs.TryGetValue("X-RateLimit-Remaining", out var usesleft);
            var r3 = hs.TryGetValue("X-RateLimit-Reset", out var reset);
            var r4 = hs.TryGetValue("X-Ratelimit-Reset-After", out var resetAfter);
            var r5 = hs.TryGetValue("X-Ratelimit-Bucket", out var hash);

            if (!r1 || !r2 || !r3 || !r4)
            {
                //If the limits were determined before this request, make the bucket initial again.
                if (Response.ResponseCode != 429)
                    this.FailInitialRateLimitTest(Request, RatelimitTcs, RatelimitTcs == null);

                return;
            }

            var clienttime = DateTimeOffset.UtcNow;
            var resettime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(double.Parse(reset, CultureInfo.InvariantCulture));
            var servertime = clienttime;
            if (hs.TryGetValue("Date", out var rawDate))
                servertime = DateTimeOffset.Parse(rawDate, CultureInfo.InvariantCulture).ToUniversalTime();

            var resetdelta = resettime - servertime;
            //var difference = clienttime - servertime;
            //if (Math.Abs(difference.TotalSeconds) >= 1)
            ////    this.Logger.LogMessage(LogLevel.DebugBaseDiscordClient.RestEventId,  $"Difference between machine and server time: {difference.TotalMilliseconds.ToString("#,##0.00", CultureInfo.InvariantCulture)}ms", DateTime.Now);
            //else
            //    difference = TimeSpan.Zero;

            if (Request.RateLimitWaitOverride.HasValue)
                resetdelta = TimeSpan.FromSeconds(Request.RateLimitWaitOverride.Value);
            var newReset = clienttime + resetdelta;

            if (this.UseResetAfter)
            {
                bucket.ResetAfter = TimeSpan.FromSeconds(double.Parse(resetAfter, CultureInfo.InvariantCulture));
                newReset = clienttime + bucket.ResetAfter.Value + (Request.RateLimitWaitOverride.HasValue
                    ? resetdelta
                    : TimeSpan.Zero);
                bucket.ResetAfterOffset = newReset;
            }
            else
                bucket.Reset = newReset;

            var maximum = int.Parse(usesmax, CultureInfo.InvariantCulture);
            var remaining = int.Parse(usesleft, CultureInfo.InvariantCulture);

            if (RatelimitTcs != null)
            {
                // initial population of the ratelimit data
                bucket.SetInitialValues(maximum, remaining, newReset);

                _ = Task.Run(() => RatelimitTcs.TrySetResult(true));
            }
            else
            {
                // only update the bucket values if this request was for a newer interval than the one
                // currently in the bucket, to avoid issues with concurrent requests in one bucket
                // remaining is reset by TryResetLimit and not the response, just allow that to happen when it is time
                if (bucket._nextReset == 0)
                    bucket._nextReset = newReset.UtcTicks;
            }

            this.UpdateHashCaches(Request, bucket, hash);
        }

        /// <summary>
        /// Updates the hash caches.
        /// </summary>
        /// <param name="Request">The request.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="NewHash">The new hash.</param>
        private void UpdateHashCaches(BaseRestRequest Request, RateLimitBucket Bucket, string NewHash = null)
        {
            var hashKey = RateLimitBucket.GenerateHashKey(Request.Method, Request.Route);

            if (!this.RoutesToHashes.TryGetValue(hashKey, out var oldHash))
                return;

            // This is an unlimited bucket, which we don't need to keep track of.
            if (NewHash == null)
            {
                _ = this.RoutesToHashes.TryRemove(hashKey, out _);
                _ = this.HashesToBuckets.TryRemove(Bucket.BucketId, out _);
                return;
            }

            // Only update the hash once, due to a bug on Discord's end.
            // This will cause issues if the bucket hashes are dynamically changed from the API while running,
            // in which case, Dispose will need to be called to clear the caches.
            if (Bucket._isUnlimited && NewHash != oldHash)
            {
                this.Logger.LogDebug(LoggerEvents.RestHashMover, "Updating hash in {0}: \"{1}\" -> \"{2}\"", hashKey, oldHash, NewHash);
                var bucketId = RateLimitBucket.GenerateBucketId(NewHash, Bucket.GuildId, Bucket.ChannelId, Bucket.WebhookId);

                _ = this.RoutesToHashes.AddOrUpdate(hashKey, NewHash, (Key, OldHash) =>
                {
                    Bucket.Hash = NewHash;

                    var oldBucketId = RateLimitBucket.GenerateBucketId(OldHash, Bucket.GuildId, Bucket.ChannelId, Bucket.WebhookId);

                    // Remove the old unlimited bucket.
                    _ = this.HashesToBuckets.TryRemove(oldBucketId, out _);
                    _ = this.HashesToBuckets.AddOrUpdate(bucketId, Bucket, (Key, OldBucket) => Bucket);

                    return NewHash;
                });
            }

            return;
        }

        /// <summary>
        /// Cleanups the buckets.
        /// </summary>
        private async Task CleanupBuckets()
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
                foreach (var key in this.RequestQueue.Keys)
                {
                    var bucket = this.HashesToBuckets.Values.FirstOrDefault(X => X.RouteHashes.Contains(key));

                    if (bucket == null || (bucket != null && bucket.LastAttemptAt.AddSeconds(5) < DateTimeOffset.UtcNow))
                        _ = this.RequestQueue.TryRemove(key, out _);
                }

                var removedBuckets = 0;
                StringBuilder bucketIdStrBuilder = default;

                foreach (var kvp in this.HashesToBuckets)
                {
                    if (bucketIdStrBuilder == null)
                        bucketIdStrBuilder = new StringBuilder();

                    var key = kvp.Key;
                    var value = kvp.Value;

                    // Don't remove the bucket if it's currently being handled by the rest client, unless it's an unlimited bucket.
                    if (this.RequestQueue.ContainsKey(value.BucketId) && !value._isUnlimited)
                        continue;

                    var resetOffset = this.UseResetAfter ? value.ResetAfterOffset : value.Reset;

                    // Don't remove the bucket if it's reset date is less than now + the additional wait time, unless it's an unlimited bucket.
                    if (resetOffset != null && !value._isUnlimited && (resetOffset > DateTimeOffset.UtcNow || DateTimeOffset.UtcNow - resetOffset < this._bucketCleanupDelay))
                        continue;

                    _ = this.HashesToBuckets.TryRemove(key, out _);
                    removedBuckets++;
                    bucketIdStrBuilder.Append(value.BucketId + ", ");
                }

                if (removedBuckets > 0)
                    this.Logger.LogDebug(LoggerEvents.RestCleaner, "Removed {0} unused bucket{1}: [{2}]", removedBuckets, removedBuckets > 1 ? "s" : string.Empty, bucketIdStrBuilder.ToString().TrimEnd(',', ' '));

                if (this.HashesToBuckets.Count == 0)
                    break;
            }

            if (!this._bucketCleanerTokenSource.IsCancellationRequested)
                this._bucketCleanerTokenSource.Cancel();

            this._cleanerRunning = false;
            this.Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
        }

        ~RestClient()
            => this.Dispose();

        /// <summary>
        /// Disposes the rest client.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;

            this.GlobalRateLimitEvent.Reset();

            if (this._bucketCleanerTokenSource?.IsCancellationRequested == false)
            {
                this._bucketCleanerTokenSource?.Cancel();
                this.Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
            }

            try
            {
                this._cleanerTask?.Dispose();
                this._bucketCleanerTokenSource?.Dispose();
                this.HttpClient?.Dispose();
            }
            catch { }

            this.RoutesToHashes.Clear();
            this.HashesToBuckets.Clear();
            this.RequestQueue.Clear();
        }
    }
}
