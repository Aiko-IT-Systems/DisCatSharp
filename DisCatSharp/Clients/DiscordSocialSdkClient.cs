using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Serialization;

namespace DisCatSharp;

/// <summary>Provides bot and OAuth access to Discord Social SDK and game-stat operations.</summary>
/// <remarks>Individual operations require either bot or user authorization. Discord may restrict access to Social SDK or game-stat features.</remarks>
public sealed class DiscordSocialSdkClient
{
	/// <summary>The REST client shared with the originating Discord client.</summary>
	private readonly DiscordApiClient _api;
	/// <summary>The bot client, when using bot authentication.</summary>
	private readonly DiscordClient? _bot;
	/// <summary>The OAuth client, when using bearer authentication.</summary>
	private readonly DiscordOAuth2Client? _oauth;
	/// <summary>The bearer token, when using OAuth authentication.</summary>
	private readonly DiscordAccessToken? _token;

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

	/// <summary>Creates a bot-managed lobby with at most 25 initial members.</summary>
	/// <param name="metadata">Optional lobby metadata.</param>
	/// <param name="members">Optional initial members.</param>
	/// <param name="idleTimeoutSeconds">Optional idle timeout in seconds.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created lobby.</returns>
	public Task<DiscordLobby> CreateLobbyAsync(Dictionary<string, string>? metadata = null, IEnumerable<DiscordLobbyMemberUpdate>? members = null, int? idleTimeoutSeconds = null, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		var memberArray = ValidateMembers(members, false);
		var body = new Dictionary<string, object>();
		if (metadata is not null) body["metadata"] = metadata;
		if (memberArray is not null) body["members"] = memberArray;
		if (idleTimeoutSeconds.HasValue) body["idle_timeout_seconds"] = idleTimeoutSeconds.Value;
		return this.RequestObjectAsync<DiscordLobby>(RestRequestMethod.POST, "/lobbies", null, null, body, cancellationToken);
	}

	/// <summary>Gets a bot-managed lobby.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>The lobby.</returns>
	public Task<DiscordLobby> GetLobbyAsync(ulong lobbyId, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		return this.RequestObjectAsync<DiscordLobby>(RestRequestMethod.GET, "/lobbies/:lobby_id", lobbyId, null, null, cancellationToken);
	}

	/// <summary>Replaces supplied lobby fields; a supplied members list replaces the entire membership.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="metadata">Optional replacement metadata; an explicit null clears it.</param>
	/// <param name="members">Optional replacement members, at most 25.</param><param name="idleTimeoutSeconds">Optional idle timeout in seconds.</param>
	/// <param name="cancellationToken">A cancellation token.</param><returns>The updated lobby.</returns>
	public Task<DiscordLobby> ModifyLobbyAsync(ulong lobbyId, Optional<Dictionary<string, string>?> metadata = default, IEnumerable<DiscordLobbyMemberUpdate>? members = null, int? idleTimeoutSeconds = null, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		var body = new Dictionary<string, object?>();
		if (metadata.HasValue) body["metadata"] = metadata.Value;
		if (members is not null) body["members"] = ValidateMembers(members, false)!;
		if (idleTimeoutSeconds.HasValue) body["idle_timeout_seconds"] = idleTimeoutSeconds.Value;
		return this.RequestObjectAsync<DiscordLobby>(RestRequestMethod.PATCH, "/lobbies/:lobby_id", lobbyId, null, body, cancellationToken);
	}

	/// <summary>Deletes a bot-managed lobby.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="cancellationToken">A cancellation token.</param><returns>A task representing the request.</returns>
	public Task DeleteLobbyAsync(ulong lobbyId, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		return this.RequestAsync(RestRequestMethod.DELETE, "/lobbies/:lobby_id", lobbyId, null, null, cancellationToken);
	}

