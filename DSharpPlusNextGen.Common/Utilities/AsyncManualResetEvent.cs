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

using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlusNextGen.Common.Utilities
{
    /// <summary>
    /// Represents a thread synchronization event that, when signaled, must be reset manually. Unlike <see cref="ManualResetEventSlim"/>, this event is asynchronous.
    /// </summary>
    public sealed class AsyncManualResetEvent
    {
        /// <summary>
        /// Gets whether this event has been signaled.
        /// </summary>
        public bool IsSet => this._resetTcs?.Task?.IsCompleted == true;

        private volatile TaskCompletionSource<bool> _resetTcs;

        /// <summary>
        /// Creates a new asynchronous synchronization event with initial state.
        /// </summary>
        /// <param name="initialState">Initial state of this event.</param>
        public AsyncManualResetEvent(bool initialState)
        {
            this._resetTcs = new TaskCompletionSource<bool>();
            if (initialState)
                this._resetTcs.TrySetResult(initialState);
        }

        // Spawn a threadpool thread instead of making a task
        // Maybe overkill, but I am less unsure of this than awaits and
        // potentially cross-scheduler interactions
        /// <summary>
        /// Asynchronously signal this event.
        /// </summary>
        /// <returns></returns>
        public Task SetAsync()
            => Task.Run(() => this._resetTcs.TrySetResult(true));

        /// <summary>
        /// Asynchronously wait for this event to be signaled.
        /// </summary>
        /// <returns></returns>
        public Task WaitAsync()
            => this._resetTcs.Task;

        /// <summary>
        /// Reset this event's signal state to unsignaled.
        /// </summary>
        public void Reset()
        {
            while (true)
            {
                var tcs = this._resetTcs;
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref this._resetTcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                    return;
            }
        }
    }
}
