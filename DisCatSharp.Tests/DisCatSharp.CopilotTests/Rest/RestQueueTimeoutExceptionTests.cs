using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Exceptions;
using DisCatSharp.Net;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Tests for <see cref="RestQueueTimeoutException" /> diagnostic properties.
/// </summary>
public class RestQueueTimeoutExceptionTests
{
	[Fact]
	public void Ctor_SetsAllDiagnosticProperties()
	{
		var ex = new RestQueueTimeoutException(
			"POST:/channels/channel_id/messages",
			"abc123",
			TimeSpan.FromSeconds(301.5),
			12,
			true
		);

		Assert.Equal("POST:/channels/channel_id/messages", ex.Route);
		Assert.Equal("abc123", ex.BucketId);
		Assert.Equal(TimeSpan.FromSeconds(301.5), ex.WaitedDuration);
		Assert.Equal(12, ex.QueueLength);
		Assert.True(ex.GlobalGateActive);
		Assert.Contains("POST:/channels/channel_id/messages", ex.Message);
		Assert.Contains("abc123", ex.Message);
		Assert.Contains("301", ex.Message); // Locale-safe substring of the duration
	}

	[Fact]
	public void Ctor_NullBucketId_ShowsUnknown()
	{
		var ex = new RestQueueTimeoutException("GET:/test", null, TimeSpan.FromMinutes(5), 0, false);

		Assert.Null(ex.BucketId);
		Assert.Contains("unknown", ex.Message);
		Assert.False(ex.GlobalGateActive);
	}

	[Fact]
	public void IsDisCatSharpException()
	{
		var ex = new RestQueueTimeoutException("GET:/test", "b1", TimeSpan.FromSeconds(1), 0, false);
		Assert.IsType<DisCatSharpException>(ex, exactMatch: false);
	}
}
