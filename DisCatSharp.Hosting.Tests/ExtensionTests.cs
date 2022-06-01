// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Interactivity;
using DisCatSharp.Lavalink;

using Microsoft.Extensions.Configuration;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public class HostExtensionTests
{
	#region Reference to external assemblies - required to ensure they're loaded
#pragma warning disable 414

	private InteractivityConfiguration? _interactivityConfig = null;
	private LavalinkConfiguration? _lavalinkConfig = null;
	private DiscordConfiguration? _discordConfig = null;

#pragma warning restore 414
	#endregion

	private Dictionary<string, string> DefaultDiscord() =>
		new()
		{
			{ "DisCatSharp:Discord:Token", "1234567890" },
			{ "DisCatSharp:Discord:TokenType", "Bot" },
			{ "DisCatSharp:Discord:MinimumLogLevel", "Information" },
			{ "DisCatSharp:Discord:UseRelativeRateLimit", "true" },
			{ "DisCatSharp:Discord:LogTimestampFormat", "yyyy-MM-dd HH:mm:ss zzz" },
			{ "DisCatSharp:Discord:LargeThreshold", "250" },
			{ "DisCatSharp:Discord:AutoReconnect", "true" },
			{ "DisCatSharp:Discord:ShardId", "123123" },
			{ "DisCatSharp:Discord:GatewayCompressionLevel", "Stream" },
			{ "DisCatSharp:Discord:MessageCacheSize", "1024" },
			{ "DisCatSharp:Discord:HttpTimeout", "00:00:20" },
			{ "DisCatSharp:Discord:ReconnectIndefinitely", "false" },
			{ "DisCatSharp:Discord:AlwaysCacheMembers", "true" },
			{ "DisCatSharp:Discord:DiscordIntents", "AllUnprivileged" },
			{ "DisCatSharp:Discord:MobileStatus", "false" },
			{ "DisCatSharp:Discord:UseCanary", "false" },
			{ "DisCatSharp:Discord:AutoRefreshChannelCache", "false" },
			{ "DisCatSharp:Discord:Intents", "AllUnprivileged" }
		};

	public IConfiguration DiscordInteractivityConfiguration() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string>(this.DefaultDiscord())
		{
			{"DisCatSharp:Using", "[\"DisCatSharp.Interactivity\"]"} // this should be enough to automatically add the extension
            })
		.Build();

	public IConfiguration DiscordOnlyConfiguration() => new ConfigurationBuilder()
		.AddInMemoryCollection(this.DefaultDiscord())
		.Build();

	public IConfiguration DiscordInteractivityAndLavaLinkConfiguration() => new ConfigurationBuilder()
		.AddJsonFile("interactivity-lavalink.json")
		.Build();

	[Fact]
	public void DiscoverExtensions_Interactivity()
	{
		var source = this.DiscordInteractivityConfiguration();
		var discovered = source.FindImplementedExtensions();

		// Remember that DiscordConfiguration does not have an implementation type which is assignable to BaseExtension
		Assert.Single(discovered);
		var item = discovered.First();

		Assert.Equal(typeof(InteractivityConfiguration), item.Value.ConfigType);
		Assert.Equal(typeof(InteractivityExtension), item.Value.ImplementationType);
		Assert.Equal("InteractivityExtension", item.Key);
	}

	[Fact]
	public void DiscoverExtensions_InteractivityAndLavaLink()
	{
		var source = this.DiscordInteractivityAndLavaLinkConfiguration();
		var discovered = source.FindImplementedExtensions();

		Assert.Equal(2, discovered.Count);
		var first = discovered.First();
		var last = discovered.Last();

		Assert.Equal(typeof(InteractivityConfiguration), first.Value.ConfigType);
		Assert.Equal(typeof(InteractivityExtension), first.Value.ImplementationType);
		Assert.True("InteractivityExtension".Equals(first.Key, StringComparison.OrdinalIgnoreCase));

		Assert.Equal(typeof(LavalinkConfiguration), last.Value.ConfigType);
		Assert.Equal(typeof(LavalinkExtension), last.Value.ImplementationType);
		Assert.True("LavalinkExtension".Equals(last.Key, StringComparison.OrdinalIgnoreCase));
	}
}


