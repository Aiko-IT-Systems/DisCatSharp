using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Configuration.Models;
using DisCatSharp.Enums;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Xunit;

namespace DisCatSharp.Configuration.Tests;

public class ConfigurationExtensionTests
{
#region Test Classes

	private class SampleClass
	{
		public int Amount { get; set; }
		public string? Email { get; set; }
	}

	private class ClassWithArray
	{
		public int[] Values { get; set; } = [1, 2, 3, 4, 5];

		public string[] Strings { get; set; } = ["1", "2", "3", "4", "5"];
	}

	private class ClassWithEnumerable
	{
		public IEnumerable<int> Values { get; set; } = new[] { 1, 2, 3, 4, 5 };

		public IEnumerable<string> Strings { get; set; } = new[] { "1", "2", "3", "4", "5" };
	}

	private class ClassWithList
	{
		public List<string> Strings { get; set; } = ["1", "2", "3", "4", "5"];

		public List<int> Values { get; set; } = [1, 2, 3, 4, 5];
	}

	private class SampleClass2(string value)
	{
		public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(7);
		public string Name { get; set; } = "Sample";
		public string ConstructorValue { get; } = value;
	}

#endregion

	private IConfiguration EnumerableTestConfiguration() =>
		new ConfigurationBuilder()
			.AddJsonFile("enumerable-test.json")
			.Build();

	private IConfiguration HasSectionWithSuffixConfiguration() =>
		new ConfigurationBuilder()
			.AddJsonFile("section-with-suffix.json")
			.Build();

	private IConfiguration HasSectionNoSuffixConfiguration() =>
		new ConfigurationBuilder()
			.AddJsonFile("section-no-suffix.json")
			.Build();

	private IConfiguration BasicDiscordConfiguration() => new ConfigurationBuilder()
		.AddJsonFile("default-discord.json")
		.Build();

	private IConfiguration DiscordIntentsConfig() => new ConfigurationBuilder()
		.AddJsonFile("intents-discord.json")
		.Build();

	private IConfiguration DiscordHaphazardConfig() => new ConfigurationBuilder()
		.AddJsonFile("haphazard-discord.json")
		.Build();

