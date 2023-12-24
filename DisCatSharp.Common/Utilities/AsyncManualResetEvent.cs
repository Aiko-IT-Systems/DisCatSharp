using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Represents a thread synchronization event that, when signaled, must be reset manually. Unlike <see cref="ManualResetEventSlim"/>, this event is asynchronous.
/// </summary>
public sealed class AsyncManualResetEvent
{
	/// <summary>
	/// Gets whether this event has been signaled.
	/// </summary>
	public bool IsSet => this._resetTcs?.Task?.IsCompleted == true;

	/// <summary>
	/// Gets the task completion source for this event.
	/// </summary>
	private volatile TaskCompletionSource<bool> _resetTcs;

	/// <summary>
	/// Creates a new asynchronous synchronization event with initial state.
	/// </summary>
	/// <param name="initialState">Initial state of this event.</param>
	public AsyncManualResetEvent(bool initialState)
	{
		this._resetTcs = new();
		if (initialState)
			this._resetTcs.TrySetResult(initialState);
	}

	// Spawn a threadpool thread instead of making a task
	// Maybe overkill, but I am less unsure of this than awaits and
	// potentially cross-scheduler interactions
	/// <summary>
	/// Asynchronously signal this event.
	/// </summary>
	/// <returns></returns>
	public Task SetAsync()
		=> Task.Run(() => this._resetTcs.TrySetResult(true));

	/// <summary>
	/// Asynchronously wait for this event to be signaled.
	/// </summary>
	/// <returns></returns>
	public Task WaitAsync()
		=> this._resetTcs.Task;

	/// <summary>
	/// Reset this event's signal state to unsignaled.
	/// </summary>
	public void Reset()
	{
		while (true)
		{
			var tcs = this._resetTcs;
			if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref this._resetTcs, new(), tcs) == tcs)
				return;
		}
	}
}
