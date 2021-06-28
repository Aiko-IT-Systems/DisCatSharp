// This file is part of DSharpPlusNextGen.Common project
//
// Copyright 2020 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace DSharpPlusNextGen.Common.Utilities
{
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
}
