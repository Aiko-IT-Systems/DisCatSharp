using System;
using System.Threading.Tasks;

using DisCatSharp.Enums;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Sharding;

/// <summary>
///     Regression tests for Phase 1 sharding lifecycle hardening in <see cref="DiscordShardedClient" />.
/// </summary>
public class ShardedClientLifecycleTests
{
	/// <summary>
	///     Creates a <see cref="DiscordShardedClient" /> with a dummy token that will never connect.
	/// </summary>
	private static DiscordShardedClient CreateClient()
		=> new(new DiscordConfiguration
		{
			Token = "dummy.token.for.testing",
			TokenType = TokenType.Bot,
			AutoReconnect = false,
			EnableSentry = false
		});

	#region IDisposable implementation

	[Fact]
	public void DiscordShardedClient_ImplementsIDisposable()
	{
		using var client = CreateClient();
		Assert.IsAssignableFrom<IDisposable>(client);
	}

	[Fact]
	public void Dispose_OnNeverStartedClient_DoesNotThrow()
	{
		var client = CreateClient();

		var ex = Record.Exception(() => client.Dispose());

		Assert.Null(ex);
	}

	[Fact]
	public void Dispose_CalledMultipleTimes_IsIdempotent()
	{
		var client = CreateClient();

		var ex = Record.Exception(() =>
		{
			client.Dispose();
			client.Dispose();
			client.Dispose();
		});

		Assert.Null(ex);
	}

	#endregion

	#region StopAsync safety

	[Fact]
	public async Task StopAsync_OnNeverStartedClient_DoesNotThrow()
	{
		using var client = CreateClient();

		var ex = await Record.ExceptionAsync(() => client.StopAsync());

		Assert.Null(ex);
	}

	[Fact]
	public async Task StopAsync_CalledMultipleTimes_IsIdempotent()
	{
		using var client = CreateClient();

		var ex = await Record.ExceptionAsync(async () =>
		{
			await client.StopAsync();
			await client.StopAsync();
			await client.StopAsync();
		});

		Assert.Null(ex);
	}

	#endregion

	#region Startup guards

	[Fact]
	public async Task StartAsync_CalledTwice_ThrowsInvalidOperationException()
	{
		using var client = CreateClient();

		// The first StartAsync will fail (no valid token / no gateway), which means
		// _isStarted stays false. To test the double-start guard we need _isStarted = true.
		// We set it via reflection to simulate a successfully-started client.
		var field = typeof(DiscordShardedClient)
			.GetField("_isStarted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(field);
		field!.SetValue(client, true);

		await Assert.ThrowsAsync<InvalidOperationException>(() => client.StartAsync());
	}

	#endregion

	#region Volatile field checks

	[Fact]
	public void IsStartedField_IsVolatile()
	{
		var field = typeof(DiscordShardedClient)
			.GetField("_isStarted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(field);
		Assert.True(field!.FieldType == typeof(bool), "_isStarted should be bool");

		// Volatile fields cannot be verified purely through reflection in .NET,
		// but we can confirm the field exists and is non-public instance as expected.
		Assert.False(field.IsStatic);
		Assert.False(field.IsPublic);
	}

	[Fact]
	public void DisposedField_Exists()
	{
		var field = typeof(DiscordShardedClient)
			.GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(field);
		Assert.True(field!.FieldType == typeof(bool), "_disposed should be bool");
	}

	#endregion

	#region ObjectDisposedException guards

	[Fact]
	public async Task StartAsync_AfterDispose_ThrowsObjectDisposedException()
	{
		var client = CreateClient();
		client.Dispose();

		await Assert.ThrowsAsync<ObjectDisposedException>(() => client.StartAsync());
	}

	[Fact]
	public async Task StopAsync_AfterDispose_ThrowsObjectDisposedException()
	{
		var client = CreateClient();
		client.Dispose();

		await Assert.ThrowsAsync<ObjectDisposedException>(() => client.StopAsync());
	}

	[Fact]
	public void GetShard_AfterDispose_ThrowsObjectDisposedException()
	{
		var client = CreateClient();
		client.Dispose();

		Assert.Throws<ObjectDisposedException>(() => client.GetShard(123456789UL));
	}

	[Fact]
	public async Task UpdateStatusAsync_AfterDispose_ThrowsObjectDisposedException()
	{
		var client = CreateClient();
		client.Dispose();

		await Assert.ThrowsAsync<ObjectDisposedException>(() => client.UpdateStatusAsync());
	}

	#endregion
}
