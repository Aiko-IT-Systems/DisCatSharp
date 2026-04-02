using System;
using System.Threading.Tasks;

using DisCatSharp.Net;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Tests for <see cref="BucketWorker.MigrateQueueTo" /> and the bucket-merge path
///     in <see cref="BucketRegistry.UpdateHashCaches" />.
/// </summary>
public class BucketWorkerMigrationTests
{
	/// <summary>
	///     Creates a <see cref="BucketRegistry" /> for testing, using a NullLogger and default config.
	/// </summary>
	private static BucketRegistry CreateTestRegistry()
		=> new(null!, new RestAdvancedConfiguration(), NullLogger<BucketRegistry>.Instance, useResetAfter: false, TimeSpan.FromSeconds(60));

	/// <summary>
	///     Creates a <see cref="BucketWorker" /> for testing.
	///     The RestClient is null so the worker cannot execute HTTP, but enqueue/migrate paths work.
	/// </summary>
	private static BucketWorker CreateTestWorker(RateLimitBucket bucket)
		=> new(null!, bucket, new RestAdvancedConfiguration(), NullLogger<BucketWorker>.Instance);

	/// <summary>
	///     Creates a <see cref="RestRequest" /> for testing with the given bucket.
	/// </summary>
	private static RestRequest CreateTestRequest(RateLimitBucket bucket, string url = "https://discord.com/api/v10/test")
		=> new(
			(BaseDiscordClient)null!,
			bucket,
			new Uri(url),
			RestRequestMethod.GET,
			"/test"
		);

	// ── MigrateQueueTo ──────────────────────────────────────────────────────

	[Fact]
	public void MigrateQueueTo_TransfersQueuedRequests()
	{
		var sourceBucket = new RateLimitBucket("old_hash", "", "100", "");
		var targetBucket = new RateLimitBucket("new_hash", "", "100", "");

		using var source = CreateTestWorker(sourceBucket);
		using var target = CreateTestWorker(targetBucket);

		// Enqueue 3 requests into source (worker loop won't run — no RestClient)
		var r1 = CreateTestRequest(sourceBucket);
		var r2 = CreateTestRequest(sourceBucket);
		var r3 = CreateTestRequest(sourceBucket);

		source.Enqueue(r1);
		source.Enqueue(r2);
		source.Enqueue(r3);

		// Migrate — verify count
		var migrated = source.MigrateQueueTo(target);

		Assert.Equal(3, migrated);

		// Verify source no longer accepts work (writer completed)
		var postMigrate = CreateTestRequest(sourceBucket);
		source.Enqueue(postMigrate);
		Assert.True(postMigrate.WaitForCompletionAsync().IsFaulted);
	}

	[Fact]
	public void MigrateQueueTo_ReturnsCorrectCount()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		using var source = CreateTestWorker(bucket);
		using var target = CreateTestWorker(bucket);

		// Empty source → 0 migrated
		Assert.Equal(0, source.MigrateQueueTo(target));

		// Re-create source since writer is completed after first migration
		var bucket2 = new RateLimitBucket("hash2", "", "200", "");
		using var source2 = CreateTestWorker(bucket2);

		source2.Enqueue(CreateTestRequest(bucket2));
		source2.Enqueue(CreateTestRequest(bucket2));

