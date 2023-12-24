using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// ABC for <see cref="AsyncEvent{TSender, TArgs}"/>, allowing for using instances thereof without knowing the underlying instance's type parameters.
/// </summary>
public abstract class AsyncEvent
{
	/// <summary>
	/// Gets the name of this event.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Prevents a default instance of the <see cref="AsyncEvent"/> class from being created.
	/// </summary>
	/// <param name="name">The name.</param>
	private protected AsyncEvent(string name)
	{
		this.Name = name;
	}
}

/// <summary>
/// Implementation of asynchronous event. The handlers of such events are executed asynchronously, but sequentially.
/// </summary>
/// <typeparam name="TSender">Type of the object that dispatches this event.</typeparam>
/// <typeparam name="TArgs">Type of event argument object passed to this event's handlers.</typeparam>
/// <remarks>
/// Creates a new asynchronous event with specified name and exception handler.
/// </remarks>
/// <param name="name">Name of this event.</param>
/// <param name="maxExecutionTime">Maximum handler execution time. A value of <see cref="TimeSpan.Zero"/> means infinite.</param>
/// <param name="exceptionHandler">Delegate which handles exceptions caused by this event.</param>
public sealed class AsyncEvent<TSender, TArgs>(string name, TimeSpan maxExecutionTime, AsyncEventExceptionHandler<TSender, TArgs>? exceptionHandler) : AsyncEvent(name)
	where TArgs : AsyncEventArgs
{
	/// <summary>
	/// Gets the maximum allotted execution time for all handlers. Any event which causes the handler to time out
	/// will raise a non-fatal <see cref="AsyncEventTimeoutException{TSender, TArgs}"/>.
	/// </summary>
	public TimeSpan MaximumExecutionTime { get; } = maxExecutionTime;

	/// <summary>
	/// Gets the lock.
	/// </summary>
	private readonly object _lock = new();

	/// <summary>
	/// Gets or sets the event handlers.
	/// </summary>
	private ImmutableArray<AsyncEventHandler<TSender, TArgs>> _handlers = [];

	/// <summary>
	/// Gets or sets the exception handler.
	/// </summary>
	private readonly AsyncEventExceptionHandler<TSender, TArgs>? _exceptionHandler = exceptionHandler;

	/// <summary>
	/// Registers a new handler for this event.
	/// </summary>
	/// <param name="handler">Handler to register for this event.</param>
	public void Register(AsyncEventHandler<TSender, TArgs> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		lock (this._lock)
		{
			this._handlers = this._handlers.Add(handler);
		}
	}

	/// <summary>
	/// Unregisters an existing handler from this event.
	/// </summary>
	/// <param name="handler">Handler to unregister from the event.</param>
	public void Unregister(AsyncEventHandler<TSender, TArgs> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		lock (this._lock)
		{
			this._handlers = this._handlers.Remove(handler);
		}
	}

	/// <summary>
	/// Unregisters all existing handlers from this event.
	/// </summary>
	public void UnregisterAll() => this._handlers = [];

	/// <summary>
	/// <para>Raises this event by invoking all of its registered handlers, in order of registration.</para>
	/// <para>All exceptions throw during invocation will be handled by the event's registered exception handler.</para>
	/// </summary>
	/// <param name="sender">Object which raised this event.</param>
	/// <param name="e">Arguments for this event.</param>
	/// <param name="exceptionMode">Defines what to do with exceptions caught from handlers.</param>
	/// <returns></returns>
	public async Task InvokeAsync(TSender sender, TArgs e, AsyncEventExceptionMode exceptionMode = AsyncEventExceptionMode.Default)
	{
		var handlers = this._handlers;
		if (handlers.Length == 0)
			return;

		// Collect exceptions
		List<Exception> exceptions = [];
		if ((exceptionMode & AsyncEventExceptionMode.ThrowAll) != 0)
			exceptions = new(handlers.Length * 2 /* timeout + regular */);

		// If we have a timeout configured, start the timeout task
		var timeout = this.MaximumExecutionTime > TimeSpan.Zero ? Task.Delay(this.MaximumExecutionTime) : null;
		foreach (var handler in handlers)
			try
			{
				// Start the handler execution
				var handlerTask = handler(sender, e);
				if (handlerTask != null && timeout != null)
				{
					// If timeout is configured, wait for any task to finish
					// If the timeout task finishes first, the handler is causing a timeout
					var result = await Task.WhenAny(timeout, handlerTask).ConfigureAwait(false);
					if (result == timeout)
					{
						timeout = null;
						var timeoutEx = new AsyncEventTimeoutException<TSender, TArgs>(this, handler);

						// Notify about the timeout and complete execution
						if ((exceptionMode & AsyncEventExceptionMode.HandleNonFatal) == AsyncEventExceptionMode.HandleNonFatal)
							this.HandleException(timeoutEx, handler, sender, e);

						if ((exceptionMode & AsyncEventExceptionMode.ThrowNonFatal) == AsyncEventExceptionMode.ThrowNonFatal)
							exceptions.Add(timeoutEx);

						await handlerTask.ConfigureAwait(false);
					}
				}
				else if (handlerTask != null)
					// No timeout is configured, or timeout already expired, proceed as usual
					await handlerTask.ConfigureAwait(false);

				if (e.Handled)
					break;
			}
			catch (Exception ex)
			{
				e.Handled = false;

				if ((exceptionMode & AsyncEventExceptionMode.HandleFatal) == AsyncEventExceptionMode.HandleFatal)
					this.HandleException(ex, handler, sender, e);

				if ((exceptionMode & AsyncEventExceptionMode.ThrowFatal) == AsyncEventExceptionMode.ThrowFatal)
					exceptions.Add(ex);
			}

		if ((exceptionMode & AsyncEventExceptionMode.ThrowAll) != 0 && exceptions.Count > 0)
			throw new AggregateException("Exceptions were thrown during execution of the event's handlers.", exceptions);
	}

	/// <summary>
	/// Handles the exception.
	/// </summary>
	/// <param name="ex">The ex.</param>
	/// <param name="handler">The handler.</param>
	/// <param name="sender">The sender.</param>
	/// <param name="args">The args.</param>
	private void HandleException(Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs args)
		=> this._exceptionHandler?.Invoke(this, ex, handler, sender, args);
}
