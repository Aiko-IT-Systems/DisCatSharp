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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlusNextGen.Common.Utilities
{
    /// <summary>
    /// Defines the behaviour for throwing exceptions from <see cref="AsyncEvent{TSender, TArgs}.InvokeAsync(TSender, TArgs, AsyncEventExceptionMode)"/>.
    /// </summary>
    public enum AsyncEventExceptionMode : int
    {
        /// <summary>
        /// Defines that no exceptions should be thrown. Only exception handlers will be used.
        /// </summary>
        IgnoreAll = 0,

        /// <summary>
        /// Defines that only fatal (i.e. non-<see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions 
        /// should be thrown.
        /// </summary>
        ThrowFatal = 1,

        /// <summary>
        /// Defines that only non-fatal (i.e. <see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions 
        /// should be thrown.
        /// </summary>
        ThrowNonFatal = 2,

        /// <summary>
        /// Defines that all exceptions should be thrown. This is equivalent to combining <see cref="ThrowFatal"/> and 
        /// <see cref="ThrowNonFatal"/> flags.
        /// </summary>
        ThrowAll = ThrowFatal | ThrowNonFatal,

        /// <summary>
        /// Defines that only fatal (i.e. non-<see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions 
        /// should be handled by the specified exception handler.
        /// </summary>
        HandleFatal = 4,

        /// <summary>
        /// Defines that only non-fatal (i.e. <see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions 
        /// should be handled by the specified exception handler.
        /// </summary>
        HandleNonFatal = 8,

        /// <summary>
        /// Defines that all exceptions should be handled by the specified exception handler. This is equivalent to 
        /// combining <see cref="HandleFatal"/> and <see cref="HandleNonFatal"/> flags.
        /// </summary>
        HandleAll = HandleFatal | HandleNonFatal,

        /// <summary>
        /// Defines that all exceptions should be thrown and handled by the specified exception handler. This is 
        /// equivalent to combinind <see cref="HandleAll"/> and <see cref="ThrowAll"/> flags.
        /// </summary>
        ThrowAllHandleAll = ThrowAll | HandleAll,

        /// <summary>
        /// Default mode, equivalent to <see cref="HandleAll"/>.
        /// </summary>
        Default = HandleAll
    }
}
