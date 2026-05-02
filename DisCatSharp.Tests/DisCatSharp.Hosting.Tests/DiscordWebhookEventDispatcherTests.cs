using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.EventArgs;
using DisCatSharp.Hosting.AspNetCore.Ingress;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public sealed class DiscordWebhookEventDispatcherTests
{
	[Fact]
	public void AddDisCatSharpAspNetCore_RegistersWebhookEventDispatcher()
	{
		using var provider = BuildProvider();

		Assert.NotNull(provider.GetRequiredService<DiscordWebhookEventDispatcher>());
	}

	[Fact]
	public async Task WebhookEventIngressService_DispatchesTypedApplicationAuthorizedEvents()
	{
		using var provider = BuildProvider();
		var service = provider.GetRequiredService<DiscordWebhookEventIngressService>();
		var dispatcher = provider.GetRequiredService<DiscordWebhookEventDispatcher>();
		TaskCompletionSource<WebhookApplicationAuthorizedEventArgs> received = new(TaskCreationOptions.RunContinuationsAsynchronously);

		dispatcher.ApplicationAuthorized += (_, eventArgs) =>
		{
			received.TrySetResult(eventArgs);
			return Task.CompletedTask;
		};

		var result = await service.HandleAsync(CreateWebhookRequest("""
		                                                  {
		                                                    "version": 1,
		                                                    "application_id": "1234560123453231555",
		                                                    "type": 1,
		                                                    "event": {
		                                                      "type": "APPLICATION_AUTHORIZED",
		                                                      "timestamp": "2024-10-18T14:42:53.064834",
		                                                      "data": {
		                                                        "integration_type": 1,
		                                                        "scopes": ["applications.commands", "bot"],
		                                                        "user": {
		                                                          "id": "111178765189277770",
		                                                          "username": "lala",
		                                                          "discriminator": "0",
		                                                          "avatar": null
		                                                        }
		                                                      }
		                                                    }
		                                                  }
		                                                  """));

		Assert.Equal(StatusCodes.Status204NoContent, result.Response.StatusCode);

		var completedTask = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(2)));
		Assert.Same(received.Task, completedTask);

		var eventArgs = await received.Task;
		Assert.Equal("APPLICATION_AUTHORIZED", eventArgs.Envelope.EventType);
		Assert.Equal("lala", eventArgs.Authorization.User?.Username);
		Assert.Collection(eventArgs.Authorization.Scopes,
			scope =>
			{
				Assert.Equal("applications.commands", scope);
			},
			scope =>
			{
				Assert.Equal("bot", scope);
			});
		Assert.Equal("https://example.com/discord/webhooks/events", eventArgs.Request.RequestUri?.AbsoluteUri);
	}

	[Fact]
	public async Task WebhookEventIngressService_RaisesUnknownWebhookEventForUnmappedPayloads()
	{
		using var provider = BuildProvider();
		var service = provider.GetRequiredService<DiscordWebhookEventIngressService>();
		var dispatcher = provider.GetRequiredService<DiscordWebhookEventDispatcher>();
		TaskCompletionSource<UnknownWebhookEventArgs> received = new(TaskCreationOptions.RunContinuationsAsynchronously);

		dispatcher.UnknownWebhookEventReceived += (_, eventArgs) =>
		{
			received.TrySetResult(eventArgs);
			return Task.CompletedTask;
		};

		var result = await service.HandleAsync(CreateWebhookRequest("""
		                                                  {
		                                                    "version": 1,
		                                                    "application_id": "1234560123453231555",
		                                                    "type": 1,
		                                                    "event": {
		                                                      "type": "QUEST_USER_ENROLLMENT",
		                                                      "timestamp": "2024-10-18T14:42:53.064834",
		                                                      "data": {
		                                                        "id": "1234567890"
		                                                      }
		                                                    }
		                                                  }
		                                                  """));

		Assert.Equal(StatusCodes.Status204NoContent, result.Response.StatusCode);

		var completedTask = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(2)));
		Assert.Same(received.Task, completedTask);
		Assert.Equal("QUEST_USER_ENROLLMENT", (await received.Task).Envelope.EventType);
	}

	[Fact]
	public async Task WebhookEventIngressService_AcknowledgesWithoutWaitingForAsyncHandlers()
	{
		using var provider = BuildProvider();
		var service = provider.GetRequiredService<DiscordWebhookEventIngressService>();
		var dispatcher = provider.GetRequiredService<DiscordWebhookEventDispatcher>();
		TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
		TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
		TaskCompletionSource completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

		dispatcher.ApplicationAuthorized += async (_, eventArgs) =>
		{
			started.TrySetResult();
			await release.Task;
			completed.TrySetResult();
		};

		var handleTask = service.HandleAsync(CreateWebhookRequest("""
		                                                  {
		                                                    "version": 1,
		                                                    "application_id": "1234560123453231555",
		                                                    "type": 1,
		                                                    "event": {
		                                                      "type": "APPLICATION_AUTHORIZED",
		                                                      "timestamp": "2024-10-18T14:42:53.064834",
		                                                      "data": {
		                                                        "integration_type": 1,
		                                                        "scopes": ["applications.commands"],
		                                                        "user": {
		                                                          "id": "111178765189277770",
		                                                          "username": "lala",
		                                                          "discriminator": "0",
		                                                          "avatar": null
		                                                        }
		                                                      }
		                                                    }
		                                                  }
		                                                  """)).AsTask();

		var finished = await Task.WhenAny(handleTask, Task.Delay(TimeSpan.FromSeconds(2)));
		Assert.Same(handleTask, finished);
		Assert.Equal(StatusCodes.Status204NoContent, (await handleTask).Response.StatusCode);

		var startedTask = await Task.WhenAny(started.Task, Task.Delay(TimeSpan.FromSeconds(2)));
		Assert.Same(started.Task, startedTask);

		release.TrySetResult();
		var completedTask = await Task.WhenAny(completed.Task, Task.Delay(TimeSpan.FromSeconds(2)));
		Assert.Same(completed.Task, completedTask);
	}

	private static ServiceProvider BuildProvider()
	{
		ServiceCollection services = [];
		services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug));
		services.AddDisCatSharpAspNetCore();
		services.Replace(ServiceDescriptor.Singleton<IDiscordIngressSignatureValidationService, AlwaysValidSignatureValidationService>());
		return services.BuildServiceProvider();
	}

	private static DiscordIngressRequest CreateWebhookRequest(string body)
		=> new(
			HttpMethods.Post,
			new Uri("https://example.com/discord/webhooks/events"),
			new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
			{
				[DiscordIngressHeaderNames.SignatureTimestamp] = "1710000000",
				[DiscordIngressHeaderNames.SignatureEd25519] = "ignored-by-test"
			},
			DiscordIngressPayload.FromString(body));

	private sealed class AlwaysValidSignatureValidationService : IDiscordIngressSignatureValidationService
	{
		public ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, System.Threading.CancellationToken cancellationToken = default)
			=> new(DiscordIngressSignatureValidationResult.Valid("test"));
	}
}
