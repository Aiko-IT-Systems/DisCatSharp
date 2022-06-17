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
using System.Threading.Tasks;

using DisCatSharp.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the guild ban async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	internal async Task<DiscordBan> GetGuildBanAsync(ulong guildId, ulong userId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guildId, user_id = userId }, out var path);
		var uri = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, uri, RestRequestMethod.GET, route).ConfigureAwait(false);
		var json = JObject.Parse(res.Response);

		var ban = json.ToObject<DiscordBan>();

		return ban;
	}


	/// <summary>
	/// Implements https://discord.com/developers/docs/resources/guild#get-guild-bans.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guildId, int? limit, ulong? before, ulong? after)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guildId }, out var path);

		var urlParams = new Dictionary<string, string>();
		if (limit != null)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (after != null)
			urlParams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);

		var url = Utilities.GetApiUriFor(path, BuildQueryString(urlParams), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var bansRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordBan>>(res.Response).Select(xb =>
		{
			if (!this.Discord.TryGetCachedUserInternal(xb.RawUser.Id, out var usr))
			{
				usr = new DiscordUser(xb.RawUser) { Discord = this.Discord };
				usr = this.Discord.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					return old;
				});
			}

			xb.User = usr;
			return xb;
		});
		var bans = new ReadOnlyCollection<DiscordBan>(new List<DiscordBan>(bansRaw));

		return bans;
	}

	/// <summary>
	/// Creates the guild ban async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="deleteMessageDays">The delete_message_days.</param>
	/// <param name="reason">The reason.</param>
	internal Task CreateGuildBanAsync(ulong guildId, ulong userId, int deleteMessageDays, string reason)
	{
		if (deleteMessageDays < 0 || deleteMessageDays > 7)
			throw new ArgumentException("Delete message days must be a number between 0 and 7.", nameof(deleteMessageDays));

		var urlParams = new Dictionary<string, string>
		{
			["delete_message_days"] = deleteMessageDays.ToString(CultureInfo.InvariantCulture)
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, BuildQueryString(urlParams), this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
	}

	/// <summary>
	/// Removes the guild ban async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task RemoveGuildBanAsync(ulong guildId, ulong userId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
}

