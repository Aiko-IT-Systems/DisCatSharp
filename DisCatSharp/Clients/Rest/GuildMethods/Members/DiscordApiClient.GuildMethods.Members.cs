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
	/// Searches the members async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="limit">The limit.</param>
	internal async Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(ulong guildId, string name, int? limit)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.SEARCH}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);
		var querydict = new Dictionary<string, string>
		{
			["query"] = name,
			["limit"] = limit.ToString()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var json = JArray.Parse(res.Response);
		var tms = json.ToObject<IReadOnlyList<TransportMember>>();

		var mbrs = new List<DiscordMember>();
		foreach (var xtm in tms)
		{
			var usr = new DiscordUser(xtm.User) { Discord = this.Discord };

			this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discord = usr.Discord;
				old.AvatarHash = usr.AvatarHash;

				return old;
			});

			mbrs.Add(new DiscordMember(xtm) { Discord = this.Discord, GuildId = guildId });
		}

		return mbrs;
	}

	/// <summary>
	/// Adds the guild member async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="accessToken">The access_token.</param>
	/// <param name="nick">The nick.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="muted">If true, muted.</param>
	/// <param name="deafened">If true, deafened.</param>

	internal async Task<DiscordMember> AddGuildMemberAsync(ulong guildId, ulong userId, string accessToken, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
	{
		var pld = new RestGuildMemberAddPayload
		{
			AccessToken = accessToken,
			Nickname = nick ?? "",
			Roles = roles ?? new List<DiscordRole>(),
			Deaf = deafened,
			Mute = muted
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

		return new DiscordMember(tm) { Discord = this.Discord, GuildId = guildId };
	}

	/// <summary>
	/// Lists the guild members async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="limit">The limit.</param>
	/// <param name="after">The after.</param>

	internal async Task<IReadOnlyList<TransportMember>> ListGuildMembersAsync(ulong guildId, int? limit, ulong? after)
	{
		var urlParams = new Dictionary<string, string>();
		if (limit != null && limit > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
		if (after != null)
			urlParams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var membersRaw = JsonConvert.DeserializeObject<List<TransportMember>>(res.Response);
		return new ReadOnlyCollection<TransportMember>(membersRaw);
	}

	/// <summary>
	/// Adds the guild member role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task AddGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {guild_id = guildId, user_id = userId, role_id = roleId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
	}

	/// <summary>
	/// Removes the guild member role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task RemoveGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, user_id = userId, role_id = roleId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Gets the guild member async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>

	internal async Task<DiscordMember> GetGuildMemberAsync(ulong guildId, ulong userId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

		var usr = new DiscordUser(tm.User) { Discord = this.Discord };
		usr = this.Discord.UserCache.AddOrUpdate(tm.User.Id, usr, (id, old) =>
		{
			old.Username = usr.Username;
			old.Discriminator = usr.Discriminator;
			old.AvatarHash = usr.AvatarHash;
			return old;
		});

		return new DiscordMember(tm)
		{
			Discord = this.Discord,
			GuildId = guildId
		};
	}

	/// <summary>
	/// Removes the guild member async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string reason)
	{
		var urlParams = new Dictionary<string, string>();
		if (reason != null)
			urlParams["reason"] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, BuildQueryString(urlParams), this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Modifies the guild member async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="nick">The nick.</param>
	/// <param name="roleIds">The role_ids.</param>
	/// <param name="mute">The mute.</param>
	/// <param name="deaf">The deaf.</param>
	/// <param name="voiceChannelId">The voice_channel_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task ModifyGuildMemberAsync(ulong guildId, ulong userId, Optional<string> nick,
		Optional<IEnumerable<ulong>> roleIds, Optional<bool> mute, Optional<bool> deaf,
		Optional<ulong?> voiceChannelId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var pld = new RestGuildMemberModifyPayload
		{
			Nickname = nick,
			RoleIds = roleIds,
			Deafen = deaf,
			Mute = mute,
			VoiceChannelId = voiceChannelId
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Modifies the time out of a guild member.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="until">Datetime offset.</param>
	/// <param name="reason">The reason.</param>

	internal Task ModifyTimeoutAsync(ulong guildId, ulong userId, DateTimeOffset? until, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var pld = new RestGuildMemberTimeoutModifyPayload
		{
			CommunicationDisabledUntil = until
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Modifies the current member nickname async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="nick">The nick.</param>
	/// <param name="reason">The reason.</param>

	internal Task ModifyCurrentMemberNicknameAsync(ulong guildId, string nick, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var pld = new RestGuildMemberModifyPayload
		{
			Nickname = nick
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Gets the guild prune count async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="days">The days.</param>
	/// <param name="includeRoles">The include_roles.</param>

	internal async Task<int> GetGuildPruneCountAsync(ulong guildId, int days, IEnumerable<ulong> includeRoles)
	{
		if (days < 0 || days > 30)
			throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));

		var urlParams = new Dictionary<string, string>
		{
			["days"] = days.ToString(CultureInfo.InvariantCulture)
		};

		var sb = includeRoles?.Aggregate(new StringBuilder(),
				 (sb, id) => sb.Append($"&include_roles={id}"))
			 ?? new StringBuilder();

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PRUNE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);
		var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlParams)}{sb}", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

		return pruned.Pruned.Value;
	}

	/// <summary>
	/// Begins the guild prune async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="days">The days.</param>
	/// <param name="computePruneCount">If true, compute_prune_count.</param>
	/// <param name="includeRoles">The include_roles.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<int?> BeginGuildPruneAsync(ulong guildId, int days, bool computePruneCount, IEnumerable<ulong> includeRoles, string reason)
	{
		if (days < 0 || days > 30)
			throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));

		var urlParams = new Dictionary<string, string>
		{
			["days"] = days.ToString(CultureInfo.InvariantCulture),
			["compute_prune_count"] = computePruneCount.ToString()
		};

		var sb = includeRoles?.Aggregate(new StringBuilder(),
				 (sb, id) => sb.Append($"&include_roles={id}"))
			 ?? new StringBuilder();

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PRUNE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlParams)}{sb}", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers).ConfigureAwait(false);

		var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

		return pruned.Pruned;
	}
}
