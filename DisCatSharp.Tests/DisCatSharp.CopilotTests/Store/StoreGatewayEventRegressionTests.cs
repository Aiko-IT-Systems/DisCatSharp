using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Store;

public class StoreGatewayEventRegressionTests
{
	[Fact]
	public async Task GuildAppliedBoostsCreateEvent_RaisesExpectedArguments()
	{
		var client = CreateClient();
		var guild = new DiscordGuild
		{
			Id = 804032421678153819,
			Discord = client
		};
		var user = new DiscordUser
		{
			Id = 856780995629154305,
			Discord = client
		};
		client.GuildsInternal[guild.Id] = guild;
		client.UserCache[user.Id] = user;

		GuildAppliedBoostsCreateEventArgs? captured = null;
		client.GuildAppliedBoostsCreated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		var pauseEndsAt = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
		var endsAt = new DateTimeOffset(2025, 2, 15, 12, 0, 0, TimeSpan.Zero);

		await client.OnGuildAppliedBoostsCreateEventAsync(1417215769351163964, guild.Id, user.Id, pauseEndsAt, endsAt, false);

		Assert.NotNull(captured);
		Assert.Same(guild, captured!.Guild);
		Assert.Same(user, captured.User);
		Assert.Equal((ulong)1417215769351163964, captured.BoostId);
		Assert.Equal(pauseEndsAt, captured.PauseEndsAt);
		Assert.Equal(endsAt, captured.EndsAt);
		Assert.False(captured.Ended);
	}

	[Fact]
	public async Task GuildAppliedBoostsDeleteEvent_RaisesExpectedArguments()
	{
		var client = CreateClient();
		var guild = new DiscordGuild
		{
			Id = 804032421678153819,
			Discord = client
		};
		var user = new DiscordUser
		{
			Id = 856780995629154305,
			Discord = client
		};
		client.GuildsInternal[guild.Id] = guild;
		client.UserCache[user.Id] = user;

		GuildAppliedBoostsDeleteEventArgs? captured = null;
		client.GuildAppliedBoostsDeleted += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		await client.OnGuildAppliedBoostsDeleteEventAsync(1417215769351163964, guild.Id, user.Id, null, null, true);

		Assert.NotNull(captured);
		Assert.Same(guild, captured!.Guild);
		Assert.Same(user, captured.User);
		Assert.Equal((ulong)1417215769351163964, captured.BoostId);
		Assert.Null(captured.PauseEndsAt);
		Assert.Null(captured.EndsAt);
		Assert.True(captured.Ended);
	}

	[Fact]
	public async Task GuildPowerupEntitlementsCreateEvent_BackfillsGuildAndRaisesExpectedArguments()
	{
		var client = CreateClient();
		var guild = new DiscordGuild
		{
			Id = 804032421678153819,
			Discord = client
		};
		client.GuildsInternal[guild.Id] = guild;

		var entitlements = new List<DiscordEntitlement>
		{
			new()
			{
				Id = 1417215769351163964,
				ApplicationId = 1340102344645283891,
				UserId = 856780995629154305,
				Type = EntitlementType.GuildPowerup,
				SourceType = EntitlementSourceType.GuildPowerup,
				SkuId = 1395150923734581339,
				Sku = new DiscordSku
				{
					Id = 1395150923734581339
				}
			}
		};

		GuildPowerupEntitlementsCreateEventArgs? captured = null;
		client.GuildPowerupEntitlementsCreated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		await client.OnGuildPowerupEntitlementsCreateEventAsync(entitlements, guild.Id);

		Assert.NotNull(captured);
		Assert.Same(guild, captured!.Guild);
		Assert.Single(captured.Entitlements);
		Assert.Equal(guild.Id, captured.Entitlements[0].GuildId);
		Assert.Same(client, captured.Entitlements[0].Discord);
		Assert.NotNull(captured.Entitlements[0].Sku);
		Assert.Same(client, captured.Entitlements[0].Sku!.Discord);
		Assert.Equal(EntitlementType.GuildPowerup, captured.Entitlements[0].Type);
		Assert.Equal(EntitlementSourceType.GuildPowerup, captured.Entitlements[0].SourceType);
	}

	[Fact]
	public async Task GuildPowerupEntitlementsDeleteEvent_BackfillsGuildAndRaisesExpectedArguments()
	{
		var client = CreateClient();
		var guild = new DiscordGuild
		{
			Id = 804032421678153819,
			Discord = client
		};
		client.GuildsInternal[guild.Id] = guild;

		var entitlements = new List<DiscordEntitlement>
		{
			new()
			{
				Id = 1417215769351163964,
				ApplicationId = 1340102344645283891,
				UserId = 856780995629154305,
				Type = EntitlementType.GuildPowerup,
				SourceType = EntitlementSourceType.GuildPowerup,
				SkuId = 1395150923734581339,
				Sku = new DiscordSku
				{
					Id = 1395150923734581339
				}
			}
		};

		GuildPowerupEntitlementsDeleteEventArgs? captured = null;
		client.GuildPowerupEntitlementsDeleted += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		await client.OnGuildPowerupEntitlementsDeleteEventAsync(entitlements, guild.Id);

		Assert.NotNull(captured);
		Assert.Same(guild, captured!.Guild);
		Assert.Single(captured.Entitlements);
		Assert.Equal(guild.Id, captured.Entitlements[0].GuildId);
		Assert.Same(client, captured.Entitlements[0].Discord);
		Assert.NotNull(captured.Entitlements[0].Sku);
		Assert.Same(client, captured.Entitlements[0].Sku!.Discord);
		Assert.Equal(EntitlementType.GuildPowerup, captured.Entitlements[0].Type);
		Assert.Equal(EntitlementSourceType.GuildPowerup, captured.Entitlements[0].SourceType);
	}

	[Fact]
	public async Task GuildSoundboardSoundsUpdateEvent_RefreshesGuildCache()
	{
		var client = CreateClient();
		var guild = new DiscordGuild
		{
			Id = 804032421678153819,
			Discord = client
		};
		client.GuildsInternal[guild.Id] = guild;
		guild.SoundboardSoundsInternal[1] = new DiscordSoundboardSound
		{
			Id = 1,
			Name = "Old sound",
			Discord = client,
			GuildId = guild.Id
		};

		var sounds = new List<DiscordSoundboardSound>
		{
			new()
			{
				Id = 2,
				Name = "Fresh sound",
				Discord = client
			},
			new()
			{
				Id = 3,
				Name = "Another sound",
				Discord = client
			}
		};

		await client.OnGuildSoundboardSoundsUpdateEventAsync(sounds, guild.Id);

		Assert.Equal(2, guild.SoundboardSoundsInternal.Count);
		Assert.False(guild.SoundboardSoundsInternal.ContainsKey(1));
		Assert.True(guild.SoundboardSoundsInternal.ContainsKey(2));
		Assert.True(guild.SoundboardSoundsInternal.ContainsKey(3));
		Assert.Equal(guild.Id, guild.SoundboardSoundsInternal[2].GuildId);
		Assert.Equal(guild.Id, guild.SoundboardSoundsInternal[3].GuildId);
	}

	private static DiscordClient CreateClient()
		=> new(new DiscordConfiguration
		{
			Token = "1"
		});
}
