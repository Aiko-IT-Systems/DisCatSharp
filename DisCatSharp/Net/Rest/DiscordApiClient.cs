using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;
using DisCatSharp.Net.V2;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MessageType = DisCatSharp.Enums.MessageType;

namespace DisCatSharp.Net;

/// <summary>
///     Represents a discord api client.
/// </summary>
public sealed class DiscordApiClient
{
	/// <summary>
	///     The audit log reason header name.
	/// </summary>
	private const string REASON_HEADER_NAME = CommonHeaders.AUDIT_LOG_REASON_HEADER;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordApiClient" /> class.
	/// </summary>
	/// <param name="client">The base discord client.</param>
	internal DiscordApiClient(BaseDiscordClient client)
	{
		this.Discord = client;
		this.OAuth2Client = null!;
		this.Rest = new(client);
		this.RestV2 = new(client);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordApiClient" /> class.
	/// </summary>
	/// <param name="client">The oauth2 client.</param>
	/// <param name="proxy">The proxy.</param>
	/// <param name="timeout">The timeout.</param>
	/// <param name="useRelativeRateLimit">If true, use relative rate limit.</param>
	/// <param name="logger">The logger.</param>
	internal DiscordApiClient(DiscordOAuth2Client client, IWebProxy proxy, TimeSpan timeout, bool useRelativeRateLimit, ILogger logger)
	{
		this.OAuth2Client = client;
		this.Discord = null!;
		this.Rest = new(new()
		{
			Proxy = proxy,
			HttpTimeout = timeout,
			UseRelativeRatelimit = useRelativeRateLimit,
			ApiChannel = ApiChannel.Stable,
			ApiVersion = "10"
		}, logger);
		this.RestV2 = new(new()
		{
			Proxy = proxy,
			HttpTimeout = timeout,
			UseRelativeRatelimit = useRelativeRateLimit,
			ApiChannel = ApiChannel.Stable,
			ApiVersion = "10"
		}, logger);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordApiClient" /> class.
	/// </summary>
	/// <param name="proxy">The proxy.</param>
	/// <param name="timeout">The timeout.</param>
	/// <param name="useRelativeRateLimit">If true, use relative rate limit.</param>
	/// <param name="logger">The logger.</param>
	internal DiscordApiClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRateLimit, ILogger logger)
	{
		this.Discord = null!;
		this.OAuth2Client = null!;
		this.Rest = new(new()
		{
			Proxy = proxy,
			HttpTimeout = timeout,
			UseRelativeRatelimit = useRelativeRateLimit,
			ApiChannel = ApiChannel.Stable,
			ApiVersion = "10"
		}, logger);
		this.RestV2 = new(new()
		{
			Proxy = proxy,
			HttpTimeout = timeout,
			UseRelativeRatelimit = useRelativeRateLimit,
			ApiChannel = ApiChannel.Stable,
			ApiVersion = "10"
		}, logger);
	}

	/// <summary>
	///     Gets the discord client.
	/// </summary>
	internal BaseDiscordClient Discord { get; }

	/// <summary>
	///     Gets the oauth2 client.
	/// </summary>
	internal DiscordOAuth2Client OAuth2Client { get; }

	/// <summary>
	///     Gets the rest client.
	/// </summary>
	internal RestClient Rest { get; }

	internal RestClientV2 RestV2 { get; }

	/// <summary>
	///     Builds the query string.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <param name="post">Whether this query will be transmitted via POST.</param>
	private static string BuildQueryString(Dictionary<string, string> values, bool post = false)
	{
		if (values == null || values.Count == 0)
			return string.Empty;

		var valsCollection = values.Select(xkvp =>
			$"{WebUtility.UrlEncode(xkvp.Key)}={WebUtility.UrlEncode(xkvp.Value)}");
		var vals = string.Join("&", valsCollection);

		return !post ? $"?{vals}" : vals;
	}

	/// <summary>
	///     Prepares the message.
	/// </summary>
	/// <param name="msgRaw">The raw message.</param>
	private DiscordMessage PrepareMessage(JToken msgRaw)
	{
		var author = msgRaw["author"].ToObject<TransportUser>();
		var ret = msgRaw.ToDiscordObject<DiscordMessage>();
		ret.Discord = this.Discord;

		this.PopulateMessage(author, ret);

		var referencedMsg = msgRaw["referenced_message"];
		if (ret is { InternalReference.Type: ReferenceType.Default, MessageType: MessageType.Reply } && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
		{
			author = referencedMsg["author"].ToObject<TransportUser>();
			ret.ReferencedMessage.Discord = this.Discord;
			this.PopulateMessage(author, ret.ReferencedMessage);
		}

		if (ret.Channel != null)
		{
#pragma warning disable CS0472
			if (ret.ChannelId == null!)
				ret.ChannelId = ret.Channel.Id;
#pragma warning restore CS0472
			if (ret.Poll is not null)
			{
				ret.Poll.ChannelId = ret.ChannelId;
				ret.Poll.MessageId = ret.Id;
				ret.Poll.AuthorId = author.Id;
			}

			return ret;
		}

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
#pragma warning disable CS0472
		if (ret.ChannelId == null!)
			ret.ChannelId = ret.Channel.Id;
#pragma warning restore CS0472
		if (ret.Poll is not null)
		{
			ret.Poll.ChannelId = ret.ChannelId;
			ret.Poll.MessageId = ret.Id;
			ret.Poll.AuthorId = author.Id;
		}

		return ret;
	}

	/// <summary>
	///     Populates the message.
	/// </summary>
	/// <param name="author">The author.</param>
	/// <param name="ret">The message.</param>
	private void PopulateMessage(TransportUser author, DiscordMessage ret)
	{
		var guild = ret.Channel?.Guild;

		//If this is a webhook, it shouldn't be in the user cache.
		if (author.IsBot && int.Parse(author.Discriminator) == 0)
			ret.Author = new(author)
			{
				Discord = this.Discord
			};
		else
		{
			if (!this.Discord.UserCache.TryGetValue(author.Id, out var usr))
				this.Discord.UserCache[author.Id] = usr = new(author)
				{
					Discord = this.Discord
				};

			if (guild != null)
			{
				if (!guild.Members.TryGetValue(author.Id, out var mbr))
					mbr = new(usr)
					{
						Discord = this.Discord,
						GuildId = guild.Id
					};
				ret.Author = mbr;
			}
			else
				ret.Author = usr;
		}

		ret.PopulateMentions();

		ret.MessageSnapshots?.ForEach(x =>
		{
			x.Message.Discord = this.Discord;
			if (x.Message.MentionedUsersInternal.Count is not 0)
				x.Message.MentionedUsersInternal.ForEach(u => u.Discord = this.Discord);
			if (x.Message.AttachmentsInternal.Count is not 0)
				x.Message.AttachmentsInternal.ForEach(a => a.Discord = this.Discord);
			if (x.Message.EmbedsInternal.Count is not 0)
				x.Message.EmbedsInternal.ForEach(a => a.Discord = this.Discord);
			if (ret.Reference is { GuildId: not null })
				x.Message.GuildId = ret.Reference.GuildId;
			if (x.Message.MentionedChannelsInternal.Count is not 0)
				x.Message.MentionedChannelsInternal.ForEach(u => u.Discord = this.Discord);
			if (x.Message.MentionedRolesInternal.Count is not 0)
				x.Message.MentionedRolesInternal.ForEach(u => u.Discord = this.Discord);
			x.Message.PopulateMentions();
		});

		ret.ReactionsInternal ??= [];
		foreach (var xr in ret.ReactionsInternal)
			xr.Emoji.Discord = this.Discord;

		if (ret.Poll is not null)
			ret.Poll.Discord = this.Discord;

		if (ret.InteractionMetadata is not null)
			ret.InteractionMetadata.Discord = this.Discord;
	}

	/// <summary>
	///     Executes a rest request.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="payload">The payload.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	internal Task<RestResponse> DoRequestAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string>? headers = null, string? payload = null, double? ratelimitWaitOverride = null, bool targetDebug = false)
	{
		var req = new RestRequest(client, bucket, url, method, route, headers, payload, ratelimitWaitOverride);

		if (this.Discord is not null)
			this.Rest.ExecuteRequestAsync(req, targetDebug).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, $"Error while executing request. Url: {url.AbsoluteUri}");
		else
			_ = this.Rest.ExecuteRequestAsync(req, targetDebug);

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	///     Executes a rest form data request.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="formData">The form data.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	internal Task<RestResponse> DoFormRequestAsync(DiscordOAuth2Client client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, Dictionary<string, string> formData, Dictionary<string, string>? headers = null, double? ratelimitWaitOverride = null, bool targetDebug = false)
	{
		var req = new RestFormRequest(client, bucket, url, method, route, formData, headers, ratelimitWaitOverride);

		this.Rest.ExecuteRequestAsync(req, targetDebug).LogTaskFault(this.OAuth2Client.Logger, LogLevel.Error, LoggerEvents.RestError, $"Error while executing request. Url: {url.AbsoluteUri}");

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	///     Executes a multipart rest request for stickers.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="name">The sticker name.</param>
	/// <param name="tags">The sticker tag.</param>
	/// <param name="description">The sticker description.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="file">The file.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	private Task<RestResponse> DoStickerMultipartAsync(
		BaseDiscordClient client,
		RateLimitBucket bucket,
		Uri url,
		RestRequestMethod method,
		string route,
		string name,
		string tags,
		string? description = null,
		IReadOnlyDictionary<string, string>? headers = null,
		DiscordMessageFile? file = null,
		double? ratelimitWaitOverride = null,
		bool targetDebug = false
	)
	{
		var req = new MultipartStickerWebRequest(client, bucket, url, method, route, name, tags, description, headers, file, ratelimitWaitOverride);

		if (this.Discord is not null)
			this.Rest.ExecuteRequestAsync(req, targetDebug).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req, targetDebug);

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	///     Executes a multipart request.
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
	/// <param name="targetDebug">Enables a possible breakpoint in the rest client for debugging purposes.</param>
	private Task<RestResponse> DoMultipartAsync(
		BaseDiscordClient client,
		RateLimitBucket bucket,
		Uri url,
		RestRequestMethod method,
		string route,
		IReadOnlyDictionary<string, string>? headers = null,
		IReadOnlyDictionary<string, string>? values = null,
		IEnumerable<DiscordMessageFile>? files = null,
		double? ratelimitWaitOverride = null,
		bool targetDebug = false
	)
	{
		var req = new MultipartWebRequest(client, bucket, url, method, route, headers, values, files, ratelimitWaitOverride);

		if (this.Discord is not null)
			this.Rest.ExecuteRequestAsync(req, targetDebug).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
		else
			_ = this.Rest.ExecuteRequestAsync(req, targetDebug);

		return req.WaitForCompletionAsync();
	}

#region Voice

	/// <summary>
	///     Lists the voice regions async.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
	{
		var route = $"{Endpoints.VOICE}{Endpoints.REGIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var regions = DiscordJson.DeserializeIEnumerableObject<List<DiscordVoiceRegion>>(res.Response, this.Discord);

		return regions;
	}

#endregion

	// begin todo

#region Guild

	/// <summary>
	///     Gets the guild async.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="withCounts">If true, with_counts.</param>
	internal async Task<DiscordGuild> GetGuildAsync(ulong guildId, bool? withCounts)
	{
		var urlParams = new Dictionary<string, string>();
		if (withCounts.HasValue)
			urlParams["with_counts"] = withCounts.Value.ToString();

		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, urlParams).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawMembers = (JArray)json["members"];
		var guildRest = DiscordJson.DeserializeObject<DiscordGuild>(res.Response, this.Discord);
		foreach (var r in guildRest.RolesInternal.Values)
			r.GuildId = guildRest.Id;

		if (this.Discord is DiscordClient dc)
		{
			await dc.OnGuildUpdateEventAsync(guildRest, rawMembers).ConfigureAwait(false);
			return dc.GuildsInternal[guildRest.Id];
		}

		guildRest.Discord = this.Discord;
		return guildRest;
	}

	/// <summary>
	///     Searches the members async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="limit">The limit.</param>
	internal async Task<IReadOnlyList<DiscordMember>> SearchGuildMembersAsync(ulong guildId, string name, int? limit)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.SEARCH}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);
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
			var usr = new DiscordUser(xtm.User)
			{
				Discord = this.Discord
			};

			this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				old.BannerHash = usr.BannerHash;
				old.BannerColorInternal = usr.BannerColorInternal;
				old.AvatarDecorationData = usr.AvatarDecorationData;
				old.ThemeColorsInternal = usr.ThemeColorsInternal;
				old.Pronouns = usr.Pronouns;
				old.Locale = usr.Locale;
				old.GlobalName = usr.GlobalName;
				old.Clan = usr.Clan;
				old.PrimaryGuild = usr.PrimaryGuild;
				return old;
			});

			mbrs.Add(new(xtm)
			{
				Discord = this.Discord,
				GuildId = guildId
			});
		}

		return mbrs;
	}

	/// <summary>
	///     Gets the guild ban async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	internal async Task<DiscordBan> GetGuildBanAsync(ulong guildId, ulong userId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);
		var uri = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, uri, RestRequestMethod.GET, route).ConfigureAwait(false);
		var json = JObject.Parse(res.Response);

		var ban = json.ToObject<DiscordBan>();

		return ban;
	}

	/// <summary>
	///     Creates the guild async.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="regionId">The region_id.</param>
	/// <param name="iconb64">The iconb64.</param>
	/// <param name="verificationLevel">The verification_level.</param>
	/// <param name="defaultMessageNotifications">The default_message_notifications.</param>
	/// <param name="systemChannelFlags">The system_channel_flags.</param>
	internal async Task<DiscordGuild> CreateGuildAsync(
		string name,
		string regionId,
		Optional<string> iconb64,
		VerificationLevel? verificationLevel,
		DefaultMessageNotifications? defaultMessageNotifications,
		SystemChannelFlags? systemChannelFlags
	)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);

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
	///     Creates the guild from template async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			template_code = templateCode
		}, out var path);

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
	///     Deletes the guild async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task DeleteGuildAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

		if (this.Discord is DiscordClient dc)
		{
			var gld = dc.GuildsInternal[guildId];
			await dc.OnGuildDeleteEventAsync(gld).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Modifies the guild.
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
	/// <param name="homeHeaderb64">The home header base64.</param>
	/// <param name="preferredLocale">The preferred locale.</param>
	/// <param name="premiumProgressBarEnabled">Whether the premium progress bar should be enabled.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordGuild> ModifyGuildAsync(
		ulong guildId,
		Optional<string> name,
		Optional<VerificationLevel> verificationLevel,
		Optional<DefaultMessageNotifications> defaultMessageNotifications,
		Optional<MfaLevel> mfaLevel,
		Optional<ExplicitContentFilter> explicitContentFilter,
		Optional<ulong?> afkChannelId,
		Optional<int> afkTimeout,
		Optional<string> iconb64,
		Optional<ulong> ownerId,
		Optional<string> splashb64,
		Optional<ulong?> systemChannelId,
		Optional<SystemChannelFlags> systemChannelFlags,
		Optional<ulong?> publicUpdatesChannelId,
		Optional<ulong?> rulesChannelId,
		Optional<string> description,
		Optional<string> bannerb64,
		Optional<string> discoverySplashb64,
		Optional<string> homeHeaderb64,
		Optional<string> preferredLocale,
		Optional<bool> premiumProgressBarEnabled,
		string? reason
	)
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
			PremiumProgressBarEnabled = premiumProgressBarEnabled,
			HomeHeaderBase64 = homeHeaderb64
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

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
	///     Modifies the guild community settings.
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
	internal async Task<DiscordGuild> ModifyGuildCommunitySettingsAsync(ulong guildId, List<string> features, Optional<ulong?> rulesChannelId, Optional<ulong?> publicUpdatesChannelId, string preferredLocale, string description, DefaultMessageNotifications defaultMessageNotifications, ExplicitContentFilter explicitContentFilter, Optional<VerificationLevel> verificationLevel, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

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
	///     Modifies the guilds inventory settings.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="isEmojiPackCollectible">Whether emoji packs are collectible.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordGuild> ModifyGuildInventorySettingsAsync(ulong guildId, bool isEmojiPackCollectible, string? reason)
	{
		var pld = new RestGuildInventoryModifyPayload
		{
			IsEmojiPackCollectible = isEmojiPackCollectible
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INVENTORY}{Endpoints.SETTINGS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		this.Discord.Guilds[guildId].InventorySettings = new()
		{
			IsEmojiPackCollectible = isEmojiPackCollectible
		};
		return this.Discord.Guilds[guildId];
	}

	/// <summary>
	///     Modifies the guilds incident actions.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="invitesDisabledUntil">Until when invites are disabled. Set <see langword="null" /> to disable.</param>
	/// <param name="dmsDisabledUntil">Until when direct messages are disabled. Set <see langword="null" /> to disable.</param>
	internal async Task<IncidentsData> ModifyGuildIncidentActionsAsync(ulong guildId, DateTimeOffset? invitesDisabledUntil, DateTimeOffset? dmsDisabledUntil)
	{
		var pld = new RestGuildIncidentActionsModifyPayload
		{
			InvitesDisabledUntil = invitesDisabledUntil,
			DmsDisabledUntil = dmsDisabledUntil
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INCIDENT_ACTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var data = DiscordJson.DeserializeObject<IncidentsData>(res.Response, this.Discord);

		this.Discord.Guilds[guildId].IncidentsData = data;
		return data;
	}

	/// <summary>
	///     Gets the guilds onboarding.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<DiscordOnboarding> GetGuildOnboardingAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ONBOARDING}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var onboarding = DiscordJson.DeserializeObject<DiscordOnboarding>(res.Response, this.Discord);

		return onboarding;
	}

	/// <summary>
	///     Modifies the guilds onboarding.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="prompts">The onboarding prompts</param>
	/// <param name="defaultChannelIds">The default channel ids.</param>
	/// <param name="enabled">Whether onboarding is enabled.</param>
	/// <param name="mode">The onboarding mode.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordOnboarding> ModifyGuildOnboardingAsync(ulong guildId, Optional<List<DiscordOnboardingPrompt>> prompts, Optional<List<ulong>> defaultChannelIds, Optional<bool> enabled, Optional<OnboardingMode> mode, string? reason = null)
	{
		var pld = new RestGuildOnboardingModifyPayload
		{
			Prompts = prompts,
			DefaultChannelIds = defaultChannelIds,
			Enabled = enabled,
			Mode = mode
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ONBOARDING}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var onboarding = DiscordJson.DeserializeObject<DiscordOnboarding>(res.Response, this.Discord);

		return onboarding;
	}

	/// <summary>
	///     Gets the guilds server guide.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<DiscordServerGuide> GetGuildServerGuideAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.NEW_MEMBER_WELCOME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var guide = DiscordJson.DeserializeObject<DiscordServerGuide>(res.Response, this.Discord);

		return guide;
	}

	/// <summary>
	///     Modifies the guilds server guide.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="enabled">Whether the server guide is enabled.</param>
	/// <param name="welcomeMessage">The server guide welcome message.</param>
	/// <param name="newMemberActions">The new member actions.</param>
	/// <param name="resourceChannels">The resource channels.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordServerGuide> ModifyGuildServerGuideAsync(ulong guildId, Optional<bool> enabled, Optional<WelcomeMessage> welcomeMessage, Optional<List<NewMemberAction>> newMemberActions, Optional<List<ResourceChannel>> resourceChannels, string? reason = null)
	{
		var pld = new RestGuildServerGuideModifyPayload
		{
			Enabled = enabled,
			WelcomeMessage = welcomeMessage,
			NewMemberActions = newMemberActions,
			ResourceChannels = resourceChannels
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.NEW_MEMBER_WELCOME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var guide = DiscordJson.DeserializeObject<DiscordServerGuide>(res.Response, this.Discord);

		return guide;
	}

	/// <summary>
	///     Modifies the guild safety settings.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="features">The guild features.</param>
	/// <param name="safetyAlertsChannelId">The safety alerts channel id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordGuild> ModifyGuildSafetyAlertsSettingsAsync(ulong guildId, List<string> features, Optional<ulong?> safetyAlertsChannelId, string? reason)
	{
		var pld = new RestGuildSafetyModifyPayload
		{
			SafetyAlertsChannelId = safetyAlertsChannelId,
			Features = features
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

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
	///     Modifies the guild features.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="features">The guild features.</param>
	/// <param name="reason">The reason.</param>
	/// <returns></returns>
	internal async Task<DiscordGuild> ModifyGuildFeaturesAsync(ulong guildId, List<string> features, string? reason)
	{
		var pld = new RestGuildFeatureModifyPayload
		{
			Features = features
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

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
	///     Enables the guilds mfa requirement.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task EnableGuildMfaAsync(ulong guildId, string? reason)
	{
		var pld = new RestGuildMfaLevelModifyPayload
		{
			Level = MfaLevel.Enabled
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MFA}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	///     Disables the guilds mfa requirement.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task DisableGuildMfaAsync(ulong guildId, string? reason)
	{
		var pld = new RestGuildMfaLevelModifyPayload
		{
			Level = MfaLevel.Disabled
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MFA}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	///     Implements https://discord.com/developers/docs/resources/guild#get-guild-bans.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guildId, int? limit, ulong? before, ulong? after)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

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
				usr = new(xb.RawUser)
				{
					Discord = this.Discord
				};
				usr = this.Discord.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					old.GlobalName = usr.GlobalName;
					return old;
				});
			}

			xb.User = usr;
			return xb;
		});
		var bans = new ReadOnlyCollection<DiscordBan>([.. bansRaw]);

		return bans;
	}

	/// <summary>
	///     Creates a guild ban.
	/// </summary>
	/// <param name="guildId">The guild id to ban from.</param>
	/// <param name="userId">The user id to ban.</param>
	/// <param name="deleteMessageSeconds">The delete message seconds.</param>
	/// <param name="reason">The reason.</param>
	internal Task CreateGuildBanAsync(ulong guildId, ulong userId, int deleteMessageSeconds, string? reason)
	{
		if (deleteMessageSeconds < 0 || deleteMessageSeconds > 604800)
			throw new ArgumentException("Delete message seconds must be a number between 0 and 604800.", nameof(deleteMessageSeconds));

		var pld = new RestGuildBanPayload
		{
			DeleteMessageSeconds = deleteMessageSeconds
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Creates a guild bulk ban.
	/// </summary>
	/// <param name="guildId">The guild id to ban from.</param>
	/// <param name="userIds">The user ids to ban.</param>
	/// <param name="deleteMessageSeconds">The delete message seconds.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordBulkBanResponse> CreateGuildBulkBanAsync(ulong guildId, List<ulong> userIds, int deleteMessageSeconds, string? reason)
	{
		if (deleteMessageSeconds < 0 || deleteMessageSeconds > 604800)
			throw new ArgumentException("Delete message seconds must be a number between 0 and 604800.", nameof(deleteMessageSeconds));
		if (userIds.Count > 200)
			throw new ArgumentException("Can only bulk-ban up to 200 users.", nameof(userIds));

		var pld = new RestGuildBulkBanPayload
		{
			UserIds = [.. userIds],
			DeleteMessageSeconds = deleteMessageSeconds
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BULK_BAN}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
		return DiscordJson.DeserializeObject<DiscordBulkBanResponse>(response.Response, this.Discord);
	}

	/// <summary>
	///     Removes the guild ban async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task RemoveGuildBanAsync(ulong guildId, ulong userId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Leaves the guild async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal Task LeaveGuildAsync(ulong guildId)
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}/:guild_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	///     Adds the guild member async.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="userId">The user id.</param>
	/// <param name="accessToken">The user's access token.</param>
	/// <param name="nickname">The nickname.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="muted">If true, muted.</param>
	/// <param name="deafened">If true, deafened.</param>
	internal async Task<DiscordMember> AddGuildMemberAsync(ulong guildId, ulong userId, string accessToken, string? nickname = null, IEnumerable<DiscordRole>? roles = null, bool? muted = null, bool? deafened = null)
	{
		var pld = new RestGuildMemberAddPayload
		{
			AccessToken = accessToken,
			Nickname = nickname,
			Roles = roles,
			Deaf = deafened,
			Mute = muted
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var tm = DiscordJson.DeserializeObject<DiscordMember>(res.Response, this.Discord);
		tm.Discord = this.Discord;
		tm.GuildId = guildId;
		return tm;
	}

	/// <summary>
	///     Lists the guild members async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="limit">The limit.</param>
	/// <param name="after">The after.</param>
	internal async Task<IReadOnlyList<TransportMember>> ListGuildMembersAsync(ulong guildId, int? limit, ulong? after)
	{
		var urlParams = new Dictionary<string, string>();
		if (limit is > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
		if (after != null)
			urlParams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var membersRaw = JsonConvert.DeserializeObject<List<TransportMember>>(res.Response);
		return new ReadOnlyCollection<TransportMember>(membersRaw);
	}

	/// <summary>
	///     Adds the guild member role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task AddGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			guild_id = guildId,
			user_id = userId,
			role_id = roleId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
	}

	/// <summary>
	///     Removes the guild member role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task RemoveGuildMemberRoleAsync(ulong guildId, ulong userId, ulong roleId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			user_id = userId,
			role_id = roleId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Modifies the guild channel position async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyGuildChannelPositionAsync(ulong guildId, IEnumerable<RestGuildChannelReorderPayload> pld, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Modifies the guild channel parent async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyGuildChannelParentAsync(ulong guildId, IEnumerable<RestGuildChannelNewParentPayload> pld, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Detaches the guild channel parent async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>
	internal Task DetachGuildChannelParentAsync(ulong guildId, IEnumerable<RestGuildChannelNoParentPayload> pld, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Modifies the guild role position async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyGuildRolePositionAsync(ulong guildId, IEnumerable<RestGuildRoleReorderPayload> pld, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Gets the audit logs async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var auditLogDataRaw = DiscordJson.DeserializeObject<AuditLog>(res.Response, this.Discord);

		return auditLogDataRaw;
	}

	/// <summary>
	///     Gets the guild vanity url async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<DiscordInvite> GetGuildVanityUrlAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VANITY_URL}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var invite = DiscordJson.DeserializeObject<DiscordInvite>(res.Response, this.Discord);

		return invite;
	}

	/// <summary>
	///     Gets the guild widget async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<DiscordWidget> GetGuildWidgetAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET_JSON}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawChannels = (JArray)json["channels"];

		var ret = json.ToDiscordObject<DiscordWidget>();
		ret.Discord = this.Discord;
		ret.Guild = this.Discord.Guilds.ContainsKey(guildId) ? this.Discord.Guilds[guildId] : null;

		ret.Channels = ret.Guild == null
			?
			[
				.. rawChannels.Select(r => new DiscordChannel
				{
					Id = (ulong)r["id"],
					Name = r["name"].ToString(),
					Position = (int)r["position"]
				})
			]
			:
			[
				.. rawChannels.Select(r =>
				{
					var c = ret.Guild.GetChannel((ulong)r["id"]);
					c.Position = (int)r["position"];
					return c;
				})
			];

		return ret;
	}

	/// <summary>
	///     Gets the guild widget settings async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordWidgetSettings>(res.Response, this.Discord);
		ret.Guild = this.Discord.Guilds[guildId];

		return ret;
	}

	/// <summary>
	///     Modifies the guild widget settings async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="isEnabled">If true, is enabled.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guildId, bool? isEnabled, ulong? channelId, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordWidgetSettings>(res.Response, this.Discord);
		ret.Guild = this.Discord.Guilds[guildId];

		return ret;
	}

	/// <summary>
	///     Gets the guild templates async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var templatesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response);

		return new ReadOnlyCollection<DiscordGuildTemplate>([.. templatesRaw]);
	}

	/// <summary>
	///     Creates the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	internal async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guildId, string name, string? description)
	{
		var pld = new RestGuildTemplateCreateOrModifyPayload
		{
			Name = name,
			Description = description
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordGuildTemplate>(res.Response, this.Discord);

		return ret;
	}

	/// <summary>
	///     Syncs the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>
	internal async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guildId, string templateCode)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			guild_id = guildId,
			template_code = templateCode
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);

		var templateRaw = DiscordJson.DeserializeObject<DiscordGuildTemplate>(res.Response, this.Discord);

		return templateRaw;
	}

	/// <summary>
	///     Modifies the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	internal async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guildId, string templateCode, string? name, string? description)
	{
		var pld = new RestGuildTemplateCreateOrModifyPayload
		{
			Name = name,
			Description = description
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			template_code = templateCode
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var templateRaw = DiscordJson.DeserializeObject<DiscordGuildTemplate>(res.Response, this.Discord);

		return templateRaw;
	}

	/// <summary>
	///     Deletes the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>
	internal async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guildId, string templateCode)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			template_code = templateCode
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

		var templateRaw = DiscordJson.DeserializeObject<DiscordGuildTemplate>(res.Response, this.Discord);

		return templateRaw;
	}

	/// <summary>
	///     Gets the guild membership screening form async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var screeningRaw = DiscordJson.DeserializeObject<DiscordGuildMembershipScreening>(res.Response, this.Discord);

		return screeningRaw;
	}

	/// <summary>
	///     Modifies the guild membership screening form async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var screeningRaw = DiscordJson.DeserializeObject<DiscordGuildMembershipScreening>(res.Response, this.Discord);

		return screeningRaw;
	}

	/// <summary>
	///     Gets the guild welcome screen async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Modifies the guild welcome screen async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Gets the current user's voice state.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<DiscordVoiceState?> GetCurrentUserVoiceStateAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		try
		{
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
			return DiscordJson.DeserializeObject<DiscordVoiceState>(res.Response, this.Discord);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Gets the voice state for a member.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="memberId">The member id.</param>
	internal async Task<DiscordVoiceState?> GetMemberVoiceStateAsync(ulong guildId, ulong memberId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/:member_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			member_id = memberId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		try
		{
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
			return DiscordJson.DeserializeObject<DiscordVoiceState>(res.Response, this.Discord);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Updates the current user voice state async.
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

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	///     Updates the user voice state async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets all auto mod rules for a guild.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <returns>A collection of all auto mod rules in the guild.</returns>
	internal async Task<ReadOnlyCollection<AutomodRule>> GetAutomodRulesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id/auto-moderation/rules";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<List<AutomodRule>>(res.Response);
		foreach (var r in ret)
			r.Discord = this.Discord;
		return ret.AsReadOnly();
	}

	/// <summary>
	///     Gets a specific auto mod rule in the guild.
	/// </summary>
	/// <param name="guildId">The guild id for the rule.</param>
	/// <param name="ruleId">The rule id.</param>
	/// <returns>The rule if one is found.</returns>
	internal async Task<AutomodRule> GetAutomodRuleAsync(ulong guildId, ulong ruleId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id/auto-moderation/rules/:rule_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			rule_id = ruleId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<AutomodRule>(res.Response, this.Discord);
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	///     Creates an auto mod rule.
	/// </summary>
	/// <param name="guildId">The guild id of the rule.</param>
	/// <param name="name">The name of the rule.</param>
	/// <param name="eventType">The event type of the rule.</param>
	/// <param name="triggerType">The trigger type.</param>
	/// <param name="actions">The actions of the rule.</param>
	/// <param name="triggerMetadata">The metadata of the rule.</param>
	/// <param name="enabled">Whether this rule is enabled.</param>
	/// <param name="exemptRoles">The exempt roles of the rule.</param>
	/// <param name="exemptChannels">The exempt channels of the rule.</param>
	/// <param name="reason">The reason for this addition.</param>
	/// <returns>The new auto mod rule.</returns>
	internal async Task<AutomodRule> CreateAutomodRuleAsync(
		ulong guildId,
		string name,
		AutomodEventType eventType,
		AutomodTriggerType triggerType,
		IEnumerable<AutomodAction> actions,
		AutomodTriggerMetadata triggerMetadata = null,
		bool enabled = false,
		IEnumerable<ulong> exemptRoles = null,
		IEnumerable<ulong> exemptChannels = null,
		string? reason = null
	)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id/auto-moderation/rules";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		RestAutomodRuleModifyPayload pld = new()
		{
			Name = name,
			EventType = eventType,
			TriggerType = triggerType,
			Actions = actions.ToArray(),
			Enabled = enabled,
			TriggerMetadata = triggerMetadata ?? null
		};

		if (exemptChannels != null)
			pld.ExemptChannels = exemptChannels.ToArray();
		if (exemptRoles != null)
			pld.ExemptRoles = exemptRoles.ToArray();

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<AutomodRule>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		if (this.Discord is DiscordClient dc)
			await dc.OnAutomodRuleCreated(ret).ConfigureAwait(false);

		return ret;
	}

	/// <summary>
	///     Modifies an auto mod role
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="ruleId">The rule id.</param>
	/// <param name="name">The new name of the rule.</param>
	/// <param name="eventType">The new event type of the rule.</param>
	/// <param name="metadata">The new metadata of the rule.</param>
	/// <param name="actions">The new actions of the rule.</param>
	/// <param name="enabled">Whether this rule is enabled.</param>
	/// <param name="exemptRoles">The new exempt roles of the rule.</param>
	/// <param name="exemptChannels">The new exempt channels of the rule.</param>
	/// <param name="reason">The reason for this modification.</param>
	/// <returns>The updated automod rule</returns>
	internal async Task<AutomodRule> ModifyAutomodRuleAsync(
		ulong guildId,
		ulong ruleId,
		Optional<string> name,
		Optional<AutomodEventType> eventType,
		Optional<AutomodTriggerMetadata> metadata,
		Optional<List<AutomodAction>> actions,
		Optional<bool> enabled,
		Optional<List<ulong>> exemptRoles,
		Optional<List<ulong>> exemptChannels,
		string? reason = null
	)
	{
		var pld = new RestAutomodRuleModifyPayload
		{
			Name = name,
			EventType = eventType,
			TriggerMetadata = metadata,
			Enabled = enabled
		};

		if (actions.HasValue)
			pld.Actions = actions.Value?.ToArray();
		if (exemptChannels.HasValue)
			pld.ExemptChannels = exemptChannels.Value?.ToArray();
		if (exemptRoles.HasValue)
			pld.ExemptRoles = exemptRoles.Value?.ToArray();

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id/auto-moderation/rules/:rule_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			rule_id = ruleId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<AutomodRule>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		if (this.Discord is DiscordClient dc)
			await dc.OnAutomodRuleUpdated(ret).ConfigureAwait(false);

		return ret;
	}

	/// <summary>
	///     Deletes an auto mod rule.
	/// </summary>
	/// <param name="guildId">The guild id of the rule.</param>
	/// <param name="ruleId">The rule id.</param>
	/// <param name="reason">The reason for this deletion.</param>
	/// <returns>The deleted auto mod rule.</returns>
	internal async Task<AutomodRule> DeleteAutomodRuleAsync(ulong guildId, ulong ruleId, string? reason = null)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id/auto-moderation/rules/:rule_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			rule_id = ruleId
		}, out var path);

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<AutomodRule>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		if (this.Discord is DiscordClient dc)
			await dc.OnAutomodRuleDeleted(ret).ConfigureAwait(false);

		return ret;
	}

	/// <summary>
	///     Creates a new soundboard sound in the guild.
	/// </summary>
	/// <param name="guildId">The guild ID.</param>
	/// <param name="name">The name of the soundboard sound.</param>
	/// <param name="sound">The base64-encoded sound data (MP3 or OGG format).</param>
	/// <param name="volume">The volume of the soundboard sound (optional, defaults to 1).</param>
	/// <param name="emojiId">The ID of the custom emoji (optional).</param>
	/// <param name="emojiName">The unicode character of the standard emoji (optional).</param>
	/// <param name="reason">The reason.</param>
	public async Task<DiscordSoundboardSound> CreateGuildSoundboardSoundAsync(ulong guildId, string name, string sound, double? volume = null, ulong? emojiId = null, string? emojiName = null, string? reason = null)
	{
		var pld = new RestSoundboardSoundCreatePayload
		{
			Name = name,
			Sound = sound,
			Volume = volume,
			EmojiId = emojiId,
			EmojiName = emojiName
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SOUNDBOARD_SOUNDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordSoundboardSound>(res.Response, this.Discord);
	}

	/// <summary>
	///     Modifies an existing soundboard sound.
	/// </summary>
	/// <param name="guildId">The guild ID.</param>
	/// <param name="soundId">The soundboard sound ID.</param>
	/// <param name="name">The new name of the soundboard sound (optional).</param>
	/// <param name="volume">The new volume of the soundboard sound (optional).</param>
	/// <param name="emojiId">The new custom emoji ID (optional).</param>
	/// <param name="emojiName">The new standard emoji name (optional).</param>
	/// <param name="reason">The reason.</param>
	public async Task<DiscordSoundboardSound> ModifyGuildSoundboardSoundAsync(ulong guildId, ulong soundId, Optional<string> name, Optional<double?> volume, Optional<ulong?> emojiId, Optional<string?> emojiName, string? reason = null)
	{
		var pld = new RestSoundboardSoundModifyPayload
		{
			Name = name,
			Volume = volume,
			EmojiId = emojiId,
			EmojiName = emojiName
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SOUNDBOARD_SOUNDS}/:sound_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			sound_id = soundId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordSoundboardSound>(res.Response, this.Discord);
	}

	/// <summary>
	///     Deletes an existing soundboard sound.
	/// </summary>
	/// <param name="guildId">The guild ID.</param>
	/// <param name="soundId">The soundboard sound ID.</param>
	/// <param name="reason">The reason.</param>
	public async Task DeleteGuildSoundboardSoundAsync(ulong guildId, ulong soundId, string? reason = null)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SOUNDBOARD_SOUNDS}/:sound_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			sound_id = soundId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets all soundboard sounds for a guild.
	/// </summary>
	/// <param name="guildId">The guild ID.</param>
	public async Task<IReadOnlyList<DiscordSoundboardSound>> ListGuildSoundboardSoundsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SOUNDBOARD_SOUNDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeIEnumerableObject<List<DiscordSoundboardSound>>(res.Response, this.Discord);
	}

	/// <summary>
	///     Gets all default soundboard sounds available for all users.
	/// </summary>
	public async Task<IReadOnlyList<DiscordSoundboardSound>> ListDefaultSoundboardSoundsAsync()
	{
		var route = $"{Endpoints.SOUNDBOARD_DEFAULT_SOUNDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeIEnumerableObject<List<DiscordSoundboardSound>>(res.Response, this.Discord);
	}

	/// <summary>
	///     Gets a specific soundboard sound for a guild.
	/// </summary>
	/// <param name="guildId">The guild ID.</param>
	/// <param name="soundId">The soundboard sound ID.</param>
	public async Task<DiscordSoundboardSound> GetGuildSoundboardSoundAsync(ulong guildId, ulong soundId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SOUNDBOARD_SOUNDS}/:sound_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			sound_id = soundId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordSoundboardSound>(res.Response, this.Discord);
	}

	/// <summary>
	///     Sends a soundboard sound to a voice channel the user is connected to.
	/// </summary>
	/// <param name="channelId">The ID of the channel to send the sound to.</param>
	/// <param name="soundId">The ID of the soundboard sound to play.</param>
	/// <param name="sourceGuildId">
	///     The ID of the guild the soundboard sound is from, required to play sounds from different
	///     servers. Optional.
	/// </param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async Task SendSoundboardSoundAsync(ulong channelId, ulong soundId, ulong? sourceGuildId = null)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.SEND_SOUNDBOARD_SOUND}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var pld = new RestSendSoundboardSoundPayload
		{
			SoundId = soundId,
			SourceGuildId = sourceGuildId
		};

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

