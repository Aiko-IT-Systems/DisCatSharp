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
        /// Overwrites the file id start.
        /// </summary>
        public int? OverwriteFileIdStart { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartWebRequest"/> class.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="Url">The url.</param>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="Headers">The headers.</param>
        /// <param name="Values">The values.</param>
        /// <param name="Files">The files.</param>
        /// <param name="ratelimit_wait_override">The ratelimit_wait_override.</param>
        /// <param name="overwrite_file_id_start">The file id start.</param>
        internal MultipartWebRequest(BaseDiscordClient Client, RateLimitBucket Bucket, Uri Url, RestRequestMethod Method, string Route, IReadOnlyDictionary<string, string> Headers = null, IReadOnlyDictionary<string, string> Values = null,
            IReadOnlyCollection<DiscordMessageFile> Files = null, double? RatelimitWaitOverride = null, int? OverwriteFileIdStart = null)
            : base(Client, Bucket, Url, Method, Route, Headers, RatelimitWaitOverride)
        {
            this.Values = Values;
            this.OverwriteFileIdStart = OverwriteFileIdStart;
            this.Files = Files.ToDictionary(X => X.FileName, X => X.Stream);
        }
    }


    /// <summary>
    /// Represents a multipart HTTP request for stickers.
    /// </summary>
    internal sealed class MultipartStickerWebRequest : BaseRestRequest
    {
        /// <summary>
        /// Gets the file.
        /// </summary>
        public DiscordMessageFile File { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        public string Tags { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartStickerWebRequest"/> class.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="Url">The url.</param>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="Headers">The headers.</param>
        /// <param name="File">The file.</param>
        /// <param name="Name">The sticker name.</param>
        /// <param name="Tags">The sticker tag.</param>
        /// <param name="Description">The sticker description.</param>
        /// <param name="ratelimit_wait_override">The ratelimit_wait_override.</param>
        internal MultipartStickerWebRequest(BaseDiscordClient Client, RateLimitBucket Bucket, Uri Url, RestRequestMethod Method, string Route, IReadOnlyDictionary<string, string> Headers = null,
            DiscordMessageFile File = null, string Name = "", string Tags = "", string Description = "", double? RatelimitWaitOverride = null)
            : base(Client, Bucket, Url, Method, Route, Headers, RatelimitWaitOverride)
        {
            this.File = File;
            this.Name = Name;
            this.Description = Description;
            this.Tags = Tags;
        }
    }
}
