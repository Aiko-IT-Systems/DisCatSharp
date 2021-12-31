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

namespace DisCatSharp.Net
{
    /// <summary>
    /// Represents a non-multipart HTTP request.
    /// </summary>
    internal sealed class RestRequest : BaseRestRequest
    {
        /// <summary>
        /// Gets the payload sent with this request.
        /// </summary>
        public string Payload { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestRequest"/> class.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="Url">The url.</param>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="Headers">The headers.</param>
        /// <param name="Payload">The payload.</param>
        /// <param name="RatelimitWaitOverride">The ratelimit wait override.</param>
        internal RestRequest(BaseDiscordClient Client, RateLimitBucket Bucket, Uri Url, RestRequestMethod Method, string Route, IReadOnlyDictionary<string, string> Headers = null, string Payload = null, double? RatelimitWaitOverride = null)
            : base(Client, Bucket, Url, Method, Route, Headers, RatelimitWaitOverride)
        {
            this.Payload = Payload;
        }
    }
}