#endregion

	// End todo

#region Guild Scheduled Events

	/// <summary>
	///     Creates a scheduled event.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="metadata">The metadata.</param>
	/// <param name="name">The name.</param>
	/// <param name="scheduledStartTime">The scheduled start time.</param>
	/// <param name="scheduledEndTime">The scheduled end time.</param>
	/// <param name="description">The description.</param>
	/// <param name="type">The type.</param>
	/// <param name="coverb64">The cover image.</param>
	/// <param name="recurrenceRule">The recurrence rule.</param>
	/// <param name="reason">The reason.</param>
	/// <returns>A scheduled event.</returns>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	internal async Task<DiscordScheduledEvent> CreateGuildScheduledEventAsync(ulong guildId, ulong? channelId, DiscordScheduledEventEntityMetadata metadata, string name, DateTimeOffset scheduledStartTime, DateTimeOffset? scheduledEndTime, string description, ScheduledEventEntityType type, Optional<string> coverb64, DiscordScheduledEventRecurrenceRule? recurrenceRule, string? reason = null)
	{
		if (recurrenceRule is not null)
		{
			var (IsValid, ErrorMessage) = recurrenceRule.Validate();
			if (!IsValid)
				throw new ValidationException(
					typeof(DiscordScheduledEventRecurrenceRule),
					"DiscordGuild.CreateScheduledEventAsync or DiscordGuild.CreateExternalScheduledEventAsync",
					ErrorMessage!
				);
		}

		var pld = new RestGuildScheduledEventCreatePayload
		{
			ChannelId = channelId,
			EntityMetadata = metadata,
			Name = name,
			ScheduledStartTime = scheduledStartTime,
			ScheduledEndTime = scheduledEndTime,
			Description = description,
			EntityType = type,
			CoverBase64 = coverb64,
			RecurrenceRule = recurrenceRule
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var scheduledEvent = DiscordJson.DeserializeObject<DiscordScheduledEvent>(res.Response, this.Discord);
		var guild = this.Discord.Guilds[guildId];

		scheduledEvent.Discord = this.Discord;

		if (scheduledEvent.Creator != null)
			scheduledEvent.Creator.Discord = this.Discord;

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildScheduledEventCreateEventAsync(scheduledEvent, guild).ConfigureAwait(false);

		return scheduledEvent;
	}

	/// <summary>
	///     Modifies a scheduled event.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="scheduledEventId">The scheduled event id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="metadata">The metadata.</param>
	/// <param name="name">The name.</param>
	/// <param name="scheduledStartTime">The scheduled start time.</param>
	/// <param name="scheduledEndTime">The scheduled end time.</param>
	/// <param name="description">The description.</param>
	/// <param name="type">The type.</param>
	/// <param name="status">The status.</param>
	/// <param name="coverb64">The cover image.</param>
	/// <param name="recurrenceRule">The recurrence rule.</param>
	/// <param name="reason">The reason.</param>
	/// <returns>A scheduled event.</returns>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	internal async Task<DiscordScheduledEvent> ModifyGuildScheduledEventAsync(
		ulong guildId,
		ulong scheduledEventId,
		Optional<ulong?> channelId,
		Optional<DiscordScheduledEventEntityMetadata> metadata,
		Optional<string> name,
		Optional<DateTimeOffset> scheduledStartTime,
		Optional<DateTimeOffset> scheduledEndTime,
		Optional<string> description,
		Optional<ScheduledEventEntityType> type,
		Optional<ScheduledEventStatus> status,
		Optional<string?> coverb64,
		Optional<DiscordScheduledEventRecurrenceRule?> recurrenceRule,
		string? reason = null
	)
	{
		if (recurrenceRule.HasValue && recurrenceRule.Value is not null)
		{
			var (IsValid, ErrorMessage) = recurrenceRule.Value.Validate();
			if (!IsValid)
				throw new ValidationException(
					typeof(DiscordScheduledEventRecurrenceRule),
					"DiscordScheduledEvent.ModifyAsync(Action<ScheduledEventEditModel> action)",
					ErrorMessage!
				);
		}

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
			CoverBase64 = coverb64,
			RecurrenceRule = recurrenceRule
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			scheduled_event_id = scheduledEventId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var scheduledEvent = DiscordJson.DeserializeObject<DiscordScheduledEvent>(res.Response, this.Discord);
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
				old.GlobalName = scheduledEvent.Creator.GlobalName;
				return old;
			});
		}

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildScheduledEventUpdateEventAsync(scheduledEvent, guild).ConfigureAwait(false);

		return scheduledEvent;
	}

	/// <summary>
	///     Modifies a scheduled event.
	/// </summary>
	internal async Task<DiscordScheduledEvent> ModifyGuildScheduledEventStatusAsync(ulong guildId, ulong scheduledEventId, ScheduledEventStatus status, string? reason = null)
	{
		var pld = new RestGuildScheduledEventModifyPayload
		{
			Status = status
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			scheduled_event_id = scheduledEventId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var scheduledEvent = DiscordJson.DeserializeObject<DiscordScheduledEvent>(res.Response, this.Discord);
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
				old.GlobalName = scheduledEvent.Creator.GlobalName;
				return old;
			});
		}

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildScheduledEventUpdateEventAsync(scheduledEvent, guild).ConfigureAwait(false);

		return scheduledEvent;
	}

	/// <summary>
	///     Gets a scheduled event.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			scheduled_event_id = scheduledEventId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var scheduledEvent = DiscordJson.DeserializeObject<DiscordScheduledEvent>(res.Response, this.Discord);
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
				old.GlobalName = scheduledEvent.Creator.GlobalName;
				return old;
			});
		}

		return scheduledEvent;
	}

	/// <summary>
	///     Gets the guilds scheduled events.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="withUserCount">Whether to include the count of users subscribed to the scheduled event.</param>
	internal async Task<IReadOnlyDictionary<ulong, DiscordScheduledEvent>> ListGuildScheduledEventsAsync(ulong guildId, bool? withUserCount)
	{
		var urlParams = new Dictionary<string, string>();
		if (withUserCount.HasValue)
			urlParams["with_user_count"] = withUserCount?.ToString();

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

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
	///     Deletes a guild scheduled event.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="scheduledEventId">The scheduled event id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteGuildScheduledEventAsync(ulong guildId, ulong scheduledEventId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			scheduled_event_id = scheduledEventId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Gets the users who RSVP'd to a scheduled event.
	///     Optional with member objects.
	///     This endpoint is paginated.
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
		if (limit is > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (after != null)
			urlParams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);
		if (withMember != null)
			urlParams["with_member"] = withMember.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.SCHEDULED_EVENTS}/:scheduled_event_id{Endpoints.USERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			scheduled_event_id = scheduledEventId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var rspvUsers = JsonConvert.DeserializeObject<IEnumerable<DiscordScheduledEventUser>>(res.Response);
		Dictionary<ulong, DiscordScheduledEventUser> rspv = [];

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

	// begin todo

#region Channel

	/// <summary>
	///     Creates a guild channel.
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
	/// <param name="flags">The channel flags.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordChannel> CreateGuildChannelAsync(ulong guildId, string name, ChannelType type, ulong? parent, Optional<string> topic, int? bitrate, int? userLimit, IEnumerable<DiscordOverwriteBuilder>? overwrites, bool? nsfw, Optional<int?> perUserRateLimit, VideoQualityMode? qualityMode, ThreadAutoArchiveDuration? defaultAutoArchiveDuration, Optional<ChannelFlags?> flags, string? reason)
	{
		var restOverwrites = new List<DiscordRestOverwrite>();
		if (overwrites != null)
			restOverwrites.AddRange(overwrites.Select(ow => ow.Build()));

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
			DefaultAutoArchiveDuration = defaultAutoArchiveDuration,
			Flags = flags
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordChannel>(res.Response, this.Discord);
		ret.Initialize(this.Discord);

		return ret;
	}

	/// <summary>
	///     Creates a guild forum channel.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="parent">The parent.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="template">The template.</param>
	/// <param name="defaultReactionEmoji">The default reaction emoji.</param>
	/// <param name="permissionOverwrites">The overwrites.</param>
	/// <param name="nsfw">If true, nsfw.</param>
	/// <param name="perUserRateLimit">The per user rate limit.</param>
	/// <param name="postCreateUserRateLimit">The per user post create rate limit.</param>
	/// <param name="defaultAutoArchiveDuration">The default auto archive duration.</param>
	/// <param name="defaultSortOrder">The default sort order.</param>
	/// <param name="defaultLayout">The default layout.</param>
	/// <param name="flags">The channel flags.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordChannel> CreateGuildForumChannelAsync(
		ulong guildId,
		string name,
		ulong? parent,
		Optional<string> topic,
		Optional<string> template,
		bool? nsfw,
		Optional<ForumReactionEmoji> defaultReactionEmoji,
		Optional<int?> perUserRateLimit,
		Optional<int?> postCreateUserRateLimit,
		Optional<ForumPostSortOrder> defaultSortOrder,
		Optional<ForumLayout?> defaultLayout,
		ThreadAutoArchiveDuration? defaultAutoArchiveDuration,
		IEnumerable<DiscordOverwriteBuilder>? permissionOverwrites,
		Optional<ChannelFlags?> flags,
		string? reason
	)
	{
		List<DiscordRestOverwrite> restoverwrites = null;
		if (permissionOverwrites != null)
		{
			restoverwrites = [];
			restoverwrites.AddRange(permissionOverwrites.Select(ow => ow.Build()));
		}

		var pld = new RestChannelCreatePayload
		{
			Name = name,
			Topic = topic,
			//Template = template,
			Nsfw = nsfw,
			Parent = parent,
			PerUserRateLimit = perUserRateLimit,
			PostCreateUserRateLimit = postCreateUserRateLimit,
			DefaultAutoArchiveDuration = defaultAutoArchiveDuration,
			DefaultReactionEmoji = defaultReactionEmoji,
			PermissionOverwrites = restoverwrites,
			DefaultSortOrder = defaultSortOrder,
			Flags = flags,
			DefaultForumLayout = defaultLayout,
			Type = ChannelType.Forum
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordChannel>(res.Response, this.Discord);
		ret.Initialize(this.Discord);

		return ret;
	}

	/// <summary>
	///     Modifies the channel async.
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
	/// <param name="flags">The channel flags.</param>
	/// <param name="forumLayout">The default forum layout.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyChannelAsync(
		ulong channelId,
		string name,
		int? position,
		Optional<string> topic,
		bool? nsfw,
		Optional<ulong?> parent,
		Optional<int?> bitrate,
		Optional<int?> userLimit,
		Optional<int?> perUserRateLimit,
		Optional<string> rtcRegion,
		VideoQualityMode? qualityMode,
		ForumLayout? forumLayout,
		ThreadAutoArchiveDuration? autoArchiveDuration,
		Optional<ChannelType> type,
		IEnumerable<DiscordOverwriteBuilder> permissionOverwrites,
		Optional<ChannelFlags?> flags,
		string? reason
	)
	{
		List<DiscordRestOverwrite> restoverwrites = null;
		if (permissionOverwrites != null)
		{
			restoverwrites = [];
			restoverwrites.AddRange(permissionOverwrites.Select(ow => ow.Build()));
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
			Flags = flags,
			PermissionOverwrites = restoverwrites,
			ForumLayout = forumLayout
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Modifies the forum channel.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="name">The name.</param>
	/// <param name="parent">The parent.</param>
	/// <param name="position">The position.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="template">The template.</param>
	/// <param name="defaultReactionEmoji">The default reaction emoji.</param>
	/// <param name="permissionOverwrites">The overwrites.</param>
	/// <param name="nsfw">If true, nsfw.</param>
	/// <param name="availableTags">The available tags.</param>
	/// <param name="defaultSortOrder">The default sort order.</param>
	/// <param name="perUserRateLimit">The per user rate limit.</param>
	/// <param name="postCreateUserRateLimit">The per user post create rate limit.</param>
	/// <param name="defaultAutoArchiveDuration">The default auto archive duration.</param>
	/// <param name="flags">The channel flags.</param>
	/// <param name="reason">The reason.</param>
	/// <param name="forumLayout"></param>
	internal async Task<DiscordChannel> ModifyForumChannelAsync(
		ulong channelId,
		string name,
		int? position,
		Optional<string> topic,
		Optional<string> template,
		bool? nsfw,
		Optional<ulong?> parent,
		Optional<List<ForumPostTag>?> availableTags,
		Optional<ForumReactionEmoji> defaultReactionEmoji,
		Optional<int?> perUserRateLimit,
		Optional<int?> postCreateUserRateLimit,
		Optional<ForumPostSortOrder?> defaultSortOrder,
		Optional<ForumLayout?> forumLayout,
		Optional<ThreadAutoArchiveDuration?> defaultAutoArchiveDuration,
		IEnumerable<DiscordOverwriteBuilder> permissionOverwrites,
		Optional<ChannelFlags?> flags,
		string? reason
	)
	{
		List<DiscordRestOverwrite> restoverwrites = null;
		if (permissionOverwrites != null)
		{
			restoverwrites = [];
			restoverwrites.AddRange(permissionOverwrites.Select(ow => ow.Build()));
		}

		var pld = new RestChannelModifyPayload
		{
			Name = name,
			Position = position,
			Topic = topic,
			//Template = template,
			Nsfw = nsfw,
			Parent = parent,
			PerUserRateLimit = perUserRateLimit,
			PostCreateUserRateLimit = postCreateUserRateLimit,
			DefaultAutoArchiveDuration = defaultAutoArchiveDuration,
			DefaultReactionEmoji = defaultReactionEmoji,
			PermissionOverwrites = restoverwrites,
			DefaultSortOrder = defaultSortOrder,
			Flags = flags,
			AvailableTags = availableTags,
			ForumLayout = forumLayout
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
		var ret = DiscordJson.DeserializeObject<DiscordChannel>(res.Response, this.Discord);
		ret.Initialize(this.Discord);

		return ret;
	}

	/// <summary>
	///     Gets the channel async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<DiscordChannel> GetChannelAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordChannel>(res.Response, this.Discord);
		ret.Initialize(this.Discord);

		return ret;
	}

	/// <summary>
	///     Deletes the channel async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteChannelAsync(ulong channelId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Gets the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	internal async Task<DiscordMessage> GetMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	///     Creates the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="content">The content.</param>
	/// <param name="embeds">The embeds.</param>
	/// <param name="sticker">The sticker.</param>
	/// <param name="replyMessageId">The reply message id.</param>
	/// <param name="mentionReply">If true, mention reply.</param>
	/// <param name="failOnInvalidReply">If true, fail on invalid reply.</param>
	/// <param name="components">The components.</param>
	/// <exception cref="ArgumentException">
	///     Thrown when the <paramref name="content" /> exceeds 2000 characters or is empty and
	///     if neither content, sticker, components and embeds are definied..
	/// </exception>
	internal async Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content, IEnumerable<DiscordEmbed> embeds, DiscordSticker sticker, ulong? replyMessageId, bool mentionReply, bool failOnInvalidReply, ReadOnlyCollection<DiscordComponent>? components = null)
	{
		if (content is { Length: > 2000 })
			throw new ArgumentException("Message content length cannot exceed 2000 characters.");

		if (!embeds?.Any() ?? true)
		{
			if (content == null && sticker == null && components == null)
				throw new ArgumentException("You must specify message content, a sticker, components or embeds.");
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
			StickersIds = sticker is null
				? Array.Empty<ulong>()
				: [sticker.Id],
			IsTts = false,
			HasEmbed = embeds?.Any() ?? false,
			Embeds = embeds,
			Components = components
		};

		if (replyMessageId != null)
			pld.MessageReference = new InternalDiscordMessageReference
			{
				Type = ReferenceType.Default,
				MessageId = replyMessageId,
				FailIfNotExists = failOnInvalidReply
			};

		if (replyMessageId != null)
			pld.Mentions = new(Mentions.All, true, mentionReply);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	internal async Task<DiscordMessage> CreateMessageV2Async(ulong channelId, string content, IEnumerable<DiscordEmbed> embeds, DiscordSticker sticker, ulong? replyMessageId, bool mentionReply, bool failOnInvalidReply, ReadOnlyCollection<DiscordComponent>? components = null)
	{
		if (content is { Length: > 2000 })
			throw new ArgumentException("Message content length cannot exceed 2000 characters.");

		if (!embeds?.Any() ?? true)
		{
			if (content == null && sticker == null && components == null)
				throw new ArgumentException("You must specify message content, a sticker, components or embeds.");
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
			StickersIds = sticker is null
				? Array.Empty<ulong>()
				: [sticker.Id],
			IsTts = false,
			HasEmbed = embeds?.Any() ?? false,
			Embeds = embeds,
			Components = components
		};

		if (replyMessageId != null)
			pld.MessageReference = new InternalDiscordMessageReference
			{
				Type = ReferenceType.Default,
				MessageId = replyMessageId,
				FailIfNotExists = failOnInvalidReply
			};

		if (replyMessageId != null)
			pld.Mentions = new(Mentions.All, true, mentionReply);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
		var bucket = this.RestV2.GetBucketV2(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestV2Async(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	internal Task<RestResponseV2> DoRequestV2Async(BaseDiscordClient client, RateLimitBucketV2 bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string>? headers = null, string? payload = null, double? ratelimitWaitOverride = null)
	{
		var req = new RestRequestV2(client, bucket, url, method, route, headers, payload, ratelimitWaitOverride);

		if (this.Discord is not null)
			this.RestV2.ExecuteRequestV2Async(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, $"Error while executing request. Url: {url.AbsoluteUri}");
		else
			_ = this.RestV2.ExecuteRequestV2Async(req);

		return req.WaitForCompletionAsync();
	}

	/// <summary>
	///     Creates the message async.
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

		var flags = MessageFlags.None;
		if (builder.EmbedsSuppressed)
			flags |= MessageFlags.SuppressedEmbeds;
		if (builder.Silent)
			flags |= MessageFlags.SuppressNotifications;
		if (builder.IsVoiceMessage)
			flags |= MessageFlags.IsVoiceMessage;
		if (builder.IsComponentsV2)
			flags |= MessageFlags.IsComponentsV2;

		var pld = new RestChannelMessageCreatePayload
		{
			HasContent = builder.Content != null,
			Content = builder.Content,
			StickersIds = builder.Sticker is null
				? Array.Empty<ulong>()
				: [builder.Sticker.Id],
			IsTts = builder.IsTts,
			HasEmbed = builder.Embeds != null,
			Embeds = builder.Embeds,
			Components = builder.Components,
			Nonce = builder.Nonce,
			EnforceNonce = builder.EnforceNonce,
			DiscordPollRequest = builder.Poll?.Build(),
			Flags = flags
		};

		if (builder.ReplyId != null)
			pld.MessageReference = new InternalDiscordMessageReference
			{
				Type = ReferenceType.Default,
				MessageId = builder.ReplyId,
				FailIfNotExists = builder.FailOnInvalidReply
			};

		pld.Mentions = new(builder.Mentions.Count == 0 ? Mentions.All : builder.Mentions, builder.Mentions.Any(), builder.MentionOnReply);

		if (builder.Files.Count == 0)
		{
			if (builder.Attachments.Any())
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = new(builder.Attachments.Count);
				foreach (var att in builder.Attachments)
				{
					att.Id = fileId;
					attachments.Add(att);
					fileId++;
				}

				pld.Attachments = attachments;
			}

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
			var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{
				channel_id = channelId
			}, out var path);

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
					Filename = file.Filename
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
			var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{
				channel_id = channelId
			}, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			foreach (var file in builder.FilesInternal.Where(x => x.ResetPositionTo.HasValue))
				file.Stream.Position = file.ResetPositionTo!.Value;

			return ret;
		}
	}

	/// <summary>
	///     Forwards a message.
	/// </summary>
	/// <param name="forwardMessage">The message to forward.</param>
	/// <param name="targetChannelId">The target channel id to forward the message to.</param>
	/// <param name="content">The content to attach.</param>
	/// <exception cref="ArgumentException">Thrown when the <paramref name="content" /> exceeds 2000 characters.</exception>
	public async Task<DiscordMessage> ForwardMessageAsync(DiscordMessage forwardMessage, ulong targetChannelId, string? content)
	{
		if (content is { Length: > 2000 })
			throw new ArgumentException("Message content length cannot exceed 2000 characters.");

		var pld = new RestChannelMessageCreatePayload
		{
			//HasContent = content != null,
			//Content = content,
			MessageReference = new InternalDiscordMessageReference
			{
				Type = ReferenceType.Forward,
				MessageId = forwardMessage.Id,
				ChannelId = forwardMessage.ChannelId,
				GuildId = forwardMessage.GuildId
			}
		};

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = targetChannelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	///     Gets the guild channels async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var channelsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(xc =>
		{
			xc.Discord = this.Discord;
			return xc;
		});

		foreach (var ret in channelsRaw)
			ret.Initialize(this.Discord);

		return new ReadOnlyCollection<DiscordChannel>([.. channelsRaw]);
	}

	/// <summary>
	///     Modifies a voice channels status.
	/// </summary>
	/// <param name="channelId">The voice channel id.</param>
	/// <param name="status">The status.</param>
	internal Task ModifyVoiceChannelStatusAsync(ulong channelId, string? status)
	{
		var pld = new RestVoiceChannelStatusModifyPayload
		{
			Status = status
		};

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.VOICE_STATUS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Creates the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="sendStartNotification">Whether everyone should be notified about the stage.</param>
	/// <param name="scheduledEventId">The associated scheduled event id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channelId, string topic, bool sendStartNotification, ulong? scheduledEventId = null, string? reason = null)
	{
		var pld = new RestStageInstanceCreatePayload
		{
			ChannelId = channelId,
			Topic = topic,
			ScheduledEventId = scheduledEventId,
			SendStartNotification = sendStartNotification
		};

		var route = $"{Endpoints.STAGE_INSTANCES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var stageInstance = DiscordJson.DeserializeObject<DiscordStageInstance>(res.Response, this.Discord);

		return stageInstance;
	}

	/// <summary>
	///     Gets the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<DiscordStageInstance> GetStageInstanceAsync(ulong channelId)
	{
		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var stageInstance = DiscordJson.DeserializeObject<DiscordStageInstance>(res.Response, this.Discord);

		return stageInstance;
	}

	/// <summary>
	///     Modifies the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyStageInstanceAsync(ulong channelId, Optional<string> topic, string? reason)
	{
		var pld = new RestStageInstanceModifyPayload
		{
			Topic = topic
		};

		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			channel_id = channelId
		}, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Deletes the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteStageInstanceAsync(ulong channelId, string? reason)
	{
		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId
		}, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Gets the channel messages async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var msgsRaw = JArray.Parse(res.Response);
		var msgs = new List<DiscordMessage>();
		foreach (var xj in msgsRaw)
			msgs.Add(this.PrepareMessage(xj));

		return new ReadOnlyCollection<DiscordMessage>([.. msgs]);
	}

	/// <summary>
	///     Gets the channel message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	internal async Task<DiscordMessage> GetChannelMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = this.PrepareMessage(JObject.Parse(res.Response));

		return ret;
	}

	/// <summary>
	///     Edits the message async.
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
	internal async Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds, Optional<IEnumerable<IMention>> mentions, IReadOnlyList<DiscordComponent> components, Optional<bool> suppressEmbed, IReadOnlyCollection<DiscordMessageFile> files, Optional<IEnumerable<DiscordAttachment>> attachments)
	{
		if (embeds is { HasValue: true, Value: not null })
			foreach (var embed in embeds.Value)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		MessageFlags? flags = suppressEmbed.HasValue && suppressEmbed.Value ? MessageFlags.SuppressedEmbeds : null;

		var pld = new RestChannelMessageEditPayload
		{
			HasContent = content.HasValue,
			Content = content.ValueOrDefault(),
			HasEmbed = embeds.HasValue && (embeds.Value?.Any() ?? false),
			Embeds = embeds.HasValue && (embeds.Value?.Any() ?? false) ? embeds.Value : null,
			Components = components,
			Flags = flags,
			Mentions = mentions
				.Map(m => new DiscordMentions(m ?? Mentions.None, false, mentions.Value?.OfType<RepliedUserMention>().Any() ?? false))
				.ValueOrDefault()
		};

		if (files?.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachmentsNew = [];
			foreach (var file in files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					Filename = file.Filename
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
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
			{
				channel_id = channelId,
				message_id = messageId
			}, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: files).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			foreach (var file in files.Where(x => x.ResetPositionTo.HasValue))
				file.Stream.Position = file.ResetPositionTo.Value;

			return ret;
		}
		else
		{
			if (attachments.HasValue && attachments.Value.Any())
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachmentsNew = new(attachments.Value.Count());
				foreach (var att in attachments.Value)
				{
					att.Id = fileId;
					attachmentsNew.Add(att);
					fileId++;
				}

				pld.Attachments = attachmentsNew;
			}

			var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
			{
				channel_id = channelId,
				message_id = messageId
			}, out var path);

			var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

			var ret = this.PrepareMessage(JObject.Parse(res.Response));

			return ret;
		}
	}

	/// <summary>
	///     Deletes the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteMessageAsync(ulong channelId, ulong messageId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Deletes the messages async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageIds">The message_ids.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds, string? reason)
	{
		var pld = new RestChannelMessageBulkDeletePayload
		{
			Messages = messageIds
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}{Endpoints.BULK_DELETE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));
	}

