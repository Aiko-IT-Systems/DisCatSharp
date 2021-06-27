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
}
