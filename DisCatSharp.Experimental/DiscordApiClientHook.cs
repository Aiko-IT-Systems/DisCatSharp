using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Experimental.Entities;
using DisCatSharp.Experimental.Payloads;
using DisCatSharp.Net;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Experimental;

/// <summary>
/// Represents a hook for the discord api client.
/// </summary>
internal sealed class DiscordApiClientHook
{
	/// <summary>
	/// Gets the api client.
	/// </summary>
	internal DiscordApiClient ApiClient { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApiClientHook"/> class.
	/// </summary>
	/// <param name="apiClient">The api client.</param>
	internal DiscordApiClientHook(DiscordApiClient apiClient)
	{
		this.ApiClient = apiClient;
	}

	/// <summary>
	/// Gets the clyde profile for the given <paramref name="profileId"/>.
	/// </summary>
	/// <param name="profileId">The profile id to get.</param>
	[DiscordDeprecated]
	internal async Task<ClydeProfile> GetClydeProfileAsync(ulong profileId)
	{
		var route = $"{Endpoints.CLYDE_PROFILES}/:profile_id";
		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			profile_id = profileId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.ApiClient.Discord.Configuration);
		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var profile = DiscordJson.DeserializeObject<ClydeProfile>(res.Response, this.ApiClient.Discord);
		return profile;
	}

