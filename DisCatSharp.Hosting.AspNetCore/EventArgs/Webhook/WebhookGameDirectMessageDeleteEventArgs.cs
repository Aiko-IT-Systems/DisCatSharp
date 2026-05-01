using System;

using DisCatSharp.Hosting.AspNetCore.Ingress;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.GameDirectMessageDeleted" />.
/// </summary>
public sealed class WebhookGameDirectMessageDeleteEventArgs : DiscordWebhookEventArgs
{
	internal WebhookGameDirectMessageDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed game direct message payload.
	/// </summary>
	public DiscordWebhookGameDirectMessageEventData Message { get; internal set; } = null!;
}
