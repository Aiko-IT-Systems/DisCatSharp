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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Creates the reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>

	internal Task CreateReactionAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PUT, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Deletes the own reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>

	internal Task DeleteOwnReactionAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Deletes the user reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteUserReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId, emoji, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Gets the reactions async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="afterId">The after_id.</param>
	/// <param name="limit">The limit.</param>

	internal async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, string emoji, ulong? afterId = null, int limit = 25)
	{
		var urlParams = new Dictionary<string, string>();
		if (afterId.HasValue)
			urlParams["after"] = afterId.Value.ToString(CultureInfo.InvariantCulture);

		urlParams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, BuildQueryString(urlParams), this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var reactersRaw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response);
		var reacters = new List<DiscordUser>();
		foreach (var xr in reactersRaw)
		{
			var usr = new DiscordUser(xr) { Discord = this.Discord };
			usr = this.Discord.UserCache.AddOrUpdate(xr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				return old;
			});

			reacters.Add(usr);
		}

		return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters));
	}

	/// <summary>
	/// Deletes the all reactions async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteAllReactionsAsync(ulong channelId, ulong messageId, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}

	/// <summary>
	/// Deletes the reactions emoji async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>

	internal Task DeleteReactionsEmojiAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId, emoji }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}
}
