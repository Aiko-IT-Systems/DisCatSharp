using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Telemetry;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     EventWaiter is a class that serves as a layer between the InteractivityExtension
///     and the DiscordClient to listen to an event and check for matches to a predicate.
/// </summary>
/// <typeparam name="T">The event args type to listen for.</typeparam>
internal class EventWaiter<T> : IDisposable where T : AsyncEventArgs
{
	private DiscordClient _client;
	private ConcurrentHashSet<CollectRequest<T>> _collectRequests;
	private bool _disposed;
	private readonly Action<AsyncEventHandler<DiscordClient, T>> _unsubscribe;
	private AsyncEventHandler<DiscordClient, T> _handler;
	private ConcurrentHashSet<MatchRequest<T>> _matchRequests;

	/// <summary>
	///     Creates a new EventWaiter object.
	/// </summary>
	/// <param name="client">Your DiscordClient.</param>
	/// <param name="subscribe">Action to subscribe the handler to the client event.</param>
	/// <param name="unsubscribe">Action to unsubscribe the handler from the client event.</param>
	public EventWaiter(
		DiscordClient client,
		Action<AsyncEventHandler<DiscordClient, T>> subscribe,
		Action<AsyncEventHandler<DiscordClient, T>> unsubscribe
	)
	{
		this._client = client;
		this._unsubscribe = unsubscribe;
		this._matchRequests = [];
		this._collectRequests = [];
		this._handler = this.HandleEvent;
		subscribe(this._handler);
	}

	/// <summary>
	///     Creates a new EventWaiter object using reflection to find the event.
	///     Use this only when the event type is not known at compile time.
	/// </summary>
	/// <param name="client">Your DiscordClient.</param>
	public EventWaiter(DiscordClient client)
	{
		this._client = client;
		var tinfo = this._client.GetType().GetTypeInfo();
		var eventField = tinfo.DeclaredFields.FirstOrDefault(x => x.FieldType == typeof(AsyncEvent<DiscordClient, T>))
			?? throw new InvalidOperationException($"No event field of type AsyncEvent<DiscordClient, {typeof(T).Name}> found on DiscordClient.");

		var asyncEvent = (AsyncEvent<DiscordClient, T>)eventField.GetValue(this._client);
		this._unsubscribe = h => asyncEvent.Unregister(h);
		this._matchRequests = [];
		this._collectRequests = [];
		this._handler = this.HandleEvent;
		asyncEvent.Register(this._handler);
	}

	/// <summary>
	///     Disposes this EventWaiter.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;

		if (this._handler is not null)
			this._unsubscribe?.Invoke(this._handler);

		this._handler = null;
		this._client = null;

		this._matchRequests?.Clear();
		this._collectRequests?.Clear();

		this._matchRequests = null;
		this._collectRequests = null;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Waits for a match to a specific request, else returns null.
	/// </summary>
	/// <param name="request">Request to match</param>
	/// <returns></returns>
	public async Task<T?> WaitForMatchAsync(MatchRequest<T> request)
	{
		T? result = null;
		this._matchRequests.Add(request);
		try
		{
			result = await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", ex);
			this._client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while waiting for {0}", typeof(T).Name);
		}
		finally
		{
			request.Dispose();
			this._matchRequests.TryRemove(request);
		}

		return result;
	}

	/// <summary>
	///     Collects the matches async.
	/// </summary>
	/// <param name="request">The request.</param>
	public async Task<ReadOnlyCollection<T>> CollectMatchesAsync(CollectRequest<T> request)
	{
		ReadOnlyCollection<T> result = null;
		this._collectRequests.Add(request);
		try
		{
			await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", ex);
			this._client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while collecting from {0}", typeof(T).Name);
		}
		finally
		{
			result = new(new HashSet<T>(request.Collected).ToList());
			request.Dispose();
			this._collectRequests.TryRemove(request);
		}

		return result;
	}

	/// <summary>
	///     Handles the event.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	private Task HandleEvent(DiscordClient client, T eventArgs)
	{
		if (this._disposed)
			return Task.CompletedTask;

		foreach (var req in this._matchRequests)
			if (req.Predicate(eventArgs))
				req.Tcs.TrySetResult(eventArgs);

		foreach (var req in this._collectRequests)
			if (req.Predicate(eventArgs))
				req.Collected.Add(eventArgs);

		return Task.CompletedTask;
	}

	~EventWaiter()
	{
		this.Dispose();
	}
}
