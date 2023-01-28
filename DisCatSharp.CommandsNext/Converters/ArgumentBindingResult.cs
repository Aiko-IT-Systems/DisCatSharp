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
using System.Collections.Generic;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents a argument binding result.
/// </summary>
public readonly struct ArgumentBindingResult
{
	/// <summary>
	/// Gets a value indicating whether the binding is successful.
	/// </summary>
	public bool IsSuccessful { get; }
	/// <summary>
	/// Gets the converted.
	/// </summary>
	public object[] Converted { get; }
	/// <summary>
	/// Gets the raw.
	/// </summary>
	public IReadOnlyList<string> Raw { get; }
	/// <summary>
	/// Gets the reason.
	/// </summary>
	public Exception Reason { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ArgumentBindingResult"/> class.
	/// </summary>
	/// <param name="converted">The converted.</param>
	/// <param name="raw">The raw.</param>
	public ArgumentBindingResult(object[] converted, IReadOnlyList<string> raw)
	{
		this.IsSuccessful = true;
		this.Reason = null;
		this.Converted = converted;
		this.Raw = raw;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ArgumentBindingResult"/> class.
	/// </summary>
	/// <param name="ex">The ex.</param>
	public ArgumentBindingResult(Exception ex)
	{
		this.IsSuccessful = false;
		this.Reason = ex;
		this.Converted = null;
		this.Raw = null;
	}
}
