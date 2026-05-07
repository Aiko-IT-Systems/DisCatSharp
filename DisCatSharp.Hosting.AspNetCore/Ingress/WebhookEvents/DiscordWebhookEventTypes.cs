namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Contains known Discord webhook event envelope type constants.
/// </summary>
public static class DiscordWebhookEventTypes
{
	/// <summary>
	///     The Discord webhook ping envelope type.
	/// </summary>
	public const int Ping = 0;

	/// <summary>
	///     The Discord webhook event envelope type.
	/// </summary>
	public const int Event = 1;
}
