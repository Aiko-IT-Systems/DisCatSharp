using System;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Clients;

/// <summary>Verifies lobby request models and authorization boundaries without calling Discord.</summary>
public sealed class DiscordLobbyClientTests
{
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
