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
using System.Globalization;
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
	/// Creates the thread.
	/// </summary>
	/// <param name="channelId">The channel id to create the thread in.</param>
	/// <param name="messageId">The optional message id to create the thread from.</param>
	/// <param name="name">The name of the thread.</param>
	/// <param name="autoArchiveDuration">The auto_archive_duration for the thread.</param>
	/// <param name="type">Can be either <see cref="ChannelType.PublicThread"/> or <see cref="ChannelType.PrivateThread"/>.</param>
	/// <param name="rateLimitPerUser">The rate limit per user.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordThreadChannel> CreateThreadAsync(ulong channelId, ulong? messageId, string name,
		ThreadAutoArchiveDuration autoArchiveDuration, ChannelType type, int? rateLimitPerUser, string reason)
	{
		var pld = new RestThreadChannelCreatePayload
		{
			Name = name,
			AutoArchiveDuration = autoArchiveDuration,
			PerUserRateLimit = rateLimitPerUser,
			Type = type
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id";
		if (messageId is not null)
			route += $"{Endpoints.MESSAGES}/:message_id";
		route += Endpoints.THREADS;

		object param = messageId is null
			? new {channel_id = channelId}
			: new {channel_id = channelId, message_id = messageId};

		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, param, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));

		var threadChannel = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);

		threadChannel.Discord = this.Discord;

		return threadChannel;
	}

	/// <summary>
	/// Gets the thread.
	/// </summary>
	/// <param name="threadId">The thread id.</param>
	internal async Task<DiscordThreadChannel> GetThreadAsync(ulong threadId)
	{
		var route = $"{Endpoints.CHANNELS}/:thread_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {thread_id = threadId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Joins the thread.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	internal async Task JoinThreadAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
	}

	/// <summary>
	/// Leaves the thread.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	internal async Task LeaveThreadAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Adds a thread member.
	/// </summary>
	/// <param name="channelId">The channel id to add the member to.</param>
	/// <param name="userId">The user id to add.</param>
	internal async Task AddThreadMemberAsync(ulong channelId, ulong userId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
	}

	/// <summary>
	/// Gets a thread member.
	/// </summary>
	/// <param name="channelId">The channel id to get the member from.</param>
	/// <param name="userId">The user id to get.</param>
	internal async Task<DiscordThreadChannelMember> GetThreadMemberAsync(ulong channelId, ulong userId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var threadMember = JsonConvert.DeserializeObject<DiscordThreadChannelMember>(res.Response);

		return threadMember;
	}

	/// <summary>
	/// Removes a thread member.
	/// </summary>
	/// <param name="channelId">The channel id to remove the member from.</param>
	/// <param name="userId">The user id to remove.</param>
	internal async Task RemoveThreadMemberAsync(ulong channelId, ulong userId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Gets the thread members.
	/// </summary>
	/// <param name="threadId">The thread id.</param>
	internal async Task<IReadOnlyList<DiscordThreadChannelMember>> GetThreadMembersAsync(ulong threadId)
	{
		var route = $"{Endpoints.CHANNELS}/:thread_id{Endpoints.THREAD_MEMBERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {thread_id = threadId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var threadMembersRaw = JsonConvert.DeserializeObject<List<DiscordThreadChannelMember>>(res.Response);

		return new ReadOnlyCollection<DiscordThreadChannelMember>(threadMembersRaw);
	}

	/// <summary>
	/// Gets the active threads in a guild.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<DiscordThreadResult> GetActiveThreadsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.THREADS}{Endpoints.THREAD_ACTIVE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

		return threadReturn;
	}

	/// <summary>
	/// Gets the joined private archived threads in a channel.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="before">Get threads before snowflake.</param>
	/// <param name="limit">Limit the results.</param>
	internal async Task<DiscordThreadResult> GetJoinedPrivateArchivedThreadsAsync(ulong channelId, ulong? before, int? limit)
	{
		var urlParams = new Dictionary<string, string>();
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (limit != null && limit > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.USERS}{Endpoints.ME}{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PRIVATE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

		return threadReturn;
	}

	/// <summary>
	/// Gets the public archived threads in a channel.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="before">Get threads before snowflake.</param>
	/// <param name="limit">Limit the results.</param>
	internal async Task<DiscordThreadResult> GetPublicArchivedThreadsAsync(ulong channelId, ulong? before, int? limit)
	{
		var urlParams = new Dictionary<string, string>();
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (limit != null && limit > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PUBLIC}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

		return threadReturn;
	}

	/// <summary>
	/// Gets the private archived threads in a channel.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="before">Get threads before snowflake.</param>
	/// <param name="limit">Limit the results.</param>
	internal async Task<DiscordThreadResult> GetPrivateArchivedThreadsAsync(ulong channelId, ulong? before, int? limit)
	{
		var urlParams = new Dictionary<string, string>();
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (limit != null && limit > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PRIVATE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

		return threadReturn;
	}

	/// <summary>
	/// Modifies a thread.
	/// </summary>
	/// <param name="threadId">The thread to modify.</param>
	/// <param name="name">The new name.</param>
	/// <param name="locked">The new locked state.</param>
	/// <param name="archived">The new archived state.</param>
	/// <param name="perUserRateLimit">The new per user rate limit.</param>
	/// <param name="autoArchiveDuration">The new auto archive duration.</param>
	/// <param name="invitable">The new user invitable state.</param>
	/// <param name="reason">The reason for the modification.</param>
	internal Task ModifyThreadAsync(ulong threadId, string name, Optional<bool?> locked, Optional<bool?> archived, Optional<int?> perUserRateLimit, Optional<ThreadAutoArchiveDuration?> autoArchiveDuration, Optional<bool?> invitable, string reason)
	{
		var pld = new RestThreadChannelModifyPayload
		{
			Name = name,
			Archived = archived,
			AutoArchiveDuration = autoArchiveDuration,
			Locked = locked,
			PerUserRateLimit = perUserRateLimit,
			Invitable = invitable
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:thread_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {thread_id = threadId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Deletes a thread.
	/// </summary>
	/// <param name="threadId">The thread to delete.</param>
	/// <param name="reason">The reason for deletion.</param>
	internal Task DeleteThreadAsync(ulong threadId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:thread_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {thread_id = threadId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
}
