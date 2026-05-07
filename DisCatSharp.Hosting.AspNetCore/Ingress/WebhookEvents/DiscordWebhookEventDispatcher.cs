using System;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;
using DisCatSharp.Hosting.AspNetCore.Ingress;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Dispatches typed Discord webhook events raised by the ASP.NET Core ingress pipeline.
/// </summary>
/// <remarks>
///     <para>
///         The dispatcher stays alongside the webhook envelope, registry, and typed DTOs so the signed webhook feature surface can be
///         discovered from one feature area.
///     </para>
///     <para>
///         Event dispatch is intentionally fire-and-forget. The HTTP webhook endpoint acknowledges the signed request before async event
///         handlers complete, and each registered handler is subject to a one-second timeout enforced by the underlying
///         <see cref="AsyncEvent{TSender,TArgs}" /> infrastructure.
///     </para>
/// </remarks>
public sealed class DiscordWebhookEventDispatcher
{
	private static readonly TimeSpan s_eventExecutionLimit = TimeSpan.FromSeconds(1);

	private readonly IServiceProvider _provider;
	private readonly ILogger<DiscordWebhookEventDispatcher> _logger;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookApplicationAuthorizedEventArgs> _applicationAuthorized;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookApplicationDeauthorizedEventArgs> _applicationDeauthorized;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookEntitlementCreateEventArgs> _entitlementCreated;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookEntitlementUpdateEventArgs> _entitlementUpdated;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookEntitlementDeleteEventArgs> _entitlementDeleted;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookLobbyMessageCreateEventArgs> _lobbyMessageCreated;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookLobbyMessageUpdateEventArgs> _lobbyMessageUpdated;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookLobbyMessageDeleteEventArgs> _lobbyMessageDeleted;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookGameDirectMessageCreateEventArgs> _gameDirectMessageCreated;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookGameDirectMessageUpdateEventArgs> _gameDirectMessageUpdated;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, WebhookGameDirectMessageDeleteEventArgs> _gameDirectMessageDeleted;
	private readonly AsyncEvent<DiscordWebhookEventDispatcher, UnknownWebhookEventArgs> _unknownWebhookEventReceived;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordWebhookEventDispatcher" /> class.
	/// </summary>
	/// <param name="provider">The root service provider.</param>
	/// <param name="logger">The logger.</param>
	public DiscordWebhookEventDispatcher(IServiceProvider provider, ILogger<DiscordWebhookEventDispatcher> logger)
	{
		this._provider = provider ?? throw new ArgumentNullException(nameof(provider));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

		this._applicationAuthorized = new("WEBHOOK_APPLICATION_AUTHORIZED", s_eventExecutionLimit, this.EventErrorHandler);
		this._applicationDeauthorized = new("WEBHOOK_APPLICATION_DEAUTHORIZED", s_eventExecutionLimit, this.EventErrorHandler);
		this._entitlementCreated = new("WEBHOOK_ENTITLEMENT_CREATED", s_eventExecutionLimit, this.EventErrorHandler);
		this._entitlementUpdated = new("WEBHOOK_ENTITLEMENT_UPDATED", s_eventExecutionLimit, this.EventErrorHandler);
		this._entitlementDeleted = new("WEBHOOK_ENTITLEMENT_DELETED", s_eventExecutionLimit, this.EventErrorHandler);
		this._lobbyMessageCreated = new("WEBHOOK_LOBBY_MESSAGE_CREATED", s_eventExecutionLimit, this.EventErrorHandler);
		this._lobbyMessageUpdated = new("WEBHOOK_LOBBY_MESSAGE_UPDATED", s_eventExecutionLimit, this.EventErrorHandler);
		this._lobbyMessageDeleted = new("WEBHOOK_LOBBY_MESSAGE_DELETED", s_eventExecutionLimit, this.EventErrorHandler);
		this._gameDirectMessageCreated = new("WEBHOOK_GAME_DIRECT_MESSAGE_CREATED", s_eventExecutionLimit, this.EventErrorHandler);
		this._gameDirectMessageUpdated = new("WEBHOOK_GAME_DIRECT_MESSAGE_UPDATED", s_eventExecutionLimit, this.EventErrorHandler);
		this._gameDirectMessageDeleted = new("WEBHOOK_GAME_DIRECT_MESSAGE_DELETED", s_eventExecutionLimit, this.EventErrorHandler);
		this._unknownWebhookEventReceived = new("WEBHOOK_UNKNOWN_EVENT_RECEIVED", s_eventExecutionLimit, this.EventErrorHandler);
	}

