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

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Provides a simplified way of executing asynchronous code synchronously.
/// </summary>
public class AsyncExecutor
{
	/// <summary>
	/// Creates a new instance of asynchronous executor.
	/// </summary>
	public AsyncExecutor()
	{ }

	/// <summary>
	/// Executes a specified task in an asynchronous manner, waiting for its completion.
	/// </summary>
	/// <param name="task">Task to execute.</param>
	public void Execute(Task task)
	{
		// create state object
		var taskState = new StateRef<object>(new AutoResetEvent(false));

		// queue a task and wait for it to finish executing
		task.ContinueWith(TaskCompletionHandler, taskState);
		taskState.Lock.WaitOne();

		// check for and rethrow any exceptions
		if (taskState.Exception != null)
			throw taskState.Exception;

		// completion method
		void TaskCompletionHandler(Task t, object state)
		{
			// retrieve state data
			var stateRef = state as StateRef<object>;

			// retrieve any exceptions or cancellation status
			if (t.IsFaulted)
			{
				if (t.Exception.InnerExceptions.Count == 1) // unwrap if 1
					stateRef.Exception = t.Exception.InnerException;
				else
					stateRef.Exception = t.Exception;
			}
			else if (t.IsCanceled)
			{
				stateRef.Exception = new TaskCanceledException(t);
			}

			// signal that the execution is done
			stateRef.Lock.Set();
		}
	}

	/// <summary>
	/// Executes a specified task in an asynchronous manner, waiting for its completion, and returning the result.
	/// </summary>
	/// <typeparam name="T">Type of the Task's return value.</typeparam>
	/// <param name="task">Task to execute.</param>
	/// <returns>Task's result.</returns>
	public T Execute<T>(Task<T> task)
	{
		// create state object
		var taskState = new StateRef<T>(new AutoResetEvent(false));

		// queue a task and wait for it to finish executing
		task.ContinueWith(TaskCompletionHandler, taskState);
		taskState.Lock.WaitOne();

		// check for and rethrow any exceptions
		if (taskState.Exception != null)
			throw taskState.Exception;

		// return the result, if any
		if (taskState.HasResult)
			return taskState.Result;

		// throw exception if no result
		throw new Exception("Task returned no result.");

		// completion method
		void TaskCompletionHandler(Task<T> t, object state)
		{
			// retrieve state data
			var stateRef = state as StateRef<T>;

			// retrieve any exceptions or cancellation status
			if (t.IsFaulted)
			{
				if (t.Exception.InnerExceptions.Count == 1) // unwrap if 1
					stateRef.Exception = t.Exception.InnerException;
				else
					stateRef.Exception = t.Exception;
			}
			else if (t.IsCanceled)
			{
				stateRef.Exception = new TaskCanceledException(t);
			}

			// return the result from the task, if any
			if (t.IsCompleted && !t.IsFaulted)
			{
				stateRef.HasResult = true;
				stateRef.Result = t.Result;
			}

			// signal that the execution is done
			stateRef.Lock.Set();
		}
	}

	/// <summary>
	/// The state ref.
	/// </summary>
	private sealed class StateRef<T>
	{
		/// <summary>
		/// Gets the lock used to wait for task's completion.
		/// </summary>
		public AutoResetEvent Lock { get; }

		/// <summary>
		/// Gets the exception that occurred during task's execution, if any.
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		/// Gets the result returned by the task.
		/// </summary>
		public T Result { get; set; }

		/// <summary>
		/// Gets whether the task returned a result.
		/// </summary>
		public bool HasResult { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StateRef{T}"/> class.
		/// </summary>
		/// <param name="lock">The lock.</param>
		public StateRef(AutoResetEvent @lock)
		{
			this.Lock = @lock;
		}
	}
}
