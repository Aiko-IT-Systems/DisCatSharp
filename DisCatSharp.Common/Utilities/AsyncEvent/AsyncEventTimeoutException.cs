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
	public AsyncEventHandler<object, AsyncEventArgs> Handler { get; }

	/// <summary>
	/// Prevents a default instance of the <see cref="AsyncEventTimeoutException"/> class from being created.
	/// </summary>
	/// <param name="asyncEvent">The async event.</param>
	/// <param name="eventHandler">The event handler.</param>
	/// <param name="message">The message.</param>
	private protected AsyncEventTimeoutException(AsyncEvent asyncEvent, AsyncEventHandler<object, AsyncEventArgs> eventHandler, string message)
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
public class AsyncEventTimeoutException<TSender, TArgs> : AsyncEventTimeoutException
	where TArgs : AsyncEventArgs
{
	/// <summary>
	/// Gets the event the invocation of which caused the timeout.
	/// </summary>
	public new AsyncEvent<TSender, TArgs> Event => base.Event as AsyncEvent<TSender, TArgs>;

	/// <summary>
	/// Gets the handler which caused the timeout.
	/// </summary>
	public new AsyncEventHandler<TSender, TArgs> Handler => base.Handler as AsyncEventHandler<TSender, TArgs>;

	/// <summary>
	/// Creates a new timeout exception for specified event and handler.
	/// </summary>
	/// <param name="asyncEvent">Event the execution of which timed out.</param>
	/// <param name="eventHandler">Handler which timed out.</param>
	public AsyncEventTimeoutException(AsyncEvent<TSender, TArgs> asyncEvent, AsyncEventHandler<TSender, TArgs> eventHandler)
		: base(asyncEvent, eventHandler as AsyncEventHandler<object, AsyncEventArgs>, "An event handler caused the invocation of an asynchronous event to time out.")
	{ }
}
