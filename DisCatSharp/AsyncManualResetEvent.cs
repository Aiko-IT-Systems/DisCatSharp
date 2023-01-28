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
	public bool IsSet => this._tsc != null && this._tsc.Task.IsCompleted;

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
		this._tsc = new TaskCompletionSource<bool>();

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

			if (!tsc.Task.IsCompleted || Interlocked.CompareExchange(ref this._tsc, new TaskCompletionSource<bool>(), tsc) == tsc)
				return;
		}
	}
}
