using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Net.Abstractions;
using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Store;

public class PresenceCacheRegressionTests
{
	[Fact]
	public async Task GuildSyncAndPresenceUpdate_KeepSharedUserPresencesIsolatedPerGuild()
	{
		var client = CreateClient();
		var guildA = CreateGuild(client, 100);
		var guildB = CreateGuild(client, 200);
		const ulong userId = 300;

		client.UserCache[userId] = new DiscordUser
		{
			Id = userId,
			Discord = client,
			Username = "shared-user",
			Discriminator = "0001"
		};

		await client.OnGuildSyncEventAsync(guildA, false, new JArray(), [CreatePresence(userId, guildA.Id, UserStatus.Online, "Guild A")]);
		await client.OnGuildSyncEventAsync(guildB, false, new JArray(), [CreatePresence(userId, guildB.Id, UserStatus.Idle, "Guild B")]);

		PresenceUpdateEventArgs? captured = null;
		client.PresenceUpdated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		await client.OnPresenceUpdateEventAsync(
			JObject.Parse($$"""
			{
			  "guild_id": {{guildA.Id}},
			  "status": "dnd",
			  "activities": [
			    {
			      "name": "Guild A Updated",
			      "type": 0
			    }
			  ],
			  "client_status": {
			    "web": "dnd"
			  }
			}
			"""),
			new JObject
			{
				["id"] = userId
			}
		);

		Assert.Equal(UserStatus.DoNotDisturb, guildA.Presences[userId].Status);
		Assert.Equal(UserStatus.Idle, guildB.Presences[userId].Status);
		Assert.Equal("Guild B", guildB.Presences[userId].Activity?.Name);
		Assert.Same(guildA.Presences[userId], client.Presences[userId]);
		Assert.NotNull(captured);
		Assert.Equal(guildA.Id, captured!.PresenceAfter.GuildId);
		Assert.Equal(UserStatus.Online, captured.PresenceBefore?.Status);
	}

	[Fact]
	public async Task GuildMembersChunkEvent_CachesReturnedPresences()
	{
		var client = CreateClient();
		var guild = CreateGuild(client, 400);
		const ulong userId = 500;

		GuildMembersChunkEventArgs? captured = null;
		client.GuildMembersChunked += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		await client.OnGuildMembersChunkEventAsync(new JObject
		{
			["guild_id"] = guild.Id,
			["chunk_index"] = 0,
			["chunk_count"] = 1,
			["nonce"] = "presence-test",
			["members"] = new JArray(CreateMemberPayload(userId, "chunk-user")),
			["presences"] = new JArray(
				JObject.Parse($$"""
				{
				  "guild_id": {{guild.Id}},
				  "user": {
				    "id": "{{userId}}"
				  },
				  "status": "online",
				  "activities": [
				    {
				      "name": "Chunk Activity",
				      "type": 0
				    }
				  ],
				  "client_status": {
				    "web": "online"
				  }
				}
				"""))
		});

		Assert.NotNull(captured);
		Assert.Single(captured!.Presences);
		Assert.True(guild.Presences.ContainsKey(userId));
		Assert.Equal("Chunk Activity", guild.Presences[userId].Activity?.Name);
		Assert.Same(guild.Presences[userId], client.Presences[userId]);
	}

	[Fact]
	public async Task ClientGetPresences_ReturnsAllGuildScopedPresencesForSharedUser()
	{
		var client = CreateClient();
		var guildA = CreateGuild(client, 410);
		var guildB = CreateGuild(client, 420);
		const ulong userId = 430;

		client.UserCache[userId] = new DiscordUser
		{
			Id = userId,
			Discord = client,
			Username = "shared-user",
			Discriminator = "0001"
		};

		await client.OnGuildSyncEventAsync(guildA, false, new JArray(), [CreatePresence(userId, guildA.Id, UserStatus.Online, "Guild A")]);
		await client.OnGuildSyncEventAsync(guildB, false, new JArray(), [CreatePresence(userId, guildB.Id, UserStatus.Idle, "Guild B")]);

		var presences = client.GetPresences(userId);

		Assert.Equal(2, presences.Count);
		Assert.Same(guildA.Presences[userId], presences[guildA.Id]);
		Assert.Same(guildB.Presences[userId], presences[guildB.Id]);
	}