#region Polls

	/// <summary>
	///     Get a list of users that voted for a specific answer on a <see cref="DiscordPoll" />.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="messageId">The message id containing the poll.</param>
	/// <param name="answerId">The answer id.</param>
	/// <param name="limit">The max number of users to return (<c>1</c>-<c>100</c>). Defaults to <c>25</c>.</param>
	/// <param name="after">Get users after this user ID.</param>
	/// <returns>
	///     A <see cref="ReadOnlyCollection{T}" /> of <see cref="DiscordUser" />s who voted for the given
	///     <paramref name="answerId" /> on the <see cref="DiscordPoll" />.
	/// </returns>
	internal async Task<ReadOnlyCollection<DiscordUser>> GetAnswerVotersAsync(ulong channelId, ulong messageId, int answerId, int? limit, ulong? after)
	{
		var urlParams = new Dictionary<string, string>();
		if (after != null)
			urlParams["after"] = after?.ToString(CultureInfo.InvariantCulture);
		if (limit > 0)
			urlParams["limit"] = limit?.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.POLLS}/:message_id{Endpoints.ANSWERS}/:answer_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId,
			message_id = messageId,
			answer_id = answerId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

		var votersRaw = JObject.Parse(res.Response);
		var votersRawList = (JArray)votersRaw["users"]!;
		var voters = new List<DiscordUser>();
		foreach (var voterRaw in votersRawList)
		{
			var xr = voterRaw.ToDiscordObject<TransportUser>();
			var usr = new DiscordUser(xr)
			{
				Discord = this.Discord
			};
			usr = this.Discord.UserCache.AddOrUpdate(xr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				old.GlobalName = usr.GlobalName;
				return old;
			});

			voters.Add(usr);
		}

		return new([.. voters]);
	}

	/// <summary>
	///     Immediately ends a poll. Only for own polls.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="messageId">The message id containing the poll.</param>
	/// <returns>The <see cref="DiscordMessage" /> containing the <see cref="DiscordPoll" />.</returns>
	internal async Task<DiscordMessage> EndPollAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.POLLS}/:message_id{Endpoints.EXPIRE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
		return DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);
	}

