using System;
using System.Net;

using DisCatSharp.Net;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Tests for <see cref="SendResult" /> value semantics.
/// </summary>
public class SendResultTests
{
	[Fact]
	public void Default_IsNotRetryableAndNoError()
	{
		var result = new SendResult();

		Assert.False(result.ShouldRetry);
		Assert.False(result.IsGlobalRateLimit);
		Assert.Equal(TimeSpan.Zero, result.RetryDelay);
		Assert.Null(result.Error);
	}

	[Fact]
	public void RetryableResult_HasExpectedProperties()
	{
		var result = new SendResult
		{
			ShouldRetry = true,
			RetryDelay = TimeSpan.FromSeconds(2.5),
			IsGlobalRateLimit = false
		};

		Assert.True(result.ShouldRetry);
		Assert.Equal(TimeSpan.FromSeconds(2.5), result.RetryDelay);
		Assert.False(result.IsGlobalRateLimit);
	}

	[Fact]
	public void GlobalRateLimit_SetsFlag()
	{
		var result = new SendResult
		{
			ShouldRetry = true,
			IsGlobalRateLimit = true,
			RetryDelay = TimeSpan.FromSeconds(60)
		};

		Assert.True(result.IsGlobalRateLimit);
		Assert.True(result.ShouldRetry);
		Assert.Equal(TimeSpan.FromSeconds(60), result.RetryDelay);
	}

	[Fact]
	public void ErrorResult_CarriesException()
	{
		var ex = new InvalidOperationException("test error");
		var result = new SendResult
		{
			Error = ex,
			Response = new RestResponse
			{
				ResponseCode = HttpStatusCode.BadRequest
			}
		};

		Assert.Same(ex, result.Error);
		Assert.Equal(HttpStatusCode.BadRequest, result.Response.ResponseCode);
	}
}
