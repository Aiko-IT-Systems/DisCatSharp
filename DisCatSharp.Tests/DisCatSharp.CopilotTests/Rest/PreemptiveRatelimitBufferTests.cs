using System;
using System.Threading.Tasks;

using DisCatSharp.Net;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Tests for the preemptive ratelimit safety buffer feature.
///     Validates configuration defaults/validation, bucket reset timing with buffer,
///     and delay computation with buffer applied.
/// </summary>
public class PreemptiveRatelimitBufferTests
{
	// ── Configuration tests ──────────────────────────────────────────────────

	[Fact]
	public void Default_Is250Milliseconds()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Equal(TimeSpan.FromMilliseconds(250), config.PreemptiveRatelimitBuffer);
	}

	[Fact]
	public void Zero_DisablesBuffer()
	{
		var config = new RestAdvancedConfiguration { PreemptiveRatelimitBuffer = TimeSpan.Zero };
		Assert.Equal(TimeSpan.Zero, config.PreemptiveRatelimitBuffer);
	}

	[Fact]
	public void MaxAllowed_IsFiveSeconds()
	{
		var config = new RestAdvancedConfiguration { PreemptiveRatelimitBuffer = TimeSpan.FromSeconds(5) };
		Assert.Equal(TimeSpan.FromSeconds(5), config.PreemptiveRatelimitBuffer);
	}

	[Fact]
	public void Negative_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.PreemptiveRatelimitBuffer = TimeSpan.FromMilliseconds(-1));
	}

	[Fact]
	public void OverFiveSeconds_Throws()
	{
		var config = new RestAdvancedConfiguration();
		Assert.Throws<ArgumentOutOfRangeException>(() => config.PreemptiveRatelimitBuffer = TimeSpan.FromSeconds(6));
	}

	[Fact]
	public void Clone_CopiesBuffer()
	{
		var original = new RestAdvancedConfiguration { PreemptiveRatelimitBuffer = TimeSpan.FromMilliseconds(500) };
		var clone = new RestAdvancedConfiguration(original);

		Assert.Equal(TimeSpan.FromMilliseconds(500), clone.PreemptiveRatelimitBuffer);
	}

	[Fact]
	public void Clone_IsIndependent()
	{
		var original = new RestAdvancedConfiguration();
		var clone = new RestAdvancedConfiguration(original);

		clone.PreemptiveRatelimitBuffer = TimeSpan.FromMilliseconds(100);

		Assert.Equal(TimeSpan.FromMilliseconds(250), original.PreemptiveRatelimitBuffer);
	}

	// ── TryResetLimitAsync buffer tests ──────────────────────────────────────

	[Fact]
	public async Task TryReset_WithoutBuffer_OpensAtExactBoundary()
	{
		var bucket = new RateLimitBucket("test", "", "", "");
		var resetTime = DateTimeOffset.UtcNow.AddSeconds(-0.001);
		bucket.SetInitialValues(5, 0, resetTime);

		var result = await bucket.TryResetLimitAsync(DateTimeOffset.UtcNow, bufferTicks: 0);

		Assert.True(result);
		Assert.Equal(5, bucket.RemainingInternal);
	}

	[Fact]
	public async Task TryReset_WithBuffer_DoesNotOpenEarly()
	{
		var bucket = new RateLimitBucket("test", "", "", "");
		// Reset time is 10ms in the past
		var resetTime = DateTimeOffset.UtcNow.AddMilliseconds(-10);
		bucket.SetInitialValues(5, 0, resetTime);

		// Buffer is 250ms — the bucket should NOT reopen yet (10ms past < 250ms buffer)
		var bufferTicks = TimeSpan.FromMilliseconds(250).Ticks;
		var result = await bucket.TryResetLimitAsync(DateTimeOffset.UtcNow, bufferTicks);

		Assert.False(result);
		Assert.Equal(0, bucket.RemainingInternal);
	}

	[Fact]
	public async Task TryReset_WithBuffer_OpensAfterBufferElapsed()
	{
		var bucket = new RateLimitBucket("test", "", "", "");
		// Reset time is 500ms in the past — well beyond a 250ms buffer
		var resetTime = DateTimeOffset.UtcNow.AddMilliseconds(-500);
		bucket.SetInitialValues(5, 0, resetTime);

		var bufferTicks = TimeSpan.FromMilliseconds(250).Ticks;
		var result = await bucket.TryResetLimitAsync(DateTimeOffset.UtcNow, bufferTicks);

		Assert.True(result);
		Assert.Equal(5, bucket.RemainingInternal);
	}

	[Fact]
	public async Task TryReset_ZeroBuffer_BehavesLikeOriginal()
	{
		var bucket = new RateLimitBucket("test", "", "", "");
		var resetTime = DateTimeOffset.UtcNow.AddMilliseconds(-1);
		bucket.SetInitialValues(5, 0, resetTime);

		var result = await bucket.TryResetLimitAsync(DateTimeOffset.UtcNow, bufferTicks: 0);

		Assert.True(result);
		Assert.Equal(5, bucket.RemainingInternal);
	}

	[Fact]
	public async Task TryReset_NoNextReset_ReturnsFalse()
	{
		var bucket = new RateLimitBucket("test", "", "", "");
		// NextReset is 0 by default — bucket hasn't been probed yet

		var result = await bucket.TryResetLimitAsync(DateTimeOffset.UtcNow, bufferTicks: TimeSpan.FromMilliseconds(250).Ticks);

		Assert.False(result);
	}
}