#endregion

	/// <summary>
	///     Gets the channel invites async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi =>
		{
			xi.Discord = this.Discord;
			return xi;
		});

		return new ReadOnlyCollection<DiscordInvite>([.. invitesRaw]);
	}

	/// <summary>
	///     Creates the channel invite async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="maxAge">The max_age.</param>
	/// <param name="maxUses">The max_uses.</param>
	/// <param name="targetType">The target_type.</param>
	/// <param name="targetApplicationId">The target_application.</param>
	/// <param name="targetUser">The target_user.</param>
	/// <param name="temporary">If true, temporary.</param>
	/// <param name="unique">If true, unique.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordInvite> CreateChannelInviteAsync(ulong channelId, int maxAge, int maxUses, TargetType? targetType, ulong? targetApplicationId, ulong? targetUser, bool temporary, bool unique, string? reason)
	{
		var pld = new RestChannelInviteCreatePayload
		{
			MaxAge = maxAge,
			MaxUses = maxUses,
			TargetType = targetType,
			TargetApplicationId = targetApplicationId,
			TargetUserId = targetUser,
			Temporary = temporary,
			Unique = unique
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordInvite>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	///     Deletes the channel permission async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="overwriteId">The overwrite_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteChannelPermissionAsync(ulong channelId, ulong overwriteId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			overwrite_id = overwriteId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Edits the channel permissions async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="overwriteId">The overwrite_id.</param>
	/// <param name="allow">The allow.</param>
	/// <param name="deny">The deny.</param>
	/// <param name="type">The type.</param>
	/// <param name="reason">The reason.</param>
	internal Task EditChannelPermissionsAsync(ulong channelId, ulong overwriteId, Permissions allow, Permissions deny, string type, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			channel_id = channelId,
			overwrite_id = overwriteId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Triggers the typing async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal Task TriggerTypingAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.TYPING}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
	}

	/// <summary>
	///     Gets the pinned messages async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var msgsRaw = JArray.Parse(res.Response);
		var msgs = new List<DiscordMessage>();
		foreach (var xj in msgsRaw)
			msgs.Add(this.PrepareMessage(xj));

		return new ReadOnlyCollection<DiscordMessage>([.. msgs]);
	}

	/// <summary>
	///     Pins the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	internal Task PinMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
	}

	/// <summary>
	///     Unpins the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	internal Task UnpinMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	///     Adds the group dm recipient async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			channel_id = channelId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Removes the group dm recipient async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="userId">The user_id.</param>
	internal Task RemoveGroupDmRecipientAsync(ulong channelId, ulong userId)
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	///     Creates the group dm async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordDmChannel>(res.Response, this.Discord);

		return ret;
	}

	/// <summary>
	///     Creates the dm async.
	/// </summary>
	/// <param name="recipientId">The recipient_id.</param>
	internal async Task<DiscordDmChannel> CreateDmAsync(ulong recipientId)
	{
		var pld = new RestUserDmCreatePayload
		{
			Recipient = recipientId
		};

		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordDmChannel>(res.Response, this.Discord);

		return ret;
	}

	/// <summary>
	///     Follows the channel async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordFollowedChannel>(response.Response, this.Discord);
	}

	/// <summary>
	///     Crossposts the message async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	internal async Task<DiscordMessage> CrosspostMessageAsync(ulong channelId, ulong messageId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.CROSSPOST}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route).ConfigureAwait(false);
		return DiscordJson.DeserializeObject<DiscordMessage>(response.Response, this.Discord);
	}

#endregion

	// End todo

