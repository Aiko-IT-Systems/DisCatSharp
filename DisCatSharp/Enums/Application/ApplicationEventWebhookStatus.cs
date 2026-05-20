namespace DisCatSharp.Enums;

/// <summary>
///     Represents the status of the application's event webhooks subscription.
/// </summary>
public enum ApplicationEventWebhookStatus
{
	/// <summary>
	///     Event webhooks are disabled.
	/// </summary>
	Disabled = 1,

	/// <summary>
	///     Event webhooks are enabled.
	/// </summary>
	Enabled = 2,

	/// <summary>
	///     Event webhooks were disabled by Discord.
	/// </summary>
	DisabledByDiscord = 3
}