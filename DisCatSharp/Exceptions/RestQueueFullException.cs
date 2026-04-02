namespace DisCatSharp.Exceptions;

/// <summary>
///     Thrown when a REST request is rejected because the bucket queue has reached its
///     <see cref="RestAdvancedConfiguration.MaxQueueDepthPerBucket" /> limit.
/// </summary>
public sealed class RestQueueFullException : DisCatSharpException
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RestQueueFullException" /> class.
	/// </summary>
	/// <param name="route">The generic API route that was rejected.</param>
	/// <param name="bucketId">The rate-limit bucket identifier.</param>
	/// <param name="queueDepth">The current queue depth at rejection time.</param>
	/// <param name="maxDepth">The configured maximum queue depth.</param>
	internal RestQueueFullException(string route, string? bucketId, int queueDepth, int maxDepth)
		: base($"REST request rejected: bucket queue is full ({queueDepth}/{maxDepth}). Route: {route}, Bucket: {bucketId ?? "unknown"}")
	{
		this.Route = route;
		this.BucketId = bucketId;
		this.QueueDepth = queueDepth;
		this.MaxDepth = maxDepth;
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
	///     Gets the queue depth at the time of rejection.
	/// </summary>
	public int QueueDepth { get; }

	/// <summary>
	///     Gets the configured maximum queue depth.
	/// </summary>
	public int MaxDepth { get; }
}