	/// <summary>Adds or updates one member in a bot-managed lobby.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="member">The member and fields to update.</param><param name="cancellationToken">A cancellation token.</param><returns>The lobby member.</returns>
	public Task<DiscordLobbyMember> UpsertLobbyMemberAsync(ulong lobbyId, DiscordLobbyMemberUpdate member, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		ArgumentNullException.ThrowIfNull(member);
		var body = new Dictionary<string, object?>();
		if (member.Metadata.HasValue) body["metadata"] = member.Metadata.Value;
		if (member.Flags.HasValue) body["flags"] = member.Flags.Value;
		if (member.AdditionalName.HasValue) body["additional_name"] = member.AdditionalName.Value;
		return this.RequestObjectAsync<DiscordLobbyMember>(RestRequestMethod.PUT, "/lobbies/:lobby_id/members/:user_id", lobbyId, member.Id, body, cancellationToken);
	}

	/// <summary>Adds, updates, or removes one to 25 members in one request.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="members">The member updates.</param><param name="cancellationToken">A cancellation token.</param><returns>The members upserted by Discord.</returns>
	public async Task<IReadOnlyList<DiscordLobbyMember>> UpdateLobbyMembersAsync(ulong lobbyId, IEnumerable<DiscordLobbyMemberUpdate> members, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		var response = await this.RequestAsync(RestRequestMethod.POST, "/lobbies/:lobby_id/members/bulk", lobbyId, null, ValidateMembers(members, true), cancellationToken).ConfigureAwait(false);
		return DiscordJson.DeserializeIEnumerableObject<List<DiscordLobbyMember>>(response.Response, this._bot);
	}

	/// <summary>Removes a member from a bot-managed lobby.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="userId">The user's ID.</param><param name="cancellationToken">A cancellation token.</param><returns>A task representing the request.</returns>
	public Task RemoveLobbyMemberAsync(ulong lobbyId, ulong userId, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		return this.RequestAsync(RestRequestMethod.DELETE, "/lobbies/:lobby_id/members/:user_id", lobbyId, userId, null, cancellationToken);
	}

	/// <summary>Creates a single-use linked-channel invite for a specified user using bot authentication.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="userId">The target user ID.</param><param name="cancellationToken">A cancellation token.</param><returns>The invite.</returns>
	public Task<DiscordLobbyInvite> CreateLobbyUserInviteAsync(ulong lobbyId, ulong userId, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		return this.RequestObjectAsync<DiscordLobbyInvite>(RestRequestMethod.POST, "/lobbies/:lobby_id/members/:user_id/invites", lobbyId, userId, null, cancellationToken);
	}

