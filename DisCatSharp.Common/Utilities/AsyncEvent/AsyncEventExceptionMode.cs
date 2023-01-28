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

namespace DisCatSharp.Common.Utilities;

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
	/// equivalent to combining <see cref="HandleAll"/> and <see cref="ThrowAll"/> flags.
	/// </summary>
	ThrowAllHandleAll = ThrowAll | HandleAll,

	/// <summary>
	/// Default mode, equivalent to <see cref="HandleAll"/>.
	/// </summary>
	Default = HandleAll
}
