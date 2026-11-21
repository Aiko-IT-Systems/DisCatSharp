using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Net.Serialization;
using DisCatSharp.Net;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Clients;

/// <summary>Verifies lobby request models and authorization boundaries without calling Discord.</summary>
public sealed class DiscordLobbyClientTests
{
	/// <summary>Verifies that bot lobby routes use the bot token and resolve their route parameters.</summary>
	[Fact]
	public async Task BotRequest_UsesBotAuthorizationAndLobbyRoute()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "test-token" });
		var handler = new RecordingHandler("{\"id\":\"12\",\"application_id\":\"34\",\"members\":[]}");
		InstallHandler(client.ApiClient.Rest, handler);
		var lobby = await new DiscordLobbyClient(client).GetAsync(12);
		Assert.Equal(12UL, lobby.Id);
		Assert.Equal("/api/v10/lobbies/12", handler.Path);
		Assert.Equal("Bot", handler.Scheme);
	}

	/// <summary>Verifies that bearer lobby routes use the user token and preserve the message limit.</summary>
	[Fact]
	public async Task OAuthRequest_UsesBearerAuthorizationAndLimit()
	{
		using var client = new DiscordOAuth2Client(123, "test", "https://example.org/callback");
		var handler = new RecordingHandler("[]");
		InstallHandler(client.ApiClient.Rest, handler);
		var token = new DiscordAccessToken("user-token", "Bearer", 3600, "refresh", "sdk.social_layer");
		Assert.Empty(await new DiscordLobbyClient(client, token).GetMessagesAsync(12, 17));
		Assert.Equal("/api/v10/lobbies/12/messages?limit=17", handler.Path);
		Assert.Equal("Bearer", handler.Scheme);
		Assert.Equal("user-token", handler.Token);
	}

	/// <summary>Replaces the test client's transport while preserving its configured default headers.</summary>
	private static void InstallHandler(RestClient rest, HttpMessageHandler handler)
	{
		var original = rest.HttpClient;
		var replacement = new HttpClient(handler);
		foreach (var header in original.DefaultRequestHeaders)
			replacement.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
		typeof(RestClient).GetField("<HttpClient>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(rest, replacement);
		original.Dispose();
	}

	/// <summary>Records the HTTP request and returns a fixed JSON response.</summary>
	private sealed class RecordingHandler(string json) : HttpMessageHandler
	{
		/// <summary>Gets the path and query received by the test transport.</summary>
		public string? Path { get; private set; }
		/// <summary>Gets the received authorization scheme.</summary>
		public string? Scheme { get; private set; }
		/// <summary>Gets the received authorization token.</summary>
		public string? Token { get; private set; }
		/// <summary>Returns the configured response after recording the request.</summary>
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			this.Path = request.RequestUri!.PathAndQuery;
			this.Scheme = request.Headers.Authorization?.Scheme;
			this.Token = request.Headers.Authorization?.Parameter;
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
		}
	}
	/// <summary>Reads lobby and message responses without dropping metadata or member names.</summary>
	[Fact]
	public void Responses_DeserializeLobbyAndMessageFields()
	{
		var lobby = DiscordJson.DeserializeObject<DiscordLobby>("{\"id\":\"12\",\"application_id\":\"34\",\"members\":[{\"id\":\"56\",\"additional_name\":\"Cat\",\"flags\":1}]}", null);
		Assert.Equal(12UL, lobby.Id);
		Assert.Equal(DiscordLobbyMemberFlags.CanLinkLobby, Assert.Single(lobby.Members).Flags);
		Assert.Equal("Cat", lobby.Members[0].AdditionalName);
		var message = DiscordJson.DeserializeObject<DiscordLobbyMessage>("{\"id\":\"1\",\"lobby_id\":\"12\",\"content\":\"hi\",\"lobby_member\":{\"additional_name\":\"Cat\"},\"moderation_metadata\":{\"state\":\"ok\"}}", null);
		Assert.Equal("Cat", message.LobbyMember!.AdditionalName);
		Assert.Equal("ok", message.ModerationMetadata!["state"]);
	}
	/// <summary>Distinguishes an omitted additional name from an explicit clear.</summary>
	[Fact]
	public void MemberUpdate_PreservesOptionalNull()
	{
		var update = new DiscordLobbyMemberUpdate { Id = 42 };
		var omitted = JObject.Parse(DiscordJson.SerializeObject(update));
		Assert.Null(omitted["additional_name"]);
		update.AdditionalName = (string?)null;
		var cleared = JObject.Parse(DiscordJson.SerializeObject(update));
		Assert.Equal(JTokenType.Null, cleared["additional_name"]!.Type);
	}

	/// <summary>Checks lobby member limit and that bot-only clients cannot use bearer-only routes.</summary>
	[Fact]
	public void BotClient_RejectsOversizedBatchesAndBearerOperations()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "test" });
		var lobby = new DiscordLobbyClient(client);
		Assert.Throws<ArgumentOutOfRangeException>(() => { _ = lobby.CreateAsync(members: Enumerable.Range(1, 26).Select(x => new DiscordLobbyMemberUpdate { Id = (ulong)x })); });
		Assert.Throws<InvalidOperationException>(() => { _ = lobby.CreateOrJoinAsync("secret"); });
	}

	/// <summary>Checks that bearer-only clients cannot use bot routes or omit required scope.</summary>
	[Fact]
	public void OAuthClient_RejectsBotOperationsAndMissingScope()
	{
		using var client = new DiscordOAuth2Client(123, "test", "https://example.org/callback");
		var token = new DiscordAccessToken("token", "Bearer", 3600, "refresh", "identify");
		var lobby = new DiscordLobbyClient(client, token);
		Assert.Throws<InvalidOperationException>(() => { _ = lobby.GetAsync(123); });
		Assert.Throws<InvalidOperationException>(() => { _ = lobby.CreateOrJoinAsync("secret"); });
	}
}
