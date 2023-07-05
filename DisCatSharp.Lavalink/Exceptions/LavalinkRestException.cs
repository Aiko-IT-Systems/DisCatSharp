// This file is part of the DisCatSharp project.
//
// Copyright (c) 2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Net;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Exceptions;

/// <summary>
/// Represents a lavalink rest exception.
/// </summary>
public sealed class LavalinkRestException : Exception
{
	/// <summary>
	/// Gets the datetime offset when the exception was thrown.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Timestamp => Utilities.GetDateTimeOffsetFromMilliseconds(this._timestamp);
	[JsonProperty("timestamp")]
	private readonly long _timestamp;

	/// <summary>
	/// Gets the response http status code.
	/// </summary>
	[JsonProperty("status")]
	public HttpStatusCode Status { get; internal set; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	[JsonProperty("error")]
	public string Error { get; internal set; }

	/// <summary>
	/// Gets the exception message.
	/// </summary>
	[JsonProperty("message")]
	public new string Message { get; internal set; }

	/// <summary>
	/// Gets the path where the exception was thrown.
	/// </summary>
	[JsonProperty("path")]
	public string Path { get; internal set; }

	/// <summary>
	/// Gets the trace information to this exception.
	/// <para>Requires <see cref="LavalinkConfiguration.EnableTrace"/> to set to <see langword="true"/>.</para>
	/// </summary>
	[JsonProperty("trace")]
	public string? Trace { get; internal set; }

	/// <summary>
	/// Gets the response header.
	/// </summary>
	[JsonIgnore]
	public HttpResponseHeaders Headers { get; internal set; }

	/// <summary>
	/// Gets the response json.
	/// </summary>
	[JsonIgnore]
	public string Json { get; internal set; }
}
