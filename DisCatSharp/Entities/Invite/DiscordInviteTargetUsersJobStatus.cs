using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the processing status for invite target users.
/// </summary>
public sealed class DiscordInviteTargetUsersJobStatus : ObservableApiObject
{
	/// <summary>
	///     Gets the status of the job.
	/// </summary>
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public InviteTargetUsersJobStatus Status { get; internal set; }

	/// <summary>
	///     Gets the total number of users in the job.
	/// </summary>
	[JsonProperty("total_users", NullValueHandling = NullValueHandling.Ignore)]
	public int TotalUsers { get; internal set; }

	/// <summary>
	///     Gets the processed users count.
	/// </summary>
	[JsonProperty("processed_users", NullValueHandling = NullValueHandling.Ignore)]
	public int ProcessedUsers { get; internal set; }

	/// <summary>
	///     Gets when the job was created.
	/// </summary>
	[JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset CreatedAt { get; internal set; }

	/// <summary>
	///     Gets when the job completed, if applicable.
	/// </summary>
	[JsonProperty("completed_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? CompletedAt { get; internal set; }

	/// <summary>
	///     Gets the error message if the job failed.
	/// </summary>
	[JsonProperty("error_message", NullValueHandling = NullValueHandling.Ignore)]
	public string? ErrorMessage { get; internal set; }
}
