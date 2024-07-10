using System;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// ABC for <see cref="AsyncEventHandler{TSender, TArgs}"/>, allowing for using instances thereof without knowing the underlying instance's type parameters.
/// </summary>
public abstract class AsyncEventTimeoutException : Exception
{
	/// <summary>
	/// Gets the event the invocation of which caused the timeout.
	/// </summary>
	public AsyncEvent Event { get; }

	/// <summary>
	/// Gets the handler which caused the timeout.
	/// </summary>
	public AsyncEventHandler<object, AsyncEventArgs>? Handler { get; }

	/// <summary>
	/// Prevents a default instance of the <see cref="AsyncEventTimeoutException"/> class from being created.
	/// </summary>
	/// <param name="asyncEvent">The async event.</param>
	/// <param name="eventHandler">The event handler.</param>
	/// <param name="message">The message.</param>
	private protected AsyncEventTimeoutException(AsyncEvent asyncEvent, AsyncEventHandler<object, AsyncEventArgs>? eventHandler, string message)
		: base(message)
	{
		this.Event = asyncEvent;
		this.Handler = eventHandler;
	}
}

/// <summary>
/// <para>Thrown whenever execution of an <see cref="AsyncEventHandler{TSender, TArgs}"/> exceeds maximum time allowed.</para>
/// <para>This is a non-fatal exception, used primarily to inform users that their code is taking too long to execute.</para>
/// </summary>
/// <typeparam name="TSender">Type of sender that dispatched this asynchronous event.</typeparam>
/// <typeparam name="TArgs">Type of event arguments for the asynchronous event.</typeparam>
/// <remarks>
/// Creates a new timeout exception for specified event and handler.
/// </remarks>
/// <param name="asyncEvent">Event the execution of which timed out.</param>
/// <param name="eventHandler">Handler which timed out.</param>
public sealed class AsyncEventTimeoutException<TSender, TArgs>(AsyncEvent asyncEvent, AsyncEventHandler<TSender, TArgs> eventHandler)
	: AsyncEventTimeoutException(asyncEvent, eventHandler as AsyncEventHandler<object, AsyncEventArgs>, "An event handler caused the invocation of an asynchronous event to time out.")
	where TArgs : AsyncEventArgs
{
	/// <summary>
	/// Gets the event the invocation of which caused the timeout.
	/// </summary>
	public new AsyncEvent<TSender, TArgs> Event => base.Event as AsyncEvent<TSender, TArgs> ?? throw new NullReferenceException();

	/// <summary>
	/// Gets the handler which caused the timeout.
	/// </summary>
	public new AsyncEventHandler<TSender, TArgs> Handler => base.Handler as AsyncEventHandler<TSender, TArgs> ?? throw new NullReferenceException();
}
