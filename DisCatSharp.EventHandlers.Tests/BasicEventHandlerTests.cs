using System;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

using Xunit;

namespace DisCatSharp.EventHandlers.Tests;

public class BasicEventHandlerTests
{
	[EventHandler]
	private class HandlerA
	{ }

	[EventHandler]
	private class HandlerB
	{
		[Event]
		public Task MessageCreated(DiscordClient sender, MessageCreateEventArgs args) => Task.CompletedTask;

		[Event(DiscordEvent.MessageDeleted)]
		private static Task SomeEvent(DiscordClient sender, MessageDeleteEventArgs args) => Task.CompletedTask;
	}

	[EventHandler]
	private static class HandlerC
	{
		[Event]
		public static Task MessageCreated(DiscordClient sender, MessageCreateEventArgs args) => Task.CompletedTask;
	}

	public abstract class HandlerD
	{
		[Event]
		public Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs args) => Task.CompletedTask;

		[Event]
		public static Task ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs args) => Task.CompletedTask;
	}

	private class BadHandlerA
	{
		[Event]
		public int MessageCreated(object? obj, dynamic dynamic) => 1;
	}

	private abstract class BadHandlerB
	{
		[Event]
		private static Task ThisEventDoesNotExist() => Task.CompletedTask;
	}

	private readonly DiscordClient _client = new(new()
	{
		Token = "1"
	});

	[Fact]
	public void TestUtility()
	{
		Assert.False(this.IsEventRegistered(nameof(this._client.MessageCreated)));
		this._client.MessageCreated += (_, _) => Task.CompletedTask;
		Assert.True(this.IsEventRegistered(nameof(this._client.MessageCreated)));
		Assert.Throws<ArgumentException>(() => this.IsEventRegistered(""));
		Assert.Throws<ArgumentException>(() => this.IsEventRegistered("ThisEventDoesNotExist"));
	}

	[Fact]
	public void TestUnregistrationWithoutRegistration()
	{
		this._client.UnregisterEventHandler<HandlerB>();
		this._client.UnregisterEventHandlers(Assembly.GetExecutingAssembly());
		Assert.False(this.IsEventRegistered(nameof(this._client.MessageCreated)));
		Assert.False(this.IsEventRegistered(nameof(this._client.MessageDeleted)));
	}

	[Fact]
	public void TestSimpleRegistration()
	{
		this._client.RegisterEventHandler<HandlerB>();
		Assert.True(this.IsEventRegistered(nameof(this._client.MessageCreated)));
		Assert.True(this.IsEventRegistered(nameof(this._client.MessageDeleted)));
		this._client.UnregisterEventHandler<HandlerB>();
		Assert.False(this.IsEventRegistered(nameof(this._client.MessageCreated)));
		Assert.False(this.IsEventRegistered(nameof(this._client.MessageDeleted)));

		this._client.RegisterEventHandler<HandlerD>();
		Assert.False(this.IsEventRegistered(nameof(this._client.ChannelCreated)));
		Assert.True(this.IsEventRegistered(nameof(this._client.ChannelDeleted)));
		this._client.UnregisterEventHandler<HandlerD>();
		Assert.False(this.IsEventRegistered(nameof(this._client.ChannelDeleted)));
	}

	[Fact]
	public void TestAssemblyRegistration()
	{
		this._client.RegisterEventHandlers(Assembly.GetExecutingAssembly());
		Assert.True(this.IsEventRegistered(nameof(this._client.MessageCreated)));
		Assert.True(this.IsEventRegistered(nameof(this._client.MessageDeleted)));
		this._client.UnregisterEventHandlers(Assembly.GetExecutingAssembly());
		Assert.False(this.IsEventRegistered(nameof(this._client.MessageCreated)));
		Assert.False(this.IsEventRegistered(nameof(this._client.MessageDeleted)));
	}

	[Fact]
	public void TestInvalidHandlers()
	{
		Assert.Throws<ArgumentException>(() => this._client.RegisterEventHandler<BadHandlerA>());
		Assert.Throws<ArgumentException>(() => this._client.RegisterEventHandler<BadHandlerB>());
	}

	private bool IsEventRegistered(string name)
	{
		// This is super hacky, but I think it should be good enough.
		if (name.Length == 0)
			throw new ArgumentException("name mustn't be empty");

		name = "_" + char.ToLower(name[0]) + name[1..];
		var asyncEvent = typeof(DiscordClient).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(this._client);
		dynamic handlers = asyncEvent?.GetType().GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(asyncEvent)
		                   ?? throw new ArgumentException($"Unknown event \"{name}\"");
		return handlers.Length != 0;
	}
}
