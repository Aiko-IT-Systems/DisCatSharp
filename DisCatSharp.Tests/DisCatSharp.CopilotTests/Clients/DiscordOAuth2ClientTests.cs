using System;
using System.Globalization;
using System.Text;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Clients;

public sealed class DiscordOAuth2ClientTests
{
	[Fact]
	public void Constructor_SetsBasicAuthorizationWithoutLeakingCustomCredentialHeaders()
	{
		const ulong clientId = 773493116404629504;
		const string clientSecret = "super-secret";

		using var client = new DiscordOAuth2Client(
			clientId,
			clientSecret,
			"https://127.0.0.1/");

		Assert.Equal("Basic", client.ApiClient.Rest.HttpClient.DefaultRequestHeaders.Authorization?.Scheme);
		Assert.Equal(
			Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId.ToString(CultureInfo.InvariantCulture)}:{clientSecret}")),
			client.ApiClient.Rest.HttpClient.DefaultRequestHeaders.Authorization?.Parameter);
		Assert.False(client.ApiClient.Rest.HttpClient.DefaultRequestHeaders.Contains("client_id"));
		Assert.False(client.ApiClient.Rest.HttpClient.DefaultRequestHeaders.Contains("client_secret"));
	}
}
