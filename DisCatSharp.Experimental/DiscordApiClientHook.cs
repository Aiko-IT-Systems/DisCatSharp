using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
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
		/*
		// TODO: Implement proper validation for message search params.
		var (isValid, errorMessage) = searchParams.Validate();
		if (!isValid)
			throw new ValidationException(
				typeof(DiscordGuildMessageSearchParams),
				"DiscordGuild.SearchMessagesAsync(DiscordGuildMessageSearchParams searchParams)",
				errorMessage!
			);*/

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MESSAGES}{Endpoints.SEARCH}";

		var bucket = this.ApiClient.Rest.GetBucket(RestRequestMethod.GET, route, new
		{
			guild_id = guildId
		}, out var path);

		var url = Utilities.GetApiUriFor(path, BuildGuildMessageSearchQueryString(searchParams), this.ApiClient.Discord.Configuration);

		var res = await this.ApiClient.DoRequestAsync(this.ApiClient.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		if (res.ResponseCode is HttpStatusCode.Accepted)
			throw new NotIndexedException(res);
		var result = DiscordJson.DeserializeObject<DiscordSearchGuildMessagesResponse>(res.Response, this.ApiClient.Discord);
		if (result is not null)
		{
			if (result.Messages.Count is not 0)
			{
				foreach(var msgs in result.Messages)
					if (msgs.Count is not 0)
						foreach(var msg in msgs)
							msg.Discord = this.ApiClient.Discord;
				foreach (var thread in result.Threads)
					thread.Discord = this.ApiClient.Discord;
				foreach (var member in result.Members)
					member.Discord = this.ApiClient.Discord;
			}
		}
		return result;
	}

	/// <summary>
	///     Builds the query string for the guild message search endpoint.
	/// </summary>
	/// <param name="searchParams">The search parameters.</param>
	/// <returns>The encoded query string, including the leading <c>?</c> when needed.</returns>
	private static string BuildGuildMessageSearchQueryString(DiscordGuildMessageSearchParams searchParams)
	{
		ArgumentNullException.ThrowIfNull(searchParams);

		var query = new StringBuilder();

		AppendQueryParam(query, "sort_by", searchParams.SortBy);
		AppendQueryParam(query, "sort_order", searchParams.SortOrder);
		AppendQueryParam(query, "content", searchParams.Content);
		AppendQueryParams(query, "contents", searchParams.Contents);
		AppendQueryParam(query, "slop", searchParams.Slop);
		AppendQueryParams(query, "author_id", searchParams.AuthorIds);
		AppendQueryParams(query, "author_type", searchParams.AuthorTypes);
		AppendQueryParams(query, "mentions", searchParams.Mentions);
		AppendQueryParam(query, "mention_everyone", searchParams.MentionEveryone);
		AppendQueryParam(query, "min_id", searchParams.MinId);
		AppendQueryParam(query, "max_id", searchParams.MaxId);
		AppendQueryParam(query, "limit", searchParams.Limit);
		AppendQueryParam(query, "offset", searchParams.Offset);
		AppendQueryParams(query, "has", searchParams.Has);
		AppendQueryParams(query, "link_hostname", searchParams.LinkHostnames);
		AppendQueryParams(query, "embed_provider", searchParams.EmbedProviders);
		AppendQueryParams(query, "embed_type", searchParams.EmbedTypes);
		AppendQueryParams(query, "attachment_extension", searchParams.AttachmentExtensions);
		AppendQueryParam(query, "attachment_filename", searchParams.AttachmentFilename);
		AppendQueryParam(query, "pinned", searchParams.Pinned);
		AppendQueryParam(query, "command_id", searchParams.CommandId);
		AppendQueryParam(query, "command_name", searchParams.CommandName);
		AppendQueryParam(query, "include_nsfw", searchParams.IncludeNsfw);
		AppendQueryParams(query, "channel_id", searchParams.ChannelIds);
		AppendQueryParam(query, "replied_to_message_id", searchParams.RepliedToMessageId);

		return query.Length is 0 ? string.Empty : $"?{query}";
	}

	/// <summary>
	///     Appends a single query parameter when it has a value.
	/// </summary>
	/// <param name="query">The query string builder.</param>
	/// <param name="name">The name of the query parameter.</param>
	/// <param name="value">The value of the query parameter. If null, the parameter will not be appended.</param>
	/// <typeparam name="T">The type of the value.</typeparam>
	private static void AppendQueryParam<T>(StringBuilder query, string name, T? value)
	{
		if (value is null)
			return;

		AppendRawQueryParam(query, name, SerializeQueryValue(value));
	}

	/// <summary>
	///     Appends repeated query parameters for a multi-value filter.
	/// </summary>
	/// <param name="query">The query string builder.</param>
	/// <param name="name">The name of the query parameter.</param>
	/// <param name="values">The values to append. If null, no parameters will be appended.</param>
	/// <typeparam name="T">The type of the values.</typeparam>
	private static void AppendQueryParams<T>(StringBuilder query, string name, IEnumerable<T>? values)
	{
		if (values is null)
			return;

		foreach (var value in values)
			AppendRawQueryParam(query, name, SerializeQueryValue(value));
	}

	/// <summary>
	///     Appends an already serialized query parameter value.
	/// </summary>
	/// <param name="query">The query string builder.</param>
	/// <param name="name">The name of the query parameter.</param>
	/// <param name="value">The raw value of the query parameter, which will be URL-encoded as-is.</param>
	private static void AppendRawQueryParam(StringBuilder query, string name, string value)
	{
		if (query.Length is not 0)
			query.Append('&');

		query.Append(WebUtility.UrlEncode(name));
		query.Append('=');
		query.Append(WebUtility.UrlEncode(value));
	}

	/// <summary>
	///     Serializes a query value to the wire format Discord expects.
	/// </summary>
	/// <param name="value">The value to serialize.</param>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <returns>The serialized value.</returns>
	private static string SerializeQueryValue<T>(T value)
	{
		return value switch
		{
			string stringValue => stringValue,
			bool boolValue => boolValue ? "true" : "false",
			Enum enumValue => GetEnumMemberValue(enumValue),
			IFormattable formattableValue => formattableValue.ToString(null, CultureInfo.InvariantCulture),
			_ => value.ToString() ?? string.Empty
		};
	}

	/// <summary>
	///     Gets the <see cref="EnumMemberAttribute.Value" /> for an enum value, if present.
	/// </summary>
	/// <param name="value">The enum value.</param>
	/// <returns>The value specified in the <see cref="EnumMemberAttribute" />, or the enum's name if the attribute is not present.</returns>
	private static string GetEnumMemberValue(Enum value)
	{
		var member = value.GetType().GetMember(value.ToString());
		if (member.Length == 0)
			return value.ToString();

		var attribute = Attribute.GetCustomAttribute(member[0], typeof(EnumMemberAttribute)) as EnumMemberAttribute;
		return attribute?.Value ?? value.ToString();
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