	/// <summary>
	///     Fired when Discord sends an <c>APPLICATION_AUTHORIZED</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookApplicationAuthorizedEventArgs> ApplicationAuthorized
	{
		add => this._applicationAuthorized.Register(value);
		remove => this._applicationAuthorized.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends an <c>APPLICATION_DEAUTHORIZED</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookApplicationDeauthorizedEventArgs> ApplicationDeauthorized
	{
		add => this._applicationDeauthorized.Register(value);
		remove => this._applicationDeauthorized.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends an <c>ENTITLEMENT_CREATE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookEntitlementCreateEventArgs> EntitlementCreated
	{
		add => this._entitlementCreated.Register(value);
		remove => this._entitlementCreated.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends an <c>ENTITLEMENT_UPDATE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookEntitlementUpdateEventArgs> EntitlementUpdated
	{
		add => this._entitlementUpdated.Register(value);
		remove => this._entitlementUpdated.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends an <c>ENTITLEMENT_DELETE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookEntitlementDeleteEventArgs> EntitlementDeleted
	{
		add => this._entitlementDeleted.Register(value);
		remove => this._entitlementDeleted.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends a <c>LOBBY_MESSAGE_CREATE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookLobbyMessageCreateEventArgs> LobbyMessageCreated
	{
		add => this._lobbyMessageCreated.Register(value);
		remove => this._lobbyMessageCreated.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends a <c>LOBBY_MESSAGE_UPDATE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookLobbyMessageUpdateEventArgs> LobbyMessageUpdated
	{
		add => this._lobbyMessageUpdated.Register(value);
		remove => this._lobbyMessageUpdated.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends a <c>LOBBY_MESSAGE_DELETE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookLobbyMessageDeleteEventArgs> LobbyMessageDeleted
	{
		add => this._lobbyMessageDeleted.Register(value);
		remove => this._lobbyMessageDeleted.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends a <c>GAME_DIRECT_MESSAGE_CREATE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookGameDirectMessageCreateEventArgs> GameDirectMessageCreated
	{
		add => this._gameDirectMessageCreated.Register(value);
		remove => this._gameDirectMessageCreated.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends a <c>GAME_DIRECT_MESSAGE_UPDATE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookGameDirectMessageUpdateEventArgs> GameDirectMessageUpdated
	{
		add => this._gameDirectMessageUpdated.Register(value);
		remove => this._gameDirectMessageUpdated.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord sends a <c>GAME_DIRECT_MESSAGE_DELETE</c> webhook event.
	/// </summary>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, WebhookGameDirectMessageDeleteEventArgs> GameDirectMessageDeleted
	{
		add => this._gameDirectMessageDeleted.Register(value);
		remove => this._gameDirectMessageDeleted.Unregister(value);
	}

	/// <summary>
	///     Fired when Discord delivers a signed webhook event that does not yet have a typed event surface.
	/// </summary>
	/// <remarks>
	///     Subscribe to this event to handle newer webhook event types before the package ships a typed payload model for them.
	/// </remarks>
	public event AsyncEventHandler<DiscordWebhookEventDispatcher, UnknownWebhookEventArgs> UnknownWebhookEventReceived
	{
		add => this._unknownWebhookEventReceived.Register(value);
		remove => this._unknownWebhookEventReceived.Unregister(value);
	}

	/// <summary>
	///     Gets a value indicating whether any webhook event handlers are currently registered.
	/// </summary>
	public bool HasHandlers
		=> this._applicationAuthorized.HasHandlers
		   || this._applicationDeauthorized.HasHandlers
		   || this._entitlementCreated.HasHandlers
		   || this._entitlementUpdated.HasHandlers
		   || this._entitlementDeleted.HasHandlers
		   || this._lobbyMessageCreated.HasHandlers
		   || this._lobbyMessageUpdated.HasHandlers
		   || this._lobbyMessageDeleted.HasHandlers
		   || this._gameDirectMessageCreated.HasHandlers
		   || this._gameDirectMessageUpdated.HasHandlers
		   || this._gameDirectMessageDeleted.HasHandlers
		   || this._unknownWebhookEventReceived.HasHandlers;

	internal void EnqueueDispatch(DiscordIngressRequest request, DiscordWebhookEventEnvelope envelope)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(envelope);

		if (!envelope.IsEvent || !this.HasHandlers)
			return;

		_ = Task.Run(async () =>
		{
			try
			{
				await this.DispatchAsync(request, envelope).ConfigureAwait(false);
			}
			catch (Exception exception) when (exception is not OperationCanceledException)
			{
				this._logger.LogError(LoggerEvents.EventHandlerException, exception, "Unhandled exception while dispatching webhook event {EventType}", envelope.EventType);
			}
		});
	}

	internal async Task DispatchAsync(DiscordIngressRequest request, DiscordWebhookEventEnvelope envelope)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(envelope);

		if (!envelope.IsEvent)
			return;

		switch (envelope.EventType)
		{
			case DiscordWebhookEventNames.ApplicationAuthorized:
				await this.DispatchTypedAsync<DiscordWebhookApplicationAuthorizedEventData, WebhookApplicationAuthorizedEventArgs>(
					this._applicationAuthorized,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Authorization = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.ApplicationDeauthorized:
				await this.DispatchTypedAsync<DiscordWebhookApplicationDeauthorizedEventData, WebhookApplicationDeauthorizedEventArgs>(
					this._applicationDeauthorized,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Deauthorization = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.EntitlementCreate:
				await this.DispatchTypedAsync<DiscordEntitlement, WebhookEntitlementCreateEventArgs>(
					this._entitlementCreated,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Entitlement = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.EntitlementUpdate:
				await this.DispatchTypedAsync<DiscordEntitlement, WebhookEntitlementUpdateEventArgs>(
					this._entitlementUpdated,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Entitlement = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.EntitlementDelete:
				await this.DispatchTypedAsync<DiscordEntitlement, WebhookEntitlementDeleteEventArgs>(
					this._entitlementDeleted,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Entitlement = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.LobbyMessageCreate:
				await this.DispatchTypedAsync<DiscordWebhookLobbyMessageEventData, WebhookLobbyMessageCreateEventArgs>(
					this._lobbyMessageCreated,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Message = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.LobbyMessageUpdate:
				await this.DispatchTypedAsync<DiscordWebhookLobbyMessageEventData, WebhookLobbyMessageUpdateEventArgs>(
					this._lobbyMessageUpdated,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Message = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.LobbyMessageDelete:
				await this.DispatchTypedAsync<DiscordWebhookLobbyMessageDeleteEventData, WebhookLobbyMessageDeleteEventArgs>(
					this._lobbyMessageDeleted,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Message = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.GameDirectMessageCreate:
				await this.DispatchTypedAsync<DiscordWebhookGameDirectMessageEventData, WebhookGameDirectMessageCreateEventArgs>(
					this._gameDirectMessageCreated,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Message = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.GameDirectMessageUpdate:
				await this.DispatchTypedAsync<DiscordWebhookGameDirectMessageEventData, WebhookGameDirectMessageUpdateEventArgs>(
					this._gameDirectMessageUpdated,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Message = data
					}).ConfigureAwait(false);
				return;

			case DiscordWebhookEventNames.GameDirectMessageDelete:
				await this.DispatchTypedAsync<DiscordWebhookGameDirectMessageEventData, WebhookGameDirectMessageDeleteEventArgs>(
					this._gameDirectMessageDeleted,
					request,
					envelope,
					static (provider, currentRequest, currentEnvelope, data) => new(provider)
					{
						Request = currentRequest,
						Envelope = currentEnvelope,
						Message = data
					}).ConfigureAwait(false);
				return;

			default:
				await this.DispatchUnknownAsync(request, envelope).ConfigureAwait(false);
				return;
		}
	}

	private async Task DispatchTypedAsync<TPayload, TEventArgs>(
		AsyncEvent<DiscordWebhookEventDispatcher, TEventArgs> asyncEvent,
		DiscordIngressRequest request,
		DiscordWebhookEventEnvelope envelope,
		Func<IServiceProvider, DiscordIngressRequest, DiscordWebhookEventEnvelope, TPayload, TEventArgs> eventArgsFactory
	)
		where TPayload : ObservableApiObject
		where TEventArgs : DiscordWebhookEventArgs
	{
		if (!asyncEvent.HasHandlers)
			return;

		if (!envelope.TryDeserializeData<TPayload>(out var data))
		{
			await this.DispatchUnknownAsync(request, envelope).ConfigureAwait(false);
			return;
		}

		await asyncEvent.InvokeAsync(this, eventArgsFactory(this._provider, request, envelope, data)).ConfigureAwait(false);
	}

	private Task DispatchUnknownAsync(DiscordIngressRequest request, DiscordWebhookEventEnvelope envelope)
	{
		return !this._unknownWebhookEventReceived.HasHandlers
			? Task.CompletedTask
			: this._unknownWebhookEventReceived.InvokeAsync(this, new UnknownWebhookEventArgs(this._provider)
		{
			Request = request,
			Envelope = envelope
		});
	}

	private void EventErrorHandler<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
		where TArgs : AsyncEventArgs
	{
		if (ex is AsyncEventTimeoutException)
		{
			this._logger.LogWarning(LoggerEvents.EventHandlerException, "An event handler for {AsyncEventName} took too long to execute. Defined as \"{TrimStart}\" located in \"{MethodDeclaringType}\"", asyncEvent.Name, handler.Method.ToString()?.Replace(handler.Method.ReturnType.ToString(), "").TrimStart(), handler.Method.DeclaringType);
			return;
		}

		this._logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {Name} thrown from {Method} (defined in {DeclaringType})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
	}
}
