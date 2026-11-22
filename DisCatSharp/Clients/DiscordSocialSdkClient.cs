using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Net;
using DisCatSharp.Net.Serialization;

namespace DisCatSharp;

/// <summary>Provides bot and OAuth access to Discord Social SDK and game-stat operations.</summary>
/// <remarks>Individual operations require either bot or user authorization. Discord may restrict access to Social SDK or game-stat features.</remarks>
public sealed partial class DiscordSocialSdkClient
{
	/// <summary>Creates a Social SDK API client backed by a bot client.</summary>
	/// <param name="client">The bot client.</param>
	public DiscordSocialSdkClient(DiscordClient client)
	{
		this._bot = client ?? throw new ArgumentNullException(nameof(client));
		this._api = client.ApiClient;
	}

	/// <summary>Creates a Social SDK client for user-authenticated operations.</summary>
	/// <param name="client">The OAuth client providing REST and rate-limit handling.</param>
	/// <param name="token">The user access token.</param>
	public DiscordSocialSdkClient(DiscordOAuth2Client client, DiscordAccessToken token)
	{
		ArgumentNullException.ThrowIfNull(client);
		ArgumentNullException.ThrowIfNull(token);
		this._oauth = client;
		this._token = token;
		this._api = client.ApiClient;
	}

	/// <summary>Gets game-stat profile data for an authorized user's external identity.</summary>
	/// <param name="applicationId">The application's ID.</param>
	/// <param name="userId">The Discord user ID.</param>
	/// <param name="externalUserId">The provider-issued user ID.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The identity profile.</returns>
	public async Task<DiscordApplicationIdentityProfile> GetProfileAsync(ulong applicationId, ulong userId, string externalUserId, CancellationToken cancellationToken = default)
	{
		var result = await this.RequestAsync(RestRequestMethod.GET, "/applications/:application_id/users/:user_id/identities/:external_id/profile", applicationId, userId, externalId: externalUserId, cancellationToken: cancellationToken).ConfigureAwait(false);
		return DiscordJson.DeserializeObject<DiscordApplicationIdentityProfile>(result.Response, this._bot);
	}