		Assert.Equal(2, source2.MigrateQueueTo(target));
	}

	[Fact]
	public void MigrateQueueTo_DoesNotCancelSourceCts()
	{
		var sourceBucket = new RateLimitBucket("hash", "", "100", "");
		var targetBucket = new RateLimitBucket("hash2", "", "200", "");
		using var source = CreateTestWorker(sourceBucket);
		using var target = CreateTestWorker(targetBucket);

		source.Enqueue(CreateTestRequest(sourceBucket));

		// Migrate — the source CTS should NOT be cancelled (let in-flight drain)
		var migrated = source.MigrateQueueTo(target);

		// The source worker is NOT disposed — verify it can still be disposed cleanly afterward
		// (this would throw if the CTS was already disposed/cancelled improperly)
		Assert.Equal(1, migrated);
		source.Dispose();
	}

	[Fact]
	public void MigrateQueueTo_SourceRejectsNewWorkAfterMigration()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		using var source = CreateTestWorker(bucket);
		using var target = CreateTestWorker(bucket);

		source.MigrateQueueTo(target);

		// Source's writer is completed — new enqueues should fail the request
		var request = CreateTestRequest(bucket);
		source.Enqueue(request);

		// The request should be faulted since the writer is completed
		Assert.True(request.WaitForCompletionAsync().IsFaulted || request.WaitForCompletionAsync().IsCompleted);
	}

	// ── EnsureRunning guard ─────────────────────────────────────────────────

	[Fact]
	public void EnsureRunning_DoesNotRestartAfterDispose()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		var worker = CreateTestWorker(bucket);

		// Dispose first
		worker.Dispose();

		// Enqueue after dispose — should fault the request, NOT restart the loop
		var request = CreateTestRequest(bucket);
		worker.Enqueue(request);

		Assert.False(worker.IsAlive);

		// The request should be faulted
		var waitTask = request.WaitForCompletionAsync();
		Assert.True(waitTask.IsFaulted);
	}

	// ── BucketRegistry merge case ───────────────────────────────────────────

	[Fact]
	public void BucketRegistry_MergeCase_MigratesWork()
	{
		var registry = CreateTestRegistry();

		// Route A and Route B initially get separate unlimited buckets
		var paramsA = new { guild_id = "", channel_id = "100", webhook_id = "" };
		var paramsB = new { guild_id = "", channel_id = "100", webhook_id = "" };

		const string routeA = "/channels/:channel_id/messages";
		const string routeB = "/channels/:channel_id/pins";

		var bucketA = registry.GetBucket(RestRequestMethod.POST, routeA, paramsA, out _);
		var bucketB = registry.GetBucket(RestRequestMethod.GET, routeB, paramsB, out _);

		// They should be different buckets (different routes → different unlimited hashes)
		Assert.NotSame(bucketA, bucketB);
		Assert.True(bucketA.IsUnlimited);
		Assert.True(bucketB.IsUnlimited);

		// Create workers and enqueue work into both
		var workerA = registry.GetOrCreateWorker(bucketA);
		var workerB = registry.GetOrCreateWorker(bucketB);

		var reqA = CreateTestRequest(bucketA, "https://discord.com/api/v10/channels/100/messages");
		var reqB1 = CreateTestRequest(bucketB, "https://discord.com/api/v10/channels/100/pins");
		var reqB2 = CreateTestRequest(bucketB, "https://discord.com/api/v10/channels/100/pins");

		workerA.Enqueue(reqA);
		workerB.Enqueue(reqB1);
		workerB.Enqueue(reqB2);

		Assert.Equal(2, registry.WorkerCount);

		// Discord reveals Route A has real hash "shared_hash_xyz"
		var requestA = new RestRequest(
			(BaseDiscordClient)null!,
			bucketA,
			new Uri("https://discord.com/api/v10/channels/100/messages"),
			RestRequestMethod.POST,
			routeA
		);
		registry.UpdateHashCaches(requestA, bucketA, "shared_hash_xyz");

		Assert.False(bucketA.IsUnlimited);
		Assert.Equal("shared_hash_xyz", bucketA.Hash);

		// Now Discord reveals Route B ALSO has real hash "shared_hash_xyz" — merge case!
		var requestB = new RestRequest(
			(BaseDiscordClient)null!,
			bucketB,
			new Uri("https://discord.com/api/v10/channels/100/pins"),
			RestRequestMethod.GET,
			routeB
		);
		registry.UpdateHashCaches(requestB, bucketB, "shared_hash_xyz");

		// After merge, bucketA's worker should still be in the registry
		var finalWorker = registry.GetOrCreateWorker(bucketA);
		Assert.Same(workerA, finalWorker);

		// bucketB's worker should have been removed from registry
		Assert.Equal(1, registry.WorkerCount);
	}

	[Fact]
	public void BucketRegistry_MigrateBucketWorker_NoSourceWorker_ReturnsZero()
	{
		var registry = CreateTestRegistry();
		var sourceBucket = new RateLimitBucket("hash_a", "", "100", "");
		var targetBucket = new RateLimitBucket("hash_b", "", "100", "");

		// No worker created for source — migration should be a no-op
		var migrated = registry.MigrateBucketWorker(sourceBucket, targetBucket);

		Assert.Equal(0, migrated);
	}

	[Fact]
	public void BucketRegistry_MigrateBucketWorker_RemovesSourceWorkerFromRegistry()
	{
		var registry = CreateTestRegistry();
		var sourceBucket = new RateLimitBucket("hash_a", "", "100", "");
		var targetBucket = new RateLimitBucket("hash_b", "", "100", "");

		// Create workers for both
		var sourceWorker = registry.GetOrCreateWorker(sourceBucket);
		_ = registry.GetOrCreateWorker(targetBucket);

		Assert.Equal(2, registry.WorkerCount);

		// Enqueue work into source
		sourceWorker.Enqueue(CreateTestRequest(sourceBucket));

		// Migrate
		var migrated = registry.MigrateBucketWorker(sourceBucket, targetBucket);

		Assert.Equal(1, migrated);
		Assert.Equal(1, registry.WorkerCount); // Source worker removed
	}
}