	[Fact]
	public async Task PresenceUpdate_WhenActivityCountChanges_RebuildsActivitiesAndPrimaryActivity()
	{
		var client = CreateClient();
		var guild = CreateGuild(client, 550);
		const ulong userId = 551;

		client.UserCache[userId] = new DiscordUser
		{
			Id = userId,
			Discord = client,
			Username = "activity-user",
			Discriminator = "0001"
		};

		await client.OnGuildSyncEventAsync(guild, false, new JArray(), [CreatePresence(userId, guild.Id, UserStatus.Online, "Original")]);

		await client.OnPresenceUpdateEventAsync(
			JObject.Parse($$"""
			{
			  "guild_id": {{guild.Id}},
			  "status": "online",
			  "activities": [
			    {
			      "name": "Primary Updated",
			      "type": 0
			    },
			    {
			      "name": "Secondary Updated",
			      "type": 2
			    }
			  ],
			  "client_status": {
			    "web": "online"
			  }
			}
			"""),
			new JObject
			{
				["id"] = userId
			}
		);

		var presence = guild.Presences[userId];
		Assert.NotNull(presence.Activities);
		Assert.Equal(2, presence.Activities!.Count);
		Assert.Equal("Primary Updated", presence.Activities[0].Name);
		Assert.Equal("Secondary Updated", presence.Activities[1].Name);
		Assert.Equal("Primary Updated", presence.Activity?.Name);
	}

	[Fact]
	public async Task PresenceUpdate_TracksEmbeddedAndVrClientStatusFields()
	{
		var client = CreateClient();
		var guild = CreateGuild(client, 560);
		const ulong userId = 561;

		client.UserCache[userId] = new DiscordUser
		{
			Id = userId,
			Discord = client,
			Username = "platform-user",
			Discriminator = "0001"
		};

		await client.OnGuildSyncEventAsync(guild, false, new JArray(), [CreatePresence(userId, guild.Id, UserStatus.Online, "Tracked")]);

		await client.OnPresenceUpdateEventAsync(
			JObject.Parse($$"""
			{
			  "guild_id": {{guild.Id}},
			  "status": "online",
			  "activities": [],
			  "client_status": {
			    "embedded": "idle",
			    "vr": "dnd"
			  }
			}
			"""),
			new JObject
			{
				["id"] = userId
			}
		);

		var presence = guild.Presences[userId];
		Assert.Equal(UserStatus.Idle, presence.ClientStatus.Embedded.Value);
		Assert.Equal(UserStatus.DoNotDisturb, presence.ClientStatus.Vr.Value);
	}

	[Fact]
	public async Task AggregatePresenceCacheSize_EvictsOldestAggregateEntryWithoutTouchingGuildCache()
	{
		var client = CreateClient(presenceCacheSize: 2);
		var guildA = CreateGuild(client, 610);
		var guildB = CreateGuild(client, 620);
		var guildC = CreateGuild(client, 630);

		await client.OnGuildSyncEventAsync(guildA, false, new JArray(), [CreatePresence(611, guildA.Id, UserStatus.Online, "Guild A")]);
		await client.OnGuildSyncEventAsync(guildB, false, new JArray(), [CreatePresence(621, guildB.Id, UserStatus.Idle, "Guild B")]);
		await client.OnGuildSyncEventAsync(guildC, false, new JArray(), [CreatePresence(631, guildC.Id, UserStatus.DoNotDisturb, "Guild C")]);

		Assert.Equal(2, client.Presences.Count);
		Assert.False(client.Presences.ContainsKey(611));
		Assert.True(guildA.Presences.ContainsKey(611));

		var evictedUserPresences = client.GetPresences(611);
		Assert.Single(evictedUserPresences);
		Assert.Same(guildA.Presences[611], evictedUserPresences[guildA.Id]);
	}

