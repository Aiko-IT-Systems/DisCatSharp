using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Default in-memory implementation for pending ingress state.
/// </summary>
public sealed class InMemoryDiscordIngressPendingStateStore : IDiscordIngressPendingStateStore
{
	private static readonly IReadOnlyDictionary<string, string?> EmptyProperties =
		new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>());

	private readonly DiscordWebIngressOptions _options;
	private readonly ConcurrentDictionary<string, DiscordIngressPendingState> _states = new(StringComparer.Ordinal);
	private readonly TimeProvider _timeProvider;
	private long _nextCleanupUtcTicks;

	/// <summary>
	///     Initializes a new instance of the <see cref="InMemoryDiscordIngressPendingStateStore" /> class.
	/// </summary>
	/// <param name="options">The ingress options.</param>
	/// <param name="timeProvider">The time provider used for expiration checks.</param>
	public InMemoryDiscordIngressPendingStateStore(IOptions<DiscordWebIngressOptions> options, TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(options);

		this._options = options.Value;
		this._timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
	}

	/// <inheritdoc />
	public ValueTask StoreAsync(DiscordIngressPendingState state, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(state);
		ArgumentException.ThrowIfNullOrWhiteSpace(state.Key);

		cancellationToken.ThrowIfCancellationRequested();

		var now = this._timeProvider.GetUtcNow();
		this.PruneExpiredStates(now);

		var createdAt = state.CreatedAt == default ? now : state.CreatedAt;
		var expiresAt = state.ExpiresAt == default ? createdAt.Add(this._options.PendingStateLifetime) : state.ExpiresAt;
		if (expiresAt <= createdAt)
			throw new InvalidOperationException("Pending ingress state expiration must be later than creation.");

		this._states[state.Key] = new DiscordIngressPendingState
		{
			Key = state.Key,
			Flow = string.IsNullOrWhiteSpace(state.Flow) ? "oauth" : state.Flow,
			RequestUri = state.RequestUri,
			CreatedAt = createdAt,
			ExpiresAt = expiresAt,
			Properties = NormalizeProperties(state.Properties)
		};

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public ValueTask<DiscordIngressPendingState?> GetAsync(string key, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);

		cancellationToken.ThrowIfCancellationRequested();

		return new ValueTask<DiscordIngressPendingState?>(this.TryGetActiveState(key, out var state) ? state : null);
	}

	/// <inheritdoc />
	public ValueTask<DiscordIngressPendingState?> ConsumeAsync(string key, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);

		cancellationToken.ThrowIfCancellationRequested();

		if (!this.TryGetActiveState(key, out var state))
			return new ValueTask<DiscordIngressPendingState?>((DiscordIngressPendingState?)null);

		this._states.TryRemove(key, out _);
		return new ValueTask<DiscordIngressPendingState?>(state);
	}

	/// <inheritdoc />
	public ValueTask<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);

		cancellationToken.ThrowIfCancellationRequested();

		return new ValueTask<bool>(this._states.TryRemove(key, out _));
	}

	private static IReadOnlyDictionary<string, string?> NormalizeProperties(IReadOnlyDictionary<string, string?>? properties)
	{
		if (properties is null || properties.Count == 0)
			return EmptyProperties;

		Dictionary<string, string?> normalizedProperties = new(StringComparer.Ordinal);
		foreach (var (key, value) in properties)
			normalizedProperties[key] = value;

		return new ReadOnlyDictionary<string, string?>(normalizedProperties);
	}

	private void PruneExpiredStates(DateTimeOffset now)
	{
		var nextCleanupUtcTicks = Interlocked.Read(ref this._nextCleanupUtcTicks);
		if (nextCleanupUtcTicks > now.UtcTicks)
			return;

		Interlocked.Exchange(ref this._nextCleanupUtcTicks, now.Add(this._options.PendingStateCleanupInterval).UtcTicks);

		foreach (var (key, value) in this._states)
			if (value.ExpiresAt <= now)
				this._states.TryRemove(key, out _);
	}

	private bool TryGetActiveState(string key, out DiscordIngressPendingState? state)
	{
		var now = this._timeProvider.GetUtcNow();
		this.PruneExpiredStates(now);

		if (!this._states.TryGetValue(key, out state))
			return false;

		if (state.ExpiresAt > now)
			return true;

		this._states.TryRemove(key, out _);
		state = null;
		return false;
	}
}
