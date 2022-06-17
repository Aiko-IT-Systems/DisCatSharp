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
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Modifies the guild channel position async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyGuildChannelPositionAsync(ulong guildId, IEnumerable<RestGuildChannelReorderPayload> pld, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Modifies the guild channel parent async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>

	internal Task ModifyGuildChannelParentAsync(ulong guildId, IEnumerable<RestGuildChannelNewParentPayload> pld, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Detaches the guild channel parent async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>

	internal Task DetachGuildChannelParentAsync(ulong guildId, IEnumerable<RestGuildChannelNoParentPayload> pld, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Creates the guild channel async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="type">The type.</param>
	/// <param name="parent">The parent.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="bitrate">The bitrate.</param>
	/// <param name="userLimit">The user_limit.</param>
	/// <param name="overwrites">The overwrites.</param>
	/// <param name="nsfw">If true, nsfw.</param>
	/// <param name="perUserRateLimit">The per user rate limit.</param>
	/// <param name="qualityMode">The quality mode.</param>
	/// <param name="defaultAutoArchiveDuration">The default auto archive duration.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordChannel> CreateGuildChannelAsync(ulong guildId, string name, ChannelType type, ulong? parent, Optional<string> topic, int? bitrate, int? userLimit, IEnumerable<DiscordOverwriteBuilder> overwrites, bool? nsfw, Optional<int?> perUserRateLimit, VideoQualityMode? qualityMode, ThreadAutoArchiveDuration? defaultAutoArchiveDuration, string reason)
	{
		var restOverwrites = new List<DiscordRestOverwrite>();
		if (overwrites != null)
			foreach (var ow in overwrites)
				restOverwrites.Add(ow.Build());

		var pld = new RestChannelCreatePayload
		{
			Name = name,
			Type = type,
			Parent = parent,
			Topic = topic,
			Bitrate = bitrate,
			UserLimit = userLimit,
			PermissionOverwrites = restOverwrites,
			Nsfw = nsfw,
			PerUserRateLimit = perUserRateLimit,
			QualityMode = qualityMode,
			DefaultAutoArchiveDuration = defaultAutoArchiveDuration
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
	/// Modifies the channel async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="position">The position.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="nsfw">If true, nsfw.</param>
	/// <param name="parent">The parent.</param>
	/// <param name="bitrate">The bitrate.</param>
	/// <param name="userLimit">The user_limit.</param>
	/// <param name="perUserRateLimit">The per user rate limit.</param>
	/// <param name="rtcRegion">The rtc region.</param>
	/// <param name="qualityMode">The quality mode.</param>
	/// <param name="autoArchiveDuration">The default auto archive duration.</param>
	/// <param name="type">The type.</param>
	/// <param name="permissionOverwrites">The permission overwrites.</param>
	/// <param name="bannerb64">The banner.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyChannelAsync(ulong channelId, string name, int? position, Optional<string> topic, bool? nsfw, Optional<ulong?> parent, int? bitrate, int? userLimit, Optional<int?> perUserRateLimit, Optional<string> rtcRegion, VideoQualityMode? qualityMode, ThreadAutoArchiveDuration? autoArchiveDuration, Optional<ChannelType> type, IEnumerable<DiscordOverwriteBuilder> permissionOverwrites, Optional<string> bannerb64, string reason)
	{

		List<DiscordRestOverwrite> restoverwrites = null;
		if (permissionOverwrites != null)
		{
			restoverwrites = new List<DiscordRestOverwrite>();
			foreach (var ow in permissionOverwrites)
				restoverwrites.Add(ow.Build());
		}

		var pld = new RestChannelModifyPayload
		{
			Name = name,
			Position = position,
			Topic = topic,
			Nsfw = nsfw,
			Parent = parent,
			Bitrate = bitrate,
			UserLimit = userLimit,
			PerUserRateLimit = perUserRateLimit,
			RtcRegion = rtcRegion,
			QualityMode = qualityMode,
			DefaultAutoArchiveDuration = autoArchiveDuration,
			Type = type,
			PermissionOverwrites = restoverwrites,
			BannerBase64 = bannerb64
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Gets the guild channels async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var channelsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

		foreach (var ret in channelsRaw)
			foreach (var xo in ret.PermissionOverwritesInternal)
			{
				xo.Discord = this.Discord;
				xo.ChannelId = ret.Id;
			}

		return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channelsRaw));
	}

	/// <summary>
	/// Deletes the channel permission async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="overwriteId">The overwrite_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteChannelPermissionAsync(ulong channelId, ulong overwriteId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, overwrite_id = overwriteId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Edits the channel permissions async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="overwriteId">The overwrite_id.</param>
	/// <param name="allow">The allow.</param>
	/// <param name="deny">The deny.</param>
	/// <param name="type">The type.</param>
	/// <param name="reason">The reason.</param>

	internal Task EditChannelPermissionsAsync(ulong channelId, ulong overwriteId, Permissions allow, Permissions deny, string type, string reason)
	{
		var pld = new RestChannelPermissionEditPayload
		{
			Type = type,
			Allow = allow & PermissionMethods.FullPerms,
			Deny = deny & PermissionMethods.FullPerms
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId, overwrite_id = overwriteId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Follows the channel async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="webhookChannelId">The webhook_channel_id.</param>

	internal async Task<DiscordFollowedChannel> FollowChannelAsync(ulong channelId, ulong webhookChannelId)
	{
		var pld = new FollowedChannelAddPayload
		{
			WebhookChannelId = webhookChannelId
		};

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.FOLLOWERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var response = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return JsonConvert.DeserializeObject<DiscordFollowedChannel>(response.Response);
	}
}