	/// <summary>
	/// Gets the clyde settings for the given <paramref name="guildId"/>.
	/// </summary>
	/// <param name="guildId">The guild id to get clyde's settings for.</param>
	[DiscordDeprecated]
	internal async Task<ClydeSettings> GetClydeSettingsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CLYDE_SETTINGS}";
		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.ApiClient.Discord.Configuration);
		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var settings = DiscordJson.DeserializeObject<ClydeSettings>(res.Response, this.ApiClient.Discord);
		return settings;
	}

	/// <summary>
	/// Modifies the clyde settings for the given <paramref name="guildId"/> by applying a <paramref name="profileId"/>.
	/// </summary>
	/// <param name="guildId">The guild id to modify clyde's settings for.</param>
	/// <param name="profileId">The profile id to apply.</param>
	[DiscordDeprecated]
	internal async Task<ClydeSettings> ModifyClydeSettingsAsync(ulong guildId, ulong profileId)
	{
		ClydeSettingsProfileIdOnlyUpdatePayload pld = new()
		{
			ClydeProfileId = profileId
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CLYDE_SETTINGS}";
		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.ApiClient.Discord.Configuration);
		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var obj = JObject.Parse(res.Response);
		var settingsString = obj.GetValue("settings")!.ToString();
		var settings = DiscordJson.DeserializeObject<ClydeSettings>(settingsString, this.ApiClient.Discord);
		return settings;
	}

	/// <summary>
	/// Modifies the clyde settings for the given <paramref name="guildId"/>.
	/// </summary>
	/// <param name="guildId">The guild id to modify clyde's settings for.</param>
	/// <param name="name">The new name.</param>
	/// <param name="personality">The new basePersonality.</param>
	/// <param name="avatarBase64">The new avatar.</param>
	/// <param name="bannerBase64">The new banner.</param>
	/// <param name="themeColors">The new theme colors.</param>
	[DiscordDeprecated]
	internal async Task<ClydeSettings> ModifyClydeSettingsAsync(
		ulong guildId,
		Optional<string?> name,
		Optional<string> personality,
		Optional<string?> avatarBase64,
		Optional<string?> bannerBase64,
		Optional<List<int>?> themeColors
	)
	{
		ClydeSettingsProfileUpdatePayload pld = new()
		{
			Nick = name,
			Personality = personality,
			Avatar = avatarBase64,
			Banner = bannerBase64,
			ThemeColors = themeColors
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CLYDE_SETTINGS}";
		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.PATCH, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.ApiClient.Discord.Configuration);
		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var obj = JObject.Parse(res.Response);
		var settingsString = obj.GetValue("settings")!.ToString();
		var settings = DiscordJson.DeserializeObject<ClydeSettings>(settingsString, this.ApiClient.Discord);
		return settings;
	}

	/// <summary>
	/// Generates a basePersonality for clyde based on the given <paramref name="basePersonality"/>.
	/// </summary>
	/// <param name="basePersonality">The base base personality to generate a new one from.</param>
	[DiscordDeprecated]
	internal async Task<string> GenerateClydePersonalityAsync(string? basePersonality = null)
	{
		PersonalityGenerationPayload pld = new()
		{
			Personality = basePersonality ?? string.Empty
		};

		var route = $"{Endpoints.CLYDE_PROFILES}{Endpoints.GENERATE_PERSONALITY}";
		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.POST, route, new
			{ }, out var path);

		var url = Utilities.GetApiUriFor(path, this.ApiClient.Discord.Configuration);
		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var generatedPersonality = DiscordJson.DeserializeObject<PersonalityGenerationPayload>(res.Response, this.ApiClient.Discord);
		return generatedPersonality.Personality;
	}

	/// <summary>
	/// Searches the guild members asynchronously based on the provided search parameters.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="searchParams">The search parameters.</param>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotIndexedException">Thrown if the elastisearch endpoint has not finished indexing yet.</exception>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild" /> permission.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <returns>A list of supplemental guild members that match the search criteria.</returns>
	internal async Task<DiscordSearchGuildMembersResponse> SearchGuildMembersAsync(ulong guildId, DiscordGuildMemberSearchParams searchParams)
	{
		var validationResult = searchParams.Validate();
		if (!validationResult.IsValid)
			throw new ValidationException(
				typeof(DiscordGuildMemberSearchParams),
				"DiscordGuild.SearchGuildMembersAsync(DiscordGuildMemberSearchParams searchParams)",
				validationResult.ErrorMessage!
			);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS_SEARCH}";

		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			guild_id = guildId
		}, out var path);

		var pld = DiscordJson.SerializeObject(searchParams);

		var url = Utilities.GetApiUriFor(path, this.ApiClient.Discord.Configuration);

		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.POST, route, payload: pld).ConfigureAwait(false);

		if (res.ResponseCode is HttpStatusCode.Accepted)
			throw new NotIndexedException(res);

		var searchResponse = DiscordJson.DeserializeObject<DiscordSearchGuildMembersResponse>(res.Response, this.ApiClient.Discord);

		foreach (var supplementalMember in searchResponse.Members)
		{
			supplementalMember.Discord = this.ApiClient.Discord;

			if (supplementalMember.TransportMember.User is not null)
			{
				var usr = new DiscordUser(supplementalMember.TransportMember.User)
				{
					Discord = this.ApiClient.Discord
				};

				this.ApiClient.Discord.UserCache.AddOrUpdate(supplementalMember.TransportMember.User.Id, usr, (id, old) =>
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
					return old;
				});
			}

			supplementalMember.Member = new(supplementalMember.TransportMember)
			{
				Discord = this.ApiClient.Discord,
				GuildId = guildId
			};
		}

		return searchResponse;
	}

	internal async Task<GcpAttachmentsResponse> RequestFileUploadAsync(ulong channelId, GcpAttachment attachment)
	{
		var pld = new RestGcpAttachmentsPayload
		{
			GcpAttachments = [attachment]
		};

		var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.ATTACHMENTS}";
		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.POST, route, new
		{
			channel_id = channelId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, this.ApiClient.Discord.Configuration);
		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		return DiscordJson.DeserializeObject<GcpAttachmentsResponse>(res.Response, this.ApiClient.Discord);
	}

	internal void UploadGcpFile(GcpAttachmentUploadInformation target, Stream file)
	{
		HttpRequestMessage request = new(HttpMethod.Put, target.UploadUrl)
		{
			Content = new StreamContent(file)
		};
		this.ApiClient.Rest.HttpClient.Send(request);
	}
}
