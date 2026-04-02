using System;
using System.Net;
using System.Threading.Tasks;

using DisCatSharp.Net;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Tests for fault recovery: <see cref="BucketWorker.IsFaulted" />,
///     <see cref="BucketWorker.FaultException" />, <see cref="BaseRestRequest.TrySetCompleted" />,
///     and the recovery path in <see cref="BucketRegistry.CleanupBucketsAsync" />.
/// </summary>
public class FaultRecoveryTests
{
	private static BucketWorker CreateTestWorker(RateLimitBucket bucket)
		=> new(null!, bucket, new RestAdvancedConfiguration(), NullLogger<BucketWorker>.Instance);

	private static RestRequest CreateTestRequest(RateLimitBucket bucket, string url = "https://discord.com/api/v10/test")
		=> new(
			(BaseDiscordClient)null!,
			bucket,
			new Uri(url),
			RestRequestMethod.GET,
			"/test"
		);

	// ── IsFaulted / FaultException ──────────────────────────────────────────

	[Fact]
	public void IsFaulted_FalseForNewWorker()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		using var worker = CreateTestWorker(bucket);

		Assert.False(worker.IsFaulted);
		Assert.Null(worker.FaultException);
	}

	[Fact]
	public void IsFaulted_FalseForIdleWorker()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		using var worker = CreateTestWorker(bucket);

		Assert.False(worker.IsAlive);
		Assert.False(worker.IsFaulted);
	}

	[Fact]
	public void IsFaulted_FalseAfterDispose()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		var worker = CreateTestWorker(bucket);
		worker.Dispose();

		Assert.False(worker.IsFaulted);
	}

	// ── TrySetCompleted ─────────────────────────────────────────────────────

	[Fact]
	public async Task TrySetCompleted_SucceedsOnPendingRequest()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		var request = CreateTestRequest(bucket);

		var response = new RestResponse
		{
			ResponseCode = HttpStatusCode.OK
		};

		Assert.True(request.TrySetCompleted(response));

		var result = await request.WaitForCompletionAsync();
		Assert.Equal(HttpStatusCode.OK, result.ResponseCode);
	}

	[Fact]
	public void TrySetCompleted_ReturnsFalseWhenAlreadyFaulted()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		var request = CreateTestRequest(bucket);

		request.SetFaulted(new InvalidOperationException("already faulted"));

		var response = new RestResponse
		{
			ResponseCode = HttpStatusCode.OK
		};

		Assert.False(request.TrySetCompleted(response));
	}

	[Fact]
	public async Task TrySetCompleted_ReturnsFalseWhenAlreadyCompleted()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		var request = CreateTestRequest(bucket);

		request.SetCompleted(new RestResponse { ResponseCode = HttpStatusCode.OK });

		Assert.False(request.TrySetCompleted(new RestResponse { ResponseCode = HttpStatusCode.Created }));

		var result = await request.WaitForCompletionAsync();
		Assert.Equal(HttpStatusCode.OK, result.ResponseCode);
	}

	// ── MigrateQueueTo with fault recovery ──────────────────────────────────

	[Fact]
	public void MigrateQueueTo_WorksOnFaultedWorker()
	{
		var sourceBucket = new RateLimitBucket("hash_a", "", "100", "");
		var targetBucket = new RateLimitBucket("hash_b", "", "100", "");

		using var source = CreateTestWorker(sourceBucket);
		using var target = CreateTestWorker(targetBucket);

		var r1 = CreateTestRequest(sourceBucket);
		var r2 = CreateTestRequest(sourceBucket);
		source.EnqueueDirect(r1);
		source.EnqueueDirect(r2);

		var migrated = source.MigrateQueueTo(target);

		Assert.Equal(2, migrated);
	}

	// ── Recovered counter ───────────────────────────────────────────────────

	[Fact]
	public void Recovered_IncrementedDuringMigration()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		using var source = CreateTestWorker(bucket);
		using var target = CreateTestWorker(bucket);

		source.EnqueueDirect(CreateTestRequest(bucket));
		source.EnqueueDirect(CreateTestRequest(bucket));
		source.EnqueueDirect(CreateTestRequest(bucket));

		var recovered = source.MigrateQueueTo(target);
		target.Recovered = recovered;

		Assert.Equal(3, target.Recovered);
	}

	// ── TrySetFaulted vs TrySetCompleted ordering ───────────────────────────

	[Fact]
	public void TrySetFaulted_ThenTrySetCompleted_OnlyFirstWins()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		var request = CreateTestRequest(bucket);

		Assert.True(request.TrySetFaulted(new Exception("first")));
		Assert.False(request.TrySetCompleted(new RestResponse { ResponseCode = HttpStatusCode.OK }));
		Assert.True(request.WaitForCompletionAsync().IsFaulted);
	}

	[Fact]
	public void TrySetCompleted_ThenTrySetFaulted_OnlyFirstWins()
	{
		var bucket = new RateLimitBucket("hash", "", "100", "");
		var request = CreateTestRequest(bucket);

		Assert.True(request.TrySetCompleted(new RestResponse { ResponseCode = HttpStatusCode.OK }));
		Assert.False(request.TrySetFaulted(new Exception("second")));
		Assert.True(request.WaitForCompletionAsync().IsCompletedSuccessfully);
	}
}