	/// <summary>Updates an authorized user's application identity profile.</summary>
	/// <remarks>When <paramref name="data"/> is supplied, it replaces the entire existing data object. Omit it to leave stats unchanged. Discord may create a profile-only identity on the first successful update.</remarks>
	/// <param name="applicationId">The application's ID.</param>
	/// <param name="userId">The Discord user ID.</param>
	/// <param name="externalUserId">The provider-issued user ID.</param>
	/// <param name="username">The optional external username.</param>
	/// <param name="data">The optional complete replacement game-stat data.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>A task that completes when Discord accepts the update. Discord returns either 201 or 204.</returns>
	public async Task UpdateProfileAsync(ulong applicationId, ulong userId, string externalUserId, Optional<string> username = default, Optional<DiscordGameProfileData> data = default, CancellationToken cancellationToken = default)
	{
		var body = new Dictionary<string, object>();
		if (username.HasValue) body["username"] = username.Value ?? throw new ArgumentNullException(nameof(username));
		if (data.HasValue)
		{
			var profileData = data.Value ?? throw new ArgumentNullException(nameof(data));
			if (profileData.Dynamic is { Count: > 30 }) throw new ArgumentOutOfRangeException(nameof(data), "Discord accepts at most 30 dynamic fields.");
			body["data"] = profileData;
		}
		await this.RequestAsync(RestRequestMethod.PATCH, "/applications/:application_id/users/:user_id/identities/:external_id/profile", applicationId, userId, externalId: externalUserId, body: body, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Lists an authorized user's application identities.</summary>
	/// <param name="applicationId">The bot application's ID.</param><param name="userId">The Discord user ID.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>The matching identities.</returns>
	public async Task<IReadOnlyList<DiscordApplicationIdentity>> GetUserIdentitiesAsync(ulong applicationId, ulong userId, CancellationToken cancellationToken = default)
	{
		var result = await this.RequestAsync(RestRequestMethod.GET, "/users/:user_id/application-identities/:application_id", applicationId, userId, cancellationToken: cancellationToken).ConfigureAwait(false);
		return DiscordJson.DeserializeObject<DiscordApplicationIdentityList>(result.Response, this._bot).Identities;
	}

	/// <summary>Finds application identities by external provider and user ID.</summary>
	/// <param name="applicationId">The bot application's ID.</param><param name="providerType">The external provider type.</param><param name="externalUserId">The provider-issued user ID.</param><param name="providerId">The optional provider-specific identifier.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>The matching identities, including Discord user IDs when supplied.</returns>
	public async Task<IReadOnlyList<DiscordApplicationIdentity>> GetIdentitiesByExternalIdAsync(ulong applicationId, string providerType, string externalUserId, string? providerId = null, CancellationToken cancellationToken = default)
	{
		var result = await this.RequestAsync(RestRequestMethod.GET, "/applications/:application_id/application-identities/:provider_type/:external_id", applicationId, providerType: providerType, externalId: externalUserId, providerId: providerId, cancellationToken: cancellationToken).ConfigureAwait(false);
		return DiscordJson.DeserializeObject<DiscordApplicationIdentityList>(result.Response, this._bot).Identities;
	}

	/// <summary>Deletes an application identity and its associated profile data.</summary>
	/// <remarks>Discord prevents deletion of a user's last account-linking identity; profile-only identities can be deleted.</remarks>
	/// <param name="applicationId">The bot application's ID.</param><param name="userId">The Discord user ID.</param><param name="providerType">The external provider type.</param><param name="externalUserId">The provider-issued user ID.</param><param name="providerId">The optional provider-specific identifier.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task representing the request.</returns>
	public async Task DeleteIdentityAsync(ulong applicationId, ulong userId, string providerType, string externalUserId, string? providerId = null, CancellationToken cancellationToken = default)
	{
		var body = providerId is null ? null : new Dictionary<string, object> { ["provider_id"] = providerId };
		await this.RequestAsync(RestRequestMethod.POST, "/users/:user_id/application-identities/:application_id/:provider_type/:external_id/delete", applicationId, userId, providerType: providerType, externalId: externalUserId, body: body, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Creates or retrieves a provisional account token using the bot token.</summary>
	/// <remarks>Keep the bot token on a trusted backend. The returned access and identity tokens are secrets and must not be logged.</remarks>
	/// <param name="externalUserId">The user ID in the application's own account system.</param><param name="preferredGlobalName">An optional preferred display name.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>The issued provisional token; no refresh token is guaranteed.</returns>
	public async Task<DiscordProvisionalToken> GetProvisionalTokenAsync(string externalUserId, string? preferredGlobalName = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(externalUserId);
		var body = new Dictionary<string, object> { ["external_user_id"] = externalUserId };
		if (preferredGlobalName is not null) body["preferred_global_name"] = preferredGlobalName;
		var result = await this.RequestAsync(RestRequestMethod.POST, "/partner-sdk/token/bot", body: body, cancellationToken: cancellationToken).ConfigureAwait(false);
		// Token responses must not enter missing-field diagnostics, which can log the raw JSON.
		return DiscordJson.DeserializeObject<DiscordProvisionalToken>(result.Response, null);
	}

	/// <summary>Unmerges a bot-issued provisional account from a Discord account.</summary>
	/// <param name="externalUserId">The same external user ID used for bot-token issuance.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task representing the request.</returns>
	public async Task UnmergeProvisionalAccountAsync(string externalUserId, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(externalUserId);
		await this.RequestAsync(RestRequestMethod.POST, "/partner-sdk/provisional-accounts/unmerge/bot", body: new { external_user_id = externalUserId }, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Updates application-scoped moderation metadata on a Social SDK direct message.</summary>
	/// <param name="firstUserId">One direct-message participant.</param><param name="secondUserId">The other participant; their order does not matter.</param><param name="messageId">The message ID.</param><param name="metadata">The moderation metadata to send.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task representing the request.</returns>
	public async Task UpdateDirectMessageModerationAsync(ulong firstUserId, ulong secondUserId, ulong messageId, IReadOnlyDictionary<string, string> metadata, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(metadata);
		if (metadata.Count > 5 || metadata.Any(pair => pair.Key.Length > 1024 || pair.Value is null || pair.Value.Length > 2000))
			throw new ArgumentException("Discord allows at most five metadata entries, with keys up to 1024 and values up to 2000 characters.", nameof(metadata));
		await this.RequestAsync(RestRequestMethod.PUT, "/partner-sdk/dms/:user_id/:other_user_id/messages/:message_id/moderation-metadata", userId: firstUserId, otherUserId: secondUserId, messageId: messageId, body: metadata, cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	/// <summary>Encodes a required external path segment without allowing dot-segment normalization.</summary>
	/// <param name="value">The provider value.</param><param name="parameterName">The originating parameter name.</param><returns>The encoded path segment.</returns>
	private static string EncodeSegment(string value, string parameterName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
		if (value is "." or "..") throw new ArgumentException("Dot segments cannot be used as path identifiers.", parameterName);
		return Uri.EscapeDataString(value);
	}

	/// <summary>Sends a bot-authenticated request through the shared rate-limit transport.</summary>
	/// <param name="method">The HTTP method.</param><param name="route">The route template.</param><param name="applicationId">The application ID.</param><param name="userId">The first user ID.</param><param name="otherUserId">The second user ID.</param><param name="messageId">The message ID.</param><param name="providerType">The provider type, if used in the route.</param><param name="externalId">The provider-issued ID, if used in the route.</param><param name="providerId">The optional provider ID query parameter.</param><param name="body">The request body, when present.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>The REST response.</returns>
	private Task<RestResponse> RequestAsync(RestRequestMethod method, string route, ulong applicationId = 0, ulong userId = 0, ulong otherUserId = 0, ulong messageId = 0, string? providerType = null, string? externalId = null, string? providerId = null, object? body = null, CancellationToken cancellationToken = default)
	{
		if (this._bot is null) throw new InvalidOperationException("This Social SDK operation requires a bot client.");
		var parameters = new
		{
			application_id = applicationId,
			user_id = userId,
			other_user_id = otherUserId,
			message_id = messageId,
			provider_type = providerType is null ? string.Empty : EncodeSegment(providerType, nameof(providerType)),
			external_id = externalId is null ? string.Empty : EncodeSegment(externalId, nameof(externalId))
		};
		if (route.Contains(":external_id", StringComparison.Ordinal) && externalId is null) throw new ArgumentNullException(nameof(externalId));
		if (route.Contains(":provider_type", StringComparison.Ordinal) && providerType is null) throw new ArgumentNullException(nameof(providerType));
		var bucket = this._bot.ApiClient.Rest.GetBucket(method, route, parameters, out var path);
		var url = Utilities.GetApiUriBuilderFor(path, this._bot.Configuration);
		if (providerId is not null) url.AddParameter("provider_id", providerId);
		return this._bot.ApiClient.DoRequestAsync(this._bot, bucket, url.Build(), method, route, payload: body is null ? null : DiscordJson.SerializeObject(body), cancellationToken: cancellationToken);
	}
}
