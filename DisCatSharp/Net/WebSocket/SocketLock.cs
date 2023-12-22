using System;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Net.WebSocket;

// Licensed from Clyde.NET

/// <summary>
/// Represents a socket lock.
/// </summary>
internal sealed class SocketLock : IDisposable
{
	/// <summary>
	/// Gets the application id.
	/// </summary>
	public ulong ApplicationId { get; }

	/// <summary>
	/// Gets the lock semaphore.
	/// </summary>
	private readonly SemaphoreSlim _lockSemaphore;

	/// <summary>
	/// Gets or sets the timeout cancel source.
	/// </summary>
	private CancellationTokenSource? _timeoutCancelSource;

	/// <summary>
	/// Gets the cancel token.
	/// </summary>
	private CancellationToken? _timeoutCancel;

	/// <summary>
	/// Gets or sets the unlock task.
	/// </summary>
	private Task _unlockTask;

	/// <summary>
	/// Gets or sets the max concurrency.
	/// </summary>
	private readonly int _maxConcurrency;

	/// <summary>
	/// Initializes a new instance of the <see cref="SocketLock"/> class.
	/// </summary>
	/// <param name="appId">The app id.</param>
	/// <param name="maxConcurrency">The max concurrency.</param>
	public SocketLock(ulong appId, int maxConcurrency)
	{
		this.ApplicationId = appId;
		this._timeoutCancelSource = null!;
		this._timeoutCancel = null!;
		this._maxConcurrency = maxConcurrency;
		this._lockSemaphore = new(maxConcurrency);
	}

	/// <summary>
	/// Locks the socket.
	/// </summary>
	public async Task LockAsync()
	{
		await this._lockSemaphore.WaitAsync(CancellationToken.None).ConfigureAwait(false);

		this._timeoutCancelSource = new();
		this._timeoutCancel = this._timeoutCancelSource.Token;
		this._unlockTask = Task.Delay(TimeSpan.FromSeconds(30), this._timeoutCancel.Value);
		_ = this._unlockTask.ContinueWith(this.InternalUnlock, TaskContinuationOptions.NotOnCanceled);
	}

	/// <summary>
	/// Unlocks the socket after a given timespan.
	/// </summary>
	/// <param name="unlockDelay">The unlock delay.</param>
	public void UnlockAfter(TimeSpan unlockDelay)
	{
		if (this._timeoutCancelSource is null || this._lockSemaphore.CurrentCount > 0)
			return; // it's not unlockable because it's post-IDENTIFY or not locked

		try
		{
			this._timeoutCancelSource.Cancel();
			this._timeoutCancelSource.Dispose();
		}
		catch
		{ }

		this._timeoutCancelSource = null;

		this._unlockTask = Task.Delay(unlockDelay, CancellationToken.None);
		_ = this._unlockTask.ContinueWith(this.InternalUnlock);
	}

	/// <summary>
	/// Waits for the socket lock.
	/// </summary>
	/// <returns>A Task.</returns>
	public Task WaitAsync()
		=> this._lockSemaphore.WaitAsync();

	/// <summary>
	/// Disposes the socket lock.
	/// </summary>
	public void Dispose()
	{
		try
		{
			this._timeoutCancelSource?.Cancel();
			this._timeoutCancelSource?.Dispose();
		}
		catch
		{ }
	}

	/// <summary>
	/// Unlocks the socket.
	/// </summary>
	/// <param name="t">The task.</param>
	private void InternalUnlock(Task t)
		=> this._lockSemaphore.Release(this._maxConcurrency);
}
