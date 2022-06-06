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
using System.Net;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a discord api client.
/// </summary>
public sealed class DiscordApiClient
{
	/// <summary>
	/// The audit log reason header name.
	/// </summary>
	private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

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
	/// Prepares the message.
	/// </summary>
	/// <param name="msgRaw">The msg_raw.</param>
	/// <returns>A DiscordMessage.</returns>
	private DiscordMessage PrepareMessage(JToken msgRaw)
	{
		var author = msgRaw["author"].ToObject<TransportUser>();
		var ret = msgRaw.ToDiscordObject<DiscordMessage>();
		ret.Discord = this.Discord;

		this.PopulateMessage(author, ret);

		var referencedMsg = msgRaw["referenced_message"];
		if (ret.MessageType == MessageType.Reply && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
		{
			author = referencedMsg["author"].ToObject<TransportUser>();
			ret.ReferencedMessage.Discord = this.Discord;
			this.PopulateMessage(author, ret.ReferencedMessage);
		}

		if (ret.Channel != null)
			return ret;

		var channel = !ret.GuildId.HasValue
			? new DiscordDmChannel
			{
				Id = ret.ChannelId,
				Discord = this.Discord,
				Type = ChannelType.Private
			}
			: new DiscordChannel
			{
				Id = ret.ChannelId,
				GuildId = ret.GuildId,
				Discord = this.Discord
			};

		ret.Channel = channel;

		return ret;
	}

	/// <summary>
	/// Populates the message.
	/// </summary>
	/// <param name="author">The author.</param>
	/// <param name="ret">The message.</param>
	private void PopulateMessage(TransportUser author, DiscordMessage ret)
	{
		var guild = ret.Channel?.Guild;

		//If this is a webhook, it shouldn't be in the user cache.
		if (author.IsBot && int.Parse(author.Discriminator) == 0)
		{
			ret.Author = new DiscordUser(author) { Discord = this.Discord };
		}
		else
		{
			if (!this.Discord.UserCache.TryGetValue(author.Id, out var usr))
			{
				this.Discord.UserCache[author.Id] = usr = new DiscordUser(author) { Discord = this.Discord };
			}

			if (guild != null)
			{
				if (!guild.Members.TryGetValue(author.Id, out var mbr))
					mbr = new DiscordMember(usr) { Discord = this.Discord, GuildId = guild.Id };
				ret.Author = mbr;
			}
			else
			{
				ret.Author = usr;
			}
		}

		ret.PopulateMentions();

		if (ret.ReactionsInternal == null)
			ret.ReactionsInternal = new List<DiscordReaction>();
		foreach (var xr in ret.ReactionsInternal)
			xr.Emoji.Discord = this.Discord;
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
	internal Task<RestResponse> DoRequestAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, string payload = null, double? ratelimitWaitOverride = null)
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
	private Task<RestResponse> DoStickerMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null,
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
	private Task<RestResponse> DoMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
		IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimitWaitOverride = null)
	{
		var req = new MultipartWebRequest(client, bucket, url, method, route, headers, values, files, ratelimitWaitOverride);

		if (this.Discord != null)
			this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req);

