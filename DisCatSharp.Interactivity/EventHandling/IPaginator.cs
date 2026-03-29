using System;
using System.Threading.Tasks;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
///     The paginator.
/// </summary>
internal interface IPaginator : IDisposable
{
	/// <summary>
	///     Paginates.
	/// </summary>
	/// <param name="request">The request to paginate.</param>
	/// <returns>A task that completes when the pagination finishes or times out.</returns>
	Task DoPaginationAsync(IPaginationRequest request);
}
