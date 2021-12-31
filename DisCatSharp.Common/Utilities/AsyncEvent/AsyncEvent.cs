// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace DisCatSharp.Common.Utilities
{
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
        /// <param name="Name">The name.</param>
        private protected AsyncEvent(string Name)
        {
            this.Name = Name;
        }
    }

    /// <summary>
    /// Implementation of asynchronous event. The handlers of such events are executed asynchronously, but sequentially.
    /// </summary>
    /// <typeparam name="TSender">Type of the object that dispatches this event.</typeparam>
    /// <typeparam name="TArgs">Type of event argument object passed to this event's handlers.</typeparam>
    public sealed class AsyncEvent<TSender, TArgs> : AsyncEvent
        where TArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the maximum alloted execution time for all handlers. Any event which causes the handler to time out
        /// will raise a non-fatal <see cref="AsyncEventTimeoutException{TSender, TArgs}"/>.
        /// </summary>
        public TimeSpan MaximumExecutionTime { get; }

        private readonly object _lock = new object();
        private ImmutableArray<AsyncEventHandler<TSender, TArgs>> _handlers;
        private readonly AsyncEventExceptionHandler<TSender, TArgs> _exceptionHandler;

        /// <summary>
        /// Creates a new asynchronous event with specified name and exception handler.
        /// </summary>
        /// <param name="Name">Name of this event.</param>
        /// <param name="MaxExecutionTime">Maximum handler execution time. A value of <see cref="System.TimeSpan.Zero"/> means infinite.</param>
        /// <param name="ExceptionHandler">Delegate which handles exceptions caused by this event.</param>
        public AsyncEvent(string Name, TimeSpan MaxExecutionTime, AsyncEventExceptionHandler<TSender, TArgs> ExceptionHandler)
            : base(Name)
        {
            this._handlers = ImmutableArray<AsyncEventHandler<TSender, TArgs>>.Empty;
            this._exceptionHandler = ExceptionHandler;

            this.MaximumExecutionTime = MaxExecutionTime;
        }

        /// <summary>
        /// Registers a new handler for this event.
        /// </summary>
        /// <param name="Handler">Handler to register for this event.</param>
        public void Register(AsyncEventHandler<TSender, TArgs> Handler)
        {
            if (Handler == null)
                throw new ArgumentNullException(nameof(Handler));

            lock (this._lock)
                this._handlers = this._handlers.Add(Handler);
        }

        /// <summary>
        /// Unregisters an existing handler from this event.
        /// </summary>
        /// <param name="Handler">Handler to unregister from the event.</param>
        public void Unregister(AsyncEventHandler<TSender, TArgs> Handler)
        {
            if (Handler == null)
                throw new ArgumentNullException(nameof(Handler));

            lock (this._lock)
                this._handlers = this._handlers.Remove(Handler);
        }

        /// <summary>
        /// Unregisters all existing handlers from this event.
        /// </summary>
        public void UnregisterAll()
        {
            this._handlers = ImmutableArray<AsyncEventHandler<TSender, TArgs>>.Empty;
        }

        /// <summary>
        /// <para>Raises this event by invoking all of its registered handlers, in order of registration.</para>
        /// <para>All exceptions throw during invocation will be handled by the event's registered exception handler.</para>
        /// </summary>
        /// <param name="Sender">Object which raised this event.</param>
        /// <param name="E">Arguments for this event.</param>
        /// <param name="ExceptionMode">Defines what to do with exceptions caught from handlers.</param>
        /// <returns></returns>
        public async Task InvokeAsync(TSender Sender, TArgs E, AsyncEventExceptionMode ExceptionMode = AsyncEventExceptionMode.Default)
        {
            var handlers = this._handlers;
            if (handlers.Length == 0)
                return;

            // Collect exceptions
            List<Exception> exceptions = null;
            if ((ExceptionMode & AsyncEventExceptionMode.ThrowAll) != 0)
                exceptions = new List<Exception>(handlers.Length * 2 /* timeout + regular */);

            // If we have a timeout configured, start the timeout task
            var timeout = this.MaximumExecutionTime > TimeSpan.Zero ? Task.Delay(this.MaximumExecutionTime) : null;
            for (var i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];
                try
                {
                    // Start the handler execution
                    var handlerTask = handler(Sender, E);
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
                            if ((ExceptionMode & AsyncEventExceptionMode.HandleNonFatal) == AsyncEventExceptionMode.HandleNonFatal)
                                this.HandleException(timeoutEx, handler, Sender, E);

                            if ((ExceptionMode & AsyncEventExceptionMode.ThrowNonFatal) == AsyncEventExceptionMode.ThrowNonFatal)
                                exceptions.Add(timeoutEx);

                            await handlerTask.ConfigureAwait(false);
                        }
                    }
                    else if (handlerTask != null)
                    {
                        // No timeout is configured, or timeout already expired, proceed as usual
                        await handlerTask.ConfigureAwait(false);
                    }

                    if (E.Handled)
                        break;
                }
                catch (Exception ex)
                {
                    E.Handled = false;

                    if ((ExceptionMode & AsyncEventExceptionMode.HandleFatal) == AsyncEventExceptionMode.HandleFatal)
                        this.HandleException(ex, handler, Sender, E);

                    if ((ExceptionMode & AsyncEventExceptionMode.ThrowFatal) == AsyncEventExceptionMode.ThrowFatal)
                        exceptions.Add(ex);
                }
            }

            if ((ExceptionMode & AsyncEventExceptionMode.ThrowAll) != 0 && exceptions.Count > 0)
                throw new AggregateException("Exceptions were thrown during execution of the event's handlers.", exceptions);
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="Ex">The ex.</param>
        /// <param name="Handler">The handler.</param>
        /// <param name="Sender">The sender.</param>
        /// <param name="Args">The args.</param>
        private void HandleException(Exception Ex, AsyncEventHandler<TSender, TArgs> Handler, TSender Sender, TArgs Args)
        {
            if (this._exceptionHandler != null)
                this._exceptionHandler(this, Ex, Handler, Sender, Args);
        }
    }
}
