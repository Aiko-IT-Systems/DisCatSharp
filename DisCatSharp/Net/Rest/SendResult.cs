using System;

namespace DisCatSharp.Net;

/// <summary>
///     Encapsulates the outcome of a single HTTP send attempt performed by the REST client.
///     Used internally by <see cref="BucketWorker" /> to decide whether to retry, complete, or fault a request.
/// </summary>
internal readonly struct SendResult
{
	/// <summary>
	///     Gets the parsed REST response.
	/// </summary>
	internal RestResponse Response { get; init; }

	/// <summary>
	///     Gets whether the request should be retried (e.g. 429).
	/// </summary>
	internal bool ShouldRetry { get; init; }

	/// <summary>
	///     Gets the delay to wait before retrying.
	/// </summary>
	internal TimeSpan RetryDelay { get; init; }

	/// <summary>
	///     Gets whether the rate limit that caused the retry is global.
	/// </summary>
	internal bool IsGlobalRateLimit { get; init; }

	/// <summary>
	///     Gets the exception mapped from the response status, if any.
	/// </summary>
	internal Exception? Error { get; init; }
}
