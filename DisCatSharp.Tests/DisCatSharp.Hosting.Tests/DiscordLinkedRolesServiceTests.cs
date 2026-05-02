using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Enums;
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public sealed class DiscordLinkedRolesServiceTests
{
	[Fact]
	public void AddDiscordLinkedRolesSupport_RegistersLinkedRolesService()
	{
		using var provider = BuildProvider();

		Assert.NotNull(provider.GetRequiredService<DiscordLinkedRolesService>());
	}

	[Fact]
	public void LinkedRolesService_GetVerificationUrl_UsesConfiguredVerificationPath()
	{
		using var provider = BuildProvider(services => services.Configure<DiscordLinkedRolesOptions>(options => options.VerificationPath = "role-connections/verify"));
		var service = provider.GetRequiredService<DiscordLinkedRolesService>();

		var verificationUrl = service.GetVerificationUrl(new Uri("https://bot.example.com/base"));

		Assert.Equal("https://bot.example.com/base/role-connections/verify", verificationUrl.AbsoluteUri);
	}

	[Fact]
	public async Task LinkedRolesService_GetMetadataAsync_UsesRegisteredProvider()
	{
		using var provider = BuildProvider(services => services.AddDiscordLinkedRolesMetadataProvider<TestLinkedRolesMetadataProvider>());
		var service = provider.GetRequiredService<DiscordLinkedRolesService>();

		var metadata = await service.GetMetadataAsync();

		var record = Assert.Single(metadata);
		Assert.Equal("verified", record.Key);
		Assert.Equal("Verified Account", record.Name);
	}

	[Fact]
	public void LinkedRolesService_HasRoleConnectionsWriteScope_DetectsGrantedScope()
	{
		using var provider = BuildProvider();
		var service = provider.GetRequiredService<DiscordLinkedRolesService>();
		var result = DiscordOAuthCallbackResult.Success(
			"state",
			"code",
			new DiscordIngressPendingState
			{
				Key = "state"
			},
			new Uri("https://bot.example.com/discord/oauth/callback"),
			new Uri("https://bot.example.com/discord/oauth/callback?code=code&state=state"),
			DiscordAccessToken.FromJson("{\"access_token\":\"access-token\",\"token_type\":\"Bearer\",\"expires_in\":3600,\"refresh_token\":\"refresh-token\",\"scope\":\"identify role_connections.write\"}"));

		Assert.True(service.HasRoleConnectionsWriteScope(result));
	}

	[Fact]
	public async Task LinkedRolesService_PublishRoleConnectionAsync_RejectsCallbackResultsWithoutLinkedRolesScope()
	{
		using var provider = BuildProvider();
		var service = provider.GetRequiredService<DiscordLinkedRolesService>();
		using var oauthClient = new DiscordOAuth2Client(1, "secret", "https://bot.example.com/discord/oauth/callback");
		var result = DiscordOAuthCallbackResult.Success(
			"state",
			"code",
			new DiscordIngressPendingState
			{
				Key = "state"
			},
			new Uri("https://bot.example.com/discord/oauth/callback"),
			new Uri("https://bot.example.com/discord/oauth/callback?code=code&state=state"),
			DiscordAccessToken.FromJson("{\"access_token\":\"access-token\",\"token_type\":\"Bearer\",\"expires_in\":3600,\"refresh_token\":\"refresh-token\",\"scope\":\"identify\"}"));

		await Assert.ThrowsAsync<InvalidOperationException>(() => service.PublishRoleConnectionAsync(
			oauthClient,
			result,
			"Example",
			"lala",
			new ApplicationRoleConnectionMetadata().AddMetadata("verified", "1")));
	}

	private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
	{
		ServiceCollection services = [];
		services.AddDisCatSharpAspNetCore();
		services.AddDiscordLinkedRolesSupport();
		configure?.Invoke(services);
		return services.BuildServiceProvider();
	}

	private sealed class TestLinkedRolesMetadataProvider : IDiscordLinkedRolesMetadataProvider
	{
		public ValueTask<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> GetMetadataAsync(System.Threading.CancellationToken cancellationToken = default)
			=> new((IReadOnlyList<DiscordApplicationRoleConnectionMetadata>)
			[
				new DiscordApplicationRoleConnectionMetadata(
					ApplicationRoleConnectionMetadataType.BooleanEqual,
					"verified",
					"Verified Account",
					"Whether the account is verified")
			]);
	}
}
