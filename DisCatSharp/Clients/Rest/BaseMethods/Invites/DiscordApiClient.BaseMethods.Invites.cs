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
	/// Gets the guild invites async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INVITES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

		return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
	}

	/// <summary>
	/// Gets the invite async.
	/// </summary>
	/// <param name="inviteCode">The invite_code.</param>
	/// <param name="withCounts">If true, with_counts.</param>
	/// <param name="withExpiration">If true, with_expiration.</param>
	/// <param name="guildScheduledEventId">The scheduled event id to get.</param>

	internal async Task<DiscordInvite> GetInviteAsync(string inviteCode, bool? withCounts, bool? withExpiration, ulong? guildScheduledEventId)
	{
		var urlParams = new Dictionary<string, string>();
		if (withCounts.HasValue)
			urlParams["with_counts"] = withCounts?.ToString();
		if (withExpiration.HasValue)
			urlParams["with_expiration"] = withExpiration?.ToString();
		if (guildScheduledEventId.HasValue)
			urlParams["guild_scheduled_event_id"] = guildScheduledEventId?.ToString();

		var route = $"{Endpoints.INVITES}/:invite_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {invite_code = inviteCode }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes the invite async.
	/// </summary>
	/// <param name="inviteCode">The invite_code.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordInvite> DeleteInviteAsync(string inviteCode, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.INVITES}/:invite_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {invite_code = inviteCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Gets the channel invites async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

		return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
	}

	/// <summary>
	/// Creates the channel invite async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="maxAge">The max_age.</param>
	/// <param name="maxUses">The max_uses.</param>
	/// <param name="targetType">The target_type.</param>
	/// <param name="targetApplication">The target_application.</param>
	/// <param name="targetUser">The target_user.</param>
	/// <param name="temporary">If true, temporary.</param>
	/// <param name="unique">If true, unique.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordInvite> CreateChannelInviteAsync(ulong channelId, int maxAge, int maxUses, TargetType? targetType, TargetActivity? targetApplication, ulong? targetUser, bool temporary, bool unique, string reason)
	{
		var pld = new RestChannelInviteCreatePayload
		{
			MaxAge = maxAge,
			MaxUses = maxUses,
			TargetType = targetType,
			TargetApplication = targetApplication,
			TargetUserId = targetUser,
			Temporary = temporary,
			Unique = unique
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/*
         * Disabled due to API restrictions
         *
         * internal async Task<DiscordInvite> InternalAcceptInvite(string invite_code)
         * {
         *     this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "REST API", "Invite accept endpoint was used; this account is now likely unverified", DateTime.Now);
         *
         *     var url = new Uri($"{Utils.GetApiBaseUri(this.Configuration), Endpoints.INVITES}/{invite_code));
         *     var bucket = this.Rest.GetBucket(0, MajorParameterType.Unbucketed, url, HttpRequestMethod.POST);
         *     var res = await this.DoRequestAsync(this.Discord, bucket, url, HttpRequestMethod.POST).ConfigureAwait(false);
         *
         *     var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
         *     ret.Discord = this.Discord;
         *
         *     return ret;
         * }
         */
}
