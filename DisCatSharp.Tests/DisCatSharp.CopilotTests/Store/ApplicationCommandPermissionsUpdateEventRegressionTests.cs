using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Store;

public sealed class ApplicationCommandPermissionsUpdateEventRegressionTests
{
	[Fact]
	public async Task ApplicationCommandPermissionsUpdateEvent_UsesGatewayPayloadWithoutFetchingCommand()
	{
		const ulong applicationId = 1;
		const ulong commandId = 2;
		const ulong guildId = 3;
		var client = CreateClient(applicationId);
		var permissions = new List<DiscordApplicationCommandPermission>
		{
			new(4, ApplicationCommandPermissionType.User, true)
		};
		ApplicationCommandPermissionsUpdateEventArgs? captured = null;
		client.ApplicationCommandPermissionsUpdated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		await client.OnApplicationCommandPermissionsUpdateAsync(permissions, commandId, guildId, applicationId);

		Assert.NotNull(captured);
		Assert.Equal(commandId, captured!.Id);
		Assert.Equal(applicationId, captured.ApplicationId);
		Assert.Equal(guildId, captured.Guild.Id);
		Assert.Same(permissions[0], Assert.Single(captured.Permissions));
		Assert.False(captured.IsApplicationWide);
	}

	[Fact]
	public async Task ApplicationCommandPermissionsUpdateEvent_DetectsApplicationWidePermissions()
	{
		const ulong applicationId = 1;
		const ulong guildId = 3;
		var client = CreateClient(applicationId);
		ApplicationCommandPermissionsUpdateEventArgs? captured = null;
		client.ApplicationCommandPermissionsUpdated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		await client.OnApplicationCommandPermissionsUpdateAsync([], applicationId, guildId, applicationId);

		Assert.NotNull(captured);
		Assert.Equal(applicationId, captured!.Id);
		Assert.True(captured.IsApplicationWide);
	}

	private static DiscordClient CreateClient(ulong applicationId)
	{
		var client = new DiscordClient(new DiscordConfiguration
		{
			Token = "1",
			Gateway =
			{
				Advanced =
				{
					DispatchMode = GatewayDispatchMode.SequentialHandlers
				}
			}
		});
		client.CurrentApplication = new DiscordApplication { Id = applicationId, Discord = client };
		return client;
	}
}
