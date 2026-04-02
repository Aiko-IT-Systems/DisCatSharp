using System;
using System.Threading.Tasks;

using DisCatSharp.Enums;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Clients;

/// <summary>
///     Regression tests for Phase 6 async-disposal migration across the non-sharded client family.
/// </summary>
public class ClientAsyncDisposalTests
{
	private static DiscordClient CreateDiscordClient()
		=> new(new DiscordConfiguration
		{
			Token = "dummy.token.for.testing",
			TokenType = TokenType.Bot,
			AutoReconnect = false,
			EnableSentry = false
		});

	[Fact]
	public void DiscordClient_ImplementsIAsyncDisposable()
	{
		using var client = CreateDiscordClient();
		Assert.IsType<IAsyncDisposable>(client, exactMatch: false);
	}

	[Fact]
	public async Task DiscordClient_DisposeAsync_OnNeverStartedClient_DoesNotThrow()
	{
		var client = CreateDiscordClient();

		var ex = await Record.ExceptionAsync(async () => await client.DisposeAsync());

		Assert.Null(ex);
	}

	[Fact]
	public async Task DiscordClient_DisposeAsync_ThenDispose_IsIdempotent()
	{
		var client = CreateDiscordClient();

		var ex = await Record.ExceptionAsync(async () =>
		{
			await client.DisposeAsync();
			client.Dispose();
		});

		Assert.Null(ex);
	}

	[Fact]
	public void DiscordOAuth2Client_ImplementsIAsyncDisposable()
		=> Assert.True(typeof(IAsyncDisposable).IsAssignableFrom(typeof(DiscordOAuth2Client)));

	[Fact]
	public void DiscordWebhookClient_ImplementsIAsyncDisposable()
	{
		using var client = new DiscordWebhookClient();
		Assert.IsType<IAsyncDisposable>(client, exactMatch: false);
	}

	[Fact]
	public async Task DiscordWebhookClient_DisposeAsync_IsIdempotent()
	{
		var client = new DiscordWebhookClient();

		var ex = await Record.ExceptionAsync(async () =>
		{
			await client.DisposeAsync();
			await client.DisposeAsync();
			client.Dispose();
		});

		Assert.Null(ex);
	}
}
