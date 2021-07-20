// This file is part of the DisCatSharp project.
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
using System.IO;
using System.Linq;
using DisCatSharp.Entities;

namespace DisCatSharp.Net
{
    /// <summary>
    /// Represents a multipart HTTP request.
    /// </summary>
    internal sealed class MultipartWebRequest : BaseRestRequest
    {
        /// <summary>
        /// Gets the dictionary of values attached to this request.
        /// </summary>
        public IReadOnlyDictionary<string, string> Values { get; }

        /// <summary>
        /// Gets the dictionary of files attached to this request.
        /// </summary>
        public IReadOnlyDictionary<string, Stream> Files { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartWebRequest"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="bucket">The bucket.</param>
        /// <param name="url">The url.</param>
        /// <param name="method">The method.</param>
        /// <param name="route">The route.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="values">The values.</param>
        /// <param name="files">The files.</param>
        /// <param name="ratelimit_wait_override">The ratelimit_wait_override.</param>
        internal MultipartWebRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
            IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimit_wait_override = null)
            : base(client, bucket, url, method, route, headers, ratelimit_wait_override)
        {
            this.Values = values;
            this.Files = files.ToDictionary(x => x.FileName, x => x.Stream);
        }
    }
}
