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

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.ApplicationAuthorized" />.
/// </summary>
public sealed class WebhookApplicationAuthorizedEventArgs : DiscordWebhookEventArgs
{
	internal WebhookApplicationAuthorizedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed authorization payload.
	/// </summary>
	public DiscordWebhookApplicationAuthorizedEventData Authorization { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.ApplicationDeauthorized" />.
/// </summary>
public sealed class WebhookApplicationDeauthorizedEventArgs : DiscordWebhookEventArgs
{
	internal WebhookApplicationDeauthorizedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed deauthorization payload.
	/// </summary>
	public DiscordWebhookApplicationDeauthorizedEventData Deauthorization { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.EntitlementCreated" />.
/// </summary>
public sealed class WebhookEntitlementCreateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookEntitlementCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the entitlement payload.
	/// </summary>
	public DiscordEntitlement Entitlement { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.EntitlementUpdated" />.
/// </summary>
public sealed class WebhookEntitlementUpdateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookEntitlementUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the entitlement payload.
	/// </summary>
	public DiscordEntitlement Entitlement { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.EntitlementDeleted" />.
/// </summary>
public sealed class WebhookEntitlementDeleteEventArgs : DiscordWebhookEventArgs
{
	internal WebhookEntitlementDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the entitlement payload.
	/// </summary>
	public DiscordEntitlement Entitlement { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.LobbyMessageCreated" />.
/// </summary>
public sealed class WebhookLobbyMessageCreateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookLobbyMessageCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed lobby message payload.
	/// </summary>
	public DiscordWebhookLobbyMessageEventData Message { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.LobbyMessageUpdated" />.
/// </summary>
public sealed class WebhookLobbyMessageUpdateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookLobbyMessageUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed lobby message payload.
	/// </summary>
	public DiscordWebhookLobbyMessageEventData Message { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.LobbyMessageDeleted" />.
/// </summary>
public sealed class WebhookLobbyMessageDeleteEventArgs : DiscordWebhookEventArgs
{
	internal WebhookLobbyMessageDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed lobby message delete payload.
	/// </summary>
	public DiscordWebhookLobbyMessageDeleteEventData Message { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.GameDirectMessageCreated" />.
/// </summary>
public sealed class WebhookGameDirectMessageCreateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookGameDirectMessageCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed game direct message payload.
	/// </summary>
	public DiscordWebhookGameDirectMessageEventData Message { get; internal set; } = null!;
}

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.GameDirectMessageUpdated" />.
/// </summary>
public sealed class WebhookGameDirectMessageUpdateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookGameDirectMessageUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed game direct message payload.
	/// </summary>
	public DiscordWebhookGameDirectMessageEventData Message { get; internal set; } = null!;
}

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

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.UnknownWebhookEventReceived" />.
/// </summary>
public sealed class UnknownWebhookEventArgs : DiscordWebhookEventArgs
{
	internal UnknownWebhookEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
