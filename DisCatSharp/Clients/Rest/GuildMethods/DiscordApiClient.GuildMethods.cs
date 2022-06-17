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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route, urlParams).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
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
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
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
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
		ret.Guild = this.Discord.Guilds[guildId];

		return ret;
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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route);

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
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

		var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
		return ret;
	}
}
