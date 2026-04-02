using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Net;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Tests for <see cref="BucketRegistry" /> — validates bucket resolution, hash remap atomicity,
///     and worker identity stability without requiring an actual HTTP connection.
/// </summary>
public class BucketRegistryTests
{
	/// <summary>
	///     Creates a <see cref="BucketRegistry" /> for testing, using a NullLogger and default config.
	/// </summary>
	private static BucketRegistry CreateTestRegistry()
	{
		var logger = NullLogger<BucketRegistry>.Instance;
		var config = new RestAdvancedConfiguration();
		// We pass null for RestClient since tests don't execute HTTP requests.
		// Worker creation will capture this, but we won't exercise the worker loop.
		return new BucketRegistry(null!, config, logger, useResetAfter: false, TimeSpan.FromSeconds(60));
	}

	[Fact]
	public void GetBucket_ReturnsSameBucketForSameRouteParams()
	{
		var registry = CreateTestRegistry();
		var routeParams = new { guild_id = "123", channel_id = "456", webhook_id = "" };

		var bucket1 = registry.GetBucket(RestRequestMethod.POST, "/channels/:channel_id/messages", routeParams, out var url1);
		var bucket2 = registry.GetBucket(RestRequestMethod.POST, "/channels/:channel_id/messages", routeParams, out var url2);

		Assert.Same(bucket1, bucket2);
		Assert.Equal(url1, url2);
		Assert.Equal("/channels/456/messages", url1);
	}

	[Fact]
	public void GetBucket_ReturnsDifferentBucketsForDifferentGuilds()
	{
		var registry = CreateTestRegistry();
		var params1 = new { guild_id = "111", channel_id = "456", webhook_id = "" };
		var params2 = new { guild_id = "222", channel_id = "456", webhook_id = "" };

		var bucket1 = registry.GetBucket(RestRequestMethod.GET, "/guilds/:guild_id/channels", params1, out _);
		var bucket2 = registry.GetBucket(RestRequestMethod.GET, "/guilds/:guild_id/channels", params2, out _);

		Assert.NotSame(bucket1, bucket2);
	}

	[Fact]
	public void GetBucket_InitialBucketIsUnlimited()
	{
		var registry = CreateTestRegistry();
		var routeParams = new { guild_id = "", channel_id = "100", webhook_id = "" };

		var bucket = registry.GetBucket(RestRequestMethod.GET, "/channels/:channel_id", routeParams, out _);

		Assert.True(bucket.IsUnlimited);
		Assert.Contains("unlimited", bucket.Hash);
	}

	[Fact]
	public void UpdateHashCaches_RemapProducesConsistentState()
	{
		var registry = CreateTestRegistry();
		var routeParams = new { guild_id = "", channel_id = "789", webhook_id = "" };
		const string route = "/channels/:channel_id/messages";

		// Get initial unlimited bucket
		var bucket = registry.GetBucket(RestRequestMethod.POST, route, routeParams, out _);
		var oldBucketId = bucket.BucketId;
		Assert.True(bucket.IsUnlimited);

		// Simulate a response with a real hash from Discord
		var request = new RestRequest(
			(BaseDiscordClient)null!,
			bucket,
			new Uri("https://discord.com/api/v10/channels/789/messages"),
			RestRequestMethod.POST,
			route
		);

		const string realHash = "abc123def456";
		registry.UpdateHashCaches(request, bucket, realHash);

		// Verify: bucket is no longer unlimited
		Assert.False(bucket.IsUnlimited);
		Assert.Equal(realHash, bucket.Hash);

		// Verify: new bucket ID reflects the new hash
		var expectedNewId = RateLimitBucket.GenerateBucketId(realHash, "", "789", "");
		Assert.Equal(expectedNewId, bucket.BucketId);

		// Verify: a subsequent GetBucket with the same route returns the same bucket object
		var bucket2 = registry.GetBucket(RestRequestMethod.POST, route, routeParams, out _);
		Assert.Same(bucket, bucket2);
	}

	[Fact]
	public void UpdateHashCaches_NullHash_RemovesBucket()
	{
		var registry = CreateTestRegistry();
		var routeParams = new { guild_id = "", channel_id = "100", webhook_id = "" };
		const string route = "/channels/:channel_id";

		var bucket = registry.GetBucket(RestRequestMethod.GET, route, routeParams, out _);
		Assert.Equal(1, registry.BucketCount);

		var request = new RestRequest(
			(BaseDiscordClient)null!,
			bucket,
			new Uri("https://discord.com/api/v10/channels/100"),
			RestRequestMethod.GET,
			route
		);

		// Null hash means "remove this unlimited bucket"
		registry.UpdateHashCaches(request, bucket, null);

		// Bucket should be removed from the registry
		Assert.Equal(0, registry.BucketCount);
	}