#region Member

	/// <summary>
	///     Gets the current user async.
	/// </summary>
	internal Task<DiscordUser> GetCurrentUserAsync()
		=> this.GetUserAsync(Endpoints.ME);

	/// <summary>
	///     Gets the user async.
	/// </summary>
	/// <param name="userId">The user_id.</param>
	internal Task<DiscordUser> GetUserAsync(ulong userId)
		=> this.GetUserAsync(userId.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	///     Gets the user async.
	/// </summary>
	/// <param name="userId">The user_id.</param>
	internal async Task<DiscordUser> GetUserAsync(string userId)
	{
		var route = $"{Endpoints.USERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path.Replace("//", "/"), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var duser = DiscordJson.DeserializeObject<DiscordUser>(res.Response, this.Discord);
		if (this.Discord.Configuration.Intents.HasIntent(DiscordIntents.GuildPresences) && duser.Presence == null && this.Discord is DiscordClient dc)
			dc.PresencesInternal[duser.Id] = new()
			{
				Discord = dc,
				RawActivity = new(),
				Activity = new(),
				Status = UserStatus.Offline,
				InternalUser = new()
				{
					Id = duser.Id
				}
			};
		return duser;
	}

	/// <summary>
	///     Gets the guild member async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	internal async Task<DiscordMember> GetGuildMemberAsync(ulong guildId, ulong userId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var tm = DiscordJson.DeserializeObject<TransportMember>(res.Response, this.Discord);

		var usr = new DiscordUser(tm.User)
		{
			Discord = this.Discord
		};
		usr = this.Discord.UserCache.AddOrUpdate(tm.User.Id, usr, (id, old) =>
		{
			old.Username = usr.Username;
			old.Discriminator = usr.Discriminator;
			old.AvatarHash = usr.AvatarHash;
			old.GlobalName = usr.GlobalName;
			return old;
		});

		if (this.Discord.Configuration.Intents.HasIntent(DiscordIntents.GuildPresences) && usr.Presence == null && this.Discord is DiscordClient dc)
			dc.PresencesInternal[usr.Id] = new()
			{
				Discord = dc,
				RawActivity = new(),
				Activity = new(),
				Status = UserStatus.Offline
			};

		return new(tm)
		{
			Discord = this.Discord,
			GuildId = guildId
		};
	}

	/// <summary>
	///     Removes the guild member async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task RemoveGuildMemberAsync(ulong guildId, ulong userId, string? reason)
	{
		var urlParams = new Dictionary<string, string>();
		if (reason != null)
			urlParams["reason"] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, BuildQueryString(urlParams), this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

	/// <summary>
	///     Modifies the current user async.
	/// </summary>
	/// <param name="username">The new username.</param>
	/// <param name="base64Avatar">The new avatar.</param>
	/// <param name="base64Banner">The new banner.</param>
	internal async Task<DiscordUser> ModifyCurrentUserAsync(string username, Optional<string?> base64Avatar, Optional<string?> base64Banner)
	{
		var pld = new RestUserUpdateCurrentPayload
		{
			Username = username,
			AvatarBase64 = base64Avatar,
			AvatarSet = base64Avatar is { HasValue: true, Value: not null },
			BannerBase64 = base64Banner,
			BannerSet = base64Banner is { HasValue: true, Value: not null }
		};

		var route = $"{Endpoints.USERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var user = DiscordJson.DeserializeObject<DiscordUser>(res.Response, this.Discord);
		return user;
	}

	/// <summary>
	///     Gets the current user guilds async.
	/// </summary>
	/// <param name="limit">The limit.</param>
	/// <param name="before">The before.</param>
	/// <param name="after">The after.</param>
	internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}";

		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration)
			.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

		if (before != null)
			url.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
		if (after != null)
			url.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));

		var res = await this.DoRequestAsync(this.Discord, bucket, url.Build(), RestRequestMethod.GET, route).ConfigureAwait(false);

		if (this.Discord is DiscordClient)
		{
			var guildsRaw = DiscordJson.DeserializeIEnumerableObject<IEnumerable<RestUserGuild>>(res.Response, this.Discord);
			var glds = guildsRaw.Select(xug => (this.Discord as DiscordClient)?.GuildsInternal[xug.Id]);
			return new ReadOnlyCollection<DiscordGuild>([.. glds]);
		}

		return DiscordJson.DeserializeIEnumerableObject<List<DiscordGuild>>(res.Response, this.Discord);
	}

	/// <summary>
	///     Modifies the guild member async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="nick">The nick.</param>
	/// <param name="roleIds">The role_ids.</param>
	/// <param name="mute">The mute.</param>
	/// <param name="deaf">The deaf.</param>
	/// <param name="voiceChannelId">The voice_channel_id.</param>
	/// <param name="verify">Whether to verify the member.</param>
	/// <param name="flags">The member flags</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyGuildMemberAsync(
		ulong guildId,
		ulong userId,
		Optional<string> nick,
		Optional<IEnumerable<ulong>> roleIds,
		Optional<bool> mute,
		Optional<bool> deaf,
		Optional<ulong?> voiceChannelId,
		Optional<bool> verify,
		MemberFlags flags,
		string? reason
	)
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
			VoiceChannelId = voiceChannelId,
			Flags = verify is { HasValue: true, Value: true }
				? flags | MemberFlags.BypassesVerification
				: verify is { HasValue: true, Value: false }
					? flags & ~MemberFlags.BypassesVerification
					: Optional.None
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Modifies the time out of a guild member.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="until">Datetime offset.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyTimeoutAsync(ulong guildId, ulong userId, DateTimeOffset? until, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var pld = new RestGuildMemberTimeoutModifyPayload
		{
			CommunicationDisabledUntil = until
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	///     Modifies the current member nickname async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="nick">The nick.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyCurrentMemberNicknameAsync(ulong guildId, string nick, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var pld = new RestGuildMemberModifyPayload
		{
			Nickname = nick
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.ME}{Endpoints.NICK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

#endregion

#region Roles

	/// <summary>
	///     Gets the guild roles async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var roles = DiscordJson.DeserializeIEnumerableObject<IEnumerable<DiscordRole>>(res.Response, this.Discord).Select(xr =>
		{
			xr.GuildId = guildId;
			return xr;
		}).ToList();

		return roles;
	}

	/// <summary>
	///     Gets a guild role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="roleId">The role_id.</param>
	internal async Task<DiscordRole> GetGuildRoleAsync(ulong guildId, ulong roleId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			role_id = roleId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordRole>(res.Response, this.Discord);
		ret.GuildId = guildId;

		return ret;
	}

	/// <summary>
	///     Modifies the guild role async.
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
	internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong guildId, ulong roleId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, Optional<string> iconb64, Optional<string> emoji, string? reason)
	{
		var pld = new RestGuildRolePayload
		{
			Name = name,
			Permissions = permissions & PermissionMethods.FullPerms,
			Color = color,
			Hoist = hoist,
			Mentionable = mentionable
		};

		if (emoji.HasValue && iconb64.HasValue)
		{
			pld.IconBase64 = null;
			pld.UnicodeEmoji = emoji;
		}
		else if (emoji.HasValue && !iconb64.HasValue)
			pld.UnicodeEmoji = emoji;
		else if (iconb64.HasValue && !emoji.HasValue)
			pld.IconBase64 = iconb64;

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			role_id = roleId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordRole>(res.Response, this.Discord);
		ret.GuildId = guildId;

		return ret;
	}

	/// <summary>
	///     Deletes the role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteRoleAsync(ulong guildId, ulong roleId, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			role_id = roleId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Creates the guild role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="permissions">The permissions.</param>
	/// <param name="color">The color.</param>
	/// <param name="hoist">If true, hoist.</param>
	/// <param name="mentionable">If true, mentionable.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordRole> CreateGuildRoleAsync(ulong guildId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordRole>(res.Response, this.Discord);
		ret.GuildId = guildId;

		return ret;
	}

#endregion

#region Guild Member Applications

	/// <summary>
	///     Gets the guild join requests.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="limit">The maximum number of join requests to return. Defaults to 100.</param>
	/// <param name="statusType">The status type to filter join requests by. Can be Submitted, Approved, or Rejected.</param>
	/// <param name="before">Retrieve join requests before this ID.</param>
	/// <param name="after">Retrieve join requests after this ID.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the status type is not supported.</exception>
	internal async Task<DiscordGuildJoinRequestSearchResult> GetGuildJoinRequestsAsync(ulong guildId, int limit = 100, JoinRequestStatusType? statusType = null, ulong? before = null, ulong? after = null)
	{
		var urlParams = new Dictionary<string, string>
		{
			["limit"] = limit.ToString(CultureInfo.InvariantCulture)
		};

		if (before.HasValue)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (after.HasValue)
			urlParams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);

		if (statusType.HasValue)
			urlParams["status"] = statusType.Value switch
			{
				JoinRequestStatusType.Submitted or JoinRequestStatusType.Approved or JoinRequestStatusType.Rejected => statusType.Value.ToString().ToUpperInvariant(),
				_ => throw new ArgumentOutOfRangeException(nameof(statusType), $"Status type {statusType.Value} is not supported")
			};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REQUESTS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var searchResults = DiscordJson.DeserializeObject<DiscordGuildJoinRequestSearchResult>(res.Response, this.Discord);
		foreach (var joinRequest in searchResults.JoinRequests)
			joinRequest.Discord = this.Discord;

		return searchResults;
	}

	/// <summary>
	///     Gets a specific guild join request.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="joinRequestId">The ID of the join request.</param>
	internal async Task<DiscordGuildJoinRequest> GetGuildJoinRequestAsync(ulong guildId, ulong joinRequestId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REQUESTS}/:join_request_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			join_request_id = joinRequestId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordGuildJoinRequest>(res.Response, this.Discord);
	}

	/// <summary>
	///     Modifies a guild join request.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="joinRequestId">The ID of the join request.</param>
	/// <param name="statusType">The status type to set for the join request. Can be Approved or Rejected.</param>
	/// <param name="rejectionReason">The optional rejection reason.</param>
	/// <exception cref="InvalidOperationException">Thrown when the status type is not Approved or Rejected.</exception>
	internal async Task<DiscordGuildJoinRequest> ModifyGuildJoinRequestsAsync(ulong guildId, ulong joinRequestId, JoinRequestStatusType statusType, string? rejectionReason)
	{
		var pld = new RestGuildJoinRequestUpdatePayload
		{
			Action = statusType is JoinRequestStatusType.Approved or JoinRequestStatusType.Rejected ? statusType.ToString().ToUpperInvariant() : throw new InvalidOperationException($"Can not {statusType} as action for join request {joinRequestId}"),
			RejectionReason = statusType is JoinRequestStatusType.Rejected ? rejectionReason : null
		};
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REQUESTS}/:join_request_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordGuildJoinRequest>(res.Response, this.Discord);
	}

	/// <summary>
	///     Gets the settings for a clan.
	/// </summary>
	/// <param name="clanId">The ID of the clan.</param>
	internal async Task<DiscordClanSettings> GetClanSettingsAsync(ulong clanId)
	{
		var route = $"{Endpoints.CLANS}/:clan_id{Endpoints.SETTINGS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			clan_id = clanId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordClanSettings>(res.Response, this.Discord);
	}

#endregion

#region Prune

	/// <summary>
	///     Gets the guild prune count async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlParams)}{sb}", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var pruned = DiscordJson.DeserializeObject<RestGuildPruneResultPayload>(res.Response, this.Discord);

		return pruned.Pruned.Value;
	}

	/// <summary>
	///     Begins the guild prune async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="days">The days.</param>
	/// <param name="computePruneCount">If true, compute_prune_count.</param>
	/// <param name="includeRoles">The include_roles.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<int?> BeginGuildPruneAsync(ulong guildId, int days, bool computePruneCount, IEnumerable<ulong> includeRoles, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlParams)}{sb}", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers).ConfigureAwait(false);

		var pruned = DiscordJson.DeserializeObject<RestGuildPruneResultPayload>(res.Response, this.Discord);

		return pruned.Pruned;
	}

#endregion

#region GuildVarious

	/// <summary>
	///     Gets the template async.
	/// </summary>
	/// <param name="code">The code.</param>
	internal async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
	{
		var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			code
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var templatesRaw = DiscordJson.DeserializeObject<DiscordGuildTemplate>(res.Response, this.Discord);

		return templatesRaw;
	}

	/// <summary>
	///     Gets the guild integrations async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var integrations = DiscordJson.DeserializeIEnumerableObject<List<DiscordIntegration>>(res.Response, this.Discord);
		return integrations;
	}

	/// <summary>
	///     Gets the guild preview async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PREVIEW}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordGuildPreview>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Creates the guild integration async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordIntegration>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Modifies the guild integration async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			integration_id = integrationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordIntegration>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Deletes the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integration">The integration.</param>
	internal Task DeleteGuildIntegrationAsync(ulong guildId, DiscordIntegration integration)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			integration_id = integration.Id
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, payload: DiscordJson.SerializeObject(integration));
	}

	/// <summary>
	///     Syncs the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integrationId">The integration_id.</param>
	internal Task SyncGuildIntegrationAsync(ulong guildId, ulong integrationId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id{Endpoints.SYNC}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId,
			integration_id = integrationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
	}

	/// <summary>
	///     Gets the guild voice regions async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REGIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var regions = DiscordJson.DeserializeIEnumerableObject<List<DiscordVoiceRegion>>(res.Response, this.Discord);

		return regions;
	}

	/// <summary>
	///     Gets the guild invites async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INVITES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var invites = DiscordJson.DeserializeIEnumerableObject<List<DiscordInvite>>(res.Response, this.Discord);

		return invites;
	}

#endregion

#region Invite

	/// <summary>
	///     Gets the invite async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			invite_code = inviteCode
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordInvite>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Deletes the invite async.
	/// </summary>
	/// <param name="inviteCode">The invite_code.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordInvite> DeleteInviteAsync(string inviteCode, string? reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.INVITES}/:invite_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			invite_code = inviteCode
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordInvite>(res.Response, this.Discord);
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
	 *     var ret = DiscordJson.DeserializeObject<DiscordInvite>(res.Response, this.Discord);
	 *     ret.Discord = this.Discord;
	 *
	 *     return ret;
	 * }
	 */

#endregion

#region Connections

	/// <summary>
	///     Gets the users connections async.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordConnection>> GetUserConnectionsAsync()
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CONNECTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var connections = DiscordJson.DeserializeIEnumerableObject<List<DiscordConnection>>(res.Response, this.Discord);
		return connections;
	}

	/// <summary>
	///     Gets the applications role connection metadata records.
	/// </summary>
	/// <param name="id">The application id.</param>
	/// <returns>A list of metadata records or <see langword="null" />.</returns>
	/// s
	internal async Task<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> GetRoleConnectionMetadataRecords(ulong id)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.ROLE_CONNECTIONS}{Endpoints.METADATA}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = id
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var metadata = DiscordJson.DeserializeIEnumerableObject<List<DiscordApplicationRoleConnectionMetadata>>(res.Response, this.Discord);
		return metadata;
	}

	/// <summary>
	///     Updates the applications role connection metadata records.
	/// </summary>
	/// <param name="id">The application id.</param>
	/// <param name="metadataObjects">A list of metadata objects. Max 5.</param>
	/// <returns>A list of the created metadata records.</returns>
	/// s
	internal async Task<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> UpdateRoleConnectionMetadataRecords(ulong id, IEnumerable<DiscordApplicationRoleConnectionMetadata> metadataObjects)
	{
		var pld = new List<RestApplicationRoleConnectionMetadataPayload>();
		foreach (var metadataObject in metadataObjects)
			pld.Add(new()
			{
				Type = metadataObject.Type,
				Key = metadataObject.Key,
				Name = metadataObject.Name,
				Description = metadataObject.Description,
				NameLocalizations = metadataObject.NameLocalizations?.GetKeyValuePairs(),
				DescriptionLocalizations = metadataObject.DescriptionLocalizations?.GetKeyValuePairs()
			});

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.ROLE_CONNECTIONS}{Endpoints.METADATA}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			application_id = id
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
		var metadata = DiscordJson.DeserializeIEnumerableObject<List<DiscordApplicationRoleConnectionMetadata>>(res.Response, this.Discord);
		return metadata;
	}

#endregion

