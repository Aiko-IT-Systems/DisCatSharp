using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Store;

public class VoiceStateUpdateRegressionTests
{
	private const ulong GuildId = 804032421678153819;
	private const ulong UserId = 856780995629154305;

	[Fact]
	public async Task VoiceStateUpdate_WithoutMemberPayload_AndUncachedGuild_DoesNotThrow()
	{
		var client = CreateClient();
		VoiceStateUpdateEventArgs? captured = null;
		client.VoiceStateUpdated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		var raw = JObject.Parse($$"""
			{
				"guild_id": "{{GuildId}}",
				"channel_id": null,
				"user_id": "{{UserId}}",
				"session_id": "abc",
				"deaf": false,
				"mute": false,
				"self_deaf": false,
				"self_mute": false,
				"self_video": false,
				"suppress": false
			}
			""");

		await client.OnVoiceStateUpdateEventAsync(raw);

		Assert.NotNull(captured);
		Assert.Equal(UserId, captured!.After.UserId);
	}

	[Fact]
	public async Task VoiceStateUpdate_WithCachedMember_UpdatesMuteAndDeafen()
	{
		var client = CreateClient();
		var guild = new DiscordGuild
		{
			Id = GuildId,
			Discord = client
		};
		client.GuildsInternal[guild.Id] = guild;
		var member = new DiscordMember
		{
			Id = UserId,
			GuildId = GuildId,
			Discord = client
		};
		guild.MembersInternal[UserId] = member;

		var raw = JObject.Parse($$"""
			{
				"guild_id": "{{GuildId}}",
				"channel_id": null,
				"user_id": "{{UserId}}",
				"session_id": "abc",
				"deaf": true,
				"mute": true,
				"self_deaf": false,
				"self_mute": false,
				"self_video": false,
				"suppress": false
			}
			""");

		await client.OnVoiceStateUpdateEventAsync(raw);

		Assert.True(member.IsMuted);
		Assert.True(member.IsDeafened);
	}

	private static DiscordClient CreateClient()
		=> new(new DiscordConfiguration
		{
			Token = "1",
			Gateway =
			{
				Advanced =
				{
					DispatchMode = Enums.GatewayDispatchMode.SequentialHandlers
				}
			}
		});
}