	/// <summary>Updates application-scoped moderation metadata on a lobby message.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="messageId">The message ID.</param><param name="metadata">The moderation metadata.</param><param name="cancellationToken">A cancellation token.</param><returns>A task representing the request.</returns>
	public Task UpdateLobbyMessageModerationAsync(ulong lobbyId, ulong messageId, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
	{
		this.RequireBot();
		ArgumentNullException.ThrowIfNull(metadata);
		return this.RequestAsync(RestRequestMethod.PUT, "/lobbies/:lobby_id/messages/:user_id/moderation-metadata", lobbyId, messageId, metadata, cancellationToken);
	}

	/// <summary>Creates or joins a lobby using the current user's social-layer bearer token.</summary>
	/// <param name="secret">The application-scoped lobby secret.</param><param name="lobbyMetadata">Optional lobby metadata.</param><param name="memberMetadata">Optional member metadata.</param><param name="idleTimeoutSeconds">Optional idle timeout.</param><param name="cancellationToken">A cancellation token.</param><returns>The joined lobby.</returns>
	public Task<DiscordLobby> CreateOrJoinLobbyAsync(string secret, Dictionary<string, string>? lobbyMetadata = null, Dictionary<string, string>? memberMetadata = null, int? idleTimeoutSeconds = null, CancellationToken cancellationToken = default)
	{
		this.RequireBearer();
		ArgumentException.ThrowIfNullOrEmpty(secret);
		if (secret.Length > 250) throw new ArgumentOutOfRangeException(nameof(secret));
		var body = new Dictionary<string, object> { ["secret"] = secret };
		if (lobbyMetadata is not null) body["lobby_metadata"] = lobbyMetadata;
		if (memberMetadata is not null) body["member_metadata"] = memberMetadata;
		if (idleTimeoutSeconds.HasValue) body["idle_timeout_seconds"] = idleTimeoutSeconds.Value;
		return this.RequestObjectAsync<DiscordLobby>(RestRequestMethod.PUT, "/lobbies", null, null, body, cancellationToken);
	}

	/// <summary>Removes the current OAuth user from a lobby.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="cancellationToken">A cancellation token.</param><returns>A task representing the request.</returns>
	public Task LeaveLobbyAsync(ulong lobbyId, CancellationToken cancellationToken = default)
	{
		this.RequireBearer(false);
		return this.RequestAsync(RestRequestMethod.DELETE, "/lobbies/:lobby_id/members/@me", lobbyId, null, null, cancellationToken);
	}

	/// <summary>Links or unlinks a guild channel using a lobby member's bearer token.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="channelId">The channel to link, or null to unlink.</param><param name="cancellationToken">A cancellation token.</param><returns>The updated lobby.</returns>
	public Task<DiscordLobby> LinkLobbyChannelAsync(ulong lobbyId, ulong? channelId, CancellationToken cancellationToken = default)
	{
		this.RequireBearer(false);
		var body = new Dictionary<string, object>();
		if (channelId.HasValue) body["channel_id"] = channelId.Value;
		return this.RequestObjectAsync<DiscordLobby>(RestRequestMethod.PATCH, "/lobbies/:lobby_id/channel-linking", lobbyId, null, body, cancellationToken);
	}

	/// <summary>Sends a lobby message as the current OAuth user.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="content">The nonempty message content.</param><param name="metadata">Optional dispatch-only metadata.</param><param name="flags">Optional Social SDK message flags.</param><param name="cancellationToken">A cancellation token.</param><returns>The created lobby message.</returns>
	public Task<DiscordLobbyMessage> SendLobbyMessageAsync(ulong lobbyId, string content, Dictionary<string, string>? metadata = null, MessageFlags? flags = null, CancellationToken cancellationToken = default)
	{
		this.RequireBearer();
		ArgumentException.ThrowIfNullOrEmpty(content);
		var body = new Dictionary<string, object> { ["content"] = content };
		if (metadata is not null) body["metadata"] = metadata;
		if (flags.HasValue) body["flags"] = flags.Value;
		return this.RequestObjectAsync<DiscordLobbyMessage>(RestRequestMethod.POST, "/lobbies/:lobby_id/messages", lobbyId, null, body, cancellationToken);
	}

	/// <summary>Gets recent lobby messages as the current OAuth user.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="limit">The maximum number of messages, between 1 and 200.</param><param name="cancellationToken">A cancellation token.</param><returns>The recent messages.</returns>
	public async Task<IReadOnlyList<DiscordLobbyMessage>> GetLobbyMessagesAsync(ulong lobbyId, int limit = 50, CancellationToken cancellationToken = default)
	{
		this.RequireBearer();
		ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, 200);
		var response = await this.RequestAsync(RestRequestMethod.GET, "/lobbies/:lobby_id/messages", lobbyId, null, null, cancellationToken, limit).ConfigureAwait(false);
		return DiscordJson.DeserializeIEnumerableObject<List<DiscordLobbyMessage>>(response.Response, null);
	}

	/// <summary>Creates a linked-channel invite targeted at the current OAuth user.</summary>
	/// <param name="lobbyId">The lobby ID.</param><param name="cancellationToken">A cancellation token.</param><returns>The invite.</returns>
	public Task<DiscordLobbyInvite> CreateLobbySelfInviteAsync(ulong lobbyId, CancellationToken cancellationToken = default)
	{
		this.RequireBearer();
		return this.RequestObjectAsync<DiscordLobbyInvite>(RestRequestMethod.POST, "/lobbies/:lobby_id/members/@me/invites", lobbyId, null, null, cancellationToken);
	}

