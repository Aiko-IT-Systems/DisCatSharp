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
		var taskState = new StateRef<object>(new(false));

		// queue a task and wait for it to finish executing
		task.ContinueWith(TaskCompletionHandler!, taskState);
		taskState.Lock.WaitOne();

		// check for and rethrow any exceptions
		if (taskState.Exception != null)
			throw taskState.Exception;

		return;

		// completion method
		static void TaskCompletionHandler(Task t, object state)
		{
			// retrieve state data
			if (state is not StateRef<object> stateRef)
				throw new NullReferenceException();

			// retrieve any exceptions or cancellation status
			if (t.IsFaulted)
				stateRef.Exception = t.Exception?.InnerExceptions.Count is 1 ? t.Exception.InnerException : t.Exception; // unwrap if 1
			else if (t.IsCanceled)
				stateRef.Exception = new TaskCanceledException(t);

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
	public T? Execute<T>(Task<T> task)
	{
		// create state object
		var taskState = new StateRef<T>(new(false));

		// queue a task and wait for it to finish executing
		task.ContinueWith(TaskCompletionHandler!, taskState);
		taskState.Lock.WaitOne();

		// check for and rethrow any exceptions
		if (taskState.Exception != null)
			throw taskState.Exception;

		// return the result, if any
		if (taskState.HasResult)
			return taskState.Result;

		// throw exception if no result
		throw new("Task returned no result.");

		// completion method
		static void TaskCompletionHandler(Task<T> t, object state)
		{
			// retrieve state data
			if (state is not StateRef<object> stateRef)
				throw new NullReferenceException();

			// retrieve any exceptions or cancellation status
			if (t.IsFaulted)
				stateRef.Exception = t.Exception?.InnerExceptions.Count is 1 ? t.Exception.InnerException : t.Exception; // unwrap if 1
			else if (t.IsCanceled)
				stateRef.Exception = new TaskCanceledException(t);

			// return the result from the task, if any
			if (t is { IsCompleted: true, IsFaulted: false })
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
	/// <remarks>
	/// Initializes a new instance of the <see cref="StateRef{T}"/> class.
	/// </remarks>
	/// <param name="lock">The lock.</param>
	private sealed class StateRef<T>(AutoResetEvent @lock)
	{
		/// <summary>
		/// Gets the lock used to wait for task's completion.
		/// </summary>
		public AutoResetEvent Lock { get; } = @lock;

		/// <summary>
		/// Gets the exception that occurred during task's execution, if any.
		/// </summary>
		public Exception? Exception { get; set; }

		/// <summary>
		/// Gets the result returned by the task.
		/// </summary>
		public T? Result { get; set; }

		/// <summary>
		/// Gets whether the task returned a result.
		/// </summary>
		public bool HasResult { get; set; }
	}
}
