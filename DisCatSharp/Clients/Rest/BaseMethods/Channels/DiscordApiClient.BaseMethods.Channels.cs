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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the channel async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal async Task<DiscordChannel> GetChannelAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
		ret.Discord = this.Discord;
		foreach (var xo in ret.PermissionOverwritesInternal)
		{
			xo.Discord = this.Discord;
			xo.ChannelId = ret.Id;
		}

		return ret;
	}

	/// <summary>
	/// Deletes the channel async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteChannelAsync(ulong channelId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Adds the group dm recipient async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="accessToken">The access_token.</param>
	/// <param name="nickname">The nickname.</param>

	internal Task AddGroupDmRecipientAsync(ulong channelId, ulong userId, string accessToken, string nickname)
	{
		var pld = new RestChannelGroupDmRecipientAddPayload
		{
			AccessToken = accessToken,
			Nickname = nickname
		};

		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Removes the group dm recipient async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="userId">The user_id.</param>

	internal Task RemoveGroupDmRecipientAsync(ulong channelId, ulong userId)
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Creates the group dm async.
	/// </summary>
	/// <param name="accessTokens">The access_tokens.</param>
	/// <param name="nicks">The nicks.</param>

	internal async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> accessTokens, IDictionary<ulong, string> nicks)
	{
		var pld = new RestUserGroupDmCreatePayload
		{
			AccessTokens = accessTokens,
			Nicknames = nicks
		};

		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Creates the dm async.
	/// </summary>
	/// <param name="recipientId">The recipient_id.</param>

	internal async Task<DiscordDmChannel> CreateDmAsync(ulong recipientId)
	{
		var pld = new RestUserDmCreatePayload
		{
			Recipient = recipientId
		};

		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}
}