#region Webhooks

	/// <summary>
	///     Creates the webhook async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordWebhook> CreateWebhookAsync(ulong channelId, string name, Optional<string> base64Avatar, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordWebhook>(res.Response, this.Discord);
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	///     Gets the channel webhooks async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var webhooksRaw = DiscordJson.DeserializeIEnumerableObject<List<DiscordWebhook>>(res.Response, this.Discord).Select(xw =>
		{
			xw.ApiClient = this;
			return xw;
		});
		return [.. webhooksRaw];
	}

	/// <summary>
	///     Gets the guild webhooks async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WEBHOOKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var webhooksRaw = DiscordJson.DeserializeIEnumerableObject<List<DiscordWebhook>>(res.Response, this.Discord).Select(xw =>
		{
			xw.ApiClient = this;
			return xw;
		});
		return [.. webhooksRaw];
	}

	/// <summary>
	///     Gets the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhookId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			webhook_id = webhookId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord?.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordWebhook>(res.Response, this.Discord);
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	///     Gets the webhook with token async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhookId, string webhookToken)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord?.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordWebhook>(res.Response, this.Discord);
		ret.Token = webhookToken;
		ret.Id = webhookId;
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	///     Modifies the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, ulong channelId, string name, Optional<string> base64Avatar, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			webhook_id = webhookId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordWebhook>(res.Response, this.Discord);
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	///     Modifies the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhookId, string name, string base64Avatar, string webhookToken, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordWebhook>(res.Response, this.Discord);
		ret.ApiClient = this;

		return ret;
	}

	/// <summary>
	///     Deletes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteWebhookAsync(ulong webhookId, string? reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			webhook_id = webhookId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Deletes the webhook async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteWebhookAsync(ulong webhookId, string webhookToken, string? reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Executes the webhook async.
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

		var flags = MessageFlags.None;
		if (builder.EmbedsSuppressed)
			flags |= MessageFlags.SuppressedEmbeds;
		if (builder.NotificationsSuppressed)
			flags |= MessageFlags.SuppressNotifications;
		if (builder.IsVoiceMessage)
			flags |= MessageFlags.IsVoiceMessage;

		var pld = new RestWebhookExecutePayload
		{
			Content = builder.Content,
			Username = builder.Username.ValueOrDefault(),
			AvatarUrl = builder.AvatarUrl.ValueOrDefault(),
			IsTts = builder.IsTts,
			Embeds = builder.Embeds,
			Components = builder.Components,
			ThreadName = builder.ThreadName,
			Flags = flags,
			AppliedTags = builder.AppliedTags.Any() ? builder.AppliedTags : null,
			DiscordPollRequest = builder.Poll?.Build()
		};

		if (builder.Mentions.Any())
			pld.Mentions = new(builder.Mentions, builder.Mentions.Count is not 0);

		if (builder.Files?.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = [];
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					Filename = file.Filename,
					FileSize = null
				};
				attachments.Add(att);
				fileId++;
			}

			pld.Attachments = attachments;
		}
		else
		{
			if (builder.Attachments.Any())
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = new(builder.Attachments.Count);
				foreach (var att in builder.Attachments)
				{
					att.Id = fileId;
					attachments.Add(att);
					fileId++;
				}

				pld.Attachments = attachments;
			}
		}

		if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.Files?.Count > 0 || builder.IsTts || builder.Mentions.Any())
			values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken
		}, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord?.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		if (builder.WithComponents != null)
			qub.AddParameter("with_components", builder.WithComponents.Value.ToString().ToLower());

		var url = qub.Build();

		var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
		var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);

		if (this.Discord != null!)
			foreach (var att in ret.Attachments)
				att.Discord = this.Discord;

		foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
			file.Stream.Position = file.ResetPositionTo.Value;
		return ret;
	}

	/// <summary>
	///     Executes the webhook slack async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="jsonPayload">The json_payload.</param>
	/// <param name="threadId">The thread_id.</param>
	internal async Task<DiscordMessage> ExecuteWebhookSlackAsync(ulong webhookId, string webhookToken, string jsonPayload, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.SLACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken
		}, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
		var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Executes the webhook github async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="jsonPayload">The json_payload.</param>
	/// <param name="threadId">The thread_id.</param>
	internal async Task<DiscordMessage> ExecuteWebhookGithubAsync(ulong webhookId, string webhookToken, string jsonPayload, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.GITHUB}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken
		}, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: jsonPayload).ConfigureAwait(false);
		var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Edits the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>
	internal async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, DiscordWebhookBuilder builder, string threadId)
	{
		builder.Validate(true);

		MessageFlags? flags = builder.FlagsChanged ? MessageFlags.None : null;
		if (builder.EmbedsSuppressed)
			flags |= MessageFlags.SuppressedEmbeds;
		if (builder.NotificationsSuppressed)
			flags |= MessageFlags.SuppressNotifications;
		if (builder.IsComponentsV2)
			flags |= MessageFlags.IsComponentsV2;

		var pld = new RestWebhookMessageEditPayload
		{
			Content = builder.Content,
			Embeds = builder.Embeds,
			Components = builder.Components,
			Flags = flags
		};

		if (builder.Mentions.Any())
			pld.Mentions = new(builder.Mentions, builder.Mentions.Count is not 0);

		if (builder.Files?.Count > 0)
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = [];
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					Filename = file.Filename,
					FileSize = null
				};
				attachments.Add(att);
				fileId++;
			}

			if (builder.Attachments is { Count: > 0 })
				attachments.AddRange(builder.Attachments);

			pld.Attachments = attachments;

			var values = new Dictionary<string, string>
			{
				["payload_json"] = DiscordJson.SerializeObject(pld)
			};
			var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
			{
				webhook_id = webhookId,
				webhook_token = webhookToken,
				message_id = messageId
			}, out var path);

			var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
			if (threadId != null)
				qub.AddParameter("thread_id", threadId);

			var url = qub.Build();
			var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: builder.Files).ConfigureAwait(false);

			var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);

			foreach (var att in ret.AttachmentsInternal)
				att.Discord = this.Discord;

			foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
				file.Stream.Position = file.ResetPositionTo.Value;

			return ret;
		}
		else
		{
			if (builder.Attachments.Any())
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = new(builder.Attachments.Count);
				foreach (var att in builder.Attachments)
				{
					att.Id = fileId;
					attachments.Add(att);
					fileId++;
				}

				pld.Attachments = attachments;
			}

			var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
			var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
			{
				webhook_id = webhookId,
				webhook_token = webhookToken,
				message_id = messageId
			}, out var path);

			var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
			if (threadId != null)
				qub.AddParameter("thread_id", threadId);

			var url = qub.Build();
			var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

			var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);

			foreach (var att in ret.AttachmentsInternal)
				att.Discord = this.Discord;

			return ret;
		}
	}

	/// <summary>
	///     Edits the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="builder">The builder.</param>
	/// <param name="threadId">The thread_id.</param>
	internal Task<DiscordMessage> EditWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, DiscordWebhookBuilder builder, ulong threadId) =>
		this.EditWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), builder, threadId.ToString());

	/// <summary>
	///     Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>
	internal async Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken,
			message_id = messageId
		}, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	internal Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId) =>
		this.GetWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), null);

	/// <summary>
	///     Gets the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>
	internal Task<DiscordMessage> GetWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId, ulong threadId) =>
		this.GetWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), threadId.ToString());

	/// <summary>
	///     Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="threadId">The thread_id.</param>
	internal async Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, string messageId, string threadId)
	{
		var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			webhook_id = webhookId,
			webhook_token = webhookToken,
			message_id = messageId
		}, out var path);

		var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
		if (threadId != null)
			qub.AddParameter("thread_id", threadId);
		var url = qub.Build();
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes the webhook message async.
	/// </summary>
	/// <param name="webhookId">The webhook_id.</param>
	/// <param name="webhookToken">The webhook_token.</param>
	/// <param name="messageId">The message_id.</param>
	internal Task DeleteWebhookMessageAsync(ulong webhookId, string webhookToken, ulong messageId) =>
		this.DeleteWebhookMessageAsync(webhookId, webhookToken, messageId.ToString(), null);

	/// <summary>
	///     Deletes the webhook message async.
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
	///     Creates the reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>
	internal Task CreateReactionAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			channel_id = channelId,
			message_id = messageId,
			emoji
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : 0.26);
	}

	/// <summary>
	///     Deletes the own reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>
	internal Task DeleteOwnReactionAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			message_id = messageId,
			emoji
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : 0.26);
	}

	/// <summary>
	///     Deletes the user reaction async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteUserReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, string? reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			message_id = messageId,
			emoji,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : 0.26);
	}

	/// <summary>
	///     Gets the reactions async.
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId,
			message_id = messageId,
			emoji
		}, out var path);

		var url = Utilities.GetApiUriFor(path, BuildQueryString(urlParams), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var reactersRaw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response);
		var reacters = new List<DiscordUser>();
		foreach (var xr in reactersRaw)
		{
			var usr = new DiscordUser(xr)
			{
				Discord = this.Discord
			};
			usr = this.Discord.UserCache.AddOrUpdate(xr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				old.GlobalName = usr.GlobalName;
				return old;
			});

			reacters.Add(usr);
		}

		return new ReadOnlyCollection<DiscordUser>([.. reacters]);
	}

	/// <summary>
	///     Deletes the all reactions async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteAllReactionsAsync(ulong channelId, ulong messageId, string? reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			message_id = messageId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : 0.26);
	}

	/// <summary>
	///     Deletes the reactions emoji async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="messageId">The message_id.</param>
	/// <param name="emoji">The emoji.</param>
	internal Task DeleteReactionsEmojiAsync(ulong channelId, ulong messageId, string emoji)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			message_id = messageId,
			emoji
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : 0.26);
	}

#endregion

#region Threads

	/// <summary>
	///     Creates the thread.
	/// </summary>
	/// <param name="channelId">The channel id to create the thread in.</param>
	/// <param name="messageId">The optional message id to create the thread from.</param>
	/// <param name="name">The name of the thread.</param>
	/// <param name="autoArchiveDuration">The auto_archive_duration for the thread.</param>
	/// <param name="type">Can be either <see cref="ChannelType.PublicThread" /> or <see cref="ChannelType.PrivateThread" />.</param>
	/// <param name="rateLimitPerUser">The rate limit per user.</param>
	/// <param name="appliedTags">The tags to add on creation.</param>
	/// <param name="builder">The message builder.</param>
	/// <param name="isForum">Whether this thread is in a forum.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordThreadChannel> CreateThreadAsync(
		ulong channelId,
		ulong? messageId,
		string name,
		ThreadAutoArchiveDuration? autoArchiveDuration,
		ChannelType? type,
		int? rateLimitPerUser,
		IEnumerable<ForumPostTag>? appliedTags = null,
		DiscordMessageBuilder builder = null,
		bool isForum = false,
		string? reason = null
	)
	{
		var pld = new RestThreadChannelCreatePayload
		{
			Name = name,
			AutoArchiveDuration = autoArchiveDuration,
			PerUserRateLimit = rateLimitPerUser
		};

		if (isForum)
		{
			pld.Message = new()
			{
				Content = builder.Content,
				Attachments = builder.Attachments,
				Components = builder.Components,
				HasContent = true,
				Embeds = builder.Embeds,
				//Flags = builder.Flags,
				//Mentions = builder.Mentions,
				StickersIds = builder.Sticker != null
					? new List<ulong>(1)
					{
						builder.Sticker.Id
					}
					: null
			};
			if (appliedTags != null && appliedTags.Any())
			{
				List<ulong> tags = [];

				foreach (var b in appliedTags)
					tags.Add(b.Id.Value);

				pld.AppliedTags = tags;
				pld.Type = null;
			}
		}
		else
			pld.Type = type;

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:channel_id";
		if (messageId is not null)
			route += $"{Endpoints.MESSAGES}/:message_id";
		route += Endpoints.THREADS;

		object param = messageId is null
			? new
			{
				channel_id = channelId
			}
			: new
			{
				channel_id = channelId,
				message_id = messageId
			};

		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, param, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var threadChannel = DiscordJson.DeserializeObject<DiscordThreadChannel>(res.Response, this.Discord);
		return threadChannel;
	}

	/// <summary>
	///     Gets the thread.
	/// </summary>
	/// <param name="threadId">The thread id.</param>
	internal async Task<DiscordThreadChannel> GetThreadAsync(ulong threadId)
	{
		var route = $"{Endpoints.CHANNELS}/:thread_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			thread_id = threadId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordThreadChannel>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Joins the thread.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	internal async Task JoinThreadAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Leaves the thread.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	internal async Task LeaveThreadAsync(ulong channelId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Adds a thread member.
	/// </summary>
	/// <param name="channelId">The channel id to add the member to.</param>
	/// <param name="userId">The user id to add.</param>
	internal async Task AddThreadMemberAsync(ulong channelId, ulong userId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			channel_id = channelId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets a thread member.
	/// </summary>
	/// <param name="channelId">The channel id to get the member from.</param>
	/// <param name="userId">The user id to get.</param>
	/// <param name="withMember">Whether to include a <see cref="DiscordMember" /> object.</param>
	internal async Task<DiscordThreadChannelMember> GetThreadMemberAsync(ulong channelId, ulong userId, bool withMember = false)
	{
		var urlParams = new Dictionary<string, string>
		{
			["with_member"] = withMember.ToString(CultureInfo.InvariantCulture)
		};

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var threadMember = DiscordJson.DeserializeObject<DiscordThreadChannelMember>(res.Response, this.Discord);

		return threadMember;
	}

	/// <summary>
	///     Removes a thread member.
	/// </summary>
	/// <param name="channelId">The channel id to remove the member from.</param>
	/// <param name="userId">The user id to remove.</param>
	internal async Task RemoveThreadMemberAsync(ulong channelId, ulong userId)
	{
		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			channel_id = channelId,
			user_id = userId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets the thread members.
	/// </summary>
	/// <param name="threadId">The thread id.</param>
	/// <param name="withMember">Whether to include a <see cref="DiscordMember" /> object.</param>
	/// <param name="after">Get members after specified snowflake.</param>
	/// <param name="limit">Limits the results.</param>
	internal async Task<IReadOnlyList<DiscordThreadChannelMember>> GetThreadMembersAsync(ulong threadId, bool withMember = false, ulong? after = null, int? limit = null)
	{
		// TODO: Starting in API v11, List Thread Members will always return paginated results, regardless of whether with_member is passed or not.

		var urlParams = new Dictionary<string, string>
		{
			["with_member"] = withMember.ToString(CultureInfo.InvariantCulture)
		};

		if (after != null && withMember)
			urlParams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);
		if (limit is > 0 && withMember)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:thread_id{Endpoints.THREAD_MEMBERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			thread_id = threadId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var tm = DiscordJson.DeserializeIEnumerableObject<List<DiscordThreadChannelMember>>(res.Response, this.Discord);

		return tm;
	}

	/// <summary>
	///     Gets the active threads in a guild.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<DiscordThreadResult> GetActiveThreadsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.THREADS}{Endpoints.THREAD_ACTIVE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var threadReturn = DiscordJson.DeserializeObject<DiscordThreadResult>(res.Response, this.Discord);
		threadReturn.Threads.ForEach(x => x.Discord = this.Discord);

		return threadReturn;
	}

	/// <summary>
	///     Gets the joined private archived threads in a channel.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="before">Get threads before snowflake.</param>
	/// <param name="limit">Limit the results.</param>
	internal async Task<DiscordThreadResult> GetJoinedPrivateArchivedThreadsAsync(ulong channelId, ulong? before, int? limit)
	{
		var urlParams = new Dictionary<string, string>();
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (limit is > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.USERS}{Endpoints.ME}{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PRIVATE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var threadReturn = DiscordJson.DeserializeObject<DiscordThreadResult>(res.Response, this.Discord);

		return threadReturn;
	}

	/// <summary>
	///     Gets the public archived threads in a channel.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="before">Get threads before snowflake.</param>
	/// <param name="limit">Limit the results.</param>
	internal async Task<DiscordThreadResult> GetPublicArchivedThreadsAsync(ulong channelId, ulong? before, int? limit)
	{
		var urlParams = new Dictionary<string, string>();
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (limit is > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PUBLIC}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var threadReturn = DiscordJson.DeserializeObject<DiscordThreadResult>(res.Response, this.Discord);

		return threadReturn;
	}

	/// <summary>
	///     Gets the private archived threads in a channel.
	/// </summary>
	/// <param name="channelId">The channel id.</param>
	/// <param name="before">Get threads before snowflake.</param>
	/// <param name="limit">Limit the results.</param>
	internal async Task<DiscordThreadResult> GetPrivateArchivedThreadsAsync(ulong channelId, ulong? before, int? limit)
	{
		var urlParams = new Dictionary<string, string>();
		if (before != null)
			urlParams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
		if (limit is > 0)
			urlParams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PRIVATE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var threadReturn = DiscordJson.DeserializeObject<DiscordThreadResult>(res.Response, this.Discord);

		return threadReturn;
	}

	/// <summary>
	///     Modifies a thread.
	/// </summary>
	/// <param name="threadId">The thread to modify.</param>
	/// <param name="parentType">The parent channels type as failback to ignore forum fields.</param>
	/// <param name="name">The new name.</param>
	/// <param name="locked">The new locked state.</param>
	/// <param name="archived">The new archived state.</param>
	/// <param name="perUserRateLimit">The new per user rate limit.</param>
	/// <param name="autoArchiveDuration">The new auto archive duration.</param>
	/// <param name="invitable">The new user invitable state.</param>
	/// <param name="appliedTags">The tags to add on creation.</param>
	/// <param name="pinned">Whether the post is pinned.</param>
	/// <param name="reason">The reason for the modification.</param>
	internal Task ModifyThreadAsync(ulong threadId, ChannelType parentType, string name, Optional<bool?> locked, Optional<bool?> archived, Optional<int?> perUserRateLimit, Optional<ThreadAutoArchiveDuration?> autoArchiveDuration, Optional<bool?> invitable, Optional<IEnumerable<ForumPostTag>> appliedTags, Optional<bool?> pinned, string? reason)
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

		if (parentType == ChannelType.Forum)
		{
			if (appliedTags is { HasValue: true, Value: not null })
			{
				List<ulong> tags = new(appliedTags.Value.Count());

				foreach (var b in appliedTags.Value)
					tags.Add(b.Id.Value);

				pld.AppliedTags = tags;
			}

			if (pinned is { HasValue: true, Value: not null })
				pld.Flags = pinned.Value.Value ? ChannelFlags.Pinned : Optional.None;
		}

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.CHANNELS}/:thread_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			thread_id = threadId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

#endregion

#region Emoji

	/// <summary>
	///     Gets the guild emojis.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

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
	///     Gets the guild emoji.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="emojiId">The emoji id.</param>
	internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guildId, ulong emojiId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			emoji_id = emojiId
		}, out var path);

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
	///     Creates the guild emoji.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="name">The name.</param>
	/// <param name="imageb64">The imageb64.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guildId, string name, string imageb64, IEnumerable<ulong> roles, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

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
	///     Modifies the guild emoji.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="emojiId">The emoji id.</param>
	/// <param name="name">The name.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guildId, ulong emojiId, string name, IEnumerable<ulong> roles, string? reason)
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
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			emoji_id = emojiId
		}, out var path);

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
	///     Deletes the guild emoji.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="emojiId">The emoji id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteGuildEmojiAsync(ulong guildId, ulong emojiId, string? reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			emoji_id = emojiId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	///     Gets the application emojis.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	internal async Task<IReadOnlyList<DiscordApplicationEmoji>> GetApplicationEmojisAsync(ulong applicationId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var emojisRaw = JsonConvert.DeserializeObject<JObject>(res.Response);

		return [.. this.Discord.UpdateCachedApplicationEmojis(emojisRaw?.Value<JArray>("items")).Select(x => x.Value)];
	}

	/// <summary>
	///     Gets an application emoji.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="emojiId">The emoji id.</param>
	internal async Task<DiscordApplicationEmoji> GetApplicationEmojiAsync(ulong applicationId, ulong emojiId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId,
			emoji_id = emojiId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordApplicationEmoji>();

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu is not null)
			emoji.User = new(xtu);

		return this.Discord.UpdateCachedApplicationEmoji(emoji);
	}

	/// <summary>
	///     Creates an application emoji.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="name">The name.</param>
	/// <param name="imageb64">The imageb64.</param>
	internal async Task<DiscordApplicationEmoji> CreateApplicationEmojiAsync(ulong applicationId, string name, string imageb64)
	{
		var pld = new RestApplicationEmojiCreatePayload
		{
			Name = name,
			ImageB64 = imageb64
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordApplicationEmoji>();

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu is not null)
			emoji.User = new(xtu);

		return this.Discord.UpdateCachedApplicationEmoji(emoji);
	}

	/// <summary>
	///     Modifies an application emoji.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="emojiId">The emoji id.</param>
	/// <param name="name">The name.</param>
	internal async Task<DiscordApplicationEmoji> ModifyApplicationEmojiAsync(ulong applicationId, ulong emojiId, string name)
	{
		var pld = new RestApplicationEmojiModifyPayload
		{
			Name = name
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			application_id = applicationId,
			emoji_id = emojiId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordApplicationEmoji>();

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu is not null)
			emoji.User = new(xtu);

		return this.Discord.UpdateCachedApplicationEmoji(emoji);
	}

	/// <summary>
	///     Deletes an application emoji.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="emojiId">The emoji id.</param>
	internal Task DeleteApplicationEmojiAsync(ulong applicationId, ulong emojiId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			application_id = applicationId,
			emoji_id = emojiId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
	}

#endregion

#region Stickers

	/// <summary>
	///     Gets a sticker.
	/// </summary>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetStickerAsync(ulong stickerId)
	{
		var route = $"{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			sticker_id = stickerId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var ret = DiscordJson.DeserializeObject<DiscordSticker>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Gets the sticker pack.
	/// </summary>
	/// <param name="id">The sticker pack's id.</param>
	internal async Task<DiscordStickerPack> GetStickerPackAsync(ulong id)
	{
		var route = $"{Endpoints.STICKERPACKS}/:pack_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			pack_id = id
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordStickerPack>(res.Response, this.Discord);
	}

	/// <summary>
	///     Gets the sticker packs.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
	{
		var route = $"{Endpoints.STICKERPACKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response)["sticker_packs"] as JArray;
		var ret = json.ToDiscordObject<DiscordStickerPack[]>();

		return [.. ret];
	}

	/// <summary>
	///     Gets the guild stickers.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<IReadOnlyList<DiscordSticker>> GetGuildStickersAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeIEnumerableObject<List<DiscordSticker>>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Gets a guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetGuildStickerAsync(ulong guildId, ulong stickerId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId,
			sticker_id = stickerId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordSticker>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Creates the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="file">The file.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> CreateGuildStickerAsync(ulong guildId, string name, string description, string tags, DiscordMessageFile file, string? reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var res = await this.DoStickerMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, name, tags, description, headers, file).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordSticker>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Modifies the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> ModifyGuildStickerAsync(ulong guildId, ulong stickerId, Optional<string> name, Optional<string> description, Optional<string> tags, string? reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId,
			sticker_id = stickerId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var pld = new RestStickerModifyPayload
		{
			Name = name,
			Description = description,
			Tags = tags
		};

		var values = new Dictionary<string, string>
		{
			["payload_json"] = DiscordJson.SerializeObject(pld)
		};

		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route).ConfigureAwait(false);
		var ret = DiscordJson.DeserializeObject<DiscordSticker>(res.Response, this.Discord);
		return ret;
	}

	/// <summary>
	///     Deletes the guild sticker async.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task DeleteGuildStickerAsync(ulong guildId, ulong stickerId, string? reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			guild_id = guildId,
			sticker_id = stickerId
		}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);
	}

