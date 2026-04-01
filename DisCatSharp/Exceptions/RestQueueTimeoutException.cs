using System;

namespace DisCatSharp.Exceptions;

/// <summary>
///     Thrown when a REST request times out while waiting in a bucket's queue.
///     Carries rich debugging context to help diagnose pressure and hang scenarios.
/// </summary>
public sealed class RestQueueTimeoutException : DisCatSharpException
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RestQueueTimeoutException" /> class.
	/// </summary>
	/// <param name="route">The generic API route that timed out.</param>
	/// <param name="bucketId">The rate-limit bucket identifier.</param>
	/// <param name="waitedDuration">How long the request waited before timing out.</param>
	/// <param name="queueLength">The number of requests still queued in the bucket at timeout.</param>
	/// <param name="globalGateActive">Whether the global rate-limit gate was blocking at the time of timeout.</param>
	internal RestQueueTimeoutException(string route, string? bucketId, TimeSpan waitedDuration, int queueLength, bool globalGateActive)
		: base(FormatMessage(route, bucketId, waitedDuration, queueLength, globalGateActive))
	{
		this.Route = route;
		this.BucketId = bucketId;
		this.WaitedDuration = waitedDuration;
		this.QueueLength = queueLength;
		this.GlobalGateActive = globalGateActive;
	}

	/// <summary>
	///     Gets the generic API route (e.g. <c>POST:/channels/channel_id/messages</c>).
	/// </summary>
	public string Route { get; }

	/// <summary>
	///     Gets the rate-limit bucket identifier.
	/// </summary>
	public string? BucketId { get; }

	/// <summary>
	///     Gets how long the request waited before the timeout fired.
	/// </summary>
	public TimeSpan WaitedDuration { get; }

	/// <summary>
	///     Gets the number of requests still queued in the bucket when the timeout occurred.
	/// </summary>
	public int QueueLength { get; }

	/// <summary>
	///     Gets whether the global rate-limit gate was active (blocking) at the time of timeout.
	/// </summary>
	public bool GlobalGateActive { get; }

	private static string FormatMessage(string route, string? bucketId, TimeSpan waitedDuration, int queueLength, bool globalGateActive)
		=> $"REST request timed out after waiting {waitedDuration.TotalSeconds:F1}s in queue. " +
		   $"Route: {route}, Bucket: {bucketId ?? "unknown"}, " +
		   $"Queue depth: {queueLength}, Global gate active: {globalGateActive}";
}
