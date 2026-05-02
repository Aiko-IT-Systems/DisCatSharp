using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Stores pending ingress state needed by callback-driven flows such as OAuth.
/// </summary>
public interface IDiscordIngressPendingStateStore
{
	/// <summary>
	///     Stores or replaces a pending state entry.
	/// </summary>
	/// <param name="state">The state entry to persist.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>A completed task once the state has been stored.</returns>
	ValueTask StoreAsync(DiscordIngressPendingState state, CancellationToken cancellationToken = default);

	/// <summary>
	///     Resolves an existing pending state entry without removing it.
	/// </summary>
	/// <param name="key">The pending state key.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The pending state entry, or <see langword="null" /> when none is available.</returns>
	ValueTask<DiscordIngressPendingState?> GetAsync(string key, CancellationToken cancellationToken = default);

	/// <summary>
	///     Resolves and removes an existing pending state entry.
	/// </summary>
	/// <param name="key">The pending state key.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The removed pending state entry, or <see langword="null" /> when none is available.</returns>
	ValueTask<DiscordIngressPendingState?> ConsumeAsync(string key, CancellationToken cancellationToken = default);

	/// <summary>
	///     Removes a pending state entry when present.
	/// </summary>
	/// <param name="key">The pending state key.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns><see langword="true" /> when an entry was removed.</returns>
	ValueTask<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
}