	/// <summary>Ensures that this instance uses bot authentication.</summary>
	private void RequireBot()
	{
		if (this._bot is null) throw new InvalidOperationException("This lobby operation requires a bot client.");
	}

	/// <summary>Ensures that this instance uses an OAuth user token with the social layer scope.</summary>
	/// <param name="requireSocialScope">Whether Discord documents the social-layer scope for this endpoint.</param>
	private void RequireBearer(bool requireSocialScope = true)
	{
		if (this._token is null) throw new InvalidOperationException("This lobby operation requires an OAuth user token.");
		if (requireSocialScope && !this._token.Scope.Split(' ').Contains("sdk.social_layer")) throw new InvalidOperationException("The OAuth token requires the sdk.social_layer scope.");
	}

	/// <summary>Validates a member array against Discord's per-request limit.</summary>
	/// <param name="members">The optional member updates.</param><param name="required">Whether an empty array is forbidden.</param><returns>The materialized updates, or null.</returns>
	private static DiscordLobbyMemberUpdate[]? ValidateMembers(IEnumerable<DiscordLobbyMemberUpdate>? members, bool required)
	{
		var result = members?.ToArray();
		if ((required && (result is null || result.Length == 0)) || result is { Length: > 25 }) throw new ArgumentOutOfRangeException(nameof(members), "Discord accepts at most 25 members per request.");
		return result;
	}

	/// <summary>Sends an authenticated lobby request through the existing rate-limit machinery.</summary>
	/// <param name="method">The HTTP method.</param><param name="route">The route template.</param><param name="lobbyId">The lobby ID.</param><param name="userId">A user or message ID.</param><param name="body">An optional JSON body.</param><param name="cancellationToken">A cancellation token.</param><returns>The REST response.</returns>
	/// <param name="limit">An optional message limit query parameter.</param>
	private Task<RestResponse> RequestAsync(RestRequestMethod method, string route, ulong? lobbyId, ulong? userId, object? body, CancellationToken cancellationToken, int? limit = null)
	{
		var bucket = this._api.Rest.GetBucket(method, route, new { lobby_id = lobbyId.GetValueOrDefault(), user_id = userId.GetValueOrDefault() }, out var path);
		var config = this._bot?.Configuration ?? this._oauth?.DiscordConfiguration;
		var urlBuilder = Utilities.GetApiUriBuilderFor(path, config);
		if (limit.HasValue) urlBuilder.AddParameter("limit", limit.Value.ToString(CultureInfo.InvariantCulture));
		var url = urlBuilder.Build();
		Dictionary<string, string>? headers = this._token is null ? null : new() { ["Bearer"] = this._token.AccessToken };
		return this._api.DoRequestAsync(this._bot!, bucket, url, method, route, headers, body is null ? null : DiscordJson.SerializeObject(body), cancellationToken: cancellationToken);
	}

	/// <summary>Sends a request and deserializes its lobby response.</summary>
	/// <typeparam name="T">The response model.</typeparam><param name="method">The HTTP method.</param><param name="route">The route template.</param><param name="lobbyId">The lobby ID.</param><param name="userId">The user ID.</param><param name="body">An optional JSON body.</param><param name="cancellationToken">A cancellation token.</param><returns>The response object.</returns>
	private async Task<T> RequestObjectAsync<T>(RestRequestMethod method, string route, ulong? lobbyId, ulong? userId, object? body, CancellationToken cancellationToken) where T : ObservableApiObject
	{
		var result = await this.RequestAsync(method, route, lobbyId, userId, body, cancellationToken).ConfigureAwait(false);
		return DiscordJson.DeserializeObject<T>(result.Response, this._bot);
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
