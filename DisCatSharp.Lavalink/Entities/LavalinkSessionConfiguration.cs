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

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a <see cref="LavalinkSessionConfiguration"/>.
/// </summary>
public sealed class LavalinkSessionConfiguration
{
	/// <summary>
	/// Whether resuming is enabled for this session or not.
	/// </summary>
	[JsonProperty("resuming", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Resuming { get; internal set; }

	/// <summary>
	/// The timeout in seconds (default is 60s)
	/// </summary>
	[JsonProperty("timeout", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int> TimeoutSeconds { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="LavalinkSessionConfiguration"/>.
	/// </summary>
	/// <param name="resuming">Whether resuming is enabled for this session or not.</param>
	/// <param name="timeoutSeconds">The timeout in seconds.</param>
	public LavalinkSessionConfiguration(Optional<bool> resuming, Optional<int> timeoutSeconds)
	{
		this.Resuming = resuming;
		this.TimeoutSeconds = timeoutSeconds;
	}

	/// <summary>
	/// Constructs a new <see cref="LavalinkSessionConfiguration"/>.
	/// </summary>
	internal LavalinkSessionConfiguration()
	{ }
}
