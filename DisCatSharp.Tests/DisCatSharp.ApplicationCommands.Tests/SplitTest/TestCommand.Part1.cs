using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Tests.SplitTest;

[SlashCommandGroup("test", "test")]
internal partial class TestCommand : ApplicationCommandsModule
{
	[SlashCommand("test_1", "test 1")]
	internal static async Task Test1Async(InteractionContext ctx)
	{
		await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Meow"));
	}
}