	[Fact]
	public async Task GuildDeleteEvent_RemovesGuildScopedPresencesAndRepairsAggregateView()
	{
		var client = CreateClient();
		var guildA = CreateGuild(client, 600);
		var guildB = CreateGuild(client, 700);
		const ulong userId = 800;

		client.UserCache[userId] = new DiscordUser
		{
			Id = userId,
			Discord = client,
			Username = "shared-user",
			Discriminator = "0001"
		};

		await client.OnGuildSyncEventAsync(guildA, false, new JArray(), [CreatePresence(userId, guildA.Id, UserStatus.Online, "Guild A")]);
		await client.OnGuildSyncEventAsync(guildB, false, new JArray(), [CreatePresence(userId, guildB.Id, UserStatus.Idle, "Guild B")]);

		await client.OnGuildDeleteEventAsync(new DiscordGuild
		{
			Id = guildB.Id,
			Discord = client
		});

		Assert.Empty(guildB.Presences);
		Assert.False(client.GuildsInternal.ContainsKey(guildB.Id));
		Assert.Same(guildA.Presences[userId], client.Presences[userId]);

		await client.OnGuildDeleteEventAsync(new DiscordGuild
		{
			Id = guildA.Id,
			Discord = client
		});

		Assert.Empty(guildA.Presences);
		Assert.False(client.Presences.ContainsKey(userId));
	}

	[Fact]
	public async Task GuildMemberRemoveEvent_RemovesGuildScopedPresence()
	{
		var client = CreateClient();
		var guild = CreateGuild(client, 900);
		const ulong userId = 901;

		client.UserCache[userId] = new DiscordUser
		{
			Id = userId,
			Discord = client,
			Username = "removable-user",
			Discriminator = "0001"
		};

		await client.OnGuildSyncEventAsync(guild, false, new JArray(), [CreatePresence(userId, guild.Id, UserStatus.Online, "Tracked")]);

		await client.OnGuildMemberRemoveEventAsync(new DisCatSharp.Net.Abstractions.TransportUser
		{
			Id = userId,
			Username = "removable-user",
			Discriminator = "0001"
		}, guild);

		Assert.False(guild.Presences.ContainsKey(userId));
		Assert.False(client.Presences.ContainsKey(userId));
	}

	private static DiscordClient CreateClient(int presenceCacheSize = 0)
		=> new(new DiscordConfiguration
		{
			Token = "1",
			PresenceCacheSize = presenceCacheSize
		});

	private static DiscordGuild CreateGuild(DiscordClient client, ulong guildId)
	{
		var guild = new DiscordGuild
		{
			Id = guildId,
			Discord = client
		};

		client.GuildsInternal[guild.Id] = guild;
		return guild;
	}

	private static DiscordPresence CreatePresence(ulong userId, ulong guildId, UserStatus status, string activityName)
		=> new()
		{
			GuildId = guildId,
			InternalUser = new()
			{
				Id = userId
			},
			Status = status,
			RawActivities =
			[
				new DisCatSharp.Net.Abstractions.TransportActivity
				{
					Name = activityName,
					ActivityType = ActivityType.Playing
				}
			],
			ClientStatus = new DiscordClientStatus
			{
				Web = status
			}
		};

	private static JObject CreateMemberPayload(ulong userId, string username)
		=> JObject.Parse($$"""
		{
		  "user": {
		    "id": "{{userId}}",
		    "username": "{{username}}",
		    "discriminator": "0001"
		  },
		  "roles": [],
		  "joined_at": "2024-01-01T00:00:00+00:00",
		  "deaf": false,
		  "mute": false
		}
		""");
}
