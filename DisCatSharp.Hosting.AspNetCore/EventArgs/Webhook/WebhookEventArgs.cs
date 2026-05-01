using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Hosting.AspNetCore.Ingress;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs;

/// <summary>
///     Common base for all Discord webhook events raised by <see cref="DiscordWebhookEventDispatcher" />.
/// </summary>
public abstract class DiscordWebhookEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordWebhookEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal DiscordWebhookEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the transport-neutral ingress request.
	/// </summary>
	public DiscordIngressRequest Request { get; internal set; } = null!;

	/// <summary>
	///     Gets the parsed Discord webhook event envelope.
	/// </summary>
	public DiscordWebhookEventEnvelope Envelope { get; internal set; } = null!;
}