	private IConfiguration SampleConfig() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "Sample:Amount", "200" },
			{ "Sample:Email", "test@gmail.com" }
		})
		.Build();

	private IConfiguration SampleClass2Configuration_Default() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "Random:Stuff", "Meow" },
			{ "SampleClass2:Name", "Purfection" }
		})
		.Build();

	private IConfiguration SampleClass2Configuration_Change() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "SampleClass:Timeout", "01:30:00" },
			{ "SampleClass:NotValid", "Something" }
		})
		.Build();

	private IConfiguration SampleClass2EnumerableTest() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "SampleClass:EnumerableTest", "[\"10\",\"20\",\"30\"]" }
		})
		.Build();

	private IConfiguration SampleClass2ArrayTest() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "SampleClass:ArrayTest", "[\"10\",\"20\",\"30\"]" }
		})
		.Build();

	private IConfiguration SampleClass2ListTest() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "SampleClass:ListTest", "[\"10\",\"20\",\"30\"]" }
		})
		.Build();

	[Fact]
	public void TestExtractDiscordConfig_Intents()
	{
		var source = this.DiscordIntentsConfig();

		var config = source.ExtractConfig<DiscordConfiguration>("Discord");

		var expected = DiscordIntents.GuildEmojisAndStickers | DiscordIntents.GuildMembers |
		               DiscordIntents.GuildInvites | DiscordIntents.GuildMessageReactions;

		Assert.Equal(expected, config.Intents);
	}

	[Fact]
	public void TestExtractDiscordConfig_Haphazard()
	{
		var source = this.DiscordHaphazardConfig();

		var config = source.ExtractConfig<DiscordConfiguration>("Discord");
		var expectedIntents = DiscordIntents.GuildEmojisAndStickers | DiscordIntents.GuildMembers |
		                      DiscordIntents.Guilds;

		Assert.Equal(expectedIntents, config.Intents);
		Assert.True(config.MobileStatus);
		Assert.Equal(1000, config.LargeThreshold);
		Assert.Equal(TimeSpan.FromHours(10), config.HttpTimeout);
	}

	[Fact]
	public void TestExtractDiscordConfig_Default()
	{
		var source = this.BasicDiscordConfiguration();
		var config = source.ExtractConfig<DiscordConfiguration>("Discord");

		Assert.Equal("1234567890", config.Token);
		Assert.Equal(TokenType.Bot, config.TokenType);
		Assert.Equal(LogLevel.Information, config.MinimumLogLevel);
		Assert.True(config.UseRelativeRatelimit);
		Assert.Equal("yyyy-MM-dd HH:mm:ss zzz", config.LogTimestampFormat);
		Assert.Equal(250, config.LargeThreshold);
		Assert.True(config.AutoReconnect);
		Assert.Equal(123123, config.ShardId);
		Assert.Equal(GatewayCompressionLevel.Stream, config.GatewayCompressionLevel);
		Assert.Equal(1024, config.MessageCacheSize);
		Assert.Equal(TimeSpan.FromSeconds(20), config.HttpTimeout);
		Assert.False(config.ReconnectIndefinitely);
		Assert.True(config.AlwaysCacheMembers);
		Assert.Equal(DiscordIntents.AllUnprivileged, config.Intents);
		Assert.False(config.MobileStatus);
		Assert.Equal(config.ApiChannel, ApiChannel.Stable);
		Assert.False(config.AutoRefreshChannelCache);
	}

	[Fact]
	public void TestSection()
	{
		var source = this.SampleConfig();
		var config = source.ExtractConfig<SampleClass>("Sample", null);

		Assert.Equal(200, config.Amount);
		Assert.Equal("test@gmail.com", config.Email);
	}

	[Fact]
	public void TestExtractConfig_V2_Default()
	{
		var source = this.SampleClass2Configuration_Default();
		var config = (SampleClass2)source.ExtractConfig("SampleClass", () => new SampleClass2("Test"), null);
		Assert.Equal(TimeSpan.FromMinutes(7), config.Timeout);
		Assert.Equal("Test", config.ConstructorValue);
		Assert.Equal("Sample", config.Name);
	}

	[Fact]
	public void TestExtractConfig_V2_Change()
	{
		var source = this.SampleClass2Configuration_Change();
		var config = (SampleClass2)source.ExtractConfig("SampleClass", () => new SampleClass2("Test123"), null);
		var span = new TimeSpan(0, 1, 30, 0);
		Assert.Equal(span, config.Timeout);
		Assert.Equal("Test123", config.ConstructorValue);
		Assert.Equal("Sample", config.Name);
	}

	[Fact]
	public void TestExtractConfig_V3_Default()
	{
		var source = this.SampleClass2Configuration_Default();
		var config =
			(SampleClass2)new ConfigSection(ref source, "SampleClass", null).ExtractConfig(() =>
				new SampleClass2("Meow"));

		Assert.Equal("Meow", config.ConstructorValue);
		Assert.Equal(TimeSpan.FromMinutes(7), config.Timeout);
		Assert.Equal("Sample", config.Name);
	}

	[Fact]
	public void TestExtractConfig_V3_Change()
	{
		var source = this.SampleClass2Configuration_Change();
		var config =
			(SampleClass2)new ConfigSection(ref source, "SampleClass", null).ExtractConfig(() =>
				new SampleClass2("Meow"));

		Assert.Equal("Meow", config.ConstructorValue);
		var span = new TimeSpan(0, 1, 30, 0);
		Assert.Equal(span, config.Timeout);
		Assert.Equal("Sample", config.Name);
	}

	[Fact]
	public void TestExtractConfig_Enumerable()
	{
		var source = this.EnumerableTestConfiguration();
		var config =
			(ClassWithEnumerable)new ConfigSection(ref source, "ClassWithEnumerable", null).ExtractConfig(() =>
				new ClassWithEnumerable());

		Assert.NotNull(config.Values);
		Assert.Equal(3, config.Values.Count());
		Assert.NotNull(config.Strings);
		Assert.Equal(3, config.Values.Count());
	}

	[Fact]
	public void TestExtractConfig_Array()
	{
		var source = this.EnumerableTestConfiguration();
		var config =
			(ClassWithArray)new ConfigSection(ref source, "ClassWithArray", null).ExtractConfig(() =>
				new ClassWithArray());
		Assert.NotNull(config.Values);
		Assert.Equal(3, config.Values.Length);
		Assert.NotNull(config.Strings);
		Assert.Equal(3, config.Values.Length);
	}

	[Fact]
	public void TestExtractConfig_List()
	{
		var source = this.EnumerableTestConfiguration();
		var config =
			(ClassWithList)new ConfigSection(ref source, "ClassWithList", null).ExtractConfig(() =>
				new ClassWithList());
		Assert.NotNull(config.Values);
		Assert.Equal(3, config.Values.Count);
		Assert.NotNull(config.Strings);
		Assert.Equal(3, config.Values.Count);
	}

	[Fact]
	public void TestHasSectionWithSuffix()
	{
		var source = this.HasSectionWithSuffixConfiguration();

		Assert.True(source.HasSection("DiscordConfiguration"));
		Assert.False(source.HasSection("Discord"));
		Assert.False(source.HasSection("DiscordConfiguration", null));
	}

	[Fact]
	public void TestHasSectionNoSuffix()
	{
		var source = this.HasSectionNoSuffixConfiguration();

		Assert.True(source.HasSection("Discord"));
		Assert.False(source.HasSection("DiscordConfiguration"));
		Assert.False(source.HasSection("Discord", null));
	}
}
