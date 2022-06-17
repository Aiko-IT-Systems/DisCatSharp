// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
using System.Net;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a discord api client.
/// </summary>
public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	internal BaseDiscordClient Discord { get; }

	/// <summary>
	/// Gets the rest client.
	/// </summary>
	internal RestClient Rest { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApiClient"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	internal DiscordApiClient(BaseDiscordClient client)
	{
		this.Discord = client;
		this.Rest = new RestClient(client);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApiClient"/> class.
	/// </summary>
	/// <param name="proxy">The proxy.</param>
	/// <param name="timeout">The timeout.</param>
	/// <param name="useRelativeRateLimit">If true, use relative rate limit.</param>
	/// <param name="logger">The logger.</param>
	internal DiscordApiClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRateLimit, ILogger logger) // This is for meta-clients, such as the webhook client
	{
		this.Rest = new RestClient(proxy, timeout, useRelativeRateLimit, logger);
	}

	/// <summary>
	/// Builds the query string.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <param name="post">Whether this query will be transmitted via POST.</param>
	private static string BuildQueryString(IDictionary<string, string> values, bool post = false)
	{
		if (values == null || values.Count == 0)
			return string.Empty;

		var valsCollection = values.Select(xkvp =>
			$"{WebUtility.UrlEncode(xkvp.Key)}={WebUtility.UrlEncode(xkvp.Value)}");
		var vals = string.Join("&", valsCollection);

		return !post ? $"?{vals}" : vals;
	}

	/// <summary>
	/// Executes a rest request.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="payload">The payload.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	internal Task<RestResponse> ExecuteRestRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, string payload = null, double? ratelimitWaitOverride = null)
	{
		var req = new RestRequest(client, bucket, url, method, route, headers, payload, ratelimitWaitOverride);

		if (this.Discord != null)
			this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req);

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	/// Executes a multipart rest request for stickers.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="file">The file.</param>
	/// <param name="name">The sticker name.</param>
	/// <param name="tags">The sticker tag.</param>
	/// <param name="description">The sticker description.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	private Task<RestResponse> ExecuteStickerMultipartRestRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null,
		DiscordMessageFile file = null, string name = "", string tags = "", string description = "", double? ratelimitWaitOverride = null)
	{
		var req = new MultipartStickerWebRequest(client, bucket, url, method, route, headers, file, name, tags, description, ratelimitWaitOverride);

		if (this.Discord != null)
			this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req);

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	/// Executes a multipart request.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="values">The values.</param>
	/// <param name="files">The files.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	private Task<RestResponse> ExecuteMultipartRestRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
		IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimitWaitOverride = null)
	{
		var req = new MultipartWebRequest(client, bucket, url, method, route, headers, values, files, ratelimitWaitOverride);

		if (this.Discord != null)
			this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req);

		return req.WaitForCompletionAsync();
	}
}
