using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Net;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Clients;

/// <summary>Exercises bot-authenticated Social SDK routes using an in-memory HTTP transport.</summary>
public sealed class DiscordSocialSdkClientTests
{
	/// <summary>Checks user identity lookup returns Discord's identity list without losing provider IDs.</summary>
	[Fact]
	public async Task UserIdentities_UseUserScopedRoute()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(HttpStatusCode.OK, "{\"identities\":[{\"user_id\":\"34\",\"provider_type\":\"OIDC\",\"provider_id\":\"issuer\",\"provider_issued_user_id\":\"external\"}]}");
		InstallHandler(client.ApiClient.Rest, handler);
		var identity = Assert.Single(await new DiscordSocialSdkClient(client).GetUserIdentitiesAsync(12, 34));
		Assert.Equal("issuer", identity.ProviderId);
		Assert.Equal("/api/v10/users/34/application-identities/12", handler.Path);
	}

	/// <summary>Checks profile reads preserve predefined and custom data fields.</summary>
	[Fact]
	public async Task GetProfile_DeserializesData()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(HttpStatusCode.OK, "{\"username\":\"cat\",\"metadata\":{\"a\":1},\"data\":{\"primary\":{\"total_wins\":4},\"dynamic\":[{\"type\":1,\"name\":\"rank\",\"value\":\"gold\"}]}}");
		InstallHandler(client.ApiClient.Rest, handler);
		var profile = await new DiscordSocialSdkClient(client).GetProfileAsync(12, 34, "external");
		Assert.Equal(4, profile.Data!.Primary!.TotalWins);
		Assert.Equal("gold", (string?)Assert.Single(profile.Data.Dynamic!).Value);
		Assert.Equal(1, (int)profile.Metadata!["a"]!);
	}

	/// <summary>Checks identity deletion uses POST and places the optional provider ID in the JSON body.</summary>
	[Fact]
	public async Task DeleteIdentity_UsesDocumentedRouteAndBody()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(HttpStatusCode.NoContent, "");
		InstallHandler(client.ApiClient.Rest, handler);
		await new DiscordSocialSdkClient(client).DeleteIdentityAsync(12, 34, "OIDC", "external/id", "issuer");
		Assert.Equal(HttpMethod.Post, handler.Method);
		Assert.Equal("/api/v10/users/34/application-identities/12/OIDC/external%2Fid/delete", handler.Path);
		Assert.Equal("issuer", (string?)JObject.Parse(handler.Body!)["provider_id"]);
	}

	/// <summary>Checks bot unmerge identifies the provisional account using its external user ID.</summary>
	[Fact]
	public async Task Unmerge_UsesExternalUserId()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(HttpStatusCode.NoContent, "");
		InstallHandler(client.ApiClient.Rest, handler);
		await new DiscordSocialSdkClient(client).UnmergeProvisionalAccountAsync("player");
		Assert.Equal("/api/v10/partner-sdk/provisional-accounts/unmerge/bot", handler.Path);
		Assert.Equal("player", (string?)JObject.Parse(handler.Body!)["external_user_id"]);
	}
	/// <summary>Tests the bot credentials, URL escaping, provider query, and wrapped identity response.</summary>
	[Fact]
	public async Task IdentityLookup_EncodesPathAndQueryAndReturnsUser()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(HttpStatusCode.OK, "{\"identities\":[{\"user_id\":\"77\",\"provider_type\":\"OIDC\",\"provider_issued_user_id\":\"a/b\"}]}");
		InstallHandler(client.ApiClient.Rest, handler);
		var result = await new DiscordSocialSdkClient(client).GetIdentitiesByExternalIdAsync(12, "OIDC", "a/b ?", "site & app");
		Assert.Equal(77UL, Assert.Single(result).UserId);
		Assert.Equal("Bot", handler.Scheme);
		Assert.Equal("/api/v10/applications/12/application-identities/OIDC/a%2Fb%20%3F?provider_id=site%20%26%20app", handler.Path);
	}

	/// <summary>Checks whole-data replacement, optional omission, decimal and media fields, and both documented success statuses.</summary>
	[Theory]
	[InlineData(HttpStatusCode.Created)]
	[InlineData(HttpStatusCode.NoContent)]
	public async Task ProfileUpdate_SendsCompleteRequestedDataAndAcceptsEmptySuccess(HttpStatusCode status)
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(status, "");
		InstallHandler(client.ApiClient.Rest, handler);
		var data = new DiscordGameProfileData
		{
			Primary = new() { PlaytimeHours = 69.41, TotalWins = 57 },
			Dynamic = [DiscordGameDynamicField.String("tier", "gold"), DiscordGameDynamicField.Number("score", 1.5), DiscordGameDynamicField.Media("badge", "https://example.org/a.png")]
		};
		await new DiscordSocialSdkClient(client).UpdateProfileAsync(12, 34, "external/id", data: data);
		var payload = JObject.Parse(handler.Body!);
		Assert.Null(payload["username"]);
		Assert.Equal(69.41, (double)payload["data"]!["primary"]!["playtime_hours"]!);
		Assert.Equal("https://example.org/a.png", (string?)payload["data"]!["dynamic"]![2]!["value"]!["url"]);
		Assert.Equal("/api/v10/applications/12/users/34/identities/external%2Fid/profile", handler.Path);
	}

	/// <summary>Verifies that omitting data does not send a replacement data object.</summary>
	[Fact]
	public async Task ProfileUpdate_LeavesDataOutWhenNotSpecified()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(HttpStatusCode.NoContent, "");
		InstallHandler(client.ApiClient.Rest, handler);
		await new DiscordSocialSdkClient(client).UpdateProfileAsync(12, 34, "abc", username: "cat");
		Assert.Equal("cat", (string?)JObject.Parse(handler.Body!)["username"]);
		Assert.Null(JObject.Parse(handler.Body!)["data"]);
	}

	/// <summary>Checks direct-message moderation targets both participants and sends the metadata map directly.</summary>
	[Fact]
	public async Task DirectMessageModeration_UsesDocumentedRouteAndBody()
	{
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler(HttpStatusCode.NoContent, "");
		InstallHandler(client.ApiClient.Rest, handler);
		await new DiscordSocialSdkClient(client).UpdateDirectMessageModerationAsync(1, 2, 3, new Dictionary<string, string> { ["action"] = "hide" });
		Assert.Equal("/api/v10/partner-sdk/dms/1/2/messages/3/moderation-metadata", handler.Path);
		Assert.Equal("hide", (string?)JObject.Parse(handler.Body!)["action"]);
	}

	/// <summary>Tests bot token issuance and ensures the returned secrets are never included in trace logs.</summary>
	[Fact]
	public async Task ProvisionalToken_IsReturnedButNotWrittenToTraceLogs()
	{
		var provider = new RecordingLoggerProvider();
		using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Trace).AddProvider(provider));
		using var client = new DiscordClient(new DiscordConfiguration { Token = "bot-test", EnableLibraryDeveloperMode = true, Logging = new() { MinimumLogLevel = LogLevel.Trace, LoggerFactory = loggerFactory } });
		var handler = new RecordingHandler(HttpStatusCode.OK, "{\"access_token\":\"secret-access-value\",\"id_token\":\"secret-identity-value\",\"refresh_token\":\"secret-extra-value\",\"token_type\":\"Bearer\",\"expires_in\":100,\"scope\":\"sdk.social_layer\"}");
		InstallHandler(client.ApiClient.Rest, handler);
		var token = await new DiscordSocialSdkClient(client).GetProvisionalTokenAsync("player-1", "Lala");
		Assert.Equal("secret-access-value", token.AccessToken);
		Assert.Equal("/api/v10/partner-sdk/token/bot", handler.Path);
		Assert.Equal("player-1", (string?)JObject.Parse(handler.Body!)["external_user_id"]);
		Assert.DoesNotContain(provider.Messages, message => message.Contains("secret-access-value", StringComparison.Ordinal) || message.Contains("secret-identity-value", StringComparison.Ordinal) || message.Contains("secret-extra-value", StringComparison.Ordinal));
		Assert.Contains(provider.Messages, message => message.Contains("<redacted provisional token response>", StringComparison.Ordinal));
	}

	/// <summary>Replaces the test REST transport while preserving the bot's default authentication header.</summary>
	private static void InstallHandler(RestClient rest, HttpMessageHandler handler)
	{
		var original = rest.HttpClient;
		var replacement = new HttpClient(handler);
		foreach (var header in original.DefaultRequestHeaders) replacement.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
		typeof(RestClient).GetField("<HttpClient>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(rest, replacement);
		original.Dispose();
	}

	/// <summary>Records a single request and returns a configured response.</summary>
	private sealed class RecordingHandler(HttpStatusCode status, string json) : HttpMessageHandler
	{
		/// <summary>Gets the request path and query.</summary>
		public string? Path { get; private set; }
		/// <summary>Gets the request authorization scheme.</summary>
		public string? Scheme { get; private set; }
		/// <summary>Gets the request body.</summary>
		public string? Body { get; private set; }
		/// <summary>Gets the request method.</summary>
		public HttpMethod? Method { get; private set; }
		/// <summary>Records the request and produces the configured response.</summary>
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			this.Path = request.RequestUri!.PathAndQuery;
			this.Method = request.Method;
			this.Scheme = request.Headers.Authorization?.Scheme;
			this.Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
			return new(status) { Content = new StringContent(json) };
		}
	}

	/// <summary>Captures log messages without recording them outside the test.</summary>
	private sealed class RecordingLoggerProvider : ILoggerProvider
	{
		/// <summary>Gets captured messages.</summary>
		public ConcurrentQueue<string> Messages { get; } = new();
		/// <summary>Creates a logger writing to the shared in-memory queue.</summary>
		public ILogger CreateLogger(string categoryName) => new RecordingLogger(this.Messages);
		/// <summary>Disposes the provider.</summary>
		public void Dispose() { }
	}

	/// <summary>Records formatted log messages.</summary>
	private sealed class RecordingLogger(ConcurrentQueue<string> messages) : ILogger
	{
		/// <summary>Starts an unused logging scope.</summary>
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		/// <summary>Enables all log levels for the test.</summary>
		public bool IsEnabled(LogLevel logLevel) => true;
		/// <summary>Captures a formatted message.</summary>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => messages.Enqueue(formatter(state, exception));
	}
}