		return req.WaitForCompletionAsync();
	}

	#region Guild

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
	/// Gets the guild ban async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	internal async Task<DiscordBan> GetGuildBanAsync(ulong guildId, ulong userId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, user_id = userId}, out var path);
		var uri = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, uri, RestRequestMethod.GET, route).ConfigureAwait(false);
		var json = JObject.Parse(res.Response);

		var ban = json.ToObject<DiscordBan>();

		return ban;
	}

	/// <summary>
	/// Creates the guild async.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="regionId">The region_id.</param>
	/// <param name="iconb64">The iconb64.</param>
	/// <param name="verificationLevel">The verification_level.</param>
	/// <param name="defaultMessageNotifications">The default_message_notifications.</param>
	/// <param name="systemChannelFlags">The system_channel_flags.</param>
	internal async Task<DiscordGuild> CreateGuildAsync(string name, string regionId, Optional<string> iconb64, VerificationLevel? verificationLevel,
		DefaultMessageNotifications? defaultMessageNotifications, SystemChannelFlags? systemChannelFlags)
	{
		var pld = new RestGuildCreatePayload
		{
			Name = name,
			RegionId = regionId,
			DefaultMessageNotifications = defaultMessageNotifications,
			VerificationLevel = verificationLevel,
			IconBase64 = iconb64,
			SystemChannelFlags = systemChannelFlags

		};

		var route = $"{Endpoints.GUILDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawMembers = (JArray)json["members"];
		var guild = json.ToDiscordObject<DiscordGuild>();

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildCreateEventAsync(guild, rawMembers, null).ConfigureAwait(false);
		return guild;
	}

	/// <summary>
	/// Creates the guild from template async.
	/// </summary>
	/// <param name="templateCode">The template_code.</param>
	/// <param name="name">The name.</param>
	/// <param name="iconb64">The iconb64.</param>
	internal async Task<DiscordGuild> CreateGuildFromTemplateAsync(string templateCode, string name, Optional<string> iconb64)
	{
		var pld = new RestGuildCreateFromTemplatePayload
		{
			Name = name,
			IconBase64 = iconb64
		};

		var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawMembers = (JArray)json["members"];
		var guild = json.ToDiscordObject<DiscordGuild>();

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildCreateEventAsync(guild, rawMembers, null).ConfigureAwait(false);
		return guild;
	}

	/// <summary>
	/// Deletes the guild async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task DeleteGuildAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

		if (this.Discord is DiscordClient dc)
		{
			var gld = dc.GuildsInternal[guildId];
			await dc.OnGuildDeleteEventAsync(gld).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Modifies the guild.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="name">The name.</param>
	/// <param name="verificationLevel">The verification level.</param>
	/// <param name="defaultMessageNotifications">The default message notifications.</param>
	/// <param name="mfaLevel">The mfa level.</param>
	/// <param name="explicitContentFilter">The explicit content filter.</param>
	/// <param name="afkChannelId">The afk channel id.</param>
	/// <param name="afkTimeout">The afk timeout.</param>
	/// <param name="iconb64">The iconb64.</param>
	/// <param name="ownerId">The owner id.</param>
	/// <param name="splashb64">The splashb64.</param>
	/// <param name="systemChannelId">The system channel id.</param>
	/// <param name="systemChannelFlags">The system channel flags.</param>
	/// <param name="publicUpdatesChannelId">The public updates channel id.</param>
	/// <param name="rulesChannelId">The rules channel id.</param>
	/// <param name="description">The description.</param>
	/// <param name="bannerb64">The banner base64.</param>
	/// <param name="discoverySplashb64">The discovery base64.</param>
	/// <param name="preferredLocale">The preferred locale.</param>
	/// <param name="premiumProgressBarEnabled">Whether the premium progress bar should be enabled.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, Optional<string> name, Optional<VerificationLevel> verificationLevel,
		Optional<DefaultMessageNotifications> defaultMessageNotifications, Optional<MfaLevel> mfaLevel,
		Optional<ExplicitContentFilter> explicitContentFilter, Optional<ulong?> afkChannelId,
		Optional<int> afkTimeout, Optional<string> iconb64, Optional<ulong> ownerId, Optional<string> splashb64,
		Optional<ulong?> systemChannelId, Optional<SystemChannelFlags> systemChannelFlags,
		Optional<ulong?> publicUpdatesChannelId, Optional<ulong?> rulesChannelId, Optional<string> description,
		Optional<string> bannerb64, Optional<string> discoverySplashb64, Optional<string> preferredLocale, Optional<bool> premiumProgressBarEnabled, string reason)
	{
		var pld = new RestGuildModifyPayload
		{
			Name = name,
			VerificationLevel = verificationLevel,
			DefaultMessageNotifications = defaultMessageNotifications,
			MfaLevel = mfaLevel,
			ExplicitContentFilter = explicitContentFilter,
			AfkChannelId = afkChannelId,
			AfkTimeout = afkTimeout,
			IconBase64 = iconb64,
			SplashBase64 = splashb64,
			BannerBase64 = bannerb64,
			DiscoverySplashBase64 = discoverySplashb64,
			OwnerId = ownerId,
			SystemChannelId = systemChannelId,
			SystemChannelFlags = systemChannelFlags,
			RulesChannelId = rulesChannelId,
			PublicUpdatesChannelId = publicUpdatesChannelId,
			PreferredLocale = preferredLocale,
			Description = description,
			PremiumProgressBarEnabled = premiumProgressBarEnabled
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawMembers = (JArray)json["members"];
		var guild = json.ToDiscordObject<DiscordGuild>();
		foreach (var r in guild.RolesInternal.Values)
			r.GuildId = guild.Id;

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildUpdateEventAsync(guild, rawMembers).ConfigureAwait(false);
		return guild;
	}



	/// <summary>
	/// Modifies the guild community settings.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="features">The guild features.</param>
	/// <param name="rulesChannelId">The rules channel id.</param>
	/// <param name="publicUpdatesChannelId">The public updates channel id.</param>
	/// <param name="preferredLocale">The preferred locale.</param>
	/// <param name="description">The description.</param>
	/// <param name="defaultMessageNotifications">The default message notifications.</param>
	/// <param name="explicitContentFilter">The explicit content filter.</param>
	/// <param name="verificationLevel">The verification level.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordGuild> ModifyGuildCommunitySettingsAsync(ulong guildId, List<string> features, Optional<ulong?> rulesChannelId, Optional<ulong?> publicUpdatesChannelId, string preferredLocale, string description, DefaultMessageNotifications defaultMessageNotifications, ExplicitContentFilter explicitContentFilter, VerificationLevel verificationLevel, string reason)
	{
		var pld = new RestGuildCommunityModifyPayload
		{
			VerificationLevel = verificationLevel,
			DefaultMessageNotifications = defaultMessageNotifications,
			ExplicitContentFilter = explicitContentFilter,
			RulesChannelId = rulesChannelId,
			PublicUpdatesChannelId = publicUpdatesChannelId,
			PreferredLocale = preferredLocale,
			Description = Optional.FromNullable(description),
			Features = features
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawMembers = (JArray)json["members"];
		var guild = json.ToDiscordObject<DiscordGuild>();
		foreach (var r in guild.RolesInternal.Values)
			r.GuildId = guild.Id;

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildUpdateEventAsync(guild, rawMembers).ConfigureAwait(false);
		return guild;
	}

	/// <summary>
	/// Enables the guilds mfa requirement.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task EnableGuildMfaAsync(ulong guildId, string reason)
	{
		var pld = new RestGuildMfaLevelModifyPayload
		{
			Level = MfaLevel.Enabled
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MFA}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	/// Disables the guilds mfa requirement.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task DisableGuildMfaAsync(ulong guildId, string reason)
	{
		var pld = new RestGuildMfaLevelModifyPayload
		{
			Level = MfaLevel.Disabled
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);
		
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MFA}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}
	
	/// <summary>
	/// Implements https://discord.com/developers/docs/resources/guild#get-guild-bans.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guildId, int? limit, ulong? before, ulong? after)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {guild_id = guildId, user_id = userId }, out var path);

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
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Leaves the guild async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal Task LeaveGuildAsync(ulong guildId)
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Modifies the guild role position async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>

	internal Task ModifyGuildRolePositionAsync(ulong guildId, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Gets the audit logs async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="limit">The limit.</param>
	/// <param name="after">The after.</param>
	/// <param name="before">The before.</param>
	/// <param name="responsible">The responsible.</param>
	/// <param name="actionType">The action_type.</param>

	internal async Task<AuditLog> GetAuditLogsAsync(ulong guildId, int limit, ulong? after, ulong? before, ulong? responsible, int? actionType)
	{
		var urlParams = new Dictionary<string, string>
		{
			["limit"] = limit.ToString(CultureInfo.InvariantCulture)
		};
		if (after != null)
			urlParams["after"] = after?.ToString(CultureInfo.InvariantCulture);
		if (before != null)
			urlParams["before"] = before?.ToString(CultureInfo.InvariantCulture);
		if (responsible != null)
			urlParams["user_id"] = responsible?.ToString(CultureInfo.InvariantCulture);
		if (actionType != null)
			urlParams["action_type"] = actionType?.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.AUDIT_LOGS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var auditLogDataRaw = JsonConvert.DeserializeObject<AuditLog>(res.Response);

		return auditLogDataRaw;
	}

	/// <summary>
	/// Gets the guild vanity url async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<DiscordInvite> GetGuildVanityUrlAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VANITY_URL}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var invite = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);

		return invite;
	}

	/// <summary>
	/// Gets the guild widget async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<DiscordWidget> GetGuildWidgetAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET_JSON}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawChannels = (JArray)json["channels"];

		var ret = json.ToDiscordObject<DiscordWidget>();
		ret.Discord = this.Discord;
		ret.Guild = this.Discord.Guilds[guildId];

		ret.Channels = ret.Guild == null
			? rawChannels.Select(r => new DiscordChannel
			{
				Id = (ulong)r["id"],
				Name = r["name"].ToString(),
				Position = (int)r["position"]
			}).ToList()
			: rawChannels.Select(r =>
			{
				var c = ret.Guild.GetChannel((ulong)r["id"]);
				c.Position = (int)r["position"];
				return c;
			}).ToList();

		return ret;
	}

	/// <summary>
	/// Gets the guild widget settings async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
		ret.Guild = this.Discord.Guilds[guildId];

		return ret;
	}

	/// <summary>
	/// Modifies the guild widget settings async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="isEnabled">If true, is enabled.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guildId, bool? isEnabled, ulong? channelId, string reason)
	{
		var pld = new RestGuildWidgetSettingsPayload
		{
			Enabled = isEnabled,
			ChannelId = channelId
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
		ret.Guild = this.Discord.Guilds[guildId];

		return ret;
	}

	/// <summary>
	/// Gets the guild templates async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var templatesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response);

		return new ReadOnlyCollection<DiscordGuildTemplate>(new List<DiscordGuildTemplate>(templatesRaw));
	}

	/// <summary>
	/// Creates the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>

	internal async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guildId, string name, string description)
	{
		var pld = new RestGuildTemplateCreateOrModifyPayload
		{
			Name = name,
			Description = description
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return ret;
	}

	/// <summary>
	/// Syncs the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>

	internal async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guildId, string templateCode)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {guild_id = guildId, template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);

		var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templateRaw;
	}

	/// <summary>
	/// Modifies the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>

	internal async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guildId, string templateCode, string name, string description)
	{
		var pld = new RestGuildTemplateCreateOrModifyPayload
		{
			Name = name,
			Description = description
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templateRaw;
	}

	/// <summary>
	/// Deletes the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>

	internal async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guildId, string templateCode)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

		var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templateRaw;
	}

	/// <summary>
	/// Gets the guild membership screening form async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var screeningRaw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

		return screeningRaw;
	}

	/// <summary>
	/// Modifies the guild membership screening form async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="enabled">The enabled.</param>
	/// <param name="fields">The fields.</param>
	/// <param name="description">The description.</param>

	internal async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guildId, Optional<bool> enabled, Optional<DiscordGuildMembershipScreeningField[]> fields, Optional<string> description)
	{
		var pld = new RestGuildMembershipScreeningFormModifyPayload
		{
			Enabled = enabled,
			Description = description,
			Fields = fields
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var screeningRaw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

		return screeningRaw;
	}

	/// <summary>
	/// Gets the guild welcome screen async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
		return ret;
	}

	/// <summary>
	/// Modifies the guild welcome screen async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="enabled">The enabled.</param>
	/// <param name="welcomeChannels">The welcome channels.</param>
	/// <param name="description">The description.</param>

	internal async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guildId, Optional<bool> enabled, Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> welcomeChannels, Optional<string> description)
	{
		var pld = new RestGuildWelcomeScreenModifyPayload
		{
			Enabled = enabled,
			WelcomeChannels = welcomeChannels,
			Description = description
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
		return ret;
	}

	/// <summary>
	/// Updates the current user voice state async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="suppress">If true, suppress.</param>
	/// <param name="requestToSpeakTimestamp">The request to speak timestamp.</param>

	internal async Task UpdateCurrentUserVoiceStateAsync(ulong guildId, ulong channelId, bool? suppress, DateTimeOffset? requestToSpeakTimestamp)
	{
		var pld = new RestGuildUpdateCurrentUserVoiceStatePayload
		{
			ChannelId = channelId,
			Suppress = suppress,
			RequestToSpeakTimestamp = requestToSpeakTimestamp
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/@me";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Updates the user voice state async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="suppress">If true, suppress.</param>

	internal async Task UpdateUserVoiceStateAsync(ulong guildId, ulong userId, ulong channelId, bool? suppress)
	{
		var pld = new RestGuildUpdateUserVoiceStatePayload
		{
			ChannelId = channelId,
			Suppress = suppress
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
	}
	#endregion

	#region Guild Scheduled Events

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));

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

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
	#endregion

	#region Channel
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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Gets the channel async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal async Task<DiscordChannel> GetChannelAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Gets the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal async Task<DiscordMessage> GetMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	/// Creates the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="content">The content.</param>
	/// <param name="embeds">The embeds.</param>
	/// <param name="sticker">The sticker.</param>
	/// <param name="replyMessageId">The reply message id.</param>
	/// <param name="mentionReply">If true, mention reply.</param>
	/// <param name="failOnInvalidReply">If true, fail on invalid reply.</param>

	internal async Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content, IEnumerable<DiscordEmbed> embeds, DiscordSticker sticker, ulong? replyMessageId, bool mentionReply, bool failOnInvalidReply)
	{
		if (content != null && content.Length > 2000)
			throw new ArgumentException("Message content length cannot exceed 2000 characters.");

		if (!embeds?.Any() ?? true)
		{
			if (content == null && sticker == null)
				throw new ArgumentException("You must specify message content, a sticker or an embed.");
			if (content.Length == 0)
				throw new ArgumentException("Message content must not be empty.");
		}

		if (embeds != null)
			foreach (var embed in embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var pld = new RestChannelMessageCreatePayload
		{
			HasContent = content != null,
			Content = content,
			StickersIds = sticker is null ? Array.Empty<ulong>() : new[] {sticker.Id},
			IsTts = false,
			HasEmbed = embeds?.Any() ?? false,
			Embeds = embeds
		};

		if (replyMessageId != null)
			pld.MessageReference = new InternalDiscordMessageReference { MessageId = replyMessageId, FailIfNotExists = failOnInvalidReply };

		if (replyMessageId != null)
			pld.Mentions = new DiscordMentions(Mentions.All, true, mentionReply);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	/// Creates the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="builder">The builder.</param>

	internal async Task<DiscordMessage> CreateMessageAsync(ulong channelId, DiscordMessageBuilder builder)
	{
		builder.Validate();

		if (builder.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed?.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var pld = new RestChannelMessageCreatePayload
		{
			HasContent = builder.Content != null,
			Content = builder.Content,
			StickersIds = builder.Sticker is null ? Array.Empty<ulong>() : new[] {builder.Sticker.Id},
			IsTts = builder.IsTts,
			HasEmbed = builder.Embeds != null,
			Embeds = builder.Embeds,
			Components = builder.Components
		};

		if (builder.ReplyId != null)
			pld.MessageReference = new InternalDiscordMessageReference { MessageId = builder.ReplyId, FailIfNotExists = builder.FailOnInvalidReply };

		pld.Mentions = new DiscordMentions(builder.Mentions ?? Mentions.All, builder.Mentions?.Any() ?? false, builder.MentionOnReply);

		if (builder.Files.Count == 0)
		{
			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
			var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));
			return ret;
		}
		else
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = new(builder.Files.Count);
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName
				};
				attachments.Add(att);
				fileId++;
			}
			pld.Attachments = attachments;

			var values = new Dictionary<string, string>
			{
				["payload_json"] = DiscordJson.SerializeObject(pld)
			};

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
			var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			foreach (var file in builder.FilesInternal.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}

			return ret;
		}
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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
	/// Creates the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="sendStartNotification">Whether everyone should be notified about the stage.</param>
	/// <param name="privacyLevel">The privacy_level.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channelId, string topic, bool sendStartNotification, StagePrivacyLevel privacyLevel, string reason)
	{
		var pld = new RestStageInstanceCreatePayload
		{
			ChannelId = channelId,
			Topic = topic,
			PrivacyLevel = privacyLevel,
			SendStartNotification = sendStartNotification
		};

		var route = $"{Endpoints.STAGE_INSTANCES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

		return stageInstance;
	}

	/// <summary>
	/// Gets the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<DiscordStageInstance> GetStageInstanceAsync(ulong channelId)
	{
		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

		return stageInstance;
	}

	/// <summary>
	/// Modifies the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="privacyLevel">The privacy_level.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyStageInstanceAsync(ulong channelId, Optional<string> topic, Optional<StagePrivacyLevel> privacyLevel, string reason)
	{
		var pld = new RestStageInstanceModifyPayload
		{
			Topic = topic,
			PrivacyLevel = privacyLevel
		};

		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {channel_id = channelId }, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Deletes the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteStageInstanceAsync(ulong channelId, string reason)
	{
		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId }, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Gets the channel messages async.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="limit">The limit.</param>
	/// <param name="before">The before.</param>
	/// <param name="after">The after.</param>
	/// <param name="around">The around.</param>

	internal async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channelId, int limit, ulong? before, ulong? after, ulong? around)
	{
		var urlParams = new Dictionary<string, string>();
		if (around != null)
			urlParams["around"] = around?.ToString(CultureInfo.InvariantCulture);
		if (before != null)
			urlParams["before"] = before?.ToString(CultureInfo.InvariantCulture);
		if (after != null)
			urlParams["after"] = after?.ToString(CultureInfo.InvariantCulture);
		if (limit > 0)
			urlParams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var msgsRaw = JArray.Parse(res.Response);
		var msgs = new List<DiscordMessage>();
		foreach (var xj in msgsRaw)
			msgs.Add(this.PrepareMessage(xj));

		return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
	}

	/// <summary>
	/// Gets the channel message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal async Task<DiscordMessage> GetChannelMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	/// Edits the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="content">The content.</param>
	/// <param name="embeds">The embeds.</param>
	/// <param name="mentions">The mentions.</param>
	/// <param name="components">The components.</param>
	/// <param name="suppressEmbed">The suppress_embed.</param>
	/// <param name="files">The files.</param>
	/// <param name="attachments">The attachments to keep.</param>

	internal async Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds, Optional<IEnumerable<IMention>> mentions, IReadOnlyList<DiscordActionRowComponent> components, Optional<bool> suppressEmbed, IReadOnlyCollection<DiscordMessageFile> files, Optional<IEnumerable<DiscordAttachment>> attachments)
	{
		if (embeds.HasValue && embeds.Value != null)
			foreach (var embed in embeds.Value)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var pld = new RestChannelMessageEditPayload
		{
			HasContent = content.HasValue,
			Content = content.ValueOrDefault(),
			HasEmbed = embeds.HasValue && (embeds.Value?.Any() ?? false),
			Embeds = embeds.HasValue && (embeds.Value?.Any() ?? false) ? embeds.Value : null,
			Components = components,
			Flags = suppressEmbed.HasValue && (bool)suppressEmbed ? MessageFlags.SuppressedEmbeds : null,
			Mentions = mentions
			.Map(m => new DiscordMentions(m ?? Mentions.None, false, mentions.Value?.OfType<RepliedUserMention>().Any() ?? false))
			.ValueOrDefault()
		};

		if (files?.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachmentsNew = new();
			foreach (var file in files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName
				};
				attachmentsNew.Add(att);
				fileId++;
			}
			if (attachments.HasValue && attachments.Value.Any())
				attachmentsNew.AddRange(attachments.Value);

			pld.Attachments = attachmentsNew;

			var values = new Dictionary<string, string>
			{
				["payload_json"] = DiscordJson.SerializeObject(pld)
			};

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {channel_id = channelId, message_id = messageId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: files).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			foreach (var file in files.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}

			return ret;
		}
		else
		{
			pld.Attachments = attachments.ValueOrDefault();

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {channel_id = channelId, message_id = messageId }, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			return ret;
		}
	}

	/// <summary>
	/// Deletes the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteMessageAsync(ulong channelId, ulong messageId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Deletes the messages async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageIds">The message_ids.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds, string reason)
	{
		var pld = new RestChannelMessageBulkDeletePayload
		{
			Messages = messageIds
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}{Endpoints.BULK_DELETE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Triggers the typing async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal Task TriggerTypingAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.TYPING}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
	}

	/// <summary>
	/// Gets the pinned messages async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var msgsRaw = JArray.Parse(res.Response);
		var msgs = new List<DiscordMessage>();
		foreach (var xj in msgsRaw)
			msgs.Add(this.PrepareMessage(xj));

		return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
	}

	/// <summary>
	/// Pins the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task PinMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
	}

	/// <summary>
	/// Unpins the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task UnpinMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId, message_id = messageId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
		ret.Discord = this.Discord;

		return ret;
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
		var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return JsonConvert.DeserializeObject<DiscordFollowedChannel>(response.Response);
	}

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
		var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route).ConfigureAwait(false);
		return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
	}

	#endregion

	#region Member
	/// <summary>
	/// Gets the current user async.
	/// </summary>

	internal Task<DiscordUser> GetCurrentUserAsync()
		=> this.GetUserAsync("@me");

	/// <summary>
	/// Gets the user async.
	/// </summary>
	/// <param name="userId">The user_id.</param>

	internal Task<DiscordUser> GetUserAsync(ulong userId)
		=> this.GetUserAsync(userId.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	/// Gets the user async.
	/// </summary>
	/// <param name="userId">The user_id.</param>

	internal async Task<DiscordUser> GetUserAsync(string userId)
	{
		var route = $"{Endpoints.USERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
		var duser = new DiscordUser(userRaw) { Discord = this.Discord };

		return duser;
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
	/// Modifies the current user async.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>

	internal async Task<TransportUser> ModifyCurrentUserAsync(string username, Optional<string> base64Avatar)
	{
		var pld = new RestUserUpdateCurrentPayload
		{
			Username = username,
			AvatarBase64 = base64Avatar.ValueOrDefault(),
			AvatarSet = base64Avatar.HasValue
		};

		var route = $"{Endpoints.USERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);

		return userRaw;
	}

	/// <summary>
	/// Gets the current user guilds async.
	/// </summary>
	/// <param name="limit">The limit.</param>
	/// <param name="before">The before.</param>
	/// <param name="after">The after.</param>

	internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}";

		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration)
			.AddParameter($"limit", limit.ToString(CultureInfo.InvariantCulture));

		if (before != null)
			url.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
		if (after != null)
			url.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));

		var res = await this.DoRequestAsync(this.Discord, bucket, url.Build(), RestRequestMethod.GET, route).ConfigureAwait(false);

		if (this.Discord is DiscordClient)
		{
			var guildsRaw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response);
			var glds = guildsRaw.Select(xug => (this.Discord as DiscordClient)?.GuildsInternal[xug.Id]);
			return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(glds));
		}
		else
		{
			return new ReadOnlyCollection<DiscordGuild>(JsonConvert.DeserializeObject<List<DiscordGuild>>(res.Response));
		}
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

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.ME}{Endpoints.NICK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
	}
	#endregion

	#region Roles
	/// <summary>
	/// Gets the guild roles async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var rolesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = this.Discord; xr.GuildId = guildId; return xr; });

		return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(rolesRaw));
	}

	/// <summary>
	/// Gets the guild async.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="withCounts">If true, with_counts.</param>

	internal async Task<DiscordGuild> GetGuildAsync(ulong guildId, bool? withCounts)
	{
		var urlParams = new Dictionary<string, string>();
		if (withCounts.HasValue)
			urlParams["with_counts"] = withCounts?.ToString();

		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Any() ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, urlParams).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawMembers = (JArray)json["members"];
		var guildRest = json.ToDiscordObject<DiscordGuild>();
		foreach (var r in guildRest.RolesInternal.Values)
			r.GuildId = guildRest.Id;

		if (this.Discord is DiscordClient dc)
		{
			await dc.OnGuildUpdateEventAsync(guildRest, rawMembers).ConfigureAwait(false);
			return dc.GuildsInternal[guildRest.Id];
		}
		else
		{
			guildRest.Discord = this.Discord;
			return guildRest;
		}
	}

	/// <summary>
	/// Modifies the guild role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="permissions">The permissions.</param>
	/// <param name="color">The color.</param>
	/// <param name="hoist">If true, hoist.</param>
	/// <param name="mentionable">If true, mentionable.</param>
	/// <param name="iconb64">The icon.</param>
	/// <param name="emoji">The unicode emoji icon.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong guildId, ulong roleId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, Optional<string> iconb64, Optional<string> emoji, string reason)
	{
		var pld = new RestGuildRolePayload
		{
			Name = name,
			Permissions = permissions & PermissionMethods.FullPerms,
			Color = color,
			Hoist = hoist,
			Mentionable = mentionable,
		};

		if (emoji.HasValue && !iconb64.HasValue)
			pld.UnicodeEmoji = emoji;

		if (emoji.HasValue && iconb64.HasValue)
		{
			pld.IconBase64 = null;
			pld.UnicodeEmoji = emoji;
		}

		if (iconb64.HasValue)
			pld.IconBase64 = iconb64;

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, role_id = roleId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
		ret.Discord = this.Discord;
		ret.GuildId = guildId;

		return ret;
	}

	/// <summary>
	/// Deletes the role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteRoleAsync(ulong guildId, ulong roleId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, role_id = roleId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Creates the guild role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="permissions">The permissions.</param>
	/// <param name="color">The color.</param>
	/// <param name="hoist">If true, hoist.</param>
	/// <param name="mentionable">If true, mentionable.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordRole> CreateGuildRoleAsync(ulong guildId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
	{
		var pld = new RestGuildRolePayload
		{
			Name = name,
			Permissions = permissions & PermissionMethods.FullPerms,
			Color = color,
			Hoist = hoist,
			Mentionable = mentionable
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
		ret.Discord = this.Discord;
		ret.GuildId = guildId;

		return ret;
	}
	#endregion

	#region Prune
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
	#endregion

	#region GuildVarious
	/// <summary>
	/// Gets the template async.
	/// </summary>
	/// <param name="code">The code.</param>

	internal async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
	{
		var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { code }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var templatesRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templatesRaw;
	}

	/// <summary>
	/// Gets the guild integrations async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var integrationsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

		return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrationsRaw));
	}

	/// <summary>
	/// Gets the guild preview async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PREVIEW}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordGuildPreview>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Creates the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="type">The type.</param>
	/// <param name="id">The id.</param>

	internal async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guildId, string type, ulong id)
	{
		var pld = new RestGuildIntegrationAttachPayload
		{
			Type = type,
			Id = id
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Modifies the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integrationId">The integration_id.</param>
	/// <param name="expireBehaviour">The expire_behaviour.</param>
	/// <param name="expireGracePeriod">The expire_grace_period.</param>
	/// <param name="enableEmoticons">If true, enable_emoticons.</param>

	internal async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guildId, ulong integrationId, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
	{
		var pld = new RestGuildIntegrationModifyPayload
		{
			ExpireBehavior = expireBehaviour,
			ExpireGracePeriod = expireGracePeriod,
			EnableEmoticons = enableEmoticons
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, integration_id = integrationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integration">The integration.</param>

	internal Task DeleteGuildIntegrationAsync(ulong guildId, DiscordIntegration integration)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, integration_id = integration.Id }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, payload: DiscordJson.SerializeObject(integration));
	}

	/// <summary>
	/// Syncs the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integrationId">The integration_id.</param>

	internal Task SyncGuildIntegrationAsync(ulong guildId, ulong integrationId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id{Endpoints.SYNC}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId, integration_id = integrationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
	}

	/// <summary>
	/// Gets the guild voice regions async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REGIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var regionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

		return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regionsRaw));
	}

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
	#endregion

	#region Invite
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
	#endregion

	#region Connections
	/// <summary>
	/// Gets the users connections async.
	/// </summary>

	internal async Task<IReadOnlyList<DiscordConnection>> GetUserConnectionsAsync()
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CONNECTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var connectionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

		return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connectionsRaw));
	}
	#endregion

	#region Voice
	/// <summary>
	/// Lists the voice regions async.
	/// </summary>

	internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
	{
		var route = $"{Endpoints.VOICE}{Endpoints.REGIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

		return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
	}
	#endregion

	#region Webhooks
	/// <summary>
	/// Creates the webhook async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, Optional<string> base64Avatar, string reason)
	{
		var pld = new RestWebhookPayload
		{
			Name = name,
			AvatarBase64 = base64Avatar.ValueOrDefault(),
			AvatarSet = base64Avatar.HasValue
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Gets the channel webhooks async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>

	internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

		return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
	}

	/// <summary>
	/// Gets the guild webhooks async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

		return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
	}

	/// <summary>
	/// Gets the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>

	internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhookId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {webhook_id = webhookId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Gets the webhook with token async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>

	internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhookId, string webhookToken)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Token = webhookToken;
		ret.Id = webhookId;
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Modifies the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, ulong channelId, string name, Optional<string> base64Avatar, string reason)
	{
		var pld = new RestWebhookPayload
		{
			Name = name,
			AvatarBase64 = base64Avatar.ValueOrDefault(),
			AvatarSet = base64Avatar.HasValue,
			ChannelId = channelId
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Modifies the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string webhookToken, string reason)
	{
		var pld = new RestWebhookPayload
		{
			Name = name,
			AvatarBase64 = base64Avatar
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
		ret.Discord = this.Discord;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	/// Deletes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteWebhookAsync(ulong webhookId, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {webhook_id = webhookId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Deletes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteWebhookAsync(ulong webhookId, string webhookToken, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Executes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> ExecuteWebhookAsync(ulong webhookId, string webhookToken, DiscordWebhookBuilder builder, string threadId)
	{
		builder.Validate();

		if (builder.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var values = new Dictionary<string, string>();
		var pld = new RestWebhookExecutePayload
		{
			Content = builder.Content,
			Username = builder.Username.ValueOrDefault(),
			AvatarUrl = builder.AvatarUrl.ValueOrDefault(),
			IsTts = builder.IsTts,
			Embeds = builder.Embeds,
			Components = builder.Components
		};

		if (builder.Mentions != null)
			pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

		if (builder.Files?.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = new();
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName,
					FileSize = null
				};
				attachments.Add(att);
				fileId++;
			}
			pld.Attachments = attachments;
		}

		if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.Files?.Count > 0 || builder.IsTts == true || builder.Mentions != null)
			values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);

		var url = qub.Build();

		var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

		foreach (var att in ret.Attachments)
			att.Discord = this.Discord;

		foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
		{
			file.Stream.Position = file.ResetPositionTo.Value;
		}

		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Executes the webhook slack async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="jsonPayload">The json_payload.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> ExecuteWebhookSlackAsync(ulong webhookId, string webhookToken, string jsonPayload, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.SLACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Executes the webhook github async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="jsonPayload">The json_payload.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> ExecuteWebhookGithubAsync(ulong webhookId, string webhookToken, string jsonPayload, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.GITHUB}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {webhook_id = webhookId, webhook_token = webhookToken }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Edits the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, DiscordWebhookBuilder builder, string threadId)
	{
		builder.Validate(true);

		var pld = new RestWebhookMessageEditPayload
		{
			Content = builder.Content,
			Embeds = builder.Embeds,
			Mentions = builder.Mentions,
			Components = builder.Components,
		};

		if (builder.Files?.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = new();
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName,
					FileSize = null
				};
				attachments.Add(att);
				fileId++;
			}
			if (builder.Attachments != null && builder.Attachments?.Count() > 0)
				attachments.AddRange(builder.Attachments);

			pld.Attachments = attachments;

			var values = new Dictionary<string, string>
			{
				["payload_json"] = DiscordJson.SerializeObject(pld)
			};
			var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

			var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
			if (threadId != null)
				qub.AddParameter("thread_id", threadId);

			var url = qub.Build();
			var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: builder.Files);

			var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

			ret.Discord = this.Discord;

			foreach (var att in ret.AttachmentsInternal)
				att.Discord = this.Discord;

			foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}

			return ret;
		}
		else
		{
			pld.Attachments = builder.Attachments;

			var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

			var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
			if (threadId != null)
				qub.AddParameter("thread_id", threadId);

			var url = qub.Build();
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

			var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

			ret.Discord = this.Discord;

			foreach (var att in ret.AttachmentsInternal)
				att.Discord = this.Discord;

			return ret;
		}
	}

	/// <summary>
	/// Edits the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>

	internal Task<DiscordMessage> EditWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, DiscordWebhookBuilder builder, ulong threadId) =>
		this.EditWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), builder, threadId.ToString());

	/// <summary>
	/// Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId) =>
		this.GetWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), null);

	/// <summary>
	/// Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, ulong threadId) =>
		this.GetWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), threadId.ToString());

	/// <summary>
	/// Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal async Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {webhook_id = webhookId, webhook_token = webhookToken, message_id = messageId }, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>

	internal Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId) =>
		this.DeleteWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), null);

	/// <summary>
	/// Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>

	internal Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, ulong threadId) =>
		this.DeleteWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), threadId.ToString());
	#endregion

	#region Reactions
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
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
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
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
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
	}
	#endregion

	#region Threads

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

	#endregion

	#region Emoji
	/// <summary>
	/// Gets the guild emojis async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);
		var users = new Dictionary<ulong, DiscordUser>();
		var emojis = new List<DiscordGuildEmoji>();
		foreach (var rawEmoji in emojisRaw)
		{
			var xge = rawEmoji.ToObject<DiscordGuildEmoji>();
			xge.Guild = gld;

			var xtu = rawEmoji["user"]?.ToObject<TransportUser>();
			if (xtu != null)
			{
				if (!users.ContainsKey(xtu.Id))
				{
					var user = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);
					users[user.Id] = user;
				}

				xge.User = users[xtu.Id];
			}

			emojis.Add(xge);
		}

		return new ReadOnlyCollection<DiscordGuildEmoji>(emojis);
	}

	/// <summary>
	/// Gets the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>

	internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guildId, ulong emojiId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu != null)
			emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

		return emoji;
	}

	/// <summary>
	/// Creates the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="imageb64">The imageb64.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guildId, string name, string imageb64, IEnumerable<ulong> roles, string reason)
	{
		var pld = new RestGuildEmojiCreatePayload
		{
			Name = name,
			ImageB64 = imageb64,
			Roles = roles?.ToArray()
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		emoji.User = xtu != null
			? gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu)
			: this.Discord.CurrentUser;

		return emoji;
	}

	/// <summary>
	/// Modifies the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guildId, ulong emojiId, string name, IEnumerable<ulong> roles, string reason)
	{
		var pld = new RestGuildEmojiModifyPayload
		{
			Name = name,
			Roles = roles?.ToArray()
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu != null)
			emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

		return emoji;
	}

	/// <summary>
	/// Deletes the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteGuildEmojiAsync(ulong guildId, ulong emojiId, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
	#endregion

	#region Stickers

	/// <summary>
	/// Gets a sticker.
	/// </summary>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetStickerAsync(ulong stickerId)
	{
		var route = $"{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Gets the sticker packs.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
	{
		var route = $"{Endpoints.STICKERPACKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response)["sticker_packs"] as JArray;
		var ret = json.ToDiscordObject<DiscordStickerPack[]>();

		return ret.ToList();
	}

	/// <summary>
	/// Gets the guild stickers.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<IReadOnlyList<DiscordSticker>> GetGuildStickersAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var json = JArray.Parse(res.Response);
		var ret = json.ToDiscordObject<DiscordSticker[]>();

		for (var i = 0; i < ret.Length; i++)
		{
			var stkr = ret[i];
			stkr.Discord = this.Discord;

			if (json[i]["user"] is JObject obj) // Null = Missing stickers perm //
			{
				var tsr = obj.ToDiscordObject<TransportUser>();
				var usr = new DiscordUser(tsr) {Discord = this.Discord};
				usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					return old;
				});
				stkr.User = usr;
			}
		}

		return ret.ToList();
	}

	/// <summary>
	/// Gets a guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetGuildStickerAsync(ulong guildId, ulong stickerId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var ret = json.ToDiscordObject<DiscordSticker>();
		if (json["user"] is not null) // Null = Missing stickers perm //
		{
			var tsr = json["user"].ToDiscordObject<TransportUser>();
			var usr = new DiscordUser(tsr) {Discord = this.Discord};
			usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				return old;
			});
			ret.User = usr;
		}
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Creates the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="file">The file.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> CreateGuildStickerAsync(ulong guildId, string name, string description, string tags, DiscordMessageFile file, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var res = await this.DoStickerMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, file, name, tags, description);

		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Modifies the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> ModifyGuildStickerAsync(ulong guildId, ulong stickerId, Optional<string> name, Optional<string> description, Optional<string> tags, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var pld = new RestStickerModifyPayload()
		{
			Name = name,
			Description = description,
			Tags = tags
		};

		var values = new Dictionary<string, string>
		{
			["payload_json"] = DiscordJson.SerializeObject(pld)
		};

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route);
		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();
		ret.Discord = this.Discord;

		return null;
	}

	/// <summary>
	/// Deletes the guild sticker async.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task DeleteGuildStickerAsync(ulong guildId, ulong stickerId, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, sticker_id = stickerId }, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
	#endregion

	#region Application Commands

	/// <summary>
	/// Gets the global application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(ulong applicationId, bool withLocalizations = false)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId }, out var path);

		var querydict = new Dictionary<string, string>
		{
			["with_localizations"] = withLocalizations.ToString().ToLower()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Bulk overwrites the global application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commands">The commands.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ulong applicationId, IEnumerable<DiscordApplicationCommand> commands)
	{
		var pld = new List<RestApplicationCommandCreatePayload>();
		foreach (var command in commands)
		{
			pld.Add(new RestApplicationCommandCreatePayload
			{
				Type = command.Type,
				Name = command.Name,
				Description = command.Description,
				Options = command.Options,
				NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
				DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
				DefaultMemberPermission = command.DefaultMemberPermissions,
				DmPermission = command.DmPermission,
				Nsfw = command.IsNsfw
			});
		}

		this.Discord.Logger.LogDebug(DiscordJson.SerializeObject(pld));

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {application_id = applicationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Creates a global application command.
	/// </summary>
	/// <param name="applicationId">The applicationid.</param>
	/// <param name="command">The command.</param>
	internal async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(ulong applicationId, DiscordApplicationCommand command)
	{
		var pld = new RestApplicationCommandCreatePayload
		{
			Type = command.Type,
			Name = command.Name,
			Description = command.Description,
			Options = command.Options,
			NameLocalizations = command.NameLocalizations.GetKeyValuePairs(),
			DescriptionLocalizations = command.DescriptionLocalizations.GetKeyValuePairs(),
			DefaultMemberPermission = command.DefaultMemberPermissions,
			DmPermission = command.DmPermission,
			Nsfw = command.IsNsfw
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {application_id = applicationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Gets a global application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong applicationId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Edits a global application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commandId">The command id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="options">The options.</param>
	/// <param name="nameLocalization">The localizations of the name.</param>
	/// <param name="descriptionLocalization">The localizations of the description.</param>
	/// <param name="defaultMemberPermission">The default member permissions.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	internal async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong applicationId, ulong commandId,
		Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options,
		Optional<DiscordApplicationCommandLocalization> nameLocalization, Optional<DiscordApplicationCommandLocalization> descriptionLocalization,
		Optional<Permissions> defaultMemberPermission, Optional<bool> dmPermission, Optional<bool> isNsfw)
	{
		var pld = new RestApplicationCommandEditPayload
		{
			Name = name,
			Description = description,
			Options = options,
			DefaultMemberPermission = defaultMemberPermission,
			DmPermission = dmPermission,
			NameLocalizations = nameLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault(),
			DescriptionLocalizations = descriptionLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault(),
			Nsfw = isNsfw
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {application_id = applicationId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes a global application command.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>
	/// <param name="commandId">The command_id.</param>

	internal async Task DeleteGlobalApplicationCommandAsync(ulong applicationId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {application_id = applicationId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	/// Gets the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, bool withLocalizations = false)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var querydict = new Dictionary<string, string>
		{
			["with_localizations"] = withLocalizations.ToString().ToLower()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Bulk overwrites the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commands">The commands.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, IEnumerable<DiscordApplicationCommand> commands)
	{
		var pld = new List<RestApplicationCommandCreatePayload>();
		foreach (var command in commands)
		{
			pld.Add(new RestApplicationCommandCreatePayload
			{
				Type = command.Type,
				Name = command.Name,
				Description = command.Description,
				Options = command.Options,
				NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
				DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
				DefaultMemberPermission = command.DefaultMemberPermissions,
				DmPermission = command.DmPermission,
				Nsfw = command.IsNsfw
			});
		}
		this.Discord.Logger.LogDebug(DiscordJson.SerializeObject(pld));

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
		foreach (var app in ret)
			app.Discord = this.Discord;
		return ret.ToList();
	}

	/// <summary>
	/// Creates a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="command">The command.</param>
	internal async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong applicationId, ulong guildId, DiscordApplicationCommand command)
	{
		var pld = new RestApplicationCommandCreatePayload
		{
			Type = command.Type,
			Name = command.Name,
			Description = command.Description,
			Options = command.Options,
			NameLocalizations = command.NameLocalizations.GetKeyValuePairs(),
			DescriptionLocalizations = command.DescriptionLocalizations.GetKeyValuePairs(),
			DefaultMemberPermission = command.DefaultMemberPermissions,
			DmPermission = command.DmPermission,
			Nsfw = command.IsNsfw
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Gets a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Edits a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="options">The options.</param>
	/// <param name="nameLocalization">The localizations of the name.</param>
	/// <param name="descriptionLocalization">The localizations of the description.</param>
	/// <param name="defaultMemberPermission">The default member permissions.</param>
	/// <param name="dmPermission">The dm permission.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	internal async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId,
		Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options,
		Optional<DiscordApplicationCommandLocalization> nameLocalization, Optional<DiscordApplicationCommandLocalization> descriptionLocalization,
		Optional<Permissions> defaultMemberPermission, Optional<bool> dmPermission, Optional<bool> isNsfw)
	{
		var pld = new RestApplicationCommandEditPayload
		{
			Name = name,
			Description = description,
			Options = options,
			DefaultMemberPermission = defaultMemberPermission,
			DmPermission = dmPermission,
			NameLocalizations = nameLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault(),
			DescriptionLocalizations = descriptionLocalization.Map(l => l.GetKeyValuePairs()).ValueOrDefault(),
			Nsfw = isNsfw
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task DeleteGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	#region Permissions 2.1

	/// <summary>
	/// Gets the guild application command permissions.
	/// </summary>
	/// <param name="applicationId">The target application id.</param>
	/// <param name="guildId">The target guild id.</param>
	internal async Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> GetGuildApplicationCommandPermissionsAsync(ulong applicationId, ulong guildId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}{Endpoints.PERMISSIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermission>>(res.Response);

		foreach (var app in ret)
			app.Discord = this.Discord;

		return ret.ToList();
	}

	/// <summary>
	/// Gets a guild application command permission.
	/// </summary>
	/// <param name="applicationId">The target application id.</param>
	/// <param name="guildId">The target guild id.</param>
	/// <param name="commandId">The target command id.</param>
	internal async Task<DiscordGuildApplicationCommandPermission> GetGuildApplicationCommandPermissionAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id{Endpoints.PERMISSIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId, guild_id = guildId, command_id = commandId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermission>(res.Response);

		ret.Discord = this.Discord;

		return ret;
	}

	#endregion

	/// <summary>
	/// Creates the interaction response.
	/// </summary>
	/// <param name="interactionId">The interaction id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="type">The type.</param>
	/// <param name="builder">The builder.</param>
	internal async Task CreateInteractionResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionResponseBuilder builder)
	{
		if (builder?.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		RestInteractionResponsePayload pld;

		if (type != InteractionResponseType.AutoCompleteResult)
		{
			var data = builder != null ? new DiscordInteractionApplicationCommandCallbackData
			{
				Content = builder.Content ?? null,
				Embeds = builder.Embeds ?? null,
				IsTts = builder.IsTts,
				Mentions = builder.Mentions ?? null,
				Flags = builder.IsEphemeral ? MessageFlags.Ephemeral : null,
				Components = builder.Components ?? null,
				Choices = null
			} : null;


			pld = new RestInteractionResponsePayload
			{
				Type = type,
				Data = data
			};


			if (builder != null && builder.Files != null && builder.Files.Count > 0)
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = new();
				foreach (var file in builder.Files)
				{
					DiscordAttachment att = new()
					{
						Id = fileId,
						Discord = this.Discord,
						Description = file.Description,
						FileName = file.FileName,
						FileSize = null
					};
					attachments.Add(att);
					fileId++;
				}
				pld.Attachments = attachments;
				pld.Data.Attachments = attachments;
			}
		}
		else
		{
			pld = new RestInteractionResponsePayload
			{
				Type = type,
				Data = new DiscordInteractionApplicationCommandCallbackData
				{
					Content = null,
					Embeds = null,
					IsTts = null,
					Mentions = null,
					Flags = null,
					Components = null,
					Choices = builder.Choices,
					Attachments = null
				},
				Attachments = null
			};
		}

		var values = new Dictionary<string, string>();

		if (builder != null)
			if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTts == true || builder.Mentions != null || builder.Files?.Count > 0)
				values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {interaction_id = interactionId, interaction_token = interactionToken }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "false").Build();
		if (builder != null)
		{
			await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files);

			foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
			{
				file.Stream.Position = file.ResetPositionTo.Value;
			}
		}
		else
		{
			await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
		}
	}

	/// <summary>
	/// Creates the interaction response.
	/// </summary>
	/// <param name="interactionId">The interaction id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="type">The type.</param>
	/// <param name="builder">The builder.</param>
	internal async Task CreateInteractionModalResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionModalBuilder builder)
	{
		if (builder.ModalComponents.Any(mc => mc.Components.Any(c => c.Type != Enums.ComponentType.InputText)))
			throw new NotSupportedException("Can't send any other type then Input Text as Modal Component.");

		var pld = new RestInteractionModalResponsePayload
		{
			Type = type,
			Data = new DiscordInteractionApplicationCommandModalCallbackData
			{
				Title = builder.Title,
				CustomId = builder.CustomId,
				ModalComponents = builder.ModalComponents
			}
		};

		var values = new Dictionary<string, string>();

		var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {interaction_id = interactionId, interaction_token = interactionToken }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Gets the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	internal Task<DiscordMessage> GetOriginalInteractionResponseAsync(ulong applicationId, string interactionToken) =>
		this.GetWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, null);

	/// <summary>
	/// Edits the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="builder">The builder.</param>
	internal Task<DiscordMessage> EditOriginalInteractionResponseAsync(ulong applicationId, string interactionToken, DiscordWebhookBuilder builder) =>
		this.EditWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, builder, null);

	/// <summary>
	/// Deletes the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	internal Task DeleteOriginalInteractionResponseAsync(ulong applicationId, string interactionToken) =>
		this.DeleteWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, null);

	/// <summary>
	/// Creates the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="builder">The builder.</param>
	internal async Task<DiscordMessage> CreateFollowupMessageAsync(ulong applicationId, string interactionToken, DiscordFollowupMessageBuilder builder)
	{
		builder.Validate();

		if (builder.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		var values = new Dictionary<string, string>();
		var pld = new RestFollowupMessageCreatePayload
		{
			Content = builder.Content,
			IsTts = builder.IsTts,
			Embeds = builder.Embeds,
			Flags = builder.Flags,
			Components = builder.Components
		};


		if (builder.Files != null && builder.Files.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = new();
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					FileName = file.FileName,
					FileSize = null
				};
				attachments.Add(att);
				fileId++;
			}
			pld.Attachments = attachments;
		}

		if (builder.Mentions != null)
			pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

		if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTts == true || builder.Mentions != null || builder.Files?.Count > 0)
			values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.WEBHOOKS}/:application_id/:interaction_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {application_id = applicationId, interaction_token = interactionToken }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
		var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
		var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

		foreach (var att in ret.AttachmentsInternal)
		{
			att.Discord = this.Discord;
		}

		foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
		{
			file.Stream.Position = file.ResetPositionTo.Value;
		}

		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Gets the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	internal Task<DiscordMessage> GetFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId) =>
		this.GetWebhookMessageAsync(applicationId, interactionToken, messageId);

	/// <summary>
	/// Edits the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	/// <param name="builder">The builder.</param>
	internal Task<DiscordMessage> EditFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId, DiscordWebhookBuilder builder) =>
		this.EditWebhookMessageAsync(applicationId, interactionToken, messageId.ToString(), builder, null);

	/// <summary>
	/// Deletes the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	internal Task DeleteFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId) =>
		this.DeleteWebhookMessageAsync(applicationId, interactionToken, messageId);
	#endregion

	#region Misc
	/// <summary>
	/// Gets the current application info async.
	/// </summary>

	internal Task<TransportApplication> GetCurrentApplicationInfoAsync()
		=> this.GetApplicationInfoAsync("@me");

	/// <summary>
	/// Gets the application info async.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>

	internal Task<TransportApplication> GetApplicationInfoAsync(ulong applicationId)
		=> this.GetApplicationInfoAsync(applicationId.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	/// Gets the application info async.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>

	private async Task<TransportApplication> GetApplicationInfoAsync(string applicationId)
	{
		var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {application_id = applicationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return JsonConvert.DeserializeObject<TransportApplication>(res.Response);
	}

	/// <summary>
	/// Gets the application assets async.
	/// </summary>
	/// <param name="application">The application.</param>

	internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
	{
		var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id{Endpoints.ASSETS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id = application.Id }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
		foreach (var asset in assets)
		{
			asset.Discord = application.Discord;
			asset.Application = application;
		}

		return new ReadOnlyCollection<DiscordApplicationAsset>(new List<DiscordApplicationAsset>(assets));
	}

	/// <summary>
	/// Gets the gateway info async.
	/// </summary>

	internal async Task<GatewayInfo> GetGatewayInfoAsync()
	{
		var headers = Utilities.GetBaseHeaders();
		var route = Endpoints.GATEWAY;
		if (this.Discord.Configuration.TokenType == TokenType.Bot)
			route += Endpoints.BOT;
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, headers).ConfigureAwait(false);

		var info = JObject.Parse(res.Response).ToObject<GatewayInfo>();
		info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.ResetAfterInternal);
		return info;
	}
	#endregion
}
