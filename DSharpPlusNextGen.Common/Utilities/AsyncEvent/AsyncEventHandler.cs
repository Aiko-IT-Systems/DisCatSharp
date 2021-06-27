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

using System.Threading.Tasks;

namespace DSharpPlusNextGen.Common.Utilities
{
    /// <summary>
    /// Handles an asynchronous event of type <see cref="AsyncEvent{TSender, TArgs}"/>. The handler will take an instance of <typeparamref name="TArgs"/> as its arguments.
    /// </summary>
    /// <typeparam name="TSender">Type of the object that dispatches this event.</typeparam>
    /// <typeparam name="TArgs">Type of the object which holds arguments for this event.</typeparam>
    /// <param name="sender">Object which raised this event.</param>
    /// <param name="e">Arguments for this event.</param>
    /// <returns></returns>
    public delegate Task AsyncEventHandler<in TSender, in TArgs>(TSender sender, TArgs e) where TArgs : AsyncEventArgs;
}