	[Fact]
	public void UpdateHashCaches_SkipsIfNotUnlimited()
	{
		var registry = CreateTestRegistry();
		var routeParams = new { guild_id = "", channel_id = "789", webhook_id = "" };
		const string route = "/channels/:channel_id/messages";

		var bucket = registry.GetBucket(RestRequestMethod.POST, route, routeParams, out _);

		var request = new RestRequest(
			(BaseDiscordClient)null!,
			bucket,
			new Uri("https://discord.com/api/v10/channels/789/messages"),
			RestRequestMethod.POST,
			route
		);

		// First remap — should succeed
		registry.UpdateHashCaches(request, bucket, "hash_v1");
		Assert.Equal("hash_v1", bucket.Hash);

		// Second remap — bucket is no longer unlimited, should be skipped
		registry.UpdateHashCaches(request, bucket, "hash_v2");
		Assert.Equal("hash_v1", bucket.Hash);
	}

	[Fact]
	public void GetOrCreateWorker_ReturnsSameWorkerForSameBucket()
	{
		var registry = CreateTestRegistry();
		var bucket = new RateLimitBucket("test_hash", "", "100", "");

		// Note: This will create a worker with null RestClient, but we won't run the loop
		var worker1 = registry.GetOrCreateWorker(bucket);
		var worker2 = registry.GetOrCreateWorker(bucket);

		Assert.Same(worker1, worker2);
		Assert.Equal(1, registry.WorkerCount);
	}

	[Fact]
	public void GetOrCreateWorker_DifferentBucketsGetDifferentWorkers()
	{
		var registry = CreateTestRegistry();
		var bucket1 = new RateLimitBucket("hash1", "", "100", "");
		var bucket2 = new RateLimitBucket("hash2", "", "200", "");

		var worker1 = registry.GetOrCreateWorker(bucket1);
		var worker2 = registry.GetOrCreateWorker(bucket2);

		Assert.NotSame(worker1, worker2);
		Assert.Equal(2, registry.WorkerCount);
	}

	[Fact]
	public void Dispose_ClearsAllCollections()
	{
		var registry = CreateTestRegistry();
		var routeParams = new { guild_id = "", channel_id = "100", webhook_id = "" };

		registry.GetBucket(RestRequestMethod.GET, "/channels/:channel_id", routeParams, out _);
		Assert.True(registry.BucketCount > 0);

		registry.Dispose();

		Assert.Equal(0, registry.BucketCount);
		Assert.Equal(0, registry.WorkerCount);
		Assert.True(registry.IsEmpty);
	}

	[Fact]
	public async Task ConcurrentGetBucket_AndUpdateHashCaches_DoesNotCreateDuplicates()
	{
		var registry = CreateTestRegistry();
		const string route = "/channels/:channel_id/messages";
		var routeParams = new { guild_id = "", channel_id = "999", webhook_id = "" };

		// Get initial bucket (unlimited)
		var initialBucket = registry.GetBucket(RestRequestMethod.POST, route, routeParams, out _);

		// Create a request for the remap
		var request = new RestRequest(
			(BaseDiscordClient)null!,
			initialBucket,
			new Uri("https://discord.com/api/v10/channels/999/messages"),
			RestRequestMethod.POST,
			route
		);

		// Run concurrent GetBucket + UpdateHashCaches
		var seenBuckets = new ConcurrentBag<RateLimitBucket>();
		const int concurrency = 50;
		var barrier = new Barrier(concurrency + 1);

		var tasks = new Task[concurrency + 1];

		// One task does the remap
		tasks[0] = Task.Run(() =>
		{
			barrier.SignalAndWait();
			registry.UpdateHashCaches(request, initialBucket, "real_hash_abc");
		});

		// All other tasks do concurrent GetBucket calls
		for (var i = 1; i <= concurrency; i++)
		{
			tasks[i] = Task.Run(() =>
			{
				barrier.SignalAndWait();
				var b = registry.GetBucket(RestRequestMethod.POST, route, routeParams, out _);
				seenBuckets.Add(b);
			});
		}

		await Task.WhenAll(tasks);

		// After the remap, all new GetBucket calls should return the same remapped bucket
		var postRemapBucket = registry.GetBucket(RestRequestMethod.POST, route, routeParams, out _);

		// The post-remap bucket should be the same object as the initial one (remapped in place)
		Assert.Same(initialBucket, postRemapBucket);

		// Verify: every bucket seen during concurrency is either the initial bucket or the same object
		foreach (var b in seenBuckets)
			Assert.Same(initialBucket, b);
	}
}