#endregion

#region Application Commands

	/// <summary>
	///     Gets the global application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(ulong applicationId, bool withLocalizations = false)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId
		}, out var path);

		var querydict = new Dictionary<string, string>
		{
			["with_localizations"] = withLocalizations.ToString().ToLower()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeIEnumerableObject<IEnumerable<DiscordApplicationCommand>>(res.Response, this.Discord);
		return [.. ret];
	}

	/// <summary>
	///     Bulk overwrites the global application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commands">The commands.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ulong applicationId, IEnumerable<DiscordApplicationCommand> commands)
	{
		var pld = new List<RestApplicationCommandCreatePayload>();
		if (commands.Any())
			pld.AddRange(commands.Select(command => new RestApplicationCommandCreatePayload
			{
				Type = command.Type,
				Name = command.Name,
				Description = command.Type is ApplicationCommandType.ChatInput ? command.Description : null,
				Options = command.Options,
				NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
				DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
				DefaultMemberPermission = command.DefaultMemberPermissions,
				Nsfw = command.IsNsfw,
				AllowedContexts = command.AllowedContexts,
				IntegrationTypes = command.IntegrationTypes
			}));

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeIEnumerableObject<IEnumerable<DiscordApplicationCommand>>(res.Response, this.Discord);
		return [.. ret];
	}

	/// <summary>
	///     Creates a global application command.
	/// </summary>
	/// <param name="applicationId">The applicationid.</param>
	/// <param name="command">The command.</param>
	internal async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(ulong applicationId, DiscordApplicationCommand command)
	{
		var pld = new RestApplicationCommandCreatePayload
		{
			Type = command.Type,
			Name = command.Name,
			Description = command.Type is ApplicationCommandType.ChatInput or ApplicationCommandType.PrimaryEntryPoint ? command.Description : null,
			Options = command.Options,
			NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
			DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
			DefaultMemberPermission = command.DefaultMemberPermissions,
			Nsfw = command.IsNsfw,
			AllowedContexts = command.AllowedContexts,
			IntegrationTypes = command.IntegrationTypes
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordApplicationCommand>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	///     Gets a global application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong applicationId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId,
			command_id = commandId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordApplicationCommand>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	///     Edits a global application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="commandId">The command id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="options">The options.</param>
	/// <param name="nameLocalization">The localizations of the name.</param>
	/// <param name="descriptionLocalization">The localizations of the description.</param>
	/// <param name="defaultMemberPermission">The default member permissions.</param>
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts.</param>
	/// <param name="integrationTypes">The allowed integration types.</param>
	internal async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(
		ulong applicationId,
		ulong commandId,
		Optional<string> name,
		Optional<string?> description,
		Optional<List<DiscordApplicationCommandOption>?> options,
		Optional<DiscordApplicationCommandLocalization?> nameLocalization,
		Optional<DiscordApplicationCommandLocalization?> descriptionLocalization,
		Optional<Permissions?> defaultMemberPermission,
		Optional<bool> isNsfw,
		Optional<List<InteractionContextType>?> allowedContexts,
		Optional<List<ApplicationCommandIntegrationTypes>?> integrationTypes
	)
	{
		var pld = new RestApplicationCommandEditPayload
		{
			Name = name,
			Description = description,
			Options = options,
			DefaultMemberPermission = defaultMemberPermission,
			NameLocalizations = nameLocalization.ValueOrDefault()?.GetKeyValuePairs(),
			DescriptionLocalizations = descriptionLocalization.ValueOrDefault()?.GetKeyValuePairs(),
			Nsfw = isNsfw,
			AllowedContexts = allowedContexts,
			IntegrationTypes = integrationTypes
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			application_id = applicationId,
			command_id = commandId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordApplicationCommand>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	///     Deletes a global application command.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>
	/// <param name="commandId">The command_id.</param>
	internal async Task DeleteGlobalApplicationCommandAsync(ulong applicationId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			application_id = applicationId,
			command_id = commandId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, bool withLocalizations = false)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId,
			guild_id = guildId
		}, out var path);

		var querydict = new Dictionary<string, string>
		{
			["with_localizations"] = withLocalizations.ToString().ToLower()
		};
		var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeIEnumerableObject<IEnumerable<DiscordApplicationCommand>>(res.Response, this.Discord);
		return [.. ret];
	}

	/// <summary>
	///     Bulk overwrites the guild application commands.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commands">The commands.</param>
	internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong applicationId, ulong guildId, IEnumerable<DiscordApplicationCommand> commands)
	{
		var pld = new List<RestApplicationCommandCreatePayload>();
		if (commands.Any())
			pld.AddRange(commands.Select(command => new RestApplicationCommandCreatePayload
			{
				Type = command.Type,
				Name = command.Name,
				Description = command.Type is ApplicationCommandType.ChatInput ? command.Description : null,
				Options = command.Options,
				NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
				DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
				DefaultMemberPermission = command.DefaultMemberPermissions,
				Nsfw = command.IsNsfw,
				AllowedContexts = command.AllowedContexts
			}));

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			application_id = applicationId,
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeIEnumerableObject<IEnumerable<DiscordApplicationCommand>>(res.Response, this.Discord);
		return [.. ret];
	}

	/// <summary>
	///     Creates a guild application command.
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
			Description = command.Type == ApplicationCommandType.ChatInput ? command.Description : null,
			Options = command.Options,
			NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
			DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs(),
			DefaultMemberPermission = command.DefaultMemberPermissions,
			Nsfw = command.IsNsfw,
			AllowedContexts = command.AllowedContexts
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			application_id = applicationId,
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordApplicationCommand>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	///     Gets a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId,
			guild_id = guildId,
			command_id = commandId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordApplicationCommand>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	///     Edits a guild application command.
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
	/// <param name="isNsfw">Whether this command is marked as NSFW.</param>
	/// <param name="allowedContexts">The allowed contexts.</param>
	/// <param name="integrationTypes">The allowed integration types.</param>
	internal async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(
		ulong applicationId,
		ulong guildId,
		ulong commandId,
		Optional<string> name,
		Optional<string?> description,
		Optional<List<DiscordApplicationCommandOption>?> options,
		Optional<DiscordApplicationCommandLocalization?> nameLocalization,
		Optional<DiscordApplicationCommandLocalization?> descriptionLocalization,
		Optional<Permissions?> defaultMemberPermission,
		Optional<bool> isNsfw,
		Optional<List<InteractionContextType>?> allowedContexts,
		Optional<List<ApplicationCommandIntegrationTypes>?> integrationTypes
	)
	{
		var pld = new RestApplicationCommandEditPayload
		{
			Name = name,
			Description = description,
			Options = options,
			DefaultMemberPermission = defaultMemberPermission,
			NameLocalizations = nameLocalization.ValueOrDefault()?.GetKeyValuePairs(),
			DescriptionLocalizations = descriptionLocalization.ValueOrDefault()?.GetKeyValuePairs(),
			Nsfw = isNsfw,
			AllowedContexts = allowedContexts,
			IntegrationTypes = integrationTypes
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			application_id = applicationId,
			guild_id = guildId,
			command_id = commandId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = DiscordJson.DeserializeObject<DiscordApplicationCommand>(res.Response, this.Discord);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	///     Deletes a guild application command.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="commandId">The command id.</param>
	internal async Task DeleteGuildApplicationCommandAsync(ulong applicationId, ulong guildId, ulong commandId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			application_id = applicationId,
			guild_id = guildId,
			command_id = commandId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Creates an interaction response.
	/// </summary>
	/// <param name="interactionId">The interaction id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="type">The type.</param>
	/// <param name="builder">The builder.</param>
	internal async Task<DiscordInteractionCallbackResponse> CreateInteractionResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionResponseBuilder? builder)
	{
		if (builder?.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		RestInteractionResponsePayload pld;

		if (type is not InteractionResponseType.AutoCompleteResult)
		{
			MessageFlags? flags = builder is { FlagsChanged: true } ? MessageFlags.None : null;
			if (builder is not null)
			{
				if (builder.IsEphemeral)
					flags |= MessageFlags.Ephemeral;
				if (builder.EmbedsSuppressed)
					flags |= MessageFlags.SuppressedEmbeds;
				if (builder.NotificationsSuppressed)
					flags |= MessageFlags.SuppressNotifications;
				if (builder.IsComponentsV2)
					flags |= MessageFlags.IsComponentsV2;
			}

			var data = builder is not null
				? new DiscordInteractionApplicationCommandCallbackData
				{
					Content = builder?.Content ?? null,
					Embeds = builder?.Embeds ?? null,
					IsTts = builder?.IsTts,
					Mentions = builder?.Mentions.Any() ?? false ? new(builder.Mentions, builder.Mentions.Count is not 0) : null,
					Flags = flags,
					Components = builder?.Components ?? null,
					Choices = null,
					DiscordPollRequest = builder?.Poll?.Build()
				}
				: null;

			pld = new()
			{
				Type = type,
				Data = data,
				CallbackHints = builder?.CallbackHints
			};

			if (builder is { Files.Count: > 0 })
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = [];
				foreach (var file in builder.Files)
				{
					DiscordAttachment att = new()
					{
						Id = fileId,
						Discord = this.Discord,
						Description = file.Description,
						Filename = file.Filename,
						FileSize = null
					};
					attachments.Add(att);
					fileId++;
				}

				if (pld.Data is not null)
					pld.Data.Attachments = attachments;
			}
			else if (builder is { Attachments.Count: > 0 })
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = new(builder.Attachments.Count);
				foreach (var att in builder.Attachments)
				{
					att.Id = fileId;
					attachments.Add(att);
					fileId++;
				}

				if (pld.Data is not null)
					pld.Data.Attachments = attachments;
			}
		}
		else
			pld = new()
			{
				Type = type,
				Data = new()
				{
					Content = null,
					Embeds = null,
					IsTts = null,
					Mentions = null,
					Flags = null,
					Components = null,
					Choices = builder?.Choices,
					Attachments = null,
					DiscordPollRequest = null
				},
				CallbackHints = null
			};

		var values = new Dictionary<string, string>();

		if (builder is not null)
			if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTts || builder.Mentions.Any() || builder.Files?.Count > 0 || builder.Components?.Count > 0)
				values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			interaction_id = interactionId,
			interaction_token = interactionToken
		}, out var path);

		RestResponse response;

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "false").AddParameter("with_response", "true").Build();
		if (builder is not null && values.Count is not 0)
		{
			response = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);

			if (builder.Files is not null)
				foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
					file.Stream.Position = file.ResetPositionTo!.Value;
		}
		else
			response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return response.ResponseCode is not HttpStatusCode.NoContent && !string.IsNullOrEmpty(response.Response)
			? DiscordJson.DeserializeObject<DiscordInteractionCallbackResponse>(response.Response, this.Discord)
			: null;
	}

	/// <summary>
	///     Creates an interaction response with a modal.
	/// </summary>
	/// <param name="interactionId">The interaction id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="type">The type.</param>
	/// <param name="builder">The builder.</param>
	internal async Task CreateInteractionModalResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionModalBuilder builder)
	{
		if (builder.ModalComponents.Any(mc => mc.Components.Any(c => c.Type != ComponentType.InputText)))
			throw new NotSupportedException("Can't send any other type then Input Text as Modal Component.");

		var pld = new RestInteractionModalResponsePayload
		{
			Type = type,
			Data = new()
			{
				Title = builder.Title,
				CustomId = builder.CustomId,
				ModalComponents = builder.ModalComponents
			},
			CallbackHints = builder?.CallbackHints
		};

		var values = new Dictionary<string, string>();

		var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			interaction_id = interactionId,
			interaction_token = interactionToken
		}, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	///     Creates an interaction response with an iFrame.
	/// </summary>
	/// <param name="interactionId">The interaction id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="type">The type.</param>
	/// <param name="customId">The custom id of the iframe.</param>
	/// <param name="title">The title of the iframe.</param>
	/// <param name="modalSize">The size of the iframe.</param>
	/// <param name="iFramePath">The path of the iframe.</param>
	internal async Task CreateInteractionIframeResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, string customId, string title, IframeModalSize modalSize, string? iFramePath = null)
	{
		var pld = new RestInteractionIframeResponsePayload
		{
			Type = type,
			Data = new()
			{
				Title = title,
				CustomId = customId,
				ModalSize = modalSize,
				IframePath = iFramePath
			},
			CallbackHints = null
		};

		var values = new Dictionary<string, string>();

		var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			interaction_id = interactionId,
			interaction_token = interactionToken
		}, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	internal Task<DiscordMessage> GetOriginalInteractionResponseAsync(ulong applicationId, string interactionToken) =>
		this.GetWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, null);

	/// <summary>
	///     Edits the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="builder">The builder.</param>
	internal Task<DiscordMessage> EditOriginalInteractionResponseAsync(ulong applicationId, string interactionToken, DiscordWebhookBuilder builder) =>
		this.EditWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, builder, null);

	/// <summary>
	///     Deletes the original interaction response.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	internal Task DeleteOriginalInteractionResponseAsync(ulong applicationId, string interactionToken) =>
		this.DeleteWebhookMessageAsync(applicationId, interactionToken, Endpoints.ORIGINAL, null);

	/// <summary>
	///     Creates the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="builder">The builder.</param>
	internal async Task<DiscordMessage> CreateFollowupMessageAsync(ulong applicationId, string interactionToken, DiscordFollowupMessageBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder, nameof(builder));

		builder.Validate();

		if (builder.Embeds != null)
			foreach (var embed in builder.Embeds)
				if (embed.Timestamp != null)
					embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

		MessageFlags? flags = builder is { FlagsChanged: true } ? MessageFlags.None : null;
		if (builder.IsEphemeral)
			flags |= MessageFlags.Ephemeral;
		if (builder.EmbedsSuppressed)
			flags |= MessageFlags.SuppressedEmbeds;
		if (builder.NotificationsSuppressed)
			flags |= MessageFlags.SuppressNotifications;
		if (builder.IsComponentsV2)
			flags |= MessageFlags.IsComponentsV2;

		var values = new Dictionary<string, string>();
		var pld = new RestFollowupMessageCreatePayload
		{
			Content = builder.Content,
			IsTts = builder.IsTts,
			Embeds = builder.Embeds,
			Flags = flags,
			Components = builder.Components,
			DiscordPollRequest = builder.Poll?.Build()
		};

		if (builder.Files is { Count: > 0 })
		{
			ulong fileId = 0;
			List<DiscordAttachment> attachments = [];
			foreach (var file in builder.Files)
			{
				DiscordAttachment att = new()
				{
					Id = fileId,
					Discord = this.Discord,
					Description = file.Description,
					Filename = file.Filename,
					FileSize = null
				};
				attachments.Add(att);
				fileId++;
			}

			pld.Attachments = attachments;
		}
		else
		{
			if (builder.Attachments.Any())
			{
				ulong fileId = 0;
				List<DiscordAttachment> attachments = new(builder.Attachments.Count);
				foreach (var att in builder.Attachments)
				{
					att.Id = fileId;
					attachments.Add(att);
					fileId++;
				}

				pld.Attachments = attachments;
			}
		}

		if (builder.Mentions.Any())
			pld.Mentions = new(builder.Mentions, builder.Mentions.Count is not 0);

		if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count > 0 || builder.IsTts || builder.Mentions.Any() || builder.Files?.Count > 0 || builder.Components?.Count > 0)
			values["payload_json"] = DiscordJson.SerializeObject(pld);

		var route = $"{Endpoints.WEBHOOKS}/:application_id/:interaction_token";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			application_id = applicationId,
			interaction_token = interactionToken
		}, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
		RestResponse res;
		if (values.Count is not 0)
		{
			res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
			var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);

			foreach (var att in ret.AttachmentsInternal)
				att.Discord = this.Discord;

			foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
				file.Stream.Position = file.ResetPositionTo.Value;
			ret.Discord = this.Discord;
			return ret;
		}
		else
		{
			res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
			var ret = DiscordJson.DeserializeObject<DiscordMessage>(res.Response, this.Discord);

			ret.Discord = this.Discord;
			return ret;
		}
	}

	/// <summary>
	///     Gets the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	internal Task<DiscordMessage> GetFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId) =>
		this.GetWebhookMessageAsync(applicationId, interactionToken, messageId);

	/// <summary>
	///     Edits the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	/// <param name="builder">The builder.</param>
	internal Task<DiscordMessage> EditFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId, DiscordWebhookBuilder builder) =>
		this.EditWebhookMessageAsync(applicationId, interactionToken, messageId.ToString(), builder, null);

	/// <summary>
	///     Deletes the followup message.
	/// </summary>
	/// <param name="applicationId">The application id.</param>
	/// <param name="interactionToken">The interaction token.</param>
	/// <param name="messageId">The message id.</param>
	internal Task DeleteFollowupMessageAsync(ulong applicationId, string interactionToken, ulong messageId) =>
		this.DeleteWebhookMessageAsync(applicationId, interactionToken, messageId);

#endregion

