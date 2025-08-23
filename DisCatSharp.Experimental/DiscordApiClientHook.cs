using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Experimental.Entities;
using DisCatSharp.Experimental.Payloads;
using DisCatSharp.Net;
using DisCatSharp.Net.Serialization;

namespace DisCatSharp.Experimental;

/// <summary>
///     Represents a hook for the discord api client.
/// </summary>
internal sealed class DiscordApiClientHook
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordApiClientHook" /> class.
	/// </summary>
	/// <param name="apiClient">The api client.</param>
	internal DiscordApiClientHook(DiscordApiClient apiClient)
	{
		this.ApiClient = apiClient;
	}

	/// <summary>
	///     Gets the api client.
	/// </summary>
	internal DiscordApiClient ApiClient { get; set; }

	// TODO: Implement
	/// <summary>
	///    Searches the guild messages asynchronously based on the provided search parameters.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="searchParams">The search parameters.</param>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotIndexedException">Thrown if the elastisearch endpoint has not finished indexing yet.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <returns>A list of messages that match the search criteria.</returns>
	internal async Task<DiscordSearchGuildMessagesResponse?> SearchGuildMessagesAsync(ulong guildId, DiscordGuildMessageSearchParams searchParams)
	{
		await Task.Delay(1000);
		return null;
	}

	/// <summary>
	///     Searches the guild members asynchronously based on the provided search parameters.
	/// </summary>
	/// <param name="guildId">The ID of the guild.</param>
	/// <param name="searchParams">The search parameters.</param>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotIndexedException">Thrown if the elastisearch endpoint has not finished indexing yet.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <returns>A list of supplemental guild members that match the search criteria.</returns>
	internal async Task<DiscordSearchGuildMembersResponse> SearchGuildMembersAsync(ulong guildId, DiscordGuildMemberSearchParams searchParams)
	{
		var (isValid, errorMessage) = searchParams.Validate();
		if (!isValid)
			throw new ValidationException(
				typeof(DiscordGuildMemberSearchParams),
				"DiscordGuild.SearchGuildMembersAsync(DiscordGuildMemberSearchParams searchParams)",
				errorMessage!
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
					old.Collectibles = usr.Collectibles;
					old.IsSystem = usr.IsSystem;
					old.IsBot = usr.IsBot;
					old.ThemeColorsInternal = usr.ThemeColorsInternal;
					old.Pronouns = usr.Pronouns;
					old.Locale = usr.Locale;
					old.GlobalName = usr.GlobalName;
					old.PrimaryGuild = usr.PrimaryGuild;
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

	/// <summary>
	///    Requests a file upload to GCP.
	/// </summary>
	/// <param name="channelId">The ID of the channel.</param>
	/// <param name="attachment">The attachment information.</param>
	/// <returns>The GCP attachment response.</returns>
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

	/// <summary>
	///   Uploads a file to GCP based on the given <paramref name="target" /> information.
	/// </summary>
	/// <param name="target">The target upload information.</param>
	/// <param name="file">The file stream to upload.</param>
	internal void UploadGcpFile(GcpAttachmentUploadInformation target, Stream file)
	{
		HttpRequestMessage request = new(HttpMethod.Put, target.UploadUrl)
		{
			Content = new StreamContent(file)
		};
		this.ApiClient.Rest.HttpClient.Send(request);
	}
}
