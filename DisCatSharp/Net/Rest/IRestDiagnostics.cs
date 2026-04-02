using System.Collections.Generic;

namespace DisCatSharp.Net;

/// <summary>
///     Provides a read-only diagnostic view of the REST subsystem's runtime state.
///     Exposed via <see cref="BaseDiscordClient.RestDiagnostics" />.
/// </summary>
public interface IRestDiagnostics
{
	/// <summary>
	///     Gets the total number of active bucket workers.
	/// </summary>
	int ActiveWorkerCount { get; }

	/// <summary>
	///     Gets the total number of requests queued across all bucket workers.
	/// </summary>
	int TotalQueuedRequests { get; }

	/// <summary>
	///     Gets a snapshot of per-bucket diagnostic information.
	/// </summary>
	IReadOnlyList<BucketDiagnostics> GetBucketSnapshots();
}

/// <summary>
///     Safe wrapper that delegates to an <see cref="IRestDiagnostics" /> implementation
///     without exposing the underlying object (prevents casting to <see cref="RestClient" />).
/// </summary>
internal sealed class RestDiagnosticsWrapper(IRestDiagnostics inner) : IRestDiagnostics
{
	public int ActiveWorkerCount => inner.ActiveWorkerCount;

	public int TotalQueuedRequests => inner.TotalQueuedRequests;

	public IReadOnlyList<BucketDiagnostics> GetBucketSnapshots()
		=> inner.GetBucketSnapshots();
}

/// <summary>
///     Diagnostic snapshot of a single bucket worker's state.
/// </summary>
/// <param name="BucketId">The bucket identifier string.</param>
/// <param name="QueueLength">Current number of queued requests.</param>
/// <param name="Processed">Total requests processed (completed + faulted).</param>
/// <param name="Retried">Total retry attempts.</param>
/// <param name="TimedOut">Total requests that timed out in queue.</param>
/// <param name="Cancelled">Total requests cancelled before execution.</param>
/// <param name="ConsecutiveFailures">Current consecutive failure count (circuit breaker).</param>
/// <param name="IsAlive">Whether the worker loop task is still running.</param>
/// <param name="IsFaulted">Whether the worker loop task has faulted.</param>
public readonly record struct BucketDiagnostics(
	string BucketId,
	int QueueLength,
	long Processed,
	long Retried,
	long TimedOut,
	long Cancelled,
	int ConsecutiveFailures,
	bool IsAlive,
	bool IsFaulted);
