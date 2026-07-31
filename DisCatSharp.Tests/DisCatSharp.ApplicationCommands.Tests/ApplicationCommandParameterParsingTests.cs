using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Checks;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Xunit;

namespace DisCatSharp.ApplicationCommands.Tests;

public class ApplicationCommandParameterParsingTests
{
	[Fact]
	public async Task ParseParametersClearsOnlyInapplicableOptionMetadata()
	{
		var parameters = typeof(ParameterParsingCommands)
			.GetMethod(nameof(ParameterParsingCommands.MixedOptions), BindingFlags.Public | BindingFlags.Static)!
			.GetParameters()
			.Skip(1);

		var options = await ApplicationCommandsExtension.ParseParametersAsync(parameters, "mixed", null);

		var text = Assert.Single(options, option => option.Name == "text");
		Assert.Equal(ApplicationCommandOptionType.String, text.Type);
		Assert.Equal(2, text.MinimumLength);
		Assert.Equal(8, text.MaximumLength);
		Assert.Null(text.MinimumValue);
		Assert.Null(text.MaximumValue);
		Assert.Null(text.ChannelTypes);
		Assert.Null(text.FileTypes);

		var amount = Assert.Single(options, option => option.Name == "amount");
		Assert.Equal(ApplicationCommandOptionType.Integer, amount.Type);
		Assert.Equal(1, amount.MinimumValue);
		Assert.Equal(5, amount.MaximumValue);
		Assert.Null(amount.MinimumLength);
		Assert.Null(amount.MaximumLength);
		Assert.Null(amount.ChannelTypes);
		Assert.Null(amount.FileTypes);

		var channel = Assert.Single(options, option => option.Name == "channel");
		Assert.Equal(ApplicationCommandOptionType.Channel, channel.Type);
		Assert.NotNull(channel.ChannelTypes);
		Assert.Contains(ChannelType.Text, channel.ChannelTypes);
		Assert.Null(channel.FileTypes);

		var attachment = Assert.Single(options, option => option.Name == "attachment");
		Assert.Equal(ApplicationCommandOptionType.Attachment, attachment.Type);
		Assert.Null(attachment.ChannelTypes);
		Assert.NotNull(attachment.FileTypes);
		Assert.Contains(".png", attachment.FileTypes);
	}

	[Fact]
	public void DeepEqualDetectsAttachmentFileTypeChanges()
	{
		var client = new DiscordClient(new() { Token = "1" });
		var source = new DiscordApplicationCommand("upload", "upload", [
			new("attachment", "attachment", ApplicationCommandOptionType.Attachment, fileTypes: [".png"])
		]);
		var target = new DiscordApplicationCommand("upload", "upload", [
			new("attachment", "attachment", ApplicationCommandOptionType.Attachment, fileTypes: [".pdf"])
		]);

		Assert.False(ApplicationCommandEqualityChecks.DeepEqual(source, target, client));
	}

	private static class ParameterParsingCommands
	{
		public static Task MixedOptions(
			InteractionContext context,
			[Option("text", "text")]
			[MinimumLength(2)]
			[MaximumLength(8)]
			[MinimumValue(1)]
			string text,
			[Option("amount", "amount")]
			[MinimumValue(1)]
			[MaximumValue(5)]
			[MinimumLength(2)]
			int amount,
			[Option("channel", "channel")]
			[ChannelTypes(ChannelType.Text)]
			[FileTypes(".png")]
			DiscordChannel channel,
			[Option("attachment", "attachment")]
			[ChannelTypes(ChannelType.Text)]
			[FileTypes(".png")]
			DiscordAttachment attachment
		)
			=> Task.CompletedTask;
	}
}
