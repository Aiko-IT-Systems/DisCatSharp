using System;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     Represents a match that is being waited for.
/// </summary>
internal class ComponentMatchRequest
{
	protected readonly CancellationToken Cancellation;
	protected readonly Func<ComponentInteractionCreateEventArgs, bool> Predicate;

	/// <summary>
	///     Initializes a new instance of the <see cref="ComponentMatchRequest" /> class.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <param name="predicate">The predicate.</param>
	/// <param name="cancellation">The cancellation token.</param>
	public ComponentMatchRequest(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken cancellation)
	{
		this.Message = message;
		this.Predicate = predicate;
		this.Cancellation = cancellation;
		this._cancellationRegistration = this.Cancellation.Register(() => this.Tcs.TrySetResult(null));
	}

	private readonly CancellationTokenRegistration _cancellationRegistration;

	/// <summary>
	///     The id to wait on. This should be uniquely formatted to avoid collisions.
	/// </summary>
	public DiscordMessage Message { get; private set; }

	/// <summary>
	///     The completion source that represents the result of the match.
	/// </summary>
	public TaskCompletionSource<ComponentInteractionCreateEventArgs> Tcs { get; } = new();

	/// <summary>
	///     Whether it is a match.
	/// </summary>
	/// <param name="args">The arguments.</param>
	public bool IsMatch(ComponentInteractionCreateEventArgs args) => this.Predicate(args);
}
