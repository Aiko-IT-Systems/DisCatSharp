using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments to <see cref="DiscordClient.WebhooksUpdated"/> event.
/// </summary>
public class WebhooksUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild that had its webhooks updated.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the channel to which the webhook belongs to.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="WebhooksUpdateEventArgs"/> class.
	/// </summary>
	internal WebhooksUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
