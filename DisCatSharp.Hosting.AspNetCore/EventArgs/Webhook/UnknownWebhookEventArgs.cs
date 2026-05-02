using System;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.UnknownWebhookEventReceived" />.
/// </summary>
public sealed class UnknownWebhookEventArgs : DiscordWebhookEventArgs
{
	internal UnknownWebhookEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
