using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp;

// source: https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-1-asyncmanualresetevent/
/// <summary>
/// Implements an async version of a <see cref="ManualResetEvent"/>
/// This class does currently not support Timeouts or the use of CancellationTokens
/// </summary>
internal class AsyncManualResetEvent
{
	/// <summary>
	/// Gets a value indicating whether this is set.
	/// </summary>
	public bool IsSet => this._tsc is { Task.IsCompleted: true };

	/// <summary>
	/// The task completion source.
	/// </summary>
	private TaskCompletionSource<bool> _tsc;

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncManualResetEvent"/> class.
	/// </summary>
	public AsyncManualResetEvent()
		: this(false)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncManualResetEvent"/> class.
	/// </summary>
	/// <param name="initialState">If true, initial state.</param>
	public AsyncManualResetEvent(bool initialState)
	{
		this._tsc = new();

		if (initialState) this._tsc.TrySetResult(true);
	}

	/// <summary>
	/// Waits the async waiter.
	/// </summary>
	public Task WaitAsync() => this._tsc.Task;

	/// <summary>
	/// Sets the async task.
	/// </summary>
	public Task SetAsync() => Task.Run(() => this._tsc.TrySetResult(true));

	/// <summary>
	/// Resets the async waiter.
	/// </summary>
	public void Reset()
	{
		while (true)
		{
			var tsc = this._tsc;

			if (!tsc.Task.IsCompleted || Interlocked.CompareExchange(ref this._tsc, new(), tsc) == tsc)
				return;
		}
	}
}
