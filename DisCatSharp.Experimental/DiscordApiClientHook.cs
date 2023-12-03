using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
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
}