#region Misc

	/// <summary>
	///     Gets the published store sku listings (premium application subscription).
	/// </summary>
	/// <param name="applicationId">The application id to fetch the listenings for.</param>
	/// <returns>A list of published listings with <see cref="DiscordStoreSku" />s.</returns>
	internal async Task<IReadOnlyList<DiscordStoreSku>> GetPublishedListingsAsync(ulong applicationId)
	{
		var urlParams = new Dictionary<string, string>
		{
			["application_id"] = applicationId.ToString()
		};

		var route = $"{Endpoints.STORE}{Endpoints.PUBLISHED_LISTINGS}{Endpoints.SKUS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var storeSkus = DiscordJson.DeserializeIEnumerableObject<List<DiscordStoreSku>>(res.Response, this.Discord);
		foreach (var storeSku in storeSkus)
			storeSku.Sku.Discord = this.Discord;

		return new ReadOnlyCollection<DiscordStoreSku>(storeSkus);
	}

	/// <summary>
	///     Gets the applications skus.
	/// </summary>
	/// <param name="applicationId">The application id to fetch the listenings for.</param>
	/// <returns>A list of published listings with <see cref="DiscordStoreSku" />s.</returns>
	internal async Task<IReadOnlyList<DiscordSku>> GetSkusAsync(ulong applicationId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.SKUS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return new ReadOnlyCollection<DiscordSku>(DiscordJson.DeserializeIEnumerableObject<List<DiscordSku>>(res.Response, this.Discord));
	}

	/// <summary>
	///     Gets the applications entitlements.
	/// </summary>
	/// <param name="applicationId">The application id to fetch the entitlements for.</param>
	/// <param name="guildId">Filter returned entitlements to a specific guild id.</param>
	/// <param name="userId">Filter returned entitlements to a specific user id.</param>
	/// <param name="skuIds">Optional list of SKU IDs to check entitlements for.</param>
	/// <param name="before">Retrieve entitlements before this entitlement ID.</param>
	/// <param name="after">Retrieve entitlements after this entitlement ID.</param>
	/// <param name="limit">Number of entitlements to return, 1-100, default 100.</param>
	/// <param name="excludeEnded">
	///     Whether or not ended entitlements should be omitted. Defaults to false, ended entitlements
	///     are included by default.
	/// </param>
	/// <param name="excludeDeleted">
	///     Whether or not deleted entitlements should be omitted. Defaults to true, deleted
	///     entitlements are not included by default.
	/// </param>
	/// <returns>A list of <see cref="DiscordEntitlement" />.</returns>
	internal async Task<IReadOnlyList<DiscordEntitlement>> GetEntitlementsAsync(ulong applicationId, ulong? guildId, ulong? userId, List<ulong>? skuIds = null, ulong? before = null, ulong? after = null, int limit = 100, bool? excludeEnded = null, bool? excludeDeleted = null)
	{
		var urlParams = new Dictionary<string, string>();
		urlParams["limit"] = limit.ToString();
		if (guildId.HasValue)
			urlParams["guild_id"] = guildId.Value.ToString();
		if (userId.HasValue)
			urlParams["user_id"] = userId.Value.ToString();
		if (skuIds != null && skuIds.Count > 0)
			urlParams["sku_ids"] = string.Join(",", skuIds);
		if (before.HasValue)
			urlParams["before"] = before.Value.ToString();
		if (after.HasValue)
			urlParams["after"] = after.Value.ToString();
		if (excludeEnded.HasValue)
			urlParams["exclude_ended"] = excludeEnded.Value.ToString().ToLower();
		if (excludeDeleted.HasValue)
			urlParams["exclude_deleted"] = excludeDeleted.Value.ToString().ToLower();

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.ENTITLEMENTS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return new ReadOnlyCollection<DiscordEntitlement>(DiscordJson.DeserializeIEnumerableObject<List<DiscordEntitlement>>(res.Response, this.Discord));
	}

	/// <summary>
	///     Gets an entitlement for given <paramref name="applicationId" />.
	/// </summary>
	/// <param name="applicationId">The application id to fetch the entitlement for.</param>
	/// <param name="entitlementId">The entitlement id to fetch.</param>
	/// <returns>The requested <see cref="DiscordEntitlement" />.</returns>
	internal async Task<DiscordEntitlement?> GetEntitlementAsync(ulong applicationId, ulong entitlementId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.ENTITLEMENTS}/:entitlement_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId,
			entitlement_id = entitlementId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordEntitlement>(res.Response, this.Discord);
	}

	// TODO: userId can be skipped if using oauth
	/// <summary>
	///     Gets the subscriptions of an sku.
	/// </summary>
	/// <param name="skuId">The sku id to fetch the subscriptions for.</param>
	/// <param name="userId">The user ID for which to return subscriptions. Required except for OAuth queries.</param>
	/// <param name="before">List subscriptions before this ID.</param>
	/// <param name="after">List subscriptions after this ID.</param>
	/// <param name="limit">Number of results to return (1-100).</param>
	/// <returns>A list of <see cref="DiscordSubscription" />.</returns>
	internal async Task<IReadOnlyList<DiscordSubscription>> GetSkuSubscriptionsAsync(ulong skuId, ulong userId, ulong? before = null, ulong? after = null, int limit = 100)
	{
		var urlParams = new Dictionary<string, string>();
		urlParams["userId"] = userId.ToString();
		if (before.HasValue)
			urlParams["before"] = before.Value.ToString();
		if (after.HasValue)
			urlParams["after"] = after.Value.ToString();
		if (after.HasValue)
			urlParams["after"] = after.Value.ToString();

		var route = $"{Endpoints.SKUS}/:sku_id{Endpoints.SUBSCRIPTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			sku_id = skuId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, urlParams.Count != 0 ? BuildQueryString(urlParams) : "", this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return new ReadOnlyCollection<DiscordSubscription>(DiscordJson.DeserializeIEnumerableObject<List<DiscordSubscription>>(res.Response, this.Discord));
	}

	/// <summary>
	///     Gets a subscription of an sku.
	/// </summary>
	/// <param name="skuId">The sku id to fetch the subscription for.</param>
	/// <param name="subscriptionId">The subscription id to fetch.</param>
	/// <returns>The requested <see cref="DiscordSubscription" />.</returns>
	internal async Task<DiscordSubscription?> GetSkuSubscriptionAsync(ulong skuId, ulong subscriptionId)
	{
		var route = $"{Endpoints.SKUS}/:sku_id{Endpoints.SUBSCRIPTIONS}/:subscription_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			sku_id = skuId,
			subscription_id = subscriptionId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordSubscription>(res.Response, this.Discord);
	}

	/// <summary>
	///     Creates a test entitlement.
	/// </summary>
	/// <param name="applicationId">The application id to create the entitlement for.</param>
	/// <param name="skuId">The sku id to create the entitlement for.</param>
	/// <param name="ownerId">The owner id to create the entitlement for.</param>
	/// <param name="ownerType">The owner type to create the entitlement for.</param>
	/// <returns>A partial <see cref="DiscordEntitlement" />.</returns>
	internal async Task<DiscordEntitlement> CreateTestEntitlementsAsync(ulong applicationId, ulong skuId, ulong ownerId, EntitlementOwnerType ownerType)
	{
		TestEntitlementCreatePayload pld = new()
		{
			SkuId = skuId,
			OwnerId = ownerId,
			OwnerType = ownerType
		};

		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.ENTITLEMENTS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordEntitlement>(res.Response, this.Discord);
	}

	/// <summary>
	///     Deletes a test entitlement.
	/// </summary>
	/// <param name="applicationId">The application id to delete the entitlement for.</param>
	/// <param name="entitlementId">The entitlement id to delete.</param>
	internal async Task DeleteTestEntitlementsAsync(ulong applicationId, ulong entitlementId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.ENTITLEMENTS}/:entitlement_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new
		{
			application_id = applicationId,
			entitlement_id = entitlementId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets the current application info via oauth2.
	/// </summary>
	internal Task<TransportApplication> GetCurrentApplicationOauth2InfoAsync()
		=> this.GetApplicationOauth2InfoAsync(Endpoints.ME);

	/// <summary>
	///     Gets the application rpc info.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>
	internal Task<DiscordRpcApplication> GetApplicationRpcInfoAsync(ulong applicationId)
		=> this.GetApplicationRpcInfoAsync(applicationId.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	///     Gets the application info via oauth2.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>
	private async Task<TransportApplication> GetApplicationOauth2InfoAsync(string applicationId)
	{
		var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path.Replace("//", "/"), this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var app = DiscordJson.DeserializeObject<TransportApplication>(res.Response, this.Discord);

		return app;
	}

	/// <summary>
	///     Gets the application info.
	/// </summary>
	internal async Task<TransportApplication> GetCurrentApplicationInfoAsync()
	{
		var route = $"{Endpoints.APPLICATIONS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var tapp = DiscordJson.DeserializeObject<TransportApplication>(res.Response, this.Discord);

		if (tapp.Guild.HasValue)
			tapp.Guild.Value.Discord = this.Discord;
		return tapp;
	}

	/// <summary>
	///     Gets the application info.
	/// </summary>
	internal async Task<TransportApplication> ModifyCurrentApplicationInfoAsync(
		Optional<string?> description,
		Optional<string?> interactionsEndpointUrl,
		Optional<string?> roleConnectionsVerificationUrl,
		Optional<string?> customInstallUrl,
		Optional<List<string>?> tags,
		Optional<string?> iconb64,
		Optional<string?> coverImageb64,
		Optional<ApplicationFlags> flags,
		Optional<DiscordApplicationInstallParams?> installParams,
		Optional<DiscordIntegrationTypesConfig?> integrationTypesConfig
	)
	{
		var pld = new RestApplicationModifyPayload
		{
			Description = description,
			InteractionsEndpointUrl = interactionsEndpointUrl,
			RoleConnectionsVerificationUrl = roleConnectionsVerificationUrl,
			CustomInstallUrl = customInstallUrl,
			Tags = tags,
			IconBase64 = iconb64,
			ConverImageBase64 = coverImageb64,
			Flags = flags,
			InstallParams = installParams,
			IntegrationTypesConfig = integrationTypesConfig
		};

		var route = $"{Endpoints.APPLICATIONS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var app = DiscordJson.DeserializeObject<TransportApplication>(res.Response, this.Discord);
		app.Discord = this.Discord;

		return app;
	}

	/// <summary>
	///     Gets the application info.
	/// </summary>
	/// <param name="applicationId">The application_id.</param>
	private async Task<DiscordRpcApplication> GetApplicationRpcInfoAsync(string applicationId)
	{
		var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.RPC}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = applicationId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<DiscordRpcApplication>(res.Response, this.Discord);
	}

	/// <summary>
	///     Gets the application assets async.
	/// </summary>
	/// <param name="application">The application.</param>
	internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
	{
		var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id{Endpoints.ASSETS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			application_id = application.Id
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
		foreach (var asset in assets)
		{
			asset.Discord = application.Discord;
			asset.Application = application;
		}

		return new ReadOnlyCollection<DiscordApplicationAsset>([.. assets]);
	}

	/// <summary>
	///     Gets the gateway info async.
	/// </summary>
	internal async Task<GatewayInfo> GetGatewayInfoAsync()
	{
		var headers = Utilities.GetBaseHeaders();
		var route = Endpoints.GATEWAY;
		if (this.Discord.Configuration.TokenType == TokenType.Bot)
			route += Endpoints.BOT;
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, headers).ConfigureAwait(false);

		var info = JObject.Parse(res.Response).ToObject<GatewayInfo>();
		info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.ResetAfterInternal);
		return info;
	}

#endregion

#region OAuth2

	/// <summary>
	///     Gets the current oauth2 authorization information.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	internal async Task<DiscordAuthorizationInformation> GetCurrentOAuth2AuthorizationInformationAsync(string accessToken)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.OAUTH2}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Bearer", accessToken);

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoRequestAsync(null, bucket, url, RestRequestMethod.GET, route, headers).ConfigureAwait(false);

		var oauth2Info = DiscordJson.DeserializeObject<DiscordAuthorizationInformation>(res.Response, null);
		return oauth2Info;
	}

	/// <summary>
	///     Gets the current user.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	internal async Task<DiscordUser> GetCurrentUserAsync(string accessToken)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.USERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route,
			new
			{
				application_id = this.OAuth2Client.ClientId.ToString(CultureInfo.InvariantCulture)
			}, out var path);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Bearer", accessToken);

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoRequestAsync(null, bucket, url, RestRequestMethod.GET, route, headers)
			.ConfigureAwait(false);
		var tuser = DiscordJson.DeserializeObject<TransportUser>(res.Response, null);
		return new(tuser);
	}

	/// <summary>
	///     Gets the current user's connections.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	internal async Task<IReadOnlyList<DiscordConnection>> GetCurrentUserConnectionsAsync(string accessToken)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CONNECTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Bearer", accessToken);

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoRequestAsync(null, bucket, url, RestRequestMethod.GET, route, headers)
			.ConfigureAwait(false);
		return DiscordJson.DeserializeIEnumerableObject<List<DiscordConnection>>(res.Response, null);
	}

	/// <summary>
	///     Gets the current user's guilds.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(string accessToken)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
			{ }, out var path);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Bearer", accessToken);

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoRequestAsync(null, bucket, url, RestRequestMethod.GET, route, headers)
			.ConfigureAwait(false);
		return DiscordJson.DeserializeIEnumerableObject<List<DiscordGuild>>(res.Response, null);
	}

	/// <summary>
	///     Gets the current user's guilds.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	/// <param name="guildId">The guild id to get the member for.</param>
	internal async Task<DiscordMember> GetCurrentUserGuildMemberAsync(string accessToken, ulong guildId)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId.ToString(CultureInfo.InvariantCulture)
		}, out var path);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Bearer", accessToken);

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoRequestAsync(null, bucket, url, RestRequestMethod.GET, route, headers)
			.ConfigureAwait(false);
		var tmember = DiscordJson.DeserializeObject<TransportMember>(res.Response, null);
		return new(tmember);
	}

	/// <summary>
	///     Gets the current user's role connection.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	internal async Task<DiscordApplicationRoleConnection> GetCurrentUserApplicationRoleConnectionAsync(string accessToken)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.APPLICATIONS}/:application_id{Endpoints.ROLE_CONNECTION}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route,
			new
			{
				application_id = this.OAuth2Client.ClientId.ToString(CultureInfo.InvariantCulture)
			}, out var path);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Bearer", accessToken);

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoRequestAsync(null, bucket, url, RestRequestMethod.GET, route, headers)
			.ConfigureAwait(false);
		return DiscordJson.DeserializeObject<DiscordApplicationRoleConnection>(res.Response, null);
	}

	/// <summary>
	///     Updates the current user's role connection.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	/// <param name="platformName">The platform name.</param>
	/// <param name="platformUsername">The platform username.</param>
	/// <param name="metadata">The metadata.</param>
	internal async Task<DiscordApplicationRoleConnection> ModifyCurrentUserApplicationRoleConnectionAsync(string accessToken, Optional<string> platformName, Optional<string> platformUsername, Optional<ApplicationRoleConnectionMetadata> metadata)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		RestOAuth2ApplicationRoleConnectionPayload pld = new()
		{
			PlatformName = platformName,
			PlatformUsername = platformUsername,
			Metadata = metadata.HasValue ? metadata.Value.GetKeyValuePairs() : Optional<Dictionary<string, string>>.None
		};

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.APPLICATIONS}/:application_id{Endpoints.ROLE_CONNECTION}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new
		{
			application_id = this.OAuth2Client.ClientId.ToString(CultureInfo.InvariantCulture)
		}, out var path);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Bearer", accessToken);

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoRequestAsync(null, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);
		return DiscordJson.DeserializeObject<DiscordApplicationRoleConnection>(res.Response, null);
	}

	/// <summary>
	///     Exchanges a code for an access token.
	/// </summary>
	/// <param name="code">The code.</param>
	internal async Task<DiscordAccessToken> ExchangeOAuth2AccessTokenAsync(string code)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.OAUTH2}{Endpoints.TOKEN}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);

		var formData = new Dictionary<string, string>
		{
			{ "client_id", this.OAuth2Client.ClientId.ToString(CultureInfo.InvariantCulture) },
			{ "client_secret", this.OAuth2Client.ClientSecret },
			{ "grant_type", "authorization_code" },
			{ "code", code },
			{ "redirect_uri", this.OAuth2Client.RedirectUri.AbsoluteUri }
		};

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoFormRequestAsync(this.OAuth2Client, bucket, url, RestRequestMethod.POST, route, formData).ConfigureAwait(false);

		var accessTokenInformation = DiscordJson.DeserializeObject<DiscordAccessToken>(res.Response, null);
		return accessTokenInformation;
	}

	/// <summary>
	///     Exchanges a refresh token for a new access token
	/// </summary>
	/// <param name="refreshToken">The refresh token.</param>
	internal async Task<DiscordAccessToken> RefreshOAuth2AccessTokenAsync(string refreshToken)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.OAUTH2}{Endpoints.TOKEN}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);

		var formData = new Dictionary<string, string>
		{
			{ "client_id", this.OAuth2Client.ClientId.ToString(CultureInfo.InvariantCulture) },
			{ "client_secret", this.OAuth2Client.ClientSecret },
			{ "grant_type", "refresh_token" },
			{ "refresh_token", refreshToken },
			{ "redirect_uri", this.OAuth2Client.RedirectUri.AbsoluteUri }
		};

		var url = Utilities.GetApiUriFor(path);
		var res = await this.DoFormRequestAsync(this.OAuth2Client, bucket, url, RestRequestMethod.POST, route, formData).ConfigureAwait(false);

		var accessTokenInformation = DiscordJson.DeserializeObject<DiscordAccessToken>(res.Response, null);
		return accessTokenInformation;
	}

	/// <summary>
	///     Revokes an oauth2 token.
	/// </summary>
	/// <param name="token">The token to revoke.</param>
	/// <param name="type">The type of token to revoke.</param>
	internal async Task RevokeOAuth2TokenAsync(string token, string type)
	{
		if (this.Discord != null!)
			throw new InvalidOperationException("Cannot use oauth2 endpoints with discord client");

		// ReSharper disable once HeuristicUnreachableCode
		var route = $"{Endpoints.OAUTH2}{Endpoints.TOKEN}{Endpoints.REVOKE}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);

		var authorizationString = Encoding.UTF8.GetBytes($"{this.OAuth2Client.ClientId.ToString(CultureInfo.InvariantCulture)}:{this.OAuth2Client.ClientSecret}");
		var base64EncodedAuthorizationString = Convert.ToBase64String(authorizationString);

		var headers = Utilities.GetBaseHeaders();
		headers.Add("Basic", base64EncodedAuthorizationString);

		var formData = new Dictionary<string, string>
		{
			{ "token", token },
			{ "token_type_hint", type }
		};

		var url = Utilities.GetApiUriFor(path);
		await this.DoFormRequestAsync(this.OAuth2Client, bucket, url, RestRequestMethod.POST, route, formData, headers).ConfigureAwait(false);
	}

#endregion
}
