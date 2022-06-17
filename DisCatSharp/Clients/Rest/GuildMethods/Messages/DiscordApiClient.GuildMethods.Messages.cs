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

using System.Threading.Tasks;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Crossposts the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal async Task<DiscordMessage> CrosspostMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.CROSSPOST}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var response = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route).ConfigureAwait(false);
		return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
	}
}
