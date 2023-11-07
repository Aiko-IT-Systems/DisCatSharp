namespace DisCatSharp.Enums;

/// <summary>
/// Represents the type of a webhook.
/// </summary>
public enum WebhookType : int
{
	/// <summary>
	/// Incoming webhooks can post messages to channels with a generated token.
	/// </summary>
	Incoming = 1,

	/// <summary>
	/// Channel follower webhooks are internal webhooks used with channel following to post new messages into channels.
	/// </summary>
	ChannelFollower = 2,

	/// <summary>
	/// Application webhooks are webhooks used with interactions.
	/// </summary>
	Application = 3
}
