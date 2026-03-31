using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Telemetry;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     A component-based version of <see cref="EventWaiter{T}" />
/// </summary>
internal class ComponentEventWaiter : IDisposable
{
	private readonly DiscordClient _client;
	private readonly ConcurrentHashSet<ComponentCollectRequest> _collectRequests = [];
	private readonly ConcurrentHashSet<ComponentMatchRequest> _matchRequests = [];

	/// <summary>
	///     Initializes a new instance of the <see cref="ComponentEventWaiter" /> class.
	/// </summary>
	/// <param name="client">The client.</param>
	public ComponentEventWaiter(DiscordClient client)
	{
		this._client = client;
		this._client.InternalComponentInteractionCreated.Register(this.Handle);
	}

	/// <summary>
	///     Disposes the waiter.
	/// </summary>
	public void Dispose()
	{
		this._matchRequests.Clear();
		this._collectRequests.Clear();
		this._client.InternalComponentInteractionCreated.Unregister(this.Handle);
	}

	/// <summary>
	///     Waits for a specified <see cref="ComponentMatchRequest" />'s predicate to be fulfilled.
	/// </summary>
	/// <param name="request">The request to wait for.</param>
	/// <returns>The returned args, or null if it timed out.</returns>
	public async Task<ComponentInteractionCreateEventArgs?> WaitForMatchAsync(ComponentMatchRequest request)
	{
		this._matchRequests.Add(request);

		try
		{
			return await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", e);
			this._client.Logger.LogError(InteractivityEvents.InteractivityWaitError, e, "An exception was thrown while waiting for components.");
			return null;
		}
		finally
		{
			this._matchRequests.TryRemove(request);
		}
	}

	/// <summary>
	///     Collects reactions and returns the result when the <see cref="ComponentMatchRequest" />'s cancellation token is
	///     canceled.
	/// </summary>
	/// <param name="request">The request to wait on.</param>
	/// <returns>The result from request's predicate over the period of time leading up to the token's cancellation.</returns>
	public async Task<IReadOnlyList<ComponentInteractionCreateEventArgs>> CollectMatchesAsync(ComponentCollectRequest request)
	{
		this._collectRequests.Add(request);
		try
		{
			await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", e);
			this._client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, e, "There was an error while collecting component event args.");
		}
		finally
		{
			this._collectRequests.TryRemove(request);
		}

		return [.. request.Collected];
	}

	/// <summary>
	///     Handles the waiter.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="args">The args.</param>
	private async Task Handle(DiscordClient client, ComponentInteractionCreateEventArgs args)
	{
		foreach (var mreq in this._matchRequests)
			if (mreq.Message == args.Message && mreq.IsMatch(args))
				mreq.Tcs.TrySetResult(args);

		foreach (var creq in this._collectRequests.Where(creq => creq.Message == args.Message && creq.IsMatch(args)))
		{
			await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
			creq.Collected.Add(args);
		}
	}
}
