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
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlusNextGen.Common.Utilities
{
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

        private sealed class StateRef<T>
        {
            /// <summary>
            /// Gets the lock used to wait for task's completion.
            /// </summary>
            public AutoResetEvent Lock { get; }

            /// <summary>
            /// Gets the exception that occured during task's execution, if any.
            /// </summary>
            public Exception Exception { get; set; }

            /// <summary>
            /// Gets the result returned by the task.
            /// </summary>
            public T Result { get; set; }

            /// <summary>
            /// Gets whether the task returned a result.
            /// </summary>
            public bool HasResult { get; set; } = false;

            public StateRef(AutoResetEvent @lock)
            {
                this.Lock = @lock;
            }
        }
    }
}
