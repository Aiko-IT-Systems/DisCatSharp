using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Xunit;
using Xunit.Abstractions;

namespace DisCatSharp.ApplicationCommands.Tests;

public class AppCommandSplitPartialTests(ITestOutputHelper testOutputHelper)
{
	private readonly DiscordClient _client = new(new()
	{
		Token = "1"
	});

	internal readonly ReadOnlyDictionary<ulong, DiscordGuild> Guilds = new Dictionary<ulong, DiscordGuild>().AsReadOnly();

	[Fact(DisplayName = "Test of registration for commands split across files")]
	public async Task TestSplitRegistrationAndProperLocalRegistration()
	{
		var extension = this._client.UseApplicationCommands(new()
		{
			UnitTestMode = true,
			EnableDefaultHelp = false
		});
		var regFired = false;
		extension.GlobalApplicationCommandsRegistered += (_, args) =>
		{
			regFired = true;
			return Task.CompletedTask;
		};
		extension.RegisterGlobalCommands<SplitTest.TestCommand>();
		await this._client.GuildDownloadCompletedEv.InvokeAsync(this._client, new(this.Guilds, true, null!));
		while (!regFired)
			await Task.Delay(1);

		Assert.True(regFired);
		var commands = extension.RegisteredCommands.ToList();
		Assert.Single(commands);
		Assert.NotNull(commands.FirstOrDefault(x => x.Key is null).Value);
		Assert.Single(commands.FirstOrDefault(x => x.Key is null).Value);
		var command = commands.First(x => x.Key is null).Value[0];
		Assert.Equal("test", command.Name);
		Assert.Equal("test", command.Description);
		var attributes = command.CommandType?.GetCustomAttributes(typeof(ApplicationCommandRequireDirectMessageAttribute), true);
		Assert.NotNull(attributes);
		Assert.Single(attributes);
		Assert.NotNull(command.Options);
		Assert.All(command.Options, x => Assert.True(x.Type is ApplicationCommandOptionType.SubCommand));
		Assert.Contains(command.Options, x => x.Name is "test_1");
		Assert.Contains(command.Options, x => x.Name is "test_2");
		Assert.NotNull(command.Options.First(x => x.Name is "test_2").Options);
		var option = command.Options.First(x => x.Name is "test_2").Options;
		Assert.Contains(option!, y => y is { Name: "user", Required: false, Type: ApplicationCommandOptionType.User });
		testOutputHelper.WriteLine("Command registration successful");
	}
}
