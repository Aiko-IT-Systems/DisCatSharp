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

using Newtonsoft.Json;

namespace DisCatSharp.Net;

/// <summary>
/// Represents the bucket limits for identifying to Discord.
/// <para>This is only relevant for clients that are manually sharding.</para>
/// </summary>
public class SessionBucket
{
	/// <summary>
	/// Gets the total amount of sessions per token.
	/// </summary>
	[JsonProperty("total")]
	public int Total { get; internal set; }

	/// <summary>
	/// Gets the remaining amount of sessions for this token.
	/// </summary>
	[JsonProperty("remaining")]
	public int Remaining { get; internal set; }

	/// <summary>
	/// Gets the datetime when the <see cref="Remaining"/> will reset.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset ResetAfter { get; internal set; }

	/// <summary>
	/// Gets the maximum amount of shards that can boot concurrently.
	/// </summary>
	[JsonProperty("max_concurrency")]
	public int MaxConcurrency { get; internal set; }

	/// <summary>
	/// Gets the reset after value.
	/// </summary>
	[JsonProperty("reset_after")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
	internal int ResetAfterInternal { get; set; }

	/// <summary>
	/// Returns a readable session bucket string.
	/// </summary>
	public override string ToString()
		=> $"[{this.Remaining}/{this.Total}] {this.ResetAfter}. {this.MaxConcurrency}x concurrency";
}
