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
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Creates a scheduled event.
	/// </summary>
	internal async Task<DiscordScheduledEvent> CreateGuildScheduledEventAsync(ulong guildId, ulong? channelId, DiscordScheduledEventEntityMetadata metadata, string name, DateTimeOffset scheduledStartTime, DateTimeOffset? scheduledEndTime, string description, ScheduledEventEntityType type, Optional<string> coverb64, string reason = null)
	{
		var pld = new RestGuildScheduledEventCreatePayload
		{
			ChannelId = channelId,
			EntityMetadata = metadata,
			Name = name,
			ScheduledStartTime = scheduledStartTime,
			ScheduledEndTime = scheduledEndTime,
			Description = description,
			EntityType = type,
			CoverBase64 = coverb64
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));

		var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
		var guild = this.Discord.Guilds[guildId];

		scheduledEvent.Discord = this.Discord;

		if (scheduledEvent.Creator != null)
			scheduledEvent.Creator.Discord = this.Discord;

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildScheduledEventCreateEventAsync(scheduledEvent, guild);

		return scheduledEvent;
	}

	/// <summary>
	/// Modifies a scheduled event.
	/// </summary>
	internal async Task<DiscordScheduledEvent> ModifyGuildScheduledEventAsync(ulong guildId, ulong scheduledEventId, Optional<ulong?> channelId, Optional<DiscordScheduledEventEntityMetadata> metadata, Optional<string> name, Optional<DateTimeOffset> scheduledStartTime, Optional<DateTimeOffset> scheduledEndTime, Optional<string> description, Optional<ScheduledEventEntityType> type, Optional<ScheduledEventStatus> status, Optional<string> coverb64, string reason = null)
	{
		var pld = new RestGuildScheduledEventModifyPayload
		{
			ChannelId = channelId,
			EntityMetadata = metadata,
			Name = name,
			ScheduledStartTime = scheduledStartTime,
			ScheduledEndTime = scheduledEndTime,
			Description = description,
			EntityType = type,
			Status = status,
			CoverBase64 = coverb64
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, scheduled_event_id = scheduledEventId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));

		var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
		var guild = this.Discord.Guilds[guildId];

		scheduledEvent.Discord = this.Discord;

		if (scheduledEvent.Creator != null)
		{
			scheduledEvent.Creator.Discord = this.Discord;
			this.Discord.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (id, old) =>
			{
				old.Username = scheduledEvent.Creator.Username;
				old.Discriminator = scheduledEvent.Creator.Discriminator;
				old.AvatarHash = scheduledEvent.Creator.AvatarHash;
				old.Flags = scheduledEvent.Creator.Flags;
				return old;
			});
		}

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildScheduledEventUpdateEventAsync(scheduledEvent, guild);

		return scheduledEvent;
	}

	/// <summary>
	/// Modifies a scheduled event.
	/// </summary>
	internal async Task<DiscordScheduledEvent> ModifyGuildScheduledEventStatusAsync(ulong guildId, ulong scheduledEventId, ScheduledEventStatus status, string reason = null)
	{
		var pld = new RestGuildScheduledEventModifyPayload
		{
			Status = status
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, scheduled_event_id = scheduledEventId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));

		var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
		var guild = this.Discord.Guilds[guildId];

		scheduledEvent.Discord = this.Discord;

		if (scheduledEvent.Creator != null)
		{
			scheduledEvent.Creator.Discord = this.Discord;
			this.Discord.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (id, old) =>
			{
				old.Username = scheduledEvent.Creator.Username;
				old.Discriminator = scheduledEvent.Creator.Discriminator;
				old.AvatarHash = scheduledEvent.Creator.AvatarHash;
				old.Flags = scheduledEvent.Creator.Flags;
				return old;
			});
		}

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildScheduledEventUpdateEventAsync(scheduledEvent, guild);

		return scheduledEvent;
	}

	/// <summary>
	/// Gets a scheduled event.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="scheduledEventId">The event id.</param>
	/// <param name="withUserCount">Whether to include user count.</param>
	internal async Task<DiscordScheduledEvent> GetGuildScheduledEventAsync(ulong guildId, ulong scheduledEventId, bool? withUserCount)
	{
		var urlParams = new Dictionary<string, string>();
		if (withUserCount.HasValue)
			urlParams["with_user_count"] = withUserCount?.ToString();

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, scheduled_event_id = scheduledEventId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);

		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
		var guild = this.Discord.Guilds[guildId];

		scheduledEvent.Discord = this.Discord;

		if (scheduledEvent.Creator != null)
		{
			scheduledEvent.Creator.Discord = this.Discord;
			this.Discord.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (id, old) =>
			{
				old.Username = scheduledEvent.Creator.Username;
				old.Discriminator = scheduledEvent.Creator.Discriminator;
				old.AvatarHash = scheduledEvent.Creator.AvatarHash;
				old.Flags = scheduledEvent.Creator.Flags;
				return old;
			});
		}

		return scheduledEvent;
	}

	/// <summary>
	/// Gets the guilds scheduled events.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="withUserCount">Whether to include the count of users subscribed to the scheduled event.</param>
	internal async Task<IReadOnlyDictionary<ulong, DiscordScheduledEvent>> ListGuildScheduledEventsAsync(ulong guildId, bool? withUserCount)
	{
		var urlParams = new Dictionary<string, string>();
		if (withUserCount.HasValue)
			urlParams["with_user_count"] = withUserCount?.ToString();

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var events = new Dictionary<ulong, DiscordScheduledEvent>();
		var eventsRaw = JsonConvert.DeserializeObject<List<DiscordScheduledEvent>>(res.Response);
		var guild = this.Discord.Guilds[guildId];

		foreach (var ev in eventsRaw)
		{
			ev.Discord = this.Discord;
			if (ev.Creator != null)
			{
				ev.Creator.Discord = this.Discord;
				this.Discord.UserCache.AddOrUpdate(ev.Creator.Id, ev.Creator, (id, old) =>
				{
					old.Username = ev.Creator.Username;
					old.Discriminator = ev.Creator.Discriminator;
					old.AvatarHash = ev.Creator.AvatarHash;
					old.Flags = ev.Creator.Flags;
					return old;
				});
			}

			events.Add(ev.Id, ev);
		}

		return new ReadOnlyDictionary<ulong, DiscordScheduledEvent>(new Dictionary<ulong, DiscordScheduledEvent>(events));
	}

	/// <summary>
	/// Deletes a guild scheduled event.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="scheduledEventId">The scheduled event id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteGuildScheduledEventAsync(ulong guildId, ulong scheduledEventId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, scheduled_event_id = scheduledEventId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Gets the users who RSVP'd to a scheduled event.
	/// Optional with member objects.
	/// This endpoint is paginated.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="scheduledEventId">The scheduled event id.</param>
	/// <param name="limit">The limit how many users to receive from the event.</param>
	/// <param name="before">Get results before the given id.</param>
	/// <param name="after">Get results after the given id.</param>
	/// <param name="withMember">Whether to include guild member data. attaches guild_member property to the user object.</param>
	internal async Task<IReadOnlyDictionary<ulong, DiscordScheduledEventUser>> GetGuildScheduledEventRspvUsersAsync(ulong guildId, ulong scheduledEventId, int? limit, ulong? before, ulong? after, bool? withMember)
	{
		var urlParams = new Dictionary<string, string>();
		if (limit != null && limit > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (after != null)
			urlParams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);
		if (withMember != null)
			urlParams["with_member"] = withMember.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id{Endpoints.USERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, scheduled_event_id = scheduledEventId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var rspvUsers = JsonConvert.DeserializeObject<IEnumerable<DiscordScheduledEventUser>>(res.Response);
		Dictionary<ulong, DiscordScheduledEventUser> rspv = new();

		foreach (var rspvUser in rspvUsers)
		{

			rspvUser.Discord = this.Discord;
			rspvUser.GuildId = guildId;

			rspvUser.User.Discord = this.Discord;
			rspvUser.User = this.Discord.UserCache.AddOrUpdate(rspvUser.User.Id, rspvUser.User, (id, old) =>
			{
				old.Username = rspvUser.User.Username;
				old.Discriminator = rspvUser.User.Discriminator;
				old.AvatarHash = rspvUser.User.AvatarHash;
				old.BannerHash = rspvUser.User.BannerHash;
				old.BannerColorInternal = rspvUser.User.BannerColorInternal;
				return old;
			});

			/*if (with_member.HasValue && with_member.Value && rspv_user.Member != null)
				{
					rspv_user.Member.Discord = this.Discord;
				}*/

			rspv.Add(rspvUser.User.Id, rspvUser);
		}

		return new ReadOnlyDictionary<ulong, DiscordScheduledEventUser>(new Dictionary<ulong, DiscordScheduledEventUser>(rspv));
	}
}
