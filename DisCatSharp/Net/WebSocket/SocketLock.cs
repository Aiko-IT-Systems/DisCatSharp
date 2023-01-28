// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
	private CancellationTokenSource _timeoutCancelSource;

	/// <summary>
	/// Gets the cancel token.
	/// </summary>
	private CancellationToken TIMEOUT_CANCEL => this._timeoutCancelSource.Token;

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
		this._timeoutCancelSource = null;
		this._maxConcurrency = maxConcurrency;
		this._lockSemaphore = new SemaphoreSlim(maxConcurrency);
	}

	/// <summary>
	/// Locks the socket.
	/// </summary>
	public async Task LockAsync()
	{
		await this._lockSemaphore.WaitAsync().ConfigureAwait(false);

		this._timeoutCancelSource = new CancellationTokenSource();
		this._unlockTask = Task.Delay(TimeSpan.FromSeconds(30), this.TIMEOUT_CANCEL);
		_ = this._unlockTask.ContinueWith(this.InternalUnlock, TaskContinuationOptions.NotOnCanceled);
	}

	/// <summary>
	/// Unlocks the socket after a given timespan.
	/// </summary>
	/// <param name="unlockDelay">The unlock delay.</param>
	public void UnlockAfter(TimeSpan unlockDelay)
	{
		if (this._timeoutCancelSource == null || this._lockSemaphore.CurrentCount > 0)
			return; // it's not unlockable because it's post-IDENTIFY or not locked

		try
		{
			this._timeoutCancelSource.Cancel();
			this._timeoutCancelSource.Dispose();
		}
		catch { }
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
		catch { }
	}

	/// <summary>
	/// Unlocks the socket.
	/// </summary>
	/// <param name="t">The task.</param>
	private void InternalUnlock(Task t)
		=> this._lockSemaphore.Release(this._maxConcurrency);
}
