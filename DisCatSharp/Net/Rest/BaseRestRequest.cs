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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisCatSharp.Net
{
    /// <summary>
    /// Represents a request sent over HTTP.
    /// </summary>
    public abstract class BaseRestRequest
    {
        /// <summary>
        /// Gets the discord client.
        /// </summary>
        protected internal BaseDiscordClient Discord { get; }

        /// <summary>
        /// Gets the request task source.
        /// </summary>
        protected internal TaskCompletionSource<RestResponse> RequestTaskSource { get; }

        /// <summary>
        /// Gets the url to which this request is going to be made.
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Gets the HTTP method used for this request.
        /// </summary>
        public RestRequestMethod Method { get; }

        /// <summary>
        /// Gets the generic path (no parameters) for this request.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Gets the headers sent with this request.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; } = null;

        /// <summary>
        /// Gets the override for the rate limit bucket wait time.
        /// </summary>
        public double? RateLimitWaitOverride { get; }

        /// <summary>
        /// Gets the rate limit bucket this request is in.
        /// </summary>
        internal RateLimitBucket RateLimitBucket { get; }

        /// <summary>
        /// Creates a new <see cref="BaseRestRequest"/> with specified parameters.
        /// </summary>
        /// <param name="Client"><see cref="DiscordClient"/> from which this request originated.</param>
        /// <param name="Bucket">Rate limit bucket to place this request in.</param>
        /// <param name="Url">Uri to which this request is going to be sent to.</param>
        /// <param name="Method">Method to use for this request,</param>
        /// <param name="Route">The generic route the request url will use.</param>
        /// <param name="Headers">Additional headers for this request.</param>
        /// <param name="RatelimitWaitOverride">Override for ratelimit bucket wait time.</param>
        internal BaseRestRequest(BaseDiscordClient Client, RateLimitBucket Bucket, Uri Url, RestRequestMethod Method, string Route, IReadOnlyDictionary<string, string> Headers = null, double? RatelimitWaitOverride = null)
        {
            this.Discord = Client;
            this.RateLimitBucket = Bucket;
            this.RequestTaskSource = new TaskCompletionSource<RestResponse>();
            this.Url = Url;
            this.Method = Method;
            this.Route = Route;
            this.RateLimitWaitOverride = RatelimitWaitOverride;

            if (Headers != null)
            {
                Headers = Headers.Select(X => new KeyValuePair<string, string>(X.Key, Uri.EscapeDataString(X.Value)))
                    .ToDictionary(X => X.Key, X => X.Value);
                this.Headers = Headers;
            }
        }

        /// <summary>
        /// Asynchronously waits for this request to complete.
        /// </summary>
        /// <returns>HTTP response to this request.</returns>
        public Task<RestResponse> WaitForCompletion()
            => this.RequestTaskSource.Task;

        /// <summary>
        /// Sets as completed.
        /// </summary>
        /// <param name="Response">The response to set.</param>
        protected internal void SetCompleted(RestResponse Response)
            => this.RequestTaskSource.SetResult(Response);

        /// <summary>
        /// Sets as faulted.
        /// </summary>
        /// <param name="Ex">The exception to set.</param>
        protected internal void SetFaulted(Exception Ex)
            => this.RequestTaskSource.SetException(Ex);

        /// <summary>
        /// Tries to set as faulted.
        /// </summary>
        /// <param name="Ex">The exception to set.</param>
        protected internal bool TrySetFaulted(Exception Ex)
            => this.RequestTaskSource.TrySetException(Ex);

    }
}
