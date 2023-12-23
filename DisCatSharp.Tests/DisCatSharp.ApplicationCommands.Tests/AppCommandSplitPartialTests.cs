using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using SixLabors.ImageSharp.ColorSpaces;

using Xunit;
using Xunit.Abstractions;

namespace DisCatSharp.ApplicationCommands.Tests;

public class AppCommandSplitPartialTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	private readonly DiscordClient _client = new(new()
	{
		Token = "1"
	});

	internal readonly ReadOnlyDictionary<ulong, DiscordGuild> Guilds = new Dictionary<ulong, DiscordGuild>().AsReadOnly();

	public AppCommandSplitPartialTests(ITestOutputHelper testOutputHelper)
	{
		this._testOutputHelper = testOutputHelper;
	}

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
		await this._client._guildDownloadCompletedEv.InvokeAsync(this._client, new(this.Guilds, true, null!));
		while (!regFired)
			await Task.Delay(1);

		Assert.True(regFired);
		var commands = extension.RegisteredCommands;
		Assert.Single(commands);
		Assert.NotNull(commands.FirstOrDefault(x => x.Key is null).Value);
		Assert.Single(commands.FirstOrDefault(x => x.Key is null).Value);
		var command = commands.FirstOrDefault(x => x.Key is null).Value.First();
		Assert.Equal("test", command.Name);
		Assert.Equal("test", command.Description);
		Assert.NotNull(command.Options);
		Assert.All(command.Options, x => Assert.True(x.Type is ApplicationCommandOptionType.SubCommand));
		Assert.True(command.Options.Any(x => x.Name is "test_1"));
		Assert.True(command.Options.Any(x => x.Name is "test_2"));
		Assert.NotNull(command.Options.First(x => x.Name is "test_2").Options);
		var option = command.Options.First(x => x.Name is "test_2").Options;
		Assert.True(option!.Any(y => y is { Name: "user", Required: false, Type: ApplicationCommandOptionType.User }));
		this._testOutputHelper.WriteLine("Command registration successful");
	}
}
