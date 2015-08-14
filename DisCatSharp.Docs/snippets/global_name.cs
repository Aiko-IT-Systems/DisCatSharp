using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
namespace Bot.Commands;

internal class SlashCommand : ApplicationCommandsModule
{
	[SlashCommand("execute", "Execute command")]
	internal static async Task ExecuteCommandAsync(InteractionContext ctx)
	{
		var name = ctx.User.IsMigrated ? ctx.User.UsernameWithGlobalName : ctx.User.UsernameWithDiscriminator;
		await ctx.CreateResponseAsync(
			InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder().WithContent($"Your name: {name}")
		);
	}
}
