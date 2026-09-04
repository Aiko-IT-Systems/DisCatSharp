using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the privileged intents review status for a Discord application.
/// </summary>
public sealed class DiscordPrivilegedIntentsReview
{
	/// <summary>
	///     Gets the timestamp when the limited intents threshold was exceeded.
	/// </summary>
	[JsonProperty("limited_intents_threshold_exceeded_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? LimitedIntentsThresholdExceededAt { get; internal set; }

	/// <summary>
	///     Gets the deadline for limited intents revocation.
	/// </summary>
	[JsonProperty("limited_intents_revocation_deadline", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? LimitedIntentsRevocationDeadline { get; internal set; }

	/// <summary>
	///     Gets whether the application has submitted a privileged intents review.
	/// </summary>
	[JsonProperty("has_submitted", NullValueHandling = NullValueHandling.Ignore)]
	public bool HasSubmitted { get; internal set; }
}
