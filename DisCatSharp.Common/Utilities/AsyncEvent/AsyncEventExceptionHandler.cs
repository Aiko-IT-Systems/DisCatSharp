using System;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Handles any exception raised by an <see cref="AsyncEvent{TSender, TArgs}"/> or its handlers.
/// </summary>
/// <typeparam name="TSender">Type of the object that dispatches this event.</typeparam>
/// <typeparam name="TArgs">Type of the object which holds arguments for this event.</typeparam>
/// <param name="asyncEvent">Asynchronous event which threw the exception.</param>
/// <param name="exception">Exception that was thrown</param>
/// <param name="handler">Handler which threw the exception.</param>
/// <param name="sender">Object which dispatched the event.</param>
/// <param name="eventArgs">Arguments with which the event was dispatched.</param>
public delegate void AsyncEventExceptionHandler<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception exception, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
	where TArgs : AsyncEventArgs;
