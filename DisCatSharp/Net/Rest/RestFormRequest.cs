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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a form data HTTP request.
/// </summary>
internal sealed class RestFormRequest : BaseRestRequest
{
	/// <summary>
	/// Gets the form data sent with this request.
	/// </summary>
	public Dictionary<string, string> FormData { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RestRequest"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="formData">The form data.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	internal RestFormRequest(DiscordOAuth2Client client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, Dictionary<string, string> formData, IReadOnlyDictionary<string, string> headers = null, double? ratelimitWaitOverride = null)
		: base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
	{
		this.FormData = formData;
	}
}
