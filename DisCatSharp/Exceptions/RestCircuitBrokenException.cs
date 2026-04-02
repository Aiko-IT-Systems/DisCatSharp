using System;

namespace DisCatSharp.Exceptions;

/// <summary>
///     Thrown when a REST request is rejected because the bucket's circuit breaker is open
///     due to consecutive failures exceeding <see cref="RestAdvancedConfiguration.CircuitBreakerThreshold" />.
/// </summary>
public sealed class RestCircuitBrokenException : DisCatSharpException
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RestCircuitBrokenException" /> class.
	/// </summary>
	/// <param name="route">The generic API route that was rejected.</param>
	/// <param name="bucketId">The rate-limit bucket identifier.</param>
	/// <param name="consecutiveFailures">The number of consecutive failures that tripped the circuit.</param>
	/// <param name="openSince">When the circuit was opened.</param>
	internal RestCircuitBrokenException(string route, string? bucketId, int consecutiveFailures, DateTimeOffset openSince)
		: base($"REST request rejected: circuit breaker open for bucket after {consecutiveFailures} consecutive failures. Route: {route}, Bucket: {bucketId ?? "unknown"}, Open since: {openSince:u}")
	{
		this.Route = route;
		this.BucketId = bucketId;
		this.ConsecutiveFailures = consecutiveFailures;
		this.OpenSince = openSince;
	}

	/// <summary>
	///     Gets the generic API route.
	/// </summary>
	public string Route { get; }

	/// <summary>
	///     Gets the rate-limit bucket identifier.
	/// </summary>
	public string? BucketId { get; }

	/// <summary>
	///     Gets the number of consecutive failures that tripped the circuit.
	/// </summary>
	public int ConsecutiveFailures { get; }

	/// <summary>
	///     Gets when the circuit breaker was opened.
	/// </summary>
	public DateTimeOffset OpenSince { get; }
}
