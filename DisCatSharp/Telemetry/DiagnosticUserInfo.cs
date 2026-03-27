namespace DisCatSharp.Telemetry;

/// <summary>
///     Represents optional user context for diagnostic reports.
/// </summary>
internal sealed class DiagnosticUserInfo
{
	/// <summary>
	///     Gets or sets the bot user ID.
	/// </summary>
	public required string Id { get; init; }

	/// <summary>
	///     Gets or sets the bot username.
	/// </summary>
	public required string Username { get; init; }

	/// <summary>
	///     Gets or sets the developer user ID.
	/// </summary>
	public string? DeveloperUserId { get; init; }

	/// <summary>
	///     Gets or sets the feedback email.
	/// </summary>
	public string? FeedbackEmail { get; init; }
}
