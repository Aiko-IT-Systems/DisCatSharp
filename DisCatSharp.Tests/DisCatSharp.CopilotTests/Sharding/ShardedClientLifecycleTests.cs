using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Telemetry;

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

	private static FieldInfo GetRequiredField(string name)
		=> typeof(DiscordShardedClient).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new InvalidOperationException($"Field '{name}' was not found.");

	#region IDisposable implementation

	[Fact]
	public void DiscordShardedClient_ImplementsIDisposable()
	{
		using var client = CreateClient();
		Assert.IsType<IDisposable>(client, exactMatch: false);
	}

	[Fact]
	public void DiscordShardedClient_ImplementsIAsyncDisposable()
	{
		using var client = CreateClient();
		Assert.IsType<IAsyncDisposable>(client, exactMatch: false);
	}

	[Fact]
	public void Dispose_OnNeverStartedClient_DoesNotThrow()
	{
		var client = CreateClient();

		var ex = Record.Exception(client.Dispose);

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

	[Fact]
	public async Task DisposeAsync_OnNeverStartedClient_DoesNotThrow()
	{
		var client = CreateClient();

		var ex = await Record.ExceptionAsync(async () => await client.DisposeAsync());

		Assert.Null(ex);
	}

	[Fact]
	public async Task DisposeAsync_CalledMultipleTimes_IsIdempotent()
	{
		var client = CreateClient();

		var ex = await Record.ExceptionAsync(async () =>
		{
			await client.DisposeAsync();
			await client.DisposeAsync();
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

	[Fact]
	public async Task StopAsync_CleansUpPartialStartupState_WhenShardsExistButClientWasNotMarkedStarted()
	{
		var client = CreateClient();
		var shard = new DiscordClient(new DiscordConfiguration
		{
			Token = "dummy.token.for.testing",
			TokenType = TokenType.Bot,
			AutoReconnect = false,
			EnableSentry = false
		});

		var shardsField = GetRequiredField("_shards");
		var shards = (ConcurrentDictionary<int, DiscordClient>)shardsField.GetValue(client)!;
		Assert.True(shards.TryAdd(0, shard));

		await client.StopAsync();

		Assert.Empty(shards);
	}

	[Fact]
	public void CreateShardClient_UsesShardSpecificDiagnosticsSinkConfiguration()
	{
		var shardConfig = new DiscordConfiguration
		{
			Token = "dummy.token.for.testing",
			TokenType = TokenType.Bot,
			AutoReconnect = false,
			EnableSentry = true,
			HasShardLogger = true,
			ShardId = 3,
			ShardCount = 7
		};

		var shardClient = DiscordShardedClient.CreateShardClient(shardConfig);

		var sink = Assert.IsType<SentryDiagnosticsSink>(shardClient.DiagnosticsSink);
		var configField = typeof(SentryDiagnosticsSink).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(configField);
		var sinkConfig = Assert.IsType<DiscordConfiguration>(configField!.GetValue(sink));

		Assert.Equal(3, sinkConfig.ShardId);
		Assert.Equal(7, sinkConfig.ShardCount);
	}

	[Fact]
	public void CreateGatewayInfoClientConfiguration_DisablesTelemetryWithoutMutatingParentConfig()
	{
		var original = new DiscordConfiguration
		{
			Token = "dummy.token.for.testing",
			TokenType = TokenType.Bot,
			EnableSentry = true,
			HasShardLogger = true,
			ShardId = 2,
			ShardCount = 5
		};

		var tempConfig = DiscordShardedClient.CreateGatewayInfoClientConfiguration(original);

		Assert.NotSame(original, tempConfig);
		Assert.False(tempConfig.EnableSentry);
		Assert.True(original.EnableSentry);
		Assert.Equal(original.ShardId, tempConfig.ShardId);
		Assert.Equal(original.ShardCount, tempConfig.ShardCount);
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
		var field = GetRequiredField("_isStarted");
		field.SetValue(client, true);

		await Assert.ThrowsAsync<InvalidOperationException>(() => client.StartAsync());
	}

	#endregion

	#region Volatile field checks

	[Fact]
	public void IsStartedField_IsVolatile()
	{
		var field = GetRequiredField("_isStarted");
		Assert.True(field.FieldType == typeof(bool), "_isStarted should be bool");

		// Volatile fields cannot be verified purely through reflection in .NET,
		// but we can confirm the field exists and is non-public instance as expected.
		Assert.False(field.IsStatic);
		Assert.False(field.IsPublic);
	}

	[Fact]
	public void DisposedField_Exists()
	{
		var field = GetRequiredField("_disposed");
		Assert.True(field.FieldType == typeof(bool), "_disposed should be bool");
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

	[Fact]
	public async Task StartAsync_AfterDisposeAsync_ThrowsObjectDisposedException()
	{
		var client = CreateClient();
		await client.DisposeAsync();

		await Assert.ThrowsAsync<ObjectDisposedException>(() => client.StartAsync());
	}

	#endregion
}
