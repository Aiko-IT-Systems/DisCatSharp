using System;
using System.Collections.Concurrent;
using System.Threading;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// Represents a component event that is being waited for.
/// </summary>
internal sealed class ComponentCollectRequest : ComponentMatchRequest
{
	/// <summary>
	/// Gets the collected.
	/// </summary>
	public ConcurrentBag<ComponentInteractionCreateEventArgs> Collected { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ComponentCollectRequest"/> class.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="predicate">The predicate.</param>
	/// <param name="cancellation">The cancellation token.</param>
	public ComponentCollectRequest(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken cancellation)
		: base(message, predicate, cancellation)
	{ }
}
