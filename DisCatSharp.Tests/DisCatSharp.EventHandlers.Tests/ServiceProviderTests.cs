using System;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace DisCatSharp.EventHandlers.Tests;

public class ServiceProviderTests
{
	private class Resource
	{ }

	private class Handler
	{
		public Handler(Resource res)
		{ }
	}

	[Fact]
	public void Test()
	{
		var poorClient = new DiscordClient(new()
		{
			Token = "1"
		});
		Assert.ThrowsAny<Exception>(() => poorClient.RegisterEventHandler<Handler>());

		var richClient = new DiscordClient(new()
		{
			Token = "2",
			ServiceProvider = new ServiceCollection().AddSingleton<Resource>().BuildServiceProvider()
		});
		richClient.RegisterEventHandler<Handler>(); // May not throw.
	}
}
