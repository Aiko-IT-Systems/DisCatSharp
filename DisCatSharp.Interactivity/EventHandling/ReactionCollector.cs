using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Telemetry;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     EventWaiter is a class that serves as a layer between the InteractivityExtension
///     and the DiscordClient to listen to an event and check for matches to a predicate.
/// </summary>
internal class ReactionCollector : IDisposable
{
	private DiscordClient _client;
	private bool _disposed;
	private ConcurrentHashSet<ReactionCollectRequest> _requests;

	/// <summary>
	///     Creates a new EventWaiter object.
	/// </summary>
	/// <param name="client">Your DiscordClient</param>
	public ReactionCollector(DiscordClient client)
	{
		this._client = client;
		this._requests = [];

		this._client.InternalMessageReactionAdded.Register(this.HandleReactionAdd);
		this._client.InternalMessageReactionRemoved.Register(this.HandleReactionRemove);
		this._client.InternalMessageReactionsCleared.Register(this.HandleReactionClear);
	}

	/// <summary>
	///     Disposes this EventWaiter.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;

		var client = this._client;
		if (client is not null)
		{
			client.InternalMessageReactionAdded.Unregister(this.HandleReactionAdd);
			client.InternalMessageReactionRemoved.Unregister(this.HandleReactionRemove);
			client.InternalMessageReactionsCleared.Unregister(this.HandleReactionClear);
		}

		this._client = null;
		this._requests?.Clear();
		this._requests = null;
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Collects the async.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <returns>A Task.</returns>
	public async Task<ReadOnlyCollection<Reaction>> CollectAsync(ReactionCollectRequest request)
	{
		this._requests.Add(request);
		var result = (ReadOnlyCollection<Reaction>)null;

		try
		{
			await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			this._client.DiagnosticsSink.CaptureException("DisCatSharp.Interactivity", ex);
			this._client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, ex, "Exception occurred while collecting reactions");
		}
		finally
		{
			result = new(new HashSet<Reaction>(request.Collected).ToList());
			request.Dispose();
			this._requests.TryRemove(request);
		}

		return result;
	}

	/// <summary>
	///     Handles the reaction add.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	/// <returns>A Task.</returns>
	private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventArgs)
	{
		// foreach request add
		foreach (var req in this._requests)
			if (req.Message.Id == eventArgs.Message.Id)
			{
				lock (req)
				{
					var reaction = req.Collected.FirstOrDefault(x => x.Emoji == eventArgs.Emoji);
					if (reaction is not null)
					{
						req.Collected.TryRemove(reaction);
						reaction.Users.Add(eventArgs.User);
						req.Collected.Add(reaction);
					}
					else
						req.Collected.Add(new()
						{
							Emoji = eventArgs.Emoji,
							Users = [eventArgs.User]
						});
				}
			}

		return Task.CompletedTask;
	}

	/// <summary>
	///     Handles the reaction remove.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	/// <returns>A Task.</returns>
	private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
	{
		// foreach request remove
		foreach (var req in this._requests)
			if (req.Message.Id == eventArgs.Message.Id)
			{
				lock (req)
				{
					var reaction = req.Collected.FirstOrDefault(x => x.Emoji == eventArgs.Emoji && x.Users.Any(y => y.Id == eventArgs.User.Id));
					if (reaction is not null)
					{
						req.Collected.TryRemove(reaction);
						reaction.Users.TryRemove(eventArgs.User);
						if (reaction.Users.Count > 0)
							req.Collected.Add(reaction);
					}
				}
			}

		return Task.CompletedTask;
	}

	/// <summary>
	///     Handles the reaction clear.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="eventArgs">The event's arguments.</param>
	/// <returns>A Task.</returns>
	private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventArgs)
	{
		// foreach request add
		foreach (var req in this._requests)
			if (req.Message.Id == eventArgs.Message.Id)
				req.Collected.Clear();
		return Task.CompletedTask;
	}

	~ReactionCollector()
	{
		this.Dispose();
	}
}

/// <summary>
///     The reaction collect request.
/// </summary>
public class ReactionCollectRequest : IDisposable
{
	internal ConcurrentHashSet<Reaction> Collected;
	internal CancellationTokenSource Ct;
	internal DiscordMessage Message;
	internal TaskCompletionSource<Reaction> Tcs;
	internal TimeSpan Timeout;

	/// <summary>
	///     Initializes a new instance of the <see cref="ReactionCollectRequest" /> class.
	/// </summary>
	/// <param name="msg">The msg.</param>
	/// <param name="timeout">The timeout.</param>
	public ReactionCollectRequest(DiscordMessage msg, TimeSpan timeout)
	{
		this.Message = msg;
		this.Collected = [];
		this.Timeout = timeout;
		this.Tcs = new();
		this.Ct = new(this.Timeout);
		this.Ct.Token.Register(() => this.Tcs.TrySetResult(null));
	}

	/// <summary>
	///     Disposes the.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;
		GC.SuppressFinalize(this);
		this.Ct.Dispose();
		this.Tcs = null;
		this.Message = null;
		this.Collected?.Clear();
		this.Collected = null;
	}

	private bool _disposed;

	~ReactionCollectRequest()
	{
		this.Dispose();
	}
}

/// <summary>
///     The reaction.
/// </summary>
public class Reaction
{
	/// <summary>
	///     Gets the emoji.
	/// </summary>
	public DiscordEmoji Emoji { get; internal set; }

	/// <summary>
	///     Gets the users.
	/// </summary>
	public ConcurrentHashSet<DiscordUser> Users { get; internal set; }

	/// <summary>
	///     Gets the total.
	/// </summary>
	public int Total => this.Users.Count;
}
