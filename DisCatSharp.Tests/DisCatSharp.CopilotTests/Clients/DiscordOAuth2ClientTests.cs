using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Clients;

public sealed class DiscordOAuth2ClientTests
{
	[Fact]
	public void PublicApi_DoesNotExposeOAuthClientSecrets()
	{
		Assert.Null(typeof(DiscordOAuth2Client).GetField("ClientSecret", BindingFlags.Instance | BindingFlags.Public));

		var configurationSecret = typeof(DiscordOAuth2ClientConfiguration).GetProperty(nameof(DiscordOAuth2ClientConfiguration.ClientSecret));
		Assert.NotNull(configurationSecret);
		Assert.False(configurationSecret.GetMethod?.IsPublic);
		Assert.True(configurationSecret.SetMethod?.IsPublic);
	}

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

	[Fact]
	public async Task Constructor_AtomicallySharesConfiguredRsaKeyAcrossConcurrentClients()
	{
		var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"dcs-oauth-tests-{Guid.NewGuid():N}");
		var keyFilePath = Path.Combine(temporaryDirectory, "shared-renamed-key.pem");
		const int clientCount = 16;
		using var startGate = new ManualResetEventSlim(false);
		var clients = new DiscordOAuth2Client?[clientCount];
		var constructionTasks = Enumerable.Range(0, clientCount)
			.Select(index => Task.Run(() =>
			{
				startGate.Wait();
				clients[index] = new DiscordOAuth2Client(new DiscordOAuth2ClientConfiguration
				{
					ClientId = 773493116404629504,
					ClientSecret = "super-secret",
					RedirectUri = "https://127.0.0.1/",
					RsaKeyFilePath = keyFilePath
				});
			}))
			.ToArray();

		startGate.Set();
		try
		{
			await Task.WhenAll(constructionTasks);
			var initializedClients = clients.Cast<DiscordOAuth2Client>().ToArray();
			var state = initializedClients[0].GenerateSecureState(856780995629154305);

			Assert.True(File.Exists(keyFilePath));
			Assert.All(initializedClients, client =>
			{
				Assert.Equal(Path.GetFullPath(keyFilePath), client.RsaKeyFilePath);
				Assert.Contains("::856780995629154305::", client.ReadSecureState(state), StringComparison.Ordinal);
			});
			Assert.Empty(Directory.EnumerateFiles(temporaryDirectory, "*.tmp"));
		}
		finally
		{
			foreach (var client in clients)
				client?.Dispose();

			if (Directory.Exists(temporaryDirectory))
				Directory.Delete(temporaryDirectory, true);
		}
	}
}
