using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Net;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Xunit;

namespace DisCatSharp.Hosting.Tests;

/// <summary>Verifies Social SDK service registration and webhook-to-REST mapping.</summary>
public sealed class DiscordSocialSdkIntegrationTests
{
	/// <summary>The unified client uses the supplied bot and registration remains idempotent.</summary>
	[Fact]
	public void Services_ResolveClientsUsingConfiguredBot()
	{
		using var bot = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var services = new ServiceCollection();
		services.AddDiscordSocialSdkSupport(_ => bot).AddDiscordSocialSdkSupport(_ => throw new InvalidOperationException());
		using var provider = services.BuildServiceProvider();
		Assert.Same(provider.GetRequiredService<DiscordSocialSdkClient>(), provider.GetRequiredService<DiscordSocialSdkClient>());
	}

	/// <summary>Webhook identifiers are passed through to their corresponding moderation endpoints.</summary>
	[Fact]
	public async Task WebhookMessages_UsePayloadIdentifiersForModeration()
	{
		using var bot = new DiscordClient(new DiscordConfiguration { Token = "bot-test" });
		var handler = new RecordingHandler();
		var rest = bot.ApiClient.Rest;
		var original = rest.HttpClient;
		var http = new HttpClient(handler);
		foreach (var header in original.DefaultRequestHeaders) http.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
		typeof(RestClient).GetField("<HttpClient>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(rest, http);
		original.Dispose();
		var lobby = JsonConvert.DeserializeObject<DiscordWebhookLobbyMessageEventData>("{\"id\":\"33\",\"lobby_id\":\"22\"}")!;
		var direct = JsonConvert.DeserializeObject<DiscordWebhookGameDirectMessageEventData>("{\"id\":\"44\",\"author\":{\"id\":\"11\"},\"recipient_id\":\"22\"}")!;
		await lobby.UpdateModerationMetadataAsync(new DiscordSocialSdkClient(bot), new() { ["action"] = "hide" });
		Assert.Equal("/api/v10/lobbies/22/messages/33/moderation-metadata", handler.Path);
		await direct.UpdateModerationMetadataAsync(new DiscordSocialSdkClient(bot), new Dictionary<string, string> { ["action"] = "hide" });
		Assert.Equal("/api/v10/partner-sdk/dms/11/22/messages/44/moderation-metadata", handler.Path);
		var partial = JsonConvert.DeserializeObject<DiscordWebhookGameDirectMessageEventData>("{\"id\":\"44\"}")!;
		Assert.Throws<InvalidOperationException>(() => { _ = partial.UpdateModerationMetadataAsync(new DiscordSocialSdkClient(bot), new Dictionary<string, string>()); });
	}

	/// <summary>Captures the outgoing request without contacting Discord.</summary>
	private sealed class RecordingHandler : HttpMessageHandler
	{
		/// <summary>Gets the latest request path.</summary>
		public string? Path { get; private set; }
		/// <summary>Records a request and responds successfully.</summary>
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			this.Path = request.RequestUri!.AbsolutePath;
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
		}
	}
}
