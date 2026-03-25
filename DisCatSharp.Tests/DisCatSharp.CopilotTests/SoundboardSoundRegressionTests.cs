using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;

using Xunit;

namespace DisCatSharp.Copilot.Tests;

public class SoundboardSoundRegressionTests
{
	[Fact]
	public void RestGuildSoundboardSoundsResponse_GetItems_HydratesNestedSounds()
	{
		var client = CreateClient();
		const ulong guildId = 804032421678153819;
		var response = new RestGuildSoundboardSoundsResponse
		{
			Items =
			[
				new DiscordSoundboardSound
				{
					Id = 1,
					Name = "Fresh sound"
				},
				new DiscordSoundboardSound
				{
					Id = 2,
					Name = "Remote sound",
					GuildId = 42
				}
			]
		};

		var sounds = response.GetItems(client, guildId);

		Assert.Collection(
			sounds,
			sound =>
			{
				Assert.Same(client, sound.Discord);
				Assert.Equal(guildId, sound.GuildId);
			},
			sound =>
			{
				Assert.Same(client, sound.Discord);
				Assert.Equal((ulong)42, sound.GuildId);
			}
		);
	}

	private static DiscordClient CreateClient()
		=> new(new DiscordConfiguration
		{
			Token = "1"
		});
}
